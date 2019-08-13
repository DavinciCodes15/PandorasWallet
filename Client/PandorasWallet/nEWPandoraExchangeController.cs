using Pandora.Client.ClientLib;
using Pandora.Client.Exchange;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.Client.PandorasWallet
{
    public class NewPandoraExchangeController
    {
        private static NewPandoraExchangeController FInstance;
        private PandoraExchangeProfileManager FProfileManager;
        private PandoraExchangeOrderManager FOrderManager;
        private List<Tuple<long, string>> FCurrencyTracked;
        private Dictionary<int, OrderCoinTx> FOrdersCoinTxs;
        private CancellationTokenSource FExchangeTaskCancellationSource;
        private Task FLocalScanCore;

        public delegate void OrderChangeDelegate(int aProfileID, string aCurrencyID, int aOrderInternalID);

        public delegate string OrderCoinTxDelegate(decimal aAmount, long aCurrencyID, string aAddressToSend);

        public event OrderChangeDelegate OnNewOrderLogs;

        public event OrderChangeDelegate OnNewOrderStatus;

        public event OrderCoinTxDelegate OnSendCoinToTx;

        public string[] AvailableExchanges => FProfileManager.ExchangesInventory.Select(lEx => lEx.Value).ToArray();
        public Dictionary<int, string> Profiles => FProfileManager.ProfileNames;

        private NewPandoraExchangeController()
        {
            FProfileManager = PandoraExchangeProfileManager.GetProfiler();
            FOrderManager = PandoraExchangeOrderManager.GetOrderManager();
            FCurrencyTracked = new List<Tuple<long, string>>();
            FOrdersCoinTxs = new Dictionary<int, OrderCoinTx>();
            FLocalScanCore = Task.Run(() => LocalOrderScanTask(), FExchangeTaskCancellationSource.Token);
        }

        public static NewPandoraExchangeController GetInstance()
        {
            if (FInstance == null)
                FInstance = new NewPandoraExchangeController();

            return FInstance;
        }

        //Initializing Methods

        public void InitializeDefaultDataFolder(string aDataPath, string aFileID)
        {
            FOrderManager.ConfigureSaveLocation(false, aDataPath, aFileID);
            FProfileManager.ConfigureSaveLocation(false, aDataPath, aFileID);
        }

        public void SetProfileCredential(int aProfileID, string aKey, string aSecret)
        {
            var lProfileExEntity = FProfileManager.GetProfileExchangeEntity(aProfileID);
            lProfileExEntity.SetCredentials(aKey, aSecret);
        }

        public int GetExchangeMinimumConfirmations(CurrencyItem aCurrencyItem, int aProfileID)
        {
            var lProfileExEntity = FProfileManager.GetProfileExchangeEntity(aProfileID);
            return lProfileExEntity.GetMinimumConfirmations(aCurrencyItem.Ticker);
        }

        public void SetCurrencyToScanOrders(CurrencyItem aSelectedCurrencyItem, int aProfileID)
        {
            var lProfileExEntity = FProfileManager.GetProfileExchangeEntity(aProfileID);
            if (!FOrderManager.GetTransactionsByTicker(aSelectedCurrencyItem.Ticker, out MarketOrder[] lSavedExTxs))
                lSavedExTxs = FOrderManager.LoadTransactions(aSelectedCurrencyItem.Ticker);
            if (!lProfileExEntity.OnOrderCompletedSet)
                lProfileExEntity.OnOrderCompleted += ProfileExEntity_OnOrderCompleted;
            if (!lProfileExEntity.OnOrderCancellaedSet)
                lProfileExEntity.OnOrderCancelled += ProfileExEntity_OnOrderCancelled;
            var lCurrencyID = new Tuple<long, string>(aSelectedCurrencyItem.Id, aSelectedCurrencyItem.Ticker);
            lock (FCurrencyTracked)
                if (!FCurrencyTracked.Contains(lCurrencyID))
                    FCurrencyTracked.Add(lCurrencyID);
        }

        //End of initializing methods

        public void MarkTrasactionAsConfirmed(string aTxId, long aCurrencyID)
        {
        }

        public void CreateNewProfile(string aExchangeName, string aProfileName)
        {
            if (!TryToGetExchangeID(aExchangeName, out uint lExchangeID))
                throw new Exception("Exchange selected not found");
            FProfileManager.AddNewExchangeProfile(lExchangeID, aProfileName);
        }

        private bool TryToGetExchangeID(string aExchangeName, out uint aExchangeID)
        {
            aExchangeID = FProfileManager.ExchangesInventory.Where(lEx => lEx.Value == aExchangeName).Select(lEx => lEx.Key).FirstOrDefault();
            return aExchangeID > 0;
        }

        private void ProfileExEntity_OnOrderCancelled(int[] aInternalID)
        {
            throw new NotImplementedException();
        }

        private void ProfileExEntity_OnOrderCompleted(int[] aInternalID)
        {
            throw new NotImplementedException();
        }

        public ExchangeMarketViewModel[] GetMarketsForSelectedCoin(CurrencyItem aSelectedCurrencyItem, int aProfileID)
        {
            List<ExchangeMarketViewModel> lExchangeMarketViewModels = new List<ExchangeMarketViewModel>();
            var lProfileExEntity = FProfileManager.GetProfileExchangeEntity(aProfileID);
            var lMarkets = lProfileExEntity.GetExchangeMarkets(aSelectedCurrencyItem.Ticker);
            lExchangeMarketViewModels.AddRange(lMarkets.Select(lMarket => new ExchangeMarketViewModel(lMarket)));
            return lExchangeMarketViewModels.ToArray();
        }

        public void TryMakeNewExchangeTradingOrder(int aProfilleID, ExchangeMarket2 aMarket, decimal aPriceTarget, decimal aStopPrice, decimal aAmount, string aName = null)
        {
            string lTempGUID = Guid.NewGuid().ToString("N");
            MarketOrder lNewOrder = new MarketOrder
            {
                CoinTxID = lTempGUID,
                SentQuantity = aAmount,
                Market = aMarket.MarketName,
                Rate = aPriceTarget,
                StopPrice = aStopPrice,
                Status = OrderStatus.Initial,
                BaseTicker = aMarket.BaseTicker,
                OpenTime = DateTime.UtcNow,
                Name = string.IsNullOrEmpty(aName) ? $"{aMarket.MarketName}: {DateTime.Now}" : aName,
                MarketInstance = aMarket,
                ProfileID = aProfilleID
            };
            FOrderManager.AddNewOrder(ref lNewOrder);
            FOrderManager.WriteMarketOrderLogEntry(lNewOrder);
        }

        /// <summary>
        /// Performs a scan of all orders and executes actions as needed
        /// </summary>
        private void LocalOrderScanTask()
        {
            try
            {
                if (FCurrencyTracked.Any())
                {
                    foreach (var lCurrencyIDTuple in FCurrencyTracked)
                    {
                        if (!FOrderManager.GetTransactionsByTicker(lCurrencyIDTuple.Item2, out MarketOrder[] lOrders))
                            continue;
                        List<MarketOrder> lOrdersInitial = lOrders.Where(lOrder => lOrder.Status == OrderStatus.Initial).ToList();
                        VerifyInitialOrders(lOrdersInitial, lCurrencyIDTuple);
                        List<MarketOrder> lOrderWaiting = lOrders.Where(lOrder => lOrder.Status == OrderStatus.Waiting).ToList();
                        VerifyWaitingOrders(lOrderWaiting);
                    }
                }
            }
            catch (Exception ex)
            {
                Universal.Log.Write(Universal.LogLevel.Error, string.Format("Failed to Update Exchange Order. Details: {0}", ex));
            }
            finally
            {
                Task.Delay(1000).Wait(FExchangeTaskCancellationSource.Token);
                Interlocked.Exchange(ref FLocalScanCore, Task.Run(() => LocalOrderScanTask(), FExchangeTaskCancellationSource.Token));
            }
        }

        /// <summary>
        /// Checks order that are in initial state, and changes it to waiting state if is needed (Here the stop price is verified)
        /// </summary>
        /// <param name="aInitialOrders">Orders with initial state</param>
        /// <param name="aListExchangeCoinMarket">Markets to look fo</param>
        /// <param name="aTicker">Order currency ticker</param>
        /// <param name="aCurrencyID">Order currency id</param>
        private void VerifyInitialOrders(List<MarketOrder> aInitialOrders, Tuple<long, string> aCurrencyIDTuple)
        {
            if (!aInitialOrders.Any())
                return;

            foreach (MarketOrder lOrder in aInitialOrders)
            {
                decimal lTargetPrice = lOrder.Rate;
                bool lPlaceOrderFlag = false;
                var lMarket = lOrder.MarketInstance;
                if (lMarket.IsSell)
                    lPlaceOrderFlag = lOrder.StopPrice <= lMarket.BasePrice;
                else
                    lPlaceOrderFlag = lOrder.StopPrice >= lMarket.BasePrice;

                if (lPlaceOrderFlag)
                {
                    FOrderManager.WriteMarketOrderLogEntry(lOrder, OrderMessage.OrderMessageLevel.Info, "Stop price reached, trying to send coins to exchange.");
                    TryToTransferMoneyToExchange(lOrder, lMarket, aCurrencyIDTuple);
                }
            }
        }

        /// <summary>
        /// Tries to form a transaction that meets the order requeriments and then sends it to the exchange user address. Also does amount verification
        /// </summary>
        /// <param name="aOrder">Order to fulfill</param>
        /// <param name="aMarket">Order's market</param>
        /// <param name="aCurrencyID">Selected currency id</param>
        private void TryToTransferMoneyToExchange(MarketOrder aOrder, ExchangeMarket2 aMarket, Tuple<long, string> aCurrencyIDTuple)
        {
            try
            {
                var lExchangeEntity = FProfileManager.GetProfileExchangeEntity(aOrder.ProfileID);
                string lExchangeAddress = lExchangeEntity.GetDepositAddress(aCurrencyIDTuple.Item2);
                decimal lAmount = aOrder.SentQuantity;
                string lTxID = OnSendCoinToTx.Invoke(lAmount, aCurrencyIDTuple.Item1, lExchangeAddress);
                lock (FOrdersCoinTxs)
                    FOrdersCoinTxs.Add(aOrder.InternalID, new OrderCoinTx
                    {
                        CurrencyID = aCurrencyIDTuple.Item1,
                        Confirmed = false,
                        TxID = lTxID
                    });
                aOrder.CoinTxID = lTxID;
                FOrderManager.UpdateOrderStatus(aOrder.InternalID, OrderStatus.Waiting);
                FOrderManager.WriteMarketOrderLogEntry(aOrder);
                FOrderManager.WriteMarketOrderLogEntry(aOrder, OrderMessage.OrderMessageLevel.Info, string.Format("Number of confirmations needed: {0} confirmations", lExchangeEntity.GetMinimumConfirmations(aMarket.BaseTicker)));
            }
            catch (Exception ex)
            {
                FOrderManager.UpdateOrderStatus(aOrder.InternalID, OrderStatus.Interrupted);
                FOrderManager.WriteMarketOrderLogEntry(aOrder, OrderMessage.OrderMessageLevel.FatalError, string.Format("Failed to send transaction. Details: {0}.", ex.Message));
            }
        }

        /// <summary>
        /// Checks order that are in waiting state, and changes it to placed state if is needed (Here transaction confirmations are verified))
        /// </summary>
        /// <param name="aWaitingOrders">Orders with waiting state</param>
        /// <param name="aCoinTicker">Order currency ticker</param>
        /// <param name="aCurrencyTransactions">Pandora's currency transactions</param>
        /// <param name="aListExchangeCoinMarket">Markets to look for</param>
        private void VerifyWaitingOrders(List<MarketOrder> aWaitingOrders)
        {
            if (!aWaitingOrders.Any())
                return;

            Dictionary<int, OrderCoinTx> lCoinTxs;
            lock (FOrdersCoinTxs)
                lCoinTxs = new Dictionary<int, OrderCoinTx>(FOrdersCoinTxs);

            foreach (var lOrder in aWaitingOrders)
            {
                if (lCoinTxs.TryGetValue(lOrder.InternalID, out OrderCoinTx lOrderCoinTx) && lOrderCoinTx.Confirmed && lOrder.Status == OrderStatus.Waiting)
                    TryToPlaceOrder(lOrder);
            }
        }

        private void TryToPlaceOrder(MarketOrder aOrder)
        {
            if (aOrder.ErrorCounter % 60 == 0)
            {
                try
                {
                    FOrderManager.WriteMarketOrderLogEntry(aOrder, OrderMessage.OrderMessageLevel.Info, "Attempting to place order in exchange.");
                    var lEntity = FProfileManager.GetProfileExchangeEntity(aOrder.ProfileID);
                    aOrder.ID = lEntity.PlaceOrder(aOrder);
                    FOrderManager.UpdateOrderStatus(aOrder.InternalID, OrderStatus.Placed);
                    FOrderManager.WriteMarketOrderLogEntry(aOrder);
                    aOrder.ErrorCounter = 0;
                }
                catch (Exception ex)
                {
                    int lNumberofretrys = aOrder.ErrorCounter / 60;

                    if (lNumberofretrys >= 9)
                    {
                        aOrder.Cancelled = true;
                        FOrderManager.UpdateOrderStatus(aOrder.InternalID, OrderStatus.Interrupted);
                        FOrderManager.WriteMarketOrderLogEntry(aOrder);
                    }

                    aOrder.ErrorCounter += 1;
                    Universal.Log.Write(Universal.LogLevel.Error, string.Format("Order: {0}. Exception: {1}", aOrder.InternalID, ex.ToString()));
                    FOrderManager.WriteMarketOrderLogEntry(aOrder, OrderMessage.OrderMessageLevel.Error, "Error while placing Order: " + ex.Message);
                    FOrderManager.WriteMarketOrderLogEntry(aOrder, OrderMessage.OrderMessageLevel.Info, string.Format("Retrying in 1 minute. Attempt: {0}/10", (lNumberofretrys + 1)));
                }
            }
            else
            {
                aOrder.ErrorCounter++;
            }
        }

        private class OrderCoinTx
        {
            public string TxID { get; set; }
            public long CurrencyID { get; set; }
            public bool Confirmed { get; set; }
        }
    }

    public class ExchangeMarketViewModel
    {
        public ExchangeMarketViewModel(ExchangeMarket2 aMarket)
        {
            ParentMarket = aMarket;
        }

        public ExchangeMarket2 ParentMarket { get; private set; }
        public string Name { get => ParentMarket.CoinName; }
        public decimal Price { get => ParentMarket.BasePrice; }
    }
}