using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandora.Client.Exchange.Factories;
using Pandora.Client.Exchange.SaveManagers;

namespace Pandora.Client.Exchange
{
    public class PandoraExchangeOrderManager : ISaveManagerConfigurable
    {
        private static PandoraExchangeOrderManager FInstance;
        private IPandoraSaveManager FSaveManager;
        private Dictionary<string, List<MarketOrder>> FLoadedOrders;

        private PandoraExchangeOrderManager()
        {
            FLoadedOrders = new Dictionary<string, List<MarketOrder>>();
            var lSaveManagerFactory = PandoraSaveManagerFactory.GetSaveMangerFactory();
            FSaveManager = lSaveManagerFactory.GetNewPandoraSaveManager(SavePlace.SQLiteDisk);
        }

        public static PandoraExchangeOrderManager GetOrderManager()
        {
            if (FInstance == null)
                FInstance = new PandoraExchangeOrderManager();
            return FInstance;
        }

        public bool ConfigureSaveLocation(bool aForce = false, params string[] aSaveInitializingParams)
        {
            if (FSaveManager.Initialized && !aForce)
                return false;
            FSaveManager.Initialize(aSaveInitializingParams);
            return true;
        }

        public bool GetTransactionsByTicker(string aTicker, out MarketOrder[] aOrders)
        {
            bool lResult;
            lock (FLoadedOrders)
            {
                if (FLoadedOrders.TryGetValue(aTicker, out List<MarketOrder> lOrders))
                {
                    aOrders = lOrders.ToArray();
                    lResult = true;
                }
                else
                {
                    aOrders = null;
                    lResult = false;
                }
            }
            return lResult;
        }

        /// <summary>
        /// Loads trasactions with specific ticker from db. WARNING: This will replace memory trasactions for selected ticker.
        /// </summary>
        /// <param name="aTicker">Coin Ticker to work with</param>
        /// <returns>Market Order without MarketInstance property assigned</returns>
        public MarketOrder[] LoadTransactions(string aTicker)
        {
            if (!FSaveManager.Initialized)
                throw new Exception("Before execuging statement you need to configure save location");

            if (!FSaveManager.ReadTransactions(out MarketOrder[] lTransactions, aTicker))
                throw new Exception("Failed to read exchange transactions from disk");

            if (lTransactions != null && lTransactions.Any())
            {
                foreach (MarketOrder it in lTransactions)
                    it.ErrorCounter = GetOrderErrorCounter(it);

                lock (FLoadedOrders)
                {
                    FLoadedOrders.Remove(aTicker);
                    FLoadedOrders.Add(aTicker, lTransactions.ToList());
                }
            }
            return lTransactions;
        }

        private int GetOrderErrorCounter(MarketOrder aOrder)
        {
            int lErrorCounter;
            bool lOrdersLogs = FSaveManager.ReadOrderLogs(aOrder.InternalID, out List<OrderMessage> lMessages);
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
            return lErrorCounter;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="aInternalId"></param>
        /// <param name="aNewStatus"></param>
        /// <returns>Reference to order with status changed</returns>
        public MarketOrder UpdateOrderStatus(int aInternalId, OrderStatus aNewStatus)
        {
            if (!FSaveManager.Initialized)
                throw new Exception("Before execuging statement you need to configure save location");
            MarketOrder lOrder;
            lock (FLoadedOrders)
            {
                lOrder = FLoadedOrders
                    .Select(lOrderPair => lOrderPair.Value
                                                    .Where(lOrderEle => lOrderEle.InternalID == aInternalId)
                                                    .FirstOrDefault())
                    .Where(lList => lList != null)
                    .FirstOrDefault();
            }
            if (lOrder == null)
                throw new Exception($"Update status - Order id not found. ID: {aInternalId}");
            if (FSaveManager.UpdateTransaction(lOrder, aNewStatus))
                lOrder.Status = aNewStatus;
            return lOrder;
        }

        /// <summary>
        /// Adds a new order to the manager
        /// </summary>
        /// <param name="aNewOrder"></param>
        /// <returns>InternalID generated when order was saved</returns>
        public int AddNewOrder(ref MarketOrder aNewOrder)
        {
            if (!FSaveManager.Initialized)
                throw new Exception("Before execuging statement you need to configure save location");
            string lTemporalIdentifier = aNewOrder.CoinTxID;
            string LOrderCoinTicker = aNewOrder.BaseTicker;
            if (string.IsNullOrEmpty(aNewOrder.CoinTxID))
                throw new Exception("You must assign a temporal guid identifier to CoinTxID paramether before adding it to manager");
            FSaveManager.WriteTransaction(aNewOrder);
            FSaveManager.ReadTransactions(out MarketOrder[] lOrders, LOrderCoinTicker);
            MarketOrder lOrderWithID = lOrders.Where(x => x.CoinTxID == lTemporalIdentifier).FirstOrDefault();
            if (lOrderWithID == null)
                throw new Exception($"Failed to write transaction in disk. Not Found. Temporal GUID: {lTemporalIdentifier}");
            aNewOrder.InternalID = lOrderWithID.InternalID;
            lock (FLoadedOrders)
            {
                if (FLoadedOrders.TryGetValue(LOrderCoinTicker, out List<MarketOrder> lOrderList))
                    lOrderList.Add(lOrderWithID);
                else
                    FLoadedOrders.Add(LOrderCoinTicker, new List<MarketOrder>() { lOrderWithID });
            }
            return lOrderWithID.InternalID;
        }

        public void WriteMarketOrderLogEntry(MarketOrder aOrder, OrderMessage.OrderMessageLevel aLevel = OrderMessage.OrderMessageLevel.Info, string aMessage = null)
        {
            if (!FSaveManager.Initialized)
                throw new Exception("Before execuging statement you need to configure save location");
            var lLocalOrder = aOrder.Clone();
            if (!string.IsNullOrEmpty(aMessage))
            {
                FSaveManager.WriteOrderLog(lLocalOrder.InternalID, aMessage, aLevel);
                return;
            }

            switch (aOrder.Status)
            {
                case OrderStatus.Initial:
                    FSaveManager.WriteOrderLog(lLocalOrder.InternalID, "Starting transaction process.", OrderMessage.OrderMessageLevel.Info);
                    FSaveManager.WriteOrderLog(lLocalOrder.InternalID, "Waiting for market stop price to place order.", OrderMessage.OrderMessageLevel.StageChange);
                    break;

                case OrderStatus.Waiting:
                    FSaveManager.WriteOrderLog(lLocalOrder.InternalID, "Price reached specified stop price.", OrderMessage.OrderMessageLevel.Info);
                    FSaveManager.WriteOrderLog(lLocalOrder.InternalID, $"{lLocalOrder.SentQuantity} coins sent to exchange account. Tx ID: {lLocalOrder.CoinTxID}.", OrderMessage.OrderMessageLevel.Info);
                    FSaveManager.WriteOrderLog(lLocalOrder.InternalID, "Waiting for confirmations to place the order.", OrderMessage.OrderMessageLevel.StageChange);
                    break;

                case OrderStatus.Placed:
                    FSaveManager.WriteOrderLog(lLocalOrder.InternalID, $"Transaction id {lLocalOrder.CoinTxID} has enough confirmations to place the order.", OrderMessage.OrderMessageLevel.Info);
                    FSaveManager.WriteOrderLog(lLocalOrder.InternalID, $"Placed order in exchange. Uuid: {lLocalOrder.ID}. Waiting for order to fulfill.", OrderMessage.OrderMessageLevel.StageChange);
                    break;

                case OrderStatus.Interrupted:
                    FSaveManager.WriteOrderLog(lLocalOrder.InternalID, "Transaction cancelled or not found on the exchange.", OrderMessage.OrderMessageLevel.FatalError);
                    break;

                case OrderStatus.Completed:
                    FSaveManager.WriteOrderLog(lLocalOrder.InternalID, "Transaction completed. Waiting for withdraw.", OrderMessage.OrderMessageLevel.StageChange);
                    break;

                case OrderStatus.Withdrawed:
                    FSaveManager.WriteOrderLog(lLocalOrder.InternalID, "Cryptocurrencies succesfully withdrawn to Pandora's Wallet.", OrderMessage.OrderMessageLevel.Finisher);
                    break;
            }
        }

        public OrderMessage[] ReadMarketorderLog(int aInternalID)
        {
            if (!FSaveManager.Initialized)
                throw new Exception("Before execuging statement you need to configure save location");
            if (!FSaveManager.ReadOrderLogs(aInternalID, out List<OrderMessage> lMessages))
                throw new Exception("Failed to retrieve market order logs");
            return lMessages.OrderByDescending(lOrder => lOrder.Time).ToArray();
        }
    }
}