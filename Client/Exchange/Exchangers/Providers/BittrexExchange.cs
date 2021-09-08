using Bittrex.Net;
using Bittrex.Net.Objects;
using Bittrex.Net.Sockets;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using Pandora.Client.ClientLib.Contracts;
using Pandora.Client.Exchange.Contracts;
using Pandora.Client.Exchange.Exchangers.Contracts;
using Pandora.Client.Exchange.Models;
using Pandora.Client.Exchange.SaveManagers;
using Pandora.Client.Universal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.Exchangers.Providers
{
    internal class BittrexExchange : AbstractExchange, IPandoraExchanger
    {
        public new const AvailableExchangesList Identifier = AvailableExchangesList.Bittrex;

        private BittrexSocketClient FBittrexSocket;
        private static ConcurrentDictionary<string, MarketPriceInfo> FMarketPrices;
        private static UpdateSubscription FMarketPriceSubscription;

        /// <summary>
        /// Tuple with user credentials. First element is Key, second is Secret
        /// </summary>
        private Tuple<string, string> FUserCredentials;

        public bool IsCredentialsSet { get; private set; }

        public override string Name => Identifier.ToString();

        public override int ID => (int) Identifier;

        public event Action<IEnumerable<IExchangeMarket>> OnMarketPricesChanging;

        private ConcurrentDictionary<string, decimal> FCurrencyFees;
        private ConcurrentDictionary<string, int> FCurrencyConfirmations;
        private ConcurrentDictionary<long, IEnumerable<BittrexExchangeMarket>> FLocalCacheOfMarkets;
        private BittrexSymbol[] FMarkets;
        private DateTime FLastMarketCoinsRetrieval;
        private ConcurrentBag<string> FMarketsIDsWithPriceChanged;
        private DateTime FLastSocketPriceUpdate;
        private static Timer FPriceUpdaterWatcherTimer;
        private Timer FDispatchNewMarketPricesTimer;
        private object FPriceMarketLock = new object();

        ~BittrexExchange()
        {
            StopMarketUpdating();
            FCurrencyConfirmations.Clear();
            FCurrencyConfirmations = null;
            FCurrencyFees.Clear();
            FCurrencyFees = null;
            FLocalCacheOfMarkets?.Clear();
            FLocalCacheOfMarkets = null;
        }

        internal BittrexExchange()
        {
            FBittrexSocket = new BittrexSocketClient();
            FMarketPrices = new ConcurrentDictionary<string, MarketPriceInfo>();
            FMarketsIDsWithPriceChanged = new ConcurrentBag<string>();
            FCurrencyFees = new ConcurrentDictionary<string, decimal>();
            FCurrencyConfirmations = new ConcurrentDictionary<string, int>();
            FLocalCacheOfMarkets = new ConcurrentDictionary<long, IEnumerable<BittrexExchangeMarket>>();
            DoUpdateMarketCoins();
        }

        public void SetCredentials(string aApiKey, string aApiSecret)
        {
            ApiCredentials lCredentials = new ApiCredentials(aApiKey, aApiSecret);

            BittrexClientOptions lClientOptions = new Bittrex.Net.Objects.BittrexClientOptions { ApiCredentials = lCredentials };
            BittrexSocketClientOptions lSocketOptions = new Bittrex.Net.Objects.BittrexSocketClientOptions { ApiCredentials = lCredentials };

            BittrexClient.SetDefaultOptions(lClientOptions);
            BittrexSocketClient.SetDefaultOptions(lSocketOptions);

            using (BittrexClient lClient = new BittrexClient())
            {
                CallResult<BittrexBalance> lResponse = lClient.GetBalanceAsync("BTC").Result;
                if (!lResponse.Success)
                    throw new PandoraExchangeExceptions.InvalidExchangeCredentials("Incorrect Key Pair for selected exchange");
            }
            //Note: I generate a new instance of ApiCredentials because internally the library dispose it
            FUserCredentials = new Tuple<string, string>(aApiKey, aApiSecret);
            IsCredentialsSet = true;
        }

        public void Clear()
        {
            BittrexClientOptions lClientOptions = new Bittrex.Net.Objects.BittrexClientOptions { ApiCredentials = null };
            BittrexSocketClientOptions lSocketOptions = new Bittrex.Net.Objects.BittrexSocketClientOptions { ApiCredentials = null };
            BittrexClient.SetDefaultOptions(lClientOptions);
            BittrexSocketClient.SetDefaultOptions(lSocketOptions);
            IsCredentialsSet = false;
        }

        public decimal GetTransactionsFee(ICurrencyIdentity aCurrency)
        {
            if (!FCurrencyFees.ContainsKey(aCurrency.Ticker))
                GetCurrencyRelatedData();
            return FCurrencyFees[aCurrency.Ticker];
        }

        public int GetConfirmations(ICurrencyIdentity aCurrency)
        {
            if (!FCurrencyConfirmations.ContainsKey(aCurrency.Ticker))
                GetCurrencyRelatedData();
            if (!FCurrencyConfirmations.ContainsKey(aCurrency.Ticker))
                return -1;
            return FCurrencyConfirmations[aCurrency.Ticker];
        }

        private void GetCurrencyRelatedData()
        {
            using (BittrexClient lClient = new BittrexClient())
            {
                CallResult<IEnumerable<BittrexCurrency>> lResponse = lClient.GetCurrenciesAsync().Result;

                if (!lResponse.Success)
                    throw new Exception("Failed to retrieve balance");

                foreach (BittrexCurrency lCurrency in lResponse.Data)
                {
                    FCurrencyFees[lCurrency.Symbol] = lCurrency.TransactionFee;
                    FCurrencyConfirmations[lCurrency.Symbol] = lCurrency.MinConfirmations + 1;
                }
            }
        }

        public decimal GetBalance(IExchangeMarket aMarket)
        {
            if (!IsCredentialsSet)
            {
                throw new Exception("No Credentials were set");
            }

            using (BittrexClient lClient = new BittrexClient())
            {
                if (!aMarket.TryCastToLocalMarket(out ExchangeMarket lExchangeMarket))
                    throw new ArgumentException(nameof(aMarket), "Invalid Market");
                CallResult<BittrexBalance> lResponse = lClient.GetBalanceAsync(lExchangeMarket.MarketPairID).Result;

                if (!lResponse.Success)
                {
                    throw new Exception("Failed to retrieve balance");
                }

                return lResponse.Data.Available;
            }
        }

        public bool PlaceOrder(UserTradeOrder aOrder, bool aUseProxy = true)
        {
            if (!IsCredentialsSet)
            {
                throw new Exception("No Credentials were set");
            }

            if (aOrder == null)
                throw new ArgumentNullException(nameof(aOrder), $"Invalid order{aOrder.ID}");

            if (aOrder.Status != OrderStatus.Waiting)
                return false;

            BittrexClientOptions lBittrexClientOptions = new BittrexClientOptions()
            {
                Proxy = PandoraProxy.GetApiProxy(),
                ApiCredentials = new ApiCredentials(FUserCredentials.Item1, FUserCredentials.Item2)
            };
            using (BittrexClient lClient = aUseProxy ? new BittrexClient(lBittrexClientOptions) : new BittrexClient())
            {
                if (!aOrder.Market.TryCastToLocalMarket(out ExchangeMarket lExchangeMarket))
                    throw new ArgumentException(nameof(aOrder.Market), "Invalid Market");
                CallResult<BittrexOrder> lResponse;
                if (aOrder.Market.MarketDirection == MarketDirection.Sell)
                    lResponse = lClient.PlaceOrderAsync(lExchangeMarket.MarketPairID, OrderSide.Sell, OrderType.Limit, TimeInForce.GoodTillCancelled, aOrder.SentQuantity, limit: aOrder.Rate).Result;
                else
                    lResponse = lClient.PlaceOrderAsync(lExchangeMarket.MarketPairID, OrderSide.Buy, OrderType.Limit, TimeInForce.GoodTillCancelled, aOrder.SentQuantity / aOrder.Rate, limit: aOrder.Rate).Result;
                if (!lResponse.Success)
                    throw new Exception("Bittrex Error. Message: " + lResponse.Error.Message);

                string lUuid = lResponse.Data.Id;
                CallResult<BittrexOrder> lResponse2 = lClient.GetOrderAsync(lUuid).Result;
                if (!lResponse.Success)
                    throw new Exception("Failed to verify order with server");

                BittrexOrder lReceivedOrder = lResponse2.Data;

                aOrder.ID = lUuid;

                aOrder.OpenTime = lReceivedOrder.CreatedAt;
                aOrder.Cancelled = lReceivedOrder.OrderToCancel != null;
                aOrder.Completed = lReceivedOrder.ClosedAt.HasValue;

                return true;
            }
        }

        public void CancelOrder(UserTradeOrder aOrder, bool aUseProxy = true)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");

            if (aOrder == null)
                throw new ArgumentNullException(nameof(aOrder), "Invalid argument: " + nameof(aOrder));

            BittrexClientOptions lBittrexClientOptions = new BittrexClientOptions()
            {
                Proxy = PandoraProxy.GetApiProxy(),
                ApiCredentials = new ApiCredentials(FUserCredentials.Item1, FUserCredentials.Item2)
            };
            using (BittrexClient lClient = aUseProxy ? new BittrexClient(lBittrexClientOptions) : new BittrexClient())
            {
                var lResponse = lClient.CancelOrderAsync(aOrder.ID).Result;
                if (!lResponse.Success)
                    throw new Exception("Failed to cancel order in exchange. Message: " + lResponse.Error.Message);
            }
        }

        private async Task DoUpdateMarketCoins()
        {
            if (FLastMarketCoinsRetrieval == DateTime.MinValue || FLastMarketCoinsRetrieval < DateTime.UtcNow.AddHours(-12))
            {
                using (BittrexClient lClient = new BittrexClient())
                {
                    var lResponse = await lClient.GetSymbolsAsync();

                    if (!lResponse.Success)
                        throw new Exception("Failed to retrieve Markets");

                    FMarkets = lResponse.Data.ToArray();
                    FLastMarketCoinsRetrieval = DateTime.UtcNow;
                }
                FLocalCacheOfMarkets.Clear();
            }
        }

        public IEnumerable<IExchangeMarket> GetMarketCoins(ICurrencyIdentity aCurrency, GetWalletIDDelegate aGetWalletIDFunction)
        {
            DoUpdateMarketCoins().Wait();
            if (!FLocalCacheOfMarkets.TryGetValue(aCurrency.Id, out IEnumerable<BittrexExchangeMarket> lCoinMarkets))
            {
                lCoinMarkets = FMarkets.Where(x => x.Status == SymbolStatus.Online && (x.QuoteCurrency == aCurrency.Ticker || x.BaseCurrency == aCurrency.Ticker))
                       .Select(lMarket => new BittrexExchangeMarket(this, aCurrency, aGetWalletIDFunction, lMarket)).ToArray();
                if (aGetWalletIDFunction != null)
                    FLocalCacheOfMarkets.TryAdd(aCurrency.Id, lCoinMarkets);
            }

            return lCoinMarkets;
        }

        private class BittrexExchangeMarket : ExchangeMarket
        {
            public BittrexExchangeMarket(BittrexExchange aExchangeInstance, ICurrencyIdentity aCurrency, GetWalletIDDelegate aGetWalletIDFunction, BittrexSymbol aMarketPair) : base(aExchangeInstance)
            {
                SellingCurrencyInfo = aCurrency;
                BuyingCurrencyInfo = new CurrencyInfo(aGetWalletIDFunction)
                {
                    Ticker = aMarketPair.BaseCurrency == aCurrency.Ticker ? aMarketPair.QuoteCurrency : aMarketPair.BaseCurrency,
                    Name = aMarketPair.BaseCurrency == aCurrency.Ticker ? aMarketPair.QuoteCurrency : aMarketPair.BaseCurrency,
                    Id = aGetWalletIDFunction?.Invoke(aMarketPair.BaseCurrency == aCurrency.Ticker ? aMarketPair.QuoteCurrency : aMarketPair.BaseCurrency) ?? 0
                };
                MarketPairID = aMarketPair.Symbol;
                MarketBaseCurrencyInfo = string.Equals(aMarketPair.BaseCurrency, aCurrency.Ticker, StringComparison.OrdinalIgnoreCase) ? BuyingCurrencyInfo : SellingCurrencyInfo;
                MinimumTrade = aMarketPair.MinTradeSize;
            }
        }

        public string GetDepositAddress(IExchangeMarket aMarket)
        {
            if (!IsCredentialsSet)
            {
                throw new Exception("No Credentials were set");
            }

            using (BittrexClient lClient = new BittrexClient())
            {
                CallResult<BittrexDepositAddress> lResponse = lClient.GetDepositAddressAsync(aMarket.SellingCurrencyInfo.Ticker).Result;

                if (!lResponse.Success)
                {
                    throw new Exception("Failed to retrieve Bittrex Address. " + lResponse.Error.Message);
                }

                BittrexDepositAddress lAddress = lResponse.Data;

                return lAddress.Address;
            }
        }

        public void StartMarketPriceUpdating()
        {
            if (FMarketPriceSubscription != null)
                return;

            FPriceUpdaterWatcherTimer = new Timer((_) => DoCheckSocketConnected(), null, 0, Timeout.Infinite);
            FDispatchNewMarketPricesTimer = new Timer((_) => DoDispatchMarketPriceChanging(), null, 0, 10000);

            var lResult = FBittrexSocket.SubscribeToSymbolTickerUpdatesAsync((lBittrexSummaries) =>
            {
                try
                {
                    lock (FPriceMarketLock)
                        FLastSocketPriceUpdate = DateTime.UtcNow;
                    ProcessMarketSummaries(lBittrexSummaries.Data.Deltas);
                }
                catch (Exception ex)
                {
                    Log.Write(LogLevel.Error, $"Exception thrown when updating market price. Details: {ex}");
                }
            }).Result;
            if (!lResult.Success)
                throw new Exception($"Failed to subscribe to market price updates. Details: {lResult.Error?.Message ?? "No info"}");

            FMarketPriceSubscription = lResult.Data;
            FPriceUpdaterWatcherTimer.Change(60000, 60000); //Every Minute
        }

        private void DoCheckSocketConnected()
        {
            try
            {
                DateTime lLastPriceTime;
                lock (FPriceMarketLock)
                    lLastPriceTime = FLastSocketPriceUpdate;
                if (lLastPriceTime < DateTime.UtcNow.AddMinutes(-5))
                {
                    if (lLastPriceTime != DateTime.MinValue)
                        Log.Write(LogLevel.Warning, "No updates received from socket in 5 minutes. Attempting manual request");
                    using (BittrexClient lClient = new BittrexClient())
                    {
                        var lResponse = lClient.GetTickersAsync().Result;
                        if (lResponse.Success)
                            ProcessMarketSummaries(lResponse.Data);
                        else
                            throw new Exception(lResponse.Error.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Critical, $"Failed to get market summaries from bittrex server. Exception thrown: {ex}");
            }
        }

        private void DoDispatchMarketPriceChanging()
        {
            var lUpdatedMarkets = from lMarket in FLocalCacheOfMarkets.Values.SelectMany(lMarkets => lMarkets)
                                  join lUpdatedId in FMarketsIDsWithPriceChanged on lMarket.MarketPairID equals lUpdatedId
                                  select lMarket;
            if (lUpdatedMarkets.Any())
                OnMarketPricesChanging?.Invoke(lUpdatedMarkets);
            FMarketsIDsWithPriceChanged = new ConcurrentBag<string>();
        }

        private void ProcessMarketSummaries(IEnumerable<BittrexTick> aMarketPrices)
        {
            Parallel.ForEach(aMarketPrices, (lMarketPrice) =>
            {
                var lMarketPairID = lMarketPrice.Symbol;
                MarketPriceInfo lRemoteMarketPrice = new MarketPriceInfo
                {
                    Last = lMarketPrice.LastTradeRate,
                    Bid = lMarketPrice.BidRate,
                    Ask = lMarketPrice.AskRate
                };

                if ((FMarketPrices.TryGetValue(lMarketPairID, out MarketPriceInfo lPrice) && lPrice != lRemoteMarketPrice) || lPrice == null)
                {
                    FMarketPrices.AddOrUpdate(lMarketPairID, lRemoteMarketPrice, (key, oldValue) => lRemoteMarketPrice);
                    if (!FMarketsIDsWithPriceChanged.Contains(lMarketPairID))
                        FMarketsIDsWithPriceChanged.Add(lMarketPairID);
                }
            });
        }

        public TradeOrderStatusInfo GetOrderStatus(string lUuid)
        {
            using (BittrexClient lClient = new BittrexClient())
            {
                CallResult<BittrexOrder> lResponse = lClient.GetOrderAsync(lUuid).Result;
                if (!lResponse.Success)
                {
                    throw new Exception("Failed to retrieve order specified");
                }

                return new TradeOrderStatusInfo
                {
                    ID = lResponse.Data.Id,
                    Completed = lResponse.Data.ClosedAt.HasValue,
                    Cancelled = lResponse.Data.OrderToCancel != null,
                    Rate = lResponse.Data.Limit ?? -1
                };
            }
        }

        /// <summary>
        /// Tries to withdraw the total amount returned by an order from the exchange
        /// </summary>
        /// <param name="aMarket">Market of the order</param>
        /// <param name="aOrder">Completed order to withdraw</param>
        /// <param name="aAddress">Coin address to deposit funds</param>
        /// <param name="aTxFee">Fee to discount from balance withdrawed</param>
        /// <param name="aUseProxy">Use pandora proxy or not</param>
        public bool RefundOrder(UserTradeOrder aOrder, string aAddress, bool aUseProxy = true)
        {
            if (!IsCredentialsSet)
            {
                throw new Exception("No Credentials were set");
            }

            if (aOrder.Status == OrderStatus.Withdrawn)
            {
                return false;
            }

            BittrexClientOptions lBittrexClientOptions = new BittrexClientOptions()
            {
                Proxy = PandoraProxy.GetApiProxy(),
                ApiCredentials = new ApiCredentials(FUserCredentials.Item1, FUserCredentials.Item2)
            };
            using (BittrexClient lClient = aUseProxy ? new BittrexClient(lBittrexClientOptions) : new BittrexClient())
            {
                var lResponse = lClient.WithdrawAsync(aOrder.Market.SellingCurrencyInfo.Ticker, aOrder.SentQuantity, aAddress).Result;
                if (!lResponse.Success)
                    throw new Exception("Failed to withdraw. Error message:" + lResponse.Error.Message);
            }

            return true;
        }

        public IMarketPriceInfo GetMarketPrice(IExchangeMarket aMarket)
        {
            MarketPriceInfo lResult = new MarketPriceInfo();
            if (!aMarket.TryCastToLocalMarket(out ExchangeMarket lExchangeMarket))
                throw new ArgumentException(nameof(aMarket), "Invalid Market");
            if (FMarketPrices.TryGetValue(lExchangeMarket.MarketPairID, out MarketPriceInfo lValue))
                lResult = lValue;
            return lResult;
        }

        /// <summary>
        /// Tries to withdraw the total amount returned by an order from the exchange
        /// </summary>
        /// <param name="aMarket">Market of the order</param>
        /// <param name="aOrder">Completed order to withdraw</param>
        /// <param name="aAddress">Coin address to deposit funds</param>
        /// <param name="aTxFee">Fee to discount from balance withdrawed</param>
        /// <param name="aUseProxy">Use pandora proxy or not</param>
        public bool WithdrawOrder(UserTradeOrder aOrder, string aAddress, decimal aTxFee, bool aUseProxy = true)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");

            if (aOrder.Status == OrderStatus.Withdrawn)
                return false;

            BittrexClientOptions lBittrexClientOptions = new BittrexClientOptions()
            {
                Proxy = PandoraProxy.GetApiProxy(),
                ApiCredentials = new ApiCredentials(FUserCredentials.Item1, FUserCredentials.Item2)
            };
            using (BittrexClient lClient = aUseProxy ? new BittrexClient(lBittrexClientOptions) : new BittrexClient())
            {
                decimal lQuantityToWithdraw = 0;
                CallResult<BittrexOrder> lGetOrderResponse = lClient.GetOrderAsync(aOrder.ID).Result;
                if (!lGetOrderResponse.Success)
                    throw new Exception("Failed to get order to withdraw. Error message:" + lGetOrderResponse.Error.Message);
                var lRemoteOrder = lGetOrderResponse.Data;
                if (lRemoteOrder.Direction == OrderSide.Buy)
                    lQuantityToWithdraw = lRemoteOrder.FillQuantity;
                else if (lRemoteOrder.Direction == OrderSide.Sell)
                    lQuantityToWithdraw = lRemoteOrder.Proceeds - lRemoteOrder.Commission;
                var lWithdrawResponse = lClient.WithdrawAsync(aOrder.Market.BuyingCurrencyInfo.Ticker, lQuantityToWithdraw, aAddress).Result;
                if (!lWithdrawResponse.Success)
                    throw new Exception("Failed to withdraw. Error message:" + lWithdrawResponse.Error.Message);
            }
            return true;
        }

        public void StopMarketUpdating()
        {
            if (FMarketPriceSubscription == null)
                return;
            FPriceUpdaterWatcherTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            FPriceUpdaterWatcherTimer?.Dispose();
            FPriceUpdaterWatcherTimer = null;
            FDispatchNewMarketPricesTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            FDispatchNewMarketPricesTimer?.Dispose();
            FDispatchNewMarketPricesTimer = null;
            FMarketPriceSubscription.CloseAsync().Wait(5000);
            FMarketPriceSubscription = null;
        }

        public IEnumerable<CandlestickPoint> GetCandleStickChart(IExchangeMarket aMarket, DateTime aStartTime, DateTime aEndTime, ChartInterval aChartInterval)
        {
            IEnumerable<CandlestickPoint> lResult;
            using (BittrexClient lClient = new BittrexClient())
            {
                KlineInterval lInterval;
                switch (aChartInterval)
                {
                    case ChartInterval.FiveMinutes:
                        lInterval = KlineInterval.FiveMinutes;
                        break;

                    case ChartInterval.Hourly:
                        lInterval = KlineInterval.OneHour;
                        break;

                    case ChartInterval.Daily:
                        lInterval = KlineInterval.OneDay;
                        break;

                    default:
                        lInterval = KlineInterval.OneHour;
                        break;
                }
                if (!aMarket.TryCastToLocalMarket(out ExchangeMarket lExchangeMarket))
                    throw new ArgumentException(nameof(aMarket), "Invalid Market");
                var lResponse = lClient.GetKlinesAsync(lExchangeMarket.MarketPairID, lInterval).Result;
                if (!lResponse.Success)
                    throw new Exception($"Failed to get chart data. Error message: {lResponse.Error.Message}");
                lResult = lResponse.Data.Where(lPoint => lPoint.StartsAt.ToUniversalTime() >= aStartTime.ToUniversalTime() && lPoint.StartsAt.ToUniversalTime() <= aEndTime.ToUniversalTime()).Select(lPoint => new CandlestickPoint
                {
                    High = lPoint.High,
                    Low = lPoint.Low,
                    Open = lPoint.Open,
                    Close = lPoint.Close,
                    TimeStamp = lPoint.StartsAt
                });
            }
            return lResult;
        }
    }
}