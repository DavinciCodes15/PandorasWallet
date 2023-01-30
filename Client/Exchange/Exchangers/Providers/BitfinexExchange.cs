using Bitfinex.Net;
using Bitfinex.Net.Objects;
using Bitfinex.Net.Objects.RestV1Objects;
using CryptoExchange.Net.Authentication;
using Pandora.Client.ClientLib.Contracts;
using Pandora.Client.Exchange.Contracts;
using Pandora.Client.Exchange.Exchangers.Contracts;
using Pandora.Client.Exchange.Models;
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
    internal class BitfinexExchange : AbstractExchange, IPandoraExchanger
    {
        //This info is not provided in any place by the api calls, so I manually wrote it in
        private Dictionary<string, int> FConfirmations = new Dictionary<string, int>
        {
            {"BTC", 3 },
            {"BCH", 1},
            {"DGB",  100},
            {"LTC", 6},
            {"QTUM", 20},
            {"XVG",  12},
            {"DASH",  9}
        };

        private const decimal PERCENT_TRADE_FEE = 0.1M;

        public new const AvailableExchangesList Identifier = AvailableExchangesList.Bitfinex;
        private const int PRICE_REFRESH_PERIOD = 10000;

        private DateTime FLastMarketCoinsRetrieval;
        private DateTime FLastFeesDataRetrieval;
        private BitfinexPair[] FMarkets;
        private ConcurrentDictionary<string, decimal> FWithdrawnFeesCache;
        private ConcurrentDictionary<long, IEnumerable<BitfinexExchangeMarket>> FLocalCacheOfMarkets;

        public override string Name => Identifier.ToString();

        public override int ID => (int)Identifier;

        public event Action<IEnumerable<IExchangeMarket>> OnMarketPricesChanging;

        private Tuple<string, string> FUserCredentials;
        private static Timer FPriceUpdaterTask;
        private static ConcurrentDictionary<string, MarketPriceInfo> FMarketPrices;

        public bool IsCredentialsSet { get; private set; }

        ~BitfinexExchange()
        {
            StopMarketUpdating();
            FMarkets = null;
            FMarketPrices?.Clear();
            FMarketPrices = null;
            FWithdrawnFeesCache?.Clear();
            FWithdrawnFeesCache = null;
            FUserCredentials = null;
            FLocalCacheOfMarkets?.Clear();
            FLocalCacheOfMarkets = null;
        }

        internal BitfinexExchange()
        {
            FWithdrawnFeesCache = new ConcurrentDictionary<string, decimal>();
            FMarketPrices = new ConcurrentDictionary<string, MarketPriceInfo>();
            FLocalCacheOfMarkets = new ConcurrentDictionary<long, IEnumerable<BitfinexExchangeMarket>>();
            DoUpdatemarketCoins();
        }

        public void CancelOrder(UserTradeOrder aOrder, bool aUseProxy = true)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");

            if (aOrder == null)
                throw new ArgumentNullException(nameof(aOrder), "Invalid argument: " + nameof(aOrder));

            BitfinexClientOptions lBittrexClientOptions = new BitfinexClientOptions()
            {
                Proxy = PandoraProxy.GetApiProxy(),
                ApiCredentials = new ApiCredentials(FUserCredentials.Item1, FUserCredentials.Item2)
            };
            using (BitfinexClient lClient = aUseProxy ? new BitfinexClient(lBittrexClientOptions) : new BitfinexClient())
            {
                var lResponse = lClient.CancelOrderAsync(Convert.ToInt64(aOrder.ID)).Result;
                if (!lResponse.Success)
                    throw new Exception("Failed to cancel order in exchange. Message: " + lResponse.Error.Message);
            }
        }

        public void Clear()
        {
            var lClientOptions = new BitfinexClientOptions { ApiCredentials = null };
            BitfinexClient.SetDefaultOptions(lClientOptions);
            IsCredentialsSet = false;
        }

        public decimal GetBalance(IExchangeMarket aMarket)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");

            using (var lClient = new BitfinexClient())
            {
                var lResponse = lClient.GetBalancesAsync().Result;
                if (!lResponse.Success)
                    throw new Exception($"Failed to retrieve balance. Details: {lResponse.Error}");
                var lBalances = lResponse.Data;
                var lDecimalBalance = lBalances.Where(lBalance => lBalance.Currency == aMarket.SellingCurrencyInfo.Ticker).FirstOrDefault();
                if (lDecimalBalance == null)
                    throw new Exception($"Failed to get balance for currency {aMarket.SellingCurrencyInfo.Name}");
                return lDecimalBalance.Balance;
            }
        }

        public int GetConfirmations(ICurrencyIdentity aCurrency)
        {
            if (!FConfirmations.TryGetValue(aCurrency.Ticker, out int lConfirmations))
                lConfirmations = 6;
            return lConfirmations;
        }

        public string GetDepositAddress(IExchangeMarket aMarket)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");

            using (var lClient = new BitfinexClient())
            {
                var lAddressResponse = lClient.GetDepositAddressAsync(aMarket.SellingCurrencyInfo.Name.ToLowerInvariant(), WithdrawWallet.Exchange).Result;
                if (!lAddressResponse.Success)
                    throw new Exception($"Failed to get deposit address for bitfinex. Details: {lAddressResponse.Error}");
                return lAddressResponse.Data.Address;
            }
        }

        private class BitfinexPair
        {
            public enum MarketType
            { Trade, Founding }

            public static IEnumerable<BitfinexCurrency> ExchangeCurrencies { get; set; }
            public BitfinexSymbolDetails MarketDetails { get; private set; }
            public BitfinexCurrency BaseCurrency { get; private set; }
            public BitfinexCurrency DestinationCurrency { get; private set; }
            public MarketType SymbolType { get; private set; }
            public string ExchangeSymbol { get; private set; }

            public BitfinexPair(string aSymbol, BitfinexSymbolDetails aSymbolDetails)
            {
                if (ExchangeCurrencies == null)
                    throw new Exception("First you need to set a list of currencies from exchange");
                ExchangeSymbol = string.IsNullOrEmpty(aSymbol) ? throw new ArgumentException("Bitfinex symbol can not be empty or null") : aSymbol;
                MarketDetails = aSymbolDetails;
                DeconstructSymbol();
            }

            private void DeconstructSymbol()
            {
                SymbolType = ExchangeSymbol.First() == 't' ? MarketType.Trade : MarketType.Founding;
                var lSymbolWithoutType = ExchangeSymbol.Remove(0, 1);
                DestinationCurrency = ExchangeCurrencies.Where(lCurrency => lSymbolWithoutType.IndexOf(lCurrency.Name) == 0).FirstOrDefault();
                if (DestinationCurrency != null)
                {
                    var lSymbolWithoutDest = lSymbolWithoutType.Replace(DestinationCurrency.Name, string.Empty);
                    BaseCurrency = ExchangeCurrencies.Where(lCurrency => lSymbolWithoutDest == lCurrency.Name).FirstOrDefault();
                }
            }
        }

        private async Task DoUpdatemarketCoins()
        {
            if (FLastMarketCoinsRetrieval == DateTime.MinValue || FLastMarketCoinsRetrieval < DateTime.UtcNow.AddHours(-1))
            {
                using (var lClient = new BitfinexClient())
                {
                    var lCurrenciesResponse = await lClient.GetCurrenciesAsync();
                    var lMarketsResponse = await lClient.GetTickerAsync(symbols: "ALL");
                    var lSymbolDetails = await lClient.GetSymbolDetailsAsync();
                    if (!lCurrenciesResponse.Success || !lMarketsResponse.Success || !lSymbolDetails.Success)
                        throw new Exception("Failed to retrieve Bitfinex Markets");
                    BitfinexPair.ExchangeCurrencies = lCurrenciesResponse.Data;
                    FMarkets = lMarketsResponse.Data.Select(lMarket => new BitfinexPair(lMarket.Symbol, lSymbolDetails.Data.Where(lSymbol => string.Equals(lSymbol.Pair, lMarket.Symbol.Remove(0, 1), StringComparison.OrdinalIgnoreCase)).FirstOrDefault())).ToArray();
                }
                FLastMarketCoinsRetrieval = DateTime.UtcNow;
                FLocalCacheOfMarkets.Clear();
            }
        }

        public IEnumerable<IExchangeMarket> GetMarketCoins(ICurrencyIdentity aCurrency, GetWalletIDDelegate aGetWalletIDFunction)
        {
            DoUpdatemarketCoins().Wait();
            if (!FLocalCacheOfMarkets.TryGetValue(aCurrency.Id, out IEnumerable<BitfinexExchangeMarket> lCoinMarkets))
            {
                var lMarketsWithCompleteData = FMarkets.Where(lMarket => lMarket?.BaseCurrency?.FullName != null && lMarket?.DestinationCurrency?.FullName != null);
                lCoinMarkets = lMarketsWithCompleteData.Where(lMarket => string.Equals(lMarket.DestinationCurrency.FullName, aCurrency.Name, StringComparison.OrdinalIgnoreCase) || string.Equals(lMarket.BaseCurrency.FullName, aCurrency.Name, StringComparison.OrdinalIgnoreCase))
               .Select(lMarket => new BitfinexExchangeMarket(this, aCurrency, aGetWalletIDFunction, lMarket));
                if (aGetWalletIDFunction != null)
                    FLocalCacheOfMarkets.TryAdd(aCurrency.Id, lCoinMarkets);
            }
            return lCoinMarkets;
        }

        private class BitfinexExchangeMarket : ExchangeMarket
        {
            public BitfinexExchangeMarket(BitfinexExchange aExchangeInstance, ICurrencyIdentity aCurrency, GetWalletIDDelegate aGetWalletIDFunction, BitfinexPair aPair) : base(aExchangeInstance)
            {
                var lIsBaseCurrency = string.Equals(aPair.BaseCurrency.FullName, aCurrency.Name, StringComparison.OrdinalIgnoreCase);
                SellingCurrencyInfo = aCurrency;
                var lBuyingCurrencyTicker = lIsBaseCurrency ? aPair.DestinationCurrency.Name : aPair.BaseCurrency.Name;
                BuyingCurrencyInfo = new CurrencyInfo(aGetWalletIDFunction)
                {
                    Ticker = lBuyingCurrencyTicker,
                    Name = lIsBaseCurrency ? aPair.DestinationCurrency.FullName : aPair.BaseCurrency.FullName,
                    Id = aGetWalletIDFunction?.Invoke(lBuyingCurrencyTicker) ?? 0
                };
                MarketPairID = aPair.MarketDetails.Pair;
                MarketBaseCurrencyInfo = lIsBaseCurrency ? SellingCurrencyInfo : BuyingCurrencyInfo;
                MinimumTrade = aPair.MarketDetails?.MinimumOrderSize ?? 0;
            }
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

        public TradeOrderStatusInfo GetOrderStatus(string lUuid)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");

            using (var lClient = new BitfinexClient())
            {
                var lResponse = lClient.GetOrderAsync(Convert.ToInt64(lUuid)).Result;
                if (!lResponse.Success)
                    throw new Exception($"Failed to retrieve order status. Id: {lUuid}");
                return new TradeOrderStatusInfo
                {
                    ID = lUuid,
                    Completed = lResponse.Data.RemainingAmount == 0,
                    Cancelled = lResponse.Data.Canceled,
                    Rate = lResponse.Data.Price
                };
            }
        }

        public decimal GetTransactionsFee(ICurrencyIdentity aCurrency)
        {
            decimal? lResult = null;
            if (FLastFeesDataRetrieval == DateTime.MinValue || FLastFeesDataRetrieval < DateTime.UtcNow.AddDays(-1))
            {
                using (var lClient = new BitfinexClient())
                {
                    var lResponse = lClient.GetWithdrawalFeesAsync().Result;
                    if (!lResponse.Success)
                        throw new Exception($"Failed to get withdrawn fees from exchange server. {lResponse.Error}");
                    var lFees = lResponse.Data.Withdraw;
                    FWithdrawnFeesCache.Clear();
                    foreach (var lFee in lFees)
                        FWithdrawnFeesCache.TryAdd(lFee.Key, lFee.Value);
                }
                FLastFeesDataRetrieval = DateTime.UtcNow;
            }

            if (FWithdrawnFeesCache.TryGetValue(aCurrency.Ticker, out decimal lTxFee))
                lResult = lTxFee;
            else
            {
                using (var lClient = new BitfinexClient())
                {
                    var lResponse = lClient.GetCurrenciesAsync().Result;
                    if (lResponse.Success)
                    {
                        var lExchangeTicker = lResponse.Data.Where(lCurrency => string.Equals(lCurrency.FullName, aCurrency.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.Name;
                        if (lExchangeTicker != null && FWithdrawnFeesCache.TryGetValue(lExchangeTicker, out lTxFee))
                            lResult = lTxFee;
                    }
                }
            }

            return lResult ?? throw new Exception($"Transaction fee for currency with ticker '{aCurrency.Ticker}' not found");
        }

        public bool PlaceOrder(UserTradeOrder aOrder, bool aUseProxy = true)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");

            if (aOrder == null || aOrder.Market == null)
                throw new ArgumentNullException(aOrder == null ? nameof(aOrder) : nameof(aOrder.Market), "Invalid argument: " + aOrder == null ? nameof(aOrder) : nameof(aOrder.Market));

            if (aOrder.Status != OrderStatus.Waiting)
                return false;

            BitfinexClientOptions lBitfinexClientOptions = new BitfinexClientOptions()
            {
                Proxy = PandoraProxy.GetApiProxy(),
                ApiCredentials = new ApiCredentials(FUserCredentials.Item1, FUserCredentials.Item2)
            };
            using (BitfinexClient lClient = aUseProxy ? new BitfinexClient(lBitfinexClientOptions) : new BitfinexClient())
            {
                if (!aOrder.Market.TryCastToLocalMarket(out ExchangeMarket lExchangeMarket))
                    throw new ArgumentException(nameof(aOrder.Market), "Invalid Market");
                var lResponse = lClient.PlaceOrderAsync(lExchangeMarket.MarketPairID, lExchangeMarket.MarketDirection == MarketDirection.Sell ? OrderSide.Sell : OrderSide.Buy, OrderType.ExchangeLimit, lExchangeMarket.MarketDirection == MarketDirection.Sell ? aOrder.SentQuantity : aOrder.SentQuantity / aOrder.Rate, aOrder.Rate).Result;
                if (!lResponse.Success)
                    throw new Exception($"Bitfinex Error. Message: {lResponse.Error.Message}");
                long lUuid = lResponse.Data.Id;

                var lVerifyResponse = lClient.GetOrderAsync(lUuid).Result;
                if (!lResponse.Success)
                    throw new Exception("Failed to verify order with exchange server");
                aOrder.ID = lUuid.ToString();
                aOrder.OpenTime = lVerifyResponse.Data.Timestamp;
                aOrder.Cancelled = lVerifyResponse.Data.Canceled;
                aOrder.Completed = lVerifyResponse.Data.RemainingAmount == 0;
            }

            return true;
        }

        public bool RefundOrder(UserTradeOrder aOrder, string aAddress, bool aUseProxy = true)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");

            if (aOrder.Status == OrderStatus.Withdrawn)
                return false;

            BitfinexClientOptions lBittrexClientOptions = new BitfinexClientOptions()
            {
                Proxy = PandoraProxy.GetApiProxy(),
                ApiCredentials = new ApiCredentials(FUserCredentials.Item1, FUserCredentials.Item2)
            };
            using (BitfinexClient lClient = aUseProxy ? new BitfinexClient(lBittrexClientOptions) : new BitfinexClient())
            {
                var lResponse = lClient.WithdrawAsync(aOrder.Market.SellingCurrencyInfo.Name.ToLowerInvariant(), WithdrawWallet.Exchange, aOrder.SentQuantity, aAddress).Result;
                if (!lResponse.Success)
                    throw new Exception("Failed to withdraw. Error message:" + lResponse.Error.Message);
            }

            return true;
        }

        public void SetCredentials(string aApiKey, string aApiSecret)
        {
            var lCredentials = new ApiCredentials(aApiKey, aApiSecret);

            var lClientOptions = new BitfinexClientOptions { ApiCredentials = lCredentials };
            BitfinexClient.SetDefaultOptions(lClientOptions);
            using (BitfinexClient lClient = new BitfinexClient())
            {
                var lResponse = lClient.GetBalancesAsync().Result;
                if (!lResponse.Success)
                    throw new PandoraExchangeExceptions.InvalidExchangeCredentials("Incorrect Key Pair for selected exchange");
            }
            FUserCredentials = new Tuple<string, string>(aApiKey, aApiSecret);
            IsCredentialsSet = true;
        }

        public void StartMarketPriceUpdating()
        {
            if (FPriceUpdaterTask != null)
                return;
            FPriceUpdaterTask = new Timer(DoUpdateMarketPrices, null, 0, PRICE_REFRESH_PERIOD);
        }

        private void DoUpdateMarketPrices(object aState)
        {
            try
            {
                var lChanged = new ConcurrentBag<string>();
                using (var lClient = new BitfinexClient())

                {
                    var lMarketsData = lClient.GetTickerAsync(symbols: "ALL").Result;
                    if (!lMarketsData.Success)
                        throw new Exception("Unable to get updated market prices info");
                    Parallel.ForEach(lMarketsData.Data, (lMarketData) =>
                     {
                         string lMarketPairID = lMarketData.Symbol.Remove(0, 1).ToLowerInvariant();
                         MarketPriceInfo lRemoteMarketPrice = new MarketPriceInfo
                         {
                             Last = lMarketData.LastPrice,
                             Bid = lMarketData.Bid,
                             Ask = lMarketData.Ask
                         };

                         if ((FMarketPrices.TryGetValue(lMarketPairID, out MarketPriceInfo lPrice) && lPrice != lRemoteMarketPrice) || lPrice == null)
                         {
                             FMarketPrices.AddOrUpdate(lMarketPairID, lRemoteMarketPrice, (key, oldValue) => lRemoteMarketPrice);
                             lChanged.Add(lMarketPairID);
                         }
                     });
                }

                if (lChanged.Any())
                {
                    var lUpdatedMarkets = from lMarket in FLocalCacheOfMarkets.Values.SelectMany(lMarket => lMarket)
                                          join lUpdated in lChanged on lMarket.MarketPairID equals lUpdated
                                          select lMarket;
                    OnMarketPricesChanging?.Invoke(lUpdatedMarkets);
                }
            }
            catch (Exception ex)
            {
                Universal.Log.Write(LogLevel.Error, $"Exception thrown in market price update process. Details {ex}");
            }
        }

        public void StopMarketUpdating()
        {
            if (FPriceUpdaterTask == null)
                return;
            FPriceUpdaterTask?.Change(Timeout.Infinite, Timeout.Infinite);
            FPriceUpdaterTask?.Dispose();
            FPriceUpdaterTask = null;
        }

        public bool WithdrawOrder(UserTradeOrder aOrder, string aAddress, decimal aTxFee, bool aUseProxy = true)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");

            if (aOrder.Status == OrderStatus.Withdrawn)
                return false;

            BitfinexClientOptions lBittrexClientOptions = new BitfinexClientOptions()
            {
                Proxy = PandoraProxy.GetApiProxy(),
                ApiCredentials = new ApiCredentials(FUserCredentials.Item1, FUserCredentials.Item2)
            };
            using (BitfinexClient lClient = aUseProxy ? new BitfinexClient(lBittrexClientOptions) : new BitfinexClient())
            {
                decimal lQuantityToWithdraw = 0;
                var lGetOrderResponse = lClient.GetOrderAsync(Convert.ToInt64(aOrder.ID)).Result;
                if (!lGetOrderResponse.Success)
                    throw new Exception("Failed to get order to withdraw. Error message:" + lGetOrderResponse.Error.Message);
                var lRemoteOrder = lGetOrderResponse.Data;
                if (lRemoteOrder.AverageExecutionPrice <= 0)
                    throw new Exception("Order is not fulfilled or data from server is missing");
                if (lRemoteOrder.Side == OrderSide.Buy)
                    lQuantityToWithdraw = lRemoteOrder.ExecutedAmount;
                else if (lRemoteOrder.Side == OrderSide.Sell)
                    lQuantityToWithdraw = lRemoteOrder.ExecutedAmount * lRemoteOrder.AverageExecutionPrice;
                var lQuantityAfterFee = lQuantityToWithdraw - (lQuantityToWithdraw * (PERCENT_TRADE_FEE / 100));
                var lWithdrawResponse = lClient.WithdrawAsync(aOrder.Market.BuyingCurrencyInfo.Name.ToLowerInvariant(), WithdrawWallet.Exchange, lQuantityAfterFee, aAddress).Result;
                if (!lWithdrawResponse.Success)
                    throw new Exception("Failed to withdraw. Error message:" + lWithdrawResponse.Error.Message);
            }
            return true;
        }

        public IEnumerable<CandlestickPoint> GetCandleStickChart(IExchangeMarket aMarket, DateTime aStartTime, DateTime aEndTime, ChartInterval aChartInterval)
        {
            IEnumerable<CandlestickPoint> lResult = null;
            using (BitfinexClient lClient = new BitfinexClient())
            {
                TimeFrame lInterval;
                switch (aChartInterval)
                {
                    case ChartInterval.FiveMinutes:
                        lInterval = TimeFrame.FiveMinute;
                        break;

                    case ChartInterval.Hourly:
                        lInterval = TimeFrame.OneHour;
                        break;

                    case ChartInterval.Daily:
                        lInterval = TimeFrame.OneDay;
                        break;

                    default:
                        lInterval = TimeFrame.ThirtyMinute;
                        break;
                }
                if (!aMarket.TryCastToLocalMarket(out ExchangeMarket lExchangeMarket))
                    throw new ArgumentException(nameof(aMarket), "Invalid Market");
                var lMarket = FMarkets.FirstOrDefault(lBitFinexMarket => lBitFinexMarket.MarketDetails.Pair == lExchangeMarket.MarketPairID);
                if (lMarket != null)
                {
                    var lResponse = lClient.GetKlinesAsync(lInterval, lMarket.ExchangeSymbol, startTime: aStartTime, endTime: aEndTime).Result;
                    if (!lResponse.Success)
                        throw new Exception("Failed to get chart data. Error message:" + lResponse.Error.Message);
                    lResult = lResponse.Data.Select(lPoint => new CandlestickPoint
                    {
                        High = lPoint.High,
                        Low = lPoint.Low,
                        Open = lPoint.Open,
                        Close = lPoint.Close,
                        TimeStamp = lPoint.Timestamp
                    });
                }
            }
            return lResult;
        }
    }
}