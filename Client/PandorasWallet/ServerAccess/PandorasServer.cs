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
#if DEBUG
#define TESTING
#endif

using Newtonsoft.Json;
using Pandora.Client.ClientLib;
using Pandora.Client.ServerAccess;
using Pandora.Client.Universal;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.Client.PandorasWallet.ServerAccess
{
    public class PandorasServer : IDisposable
    {
        public PandoraWalletServiceAccess FServerAccess;

        private PandorasCache FPandoraCache;
        private string _datapath = string.Empty;
        private ClientJsonConverter FConverter;
        private CurrencyAccountList FCurrencyAccountList;

        public event Action OnCurrencyFullListExpired;

        public event Action OnCurrencyStatusFullListExpired;

        public event Action OnMonitoredAccountsFullListExpired;

        public CurrencyStatusEvent OnCurrencyStatus;

        private CancellationTokenSource FTxUpdateCancellationSource;

        private Task TxUpdating = null;

        public event TransactionEvent OnTransactions;

        public event Action<ulong> OnCurrencyItemUpdated;

        public event Func<long, bool> OnCurrencyItemMustUpdate;

        /// <summary>
        /// Provides a copy of current currencyIds
        /// </summary>
        public long[] CurrencyIds => FCurrencyIdList.ToArray();

        private ConcurrentBag<long> FCurrencyIdList;

        private Dictionary<ulong, long> FStatusNumber = new Dictionary<ulong, long>();
        private Dictionary<ulong, ulong> FCurrencyAccountCheckpointIds = new Dictionary<ulong, ulong>();
        private Dictionary<ulong, ulong> FTransactionCheckpointIds = new Dictionary<ulong, ulong>();
        private List<Tuple<uint, ulong>> FToConfirm;
        private Dictionary<uint, ulong> FBlockHeights = new Dictionary<uint, ulong>();
        private ulong FTemporalCurrencyCheckpoint = 0;
        private Timer FCurrencyStatusTimer;
        private Timer FCurrencyItemTimer;

        public PandorasServer(string aDataPath, string aRemoteServer = "localhost", int aPort = 20159, bool aEncryptationConnection = false)
        {
            DataPath = aDataPath;
            FServerAccess = new PandoraWalletServiceAccess(aRemoteServer, aPort, aEncryptationConnection);
            FConverter = new ClientJsonConverter(this);

            FCurrencyAccountList = new CurrencyAccountList(this);
            FToConfirm = new List<Tuple<uint, ulong>>();
        }

        public string Username => FServerAccess.Username;

        public string Email => FServerAccess.Email;

        public string InstanceId { get; private set; }
        public bool Connected => FServerAccess.Connected;

        public CurrencyAccountList MonitoredAccounts => FCurrencyAccountList;

        public string DataPath
        {
            get => _datapath;
            set
            {
                if (Directory.Exists(value))
                {
                    _datapath = value;
                    FPandoraCache?.RefreshCacheFolder();
                    return;
                }
                throw new DirectoryNotFoundException("Datapath directory not found");
            }
        }

        private bool CheckforNewTransactions(out List<uint> aListOfCurrencies)
        {
            return FPandoraCache.CheckforNewTransactions(out aListOfCurrencies);
        }

        private bool CheckforNewCurrencyStatus(out List<uint> aListOfCurrencies)
        {
            return FPandoraCache.CheckforNewCurrencyStatus(out aListOfCurrencies);
        }

        public void CheckIfConfirmed(uint aId, ulong aBlockHeight)
        {
            Tuple<uint, ulong> lElement = new Tuple<uint, ulong>(aId, aBlockHeight);

            if (FToConfirm.Contains(lElement) || aId == 0)
            {
                return;
            }

            lock (FToConfirm)
            {
                FToConfirm.Add(lElement);
            }
        }

        public bool VerifyAddress(uint aCurrencyId, string aAddress)
        {
            return FServerAccess.CheckAddress(aCurrencyId, aAddress);
        }

        private byte[] FetchCurrencyIcon(uint aCurrencyId)
        {
            return JsonConvert.DeserializeObject<byte[]>(FServerAccess.GetCurrencyIcon(aCurrencyId), FConverter);
        }

        public List<CurrencyItem> FetchCurrencies(long? aId = null)
        {
            List<CurrencyItem> lReturningList = new List<CurrencyItem>();
            long[] lToDownload;

            if (!aId.HasValue)
            {
                List<long> lJsonListOfCurrencies = new List<long>();
                List<long> lReturned;
                do
                {
                    lReturned = JsonConvert.DeserializeObject<List<long>>(FServerAccess.GetCurrencyList(lJsonListOfCurrencies.Any() ? (uint)lJsonListOfCurrencies.Max() : 0), FConverter);
                    lJsonListOfCurrencies.AddRange(lReturned);
                } while (lReturned.Any());
                lToDownload = lJsonListOfCurrencies.Except(FCurrencyIdList).ToArray();
            }
            else
                lToDownload = new long[1] { aId.Value };

            foreach (long it in lToDownload)
            {
                lReturningList.Add(JsonConvert.DeserializeObject<CurrencyItem>(FServerAccess.GetCurrency((uint)it), FConverter));
                if (!FCurrencyIdList.Contains(it))
                    FCurrencyIdList.Add(it);
            }

            return lReturningList;
        }

        public List<CurrencyStatusItem> FetchCurrencyStatus(uint aId)
        {
            bool lFirstFetching = false;

            if (!FCurrencyIdList.Contains(aId))
            {
                throw new Exception("Id out of range");
            }

            if (!FStatusNumber.Keys.Contains(aId))
            {
                FStatusNumber.Add(aId, 0);
                lFirstFetching = true;
            }

            List<CurrencyStatusItem> Returninglist = new List<CurrencyStatusItem>();
            List<CurrencyStatusItem> JsonListOfStatuses;
            do
            {
                JsonListOfStatuses = JsonConvert.DeserializeObject<List<CurrencyStatusItem>>(FServerAccess.GetCurrencyStatusList(aId, FStatusNumber[aId]), FConverter);
                if (JsonListOfStatuses.Any())
                    FStatusNumber[aId] = JsonListOfStatuses.Max(x => x.StatusId);
                Returninglist.AddRange(JsonListOfStatuses);
            } while (JsonListOfStatuses.Any());

            //Look for updated status elements, if we found something notify for coin to re-download all specific id data
            bool lSomethingUpdated = Returninglist.Where(x => x.Status == CurrencyStatus.Updated).Any();
            if (lSomethingUpdated)
            {
                if (!lFirstFetching)
                    CurrencyStatusUpdated(aId);

                Returninglist.RemoveAll(x => x.Status == CurrencyStatus.Updated);
            }

            //We set the last element on the list with the last id from previous list, so in case we removed an updated status we can continue downloading
            if (Returninglist.Any())
            {
                Returninglist = Returninglist.OrderBy((x) => x.StatusId).ToList();
                if (lSomethingUpdated)
                {
                    CurrencyStatusItem lLastObject = Returninglist[Returninglist.Count - 1];
                    if (lLastObject.StatusId != FStatusNumber[aId])
                        Returninglist[Returninglist.Count - 1] = new CurrencyStatusItem(FStatusNumber[aId], lLastObject.CurrencyId, lLastObject.StatusTime, lLastObject.Status, lLastObject.ExtendedInfo, lLastObject.BlockHeight);
                }
            }

            return Returninglist;
        }

        public bool IsNewAccount()
        {
            List<CurrencyAccount> lJsonListOfAccounts = JsonConvert.DeserializeObject<List<CurrencyAccount>>(FServerAccess.GetMonitoredAcccounts(1, 0), FConverter);
            return lJsonListOfAccounts.Count == 0;
        }

        private void CurrencyStatusUpdated(ulong aCurrencyId)
        {
            bool? lReturn = OnCurrencyItemMustUpdate?.Invoke((long)aCurrencyId);

            if (lReturn.HasValue && lReturn.Value)
            {
                OnCurrencyItemUpdated?.Invoke(aCurrencyId);
            }
        }

        public List<CurrencyAccount> FetchMonitoredAccounts(ulong aId)

        {
            if (!FCurrencyIdList.Contains((long)aId))
            {
                throw new Exception("Id out of range");
            }

            if (!FCurrencyAccountCheckpointIds.Keys.Contains(aId))
            {
                FCurrencyAccountCheckpointIds.Add(aId, 0);
            }

            List<CurrencyAccount> JsonListOfAccounts;
            List<CurrencyAccount> ReturningList = new List<CurrencyAccount>();

            do
            {
                JsonListOfAccounts = JsonConvert.DeserializeObject<List<CurrencyAccount>>(FServerAccess.GetMonitoredAcccounts(aId, FCurrencyAccountCheckpointIds[aId]), FConverter);
                if (JsonListOfAccounts.Any())
                {
                    FCurrencyAccountCheckpointIds[aId] = JsonListOfAccounts.Max(x => x.Id);
                }

                ReturningList.AddRange(JsonListOfAccounts);
            } while (JsonListOfAccounts.Any());

            return ReturningList;
        }

        public List<TransactionRecord> FetchTransactions(uint aId)

        {
            if (!FCurrencyIdList.Contains(aId))
            {
                throw new Exception("Id out of range");
            }

            if (!FTransactionCheckpointIds.Keys.Contains(aId))
            {
                FTransactionCheckpointIds.Add(aId, 0);
            }

            List<TransactionRecord> JsonListOfTransactions;
            List<TransactionRecord> ReturningList = new List<TransactionRecord>();

            do
            {
                JsonListOfTransactions = JsonConvert.DeserializeObject<List<TransactionRecord>>(FServerAccess.GetTransactionRecords(aId, FTransactionCheckpointIds[aId]), FConverter);
                if (JsonListOfTransactions.Any())
                {
                    FTransactionCheckpointIds[aId] = JsonListOfTransactions.Max(x => x.TransactionRecordId);
                }

                ReturningList.AddRange(JsonListOfTransactions);
            } while (JsonListOfTransactions.Any());

            List<TransactionRecord> lUnConfirmedTX = ReturningList.Where(x => x.Block == 0).ToList();

            if (lUnConfirmedTX.Any())
            {
                FTransactionCheckpointIds[aId] = lUnConfirmedTX.Min(x => x.TransactionRecordId) - 1;
            }

            return ReturningList;
        }

        public void SetCheckpoints(Dictionary<string, long[]> aCheckpoints)
        {
            FCurrencyIdList = new ConcurrentBag<long>(aCheckpoints["Currencies2"]);

            if (aCheckpoints["CurrenciesStatus"].Any())
            {
                foreach (long it in FCurrencyIdList)
                {
                    int index = aCheckpoints["CurrenciesStatusIndex"].ToList().IndexOf(it);
                    FStatusNumber[Convert.ToUInt64(it)] = index != -1 ? Convert.ToInt64(aCheckpoints["CurrenciesStatus"][index]) : 0;
                }
            }

            if (aCheckpoints["MonitoredAccounts"].Any())
            {
                foreach (long it in FCurrencyIdList)
                {
                    int index = aCheckpoints["MonitoredAccountsIndex"].ToList().IndexOf(it);
                    FCurrencyAccountCheckpointIds[Convert.ToUInt64(it)] = index != -1 ? Convert.ToUInt64(aCheckpoints["MonitoredAccounts"][index]) : 0;
                }
            }

            if (aCheckpoints["TxTable"].Any())
            {
                foreach (long it in FCurrencyIdList)
                {
                    int index = aCheckpoints["TxTableIndex"].ToList().IndexOf(it);
                    FTransactionCheckpointIds[Convert.ToUInt64(it)] = index != -1 ? Convert.ToUInt64(aCheckpoints["TxTable"][index]) : 0;
                }
            }
        }

        public void StartTxUpdatingTask()
        {
            if ((TxUpdating == null || TxUpdating.IsCanceled) && FCurrencyIdList.Any())
                TxUpdating = Task.Run(() => TxUpdatingTask(this, FTxUpdateCancellationSource.Token)); //Start fetching transactions now that I have currencies to work with
        }

        public void StartCurrencyStatusUpdatingTask()
        {
            if (FCurrencyStatusTimer == null)
            {
                FCurrencyStatusTimer = new Timer(new TimerCallback(CurrencyStatusUpdatingTask), null, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));
            }
        }

        public void StartCurrencyItemUpdatingTask()
        {
            if (FCurrencyItemTimer == null)
            {
                FCurrencyItemTimer = new Timer(new TimerCallback(CurrencyItemUpdatingTask), null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(120));
            }
        }

        private void CurrencyItemUpdatingTask(object state)
        {
            FPandoraCache.CheckfornewCurrencies();
        }

        public string CreateTransaction(uint aCurrencyID, CurrencyTransaction aSendTx)
        {
            return FServerAccess.CreateTransaction(aSendTx);
        }

        public CurrencyItem GetCurrency(uint aCurrencyId)
        {
            CurrencyItem lResult = FPandoraCache.GetCurrencyItem(aCurrencyId);
            if (lResult == null)
            {
                CurrencyItem[] llist = FPandoraCache.GetCurrencies(aCurrencyId);
                if (llist.Any())
                    lResult = llist[0];
            }
            if (lResult == null)
            {
                lResult = JsonConvert.DeserializeObject<CurrencyItem>(FServerAccess.GetCurrency(aCurrencyId), FConverter);
            }
            if (lResult == null)
                throw new Exception(string.Format("Unable to GetCurrency because currency ID {0} does not exist.", aCurrencyId));

            if (!FCurrencyIdList.Contains(aCurrencyId))
                FCurrencyIdList.Add(aCurrencyId);
            return lResult;
        }

        public byte[] GetCurrencyIcon(uint aCurrencyId)
        {
            return GetCurrency(aCurrencyId).Icon;
        }

        public CurrencyItem[] GetCurrencyList()
        {
            return FPandoraCache.GetCurrencies();
        }

        public CurrencyStatusItem GetCurrencyStatus(uint aCurrencyId)
        {
            // Only get the status from the DBCashe because another service will go get it.
            CurrencyStatusItem lResult = FPandoraCache.GetCurrencyStatusItem(aCurrencyId);
            // if the service did not fire well looks like we have to go get the status.
            if (lResult == null)
            {
                lResult = JsonConvert.DeserializeObject<CurrencyStatusItem>(FServerAccess.GetLastCurrencyStatus(aCurrencyId), FConverter);
                //CurrencyStatusItem[] lList = GetCurrencyStatus(true);
                //foreach (CurrencyStatusItem lItem in lList)
                //    if (lItem.CurrencyId == aCurrencyId)
                //        lResult = lItem;
            }
            if (lResult == null)
                throw new Exception(string.Format("Unable to GetCurrencyStatus because currency ID {0} does not exist.", aCurrencyId));
            return lResult;
        }

        public CurrencyStatusItem[] GetCurrencyStatus(bool lForce = false)
        {
            return FPandoraCache.GetCurrencyStatuses(lForce);
        }

        public void ClearMemoryCache()
        {
            FPandoraCache.Clear();
        }

        public void RefreshData()
        {
            FPandoraCache.ForceDataFetch();
        }

        public UserStatus GetUserStatus()
        {
            return JsonConvert.DeserializeObject<UserStatus>(FServerAccess.GetUserStatus(), FConverter);
        }

        public ulong GetTransactionFee(uint aCurrencyId, CurrencyTransaction aCurrencyTransaction)
        {
            CurrencyItem lCurrency = GetCurrency(aCurrencyId);

            string aTxWithNoFee = CreateTransaction(aCurrencyId, aCurrencyTransaction);

            int lFeePerKb = lCurrency.FeePerKb;
            decimal lKBSize = ((decimal)aTxWithNoFee.Length / 2) / 1024;

            ulong lTxFee = Convert.ToUInt64(lKBSize * lFeePerKb);

            return lTxFee;
        }

        public TransactionRecord[] GetTransactions(uint aCurrencyId)
        {
            return FPandoraCache.GetTxs(aCurrencyId);
        }

        public TransactionRecord[] GetTransactions(uint aCurrencyId, string aAddress)
        {
            List<TransactionRecord> LTxs = FPandoraCache.GetTxs(aCurrencyId).ToList();

            return LTxs.Where(x => x.Inputs.ToList().Exists(y => y.Address == aAddress) || x.Outputs.ToList().Exists(y => y.Address == aAddress)).ToArray();
        }

        public bool Logoff()
        {
            FTxUpdateCancellationSource.Cancel();

            FCurrencyStatusTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            FCurrencyStatusTimer = null;

            FCurrencyItemTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            FCurrencyItemTimer = null;

            return FServerAccess.Logoff();
        }

        public bool CheckTransactionSent(long aSendTxHandle, out string TxID)
        {
            if (FServerAccess.IsTransactionSent(aSendTxHandle))
            {
                TxID = FServerAccess.GetTransactionId(aSendTxHandle);
                return true;
            }

            TxID = string.Empty;
            return false;
        }

        private void CurrencyStatusUpdatingTask(object state)
        {
            try
            {
                if (CheckforNewCurrencyStatus(out List<uint> ListofUpdatedCurrencies))
                {
                    NotifyNewCurrencyStatus(ListofUpdatedCurrencies);
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("PANDORAS SERVER - New CurrencyStatus found");
#endif
                }
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Error, "Error updating currency statuses. Details: {0}", ex);
            }
        }

        private async Task TxUpdatingTask(PandorasServer aPandorasServer, CancellationToken aCancellationToken)
        {
            while (!aCancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (aPandorasServer.CheckforNewTransactions(out List<uint> ListofUpdatedCurrencies))
                    {
                        aPandorasServer.NotifyNewTransaction(ListofUpdatedCurrencies);
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("PANDORAS SERVER - New Transaction found");
#endif
                        await Task.Delay(10000, aCancellationToken);
                    }

                    List<Tuple<uint, ulong>> lToUpdate = new List<Tuple<uint, ulong>>();

                    ulong lConfirmations = 0;

                    foreach (Tuple<uint, ulong> it in FToConfirm)
                    {
                        ulong lTxBlock = it.Item2;
                        if (lTxBlock > 0)
                            lConfirmations = FetchBlockHeight(it.Item1) - lTxBlock + 1;
                        else
                            lConfirmations = 0;

                        if (lConfirmations > 0)
                            lToUpdate.Add(it);
                    }

                    if (lToUpdate.Any())
                    {
                        lock (FToConfirm)
                            FToConfirm.RemoveAll(x => lToUpdate.Contains(x));

                        aPandorasServer.NotifyNewTransaction(lToUpdate.Select((x) => x.Item1).ToList(), true);
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("PANDORAS SERVER - Checking confirmations");
#endif
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(LogLevel.Error, "Error on fetching Transactions. Details: {0}", ex);
                }
                finally
                {
                    int i = 0;
                    while (i++ < 10 && !aCancellationToken.IsCancellationRequested) Thread.Sleep(500); // no more throwing exceptions
                }
            }

            aCancellationToken.ThrowIfCancellationRequested();
        }

        public ulong GetBlockHeight(uint aCurrencyID)
        {
            if (!FBlockHeights.Keys.Contains(aCurrencyID))
            {
                FBlockHeights[aCurrencyID] = FServerAccess.GetBlockHeight(aCurrencyID);
            }

            return FBlockHeights[aCurrencyID];
        }

        //TODO: Change this to work with Blockheights and transactions;

        private ulong FetchBlockHeight(uint aCurrencyID)
        {
            if (!FBlockHeights.Keys.Contains(aCurrencyID))
            {
                FBlockHeights.Add(aCurrencyID, 0);
            }

            FBlockHeights[aCurrencyID] = FServerAccess.GetBlockHeight(aCurrencyID);
            return FBlockHeights[aCurrencyID];
        }

        private void NotifyNewTransaction(List<uint> aListOfCurrencies, bool isConfirmationUpdate = false)
        {
            OnTransactions?.Invoke(aListOfCurrencies.ToArray(), isConfirmationUpdate);
        }

        private void NotifyNewCurrencyStatus(List<uint> aListOfCurrencies)
        {
            OnCurrencyStatus?.Invoke(aListOfCurrencies.ToArray());
        }

        public string DBFileName => FPandoraCache.DBFileName;

        public string AssemblyVersion
        {
            get
            {
                Assembly lAssembly = Assembly.GetExecutingAssembly();
                System.Diagnostics.FileVersionInfo lFileVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(lAssembly.Location);
                return lFileVersion.FileVersion;
            }
        }

        public bool Logon(string aEmail, string aUserName, string aPassword)
        {
            bool lResult = false;
            if (FServerAccess.Logon2(aEmail, aUserName, aPassword, Universal.SystemUtils.GetAssemblyVersion()))
            {
                string lUserInstanceData = string.Concat(Username, Email).ToLower();
                InstanceId = HashUtility.CreateMD5(lUserInstanceData);
                FPandoraCache = new PandorasCache(this);
                FPandoraCache.OnCacheExpired += OnCacheExpiredHandler;
                FTxUpdateCancellationSource = new CancellationTokenSource();
                lResult = true;
            }
            else
                lResult = false;

            return lResult;
        }

        public long SendTransaction(ulong aCurrencyId, string aSignedTxData)
        {
            return FServerAccess.SendTransaction(aCurrencyId, aSignedTxData);
        }

        private void OnCacheExpiredHandler(string aKey)
        {
            switch (aKey)
            {
                case ("ALL" + "CurrencyAccount"):
                    OnMonitoredAccountsFullListExpired();
                    break;

                case ("ALL" + "CurrencyStatusItem"):
                    OnCurrencyStatusFullListExpired();
                    break;

                case ("ALL" + "CurrencyItem"):
                    OnCurrencyFullListExpired();
                    break;

                default:
                    return;
            }
        }

        public void Dispose()
        {
            FTxUpdateCancellationSource?.Cancel();
            FPandoraCache?.Dispose();
            FServerAccess?.Dispose();
        }

        public static class HashUtility
        {
            public static string CreateMD5(string input)
            {
                // Use input string to calculate MD5 hash
                using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                {
                    byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                    byte[] hashBytes = md5.ComputeHash(inputBytes);

                    // Convert the byte array to hexadecimal string
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        sb.Append(hashBytes[i].ToString("X2"));
                    }
                    return sb.ToString();
                }
            }
        }

        internal CurrencyAccount[] GetMonitoredAccounts()
        {
            return FPandoraCache.GetMonitoredAccounts();
        }

        internal void AddMonitoredAccount(uint aCurrencyId, string aAddress)
        {
            FServerAccess.AddMonitoredAccount(aCurrencyId, aAddress);
        }

        internal void NewMonitoredAccountAdded(uint aCurrencyId)
        {
            FPandoraCache.NewMonitoredAccountAdded(aCurrencyId);
        }

        internal CurrencyAccount[] GetCurrencyAccount(uint aId)
        {
            return FPandoraCache.GetCurrencyAccount(aId);
        }
    }

    public class CurrencyAccountList : IEnumerable<CurrencyAccount>
    {
        private PandorasServer FPandoraServer;

        public CurrencyAccountList(PandorasServer aPandoraServer)
        {
            FPandoraServer = aPandoraServer;
        }

        public CurrencyAccount this[int i] => FPandoraServer.GetMonitoredAccounts()[i];

        public void AddCurrencyAccount(uint aCurrencyId, string aAddress)
        {
            FPandoraServer.AddMonitoredAccount(aCurrencyId, aAddress);
            FPandoraServer.NewMonitoredAccountAdded(aCurrencyId);
        }

        //#if DEBUG

        //        public bool RemoveMonitoredAddress(ulong aCurrencyId)
        //        {
        //            return FPandoraServer.FServerAccess.RemoveMonitoredAcccounts(aCurrencyId);
        //        }

        //#endif

        public IEnumerator<CurrencyAccount> GetEnumerator()
        {
            return FPandoraServer.GetMonitoredAccounts().Cast<CurrencyAccount>().GetEnumerator();
        }

        public int IndexOfAddress(uint aCurrencyId, string aAddress)
        {
            return Array.FindIndex(FPandoraServer.GetMonitoredAccounts(), (x) => (x.CurrencyId == aCurrencyId) && (x.Address == aAddress));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return FPandoraServer.GetMonitoredAccounts().GetEnumerator();
        }

        public CurrencyAccount[] GetById(uint aId)
        {
            return FPandoraServer.GetCurrencyAccount(aId);
        }
    }
}