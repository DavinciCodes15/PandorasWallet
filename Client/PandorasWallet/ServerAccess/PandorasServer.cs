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
#if DEBUG
#define TESTING
#endif

using Newtonsoft.Json;
using Pandora.Client.ClientLib;
using Pandora.Client.ServerAccess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.Client.PandorasWallet.ServerAccess
{
    public class PandorasServer : IPandoraServer
    {
        //TODO: STRINGS WILL BE JSON OBJECTS
        private PandoraWalletServiceAccess FServerAccess;

        private PandorasCache FPandoraCache;
        private string _datapath = string.Empty;
        private ClientJsonConverter FConverter;
        private CurrencyAccountList FCurrencyAccountList;

        public event Action OnCurrencyFullListExpired;

        public event Action OnCurrencyStatusFullListExpired;

        public event Action OnMonitoredAccountsFullListExpired;

        public CurrencyStatusEvent OnCurrencyStatus;

        private CancellationTokenSource FTxUpdateCancellationSource;
        private CancellationTokenSource FCurrencyStatusUpdateCancellationSource;

        private Task TxUpdating = null;

        public event TransactionEvent OnTransactions;

        public event Action<ulong> OnCurrencyItemUpdated;

        public event Func<ulong, bool> OnCurrencyItemMustUpdate;

        public List<long> CurrencyNumber { get; private set; }
        private Dictionary<ulong, ulong> FStatusNumber = new Dictionary<ulong, ulong>();
        private Dictionary<ulong, ulong> FCurrencyAccountCheckpointIds = new Dictionary<ulong, ulong>();
        private Dictionary<ulong, ulong> FTransactionCheckpointIds = new Dictionary<ulong, ulong>();
        private List<Tuple<uint, ulong>> FToConfirm;
        private Dictionary<uint, long> FBlockHeights = new Dictionary<uint, long>();
        private ulong FTemporalCurrencyCheckpoint = 0;
        private Task CurrencyStatusUpdating;

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

        public ICurrencyAccountList MonitoredAccounts => FCurrencyAccountList;

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
            return FPandoraCache.CheckforNewTransactions(this, out aListOfCurrencies);
        }

        private bool CheckforNewCurrencyStatus(out List<uint> aListOfCurrencies)
        {
            return FPandoraCache.CheckforNewCurrencyStatus(this, out aListOfCurrencies);
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
            return JsonConvert.DeserializeObject<byte[]>(FServerAccess.GetCurrencyIcon(aCurrencyId));
        }

        public List<CurrencyItem> FetchCurrencies()

        {
            List<long> JsonListOfCurrencies;

            List<CurrencyItem> ReturningList = new List<CurrencyItem>();

            do
            {
                uint lCurrencyPoint = (CurrencyNumber.Any() ? (uint)CurrencyNumber.Max() : 0);
                uint lPoint = FTemporalCurrencyCheckpoint < lCurrencyPoint && FTemporalCurrencyCheckpoint > 0 ? (uint)FTemporalCurrencyCheckpoint : lCurrencyPoint;

                JsonListOfCurrencies = JsonConvert.DeserializeObject<List<long>>(FServerAccess.GetCurrencyList(lPoint), FConverter);

                if (JsonListOfCurrencies.Any())
                {
                    foreach (long it in JsonListOfCurrencies)
                    {
                        if (!CurrencyNumber.Contains(it))
                        {
                            CurrencyNumber.Add(it);
                        }
                    }

                    if (FTemporalCurrencyCheckpoint > 0)
                    {
                        FTemporalCurrencyCheckpoint = (ulong)JsonListOfCurrencies.Max();
                    }
                }

                foreach (long it in JsonListOfCurrencies)
                {
                    ReturningList.Add(JsonConvert.DeserializeObject<CurrencyItem>(FServerAccess.GetCurrency((uint)it), FConverter));
                }
            } while (JsonListOfCurrencies.Any());

            if (FTemporalCurrencyCheckpoint > 0)
            {
                FTemporalCurrencyCheckpoint = 0;
            }

            return ReturningList;
        }

        public List<CurrencyStatusItem> FetchCurrencyStatus(uint aId)

        {
            if (!CurrencyNumber.Contains(aId))
            {
                throw new Exception("Id out of range");
            }

            if (!FStatusNumber.Keys.Contains(aId))
            {
                FStatusNumber.Add(aId, 0);
            }

            List<CurrencyStatusItem> Returninglist = new List<CurrencyStatusItem>();
            List<CurrencyStatusItem> JsonListOfStatuses;

            do
            {
                JsonListOfStatuses = JsonConvert.DeserializeObject<List<CurrencyStatusItem>>(FServerAccess.GetCurrencyStatusList(aId, FStatusNumber[aId]), FConverter);
                if (JsonListOfStatuses.Any())
                {
                    FStatusNumber[aId] = (ulong)JsonListOfStatuses.Max(x => x.StatusId);
                }
                Returninglist.AddRange(JsonListOfStatuses);
            } while (JsonListOfStatuses.Any());

            ulong lCurrencyID = Returninglist.Where(x => x.Status == CurrencyStatus.Updated).Select(x => x.CurrencyId).DefaultIfEmpty().Min();

            if (lCurrencyID != 0)
            {
                CurrencyStatusUpdated(lCurrencyID);
                Returninglist.RemoveAll(x => x.Status == CurrencyStatus.Updated);
            }
            else
            {
                FTemporalCurrencyCheckpoint = 0;
            }
            if (Returninglist.Any())
            {
                Returninglist = Returninglist.OrderBy((x) => x.StatusId).ToList();

                if (lCurrencyID != 0)
                {
                    CurrencyStatusItem lLastObject = Returninglist[Returninglist.Count - 1];

                    if ((ulong)lLastObject.StatusId != FStatusNumber[aId])
                    {
                        Returninglist[Returninglist.Count - 1] = new CurrencyStatusItem((long)FStatusNumber[aId], lLastObject.CurrencyId, lLastObject.StatusTime, lLastObject.Status, lLastObject.ExtendedInfo, lLastObject.BlockHeight);
                    }
                }
            }

            return Returninglist;
        }

        private void CurrencyStatusUpdated(ulong aCurrencyId)
        {
            FTemporalCurrencyCheckpoint = aCurrencyId - 1;
            bool? lReturn = OnCurrencyItemMustUpdate?.Invoke(aCurrencyId);

            if (lReturn.HasValue && lReturn.Value)
            {
                OnCurrencyItemUpdated?.Invoke(aCurrencyId);
            }
        }

        public List<CurrencyAccount> FetchMonitoredAccounts(ulong aId)

        {
            if (!CurrencyNumber.Contains((long)aId))
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

        public List<TransactionRecord> FetchTransactions(ulong aId)

        {
            if (!CurrencyNumber.Contains((long)aId))
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
            CurrencyNumber = aCheckpoints["Currencies"].ToList();

            if (aCheckpoints["CurrenciesStatus"].Any())
            {
                foreach (long it in CurrencyNumber)
                {
                    int index = aCheckpoints["CurrenciesStatusIndex"].ToList().IndexOf(it);
                    FStatusNumber[Convert.ToUInt64(it)] = index != -1 ? Convert.ToUInt64(aCheckpoints["CurrenciesStatus"][index]) : 0;
                }
            }

            if (aCheckpoints["MonitoredAccounts"].Any())
            {
                foreach (long it in CurrencyNumber)
                {
                    int index = aCheckpoints["MonitoredAccountsIndex"].ToList().IndexOf(it);
                    FCurrencyAccountCheckpointIds[Convert.ToUInt64(it)] = index != -1 ? Convert.ToUInt64(aCheckpoints["MonitoredAccounts"][index]) : 0;
                }
            }

            if (aCheckpoints["Tx"].Any())
            {
                foreach (long it in CurrencyNumber)
                {
                    int index = aCheckpoints["TxIndex"].ToList().IndexOf(it);
                    FTransactionCheckpointIds[Convert.ToUInt64(it)] = index != -1 ? Convert.ToUInt64(aCheckpoints["Tx"][index]) : 0;
                }
            }
        }

        public void StartTxUpdatingTask()
        {
            if ((TxUpdating == null || TxUpdating.IsCanceled) && CurrencyNumber.Any())
            {
                TxUpdating = TxUpdatingTask(this, FCurrencyStatusUpdateCancellationSource.Token); //Start fetching transactions now that I have currencies to work with
            }
        }

        public void StartCurrencyStatusUpdatingTask()
        {
            if ((CurrencyStatusUpdating == null || CurrencyStatusUpdating.IsCanceled) && CurrencyNumber.Any())
            {
                CurrencyStatusUpdating = Task.Run(() => CurrencyStatusUpdatingTask(this, FTxUpdateCancellationSource.Token)); //Start fetching statuses
            }
        }

        public string CreateTransaction(uint aCurrencyID, CurrencyTransaction aSendTx)
        {
            return FServerAccess.CreateTransaction(aSendTx);
        }

        public CurrencyItem GetCurrency(uint aCurrencyId)
        {
            return FPandoraCache.GetCurrencyByID(aCurrencyId);
        }

        public byte[] GetCurrencyIcon(uint aCurrencyId)
        {
            //TODO:LUIS- Overwrite icon in database when this is called
            return FPandoraCache.GetCurrencyByID(aCurrencyId).Icon;
        }

        public CurrencyItem[] GetCurrencyList()
        {
            return FPandoraCache.GetAllCurrencies();
        }

        public CurrencyStatusItem GetCurrencyStatus(uint aCurrencyId)
        {
            return FPandoraCache.GetCurrencyStatusByID(aCurrencyId);
        }

        public CurrencyStatusItem[] GetCurrencyStatus()
        {
            return FPandoraCache.GetAllCurrencyStatuses();
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
            FCurrencyStatusUpdateCancellationSource.Cancel();

            //TODO:LUIS- CLOSE STUFF BEFORE LOGGIN OFF
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

        private async Task CurrencyStatusUpdatingTask(PandorasServer aPandorasServer, CancellationToken aCancellationToken)
        {
            while (!aCancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (aPandorasServer.CheckforNewCurrencyStatus(out List<uint> ListofUpdatedCurrencies))
                    {
                        aPandorasServer.NotifyNewCurrencyStatus(ListofUpdatedCurrencies);
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("PANDORAS SERVER - New CurrencyStatus found");
#endif
                        await Task.Delay(1000, aCancellationToken);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Utils.PandoraLog.GetPandoraLog().Write("Error on fetching Transactions. Details: " + ex.Message + " on " + ex.Source);
                }
                finally
                {
                    await Task.Delay(30000, aCancellationToken);
                }
            }

            aCancellationToken.ThrowIfCancellationRequested();
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
                        //continue;
                    }

                    List<Tuple<uint, ulong>> lToUpdate = new List<Tuple<uint, ulong>>();

                    long lConfirmations = 0;

                    foreach (Tuple<uint, ulong> it in FToConfirm)
                    {
                        long lTxBlock = (long)it.Item2;
                        if (lTxBlock > 0)
                        {
                            lConfirmations = FetchBlockHeight(it.Item1) - lTxBlock + 1;
                        }
                        else
                        {
                            lConfirmations = 0;
                        }

                        if (lConfirmations > 0)
                        {
                            lToUpdate.Add(it);
                        }
                    }

                    if (lToUpdate.Any())
                    {
                        lock (FToConfirm)
                        {
                            FToConfirm.RemoveAll(x => lToUpdate.Contains(x));
                        }

                        aPandorasServer.NotifyNewTransaction(lToUpdate.Select((x) => x.Item1).ToList(), true);
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("PANDORAS SERVER - Checking confirmations");
#endif
                    }
                }
                catch (Exception ex)
                {
                    Utils.PandoraLog.GetPandoraLog().Write("Error on fetching Transactions. Details: " + ex.Message + " on " + ex.Source);
                }
                finally
                {
                    await Task.Delay(5000, aCancellationToken);
                }
            }

            aCancellationToken.ThrowIfCancellationRequested();
        }

        public long GetBlockHeight(uint aCurrencyID)
        {
            if (!FBlockHeights.Keys.Contains(aCurrencyID))
            {
                FBlockHeights[aCurrencyID] = FServerAccess.GetBlockHeight(aCurrencyID);
            }

            return FBlockHeights[aCurrencyID];
        }

        //TODO: Change this to work with Blockheights and transactions;

        private long FetchBlockHeight(uint aCurrencyID)
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

        public bool Logon(string aEmail, string aUserName, string aPassword)
        {
            if (FServerAccess.Logon(aEmail, aUserName, aPassword))
            {
                InstanceId = HashUtility.CreateMD5(Username + Email);
                FPandoraCache = new PandorasCache(this);
                FPandoraCache.OnCacheExpired += OnCacheExpiredHandler;
                FTxUpdateCancellationSource = new CancellationTokenSource();
                FCurrencyStatusUpdateCancellationSource = new CancellationTokenSource();

                return true;
            }
            else
            {
                return false;
            }
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

        private class ClientJsonConverter : PandoraJsonConverter
        {
            private PandorasServer FPandoraServer;

            public ClientJsonConverter(PandorasServer aPandoraServer)
            {
                FPandoraServer = aPandoraServer;
                CreateConvertionEspecifications();
            }

            //protected override byte[] GetIcon(JObject aItem, JsonSerializer aSerializer)
            //{
            //    return FPandoraServer.FetchCurrencyIcon(aItem["Id"].Value<uint>());
            //}
        }

        private class CurrencyAccountList : ICurrencyAccountList
        {
            private PandorasServer FPandoraServer;

            public CurrencyAccountList(PandorasServer aPandoraServer)
            {
                FPandoraServer = aPandoraServer;
            }

            public CurrencyAccount this[int i] => FPandoraServer.FPandoraCache.GetAllMonitoredAccounts()[i];

            public void AddCurrencyAccount(uint aCurrencyId, string aAddress)
            {
                FPandoraServer.FServerAccess.AddMonitoredAccount(aCurrencyId, aAddress);
                FPandoraServer.FPandoraCache.NewMonitoredAccountAdded(aCurrencyId);
            }

#if DEBUG

            public bool RemoveMonitoredAddress(ulong aCurrencyId)
            {
                return FPandoraServer.FServerAccess.RemoveMonitoredAcccounts(aCurrencyId);
            }

#endif

            public IEnumerator<CurrencyAccount> GetEnumerator()
            {
                return FPandoraServer.FPandoraCache.GetAllMonitoredAccounts().GetEnumerator();
            }

            public int IndexOfAddress(uint aCurrencyId, string aAddress)
            {
                return FPandoraServer.FPandoraCache.GetAllMonitoredAccounts().FindIndex((x) => (x.CurrencyId == aCurrencyId) && (x.Address == aAddress));

                //TODO:LUIS-Maybe we need to change this in the future to make it work correctly
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return FPandoraServer.FPandoraCache.GetAllMonitoredAccounts().GetEnumerator();
            }

            public CurrencyAccount[] GetById(uint aId)
            {
                return FPandoraServer.FPandoraCache.GetMonitoredAccountByID(aId);
            }
        }
    }
}