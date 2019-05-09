using Newtonsoft.Json;
using Pandora.Client.ClientLib;
using Pandora.Client.ServerAccess;
using Pandora.Client.Universal;
using Pandora.Client.Universal.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.Client.PandorasWallet.ServerAccess
{
    public delegate void DelegateOnCurrency(PandoraObjectNotifier aSender, CurrencyItem aCurrencyItem);
    public delegate void DelegateOnCurrencyStatusChange(PandoraObjectNotifier aSender, CurrencyStatusItem aCurrencyStatusItem);
    public delegate void DelegateOnBlockHeightChange(PandoraObjectNotifier aSender, uint aCurrencyId, ulong aBlockHeight);
    public delegate void DelegateOnTransaction(PandoraObjectNotifier aSender, TransactionRecord aTransactionRecord);
    /// <summary>
    /// Notifies user of changes in the server
    /// For example updates to a Currency or its status or transactions of 
    /// monitored account.  The events occur in the thread of this object
    /// that is ternminated if the connection is lost.
    /// </summary>
    public class PandoraObjectNotifier : MethodJetThread
    {
        private PandoraWalletServiceAccess FServerAccess;
        private Dictionary<uint, CurrencyInfo> FExistingCurrencyIds = new Dictionary<uint, CurrencyInfo>();
        private DateTime FTimeToCheckForNewCurrencies = DateTime.Now;
        private bool FExecuted;

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

        //private FNewCurrency
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
            FServerAccess = new PandoraWalletServiceAccess(aRemoteserver, aPort, aEncryptedConnection, aConnectionId);
            OnErrorEvent += PandoraObjectNotifier_OnErrorEvent;
            LastNotifiedCurrencyId = 0; 
            MinutesBetweenCheckForNewCurrencies = 60;
        }

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
            CheckMethod();
            var lId = (uint)aCurrencyItem.Id;
            FExistingCurrencyIds.Add(lId, new CurrencyInfo() { CurrencyItem = aCurrencyItem, LastCurrencyStatusItem = aLastCurrencyStatusItem } );
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
        public bool AddCurrencyToBeNotified(uint aCurrencyId)
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
        public bool AddUnconfirmedTransactions(uint aCurrencyId, TransactionRecord[] aTansactionRecords)
        {
            CheckMethod();
            if (FExistingCurrencyIds.ContainsKey(aCurrencyId))
            {
                FExistingCurrencyIds[aCurrencyId].TransactionRecords.AddRange(aTansactionRecords);
                FExistingCurrencyIds[aCurrencyId].TransactionRecords.Sort(TransactionRecord.GetTransactionRecordIdComparer());
                FExistingCurrencyIds[aCurrencyId].LastTransactionId = FExistingCurrencyIds[aCurrencyId].TransactionRecords.First().TransactionRecordId - 1;
                return true;
            }
            return false;
        }

        public int MinutesBetweenCheckForNewCurrencies { get; set; }

        public uint LastNotifiedCurrencyId { get; private set; }


        protected void CheckMethod()
        {
            if (FExecuted) throw new InvalidOperationException("Pandora Object Notifier running. This method must be called before Run().");
        }

        protected override void InternalInitialize()
        {
            base.InternalInitialize();
            BeginInvoke(new EventHandler(ThreadEventFindServerItems), null);
            FExecuted = true;
        }

        private void ThreadEventFindServerItems(object sender, EventArgs e)
        {
            if (Terminated) return;
            GetCurrencies();
            GetBlockHeight();
            GetLatestStatus();
            GetTransactions();
            if (!Terminated)
            {
                Thread.Sleep(2000);
                BeginInvoke(new EventHandler(ThreadEventFindServerItems), null);
            }
        }

        private void GetCurrencies()
        {
            if (LastNotifiedCurrencyId == uint.MaxValue) return; // not set yet
            if (FTimeToCheckForNewCurrencies < DateTime.Now) return;  // Wait until time to check
            var lList = JsonConvert.DeserializeObject<List<long>>(FServerAccess.GetCurrencyList((uint)LastNotifiedCurrencyId));
            foreach (var lCurrencyId in lList)
                if (!FExistingCurrencyIds.ContainsKey((uint)lCurrencyId))
                    BeginInvoke(new DelegateDoNewCurrencyThreadEvent(ThreadDoNewCurrency), lCurrencyId);
            if (!lList.Any())
                FTimeToCheckForNewCurrencies = DateTime.Now.AddMinutes(MinutesBetweenCheckForNewCurrencies);
        }

        private void ThreadDoNewCurrency(uint aCurrencyId)
        {
            var lCurrencyItem = JsonConvert.DeserializeObject<CurrencyItem>(FServerAccess.GetCurrency(aCurrencyId));
            var lCurrencyStatusItem = JsonConvert.DeserializeObject<CurrencyStatusItem>(FServerAccess.GetLastCurrencyStatus(aCurrencyId));
            DoOnNewCurrency(lCurrencyItem);
            DoOnCurrencyStatusChange(lCurrencyStatusItem);
            LastNotifiedCurrencyId = (uint)lCurrencyItem.Id;
            lock (FExistingCurrencyIds)
                FExistingCurrencyIds.Add(LastNotifiedCurrencyId, new CurrencyInfo() { CurrencyItem = lCurrencyItem, LastCurrencyStatusItem = lCurrencyStatusItem });
        }

        private void DoOnCurrencyStatusChange(CurrencyStatusItem aCurrencyStatusItem)
        {
            if (this.SynchronizingObject != null)
                this.SynchronizingObject.Invoke(OnCurrencyStatusChange, new object[] { this, aCurrencyStatusItem });
            else
                OnCurrencyStatusChange(this, aCurrencyStatusItem);
        }

        private void DoOnNewCurrency(CurrencyItem aCurrencyItem)
        {
            if (this.SynchronizingObject != null)
                this.SynchronizingObject.Invoke(OnNewCurrency, new object[] { this, aCurrencyItem });
            else
                OnNewCurrency(this, aCurrencyItem);
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
                    var lCurrencyStatusItemList = JsonConvert.DeserializeObject<List<CurrencyStatusItem>>(FServerAccess.GetCurrencyStatusList(lKeyValue.Key, lKeyValue.Value.LastCurrencyStatusItem.StatusId));
                    if (Terminated) return;
                    if (lCurrencyStatusItemList.Any())
                    {
                        bool FUpdated = false;
                        foreach (var lCurrencyStatusItem in lCurrencyStatusItemList)
                            if (FUpdated = lCurrencyStatusItem.Status == CurrencyStatus.Updated) break;
                        if (FUpdated)
                        {
                            lKeyValue.Value.CurrencyItem = JsonConvert.DeserializeObject<CurrencyItem>(FServerAccess.GetCurrency(lKeyValue.Key));
                            DoOnUpdatedCurrency(lKeyValue.Value.CurrencyItem);
                        }
                        DoOnCurrencyStatusChange(lCurrencyStatusItemList.Last());
                        lKeyValue.Value.LastCurrencyStatusItem = lCurrencyStatusItemList.Last();
                    }
                    else
                        lKeyValue.Value.NextReadTime = DateTime.Now.AddMinutes(5);
                }
        }

        private void DoOnUpdatedCurrency(CurrencyItem aCurrencyItem)
        {
            if (this.SynchronizingObject != null)
                this.SynchronizingObject.Invoke(OnUpdatedCurrency, new object[] { this, aCurrencyItem });
            else
                OnUpdatedCurrency(this, aCurrencyItem);
        }

        private TransactionRecord FindTransactionId(List<TransactionRecord> aTransactionRecords, ulong aTransactionRecordId)
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
                    List<TransactionRecord> lTransactionList;
                    if (lCurrencyInfo.TransactionRecords.Any())
                    {
                        lTransactionList = JsonConvert.DeserializeObject<List<TransactionRecord>>(FServerAccess.GetTransactionRecords((uint)lCurrencyInfo.CurrencyItem.Id, lCurrencyInfo.TransactionRecords[0].TransactionRecordId));
                        if (Terminated) return;
                        int lIndex = 0;
                        while(lIndex < lCurrencyInfo.TransactionRecords.Count)
                        {
                            var lUnconfirmedTxRec = lCurrencyInfo.TransactionRecords[lIndex];
                            var lTxRecFromServer = FindTransactionId(lTransactionList, lUnconfirmedTxRec.TransactionRecordId);
                            if (lTxRecFromServer == null)
                            {
                                string s = string.Format("Transaction Record {0} TXID {1} is missing from remote Database", lUnconfirmedTxRec.TransactionRecordId, lTxRecFromServer.TxId);
                                DoErrorHandler(new Exception(s));
                                Log.Write(LogLevel.Warning, s);
                            }
                            else if (!lTxRecFromServer.IsEqual(lUnconfirmedTxRec))
                            {
                                    DoOnUpdatedTransaction(lTxRecFromServer);
                                    lCurrencyInfo.TransactionRecords[lIndex] = lUnconfirmedTxRec;
                            }
                            else if (lUnconfirmedTxRec.Block != 0 && lCurrencyInfo.BlockHeight - lUnconfirmedTxRec.Block > (ulong)lCurrencyInfo.CurrencyItem.MinConfirmations)
                            {
                                lCurrencyInfo.TransactionRecords.RemoveAt(lIndex);
                                lIndex--;
                            }
                            lTransactionList.Remove(lTxRecFromServer);
                            lIndex++;
                        }
                    }
                    else
                        lTransactionList = JsonConvert.DeserializeObject<List<TransactionRecord>>(FServerAccess.GetTransactionRecords((uint)lCurrencyInfo.CurrencyItem.Id, lCurrencyInfo.LastTransactionId));
                    if (Terminated) return;
                    if (lTransactionList.Any())
                    {
                        for (int i = 0; i < lTransactionList.Count; i++)
                        {
                            var lTx = lTransactionList[i];// this is done for RevDebug for testing
                            DoOnNewTransaction(lTx);
                            lCurrencyInfo.TransactionRecords.Add(lTx);
                        }
                    }
                }
        }

        private void DoOnNewTransaction(TransactionRecord aTransactionRecord)
        {
            if (this.SynchronizingObject != null)
                this.SynchronizingObject.Invoke(OnNewTransaction, new object[] { this, aTransactionRecord });
            else
                OnNewTransaction(this, aTransactionRecord);
        }

        private void DoOnUpdatedTransaction(TransactionRecord aTransactionRecord)
        {
            if (this.SynchronizingObject != null)
                this.SynchronizingObject.Invoke(OnUpdatedTransaction, new object[] { this, aTransactionRecord });
            else
                OnUpdatedTransaction(this, aTransactionRecord);
        }

        private void DoNotifyBlockHeight(uint aCurrencyId, ulong aCurrencyHeight)
        {
            if (OnBlockHeightChange != null)
                if (this.SynchronizingObject != null)
                    SynchronizingObject.Invoke(OnBlockHeightChange, new object[] { this, aCurrencyId, aCurrencyHeight });
                else
                    OnBlockHeightChange(this, aCurrencyId, aCurrencyHeight);
        }

        delegate void DelegateDoNewCurrencyThreadEvent(uint aCurrencyId);


        protected override void InternalFinalize()
        {
            base.InternalFinalize();
            Log.Write(LogLevel.Info, "Pandora Notifier shutting down.");
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

        class CurrencyInfo
        {
            public CurrencyInfo()
            {
                MonitoredCurrency = false;
                NextReadTime = DateTime.Now;
                TransactionRecords = new List<TransactionRecord>();
                LastCurrencyStatusItem = new CurrencyStatusItem();
            }
            public CurrencyItem CurrencyItem;
            public CurrencyStatusItem LastCurrencyStatusItem;
            public bool MonitoredCurrency;
            public DateTime NextReadTime;
            public ulong BlockHeight;
            public ulong LastTransactionId;
            public List<TransactionRecord> TransactionRecords;
        }
    }
}
