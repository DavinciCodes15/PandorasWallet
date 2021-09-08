using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using Pandora.Client.ClientLib.Contracts;
using Pandora.Client.Exchange.Contracts;
using Pandora.Client.Exchange.CustomAPI.Poloniex.Net;
using Pandora.Client.Exchange.CustomAPI.Poloniex.Net.Objects;
using Pandora.Client.Exchange.Exchangers.Contracts;
using Pandora.Client.Exchange.Models;
using Pandora.Client.Universal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.Exchangers.Providers
{
    internal class PoloniexExchange : AbstractExchange, IPandoraExchanger
    {
        private const int PRICE_REFRESH_PERIOD = 10000;
        public new const AvailableExchangesList Identifier = AvailableExchangesList.Poloniex;
        private Tuple<string, string> FUserCredentials;
        private ConcurrentDictionary<string, PoloniexCurrencyPairSummary> FCacheMarkets;
        private ConcurrentDictionary<string, PoloniexCurrency> FCacheCurrencies;
        private ConcurrentDictionary<long, IEnumerable<PoloniexExchangeMarket>> FLocalCacheOfMarkets;
        private DateTime FLastMarketCoinsRetrieval;
        private static ConcurrentDictionary<string, MarketPriceInfo> FMarketPrices;
        private static Timer FPriceUpdaterTask;

        public bool IsCredentialsSet { get; private set; }
        public override string Name => Identifier.ToString();
        public override int ID => (int) Identifier;

        public event Action<IEnumerable<IExchangeMarket>> OnMarketPricesChanging;

        ~PoloniexExchange()
        {
            StopMarketUpdating();
            FCacheMarkets?.Clear();
            FCacheMarkets = null;
            FCacheCurrencies?.Clear();
            FCacheCurrencies = null;
            FUserCredentials = null;
            FLocalCacheOfMarkets?.Clear();
            FLocalCacheOfMarkets = null;
        }

        internal PoloniexExchange()
        {
            FCacheMarkets = new ConcurrentDictionary<string, PoloniexCurrencyPairSummary>();
            FCacheCurrencies = new ConcurrentDictionary<string, PoloniexCurrency>();
            FMarketPrices = new ConcurrentDictionary<string, MarketPriceInfo>();
            FLocalCacheOfMarkets = new ConcurrentDictionary<long, IEnumerable<PoloniexExchangeMarket>>();
            DoUpdateMarketCoins();
        }

        public void CancelOrder(UserTradeOrder aOrder, bool aUseProxy = true)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");

            if (aOrder == null)
                throw new ArgumentNullException(nameof(aOrder), "Invalid argument: " + nameof(aOrder));

            PoloniexClientOptions lPoloniexClientOptions = new PoloniexClientOptions()
            {
                Proxy = PandoraProxy.GetApiProxy(),
                ApiCredentials = new ApiCredentials(FUserCredentials.Item1, FUserCredentials.Item2)
            };

            using (PoloniexClient lClient = aUseProxy ? new PoloniexClient(lPoloniexClientOptions) : new PoloniexClient())
            {
                var lResponse = lClient.CancelOrder(Convert.ToInt64(aOrder.ID));
                if (!lResponse.Success || !Convert.ToBoolean(lResponse.Data.success))
                    throw new Exception($"Failed to cancel order in exchange. Message: {lResponse.Data?.message ?? lResponse.Error.Message}");
            }
        }

        public void Clear()
        {
            var lClientOptions = new PoloniexClientOptions { ApiCredentials = null };
            PoloniexClient.SetDefaultOptions(lClientOptions);
            IsCredentialsSet = false;
        }

        public decimal GetBalance(IExchangeMarket aMarket)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");

            using (PoloniexClient lClient = new PoloniexClient())
            {
                var lResponse = lClient.GetBalances();
                if (!lResponse.Success || !lResponse.Data.TryGetValue(aMarket.SellingCurrencyInfo.Ticker, out decimal lBalance))
                    throw new Exception("Failed to retrieve balance");
                return lBalance;
            }
        }

        public int GetConfirmations(ICurrencyIdentity aCurrency)
        {
            return GetPoloniexCurrency(aCurrency.Ticker).minConf;
        }

        public string GetDepositAddress(IExchangeMarket aMarket)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");
            string lResult;
            string lTicker = aMarket.SellingCurrencyInfo.Ticker;
            using (PoloniexClient lClient = new PoloniexClient())
            {
                var lResponse = lClient.GetDepositAddresses();
                if (!lResponse.Success)
                    throw new Exception("Failed to retrieve Poloniex Address. " + lResponse.Error.Message);
                if (lResponse.Data.TryGetValue(lTicker, out string lAddress))
                    lResult = lAddress;
                else
                {
                    var lNewAddressResponse = lClient.GenerateNewAddress(lTicker);
                    if (!lNewAddressResponse.Success || !Convert.ToBoolean(lNewAddressResponse.Data.success))
                        throw new Exception("Failed to retrieve Poloniex Address. " + lResponse.Error.Message);
                    lResult = lNewAddressResponse.Data.response;
                }
            }
            return lResult;
        }

        private async Task DoUpdateMarketCoins()
        {
            if (FLastMarketCoinsRetrieval == DateTime.MinValue || FLastMarketCoinsRetrieval < DateTime.UtcNow.AddHours(-1))
                using (PoloniexClient lClient = new PoloniexClient())
                {
                    var lResponse = await lClient.GetTickerMarketsAsync();
                    if (!lResponse.Success)
                        throw new Exception("Failed to retrieve Markets");
                    FCacheMarkets.Clear();
                    foreach (var lPair in lResponse.Data)
                        FCacheMarkets.TryAdd(lPair.Key, lPair.Value);
                    FLastMarketCoinsRetrieval = DateTime.UtcNow;
                }
            FLocalCacheOfMarkets.Clear();
        }

        public IEnumerable<IExchangeMarket> GetMarketCoins(ICurrencyIdentity aCurrency, GetWalletIDDelegate aGetWalletIDFunction)
        {
            DoUpdateMarketCoins().Wait();
            if (!FLocalCacheOfMarkets.TryGetValue(aCurrency.Id, out IEnumerable<PoloniexExchangeMarket> lCoinMarkets))
            {
                lCoinMarkets = FCacheMarkets.Where(lPair => lPair.Key.EndsWith($"_{aCurrency.Ticker}") || lPair.Key.StartsWith($"{aCurrency.Ticker}_"))
                .Select(lPair => new PoloniexExchangeMarket(this, aCurrency, aGetWalletIDFunction, lPair.Key));
                if (aGetWalletIDFunction != null)
                    FLocalCacheOfMarkets.TryAdd(aCurrency.Id, lCoinMarkets);
            }

            return lCoinMarkets;
        }

        private class PoloniexExchangeMarket : ExchangeMarket
        {
            public PoloniexExchangeMarket(PoloniexExchange aExchangeInstance, ICurrencyIdentity aCurrency, GetWalletIDDelegate aGetWalletIDFunction, string aPairName) : base(aExchangeInstance)
            {
                MarketPairID = string.IsNullOrEmpty(aPairName) ? throw new ArgumentException("Poloniex pair name can not be null or empty") : aPairName;
                var lPoloniexBuyingTicker = GetPairTicker(aPairName, aCurrency.Ticker, out bool lIsBase);
                var lPoloniexBuyingName = aExchangeInstance.GetPoloniexCurrency(lPoloniexBuyingTicker)?.name ?? throw new Exception("market currency pair info not found");
                SellingCurrencyInfo = aCurrency;
                BuyingCurrencyInfo = new CurrencyInfo(aGetWalletIDFunction)
                {
                    Ticker = lPoloniexBuyingTicker,
                    Name = lPoloniexBuyingName,
                    Id = aGetWalletIDFunction?.Invoke(lPoloniexBuyingTicker) ?? 0
                };
                MarketBaseCurrencyInfo = lIsBase ? BuyingCurrencyInfo : SellingCurrencyInfo;
                MinimumTrade = 0.0001M; //This info I found it at the web
            }

            private string GetPairTicker(string aPairName, string aSourceCurrencyTicker, out bool aIsBase)
            {
                var lSplitedPair = aPairName.Split('_');
                string lResult = null;
                if (lSplitedPair.Count() > 1)
                    lResult = lSplitedPair.FirstOrDefault(lTicker => !string.Equals(lTicker, aSourceCurrencyTicker, StringComparison.OrdinalIgnoreCase)) ?? throw new Exception("Invalid market pair detected");
                aIsBase = lResult == lSplitedPair.FirstOrDefault();
                return lResult;
            }
        }

        private PoloniexCurrency GetPoloniexCurrency(string aTicker)
        {
            PoloniexCurrency lCurrency = null;
            if (!string.IsNullOrEmpty(aTicker) && !FCacheCurrencies.TryGetValue(aTicker, out lCurrency))
            {
                using (PoloniexClient lClient = new PoloniexClient())
                {
                    var lResponse = lClient.GetCurrencies();
                    if (!lResponse.Success)
                        throw new Exception($"Unable to retrieve Poloniex currency {aTicker}");
                    foreach (var lPoloniexCurrency in lResponse.Data)
                    {
                        FCacheCurrencies.AddOrUpdate(lPoloniexCurrency.Key, lPoloniexCurrency.Value, (x, y) => lPoloniexCurrency.Value);
                        if (lPoloniexCurrency.Key == aTicker)
                            lCurrency = lPoloniexCurrency.Value;
                    }
                }
            }
            return lCurrency;
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

        public TradeOrderStatusInfo GetOrderStatus(string aUuid)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");
            var lOrderNumber = Convert.ToInt64(aUuid);
            var lResult = new TradeOrderStatusInfo() { ID = aUuid };
            using (PoloniexClient lClient = new PoloniexClient())
            {
                var lResponse = lClient.GetOrderStatus(lOrderNumber);
                if (!lResponse.Success)
                    throw new Exception("Unable to retrieve order info from Poloniex");
                if (Convert.ToBoolean(lResponse.Data.success) && string.IsNullOrEmpty(lResponse.Data.error))
                {
                    if (!lResponse.Data.result.TryGetValue(aUuid, out PoloniexOrderStatus lOrderSummary))
                        throw new Exception("Order info not found");
                    lResult.Rate = lOrderSummary.rate;
                    lResult.Completed = false;
                    lResult.Cancelled = false;
                }
                else
                {
                    var lOrderTradesResponse = lClient.GetOrderTrades(lOrderNumber);
                    if (!lOrderTradesResponse.Success)
                        throw new Exception("Unable to retrieve order info from Poloniex");
                    lResult.Cancelled = !string.IsNullOrEmpty(lOrderTradesResponse.Data.error);
                    if (lResult.Completed = !lResult.Cancelled)
                    {
                        lResult.Rate = lOrderTradesResponse.Data.Average(lOrder => lOrder.rate);
                    }
                }
            }
            return lResult;
        }

        public decimal GetTransactionsFee(ICurrencyIdentity aCurrency)
        {
            return GetPoloniexCurrency(aCurrency.Ticker).txFee;
        }

        public bool PlaceOrder(UserTradeOrder aOrder, bool aUseProxy = true)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");

            if (aOrder == null)
                throw new ArgumentNullException(nameof(aOrder), $"Invalid order with id {aOrder}");

            if (aOrder.Status != OrderStatus.Waiting)
                return false;

            using (var lClient = new PoloniexClient())
            {
                WebCallResult<PoloniexOrderPlaceResult> lResponse = null;
                if (!aOrder.Market.TryCastToLocalMarket(out ExchangeMarket lExchangeMarket))
                    throw new ArgumentException(nameof(aOrder.Market), "Invalid Market");
                if (lExchangeMarket.MarketDirection == MarketDirection.Sell)
                    lResponse = lClient.PlaceSellOrder(lExchangeMarket.MarketPairID, aOrder.Rate, aOrder.SentQuantity);
                else if (lExchangeMarket.MarketDirection == MarketDirection.Buy)
                    lResponse = lClient.PlaceBuyOrder(lExchangeMarket.MarketPairID, aOrder.Rate, aOrder.SentQuantity / aOrder.Rate);
                if (lResponse == null || !lResponse.Success)
                    throw new Exception($"Unable to place trade order at exchange. Error: {lResponse?.Error.Message ?? "Failed to get response from server" }");

                aOrder.ID = lResponse.Data.orderNumber.ToString();
                aOrder.OpenTime = DateTime.UtcNow;
            }
            return true;
        }

        public bool RefundOrder(UserTradeOrder aOrder, string aAddress, bool aUseProxy = true)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");

            if (aOrder.Status == OrderStatus.Withdrawn)
                return false;

            PoloniexClientOptions lPoloniexClientOptions = new PoloniexClientOptions()
            {
                Proxy = PandoraProxy.GetApiProxy(),
                ApiCredentials = new ApiCredentials(FUserCredentials.Item1, FUserCredentials.Item2)
            };
            using (PoloniexClient lClient = aUseProxy ? new PoloniexClient(lPoloniexClientOptions) : new PoloniexClient())
            {
                var lResponse = lClient.Withdraw(aOrder.Market.SellingCurrencyInfo.Ticker, aOrder.SentQuantity, aAddress);
                if (!lResponse.Success)
                    throw new Exception("Failed to refund order. Error message:" + lResponse.Error.Message);
                if (!string.IsNullOrEmpty(lResponse.Data.error))
                    throw new Exception($"Failed to refund order. Error message: {lResponse.Data.error}");
            }

            return true;
        }

        public void SetCredentials(string aApiKey, string aApiSecret)
        {
            var lKeyGuid = aApiKey.Replace("-", string.Empty);
            var lFormatedGUID = Enumerable.Range(0, lKeyGuid.Count() / 8)
                                          .Select(lCounter => lKeyGuid.Substring(lCounter * 8, 8))
                                          .Aggregate((first, second) => string.Concat(first, "-", second)).ToUpperInvariant();
            var lCredentials = new ApiCredentials(lFormatedGUID, aApiSecret.ToLowerInvariant());
            PoloniexClientOptions lClientOptions = new PoloniexClientOptions { ApiCredentials = lCredentials };
            PoloniexClient.SetDefaultOptions(lClientOptions);
            using (PoloniexClient lClient = new PoloniexClient())
            {
                var lResponse = lClient.GetBalances();
                if (!lResponse.Success)
                    throw new PandoraExchangeExceptions.InvalidExchangeCredentials("Incorrect Key Pair for selected exchange");
            }
            FUserCredentials = new Tuple<string, string>(lFormatedGUID, aApiSecret.ToLowerInvariant());
            IsCredentialsSet = true;
        }

        public void StartMarketPriceUpdating()
        {
            if (FPriceUpdaterTask != null)
                return;
            FPriceUpdaterTask = new Timer(DoUpdateMarketPrices, null, 0, PRICE_REFRESH_PERIOD);
        }

        public void StopMarketUpdating()
        {
            if (FPriceUpdaterTask == null)
                return;
            FPriceUpdaterTask?.Change(Timeout.Infinite, Timeout.Infinite);
            FPriceUpdaterTask?.Dispose();
            FPriceUpdaterTask = null;
        }

        private void DoUpdateMarketPrices(object aState)
        {
            try
            {
                var lChanged = new ConcurrentBag<string>();
                using (var lClient = new PoloniexClient())
                {
                    var lMarketsData = lClient.GetTickerMarkets();
                    if (!lMarketsData.Success)
                        throw new Exception("Unable to get updated market prices info");
                    Parallel.ForEach(lMarketsData.Data, (lMarketData) =>
                    {
                        string lMarketPairID = lMarketData.Key;
                        MarketPriceInfo lRemoteMarketPrice = new MarketPriceInfo
                        {
                            Last = lMarketData.Value.last,
                            Bid = lMarketData.Value.highestBid,
                            Ask = lMarketData.Value.lowestAsk
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

        public bool WithdrawOrder(UserTradeOrder aOrder, string aAddress, decimal aTxFee, bool aUseProxy = true)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");

            if (aOrder.Status == OrderStatus.Withdrawn)
                return false;

            var lPoloniexClientOptions = new PoloniexClientOptions()
            {
                Proxy = PandoraProxy.GetApiProxy(),
                ApiCredentials = new ApiCredentials(FUserCredentials.Item1, FUserCredentials.Item2)
            };
            using (var lClient = aUseProxy ? new PoloniexClient(lPoloniexClientOptions) : new PoloniexClient())
            {
                decimal lQuantityToWithdraw = 0;
                var lOrderID = Convert.ToInt64(aOrder.ID);
                var lResult = lClient.GetOrderTrades(lOrderID);
                if (!lResult.Success)
                    throw new Exception("Failed to get order to withdraw. Error message:" + lResult.Error.Message);
                if (!string.IsNullOrEmpty(lResult.Data.error))
                    throw new Exception($"Failed to get order to withdraw. Error message: {lResult.Error.Message}");
                foreach (var lTrade in lResult.Data)
                {
                    if (aOrder.Market.MarketDirection == MarketDirection.Sell)
                        lQuantityToWithdraw += lTrade.total - (lTrade.total * lTrade.fee);
                    else
                        lQuantityToWithdraw += lTrade.amount - (lTrade.amount * lTrade.fee);
                }
                var lResponse = lClient.Withdraw(aOrder.Market.BuyingCurrencyInfo.Ticker, lQuantityToWithdraw, aAddress);
                if (!lResponse.Success)
                    throw new Exception($"Failed to withdraw order. Error message: {lResult.Error.Message}");
                if (!string.IsNullOrEmpty(lResponse.Data.error))
                    throw new Exception($"Failed to withdraw order. Error message: {lResponse.Data.error}");
            }
            return true;
        }

        public IEnumerable<CandlestickPoint> GetCandleStickChart(IExchangeMarket aMarket, DateTime aStartTime, DateTime aEndTime, ChartInterval aChartInterval)
        {
            IEnumerable<CandlestickPoint> lResult = null;
            using (var lClient = new PoloniexClient())
            {
                var lUnixStartTime = aStartTime.ToUnixTimestamp();
                var lUnixEndTime = aEndTime.ToUnixTimestamp();
                long lPeriod;
                switch (aChartInterval)
                {
                    case ChartInterval.Daily:
                        lPeriod = 86400;
                        break;

                    case ChartInterval.FiveMinutes:
                        lPeriod = 300;
                        break;

                    case ChartInterval.Hourly:
                        lPeriod = 1800;
                        break;

                    default:
                        lPeriod = 1800;
                        break;
                }
                if (!aMarket.TryCastToLocalMarket(out ExchangeMarket lExchangeMarket))
                    throw new ArgumentException(nameof(aMarket), "Invalid Market");
                var lResponse = lClient.GetChartData(lExchangeMarket.MarketPairID, lUnixStartTime, lUnixEndTime, lPeriod);
                if (!lResponse.Success)
                    throw new Exception($"Failed to get chart data. Error message: {lResponse.Error.Message}");
                lResult = lResponse.Data.Select(lPoint => new CandlestickPoint
                {
                    Open = lPoint.open,
                    Close = lPoint.close,
                    High = lPoint.high,
                    Low = lPoint.low,
                    TimeStamp = DateTimeOffset.FromUnixTimeSeconds(lPoint.date).UtcDateTime
                });
            }
            return lResult;
        }
    }
}