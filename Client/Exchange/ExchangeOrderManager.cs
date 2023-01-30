using Pandora.Client.ClientLib.Contracts;
using Pandora.Client.Exchange.Contracts;
using Pandora.Client.Exchange.Exchangers;
using Pandora.Client.Exchange.Exchangers.Contracts;
using Pandora.Client.Exchange.Models;
using Pandora.Client.Exchange.SaveManagers;
using Pandora.Client.Exchangers.Contracts;
using Pandora.Client.Universal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange
{
    public class ExchangeOrderManager : IDisposable
    {
        private IPandoraSaveManager FSaveManager;

        private ConcurrentDictionary<int, UserTradeOrder> FOrders;
        private ConcurrentDictionary<int, ConcurrentBag<UserTradeOrder>> FOrdersByExchange;

        public event Action<int, OrderStatus, long> OnOrderStatusChanged;

        public event Action<int, IEnumerable<OrderMessage>> OnNewOrderLogsAdded;

        public ExchangeOrderManager(IPandoraSaveManager aSaveManager)
        {
            FOrders = new ConcurrentDictionary<int, UserTradeOrder>();
            FOrdersByExchange = new ConcurrentDictionary<int, ConcurrentBag<UserTradeOrder>>();
            FSaveManager = aSaveManager;
        }

        public UserTradeOrder SaveNewOrder(UserTradeOrder aNewOrder)
        {
            var lInternalID = FSaveManager.WriteOrder(aNewOrder);
            if (!lInternalID.HasValue)
                throw new Exception("Failed to write new order in disk");
            aNewOrder.InternalID = lInternalID.Value;
            SetOrderInCache(aNewOrder);
            return aNewOrder;
        }

        public UserTradeOrder GetOrderByInternalID(int aInternalOrderId)
        {
            UserTradeOrder lOrder;
            FOrders.TryGetValue(aInternalOrderId, out lOrder);
            return lOrder;
        }

        public IEnumerable<UserTradeOrder> GetOrdersByCurrencyID(long aCurrencyId)
        {
            return FOrders.Values.Where(lOrder => lOrder.Market.SellingCurrencyInfo.Id == aCurrencyId);
        }

        /// <summary>
        /// Loads currency exchange orders from disk. This should only be called once or to reload orders.
        /// </summary>
        /// <param name="aCurrencyTicker"></param>
        /// <param name="aCurrencyID"></param>
        /// <returns></returns>
        public IEnumerable<UserTradeOrder> LoadOrders(ICurrencyIdentity aCurrency, IPandoraExchangeFactory lExchangeFactory, GetWalletIDDelegate aGetWalletIDFunction)
        {
            var lResult = new List<UserTradeOrder>();
            var lDBOrders = FSaveManager.ReadOrders(aCurrency);
            if (lDBOrders != null && lDBOrders.Any())
            {
                int lErrorCounter;
                var lMarketCache = new Dictionary<int, IEnumerable<IExchangeMarket>>();
                foreach (var lDBOrder in lDBOrders)
                {
                    bool lOrdersLogs = FSaveManager.ReadOrderLogs(lDBOrder.InternalID, out List<OrderMessage> lMessages);
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

                    var lExchanger = lExchangeFactory.GetPandoraExchange((AvailableExchangesList)lDBOrder.ExchangeID);
                    if (!lMarketCache.TryGetValue(lDBOrder.ExchangeID, out IEnumerable<IExchangeMarket> lMarkets))
                    {
                        lMarkets = lExchanger.GetMarketCoins(aCurrency, aGetWalletIDFunction);
                        lMarketCache.Add(lDBOrder.ExchangeID, lMarkets);
                    }
                    var lUserOrder = new UserTradeOrder
                    {
                        InternalID = lDBOrder.InternalID,
                        ID = lDBOrder.ID,
                        Name = lDBOrder.Name,
                        SentQuantity = lDBOrder.SentQuantity,
                        Status = lDBOrder.Status,
                        CoinTxID = lDBOrder.CoinTxID,
                        StopPrice = lDBOrder.StopPrice,
                        OpenTime = lDBOrder.OpenTime,
                        Rate = lDBOrder.Rate,
                        ProfileID = lDBOrder.ProfileID,
                        ErrorCounter = lErrorCounter,
                        Completed = lDBOrder.Completed,
                        Cancelled = lDBOrder.Cancelled,
                        Market = lMarkets.FindByMarketID(lDBOrder.MarketID)
                    };
                    if (lUserOrder.Market == null)
                    {
                        Log.Write(LogLevel.Warning, $"Exchange: Unable to find market for order of id {lUserOrder.InternalID} of exchange {lExchanger.Name}");
                        continue;
                    }
                    lResult.Add(lUserOrder);
                    SetOrderInCache(lUserOrder, lDBOrder.ExchangeID);
                }
            }
            return lResult;
        }

        private void SetOrderInCache(UserTradeOrder aOrder, int aExchangeID = -1)
        {
            var lExchangeId = aOrder.Market?.ExchangeID ?? aExchangeID;
            if (lExchangeId < 0)
                return;
            FOrders.AddOrUpdate(aOrder.InternalID, aOrder, (key, oldvalue) => aOrder);
            if (!FOrdersByExchange.TryGetValue(lExchangeId, out ConcurrentBag<UserTradeOrder> lIds))
            {
                lIds = new ConcurrentBag<UserTradeOrder>();
                FOrdersByExchange.TryAdd(lExchangeId, lIds);
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
            if (aNewOrder.Market == null) return;
            FOrders.AddOrUpdate(aNewOrder.InternalID, aNewOrder, (key, lOldValue) => aNewOrder);
            if (!FSaveManager.UpdateOrder(aNewOrder, aStatus))
                throw new Exception("Failed to write new transaction into disk");
            aNewOrder.Status = aStatus;
            OnOrderStatusChanged?.BeginInvoke(aNewOrder.InternalID, aStatus, aNewOrder.Market.SellingCurrencyInfo.Id, null, null);
        }

        public void WriteTransactionLogEntry(UserTradeOrder aOrder, OrderMessage.OrderMessageLevel aLevel = OrderMessage.OrderMessageLevel.Info, string aMessage = null)
        {
            if (string.IsNullOrEmpty(aMessage))
            {
                switch (aOrder.Status)
                {
                    case OrderStatus.Initial:
                        FSaveManager.WriteOrderLog(aOrder.InternalID, $"Starting exchange order {aOrder.Name}.", OrderMessage.OrderMessageLevel.Info);
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