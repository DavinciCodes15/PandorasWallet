using Bittrex.Net;
using Bittrex.Net.Objects;
using Pandora.Client.Exchange.JKrof.Objects;
using Pandora.Client.Exchange.JKrof.Sockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.Exchanges
{
    public class BittrexExchange : BasePandoraExchange
    {
        private static new string ExchangeName => "Bittrex";
#pragma warning disable IDE0051 // Remove unused private members
        private static new uint ExchangeID => 10;
#pragma warning restore IDE0051 // Remove unused private members

        private static ConcurrentDictionary<string, decimal> FMarketPrices;
        private ConcurrentDictionary<string, decimal> FCurrencyFees;
        private ConcurrentDictionary<string, int> FCurrencyConfirmations;
        private BittrexMarket[] FMarkets;
        private static UpdateSubscription FMarketPriceSocketSubs;
        private DateTime FLastRetrieval;

        private static event Action OnMarketPricesChangingStatic;

        public override event Action OnMarketPricesChanging;

        static BittrexExchange()
        {
            FMarketPrices = new ConcurrentDictionary<string, decimal>();
            try
            {
                RefreshMarketPrices();
                SubscribeToMarketPriceUpdating();
            }
            catch (Exception ex)
            {
                Universal.Log.Write(Universal.LogLevel.Error, $"Failed to subscribe to Market Price Update. Exception: {ex}");
            }
        }

        public BittrexExchange(string aKey, string aSecret)
        {
            FCurrencyFees = new ConcurrentDictionary<string, decimal>();
            FCurrencyConfirmations = new ConcurrentDictionary<string, int>();
            OnMarketPricesChangingStatic += OnMarketPricesChanging;
            SetCredentials(aKey, aSecret);
        }

        private BittrexClientOptions GetBittrexOptions(bool aWithProxy = false)
        {
            BittrexClientOptions lBittrexClientOptions;
            if (IsCredentialSet)
                lBittrexClientOptions = new BittrexClientOptions()
                {
                    ApiCredentials = new JKrof.Authentication.ApiCredentials(FUserCredentials[0], FUserCredentials[1])
                };
            else
                lBittrexClientOptions = new BittrexClientOptions();

            if (aWithProxy)
                lBittrexClientOptions.Proxy = PandoraProxy.GetApiProxy();
            return lBittrexClientOptions;
        }

        //private BittrexSocketClientOptions GetBittrexSocketOptions()
        //{
        //    BittrexSocketClientOptions lBittrexClientOptions;
        //    if (IsCredentailsSet)
        //        lBittrexClientOptions = new BittrexSocketClientOptions()
        //        {
        //            ApiCredentials = new JKrof.Authentication.ApiCredentials(FUserCredentials[0], FUserCredentials[1])
        //        };
        //    else
        //        lBittrexClientOptions = new BittrexSocketClientOptions();
        //    return lBittrexClientOptions;
        //}

        private static void SubscribeToMarketPriceUpdating()
        {
            if (FMarketPriceSocketSubs != null)
                return;

            CallResult<Pandora.Client.Exchange.JKrof.Sockets.UpdateSubscription> lResult;
            using (var lSocketClient = new BittrexSocketClient())
                lResult = lSocketClient.SubscribeToMarketSummariesLiteUpdate((data) =>
                {
                    foreach (BittrexStreamMarketSummaryLite it in data)
                    {
                        if (it.Last.HasValue)
                            FMarketPrices.AddOrUpdate(it.MarketName, it.Last.Value, (key, oldValue) => it.Last.Value);
                    }
                    OnMarketPricesChangingStatic?.Invoke();
                });

            if (!lResult.Success)
                throw new Exception("Failed to Subscribe to market socket updates");
            FMarketPriceSocketSubs = lResult.Data;
        }

        private void UnSubscribeToMarketPriceUpdating()
        {
            using (var lSocketClient = new BittrexSocketClient())
                lSocketClient.Unsubscribe(FMarketPriceSocketSubs).Wait();
            FMarketPriceSocketSubs = null;
        }

        private static void RefreshMarketPrices()
        {
            using (BittrexClient lClient = new BittrexClient())
            {
                CallResult<BittrexMarketSummary[]> lResponse = lClient.GetMarketSummaries();
                if (lResponse.Success)
                    foreach (BittrexMarketSummary it in lResponse.Data)
                        try
                        {
                            FMarketPrices.AddOrUpdate(it.MarketName, it.Last.Value, (key, oldValue) => it.Last.Value);
                        }
                        catch
                        {
                            FMarketPrices.AddOrUpdate(it.MarketName, 0, (key, oldValue) => 0);
                        }
            }
        }

        public override string GetDepositAddress(string aCoinIdentifier)
        {
            if (!IsCredentialSet)
                throw new Exception("No Credentials were set");

            using (BittrexClient lClient = new BittrexClient(GetBittrexOptions()))
            {
                CallResult<BittrexDepositAddress> lResponse = lClient.GetDepositAddress(aCoinIdentifier);
                if (!lResponse.Success)
                    throw new Exception("Failed to retrieve Bittrex Address. " + lResponse.Error.Message);
                BittrexDepositAddress lAddress = lResponse.Data;
                return lAddress.Address;
            }
        }

        /// <summary>
        /// Get the minimum amount of confirmations needed to be able to use balance in trading
        /// </summary>
        /// <param name="aCoinIdentifier">Ticker of the coin you want data from</param>
        /// <returns></returns>
        public override decimal GetExchangeTxMinConfirmations(string aCoinIdentifier)
        {
            if (!FCurrencyConfirmations.ContainsKey(aCoinIdentifier))
                GetCurrencyExchangeData();
            if (!FCurrencyConfirmations.TryGetValue(aCoinIdentifier, out int lConfirmations))
                lConfirmations = -1;
            return lConfirmations;
        }

        /// <summary>
        /// Get the transaction fee used by bittrex
        /// </summary>
        /// <param name="aCoinIdentifier">Ticker of the coin you want data from</param>
        /// <returns></returns>
        public override decimal GetExchangeCoinTxFee(string aCoinIdentifier)
        {
            if (!FCurrencyFees.ContainsKey(aCoinIdentifier))
                GetCurrencyExchangeData();
            return FCurrencyFees[aCoinIdentifier];
        }

        private void GetCurrencyExchangeData()
        {
            using (BittrexClient lClient = new BittrexClient())
            {
                CallResult<BittrexCurrency[]> lResponse = lClient.GetCurrencies();
                if (!lResponse.Success)
                    throw new Exception("Failed to retrieve balance");
                foreach (BittrexCurrency it in lResponse.Data)
                {
                    FCurrencyFees[it.Currency] = it.TransactionFee;
                    FCurrencyConfirmations[it.Currency] = it.MinConfirmation + 1;
                }
            }
        }

        public override ExchangeMarket2[] GetCoinExchangeMarkets(string aCoinIdentifier)
        {
            RefreshMarketsCache();
            ExchangeMarket2[] lCoinMarkets = FMarkets.Where(x => x.IsActive == true && (x.MarketCurrency == aCoinIdentifier || x.BaseCurrency == aCoinIdentifier))
                       .Select(x => new ExchangeMarket2(this)
                       {
                           BaseTicker = aCoinIdentifier,
                           CoinName = x.BaseCurrency != aCoinIdentifier ? x.BaseCurrencyLong : x.MarketCurrencyLong,
                           CoinTicker = x.BaseCurrency != aCoinIdentifier ? x.BaseCurrency : x.MarketCurrency,
                           MarketName = x.MarketName,
                           IsSell = x.BaseCurrency != aCoinIdentifier,
                           MinimumTrade = x.MinTradeSize
                       }).ToArray();
            return lCoinMarkets;
        }

        private void RefreshMarketsCache()
        {
            if (FLastRetrieval == DateTime.MinValue || FLastRetrieval < DateTime.UtcNow.AddHours(-1))
            {
                using (BittrexClient lClient = new BittrexClient())
                {
                    CallResult<BittrexMarket[]> lResponse = lClient.GetMarkets();
                    if (!lResponse.Success)
                        throw new Exception("Failed to retrieve Markets");
                    FMarkets = lResponse.Data;
                    FLastRetrieval = DateTime.UtcNow;
                }
            }
        }

        public override decimal GetMarketPrice(string aMarketIdentifier)
        {
            if (!FMarketPrices.TryGetValue(aMarketIdentifier, out decimal lValue))
                lValue = 0;
            return lValue;
        }

        public override MarketOrder GetOrderStatus(string aOrderUid)
        {
            Guid lOrderGuid = new Guid(aOrderUid);
            using (BittrexClient lClient = new BittrexClient(GetBittrexOptions(true)))
            {
                CallResult<BittrexAccountOrder> lResponse = lClient.GetOrder(lOrderGuid);
                if (!lResponse.Success)
                {
                    throw new Exception("Failed to retrieve order specified");
                }

                return new MarketOrder
                {
                    ID = lResponse.Data.OrderUuid.ToString(),
                    Completed = lResponse.Data.QuantityRemaining == 0,
                    Cancelled = lResponse.Data.CancelInitiated,
                    Market = lResponse.Data.Exchange
                };
            }
        }

        public override decimal GetUserAvailableBalance(string aCoinIdentifier)
        {
            if (!IsCredentialSet)
                throw new Exception("No Credentials were set");
            using (BittrexClient lClient = new BittrexClient(GetBittrexOptions()))
            {
                CallResult<BittrexBalance> lResponse = lClient.GetBalance(aCoinIdentifier);
                if (!lResponse.Success)
                    throw new Exception("Failed to retrieve balance");
                return lResponse.Data.Available.Value;
            }
        }

        public override MarketOrder PlaceOrder(bool aisSell, string aMarketName, decimal aQuantity, decimal aRate)
        {
            if (!IsCredentialSet)
                throw new Exception("No Credentials were set");

            using (BittrexClient lClient = new BittrexClient(GetBittrexOptions(true)))
            {
                CallResult<Bittrex.Net.Objects.BittrexGuid> lResponse;
                if (aisSell)
                    lResponse = lClient.PlaceOrder(Bittrex.Net.Objects.OrderSide.Sell, aMarketName, aQuantity, aRate);
                else
                    lResponse = lClient.PlaceOrder(Bittrex.Net.Objects.OrderSide.Buy, aMarketName, aQuantity / aRate, aRate);

                if (!lResponse.Success)
                    throw new Exception("Bittrex Error. Message: " + lResponse.Error.Message);

                Guid lUuid = lResponse.Data.Uuid;
                CallResult<BittrexAccountOrder> lResponse2 = lClient.GetOrder(lUuid);
                if (!lResponse.Success)
                    throw new Exception("Failed to verify order with server");

                BittrexAccountOrder lReceivedOrder = lResponse2.Data;
                MarketOrder lResult = new MarketOrder()
                {
                    ID = lUuid.ToString(),
                    OpenTime = lReceivedOrder.Opened,
                    Cancelled = lReceivedOrder.CancelInitiated,
                    Completed = lReceivedOrder.QuantityRemaining == 0
                };
                return lResult;
            }
        }

        /// <summary>
        /// Bittrex removes the txfee from the amount sent
        /// </summary>
        /// <param name="aAddress"></param>
        /// <param name="aCoinTicker"></param>
        /// <param name="aQuantity"></param>
        /// <param name="aTxFee"></param>
        public override void Withdraw(string aAddress, string aCoinTicker, decimal aQuantity, decimal aTxFee)
        {
            if (!IsCredentialSet)
                throw new Exception("No Credentials were set");
            using (BittrexClient lClient = new BittrexClient(GetBittrexOptions(true)))
            {
                CallResult<BittrexGuid> lResponse = lClient.Withdraw(aCoinTicker, aQuantity, aAddress);
                if (!lResponse.Success)
                    throw new Exception("Failed to withdraw. Error message:" + lResponse.Error.Message);
            }
        }

        public override bool CancelOrder(string aUid)
        {
            using (BittrexClient lClient = new BittrexClient(GetBittrexOptions(true)))
            {
                var lResponse = lClient.CancelOrder(new Guid(aUid));
                return lResponse.Success;
            }
        }

        public override bool TestCredentials()
        {
            if (IsCredentialSet)
            {
                var lBittrexOptions = new BittrexClientOptions()
                {
                    ApiCredentials = new JKrof.Authentication.ApiCredentials(FUserCredentials[0], FUserCredentials[1])
                };
                using (BittrexClient lClient = new BittrexClient(lBittrexOptions))
                {
                    CallResult<BittrexBalance> lResponse = lClient.GetBalance("BTC");
                    return lResponse.Success;
                }
            }
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            OnMarketPricesChangingStatic -= OnMarketPricesChanging;
            base.Dispose(disposing);
        }

        public override decimal CalculateExchangeTradingFee(decimal aValue)
        {
            return (aValue * (decimal)0.0025);
        }
    }
}