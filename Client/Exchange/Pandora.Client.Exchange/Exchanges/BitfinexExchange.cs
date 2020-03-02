using Bitfinex.Net;
using Bitfinex.Net.Objects;
using Bitfinex.Net.Objects.RestV1Objects;
using Pandora.Client.Exchange.Objects;
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
        private ExchangeBitfinexMarket[] FMarkets;
        private ConcurrentDictionary<string, decimal> FWithdrawnFeesCache;
        private ConcurrentDictionary<string, ExchangeMarket[]> FLocalCacheOfMarkets;

        public override string Name => Identifier.ToString();

        public override int ID => (int) Identifier;

        private static event Action<IEnumerable<string>, int> OnMarketPricesChangingInternal;
        public event Action<IEnumerable<string>, int> OnMarketPricesChanging;

        private Tuple<string, string> FUserCredentials;
        private static Timer FPriceUpdaterTask;
        private static ConcurrentDictionary<string,MarketPriceInfo> FMarketPrices;

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
            FLocalCacheOfMarkets = new ConcurrentDictionary<string, ExchangeMarket[]>();
            OnMarketPricesChangingInternal += BitfinexExchange_OnMarketPricesChangingInternal;
            DoUpdatemarketCoins();
        }

        private void BitfinexExchange_OnMarketPricesChangingInternal(IEnumerable<string> arg1, int arg2)
        {
            OnMarketPricesChanging?.Invoke(arg1, arg2);
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
                ApiCredentials = new JKrof.Authentication.ApiCredentials(FUserCredentials.Item1, FUserCredentials.Item2)
            };
            using (BitfinexClient lClient = aUseProxy ? new BitfinexClient(lBittrexClientOptions) : new BitfinexClient())
            {
                var lResponse = lClient.CancelOrder(Convert.ToInt64(aOrder.ID));
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

        public decimal GetBalance(ExchangeMarket aMarket)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");

            using (var lClient = new BitfinexClient())
            {
                var lResponse = lClient.GetBalances();
                if (!lResponse.Success)
                    throw new Exception($"Failed to retrieve balance. Details: {lResponse.Error}");
                var lBalances = lResponse.Data;
                var lDecimalBalance = lBalances.Where(lBalance => lBalance.Currency == aMarket.BaseCurrencyInfo.Ticker).FirstOrDefault();
                if (lDecimalBalance == null)
                    throw new Exception($"Failed to get balance for currency {aMarket.BaseCurrencyInfo.Name}");
                return lDecimalBalance.Balance;
            }
        }

        public int GetConfirmations(string aCurrencyName, string aTicker)
        {
            if (!FConfirmations.TryGetValue(aTicker, out int lConfirmations))
                lConfirmations = 6;                
            return lConfirmations;
        }

        public string GetDepositAddress(ExchangeMarket aMarket)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");

            using(var lClient = new BitfinexClient())
            {
                var lAddressResponse = lClient.GetDepositAddress(aMarket.BaseCurrencyInfo.Name.ToLowerInvariant(), WithdrawWallet.Exchange);
                if (!lAddressResponse.Success)
                    throw new Exception($"Failed to get deposit address for bitfinex. Details: {lAddressResponse.Error}");
                return lAddressResponse.Data.Address;
            }
        }

        private class ExchangeBitfinexMarket
        {            
            public enum MarketType { Trade, Founding }
            public static IEnumerable<BitfinexCurrency> ExchangeCurrencies { get; set; }
            public BitfinexSymbolDetails MarketDetails { get; private set; }
            public BitfinexCurrency BaseCurrency { get; private set; }
            public BitfinexCurrency DestinationCurrency { get; private set; }
            public MarketType SymbolType { get; private set; }
            public string ExchangeSymbol { get; private set; }

            public ExchangeBitfinexMarket(string aSymbol, BitfinexSymbolDetails aSymbolDetails)
            {
                if (ExchangeCurrencies == null)
                    throw new Exception("First you need to set a list of currencies from exchange");
                ExchangeSymbol = aSymbol;
                MarketDetails = aSymbolDetails;
                DeconstructSymbol();
            }

            private void DeconstructSymbol()
            {
                SymbolType = ExchangeSymbol.First() == 't' ? MarketType.Trade : MarketType.Founding;
                var lSymbolWithoutType = ExchangeSymbol.Remove(0, 1);
                BaseCurrency = ExchangeCurrencies.Where(lCurrency => lSymbolWithoutType.IndexOf(lCurrency.Name) == 0).FirstOrDefault();
                if (BaseCurrency != null)
                {
                    var lSymbolWithoutBase = lSymbolWithoutType.Replace(BaseCurrency.Name, string.Empty);
                    DestinationCurrency = ExchangeCurrencies.Where(lCurrency => lSymbolWithoutBase == lCurrency.Name).FirstOrDefault();
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
                    ExchangeBitfinexMarket.ExchangeCurrencies = lCurrenciesResponse.Data;
                    FMarkets = lMarketsResponse.Data.Select(lMarket => new ExchangeBitfinexMarket(lMarket.Symbol, lSymbolDetails.Data.Where(lSymbol => string.Equals(lSymbol.Pair, lMarket.Symbol.Remove(0,1), StringComparison.OrdinalIgnoreCase)).FirstOrDefault())).ToArray();
                }
                FLastMarketCoinsRetrieval = DateTime.UtcNow;
                FLocalCacheOfMarkets.Clear();
            }
        }

        public ExchangeMarket[] GetMarketCoins(string aCurrencyName, string aTicker, GetWalletIDDelegate aGetWalletIDFunction = null)
        {
            DoUpdatemarketCoins().Wait();
            if (!FLocalCacheOfMarkets.TryGetValue(aTicker, out ExchangeMarket[] lCoinMarkets))
            {
                var lMarketsWithCompleteData = FMarkets.Where(lMarket => lMarket?.BaseCurrency?.FullName != null && lMarket?.DestinationCurrency?.FullName != null);
                bool lIsBaseCurrency;
                lCoinMarkets = lMarketsWithCompleteData.Where(lMarket => string.Equals(lMarket.DestinationCurrency.FullName, aCurrencyName, StringComparison.OrdinalIgnoreCase) || string.Equals(lMarket.BaseCurrency.FullName, aCurrencyName, StringComparison.OrdinalIgnoreCase))
               .Select(lMarket => new ExchangeMarket(this)
               {
                   BaseCurrencyInfo = new ExchangeMarket.CurrencyInfo
                   {
                       Ticker = aTicker,
                       Name = (lIsBaseCurrency = string.Equals(lMarket.BaseCurrency.FullName, aCurrencyName, StringComparison.OrdinalIgnoreCase) || string.Equals(lMarket.BaseCurrency.Name, aTicker, StringComparison.OrdinalIgnoreCase)) ? lMarket.BaseCurrency.FullName : lMarket.DestinationCurrency.FullName,
                       WalletID = aGetWalletIDFunction?.Invoke(aCurrencyName, aTicker)
                   },
                   DestinationCurrencyInfo = new ExchangeMarket.CurrencyInfo
                   {
                       Ticker = lIsBaseCurrency ? lMarket.DestinationCurrency.Name : lMarket.BaseCurrency.Name,
                       Name = lIsBaseCurrency ? lMarket.DestinationCurrency.FullName : lMarket.BaseCurrency.FullName,
                       WalletID = aGetWalletIDFunction?.Invoke(lIsBaseCurrency ? lMarket.DestinationCurrency.FullName : lMarket.BaseCurrency.FullName, lIsBaseCurrency? lMarket.DestinationCurrency.Name : lMarket.BaseCurrency.Name)
                   },
                   MarketName = lMarket.MarketDetails.Pair,
                   IsSell = lIsBaseCurrency,
                   MinimumTrade = lMarket.MarketDetails?.MinimumOrderSize ?? 0
               }).ToArray();

                if(aGetWalletIDFunction != null)
                    FLocalCacheOfMarkets.TryAdd(aTicker, lCoinMarkets);
            }

            return lCoinMarkets;
        }

        public MarketPriceInfo GetMarketPrice(string aMarketName)
        {
            MarketPriceInfo lResult = new MarketPriceInfo();
            if (FMarketPrices.TryGetValue(aMarketName, out MarketPriceInfo lValue))
                lResult = lValue;
            return lResult;
        }

        public TradeOrderStatusInfo GetOrderStatus(string lUuid)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");

            using (var lClient = new BitfinexClient())
            {
                var lResponse = lClient.GetOrder(Convert.ToInt64(lUuid));
                if (!lResponse.Success)
                    throw new Exception($"Failed to retrieve order status. Id: {lUuid}");
                return new TradeOrderStatusInfo
                {
                    ID = lUuid,
                    Completed = lResponse.Data.RemainingAmount == 0,
                    Cancelled = lResponse.Data.Canceled,
                    ExchangeMarketName = lResponse.Data.Symbol,
                    Rate = lResponse.Data.Price
                };
            }
        }

        public decimal GetTransactionsFee(string aCurrencyName, string aTicker)
        {
            decimal? lResult = null;
            if (FLastFeesDataRetrieval == DateTime.MinValue || FLastFeesDataRetrieval < DateTime.UtcNow.AddDays(-1))
            {
                    using (var lClient = new BitfinexClient())
                    {
                        var lResponse = lClient.GetWithdrawalFees();
                        if (!lResponse.Success)
                            throw new Exception($"Failed to get withdrawn fees from exchange server. {lResponse.Error}");
                        var lFees = lResponse.Data.Withdraw;
                        FWithdrawnFeesCache.Clear();
                        foreach (var lFee in lFees)
                            FWithdrawnFeesCache.TryAdd(lFee.Key, lFee.Value);
                    }
                FLastFeesDataRetrieval = DateTime.UtcNow;
            }

            if (FWithdrawnFeesCache.TryGetValue(aTicker, out decimal lTxFee))
                lResult = lTxFee;
            else
            {
                using (var lClient = new BitfinexClient())
                {
                    var lResponse = lClient.GetCurrencies();
                    if (lResponse.Success)
                    {
                        var lExchangeTicker = lResponse.Data.Where(lCurrency => string.Equals(lCurrency.FullName, aCurrencyName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.Name;
                        if (lExchangeTicker != null && FWithdrawnFeesCache.TryGetValue(lExchangeTicker, out lTxFee))
                            lResult = lTxFee;
                    }

                }
            }

            return lResult ?? throw new Exception($"Transaction fee for currency with ticker '{aTicker}' not found");

        }

        public bool PlaceOrder(UserTradeOrder aOrder, ExchangeMarket aMarket, bool aUseProxy = true)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");

            if (aOrder == null || aMarket == null)            
                throw new ArgumentNullException(aOrder == null ? nameof(aOrder) : nameof(aMarket), "Invalid argument: " + aOrder == null ? nameof(aOrder) : nameof(aMarket));

            if (aOrder.Status != OrderStatus.Waiting)
                return false;

            BitfinexClientOptions lBitfinexClientOptions = new BitfinexClientOptions()
            {
                Proxy = PandoraProxy.GetApiProxy(),
                ApiCredentials = new JKrof.Authentication.ApiCredentials(FUserCredentials.Item1, FUserCredentials.Item2)
            };
            using (BitfinexClient lClient = aUseProxy ? new BitfinexClient(lBitfinexClientOptions) : new BitfinexClient())
            {

                var lResponse = lClient.PlaceOrder(aMarket.MarketName, aMarket.IsSell ? OrderSide.Sell : OrderSide.Buy, OrderTypeV1.ExchangeLimit, aMarket.IsSell? aOrder.SentQuantity : aOrder.SentQuantity / aOrder.Rate, aOrder.Rate);
                if (!lResponse.Success)
                    throw new Exception($"Bitfinex Error. Message: {lResponse.Error.Message}");
                long lUuid = lResponse.Data.Id;

                var lVerifyResponse = lClient.GetOrder(lUuid);
                if(!lResponse.Success)
                    throw new Exception("Failed to verify order with exchange server");
                aOrder.ID = lUuid.ToString();
                aOrder.OpenTime = lVerifyResponse.Data.Timestamp;
                aOrder.Cancelled = lVerifyResponse.Data.Canceled;
                aOrder.Completed = lVerifyResponse.Data.RemainingAmount == 0;
            }

            return true;
        }

        public bool RefundOrder(ExchangeMarket aMarket, UserTradeOrder aOrder, string aAddress, bool aUseProxy = true)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");            

            if (aOrder.Status == OrderStatus.Withdrawn)
                return false;

            BitfinexClientOptions lBittrexClientOptions = new BitfinexClientOptions()
            {
                Proxy = PandoraProxy.GetApiProxy(),
                ApiCredentials = new JKrof.Authentication.ApiCredentials(FUserCredentials.Item1, FUserCredentials.Item2)
            };
            using (BitfinexClient lClient = aUseProxy ? new BitfinexClient(lBittrexClientOptions) : new BitfinexClient())
            {
                var lResponse = lClient.Withdraw(aMarket.BaseCurrencyInfo.Name.ToLowerInvariant(), WithdrawWallet.Exchange, aOrder.SentQuantity, aAddress);
                if (!lResponse.Success)
                    throw new Exception("Failed to withdraw. Error message:" + lResponse.Error.Message);                
            }

            return true;
        }

        public void SetCredentials(string aApiKey, string aApiSecret)
        {
            var lCredentials = new JKrof.Authentication.ApiCredentials(aApiKey, aApiSecret);

            var lClientOptions = new BitfinexClientOptions { ApiCredentials = lCredentials };
            BitfinexClient.SetDefaultOptions(lClientOptions);
            using(BitfinexClient lClient = new BitfinexClient())
            {
                var lResponse = lClient.GetBalances();
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
            FPriceUpdaterTask = new Timer(DoUpdateMarketPrices, null, 0, PRICE_REFRESH_PERIOD); //Every Minute
        }

        private static void DoUpdateMarketPrices(object aState)
        {
            try
            {
                List<string> lChanged = new List<string>();
                using (var lClient = new BitfinexClient())
                
                {
                    var lMarketsData = lClient.GetTicker(symbols: "ALL");
                    if (!lMarketsData.Success)
                        throw new Exception("Unable to get updated market prices info");
                    foreach (var lMarketData in lMarketsData.Data)
                    {
                        string lMarketName = lMarketData.Symbol.Remove(0,1).ToLowerInvariant();
                        MarketPriceInfo lRemoteMarketPrice = new MarketPriceInfo
                        {
                            Last = lMarketData.LastPrice,
                            Bid = lMarketData.Bid,
                            Ask = lMarketData.Ask
                        };

                        if (FMarketPrices.TryGetValue(lMarketName, out MarketPriceInfo lPrice))
                        {
                            if (lPrice != lRemoteMarketPrice)
                            {
                                FMarketPrices.AddOrUpdate(lMarketName, lRemoteMarketPrice, (key, oldValue) => lRemoteMarketPrice);
                                lChanged.Add(lMarketName);
                            }
                        }
                        else
                        {
                            FMarketPrices.TryAdd(lMarketName, lRemoteMarketPrice) ;
                            lChanged.Add(lMarketName);
                        }
                    }
                }
                if (lChanged.Any())
                    OnMarketPricesChangingInternal?.Invoke(lChanged, (int)Identifier);
            }
            catch (Exception ex)
            {
                Universal.Log.Write(LogLevel.Error, $"Exception thrown in market price update process. Details {ex}");
            }
        }

        public void StopMarketUpdating()
        {
            OnMarketPricesChangingInternal -= BitfinexExchange_OnMarketPricesChangingInternal;
            if (FPriceUpdaterTask == null || OnMarketPricesChangingInternal != null)
                return;
            FPriceUpdaterTask?.Change(Timeout.Infinite, Timeout.Infinite);
            FPriceUpdaterTask?.Dispose();
            FPriceUpdaterTask = null;
        }

        public bool WithdrawOrder(ExchangeMarket aMarket, UserTradeOrder aOrder, string aAddress, decimal aTxFee, bool aUseProxy = true)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");

            if (aOrder.Status == OrderStatus.Withdrawn)
                return false;

            BitfinexClientOptions lBittrexClientOptions = new BitfinexClientOptions()
            {
                Proxy = PandoraProxy.GetApiProxy(),
                ApiCredentials = new JKrof.Authentication.ApiCredentials(FUserCredentials.Item1, FUserCredentials.Item2)
            };
            using (BitfinexClient lClient = aUseProxy ? new BitfinexClient(lBittrexClientOptions) : new BitfinexClient())
            {
                decimal lQuantityToWithdraw = 0;
                var lGetOrderResponse = lClient.GetOrder(Convert.ToInt64(aOrder.ID));
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
                var lWithdrawResponse = lClient.Withdraw(aMarket.DestinationCurrencyInfo.Name.ToLowerInvariant(), WithdrawWallet.Exchange, lQuantityAfterFee, aAddress);
                if (!lWithdrawResponse.Success)
                    throw new Exception("Failed to withdraw. Error message:" + lWithdrawResponse.Error.Message);
            }
            return true;
        }
    }
}
