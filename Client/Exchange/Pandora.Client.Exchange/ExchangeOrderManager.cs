using Pandora.Client.Exchange.Objects;
using Pandora.Client.Exchange.SaveManagers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange
{
    public class ExchangeOrderManager :IDisposable
    {
        private IPandoraSaveManager FSaveManager;

        private ConcurrentDictionary<int,UserTradeOrder> FOrders;
        private ConcurrentDictionary<int, ConcurrentBag<UserTradeOrder>> FOrdersByExchange;

        public event Action<int, OrderStatus, long> OnOrderStatusChanged;
        public event Action<int, IEnumerable<OrderMessage>> OnNewOrderLogsAdded;

        public ExchangeOrderManager(IPandoraSaveManager aSaveManager)
        {
            FOrders = new ConcurrentDictionary<int, UserTradeOrder>();
            FOrdersByExchange = new ConcurrentDictionary<int, ConcurrentBag<UserTradeOrder>>();
            FSaveManager = aSaveManager;
        }

        public UserTradeOrder SaveNewOrder(UserTradeOrder aNewOrder, long aCurrencyID)
        {
            var lInternalID = FSaveManager.WriteOrder(aNewOrder);
            FSaveManager.ReadOrders(out UserTradeOrder[] lOrders, aNewOrder.BaseCurrency.Ticker, aNewOrder.PandoraExchangeID);
            UserTradeOrder lOrderWithID = lOrders.Where(lOrder => lOrder.InternalID == lInternalID).FirstOrDefault();
            if (lOrderWithID == null)
                throw new Exception("Failed to write new order in disk");
            lOrderWithID.BaseCurrency.ID = aCurrencyID;
            SetOrderInCache(lOrderWithID);
            return lOrderWithID;
        }

        public UserTradeOrder GetOrderByInternalID(int aInternalOrderId)
        {
            UserTradeOrder lOrder;
            FOrders.TryGetValue(aInternalOrderId, out lOrder);            
            return lOrder;
        }

        public IEnumerable<UserTradeOrder> GetOrdersByCurrencyID(long aCurrencyId)
        {
            return FOrders.Values.Where(lOrder => lOrder.BaseCurrency.ID == aCurrencyId);
        }
        /// <summary>
        /// Loads currency exchange orders from disk. This should only be called once or to reload orders.
        /// </summary>
        /// <param name="aCurrencyTicker"></param>
        /// <param name="aCurrencyID"></param>
        /// <returns></returns>
        public UserTradeOrder[] LoadOrders(string aCurrencyTicker, long aCurrencyID)
        {
            if (!FSaveManager.ReadOrders(out UserTradeOrder[] lOrders, aCurrencyTicker))
                throw new Exception("Failed to read exchange transactions from disk");

            if (lOrders != null && lOrders.Any())
            {
                int lErrorCounter;
                foreach (UserTradeOrder it in lOrders)
                {
                    
                    bool lOrdersLogs = FSaveManager.ReadOrderLogs(it.InternalID, out List<OrderMessage> lMessages);
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
                foreach (var lOrder in lOrders)
                {
                    lOrder.BaseCurrency.ID = aCurrencyID;
                    SetOrderInCache(lOrder);
                }
            }
            return lOrders;
        }

        private void SetOrderInCache(UserTradeOrder aOrder)
        {
            FOrders.AddOrUpdate(aOrder.InternalID, aOrder, (key, oldvalue) => aOrder);
            if (!FOrdersByExchange.TryGetValue(aOrder.PandoraExchangeID, out ConcurrentBag<UserTradeOrder> lIds))
            {
                lIds = new ConcurrentBag<UserTradeOrder>();
                FOrdersByExchange.TryAdd(aOrder.PandoraExchangeID, lIds);
            }
            lIds.Add(aOrder);

        }

        public IEnumerable<UserTradeOrder> GetOrdersByExchangeID(int aExchangeID)
        {
            List<UserTradeOrder> lResult = new List<UserTradeOrder>();
            if (FOrdersByExchange.TryGetValue(aExchangeID, out ConcurrentBag<UserTradeOrder> lOrders))
                lResult.AddRange(lOrders);
            return lResult;
        }

        public void UpdateOrder(UserTradeOrder aNewOrder, OrderStatus aStatus)
        {
            FOrders.AddOrUpdate(aNewOrder.InternalID, aNewOrder, (key,lOldValue) => aNewOrder);
            if (!FSaveManager.UpdateOrder(aNewOrder, aStatus))
                throw new Exception("Failed to write new transaction into disk");             
            aNewOrder.Status = aStatus;
            OnOrderStatusChanged?.BeginInvoke(aNewOrder.InternalID, aStatus, aNewOrder.BaseCurrency.ID, null,null);
        }

        public void WriteTransactionLogEntry(UserTradeOrder aOrder, OrderMessage.OrderMessageLevel aLevel = OrderMessage.OrderMessageLevel.Info, string aMessage = null)
        {
            if (string.IsNullOrEmpty(aMessage))
            {
                switch (aOrder.Status)
                {
                    case OrderStatus.Initial:
                        FSaveManager.WriteOrderLog(aOrder.InternalID, "Starting transaction process.", OrderMessage.OrderMessageLevel.Info);
                        FSaveManager.WriteOrderLog(aOrder.InternalID, "Waiting for market stop price to place order.", OrderMessage.OrderMessageLevel.StageChange);
                        break;

                    case OrderStatus.Waiting:
                        FSaveManager.WriteOrderLog(aOrder.InternalID, "Price reached specified stop price.", OrderMessage.OrderMessageLevel.Info);
                        FSaveManager.WriteOrderLog(aOrder.InternalID, string.Format("{0} coins sent to exchange account. Tx ID: {1}.", aOrder.SentQuantity, aOrder.CoinTxID), OrderMessage.OrderMessageLevel.Info);
                        FSaveManager.WriteOrderLog(aOrder.InternalID, "Waiting for confirmations to place the order.", OrderMessage.OrderMessageLevel.StageChange);
                        break;

                    case OrderStatus.Placed:
                        FSaveManager.WriteOrderLog(aOrder.InternalID, string.Format("Transaction id {0} has enough confirmations to place the order.", aOrder.CoinTxID), OrderMessage.OrderMessageLevel.Info);
                        FSaveManager.WriteOrderLog(aOrder.InternalID, string.Format("Placed order in exchange. Uuid: {0}. Waiting for order to fulfill.", aOrder.ID.ToString()), OrderMessage.OrderMessageLevel.StageChange);
                        break;

                    case OrderStatus.Interrupted:
                        FSaveManager.WriteOrderLog(aOrder.InternalID, "Transaction cancelled or not found on the exchange.", OrderMessage.OrderMessageLevel.FatalError);
                        break;

                    case OrderStatus.Completed:
                        FSaveManager.WriteOrderLog(aOrder.InternalID, "Transaction completed. Waiting for withdrawal.", OrderMessage.OrderMessageLevel.StageChange);
                        break;

                    case OrderStatus.Withdrawn:
                        FSaveManager.WriteOrderLog(aOrder.InternalID, "Cryptocurrencies succesfully withdrawn to Pandora's Wallet.", OrderMessage.OrderMessageLevel.Finisher);
                        break;
                }
            }
            else FSaveManager.WriteOrderLog(aOrder.InternalID, aMessage, aLevel);
            if (FSaveManager.ReadOrderLogs(aOrder.InternalID, out List<OrderMessage> lOrderMessages))
                OnNewOrderLogsAdded?.BeginInvoke(aOrder.InternalID, lOrderMessages, null, null);
        }

        public void Dispose()
        {
            FOrders.Clear();
            FOrdersByExchange.Clear();
            OnNewOrderLogsAdded = null;
            OnOrderStatusChanged = null;
            FSaveManager = null;
        }
    }
}
