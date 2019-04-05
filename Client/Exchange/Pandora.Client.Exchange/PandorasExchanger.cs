using Bittrex.Net;
using Bittrex.Net.Objects;
using Pandora.Client.Exchange.JKrof.Objects;
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
        private bool FCredentialsSet;

        public bool IsCredentialsSet => FCredentialsSet;

        public event Action OnMarketPricesChanging;

        public string CurrentExchange => "Bittrex";

        public Dictionary<long, string> ExchangeList { get; private set; }

        public ExchangeTxDBHandler TransactionHandler { get; set; }

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
            Pandora.Client.Exchange.JKrof.Authentication.ApiCredentials lCredentials = new Pandora.Client.Exchange.JKrof.Authentication.ApiCredentials(aApiKey, aApiSecret);

            BittrexClientOptions lClientOptions = new Bittrex.Net.Objects.BittrexClientOptions { ApiCredentials = lCredentials };
            BittrexSocketClientOptions lSocketOptions = new Bittrex.Net.Objects.BittrexSocketClientOptions { ApiCredentials = lCredentials };

            BittrexClient.SetDefaultOptions(lClientOptions);
            BittrexSocketClient.SetDefaultOptions(lSocketOptions);

            using (BittrexClient lClient = new BittrexClient())
            {
                CallResult<BittrexBalance> lResponse = lClient.GetBalance("BTC");

                if (!lResponse.Success)
                {
                    throw new Exception("Incorrect Key Pair for selected exchange");
                }
            }

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

        public void PlaceOrder(MarketOrder aOrder, ExchangeMarket aMarket)
        {
            if (!FCredentialsSet)
            {
                throw new Exception("No Credentials were set");
            }

            if (aOrder == null || aMarket == null)
            {
                throw new ArgumentNullException(aOrder == null ? nameof(aOrder) : nameof(aMarket), "Invalid argument: " + aOrder == null ? nameof(aOrder) : nameof(aMarket));
            }

            using (BittrexClient lClient = new BittrexClient())
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

                Guid lUuid = lResponse.Data.Uuid;

                CallResult<BittrexAccountOrder> lResponse2 = lClient.GetOrder(lUuid);

                if (!lResponse.Success)
                {
                    throw new Exception("Failed to verify order with server");
                }

                BittrexAccountOrder lReceivedOrder = lResponse2.Data;

                aOrder.ID = lUuid;

                aOrder.OpenTime = lReceivedOrder.Opened;
                aOrder.Cancelled = lReceivedOrder.CancelInitiated;
                aOrder.Completed = lReceivedOrder.QuantityRemaining == 0;

                UpdateOrder(aOrder, OrderStatus.Placed);
            }
        }

        public void UpdateOrder(MarketOrder aNewOrder, OrderStatus aStatus)
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
            {
                throw new Exception("Failed to write new transaction into disk");
            }

            if (lTransactions != null && lTransactions.Any())
            {
                lock (FTransactions)
                    FTransactions.AddRange(lTransactions);

                int lErrorCounter;

                lock (FTransactions)
                {
                    MarketOrder[] lTxs = new MarketOrder[FTransactions.Count];
                    FTransactions.CopyTo(lTxs);
                }

                foreach (MarketOrder it in FTransactions)
                {
                    bool lOrdersLogs = TransactionHandler.ReadOrderLogs(it.InternalID, out List<OrderMessage> lMessages);

                    lMessages = lMessages.OrderByDescending(lMessage => lMessage.Time).ToList();

                    lErrorCounter = 0;

                    for (int it2 = 0; it2 < lMessages.Count; it2++)
                    {
                        OrderMessage lIndividualMessage = lMessages[it2];

                        if (lErrorCounter > 0)
                        {
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
                        }
                    }

                    it.ErrorCounter = lErrorCounter;
                }
            }

            return lTransactions;
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

            CallResult<Pandora.Client.Exchange.JKrof.Sockets.UpdateSubscription> lResult = FBittrexSocket.SubscribeToMarketSummariesLiteUpdate((data) =>
        {
            foreach (BittrexStreamMarketSummaryLite it in data)
            {
                if (it.Last.HasValue)
                    FMarketPrices.AddOrUpdate(it.MarketName, it.Last.Value, (key, oldValue) => it.Last.Value);
            }

            OnMarketPricesChanging?.Invoke();
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
                    ID = lResponse.Data.OrderUuid,
                    Completed = lResponse.Data.QuantityRemaining == 0,
                    Cancelled = lResponse.Data.CancelInitiated,
                    Market = lResponse.Data.Exchange
                };
            }
        }

        public void WithdrawOrder(ExchangeMarket aMarket, MarketOrder aOrder, string aAddress, decimal aTxFee)
        {
            if (!FCredentialsSet)
            {
                throw new Exception("No Credentials were set");
            }

            if (aOrder.Status == OrderStatus.Withdrawed)
            {
                return;
            }

            using (BittrexClient lClient = new BittrexClient())
            {
                decimal lRate = aMarket.IsSell ? 1 / aOrder.Rate : aOrder.Rate;
                decimal lRawAmount = aOrder.SentQuantity / lRate;
                decimal lQuantity = lRawAmount - aTxFee - (lRawAmount * (decimal)0.0025);
                CallResult<BittrexGuid> lResponse = lClient.Withdraw(aMarket.CoinTicker, lQuantity, aAddress);
                if (!lResponse.Success)
                {
                    throw new Exception("Failed to withdraw. Error message:" + lResponse.Error.Message);
                }
            }

            UpdateOrder(aOrder, OrderStatus.Withdrawed);
        }

        public void StopMarketUpdating()
        {
            FBittrexSocket.Unsubscribe(FMarketPriceSubscription).Wait();
            FMarketPriceSubscription = null;
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

    public enum OrderStatus
    {
        Waiting, Placed, Interrupted, Completed, Withdrawed, Initial
    }

    public class MarketOrder
    {
        public decimal SentQuantity { get; set; }
        public decimal Rate { get; set; }
        public Guid ID { get; set; }
        public DateTime OpenTime { get; set; }
        public bool Cancelled { get; set; }
        public bool Completed { get; set; }
        public string Market { get; set; }
        public OrderStatus Status { get; set; }
        public string CoinTxID { get; set; }
        public int InternalID { get; set; }
        public string BaseTicker { get; set; }
        public string Name { get; set; }
        public int ErrorCounter { get; set; }
        public decimal StopPrice { get; set; }
    }

    public class OrderMessage : System.Collections.IComparer
    {
        public enum OrderMessageLevel
        {
            None = 0, Info = 1, StageChange = 2, Error = 3, FatalError = 4, Finisher = 5
        }

        public string Message { get; set; }
        public DateTime Time { get; set; }
        public OrderMessageLevel Level { get; set; }

        public int Compare(object x, object y)
        {
            OrderMessage lx = (OrderMessage)x;
            OrderMessage ly = (OrderMessage)y;

            return lx.Time.CompareTo(ly.Time);
        }
    }
}