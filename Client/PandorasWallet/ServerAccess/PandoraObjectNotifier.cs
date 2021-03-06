﻿//   Copyright 2017-2019 Davinci Codes
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
using Pandora.Client.Crypto.Currencies;
using Pandora.Client.Crypto.Currencies.Ethereum;
using Pandora.Client.PandorasWallet.Models;
using Pandora.Client.ServerAccess;
using Pandora.Client.Universal;
using Pandora.Client.Universal.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Pandora.Client.PandorasWallet.ServerAccess
{
    public delegate void DelegateOnCurrency(object aSender, CurrencyItem aCurrencyItem);

    public delegate void DelegateOnCurrencyStatusChange(object aSender, CurrencyStatusItem aCurrencyStatusItem);

    public delegate void DelegateOnBlockHeightChange(object aSender, long aCurrencyId, long aBlockHeight);

    public delegate void DelegateOnTransaction(object aSender, TransactionRecord aTransactionRecord, IEnumerable<ClientTokenTransactionItem> aTokenTransaction = null);

    public delegate void DelegateOnTokenTransaction(object aSender, ClientTokenTransactionItem aTransactionRecord);

    public delegate void DelegateOnSendTransactionCompleted(object aSender, string aErrorMsg, string aTxId);

    public delegate void DelegateOnUpgradeFileReady(object aSender, string aFileName);

    public delegate void DelegateOnSentTransaction(object aSender, string aTxData, string aTxId, long aCurrencyId, DateTime aStartTime, DateTime aEndTime);

    public delegate void DelegateOnUpdatedCurrencyParams(CurrencyItem aCurrencyItem, IChainParams aPreviousChainParams);

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
        private Dictionary<long, CurrencyInfo> FCurrencyInventory = new Dictionary<long, CurrencyInfo>();
        private Dictionary<long, TransactionTracker> FTransactionTrackers = new Dictionary<long, TransactionTracker>();
        private DateTime FTimeToCheckForNewCurrencies = DateTime.Now;
        private PandoraJsonConverter FConverter = new PandoraJsonConverter();
        private bool FExecuted;
        private DateTime FLastCheckedForUpgrade = DateTime.MinValue;
        private string FUpgradePath;
        private MyWebClient FWebClientDownloader;

        private class MyWebClient : WebClient
        {
            public string FileName { get; private set; }

            public MyWebClient(string aFileName) : base()
            {
                FileName = aFileName;
            }
        }

        /// <summary>
        /// Creates a Notifier instance to send notificaitons of changes
        /// this creates a sperate remote connection to the server
        /// if connection is lost this object must be recreated.
        /// </summary>
        /// <param name="aRemoteserver">Server to connect to</param>
        /// <param name="aPort">port of the ser</param>
        /// <param name="aEncryptedConnection">HTTPS connection or not</param>
        /// <param name="ConnectionId">ID of an existing connection</param>
        public PandoraObjectNotifier(string aRemoteserver, int aPort, bool aEncryptedConnection, string aConnectionId, string aDataPath)
        {
            FUpgradePath = aDataPath;
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

        /// <summary>
        ///
        /// </summary>
        public event DelegateOnUpgradeFileReady OnUpgradeFileReady;

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
        public void AddExistingCurrency(CurrencyItem aCurrencyItem, CurrencyStatusItem aLastCurrencyStatusItem, long aCurrentBlockHeight)
        {
            CheckIfNotifierIsExecuting();
            var lId = (uint) aCurrencyItem.Id;
            var lCurrencyInfo = new CurrencyInfo()
            {
                CurrencyItem = aCurrencyItem,
                LastCurrencyStatusItem = aLastCurrencyStatusItem,
                BlockHeight = aCurrentBlockHeight
            };
            FCurrencyInventory.Add(lId, lCurrencyInfo);
            FTransactionTrackers.Add(lId, new TransactionTracker(lCurrencyInfo));
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
            lock (FCurrencyInventory)
                if (FCurrencyInventory.ContainsKey(aCurrencyId))
                {
                    FCurrencyInventory[aCurrencyId].MonitoredCurrency = true;
                    return true;
                }
            return false;
        }

        /// <summary>
        /// Provide a list of existing currencies for Accounts to be notified
        /// So this object can determine of any changes have occured to an unconfirmed
        /// transaction.
        /// </summary>
        /// <param name="aCurrencyId"></param>
        /// <param name="aTansactionRecords"></param>
        public bool AddDBTransactions(long aCurrencyId, IEnumerable<TransactionRecord> aTransactionRecords)
        {
            if (aTransactionRecords.Any())
            {
                CheckIfNotifierIsExecuting();
                if (FTransactionTrackers.TryGetValue(aCurrencyId, out TransactionTracker lTxTracker))
                {
                    foreach (var lTx in aTransactionRecords)
                        lTxTracker.AddUpdateTransaction(lTx);
                    return true;
                }
            }
            return false;
        }

        public string UpgradeFileName { get; private set; }

        public bool AutoUpdate { get; set; }

        public int MinutesBetweenCheckForNewCurrencies { get; set; }

        public long LastNotifiedCurrencyId { get; private set; }

        protected void CheckIfNotifierIsExecuting()
        {
            if (FExecuted) throw new InvalidOperationException("Pandora Object Notifier running. This method must be called before Run().");
        }

        protected override void InternalInitialize()
        {
            LastNotifiedCurrencyId = 0; //TODO: we need to load all currencies in all over again because of fuckups by Migel who can choose an number higher than thte last.
            Log.Write(LogLevel.Debug, " Setting last currency id to 0 for startup.");
            base.InternalInitialize();
            // Create the timer that will be called to do all polling of the sever
            FFindServerItemsTimer = new Timer((ex) => Timer_FindServerItems(), null, Timeout.Infinite, Timeout.Infinite);
            // Call the the event that will actually do the work and set the timer.
            BeginInvoke(new Action(ThreadEventFindServerItems));
            FExecuted = true;
        }

        // once a day
        // go check https://api.github.com/repos/DavinciCodes15/PandorasWallet/releases/latest
        // for latest version and download.
        private void LookForUpgrade()
        {
            if (!AutoUpdate || FLastCheckedForUpgrade.AddDays(1) > DateTime.Now) return;
            FLastCheckedForUpgrade = DateTime.Now;
            try
            {
                string s;
                var webRequest = WebRequest.Create("https://api.github.com/repos/DavinciCodes15/PandorasWallet/releases/latest") as HttpWebRequest;
                if (webRequest != null)
                {
                    webRequest.ContentType = "application/json";
                    webRequest.UserAgent = "Nothing";

                    using (var lStream = webRequest.GetResponse().GetResponseStream())
                    using (var sr = new StreamReader(lStream))
                        s = sr.ReadToEnd();
                    dynamic lGitHub = JsonConvert.DeserializeObject(s);
                    //string lNewVersion = lGitHub.tag_name;
                    string lRemoteVersion = lGitHub.tag_name;
                    Version lCurrentVer = new Version(SystemUtils.GetAssemblyVersion());
                    s = lRemoteVersion.Substring(1);
                    Version lNewVersion = new Version(s);
                    s = lGitHub.assets[0].browser_download_url;
                    string lFileName = Path.Combine(FUpgradePath, $"Pandara's Wallet {lRemoteVersion}.msi");
                    if (lNewVersion > lCurrentVer)
                    {
                        Log.Write(LogLevel.Info, "New verison {0} found.", lNewVersion);
                        if (!File.Exists(lFileName) && FWebClientDownloader == null)
                        {
                            if (Terminated) return;
                            FWebClientDownloader?.Dispose(); // clean up old object
                            FWebClientDownloader = new MyWebClient(lFileName);
                            FWebClientDownloader.DownloadFileCompleted += FWebClientDownloader_DownloadFileCompleted;
                            FWebClientDownloader.DownloadFileAsync(
                                    // Param1 = Link of file
                                    new System.Uri(s),
                                    // Param2 = Path to save
                                    lFileName
                                );
                        }
                        else
                        {
                            if (FWebClientDownloader != null) // this is odd the downloader should have completed by now
                                throw new Exception("FWebClientDownloader is not null meaning the download from yesterday did not complete.");
                            else if (UpgradeFileName != lFileName)
                                DoUpgradeFileReady(lFileName);
                        }
                    }
                    else if (lNewVersion == lCurrentVer) // remove install file if no longer needed.
                        if (File.Exists(lFileName)) File.Delete(lFileName);
                }
            }
            catch (Exception e)
            {
                Log.Write(LogLevel.Critical, "Error downloading upgrade file. {0}", e);
            }
        }

        private void FWebClientDownloader_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                MyWebClient lSender = sender as MyWebClient;
                if (!e.Cancelled && e.Error == null)
                    DoUpgradeFileReady(lSender.FileName);
                else
                    if (File.Exists(lSender.FileName)) File.Delete(lSender.FileName);
                FWebClientDownloader = null;
                lSender.Dispose();
            }
            catch (Exception ex)
            {
                this.DoErrorHandler(ex);
            }
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
                            aDataPackage.NextRequestTime = DateTime.Now.AddMilliseconds(RandomNumber(500, 1000));
                            if (FServerAccess.IsTransactionSent(aDataPackage.SendHandle))
                                lTxId = FServerAccess.GetTransactionId(aDataPackage.SendHandle);
                        }
                        catch (Exception ex)
                        {
                            aDataPackage.ErrorCount++;
                            Log.Write(LogLevel.Critical, "Error getting send result {0} - {1}", ex.Message, ex.StackTrace);
                            if (aDataPackage.ErrorCount > 20 || ex is PandoraServerException) // retried 20 times for 20 secs so its dead.
                                throw;
                            Thread.Sleep(1000);
                        }
                    else
                        Thread.Sleep(100);
                    if (lTxId == null)
                        BeginInvoke(new DelegateThreadSendTransaction(ThreadSendTransaction), aDataPackage);
                    else
                    {
                        if (IsByteArray(lTxId))
                            DoSendTransactionCompleted(aDataPackage, "", lTxId);
                        else
                            DoSendTransactionCompleted(aDataPackage, lTxId, "");
                    }
                }
            }
            catch (Exception e)
            {
                DoSendTransactionCompleted(aDataPackage, e.Message, "");
                DoErrorHandler(e);
            }
        }

        private static bool IsHex(char c)
        {
            return (c >= '0' && c <= '9') ||
                     (c >= 'a' && c <= 'f') ||
                     (c >= 'A' && c <= 'F');
        }

        private bool IsByteArray(string aText)
        {
            bool lResult = true;
            foreach (char c in aText)
                if (!(lResult = IsHex(c))) break;
            return lResult;
        }

        private void StopTimer(Timer aTimer)
        {
            aTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        //I make this to be one shoot his default behavior, this is because we are going to be using starttimer to reset the process anyways
        private void StartTimer(Timer aTimer, int aMilliSec, bool aIsOneShot = true)
        {
            Log.Write(LogLevel.Debug, "StartTimer: aMilliSec = {0}, aIsOneShot = {1}", aMilliSec, aIsOneShot);
            if (aIsOneShot)
                aTimer.Change(aMilliSec, Timeout.Infinite);
            else
                aTimer.Change(aMilliSec, aMilliSec);
        }

        /// <summary>
        /// This Timer call is executed in an another thread so I don't want to do the work here
        /// so we will invoke a call on actual thread that will do the work.
        /// </summary>
        private void Timer_FindServerItems()
        {
            Log.Write(LogLevel.Debug, "Timer_FindServerItems: fired");
            if (Terminated) return;
            BeginInvoke(new Action(ThreadEventFindServerItems));
        }

        private void ThreadEventFindServerItems()
        {
            try
            {
                Log.Write(LogLevel.Debug, "ThreadEventFindServerItems: GetCurrencies.");
                GetCurrencies();
                Log.Write(LogLevel.Debug, "ThreadEventFindServerItems: GetLatestStatus.");
                GetLatestStatus();
                Log.Write(LogLevel.Debug, "ThreadEventFindServerItems: GetTransactions");
                GetTransactions();
                Log.Write(LogLevel.Debug, "ThreadEventFindServerItems: GetBlockHeight.");
                GetAllBlockHeights(); //NOTE: This shoud be always the last thing (currency related) to do
                LookForUpgrade();
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Error, $"ThreadEventFindServerItems: {ex}");
            }
            finally
            {
                if (!Terminated)
                    StartTimer(FFindServerItemsTimer, RandomNumber(5000, 10000));
                else
                    Log.Write(LogLevel.Debug, "ThreadEventFindServerItems: Terminated");
            }
            Log.Write(LogLevel.Debug, "ThreadEventFindServerItems: Completed--------------------------------------------------------");
        }

        private int RandomNumber(int aMin, int aMax)
        {
            Random random = new Random();
            return random.Next(aMin, aMax);
        }

        private void GetCurrencies()
        {
            if (LastNotifiedCurrencyId == uint.MaxValue) return; // not set yet
            var lStringValueList = FServerAccess.GetCurrencyList(LastNotifiedCurrencyId);
            if (lStringValueList == null)   // this should never return a null!!! but sometimes does.
                throw new Exception("Error in GetCurrencies from server.");
            var lList = JsonConvert.DeserializeObject<List<long>>(lStringValueList, FConverter);
            if (Terminated) return;
            if (lList.Any())
                BeginInvoke(new DelegateDoNewCurrencyThreadEvent(ThreadDoNewCurrency), lList.ToArray(), 0); // do this work later so I don't waste time here as I must do other work
        }

        private void ThreadDoNewCurrency(long[] aCurrencyIdArray, int aStartIndex)
        {
            Log.Write(LogLevel.Debug, "ThreadDoNewCurrency: Begin");
            if (!Terminated)
                if (aStartIndex >= aCurrencyIdArray.Length)
                {
                    var lList = JsonConvert.DeserializeObject<List<long>>(FServerAccess.GetCurrencyList(LastNotifiedCurrencyId), FConverter);
                    if (!Terminated)
                        if (lList.Any())
                            BeginInvoke(new DelegateDoNewCurrencyThreadEvent(ThreadDoNewCurrency), lList.ToArray(), 0);
                }
                else if (!FCurrencyInventory.ContainsKey(aCurrencyIdArray[aStartIndex]))
                {
                    var lCurrencyItem = JsonConvert.DeserializeObject<CurrencyItem>(FServerAccess.GetCurrency(aCurrencyIdArray[aStartIndex]), FConverter);
                    if (!Terminated)
                    {
                        var lCurrencyStatusItem = JsonConvert.DeserializeObject<CurrencyStatusItem>(FServerAccess.GetLastCurrencyStatus(lCurrencyItem.Id), FConverter);
                        if (!Terminated)
                        {
                            DoOnNewCurrency(lCurrencyItem);
                            DoOnCurrencyStatusChange(lCurrencyStatusItem);
                            LastNotifiedCurrencyId = lCurrencyItem.Id;
                            lock (FCurrencyInventory)
                                FCurrencyInventory.Add(LastNotifiedCurrencyId, new CurrencyInfo() { CurrencyItem = lCurrencyItem, LastCurrencyStatusItem = lCurrencyStatusItem });
                            BeginInvoke(new DelegateDoNewCurrencyThreadEvent(ThreadDoNewCurrency), aCurrencyIdArray, ++aStartIndex);
                        }
                    }
                }
                else
                {
                    LastNotifiedCurrencyId = aCurrencyIdArray[aStartIndex];
                    BeginInvoke(new DelegateDoNewCurrencyThreadEvent(ThreadDoNewCurrency), aCurrencyIdArray, ++aStartIndex);
                }
            Log.Write(LogLevel.Debug, "ThreadDoNewCurrency: End");
        }

        private void GetAllBlockHeights()
        {
            if (Terminated) return;
            foreach (var lKeyValue in FCurrencyInventory)
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
            foreach (var lKeyValue in FCurrencyInventory)
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
                            var lJsonResponse = FServerAccess.GetCurrency(lKeyValue.Key);
                            lKeyValue.Value.CurrencyItem = JsonConvert.DeserializeObject<CurrencyItem>(lJsonResponse, FConverter);
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

        private void GetTransactions()
        {
            if (Terminated) return;
            foreach (var lCurrencyInfo in FCurrencyInventory.Values)
                if (lCurrencyInfo.MonitoredCurrency && FTransactionTrackers.TryGetValue(lCurrencyInfo.CurrencyItem.Id, out TransactionTracker lTxTracker))
                {
                    bool lCanBeToken = lCurrencyInfo.CurrencyItem.ChainParamaters.Capabilities.HasFlag(CapablityFlags.EthereumProtocol);
                    // Get trasactions from server that is greater than > the current ID.
                    // thus this tx id must the oldest tx and has less than max Confimations
                    // just incase the tx moves to a new block in a chain reorg
                    string s = FServerAccess.GetTransactionRecords(lCurrencyInfo.CurrencyItem.Id, lTxTracker.ScanTransactionRecordId, lCanBeToken);
                    List<TransactionRecord> lRemoteTransactionRecords = JsonConvert.DeserializeObject<List<TransactionRecord>>(s, FConverter);
                    lRemoteTransactionRecords = (List<TransactionRecord>) ProcessForEthereumTransactions(lCurrencyInfo.CurrencyItem, lRemoteTransactionRecords);
                    if (Terminated) return;
                    // if there is any remote TX that are new we will add them to our look up list
                    if (lRemoteTransactionRecords.Any())
                    {
                        TransactionRecord[] lOrderedRemoteTxRecords = lRemoteTransactionRecords.OrderBy(lTx => lTx.TransactionRecordId).ToArray();
                        for (int i = 0; i < lOrderedRemoteTxRecords.Length; i++)
                        {
                            var lTx = lOrderedRemoteTxRecords[i];
                            if (lTxTracker.Contains(lTx))
                            {
                                if (lTxTracker.CheckForChanges(lTx))
                                {
                                    DoOnUpdatedTransaction(lTx);
                                    lTxTracker.AddUpdateTransaction(lTx);
                                }
                            }
                            else
                            {
                                var lTokenTx = TryProcessNewTokenTransaction(lCurrencyInfo, lTx);
                                DoOnNewTransaction(lTx, lTokenTx);
                                lTxTracker.AddUpdateTransaction(lTx);
                            }
                        }
                    }
                }
        }

        public IEnumerable<TransactionRecord> ProcessForEthereumTransactions(CurrencyItem aCurrencyItem, IEnumerable<TransactionRecord> aTransactionRecords)
        {
            if (aCurrencyItem.ChainParamaters.Capabilities.HasFlag(CapablityFlags.EthereumProtocol))
            {
                foreach (var lTransaction in aTransactionRecords)
                {
                    var lFirstTxOutput = lTransaction.Outputs.Where(lOutput => !string.IsNullOrEmpty(lOutput.Script)).FirstOrDefault();
                    if (lFirstTxOutput != null)
                    {
                        var lTxJson = Encoding.UTF8.GetString(Convert.FromBase64String(lFirstTxOutput.Script));
                        dynamic lTokenInfo = JsonConvert.DeserializeObject(lTxJson);
                        string lNonceString = (string) lTokenInfo.nonce;
                        lFirstTxOutput.Index = int.Parse($"0{lNonceString.Replace("0x", string.Empty)}", System.Globalization.NumberStyles.HexNumber);
                    }
                }
            }

            return aTransactionRecords;
        }

        private IEnumerable<ClientTokenTransactionItem> TryProcessNewTokenTransaction(CurrencyInfo aCurrencyInfo, TransactionRecord aTransactionRecord)
        {
            IEnumerable<ClientTokenTransactionItem> lResult = null;
            if (aCurrencyInfo.CurrencyItem.ChainParamaters.Capabilities.HasFlag(CapablityFlags.EthereumProtocol))
            {
                try
                {
                    var lOutputScript = aTransactionRecord.Outputs.Where(lOutput => !string.IsNullOrEmpty(lOutput.Script)).Select(lOutput => lOutput.Script).FirstOrDefault();
                    if (lOutputScript != null)
                    {
                        var lTxJson = Encoding.UTF8.GetString(Convert.FromBase64String(lOutputScript));
                        if (aTransactionRecord.TxId.Contains("FEERETURN_"))
                        {
                            var lTokenTxReceipt = JsonConvert.DeserializeObject<TokenTransactionReceipt>(lTxJson);
                            string lTokenTxStatus = lTokenTxReceipt.Status.Replace("0x", string.Empty);
                            if (!string.IsNullOrEmpty(lTokenTxStatus) && ushort.TryParse(lTokenTxStatus, out ushort lStatus))
                            {
                                var lStatusTx = new ClientTokenTransactionItem
                                {
                                    ParentTransactionID = aTransactionRecord.TxId.Replace("FEERETURN_", string.Empty),
                                    Valid = (lStatus == 1)
                                };
                                DoOnUpdatedTransaction(null, new ClientTokenTransactionItem[] { lStatusTx });
                            }
                            else
                                Log.Write(LogLevel.Error, $"Unable to read receipt for transaction id {aTransactionRecord.TxId}");
                        }
                        else
                        {
                            var lTokenInfo = JsonConvert.DeserializeObject<TokenTransactionInfo>(lTxJson);
                            if (!string.IsNullOrEmpty(lTokenInfo.Input.Replace("0x", string.Empty)))
                                lResult = BuildERC20TokenTransaction(aTransactionRecord, lTokenInfo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(LogLevel.Error, $"Unable to process token transaction. TxID: {aTransactionRecord.TxId}. Details: {ex}");
                }
            }
            return lResult;
        }

        private IEnumerable<ClientTokenTransactionItem> BuildERC20TokenTransaction(TransactionRecord aTx, TokenTransactionInfo aTokenTxInfo, bool aIsValid = true)
        {
            var lResult = new List<ClientTokenTransactionItem>();

            if (ERC20TokenDecoder.TryDecode(aTokenTxInfo.Input, out IEnumerable<ERC20DataOutput> lDecodedTransactionPayloads))
            {
                foreach (var lDecodedPayload in lDecodedTransactionPayloads)
                {
                    lResult.Add(new ClientTokenTransactionItem
                    {
                        From = string.IsNullOrEmpty(lDecodedPayload.OriginAddress) ? aTokenTxInfo.From : lDecodedPayload.OriginAddress,
                        To = lDecodedPayload.DestinationAddress,
                        TokenAddress = string.IsNullOrEmpty(lDecodedPayload.ContractAddress) ? aTokenTxInfo.To : lDecodedPayload.ContractAddress,
                        Amount = lDecodedPayload.AmountSent,
                        ParentTransactionID = aTx.TxId,
                        Valid = aIsValid
                    });
                }
            }
            return lResult;
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

        private void DoUpgradeFileReady(string aFileName)
        {
            try
            {
                UpgradeFileName = aFileName;
                this.SynchronizingObject?.BeginInvoke(OnUpgradeFileReady, new object[] { this, aFileName });
            }
            catch (Exception e)
            {
                DoErrorHandler(e);
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

        private void DoOnNewTransaction(TransactionRecord aTransactionRecord, IEnumerable<ClientTokenTransactionItem> aTokenTransaction = null)
        {
            try
            {
                this.SynchronizingObject?.Invoke(OnNewTransaction, new object[] { this, aTransactionRecord, aTokenTransaction });
            }
            catch (Exception e)
            {
                DoErrorHandler(e);
            }
        }

        private void DoOnUpdatedTransaction(TransactionRecord aTransactionRecord, IEnumerable<ClientTokenTransactionItem> aTokenTransaction = null)
        {
            try
            {
                this.SynchronizingObject?.Invoke(OnUpdatedTransaction, new object[] { this, aTransactionRecord, aTokenTransaction });
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
            FServerAccess?.Dispose();
            FFindServerItemsTimer?.Dispose();
            FWebClientDownloader?.CancelAsync();
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

        private class TokenTransactionInfo
        {
            public string From { get; set; }
            public string To { get; set; }
            public string Input { get; set; }
        }

        private class TokenTransactionReceipt
        {
            public string GasUsed { get; set; }
            public string Status { get; set; }
        }

        private class CurrencyInfo
        {
            public CurrencyInfo()
            {
                MonitoredCurrency = false;
                NextReadTime = DateTime.Now;
                LastCurrencyStatusItem = new CurrencyStatusItem();
            }

            public CurrencyItem CurrencyItem;
            public CurrencyStatusItem LastCurrencyStatusItem;
            public bool MonitoredCurrency;
            public DateTime NextReadTime;
            public long BlockHeight;
        }

        private class TransactionTracker
        {
            private ConcurrentDictionary<string, TransactionRecord> FTransactionRecordsCache;
            private CurrencyInfo FCurrencyInfo;
            private long FLastTxRecordId;
            public long ScanTransactionRecordId => FTransactionRecordsCache.Any() ? FTransactionRecordsCache.Values.Min(lTx => lTx.TransactionRecordId) - 1 : FLastTxRecordId;

            public IEnumerable<TransactionRecord> TransactionCache => FTransactionRecordsCache.Values.OrderBy(lTx => lTx.TransactionRecordId);

            public TransactionTracker(CurrencyInfo aCurrencyInfo)
            {
                FCurrencyInfo = aCurrencyInfo;
                FTransactionRecordsCache = new ConcurrentDictionary<string, TransactionRecord>();
            }

            public void AddUpdateTransaction(TransactionRecord aTransaction)
            {
                if (aTransaction.TransactionRecordId > FLastTxRecordId)
                    FLastTxRecordId = aTransaction.TransactionRecordId;
                FTransactionRecordsCache.TryAdd(aTransaction.TxId, aTransaction);

                //Clean cache from confirmed transactions
                var lToRemoveTransactions = FTransactionRecordsCache.Where(lTxKeyPair => (lTxKeyPair.Value.Block > 0) && ((FCurrencyInfo.BlockHeight - lTxKeyPair.Value.Block) > (2 * FCurrencyInfo.CurrencyItem.MinConfirmations))).ToArray();
                foreach (var lTx in lToRemoveTransactions)
                    RemoveTransaction(lTx.Key);
            }

            public bool Contains(TransactionRecord aTx)
            {
                return FTransactionRecordsCache.ContainsKey(aTx.TxId);
            }

            public bool CheckForChanges(TransactionRecord aTx)
            {
                bool lResult = false;
                if (FTransactionRecordsCache.TryGetValue(aTx.TxId, out TransactionRecord lCacheTx))
                    lResult = !aTx.IsEqual(lCacheTx);
                return lResult;
            }

            public void RemoveTransaction(string aTxID)
            {
                FTransactionRecordsCache.TryRemove(aTxID, out _);
            }
        }
    }
}