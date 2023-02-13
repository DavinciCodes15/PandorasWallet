using Pandora.Client.ClientLib;
using Pandora.Client.ClientLib.Contracts;
using Pandora.Client.Exchange;
using Pandora.Client.Exchange.Contracts;
using Pandora.Client.Exchange.Exchangers.Contracts;
using Pandora.Client.Exchange.Models;
using Pandora.Client.PandorasWallet.ServerAccess;
using Pandora.Client.Universal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.Client.PandorasWallet
{
    public class ExchangeStageOperator
    {
        private IPandoraExchanger FExchanger;
        private ExchangeOrderManager FExchangeOrderManager;
        private ServerConnection FPandorasWalletConnection;
        private CancellationTokenSource FExchangeTaskCancellationSource;
        private Task[] FExchangeTasks;
        private static ConcurrentDictionary<int, bool> FCancelledOrderList = new ConcurrentDictionary<int, bool>();
        private ConcurrentDictionary<int, UserTradeOrder> FWaitingToRefund = new ConcurrentDictionary<int, UserTradeOrder>();

        public event Func<decimal, string, long, decimal?, string> OnTransferCoinsNeeded;

        public string ExchangerName => FExchanger.Name;

        public ExchangeStageOperator(IPandoraExchanger aExchanger, ExchangeOrderManager aOrderManager, ServerAccess.ServerConnection aPandorasWalletConnection)
        {
            FExchanger = aExchanger;
            FExchangeOrderManager = aOrderManager;
            FPandorasWalletConnection = aPandorasWalletConnection;
        }

        public static void MarkOrderToCancel(int aOrderID, bool aWithdrawIsNeeded)
        {
            FCancelledOrderList.TryAdd(aOrderID, aWithdrawIsNeeded);
        }

        public void Start()
        {
            FExchangeTaskCancellationSource = new CancellationTokenSource();
            FExchangeTasks = new Task[]
            {
                Task.Run(() => StopPriceOrderScanTask(), FExchangeTaskCancellationSource.Token),
                Task.Run(() => WaitingOrderScanTask(), FExchangeTaskCancellationSource.Token),
                Task.Run(() => RemoteOrderScanTask(), FExchangeTaskCancellationSource.Token)
            };
            Task.Run(() => TryStartMarketPriceUpdating(), FExchangeTaskCancellationSource.Token);
        }

        private void DoLocalScanBasicOperation(Action<IEnumerable<UserTradeOrder>, ICurrencyIdentity> aVerifyOperation, OrderStatus aStatus)
        {
            if (FPandorasWalletConnection != null && FExchanger.IsCredentialsSet)
            {
                var lExchangeOrders = FExchangeOrderManager.GetOrdersByExchangeID(FExchanger.ID);
                foreach (var lCoin in GetWalletCurrencies())
                {
                    var lCurrencyOrders = lExchangeOrders.Where(lOrder => string.Equals(lOrder.Market.SellingCurrencyInfo.Ticker, lCoin.Ticker, StringComparison.OrdinalIgnoreCase));
                    if (lCurrencyOrders.Any())
                    {
                        var lOrders = lCurrencyOrders.Where(lOrder => lOrder.Status == aStatus);
                        aVerifyOperation.Invoke(lOrders, lCoin);
                    }
                }
            }
        }

        private void StopPriceOrderScanTask()
        {
            try
            {
                DoLocalScanBasicOperation(VerifyInitialOrders, OrderStatus.Initial);
            }
            catch (Exception ex)
            {
                Universal.Log.Write(Universal.LogLevel.Error, string.Format("Failed to Update Exchange Order. Details: {0}", ex));
            }
            finally
            {
                if (!MyDelayCanceled(5, FExchangeTaskCancellationSource.Token))
                    FExchangeTasks[0] = Task.Run(() => StopPriceOrderScanTask(), FExchangeTaskCancellationSource.Token);
            }
        }

        /// <summary>
        /// Performs a scan of all orders Waiting orders and checks if the transaction is already confirmed
        /// </summary>
        private void WaitingOrderScanTask()
        {
            try
            {
                DoLocalScanBasicOperation((aOrders, aCurrencyItem) =>
                {
                    if (aOrders.Any() || FWaitingToRefund.Any())
                    {
                        var lCurrencyTxs = FPandorasWalletConnection.GetTransactionRecords(aCurrencyItem.Id);
                        ProcessOrdersMarkedToRefund(lCurrencyTxs, aCurrencyItem);
                        VerifyWaitingOrders(aOrders, aCurrencyItem, lCurrencyTxs);
                    }
                }, OrderStatus.Waiting);
            }
            catch (Exception ex)
            {
                Universal.Log.Write(Universal.LogLevel.Error, string.Format("Failed to Update Exchange Order. Details: {0}", ex));
            }
            finally
            {
                if (!MyDelayCanceled(10, FExchangeTaskCancellationSource.Token))
                    FExchangeTasks[1] = Task.Run(() => WaitingOrderScanTask(), FExchangeTaskCancellationSource.Token);
            }
        }

        private void TryStartMarketPriceUpdating()
        {
            try
            {
                FExchanger.StartMarketPriceUpdating();
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Error, $"Failed to start market price updating for {ExchangerName}. Exception thrown: {ex}");
                Task.Run(async () =>
                {
                    await Task.Delay(60000, FExchangeTaskCancellationSource.Token);
                    TryStartMarketPriceUpdating();
                }, FExchangeTaskCancellationSource.Token);
            }
        }

        public void Stop()
        {
            try
            {
                FExchangeTaskCancellationSource?.Cancel();
                if (FExchangeTasks != null)
                {
                    var lTasks = FExchangeTasks.Where(x => x != null);
                    Task.WaitAll(lTasks.ToArray());
                }
            }
            catch (OperationCanceledException e)
            {
                Log.Write(LogLevel.Info, e.Message);
            }
            catch (AggregateException e)
            {
                Log.Write(LogLevel.Info, e.Message);
            }
            try
            {
                FExchanger.StopMarketUpdating();
            }
            catch (Exception ex)
            {
                Log.Write($"Exception thrown at stop market price updating at operator '{ExchangerName}'. Details: {ex}");
            }
            FCancelledOrderList.Clear();
            FWaitingToRefund.Clear();
        }

        private void RemoteOrderScanTask()
        {
            try
            {
                if (FPandorasWalletConnection != null && FExchanger.IsCredentialsSet)
                {
                    var lExchangeOrders = FExchangeOrderManager.GetOrdersByExchangeID(FExchanger.ID);
                    var lPlacedOrders = lExchangeOrders.Where(x => x.Status == OrderStatus.Placed);
                    VerifyPlacedOrders(lPlacedOrders);
                    var lCompletedOrders = lExchangeOrders.Where(x => x.Status == OrderStatus.Completed);
                    VerifyCompletedOrders(lCompletedOrders);
                }
            }
            catch (Exception ex)
            {
                Log.Write("Error on Checking order statuses. Details: " + ex.Message + " on " + ex.Source);
            }
            finally
            {
                if (!MyDelayCanceled(RandomNumber(5, 7), FExchangeTaskCancellationSource.Token))
                    FExchangeTasks[1] = Task.Run(() => RemoteOrderScanTask(), FExchangeTaskCancellationSource.Token);
            }
        }

        private int RandomNumber(int aMin, int aMax)
        {
            Random random = new Random();
            return random.Next(aMin, aMax);
        }

        private void VerifyPlacedOrders(IEnumerable<UserTradeOrder> aOrders)
        {
            ScanForCancelledOrders(aOrders);
            foreach (var lOrder in aOrders)
            {
                var lRemoteOrder = FExchanger.GetOrderStatus(lOrder.ID);
                if (!lRemoteOrder.Completed && !lRemoteOrder.Cancelled)
                    continue;
                lOrder.Completed = lRemoteOrder.Completed;
                lOrder.Cancelled = lRemoteOrder.Cancelled;
                OrderStatus lStatus = OrderStatus.Placed;
                if (lOrder.Completed)
                {
                    lStatus = OrderStatus.Completed;
                    lOrder.Rate = lRemoteOrder.Rate;
                }
                if (lOrder.Cancelled)
                    lStatus = OrderStatus.Interrupted;
                FExchangeOrderManager.UpdateOrder(lOrder, lStatus);
                FExchangeOrderManager.WriteTransactionLogEntry(lOrder);
            }
        }

        private IEnumerable<ICurrencyIdentity> GetWalletCurrencies()
        {
            List<ICurrencyIdentity> lResult = new List<ICurrencyIdentity>();
            lResult.AddRange(FPandorasWalletConnection.GetCurrencies());
            lResult.AddRange(FPandorasWalletConnection.GetCurrencyTokens());
            return lResult;
        }

        private long? GetWalletCurrency(string aTicker = null, string aName = null)
        {
            var lCurrencyID = FPandorasWalletConnection.GetCurrencies()
                                                        .Where(lCurrency => string.Equals(aTicker, lCurrency.Ticker, StringComparison.OrdinalIgnoreCase) || (aName != null && string.Equals(aName, lCurrency.Name, StringComparison.OrdinalIgnoreCase)))
                                                        .FirstOrDefault()?
                                                        .Id;
            if (!lCurrencyID.HasValue)
                lCurrencyID = FPandorasWalletConnection.GetCurrencyTokens()
                                                        .Where(lToken => string.Equals(aTicker, lToken.Ticker, StringComparison.OrdinalIgnoreCase) || (aName != null && string.Equals(aName, lToken.Name, StringComparison.OrdinalIgnoreCase)))
                                                        .FirstOrDefault()?
                                                        .Id;
            return lCurrencyID;
        }

        private void VerifyCompletedOrders(IEnumerable<UserTradeOrder> aOrders)
        {
            ScanForCancelledOrders(aOrders);
            foreach (UserTradeOrder lOrder in aOrders)
                TryToWithdrawOrder(lOrder);
        }

        private void ScanForCancelledOrders(IEnumerable<UserTradeOrder> aOrders)
        {
            foreach (var lOrder in aOrders)
            {
                if (FCancelledOrderList.TryGetValue(lOrder.InternalID, out bool lMustWithdraw) && lOrder.Status != OrderStatus.Withdrawn)
                {
                    var lOriginalStatus = lOrder.Status;
                    bool lCanWithdraw = lOriginalStatus == OrderStatus.Placed || lOriginalStatus == OrderStatus.Waiting;
                    FExchangeOrderManager.UpdateOrder(lOrder, OrderStatus.Interrupted);
                    FExchangeOrderManager.WriteTransactionLogEntry(lOrder, OrderMessage.OrderMessageLevel.FatalError, string.Format("Order succesfully cancelled."));
                    if (lOriginalStatus == OrderStatus.Placed)
                        TryCancelOrder(lOrder);
                    if (lMustWithdraw && lCanWithdraw)
                        RefundOrder(lOrder, lOriginalStatus);
                    while (!FCancelledOrderList.TryRemove(lOrder.InternalID, out bool lDummy)) ;
                }
            }
        }

        private void TryCancelOrder(UserTradeOrder aOrder)
        {
            try
            {
                FExchanger.CancelOrder(aOrder);
                FExchangeOrderManager.WriteTransactionLogEntry(aOrder, OrderMessage.OrderMessageLevel.Error, $"Order id: {aOrder.ID} successfully cancelled at exchange.");
            }
            catch (Exception ex)
            {
                Universal.Log.Write(Universal.LogLevel.Error, string.Format("Order: {0}. Exception: {1}", aOrder.InternalID, ex.ToString()));
                FExchangeOrderManager.WriteTransactionLogEntry(aOrder, OrderMessage.OrderMessageLevel.Error, $"Error on cancel remote order: {ex.Message}.");
            }
        }

        /// <summary>
        /// Checks order that are in waiting state, and changes it to placed state if is needed (Here transaction confirmations are verified))
        /// </summary>
        /// <param name="aWaitingOrders">Orders with waiting state</param>
        /// <param name="aCurrencyItem">Order currency item</param>
        /// <param name="aCurrencyTransactions">Pandora's currency transactions</param>
        /// <param name="aListExchangeCoinMarket">Markets to look for</param>
        private void VerifyWaitingOrders(IEnumerable<UserTradeOrder> aWaitingOrders, ICurrencyIdentity aCurrencyItem, IEnumerable<TransactionRecord> aCurrencyTransactions)
        {
            long lCurrencyBlockHeight;
            int lExchangeminConf;
            ScanForCancelledOrders(aWaitingOrders);
            if (!aWaitingOrders.Any() || !aCurrencyTransactions.Any()
                || (lExchangeminConf = FExchanger.GetConfirmations(aCurrencyItem)) < 0
                || (lCurrencyBlockHeight = FPandorasWalletConnection.GetBlockHeight(aCurrencyItem.Id)) <= 0) return;
            var lConfirmedTxs = aCurrencyTransactions.Where(lTx => lTx.Block != 0 && (lCurrencyBlockHeight - lTx.Block) > lExchangeminConf);
            foreach (var lTx in lConfirmedTxs)
            {
                UserTradeOrder lItem = aWaitingOrders.Where(lOrder => lOrder.CoinTxID == lTx.TxId).FirstOrDefault();
                if (lItem == null)
                    continue;
                if (lItem.Status == OrderStatus.Waiting && lItem.Market != null)
                    TryToPlaceOrder(lItem);
            }
        }

        private void TryToWithdrawOrder(UserTradeOrder aOrder)
        {
            if (aOrder.Status != OrderStatus.Completed)
            {
                return;
            }

            if (aOrder.ErrorCounter % 60 == 0)
            {
                try
                {
                    if (aOrder.Market.BuyingCurrencyInfo.Id == 0) throw new Exception($"Failed to withdraw order id {aOrder.ID}. No currency data.");
                    if (FExchanger.WithdrawOrder(aOrder, FPandorasWalletConnection.GetCoinAddress(aOrder.Market.BuyingCurrencyInfo.Id), FExchanger.GetTransactionsFee(aOrder.Market.BuyingCurrencyInfo)))
                        FExchangeOrderManager.UpdateOrder(aOrder, OrderStatus.Withdrawn);
                    FExchangeOrderManager.WriteTransactionLogEntry(aOrder);
                    aOrder.ErrorCounter = 0;
                }
                catch (Exception ex)
                {
                    int lNumberofretrys = aOrder.ErrorCounter / 60;

                    if (lNumberofretrys >= 9)
                    {
                        aOrder.Cancelled = true;
                        FExchangeOrderManager.UpdateOrder(aOrder, OrderStatus.Interrupted);
                        FExchangeOrderManager.WriteTransactionLogEntry(aOrder);
                    }

                    aOrder.ErrorCounter += 1;
                    Universal.Log.Write(Universal.LogLevel.Error, string.Format("Order: {0}. Exception: {1}", aOrder.InternalID, ex.ToString()));
                    FExchangeOrderManager.WriteTransactionLogEntry(aOrder, OrderMessage.OrderMessageLevel.Error, "Error on Withdraw Order: " + ex.Message);
                    FExchangeOrderManager.WriteTransactionLogEntry(aOrder, OrderMessage.OrderMessageLevel.Info, string.Format("Retrying in 5 minute. Attempt: {0}/10", (lNumberofretrys + 1)));
                }
            }
            else
            {
                aOrder.ErrorCounter++;
            }
        }

        private bool MyDelayCanceled(int aDelaySeconds, CancellationToken aCancellationToken)
        {
            aDelaySeconds *= 10;
            while (aDelaySeconds-- > 0 && !aCancellationToken.IsCancellationRequested)
                Thread.Sleep(100);
            return aCancellationToken.IsCancellationRequested;
        }

        /// <summary>
        /// Checks order that are in initial state, and changes it to waiting state if is needed (Here the stop price is verified)
        /// </summary>
        /// <param name="aInitialOrders">Orders with initial state</param>
        /// <param name="aTicker">Order currency ticker</param>
        /// <param name="aCurrencyID">Order currency id</param>
        private void VerifyInitialOrders(IEnumerable<UserTradeOrder> aInitialOrders, ICurrencyIdentity aCurrency)
        {
            ScanForCancelledOrders(aInitialOrders);
            if (!aInitialOrders.Any())
                return;
            foreach (UserTradeOrder lOrder in aInitialOrders)
            {
                bool lPlaceOrderFlag;
                if (lOrder.Market.MarketDirection == MarketDirection.Sell)
                    lPlaceOrderFlag = lOrder.StopPrice <= lOrder.Market.Prices.Bid;
                else
                    lPlaceOrderFlag = lOrder.StopPrice >= lOrder.Market.Prices.Ask;

                if (lPlaceOrderFlag)
                    TryToTransferMoneyToExchange(lOrder);
            }
        }

        private void TryToPlaceOrder(UserTradeOrder aOrder)
        {
            if (aOrder.ErrorCounter % 60 == 0)
            {
                try
                {
                    FExchangeOrderManager.WriteTransactionLogEntry(aOrder, OrderMessage.OrderMessageLevel.Info, "Attempting to place order in exchange.");
                    if (FExchanger.PlaceOrder(aOrder))
                        FExchangeOrderManager.UpdateOrder(aOrder, OrderStatus.Placed);
                    FExchangeOrderManager.WriteTransactionLogEntry(aOrder);
                    aOrder.ErrorCounter = 0;
                }
                catch (Exception ex)
                {
                    int lNumberofretrys = aOrder.ErrorCounter / 60;

                    if (lNumberofretrys >= 9)
                    {
                        aOrder.Cancelled = true;
                        FExchangeOrderManager.UpdateOrder(aOrder, OrderStatus.Interrupted);
                        FExchangeOrderManager.WriteTransactionLogEntry(aOrder);
                    }

                    aOrder.ErrorCounter += 1;
                    Universal.Log.Write(Universal.LogLevel.Error, string.Format("Order: {0}. Exception: {1}", aOrder.InternalID, ex.ToString()));

                    FExchangeOrderManager.WriteTransactionLogEntry(aOrder, OrderMessage.OrderMessageLevel.Error, "Error while placing Order: " + ex.Message);
                    FExchangeOrderManager.WriteTransactionLogEntry(aOrder, OrderMessage.OrderMessageLevel.Info, string.Format("Retrying in 1 minute. Attempt: {0}/10", (lNumberofretrys + 1)));
                }
            }
            else
            {
                aOrder.ErrorCounter++;
            }
        }

        /// <summary>
        /// Tries to form a transaction that meets the order requeriments and then sends it to the exchange user address. Also does amount verification
        /// </summary>
        /// <param name="aOrder">Order to fulfill</param>
        /// <param name="aMarket">Order's market</param>
        /// <param name="aCurrencyID">Selected currency id</param>
        private async void TryToTransferMoneyToExchange(UserTradeOrder aOrder)
        {
            const string lAlreadyProcessingFlag = "Processing";
            try
            {
                if (aOrder.CoinTxID == lAlreadyProcessingFlag) return;
                FExchangeOrderManager.WriteTransactionLogEntry(aOrder, OrderMessage.OrderMessageLevel.Info, string.Format("Stop price reached, trying to send coins to exchange.", FExchanger.GetConfirmations(aOrder.Market.SellingCurrencyInfo)));
                string lExchangeAddress = FExchanger.GetDepositAddress(aOrder.Market);
                decimal lAmount = aOrder.Market.MarketDirection == MarketDirection.Sell ? aOrder.SentQuantity : aOrder.SentQuantity + (aOrder.SentQuantity * (decimal)0.025);
                aOrder.CoinTxID = lAlreadyProcessingFlag;
                string lTxID = await Task.Run(() => OnTransferCoinsNeeded?.Invoke(lAmount, lExchangeAddress, aOrder.Market.SellingCurrencyInfo.Id, null));
                if (string.IsNullOrEmpty(lTxID)) throw new Exception("Unable to broadcast transaction.");
                aOrder.CoinTxID = lTxID;
                FExchangeOrderManager.UpdateOrder(aOrder, OrderStatus.Waiting);
                FExchangeOrderManager.WriteTransactionLogEntry(aOrder);
                FExchangeOrderManager.WriteTransactionLogEntry(aOrder, OrderMessage.OrderMessageLevel.Info, string.Format("Number of confirmations needed: {0} confirmations", FExchanger.GetConfirmations(aOrder.Market.SellingCurrencyInfo)));
            }
            catch (Exception ex)
            {
                FExchangeOrderManager.UpdateOrder(aOrder, OrderStatus.Interrupted);
                FExchangeOrderManager.WriteTransactionLogEntry(aOrder, OrderMessage.OrderMessageLevel.FatalError, string.Format("Failed to send transaction. Details: {0}.", ex.Message));
            }
        }

        private void ProcessOrdersMarkedToRefund(TransactionRecordList aCurrencyTxs, ICurrencyIdentity aCurrencyItem)
        {
            if (!aCurrencyTxs.Any()) return;
            var lExchangeminConf = FExchanger.GetConfirmations(aCurrencyItem);
            var lCurrencyBlockHeight = FPandorasWalletConnection.GetBlockHeight(aCurrencyItem.Id);
            if (lExchangeminConf < 0 || lCurrencyBlockHeight <= 0) return;
            foreach (var lOrder in FWaitingToRefund.Values)
            {
                var lTx = aCurrencyTxs.ToList().Find(lTxItem => lTxItem.TxId == lOrder.CoinTxID);
                bool lConfirmed = lTx.Block != 0 && (lCurrencyBlockHeight - lTx.Block) > lExchangeminConf;
                if (lTx != null && lConfirmed)
                {
                    RefundOrder(lOrder);
                    while (!FWaitingToRefund.TryRemove(lOrder.InternalID, out UserTradeOrder lDummy)) ;
                }
            }
        }

        private void RefundOrder(UserTradeOrder aOrderToWithdraw, OrderStatus? aOriginalStatus = null)
        {
            if (aOriginalStatus.HasValue && aOriginalStatus.Value == OrderStatus.Waiting)
            {
                FExchangeOrderManager.WriteTransactionLogEntry(aOrderToWithdraw, OrderMessage.OrderMessageLevel.Info, $"Waiting for transaction {aOrderToWithdraw.CoinTxID} to be confirmed at exchange to try withdraw coins.");
                FWaitingToRefund.TryAdd(aOrderToWithdraw.InternalID, aOrderToWithdraw);
            }
            else
                TryRefundOrder(aOrderToWithdraw);
        }

        private void TryRefundOrder(UserTradeOrder aOrderToWithdraw)
        {
            try
            {
                FExchangeOrderManager.WriteTransactionLogEntry(aOrderToWithdraw, OrderMessage.OrderMessageLevel.Info, "Trying to refund coins from exchange.");
                FExchanger.RefundOrder(aOrderToWithdraw, FPandorasWalletConnection.GetCoinAddress(aOrderToWithdraw.Market.SellingCurrencyInfo.Id));
                FExchangeOrderManager.WriteTransactionLogEntry(aOrderToWithdraw, OrderMessage.OrderMessageLevel.Info, "Coins successfully withdrawn. Please wait some minutes to be shown in wallet");
            }
            catch (Exception ex)
            {
                Universal.Log.Write(Universal.LogLevel.Error, string.Format("Order: {0}. Exception: {1}", aOrderToWithdraw.InternalID, ex.ToString()));
                FExchangeOrderManager.WriteTransactionLogEntry(aOrderToWithdraw, OrderMessage.OrderMessageLevel.Error, $"Error on refund process in order: {ex.Message}. Please try to manually withdraw your coins from exchange");
            }
        }
    }
}