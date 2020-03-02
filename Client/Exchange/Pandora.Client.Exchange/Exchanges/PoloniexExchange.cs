using Pandora.Client.Exchange.JKrof.Logging;
using Pandora.Client.Exchange.JKrof.Poloniex.Net;
using Pandora.Client.Exchange.JKrof.Poloniex.Net.Objects;
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
    internal class PoloniexExchange : AbstractExchange, IPandoraExchanger
    {
        private const int PRICE_REFRESH_PERIOD = 10000;
        public new const AvailableExchangesList Identifier = AvailableExchangesList.Poloniex;
        private Tuple<string, string> FUserCredentials;
        private ConcurrentDictionary<string, PoloniexCurrencyPairSummary> FCacheMarkets;
        private ConcurrentDictionary<string, PoloniexCurrency> FCacheCurrencies;
        private ConcurrentDictionary<string, ExchangeMarket[]> FLocalCacheOfMarkets;
        private DateTime FLastMarketCoinsRetrieval;
        private static ConcurrentDictionary<string, MarketPriceInfo> FMarketPrices;
        private static Timer FPriceUpdaterTask;

        public bool IsCredentialsSet { get; private set; }
        public override string Name => Identifier.ToString();
        public override int ID => (int) Identifier;

        private static event Action<IEnumerable<string>, int> OnMarketPricesChangingInternal;
        public event Action<IEnumerable<string>, int> OnMarketPricesChanging;

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
            FLocalCacheOfMarkets = new ConcurrentDictionary<string, ExchangeMarket[]>();
            OnMarketPricesChangingInternal += PoloniexExchange_OnMarketPricesChangingInternal;
            DoUpdateMarketCoins();
        }

        private void PoloniexExchange_OnMarketPricesChangingInternal(IEnumerable<string> arg1, int arg2)
        {
            OnMarketPricesChanging?.Invoke(arg1, arg2);
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
                ApiCredentials = new JKrof.Authentication.ApiCredentials(FUserCredentials.Item1, FUserCredentials.Item2)
            };
            using(PoloniexClient lClient = aUseProxy? new PoloniexClient(lPoloniexClientOptions):new PoloniexClient())
            {
                var lResponse = lClient.CancelOrder(Convert.ToInt64(aOrder.ID));
                if(!lResponse.Success || Convert.ToBoolean(lResponse.Data.success))
                    throw new Exception($"Failed to cancel order in exchange. Message: {lResponse.Data?.message??lResponse.Error.Message}");
            }
        }

        public void Clear()
        {
            var lClientOptions = new PoloniexClientOptions { ApiCredentials = null };
            PoloniexClient.SetDefaultOptions(lClientOptions);
            IsCredentialsSet = false;
        }

        public decimal GetBalance(ExchangeMarket aMarket)
        {
            if (!IsCredentialsSet)            
                throw new Exception("No Credentials were set");

            using (PoloniexClient lClient = new PoloniexClient())
            {
                var lResponse = lClient.GetBalances();
                if (!lResponse.Success || !lResponse.Data.TryGetValue(aMarket.BaseCurrencyInfo.Ticker, out decimal lBalance))                
                    throw new Exception("Failed to retrieve balance");
                return lBalance;
            }
        }

        public int GetConfirmations(string aCurrencyName, string aTicker)
        {
            return GetPoloniexCurrency(aTicker).minConf;
        }

        public string GetDepositAddress(ExchangeMarket aMarket)
        {
            if (!IsCredentialsSet)           
                throw new Exception("No Credentials were set");
            string lResult;
            string lTicker = aMarket.BaseCurrencyInfo.Ticker;
            using(PoloniexClient lClient = new PoloniexClient())
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

        public ExchangeMarket[] GetMarketCoins(string aCurrencyName, string aTicker, GetWalletIDDelegate aGetWalletIDFunction = null)
        {
            DoUpdateMarketCoins().Wait();
            string[] lAuxSplitKey;
            string lAuxDestinationTicker;
            if (!FLocalCacheOfMarkets.TryGetValue(aTicker, out ExchangeMarket[] lCoinMarkets))
            {
                lCoinMarkets = FCacheMarkets.Where(lPair => lPair.Key.Contains(aTicker))
                .Select(lPair => new ExchangeMarket(this) 
                { 
                  BaseCurrencyInfo = new ExchangeMarket.CurrencyInfo
                  {
                      Ticker = aTicker,
                      Name = GetPoloniexCurrency(aTicker)?.name?? throw new Exception("Base market currency info not found"),
                      WalletID = aGetWalletIDFunction?.Invoke(aCurrencyName, aTicker)
                  },
                  DestinationCurrencyInfo = new ExchangeMarket.CurrencyInfo
                  {
                      Ticker = lAuxDestinationTicker = ((lAuxSplitKey = lPair.Key.Split('_'))[0] == aTicker)? lAuxSplitKey[1] : lAuxSplitKey[0],
                      Name = GetPoloniexCurrency(lAuxDestinationTicker)?.name?? throw new Exception("Destination market currency info not found"),
                      WalletID = aGetWalletIDFunction?.Invoke(Name, lAuxDestinationTicker)
                  },
                  MarketName = lPair.Key,
                  IsSell = lAuxSplitKey[0] != aTicker,
                  MinimumTrade = 0.0001M, //This info I found it at the web         
                  MinTradeIsBaseCurrency = true
                }).ToArray();

                if (aGetWalletIDFunction != null)
                    FLocalCacheOfMarkets.TryAdd(aTicker, lCoinMarkets);
            }

            return lCoinMarkets;

        }

        private PoloniexCurrency GetPoloniexCurrency(string aTicker)
        {
            if(!FCacheCurrencies.TryGetValue(aTicker, out PoloniexCurrency lCurrency))
            {
                using(PoloniexClient lClient = new PoloniexClient())
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

        public MarketPriceInfo GetMarketPrice(string aMarketName)
        {
            MarketPriceInfo lResult = new MarketPriceInfo();
            if (FMarketPrices.TryGetValue(aMarketName, out MarketPriceInfo lValue))
                lResult = lValue;
            return lResult;
        }

        public TradeOrderStatusInfo GetOrderStatus(string aUuid)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");
            var lOrderNumber = Convert.ToInt64(aUuid);
            var lResult = new TradeOrderStatusInfo(){ ID = aUuid };
            using(PoloniexClient lClient = new PoloniexClient())
            {
                var lResponse = lClient.GetOrderStatus(lOrderNumber);
                if (!lResponse.Success)
                    throw new Exception("Unable to retrieve order info from Poloniex");
                if(Convert.ToBoolean(lResponse.Data.success) && string.IsNullOrEmpty(lResponse.Data.error))
                {
                    if(!lResponse.Data.result.TryGetValue(aUuid, out PoloniexOrderStatus lOrderSummary))
                        throw new Exception("Order info not found");
                    lResult.ExchangeMarketName = lOrderSummary.currencyPair;
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
                        lResult.ExchangeMarketName = lOrderTradesResponse.Data.First().currencyPair;
                    }
                }
            }
            return lResult;
        }

        public decimal GetTransactionsFee(string aCurrencyName, string aTicker)
        {
            return GetPoloniexCurrency(aTicker).txFee;
        }

        public bool PlaceOrder(UserTradeOrder aOrder, ExchangeMarket aMarket, bool aUseProxy = true)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");

            if (aOrder == null || aMarket == null)
                throw new ArgumentNullException(aOrder == null ? nameof(aOrder) : nameof(aMarket), "Invalid argument: " + aOrder == null ? nameof(aOrder) : nameof(aMarket));            

            if (aOrder.Status != OrderStatus.Waiting)
                return false;

            using (var lClient = new PoloniexClient())
            {
                JKrof.Objects.WebCallResult<PoloniexOrderPlaceResult> lResponse;
                if (aMarket.IsSell)
                    lResponse = lClient.PlaceSellOrder(aMarket.MarketName, aOrder.Rate, aOrder.SentQuantity);
                else
                    lResponse = lClient.PlaceBuyOrder(aMarket.MarketName, aOrder.Rate, aOrder.SentQuantity / aOrder.Rate);
                if (!lResponse.Success)
                    throw new Exception($"Unable to place trade order at exchange. Error: {lResponse.Error}");

                aOrder.ID = lResponse.Data.orderNumber.ToString();
                aOrder.OpenTime = DateTime.UtcNow;
            }
            return true;
        }

        public bool RefundOrder(ExchangeMarket aMarket, UserTradeOrder aOrder, string aAddress, bool aUseProxy = true)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");

            if (aOrder.Status == OrderStatus.Withdrawn)
                return false;

            PoloniexClientOptions lPoloniexClientOptions = new PoloniexClientOptions()
            {
                Proxy = PandoraProxy.GetApiProxy(),
                ApiCredentials = new JKrof.Authentication.ApiCredentials(FUserCredentials.Item1, FUserCredentials.Item2)
            };
            using (PoloniexClient lClient = aUseProxy ? new PoloniexClient(lPoloniexClientOptions) : new PoloniexClient())
            {
                var lResponse = lClient.Withdraw(aMarket.BaseCurrencyInfo.Ticker, aOrder.SentQuantity, aAddress);
                if (!lResponse.Success)
                    throw new Exception("Failed to refund order. Error message:" + lResponse.Error.Message);
                if (!string.IsNullOrEmpty(lResponse.Data.error))
                    throw new Exception($"Failed to refund order. Error message: {lResponse.Data.error}");
            }

            return true;
        }

        public void SetCredentials(string aApiKey, string aApiSecret)
        {
            var lKeyGuid = aApiKey.Replace("-",string.Empty);
            var lFormatedGUID = Enumerable.Range(0, lKeyGuid.Count() / 8)
                                          .Select(lCounter => lKeyGuid.Substring(lCounter * 8, 8))
                                          .Aggregate((first, second) => string.Concat(first, "-", second)).ToUpperInvariant();
            JKrof.Authentication.ApiCredentials lCredentials = new JKrof.Authentication.ApiCredentials(lFormatedGUID, aApiSecret.ToLowerInvariant());
            PoloniexClientOptions lClientOptions = new PoloniexClientOptions { ApiCredentials = lCredentials };
            PoloniexClient.SetDefaultOptions(lClientOptions);
            using(PoloniexClient lClient = new PoloniexClient())
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
            FPriceUpdaterTask = new Timer(DoUpdateMarketPrices, null, 0, PRICE_REFRESH_PERIOD); //Every Minute
        }

        public void StopMarketUpdating()
        {
            OnMarketPricesChangingInternal -= PoloniexExchange_OnMarketPricesChangingInternal;
            if (FPriceUpdaterTask == null || OnMarketPricesChangingInternal != null)
                return;
            FPriceUpdaterTask?.Change(Timeout.Infinite, Timeout.Infinite);
            FPriceUpdaterTask?.Dispose();
            FPriceUpdaterTask = null;
        }

        private static void DoUpdateMarketPrices(object aState)
        {
            try
            {
                List<string> lChanged = new List<string>();
                using (var lClient = new PoloniexClient())
                {
                    var lMarketsData = lClient.GetTickerMarkets();
                    if (!lMarketsData.Success)
                        throw new Exception("Unable to get updated market prices info");
                    foreach(var lMarketData in lMarketsData.Data)
                    {
                        string lMarketName = lMarketData.Key;
                        MarketPriceInfo lRemoteMarketPrice = new MarketPriceInfo
                        {
                            Last = lMarketData.Value.last,
                            Bid = lMarketData.Value.highestBid,
                            Ask = lMarketData.Value.lowestAsk
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
                            FMarketPrices.TryAdd(lMarketName, lRemoteMarketPrice);
                            lChanged.Add(lMarketName);
                        }
                    }
                }
                if(lChanged.Any())
                    OnMarketPricesChangingInternal?.Invoke(lChanged, (int)Identifier);
            }
            catch (Exception ex)
            {
                Universal.Log.Write(LogLevel.Error, $"Exception thrown in market price update process. Details {ex}");
            }
        }        

        public bool WithdrawOrder(ExchangeMarket aMarket, UserTradeOrder aOrder, string aAddress, decimal aTxFee, bool aUseProxy = true)
        {
            if (!IsCredentialsSet)
                throw new Exception("No Credentials were set");

            if (aOrder.Status == OrderStatus.Withdrawn)
                return false;

            var lPoloniexClientOptions = new PoloniexClientOptions()
            {
                Proxy = PandoraProxy.GetApiProxy(),
                ApiCredentials = new JKrof.Authentication.ApiCredentials(FUserCredentials.Item1, FUserCredentials.Item2)
            };
            using (var lClient = aUseProxy ? new PoloniexClient(lPoloniexClientOptions) : new PoloniexClient())
            {
                decimal lQuantityToWithdraw = 0;
                var lOrderID = Convert.ToInt64(aOrder.ID);
                var lResult = lClient.GetOrderTrades(lOrderID);
                if(!lResult.Success)
                    throw new Exception("Failed to get order to withdraw. Error message:" + lResult.Error.Message);
                if(!string.IsNullOrEmpty(lResult.Data.error))
                    throw new Exception($"Failed to get order to withdraw. Error message: {lResult.Error.Message}");
                foreach (var lTrade in lResult.Data)
                {
                    if (aMarket.IsSell)
                        lQuantityToWithdraw += lTrade.total - (lTrade.total * lTrade.fee);
                    else
                        lQuantityToWithdraw += lTrade.amount - (lTrade.amount * lTrade.fee);
                }
                var lResponse = lClient.Withdraw(aMarket.DestinationCurrencyInfo.Ticker, lQuantityToWithdraw, aAddress);
                if (!lResponse.Success)
                    throw new Exception($"Failed to withdraw order. Error message: {lResult.Error.Message}");
                if (!string.IsNullOrEmpty(lResponse.Data.error))
                    throw new Exception($"Failed to withdraw order. Error message: {lResponse.Data.error}");
            }
            return true;
        }
    }
}
