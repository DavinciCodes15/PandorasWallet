using Bittrex.Net;
using Bittrex.Net.Objects;
using Pandora.Client.Exchange.JKrof.Objects;
using Pandora.Client.Exchange.SaveManagers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Pandora.Client.Exchange
{
    public class PandoraExchanger
    {
        private static PandoraExchanger FInstance;
        private BittrexSocketClient FBittrexSocket;
        private ConcurrentDictionary<string, decimal> FMarketPrices;
        private Pandora.Client.Exchange.JKrof.Sockets.UpdateSubscription FMarketPriceSubscription;

        /// <summary>
        /// Tuple with user credentials. First element is Key, second is Secret
        /// </summary>
        private Tuple<string, string> FUserCredentials;

        private bool FCredentialsSet;

        public bool IsCredentialsSet => FCredentialsSet;

        public event Action<IEnumerable<string>> OnMarketPricesChanging;

        public string CurrentExchange => "Bittrex";

        public Dictionary<long, string> ExchangeList { get; private set; }

        public PandoraExchangeSQLiteSaveManager TransactionHandler { get; set; }

        public string DBFile => TransactionHandler.DBFilePath;

        private List<MarketOrder> FTransactions;
        private ConcurrentDictionary<string, decimal> FCurrencyFees;
        private ConcurrentDictionary<string, int> FCurrencyConfirmations;
        private BittrexMarket[] FMarkets;
        private DateTime FLastRetrieval;

        private PandoraExchanger()
        {
            ExchangeList = new Dictionary<long, string>
            {
                { 1, "Bittrex" }
            };

            FBittrexSocket = new BittrexSocketClient();
            FMarketPrices = new ConcurrentDictionary<string, decimal>();
            FTransactions = new List<MarketOrder>();
            FCurrencyFees = new ConcurrentDictionary<string, decimal>();
            FCurrencyConfirmations = new ConcurrentDictionary<string, int>();
        }

        public MarketOrder[] GetTradeTransactions(MarketOrder aMarket)
        {
            lock (FTransactions)
                return FTransactions.Where(x => x.Market == aMarket.Market).ToArray();
        }

        public decimal GetPrice(string aMarketName)
        {
            if (FMarketPrices.ContainsKey(aMarketName))
            {
                return FMarketPrices[aMarketName];
            }

            return 0;
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
            FCredentialsSet = true;
        }

        public void Clear()
        {
            BittrexClientOptions lClientOptions = new Bittrex.Net.Objects.BittrexClientOptions { ApiCredentials = null };
            BittrexSocketClientOptions lSocketOptions = new Bittrex.Net.Objects.BittrexSocketClientOptions { ApiCredentials = null };
            BittrexClient.SetDefaultOptions(lClientOptions);
            BittrexSocketClient.SetDefaultOptions(lSocketOptions);

            TransactionHandler = null;

            FCredentialsSet = false;
        }

        public decimal GetTransactionsFee(string aTicker)
        {
            if (!FCurrencyFees.ContainsKey(aTicker))
            {
                GetCurrencyRelatedData();
            }
            return FCurrencyFees[aTicker];
        }

        public int GetConfirmations(string aTicker)
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
                CallResult<BittrexCurrency[]> lResponse = lClient.GetCurrencies();

                if (!lResponse.Success)
                {
                    throw new Exception("Failed to retrieve balance");
                }

                foreach (BittrexCurrency it in lResponse.Data)
                {
                    FCurrencyFees[it.Currency] = it.TransactionFee;
                    FCurrencyConfirmations[it.Currency] = it.MinConfirmation + 1;
                }
            }
        }

        public decimal GetBalance(ExchangeMarket aMarket)
        {
            if (!FCredentialsSet)
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

        public bool PlaceOrder(MarketOrder aOrder, ExchangeMarket aMarket, bool aUseProxy = true)
        {
            if (!FCredentialsSet)
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
                    lResponse = lClient.PlaceOrder(Bittrex.Net.Objects.OrderSide.Sell, aOrder.Market, aOrder.SentQuantity, aOrder.Rate);
                }
                else
                {
                    lResponse = lClient.PlaceOrder(Bittrex.Net.Objects.OrderSide.Buy, aOrder.Market, aOrder.SentQuantity / aOrder.Rate, aOrder.Rate);
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

        public void UpdateOrderStatus(MarketOrder aNewOrder, OrderStatus aStatus)
        {
            MarketOrder lOrder;
            lock (FTransactions)
            {
                lOrder = FTransactions.Find(x => x.ID == aNewOrder.ID);
                if (lOrder != null)
                {
                    FTransactions[FTransactions.IndexOf(lOrder)] = aNewOrder;
                    if (!TransactionHandler.UpdateTransaction(aNewOrder, aStatus))
                    {
                        throw new Exception("Failed to write new transaction into disk");
                    }
                }
                else
                {
                    FTransactions.Add(aNewOrder);
                    if (!TransactionHandler.UpdateTransaction(aNewOrder, aStatus))
                    {
                        throw new Exception("Failed to write new transaction into disk");
                    }
                }
            }

            aNewOrder.Status = aStatus;
        }

        public MarketOrder[] LoadTransactions(string aTicker)
        {
            if (!TransactionHandler.ReadTransactions(out MarketOrder[] lTransactions, aTicker))
                throw new Exception("Failed to read exchange transactions from disk");

            if (lTransactions != null && lTransactions.Any())
            {
                int lErrorCounter;
                foreach (MarketOrder it in lTransactions)
                {
                    bool lOrdersLogs = TransactionHandler.ReadOrderLogs(it.InternalID, out List<OrderMessage> lMessages);
                    lMessages = lMessages.OrderBy(lMessage => lMessage.Time).ToList();
                    lErrorCounter = 0;
                    for (int it2 = 0; it2 < lMessages.Count; it2++)
                    {
                        OrderMessage lIndividualMessage = lMessages[it2];

                        switch (lIndividualMessage.Level)
                        {
                            case OrderMessage.OrderMessageLevel.Error:
                                lErrorCounter += 60;
                                break;

                            case OrderMessage.OrderMessageLevel.StageChange:
                                lErrorCounter = 0;
                                break;

                            case OrderMessage.OrderMessageLevel.FatalError:
                                lErrorCounter = -1;
                                break;
                        }
                        if (lErrorCounter == -1)
                            break;
                    }
                    it.ErrorCounter = lErrorCounter;
                }
                lock (FTransactions)
                    FTransactions.AddRange(lTransactions);
            }
            return lTransactions;
        }

        public void CancelOrder(MarketOrder aOrder, bool aUseProxy = true)
        {
            if (!FCredentialsSet)
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

        public ExchangeMarket[] GetMarketCoins(string aTicker)
        {
            if (FLastRetrieval == DateTime.MinValue || FLastRetrieval < DateTime.UtcNow.AddHours(-1))
            {
                using (BittrexClient lClient = new BittrexClient())
                {
                    CallResult<BittrexMarket[]> lResponse = lClient.GetMarkets();

                    if (!lResponse.Success)
                    {
                        throw new Exception("Failed to retrieve Markets");
                    }

                    FMarkets = lResponse.Data;
                    FLastRetrieval = DateTime.UtcNow;
                }
            }

            ExchangeMarket[] lCoinMarkets = FMarkets.Where(x => x.IsActive == true && (x.MarketCurrency == aTicker || x.BaseCurrency == aTicker))
                   .Select(x => new ExchangeMarket
                   {
                       BaseTicker = aTicker,
                       CoinName = x.BaseCurrency != aTicker ? x.BaseCurrencyLong : x.MarketCurrencyLong,
                       CoinTicker = x.BaseCurrency != aTicker ? x.BaseCurrency : x.MarketCurrency,
                       MarketName = x.MarketName,
                       IsSell = x.BaseCurrency != aTicker,
                       MinimumTrade = x.MinTradeSize
                   }).ToArray();

            return lCoinMarkets;
        }

        public string GetDepositAddress(ExchangeMarket aMarket)
        {
            if (!FCredentialsSet)
            {
                throw new Exception("No Credentials were set");
            }

            using (BittrexClient lClient = new BittrexClient())
            {
                CallResult<BittrexDepositAddress> lResponse = lClient.GetDepositAddress(aMarket.BaseTicker);

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
            {
                return;
            }

            using (BittrexClient lClient = new BittrexClient())
            {
                CallResult<BittrexMarketSummary[]> lResponse = lClient.GetMarketSummaries();

                if (lResponse.Success)
                {
                    foreach (BittrexMarketSummary it in lResponse.Data)
                    {
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
            }

            CallResult<Pandora.Client.Exchange.JKrof.Sockets.UpdateSubscription> lResult = FBittrexSocket.SubscribeToMarketSummariesLiteUpdate((lFullData) =>
            {
                List<string> lChanged = new List<string>();
                foreach (BittrexStreamMarketSummaryLite lData in lFullData)
                    if (lData.Last.HasValue)
                    {
                        decimal lPrice;
                        while (!FMarketPrices.TryGetValue(lData.MarketName, out lPrice)) ;
                        if (lPrice != lData.Last.Value)
                        {
                            FMarketPrices.AddOrUpdate(lData.MarketName, lData.Last.Value, (key, oldValue) => lData.Last.Value);
                            lChanged.Add(lData.MarketName);
                        }
                    }
                if (lChanged.Any())
                    OnMarketPricesChanging?.Invoke(lChanged);
            });

            if (!lResult.Success)
            {
                throw new Exception("Failed to Update Market Prices");
            }

            FMarketPriceSubscription = lResult.Data;
        }

        public MarketOrder GetOrder(Guid lUuid)
        {
            using (BittrexClient lClient = new BittrexClient())
            {
                CallResult<BittrexAccountOrder> lResponse = lClient.GetOrder(lUuid);
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

        /// <summary>
        /// Tries to withdraw the total amount returned by an order from the exchange
        /// </summary>
        /// <param name="aMarket">Market of the order</param>
        /// <param name="aOrder">Completed order to withdraw</param>
        /// <param name="aAddress">Coin address to deposit funds</param>
        /// <param name="aTxFee">Fee to discount from balance withdrawed</param>
        /// <param name="aUseProxy">Use pandora proxy or not</param>
        public bool RefundOrder(ExchangeMarket aMarket, MarketOrder aOrder, string aAddress, bool aUseProxy = true)
        {
            if (!FCredentialsSet)
            {
                throw new Exception("No Credentials were set");
            }

            if (aOrder.Status == OrderStatus.Withdrawed)
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
                CallResult<BittrexGuid> lResponse = lClient.Withdraw(aMarket.BaseTicker, aOrder.SentQuantity, aAddress);
                if (!lResponse.Success)
                {
                    throw new Exception("Failed to withdraw. Error message:" + lResponse.Error.Message);
                }
            }

            return true;
        }

        /// <summary>
        /// Tries to withdraw the total amount returned by an order from the exchange
        /// </summary>
        /// <param name="aMarket">Market of the order</param>
        /// <param name="aOrder">Completed order to withdraw</param>
        /// <param name="aAddress">Coin address to deposit funds</param>
        /// <param name="aTxFee">Fee to discount from balance withdrawed</param>
        /// <param name="aUseProxy">Use pandora proxy or not</param>
        public bool WithdrawOrder(ExchangeMarket aMarket, MarketOrder aOrder, string aAddress, decimal aTxFee, bool aUseProxy = true)
        {
            if (!FCredentialsSet)
                throw new Exception("No Credentials were set");

            if (aOrder.Status == OrderStatus.Withdrawed)
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
                    lQuantityToWithdraw = (lRemoteOrder.Price - lRemoteOrder.CommissionPaid) / lRemoteOrder.PricePerUnit.Value;
                else if (lRemoteOrder.Type == OrderSideExtended.LimitSell)
                    lQuantityToWithdraw = (lRemoteOrder.Price - lRemoteOrder.CommissionPaid);
                CallResult<BittrexGuid> lWithdrawResponse = lClient.Withdraw(aMarket.CoinTicker, lQuantityToWithdraw, aAddress);
                if (!lWithdrawResponse.Success)
                    throw new Exception("Failed to withdraw. Error message:" + lWithdrawResponse.Error.Message);
            }
            return true;
        }

        public async void StopMarketUpdating()
        {
            if (FMarketPriceSubscription != null)
            {
                await FBittrexSocket.Unsubscribe(FMarketPriceSubscription);
                FMarketPriceSubscription = null;
            }
        }

        public static PandoraExchanger GetInstance()
        {
            if (FInstance == null)
            {
                FInstance = new PandoraExchanger();
            }

            return FInstance;
        }

        public class ExchangeMarket
        {
            public decimal Price => Math.Round(IsSell ? 1 / BasePrice : BasePrice, 7);

            public decimal BasePrice
            {
                get
                {
                    if (FInstance.FMarketPrices.TryGetValue(MarketName, out decimal lValue))
                    {
                        return lValue;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            public string BaseTicker { get; set; }
            public string CoinName { get; set; }
            public string CoinTicker { get; set; }
            public string MarketName { get; set; }
            public decimal MinimumTrade { get; set; }
            public bool IsSell { get; set; }
        }
    }
}