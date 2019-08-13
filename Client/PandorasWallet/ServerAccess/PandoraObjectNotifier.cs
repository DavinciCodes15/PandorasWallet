//   Copyright 2017-2019 Davinci Codes
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Also use the software for non-commercial purposes.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE

using Newtonsoft.Json;
using Pandora.Client.ClientLib;
using Pandora.Client.ServerAccess;
using Pandora.Client.Universal;
using Pandora.Client.Universal.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Pandora.Client.PandorasWallet.ServerAccess
{
    public delegate void DelegateOnCurrency(object aSender, CurrencyItem aCurrencyItem);

    public delegate void DelegateOnCurrencyStatusChange(object aSender, CurrencyStatusItem aCurrencyStatusItem);

    public delegate void DelegateOnBlockHeightChange(object aSender, long aCurrencyId, long aBlockHeight);

    public delegate void DelegateOnTransaction(object aSender, TransactionRecord aTransactionRecord);

    public delegate void DelegateOnSendTransactionCompleted(object aSender, string aErrorMsg, string aTxId);

    public delegate void DelegateOnSentTransaction(object aSender, string aTxData, string aTxId, long aCurrencyId, DateTime aStartTime, DateTime aEndTime);

    /// <summary>
    /// Notifies user of changes in the server
    /// For example updates to a Currency or its status or transactions of
    /// monitored account.  The events occur in the thread of this object
    /// that is ternminated if the connection is lost.
    /// </summary>
    public class PandoraObjectNotifier : MethodJetThread
    {
        private Timer FFindServerItemsTimer;
        private PandoraWalletServiceAccess FServerAccess;
        private Dictionary<long, CurrencyInfo> FExistingCurrencyIds = new Dictionary<long, CurrencyInfo>();
        private DateTime FTimeToCheckForNewCurrencies = DateTime.Now;
        private PandoraJsonConverter FConverter = new PandoraJsonConverter();
        private bool FExecuted;

        /// <summary>
        /// Creates a Notifier instance to send notificaitons of changes
        /// this creates a sperate remote connection to the server
        /// if connection is lost this object must be recreated.
        /// </summary>
        /// <param name="aRemoteserver">Server to connect to</param>
        /// <param name="aPort">port of the ser</param>
        /// <param name="aEncryptedConnection">HTTPS connection or not</param>
        /// <param name="ConnectionId">ID of an existing connection</param>
        public PandoraObjectNotifier(string aRemoteserver, int aPort, bool aEncryptedConnection, string aConnectionId)
        {
            OnErrorEvent += PandoraObjectNotifier_OnErrorEvent;
            LastNotifiedCurrencyId = 0;
            MinutesBetweenCheckForNewCurrencies = 60;

            FServerAccess = new PandoraWalletServiceAccess(aRemoteserver, aPort, aEncryptedConnection, aConnectionId);
        }

        public override void Dispose()
        {
            base.Dispose();
            FServerAccess?.Dispose();
        }

        /// <summary>
        /// After transaciton is sent store info into database later on for perfromance testing and improvements
        /// </summary>
        public event DelegateOnSentTransaction OnSentTransaction;

        /// <summary>
        /// This event return only currencies addes to Paandora Server
        /// </summary>
        public event DelegateOnCurrency OnNewCurrency;

        /// <summary>
        /// If an icon or name or something has changed on the currency this
        /// event will fire.
        /// </summary>
        public event DelegateOnCurrency OnUpdatedCurrency;

        /// <summary>
        /// Updates changes to the BlockHeight
        /// </summary>
        public event DelegateOnBlockHeightChange OnBlockHeightChange;

        /// <summary>
        /// If the CurrencyStatus fires then this event is fired.
        /// </summary>
        public event DelegateOnCurrencyStatusChange OnCurrencyStatusChange;

        /// <summary>
        /// Fired if a new transactoin occurs on monitored coins
        /// </summary>
        public event DelegateOnTransaction OnNewTransaction;

        /// <summary>
        /// If the transaction changes in any date, block, TX hash, this will fire.
        /// You must assume that all values in the TransactionRecord has changed and update
        /// the cache
        /// </summary>
        public event DelegateOnTransaction OnUpdatedTransaction;

        public override void Run()
        {
            FServerAccess.GetBlockHeight(1); // validates connection ID is valid.
            if (FExecuted) throw new InvalidOperationException("This object can only be executed onece.");
            base.Run();
        }

        /// <summary>
        /// If any currencies exist call this method to inform the object
        /// what currencies are not considered new while running.
        ///
        /// This call will cause this object to fire the OnUpdateCurrency,
        /// OnCurrencyStatusChange and OnNewCurrency
        /// </summary>
        /// <param name="aCurrencyItem">Id of the existing currency</param>
        /// <param name="aLastCurrencyStatusItem">Id of the last known status Item recived</param>
        public void AddExistingCurrency(CurrencyItem aCurrencyItem, CurrencyStatusItem aLastCurrencyStatusItem)
        {
            CheckIfNotifierIsExecuting();
            var lId = (uint)aCurrencyItem.Id;
            FExistingCurrencyIds.Add(lId, new CurrencyInfo() { CurrencyItem = aCurrencyItem, LastCurrencyStatusItem = aLastCurrencyStatusItem });
            if (lId > LastNotifiedCurrencyId)
                LastNotifiedCurrencyId = lId;
        }

        /// <summary>
        /// Call this method to start reciving OnNewTransaction, OnBlcokHeightChange
        /// and OnUpdatedTransaciton
        /// </summary>
        /// <param name="aCurrencyId"></param>
        /// <param name="aLastTransactionId"></param>
        /// <returns></returns>
        public bool AddCurrencyToBeNotified(long aCurrencyId)
        {
            lock (FExistingCurrencyIds)
                if (FExistingCurrencyIds.ContainsKey(aCurrencyId))
                {
                    FExistingCurrencyIds[aCurrencyId].MonitoredCurrency = true;
                    return true;
                }
            return false;
        }

        /// <summary>
        /// Provide a list of existing currencies for Accounts to be notified
        /// So this object can determin of any changes have occured to an unconfirmed
        /// transaction.
        /// </summary>
        /// <param name="aCurrencyId"></param>
        /// <param name="aTansactionRecords"></param>
        public bool AddUnconfirmedTransactions(long aCurrencyId, TransactionRecord[] aTansactionRecords)
        {
            if (aTansactionRecords.Any())
            {
                CheckIfNotifierIsExecuting();
                if (FExistingCurrencyIds.ContainsKey(aCurrencyId))
                {
                    FExistingCurrencyIds[aCurrencyId].LocalTransactionRecords.AddRange(aTansactionRecords);
                    FExistingCurrencyIds[aCurrencyId].LocalTransactionRecords.Sort(TransactionRecord.GetTransactionRecordIdComparer()); //Note is this sorted correctly?
                    FExistingCurrencyIds[aCurrencyId].LastTransactionRecordId = FExistingCurrencyIds[aCurrencyId].LocalTransactionRecords.First().TransactionRecordId - 1;
                    return true;
                }
            }
            return false;
        }

        public int MinutesBetweenCheckForNewCurrencies { get; set; }

        public long LastNotifiedCurrencyId { get; private set; }

        protected void CheckIfNotifierIsExecuting()
        {
            if (FExecuted) throw new InvalidOperationException("Pandora Object Notifier running. This method must be called before Run().");
        }

        protected override void InternalInitialize()
        {
            base.InternalInitialize();
            FFindServerItemsTimer = new Timer((ex) => Timer_FindServerItems(), null, Timeout.Infinite, Timeout.Infinite);
            BeginInvoke(new EventHandler(ThreadEventFindServerItems), this, null);
            FExecuted = true;
        }

        private class SendTxPackage
        {
            public SendTxPackage(string aTransactionData, long aCurrencyId, DelegateOnSendTransactionCompleted aEvent)
            {
                TransactionData = aTransactionData;
                CurrencyId = aCurrencyId;
                CompletedEvent = aEvent;
                NextRequestTime = DateTime.Now;
                StartTime = DateTime.Now;
            }

            public string TransactionData;
            public long CurrencyId;
            public DelegateOnSendTransactionCompleted CompletedEvent;
            public long SendHandle;
            public int ErrorCount;
            public DateTime NextRequestTime;
            public DateTime StartTime;
        }

        private delegate void DelegateThreadSendTransaction(SendTxPackage aDataPackage);

        internal void SendNewTransaction(string aTransactionData, long aCurrencyId, DelegateOnSendTransactionCompleted aEvent)
        {
            BeginInvoke(new DelegateThreadSendTransaction(ThreadSendTransaction), new SendTxPackage(aTransactionData, aCurrencyId, aEvent));
        }

        private void ThreadSendTransaction(SendTxPackage aDataPackage)
        {
            string lTxId = null;
            if (Terminated) return;
            try
            {
                if (aDataPackage.SendHandle == 0)
                {
                    Log.Write(LogLevel.Debug, "Sending transaction for {0}", aDataPackage.CurrencyId);
                    aDataPackage.SendHandle = FServerAccess.SendTransaction(aDataPackage.CurrencyId, aDataPackage.TransactionData);
                    // resend the message in the loop
                    BeginInvoke(new DelegateThreadSendTransaction(ThreadSendTransaction), aDataPackage);
                }
                else
                {
                    if (aDataPackage.NextRequestTime < DateTime.Now)
                        try
                        {
                            aDataPackage.NextRequestTime = DateTime.Now.AddMilliseconds(RandomNumber(2000, 3000));
                            if (FServerAccess.IsTransactionSent(aDataPackage.SendHandle))
                                lTxId = FServerAccess.GetTransactionId(aDataPackage.SendHandle);
                        }
                        catch (Exception ex)
                        {
                            aDataPackage.ErrorCount++;
                            Log.Write(LogLevel.Critical, "Error getting send result {0} - {1}", ex.Message, ex.StackTrace);
                            if (aDataPackage.ErrorCount > 10 || ex is PandoraServerException) // retried 20 times for 30 secs so its dead.
                                throw;
                            Thread.Sleep(3000);
                        }
                    else
                        Thread.Sleep(100);
                    if (lTxId == null)
                        BeginInvoke(new DelegateThreadSendTransaction(ThreadSendTransaction), aDataPackage);
                    else
                        DoSendTransactionCompleted(aDataPackage, "", lTxId);
                }
            }
            catch (Exception e)
            {
                DoSendTransactionCompleted(aDataPackage, e.Message, "");
                DoErrorHandler(e);
            }
        }

        private void DoSendTransactionCompleted(SendTxPackage aDataPackage, string aErrorMsg, string aTxId)
        {
            try
            {
                this.SynchronizingObject?.BeginInvoke(aDataPackage.CompletedEvent, new object[] { this, aErrorMsg, aTxId });
                if (aTxId != null)
                    this.SynchronizingObject?.Invoke(OnSentTransaction, new object[] { this, aDataPackage.TransactionData, aTxId, aDataPackage.CurrencyId, aDataPackage.StartTime, DateTime.Now });
            }
            catch (Exception e)
            {
                DoErrorHandler(e);
            }
        }

        private void StopTimer(Timer aTimer)
        {
            aTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void StartTimer(Timer aTimer, double aMilliSec)
        {
            aTimer.Change(TimeSpan.FromMilliseconds(aMilliSec), TimeSpan.FromMilliseconds(aMilliSec));
        }

        private void Timer_FindServerItems()
        {
            StopTimer(FFindServerItemsTimer);
            if (Terminated) return;
            BeginInvoke(new EventHandler(ThreadEventFindServerItems), this, null);
        }

        private void ThreadEventFindServerItems(object sender, EventArgs e)
        {
            try
            {
                GetCurrencies();
                GetBlockHeight();
                GetLatestStatus();
                GetTransactions();
            }
            finally
            {
                if (!Terminated)
                    StartTimer(FFindServerItemsTimer, RandomNumber(10000, 15000));
            }
        }

        private int RandomNumber(int aMin, int aMax)
        {
            Random random = new Random();
            return random.Next(aMin, aMax);
        }

        private void GetCurrencies()
        {
            if (LastNotifiedCurrencyId == uint.MaxValue) return; // not set yet
            var lList = JsonConvert.DeserializeObject<List<long>>(FServerAccess.GetCurrencyList(LastNotifiedCurrencyId), FConverter);
            if (Terminated) return;
            if (lList.Any())
                BeginInvoke(new DelegateDoNewCurrencyThreadEvent(ThreadDoNewCurrency), lList.ToArray(), 0);
        }

        private void ThreadDoNewCurrency(long[] aCurrencyIdArray, int aStartIndex)
        {
            if (Terminated) return;
            if (aStartIndex >= aCurrencyIdArray.Length)
            {
                var lList = JsonConvert.DeserializeObject<List<long>>(FServerAccess.GetCurrencyList(LastNotifiedCurrencyId), FConverter);
                if (Terminated) return;
                if (lList.Any())
                    BeginInvoke(new DelegateDoNewCurrencyThreadEvent(ThreadDoNewCurrency), lList.ToArray(), 0);
            }
            else if (!FExistingCurrencyIds.ContainsKey(aCurrencyIdArray[aStartIndex]))
            {
                var lCurrencyItem = JsonConvert.DeserializeObject<CurrencyItem>(FServerAccess.GetCurrency(aCurrencyIdArray[aStartIndex]), FConverter);
                if (Terminated) return;
                var lCurrencyStatusItem = JsonConvert.DeserializeObject<CurrencyStatusItem>(FServerAccess.GetLastCurrencyStatus(lCurrencyItem.Id), FConverter);
                if (Terminated) return;
                DoOnNewCurrency(lCurrencyItem);
                DoOnCurrencyStatusChange(lCurrencyStatusItem);
                LastNotifiedCurrencyId = lCurrencyItem.Id;
                lock (FExistingCurrencyIds)
                    FExistingCurrencyIds.Add(LastNotifiedCurrencyId, new CurrencyInfo() { CurrencyItem = lCurrencyItem, LastCurrencyStatusItem = lCurrencyStatusItem });
                BeginInvoke(new DelegateDoNewCurrencyThreadEvent(ThreadDoNewCurrency), aCurrencyIdArray, ++aStartIndex);
            }
        }

        private void DoOnCurrencyStatusChange(CurrencyStatusItem aCurrencyStatusItem)
        {
            try
            {
                this.SynchronizingObject?.Invoke(OnCurrencyStatusChange, new object[] { this, aCurrencyStatusItem });
            }
            catch (Exception e)
            {
                DoErrorHandler(e);
            }
        }

        private void DoOnNewCurrency(CurrencyItem aCurrencyItem)
        {
            try
            {
                this.SynchronizingObject.Invoke(OnNewCurrency, new object[] { this, aCurrencyItem });
            }
            catch (Exception e)
            {
                DoErrorHandler(e);
            }
        }

        private void GetBlockHeight()
        {
            if (Terminated) return;
            foreach (var lKeyValue in FExistingCurrencyIds)
                if (lKeyValue.Value.MonitoredCurrency)
                {
                    var lCurrentHeight = FServerAccess.GetBlockHeight(lKeyValue.Key);
                    if (Terminated) return;
                    if (lKeyValue.Value.BlockHeight != lCurrentHeight)
                    {
                        DoNotifyBlockHeight(lKeyValue.Key, lCurrentHeight);
                        lKeyValue.Value.BlockHeight = lCurrentHeight;
                    }
                }
        }

        private void GetLatestStatus()
        {
            if (Terminated) return;
            foreach (var lKeyValue in FExistingCurrencyIds)
                if (lKeyValue.Value.NextReadTime < DateTime.Now)
                {
                    string s = FServerAccess.GetCurrencyStatusList(lKeyValue.Key, lKeyValue.Value.LastCurrencyStatusItem.StatusId);
                    var lCurrencyStatusItemList = JsonConvert.DeserializeObject<List<CurrencyStatusItem>>(s, FConverter);
                    if (Terminated) return;
                    if (lCurrencyStatusItemList.Any())
                    {
                        bool FUpdated = false;
                        foreach (var lCurrencyStatusItem in lCurrencyStatusItemList)
                            if (FUpdated = lCurrencyStatusItem.Status == CurrencyStatus.Updated) break;
                        if (FUpdated)
                        {
                            lKeyValue.Value.CurrencyItem = JsonConvert.DeserializeObject<CurrencyItem>(FServerAccess.GetCurrency(lKeyValue.Key), FConverter);
                            if (Terminated) return;
                            DoOnUpdatedCurrency(lKeyValue.Value.CurrencyItem);
                        }
                        DoOnCurrencyStatusChange(lCurrencyStatusItemList.Last());
                        lKeyValue.Value.LastCurrencyStatusItem = lCurrencyStatusItemList.Last();
                    }
                    else
                        lKeyValue.Value.NextReadTime = DateTime.Now.AddMinutes(5);
                }
        }

        private TransactionRecord FindTransactionId(List<TransactionRecord> aTransactionRecords, long aTransactionRecordId)
        {
            foreach (var lTx in aTransactionRecords)
                if (lTx.TransactionRecordId == aTransactionRecordId) return lTx;
            return null;
        }

        private void GetTransactions()
        {
            if (Terminated) return;
            foreach (var lCurrencyInfo in FExistingCurrencyIds.Values)
                if (lCurrencyInfo.MonitoredCurrency)
                {
                    string s = FServerAccess.GetTransactionRecords(lCurrencyInfo.CurrencyItem.Id, lCurrencyInfo.LastTransactionRecordId);
                    List<TransactionRecord> lRemoteTransactionRecords = JsonConvert.DeserializeObject<List<TransactionRecord>>(s, FConverter);
                    if (lCurrencyInfo.LocalTransactionRecords.Any())
                    {
                        if (Terminated) return;
                        int lIndex = 0;
                        while (lIndex < lCurrencyInfo.LocalTransactionRecords.Count)
                        {
                            if (Terminated) return;
                            var lLocalTransactionRecord = lCurrencyInfo.LocalTransactionRecords[lIndex];
                            var lLocalTxConfirmation = lCurrencyInfo.BlockHeight - lLocalTransactionRecord.Block;

                            var lRemoteTransactionRecord = FindTransactionId(lRemoteTransactionRecords, lLocalTransactionRecord.TransactionRecordId);
                            if (lRemoteTransactionRecord == null)
                            {
                                s = string.Format("Transaction Record {0} TXID {1} is missing from remote Database", lLocalTransactionRecord.TransactionRecordId, lLocalTransactionRecord.TxId);
                                DoErrorHandler(new Exception(s));
                                lCurrencyInfo.LocalTransactionRecords.Remove(lLocalTransactionRecord);
                                lIndex--;
                            }
                            else if (!lRemoteTransactionRecord.IsEqual(lLocalTransactionRecord))
                            {
                                DoOnUpdatedTransaction(lRemoteTransactionRecord);
                                lCurrencyInfo.LocalTransactionRecords[lIndex] = lRemoteTransactionRecord;
                            }
                            else if ((lLocalTransactionRecord.Block != 0 && lLocalTxConfirmation > lCurrencyInfo.CurrencyItem.MinConfirmations) || !lLocalTransactionRecord.Valid)
                            {
                                lCurrencyInfo.LocalTransactionRecords.Remove(lLocalTransactionRecord);
                                lCurrencyInfo.LastTransactionRecordId = lLocalTransactionRecord.TransactionRecordId;
                                lIndex--;
                            }
                            lRemoteTransactionRecords.Remove(lRemoteTransactionRecord);
                            lIndex++;
                        }
                    }
                    if (Terminated) return;
                    if (lRemoteTransactionRecords.Any())
                    {
                        lCurrencyInfo.LastTransactionRecordId = lRemoteTransactionRecords.First().TransactionRecordId - 1;
                        for (int i = 0; i < lRemoteTransactionRecords.Count; i++)
                        {
                            var lTx = lRemoteTransactionRecords[i];// this is done for RevDebug for testing
                            DoOnNewTransaction(lTx);
                            lCurrencyInfo.LocalTransactionRecords.Add(lTx);
                        }
                    }
                }
        }

        private void DoOnUpdatedCurrency(CurrencyItem aCurrencyItem)
        {
            try
            {
                this.SynchronizingObject?.Invoke(OnUpdatedCurrency, new object[] { this, aCurrencyItem });
            }
            catch (Exception e)
            {
                DoErrorHandler(e);
            }
        }

        private void DoOnNewTransaction(TransactionRecord aTransactionRecord)
        {
            try
            {
                this.SynchronizingObject?.Invoke(OnNewTransaction, new object[] { this, aTransactionRecord });
            }
            catch (Exception e)
            {
                DoErrorHandler(e);
            }
        }

        private void DoOnUpdatedTransaction(TransactionRecord aTransactionRecord)
        {
            try
            {
                this.SynchronizingObject?.Invoke(OnUpdatedTransaction, new object[] { this, aTransactionRecord });
            }
            catch (Exception e)
            {
                DoErrorHandler(e);
            }
        }

        private void DoNotifyBlockHeight(long aCurrencyId, long aCurrencyHeight)
        {
            try
            {
                SynchronizingObject?.BeginInvoke(OnBlockHeightChange, new object[] { this, aCurrencyId, aCurrencyHeight });
            }
            catch (Exception e)
            {
                DoErrorHandler(e);
            }
        }

        private delegate void DelegateDoNewCurrencyThreadEvent(long[] aCurrencyIdArray, int aStartIndex);

        protected override void InternalFinalize()
        {
            base.InternalFinalize();
            Log.Write(LogLevel.Info, "Pandora Object Notifier shutting down.");
            FServerAccess.Dispose();
            FServerAccess = null;
        }

        /// <summary>
        /// Drop box for errors that will only be logged.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="aIsHandled"></param>
        private void PandoraObjectNotifier_OnErrorEvent(object sender, Exception e, ref bool aIsHandled)
        {
            aIsHandled = true;
            Log.Write(LogLevel.Error, "Error in PandoraObjectNotifier : {0}", e);
        }

        private class CurrencyInfo
        {
            public CurrencyInfo()
            {
                MonitoredCurrency = false;
                NextReadTime = DateTime.Now;
                LocalTransactionRecords = new List<TransactionRecord>();
                LastCurrencyStatusItem = new CurrencyStatusItem();
            }

            public CurrencyItem CurrencyItem;
            public CurrencyStatusItem LastCurrencyStatusItem;
            public bool MonitoredCurrency;
            public DateTime NextReadTime;
            public long BlockHeight;
            public long LastTransactionRecordId;
            public List<TransactionRecord> LocalTransactionRecords;
        }
    }
}