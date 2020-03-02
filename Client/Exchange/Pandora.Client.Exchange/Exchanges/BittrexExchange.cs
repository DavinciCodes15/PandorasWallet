using Bittrex.Net;
using Bittrex.Net.Objects;
using Pandora.Client.Exchange.JKrof.Objects;
using Pandora.Client.Exchange.Objects;
using Pandora.Client.Exchange.SaveManagers;
using Pandora.Client.Universal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.Exchanges
{
    internal class BittrexExchange : AbstractExchange, IPandoraExchanger
    {
        public new const AvailableExchangesList Identifier = AvailableExchangesList.Bittrex;        

        private BittrexSocketClient FBittrexSocket;
        private static ConcurrentDictionary<string, MarketPriceInfo> FMarketPrices;
        private static Pandora.Client.Exchange.JKrof.Sockets.UpdateSubscription FMarketPriceSubscription;

        /// <summary>
        /// Tuple with user credentials. First element is Key, second is Secret
        /// </summary>
        private Tuple<string, string> FUserCredentials;
        public bool IsCredentialsSet { get; private set; }

        public override string Name => Identifier.ToString();

        public override int ID => (int) Identifier;

        public event Action<IEnumerable<string>,int> OnMarketPricesChanging;

        private ConcurrentDictionary<string, decimal> FCurrencyFees;
        private ConcurrentDictionary<string, int> FCurrencyConfirmations;
        private ConcurrentDictionary<string, ExchangeMarket[]> FLocalCacheOfMarkets;
        private BittrexSymbol[] FMarkets;
        private DateTime FLastMarketCoinsRetrieval;
        private DateTime FLastSocketPriceUpdate;
        private static event Action<IEnumerable<string>, int> OnMarketPricesChangingInternal;
        private static Timer FPriceUpdaterWatcherTimer;

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

            FCurrencyFees = new ConcurrentDictionary<string, decimal>();
            FCurrencyConfirmations = new ConcurrentDictionary<string, int>();
            FLocalCacheOfMarkets = new ConcurrentDictionary<string, ExchangeMarket[]>();
            OnMarketPricesChangingInternal += BittrexExchange_OnMarketPricesChangingInternal;
            DoUpdateMarketCoins();
        }

        private void BittrexExchange_OnMarketPricesChangingInternal(IEnumerable<string> arg1, int arg2)
        {
            OnMarketPricesChanging?.Invoke(arg1, arg2);
        }

        public void SetCredentials(string aApiKey, string aApiSecret)
        {
            JKrof.Authentication.ApiCredentials lCredentials = new JKrof.Authentication.ApiCredentials(aApiKey, aApiSecret);

            BittrexClientOptions lClientOptions = new Bittrex.Net.Objects.BittrexClientOptions { ApiCredentials = lCredentials };
            BittrexSocketClientOptions lSocketOptions = new Bittrex.Net.Objects.BittrexSocketClientOptions { ApiCredentials = lCredentials };

            BittrexClient.SetDefaultOptions(lClientOptions);
            BittrexSocketClient.SetDefaultOptions(lSocketOptions);

            using (BittrexClient lClient = new BittrexClient())
            {
                CallResult<BittrexBalance> lResponse = lClient.GetBalance("BTC");
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

        public decimal GetTransactionsFee(string aCurrencyName, string aTicker)
        {
            if (!FCurrencyFees.ContainsKey(aTicker))
            {
                GetCurrencyRelatedData();
            }
            return FCurrencyFees[aTicker];
        }

        public int GetConfirmations(string aCurrencyName,  string aTicker)
        {
            if (!FCurrencyConfirmations.ContainsKey(aTicker))
            {
                GetCurrencyRelatedData();
            }

            if (!FCurrencyConfirmations.ContainsKey(aTicker))
            {
                return -1;
            }

            return FCurrencyConfirmations[aTicker];
        }

        private void GetCurrencyRelatedData()
        {
            using (BittrexClient lClient = new BittrexClient())
            {
                CallResult<IEnumerable<BittrexCurrency>> lResponse = lClient.GetCurrencies();

                if (!lResponse.Success)
                    throw new Exception("Failed to retrieve balance");                

                foreach (BittrexCurrency it in lResponse.Data)
                {
                    FCurrencyFees[it.Currency] = it.TransactionFee;
                    FCurrencyConfirmations[it.Currency] = it.MinConfirmation + 1;
                }
            }
        }

        public decimal GetBalance(ExchangeMarket aMarket)
        {
            if (!IsCredentialsSet)
            {
                throw new Exception("No Credentials were set");
            }

            using (BittrexClient lClient = new BittrexClient())
            {
                CallResult<BittrexBalance> lResponse = lClient.GetBalance(aMarket.MarketName);

                if (!lResponse.Success)
                {
                    throw new Exception("Failed to retrieve balance");
                }

                return lResponse.Data.Available.Value;
            }
        }

        public bool PlaceOrder(UserTradeOrder aOrder, ExchangeMarket aMarket, bool aUseProxy = true)
        {
            if (!IsCredentialsSet)
            {
                throw new Exception("No Credentials were set");
            }

            if (aOrder == null || aMarket == null)
            {
                throw new ArgumentNullException(aOrder == null ? nameof(aOrder) : nameof(aMarket), "Invalid argument: " + aOrder == null ? nameof(aOrder) : nameof(aMarket));
            }

            if (aOrder.Status != OrderStatus.Waiting)
                return false;

            BittrexClientOptions lBittrexClientOptions = new BittrexClientOptions()
            {
                Proxy = PandoraProxy.GetApiProxy(),
                ApiCredentials = new JKrof.Authentication.ApiCredentials(FUserCredentials.Item1, FUserCredentials.Item2)
            };
            using (BittrexClient lClient = aUseProxy ? new BittrexClient(lBittrexClientOptions) : new BittrexClient())
            {
                CallResult<Bittrex.Net.Objects.BittrexGuid> lResponse;

                if (aMarket.IsSell)
                {
                    lResponse = lClient.PlaceOrder(Bittrex.Net.Objects.OrderSide.Sell, aOrder.ExchangeMarketName, aOrder.SentQuantity, aOrder.Rate);
                }
                else
                {
                    lResponse = lClient.PlaceOrder(Bittrex.Net.Objects.OrderSide.Buy, aOrder.ExchangeMarketName, aOrder.SentQuantity / aOrder.Rate, aOrder.Rate);
                }

                if (!lResponse.Success)
                {
                    throw new Exception("Bittrex Error. Message: " + lResponse.Error.Message);
                }

                string lUuid = lResponse.Data.Uuid.ToString();

                CallResult<BittrexAccountOrder> lResponse2 = lClient.GetOrder(new Guid(lUuid));

                if (!lResponse.Success)
                {
                    throw new Exception("Failed to verify order with server");
                }

                BittrexAccountOrder lReceivedOrder = lResponse2.Data;

                aOrder.ID = lUuid;

                aOrder.OpenTime = lReceivedOrder.Opened;
                aOrder.Cancelled = lReceivedOrder.CancelInitiated;
                aOrder.Completed = lReceivedOrder.QuantityRemaining == 0;

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
                ApiCredentials = new JKrof.Authentication.ApiCredentials(FUserCredentials.Item1, FUserCredentials.Item2)
            };
            using (BittrexClient lClient = aUseProxy ? new BittrexClient(lBittrexClientOptions) : new BittrexClient())
            {
                var lResponse = lClient.CancelOrder(new Guid(aOrder.ID));
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

        public ExchangeMarket[] GetMarketCoins(string aCurrencyName, string aTicker, GetWalletIDDelegate aGetWalletIDFunction = null)
        {
            DoUpdateMarketCoins().Wait();
            if (!FLocalCacheOfMarkets.TryGetValue(aTicker, out ExchangeMarket[] lCoinMarkets))
            {
                lCoinMarkets = FMarkets.Where(x => x.IsActive == true && (x.QuoteCurrency == aTicker || x.BaseCurrency == aTicker))
                       .Select(lMarket => new ExchangeMarket(this)
                       {
                           BaseCurrencyInfo = new ExchangeMarket.CurrencyInfo
                           {
                               Ticker = aTicker,
                               Name = lMarket.BaseCurrency == aTicker ? lMarket.BaseCurrencyLong : lMarket.QuoteCurrencyLong,
                               WalletID = aGetWalletIDFunction?.Invoke(aCurrencyName, aTicker)
                           },
                           DestinationCurrencyInfo = new ExchangeMarket.CurrencyInfo
                           {
                               Ticker = lMarket.BaseCurrency == aTicker ? lMarket.QuoteCurrency : lMarket.BaseCurrency,
                               Name = lMarket.BaseCurrency == aTicker ? lMarket.QuoteCurrencyLong : lMarket.BaseCurrencyLong,
                               WalletID = aGetWalletIDFunction?.Invoke(lMarket.BaseCurrency == aTicker ? lMarket.QuoteCurrencyLong : lMarket.BaseCurrencyLong, lMarket.BaseCurrency == aTicker ? lMarket.QuoteCurrency : lMarket.BaseCurrency)
                           },
                           MarketName = lMarket.Symbol,
                           IsSell = lMarket.BaseCurrency != aTicker,
                           MinimumTrade = lMarket.MinTradeSize
                       }).ToArray();

                if (aGetWalletIDFunction != null)
                    FLocalCacheOfMarkets.TryAdd(aTicker, lCoinMarkets);
            }

            return lCoinMarkets;
        }

        public string GetDepositAddress(ExchangeMarket aMarket)
        {
            if (!IsCredentialsSet)
            {
                throw new Exception("No Credentials were set");
            }

            using (BittrexClient lClient = new BittrexClient())
            {
                CallResult<BittrexDepositAddress> lResponse = lClient.GetDepositAddress(aMarket.BaseCurrencyInfo.Ticker);

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

            FPriceUpdaterWatcherTimer = new Timer((lState) =>
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
                            CallResult<IEnumerable<BittrexSymbolSummary>> lResponse = lClient.GetSymbolSummaries();
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
            }, null, 0, Timeout.Infinite);

            CallResult<Pandora.Client.Exchange.JKrof.Sockets.UpdateSubscription> lResult = FBittrexSocket.SubscribeToSymbolSummariesUpdate((lBittrexSummaries) =>
            {
                try
                {
                    lock (FPriceMarketLock)
                        FLastSocketPriceUpdate = DateTime.UtcNow;
                    ProcessMarketSummaries(lBittrexSummaries);
                }
                catch (Exception ex)
                {
                    Log.Write(LogLevel.Error, $"Exception thrown when updating market price. Details: {ex}");
                }
            });
            if (!lResult.Success)
                throw new Exception($"Failed to subscribe to market price updates. Details: {lResult.Error?.Message ?? "No info"}");

            FMarketPriceSubscription = lResult.Data;
            FPriceUpdaterWatcherTimer.Change(60000, 60000); //Every Minute
        }

        private static void ProcessMarketSummaries(IEnumerable<JKrof.Bittrex.Net.Interfaces.IBittrexSymbolSummary> aMarketPrices)
        {
            List<string> lChanged = new List<string>();
            foreach (var lMarketPrice in aMarketPrices)
                if (lMarketPrice.Last.HasValue)
                {
                    MarketPriceInfo lRemoteMarketPrice = new MarketPriceInfo
                    {
                        Last = lMarketPrice.Last.Value,
                        Bid = lMarketPrice.Bid.Value,
                        Ask = lMarketPrice.Ask.Value
                    };

                    if (FMarketPrices.TryGetValue(lMarketPrice.Symbol, out MarketPriceInfo lPrice))
                    {
                        if (lPrice != lRemoteMarketPrice)
                        {
                            FMarketPrices.AddOrUpdate(lMarketPrice.Symbol, lRemoteMarketPrice, (key, oldValue) => lRemoteMarketPrice);
                            lChanged.Add(lMarketPrice.Symbol);
                        }
                    }
                    else
                    {
                        FMarketPrices.TryAdd(lMarketPrice.Symbol, lRemoteMarketPrice);
                        lChanged.Add(lMarketPrice.Symbol);
                    }
                }
            if (lChanged.Any())
                OnMarketPricesChangingInternal?.Invoke(lChanged, (int)Identifier);
        }

        public TradeOrderStatusInfo GetOrderStatus(string lUuid)
        {
            using (BittrexClient lClient = new BittrexClient())
            {
                CallResult<BittrexAccountOrder> lResponse = lClient.GetOrder(Guid.Parse(lUuid));
                if (!lResponse.Success)
                {
                    throw new Exception("Failed to retrieve order specified");
                }

                return new TradeOrderStatusInfo
                {
                    ID = lResponse.Data.OrderUuid.ToString(),
                    Completed = lResponse.Data.QuantityRemaining == 0,
                    Cancelled = lResponse.Data.CancelInitiated,
                    ExchangeMarketName = lResponse.Data.Exchange,
                    Rate = lResponse.Data.PricePerUnit ?? -1
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
        public bool RefundOrder(ExchangeMarket aMarket, UserTradeOrder aOrder, string aAddress, bool aUseProxy = true)
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
                ApiCredentials = new JKrof.Authentication.ApiCredentials(FUserCredentials.Item1, FUserCredentials.Item2)
            };
            using (BittrexClient lClient = aUseProxy ? new BittrexClient(lBittrexClientOptions) : new BittrexClient())
            {
                CallResult<BittrexGuid> lResponse = lClient.Withdraw(aMarket.BaseCurrencyInfo.Ticker, aOrder.SentQuantity, aAddress);
                if (!lResponse.Success)
                {
                    throw new Exception("Failed to withdraw. Error message:" + lResponse.Error.Message);
                }
            }

            return true;
        }

        public MarketPriceInfo GetMarketPrice(string aMarketName)
        {
            MarketPriceInfo lResult = new MarketPriceInfo();
            if (FMarketPrices.TryGetValue(aMarketName, out MarketPriceInfo lValue))
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
        public bool WithdrawOrder(ExchangeMarket aMarket, UserTradeOrder aOrder, string aAddress, decimal aTxFee, bool aUseProxy = true)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");

            if (aOrder.Status == OrderStatus.Withdrawn)
                return false;

            BittrexClientOptions lBittrexClientOptions = new BittrexClientOptions()
            {
                Proxy = PandoraProxy.GetApiProxy(),
                ApiCredentials = new JKrof.Authentication.ApiCredentials(FUserCredentials.Item1, FUserCredentials.Item2)
            };
            using (BittrexClient lClient = aUseProxy ? new BittrexClient(lBittrexClientOptions) : new BittrexClient())
            {
                decimal lQuantityToWithdraw = 0;
                CallResult<BittrexAccountOrder> lGetOrderResponse = lClient.GetOrder(new Guid(aOrder.ID));
                if (!lGetOrderResponse.Success)
                    throw new Exception("Failed to get order to withdraw. Error message:" + lGetOrderResponse.Error.Message);
                var lRemoteOrder = lGetOrderResponse.Data;
                if (lRemoteOrder.Price <= 0 || !lRemoteOrder.PricePerUnit.HasValue)
                    throw new Exception("Order is not fulfilled or data from server is missing");
                if (lRemoteOrder.Type == OrderSideExtended.LimitBuy)
                    lQuantityToWithdraw = lRemoteOrder.Quantity;
                else if (lRemoteOrder.Type == OrderSideExtended.LimitSell)
                    lQuantityToWithdraw = (lRemoteOrder.Quantity * lRemoteOrder.PricePerUnit.Value) - lRemoteOrder.CommissionPaid;
                CallResult<BittrexGuid> lWithdrawResponse = lClient.Withdraw(aMarket.DestinationCurrencyInfo.Ticker, lQuantityToWithdraw, aAddress);
                if (!lWithdrawResponse.Success)
                    throw new Exception("Failed to withdraw. Error message:" + lWithdrawResponse.Error.Message);
            }
            return true;
        }

        public void StopMarketUpdating()
        {
            OnMarketPricesChangingInternal -= BittrexExchange_OnMarketPricesChangingInternal;
            if (FMarketPriceSubscription == null || OnMarketPricesChangingInternal != null)
                return;
            FPriceUpdaterWatcherTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            FPriceUpdaterWatcherTimer?.Dispose();
            FPriceUpdaterWatcherTimer = null;
            _ = FMarketPriceSubscription.Close();
            FMarketPriceSubscription = null;
            
        }
    }
}
