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
using Newtonsoft.Json.Linq;
using Pandora.Client.ClientLib;
using Pandora.Client.PandorasWallet.Models;
using Pandora.Client.PandorasWallet.Wallet;
using Pandora.Client.ServerAccess;
using Pandora.Client.Universal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pandora.Client.PandorasWallet.ServerAccess
{
    public class ServerConnection : IDisposable
    {
        private PandoraWalletServiceAccess FPandoraWalletServiceAccess;
        private PandoraObjectNotifier FPandoraObjectNotifier;
        private PandoraJsonConverter FConverter = new PandoraJsonConverter();
        private LocalCacheDB FLocalCacheDB;
        private List<string> FInternalErrors = new List<string>();
        private UserStatus FCurrentStatus;

        public ServerConnection(string aDataPath, System.ComponentModel.ISynchronizeInvoke aSynchronizingObject)
        {
            DataPath = aDataPath;
            SynchronizingObject = aSynchronizingObject;
            CacheHelper = new CacheServerAccessHelper(this);
        }

        public static void RestoreLocalCasheDB(string aRestoreDBFile, ServerConnection aServerConnection)
        {
            string lDestFile = Path.Combine(aServerConnection.DataPath, $"{aServerConnection.InstanceId}.sqlite");
            aServerConnection.FLocalCacheDB.Dispose();
            aServerConnection.FLocalCacheDB = null;
            try
            {
                File.Copy(aRestoreDBFile, lDestFile);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}{Environment.NewLine} Unable to copy '{aRestoreDBFile}' to '{lDestFile}'.");
            }
            aServerConnection.FLocalCacheDB = new LocalCacheDB(lDestFile);
        }

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
        /// Fired if a new transaction occurs on monitored coins
        /// </summary>
        public event DelegateOnTransaction OnNewTransaction;

        /// <summary>
        /// If the transaction changes in any date, block, TX hash, this will fire.
        /// You must assume that all values in the TransactionRecord has changed and update
        /// the cache
        /// </summary>
        public event DelegateOnTransaction OnUpdatedTransaction;

        /// <summary>
        /// If there is a change on the chain params parameter after receiving an update of a currency, this event will fire.
        /// </summary>
        public event DelegateOnUpdatedCurrencyParams OnUpdatedCurrencyParams;

        public event DelegateOnUpgradeFileReady OnUpgradeFileReady;

        public CacheServerAccessHelper CacheHelper { get; private set; }

        public bool Connected { get => FPandoraWalletServiceAccess != null; }

        public string InstanceId { get; private set; }

        public string UserName => FPandoraWalletServiceAccess.UserName;

        public string SqLiteDbFileName { get; private set; }

        public string Email => FPandoraWalletServiceAccess.Email;

        public string DataPath { get; private set; }

        public string[] Errors { get => FInternalErrors.ToArray(); }

        public bool NewAccount { get; private set; }

        public string[] DefaultBitcoinAddress { get; private set; }

        public CurrencyItem DefualtCurrencyItem { get; private set; }

        public string UpgradeFileName { get => FPandoraObjectNotifier == null ? null : FPandoraObjectNotifier.UpgradeFileName; }

        public bool AutoUpdate { get; set; }

        public System.ComponentModel.ISynchronizeInvoke SynchronizingObject { get; private set; }

        public void ClearErrors()
        {
            FInternalErrors.Clear();
        }

        public bool LogIn(string aRemoteserver, int aPort, bool aEncryptedConnection, string aEmail, string aUserName, string aPassword)
        {
            bool lResult = false;
            NewAccount = false;
            aEmail = aEmail.ToLower();
            aUserName = aUserName.ToLower();
            var lServerAccess = new PandoraWalletServiceAccess(aRemoteserver, aPort, aEncryptedConnection);
            try
            {
                if (lResult = lServerAccess.Logon(aEmail, aUserName, aPassword, SystemUtils.GetAssemblyVersion()))
                {
                    var lResultStatus = lServerAccess.GetUserStatus();

                    JObject lResultStatusDynamic = JObject.Parse(lResultStatus);

                    if (!lResultStatusDynamic.Value<bool>("Active"))
                    {
                        throw new ClientExceptions.LoginFailedException($"The user is not active {Environment.NewLine}{Environment.NewLine} Email: {aEmail} {Environment.NewLine} User: {aUserName} ");
                    }

                    ConnectToDatabase(lServerAccess);
                    FPandoraWalletServiceAccess = lServerAccess;
                }
                else
                    lServerAccess.Dispose();
            }
            catch
            {
                lServerAccess.Dispose();
                throw;
            }
            return lResult;
        }

        private void ConnectToDatabase(PandoraWalletServiceAccess aPandoraWalletServiceAccess)
        {
            var lCurrencyAccounts = JsonConvert.DeserializeObject<List<CurrencyAccount>>(aPandoraWalletServiceAccess.GetMonitoredAcccounts(1, 0), FConverter);
            FCurrentStatus = JsonConvert.DeserializeObject<UserStatus>(aPandoraWalletServiceAccess.GetUserStatus(), FConverter);
            NewAccount = lCurrencyAccounts.Count == 0;
            if (!NewAccount)
            {
                DefaultBitcoinAddress = new string[2];
                DefaultBitcoinAddress[0] = lCurrencyAccounts[0].Address;
                if (lCurrencyAccounts.Count > 1)
                    DefaultBitcoinAddress[1] = lCurrencyAccounts[1].Address;
            }
            InstanceId = KeyManager.CreateInstanceId(aPandoraWalletServiceAccess.Email, aPandoraWalletServiceAccess.UserName);

            SqLiteDbFileName = Path.Combine(DataPath, $"{InstanceId}.sqlite");
            FLocalCacheDB = new LocalCacheDB(SqLiteDbFileName);
            try
            {
                var lFileName = Path.Combine(DataPath, $"{SqLiteDbFileName}={aPandoraWalletServiceAccess.Email}-{aPandoraWalletServiceAccess.UserName}.txt");
                if (!File.Exists(lFileName))
                    File.WriteAllText(lFileName, $"Server={aPandoraWalletServiceAccess.RemoteServer}\r\nPort={aPandoraWalletServiceAccess.Port}\r\n{aPandoraWalletServiceAccess.Email}\r\n{aPandoraWalletServiceAccess.UserName}");
            }
            catch
            { //not critical just for info when looking for what files belong to what user without decoding it.
            }

            CreatePandoraObjectNotifier(aPandoraWalletServiceAccess);
        }

        internal void DirectSendNewTransaction(string aTransactionData, long aCurrencyId, DelegateOnSendTransactionCompleted aEvent)
        {
            FPandoraObjectNotifier.SendNewTransaction(aTransactionData, aCurrencyId, aEvent);
        }

        public bool ValidateDefaultAddressExists(string aAddress)
        {
            if (aAddress == null) throw new ArgumentNullException("The address must not be null.");
            if (DefaultBitcoinAddress == null) throw new InvalidOperationException("The Default Address was not set.");
            bool lAddressExists = false;
            foreach (var lDefaultAddress in DefaultBitcoinAddress)
                lAddressExists = lAddressExists || lDefaultAddress == aAddress;
            if (!lAddressExists)
                Log.Write(LogLevel.Critical, "Local Address = {0},{1}\nRemote Address = {2}", DefaultBitcoinAddress[0], DefaultBitcoinAddress[1], aAddress);
            return lAddressExists;
        }

        private void CreatePandoraObjectNotifier(PandoraWalletServiceAccess aServerAccess)
        {
            FPandoraObjectNotifier = new PandoraObjectNotifier(aServerAccess.RemoteServer, aServerAccess.Port, aServerAccess.EncryptedConnection, aServerAccess.ConnectionId, DataPath);
            try
            {
                FPandoraObjectNotifier.AutoUpdate = AutoUpdate;
                FPandoraObjectNotifier.SynchronizingObject = SynchronizingObject;
                FPandoraObjectNotifier.OnBlockHeightChange += PandoraObjectNotifier_OnBlockHeightChange;
                FPandoraObjectNotifier.OnCurrencyStatusChange += PandoraObjectNotifier_OnCurrencyStatusChange;
                FPandoraObjectNotifier.OnErrorEvent += PandoraObjectNotifier_OnErrorEvent;
                FPandoraObjectNotifier.OnNewCurrency += PandoraObjectNotifier_OnNewCurrency;
                FPandoraObjectNotifier.OnNewTransaction += PandoraObjectNotifier_OnNewTransaction;
                FPandoraObjectNotifier.OnUpdatedCurrency += PandoraObjectNotifier_OnUpdatedCurrency;
                FPandoraObjectNotifier.OnUpdatedTransaction += PandoraObjectNotifier_OnUpdatedTransaction;
                FPandoraObjectNotifier.OnSentTransaction += PandoraObjectNotifier_OnSentTransaction;
                FPandoraObjectNotifier.OnUpgradeFileReady += PandoraObjectNotifier_OnUpgradeFileReady;
                if (!NewAccount)
                {
                    FLocalCacheDB.ReadCurrencies(out List<CurrencyItem> lCurrencies);
                    // adding currencies we own in the database
                    // if there is new ones beyond that we will be notified
                    foreach (var lCurrency in lCurrencies)
                    {
                        long lCurrencyId = lCurrency.Id;
                        var lCurrencyStatusItem = FLocalCacheDB.ReadCurrencyStatus(lCurrencyId);
                        if (lCurrencyStatusItem != null)
                            FPandoraObjectNotifier.AddExistingCurrency(lCurrency, lCurrencyStatusItem);

                        //If the currency is being monitored (selected as a currency)
                        List<CurrencyAccount> lCurrencyAccountList = FLocalCacheDB.ReadMonitoredAccounts(lCurrencyId);
                        if (lCurrencyAccountList.Any())
                        {
                            FPandoraObjectNotifier.AddCurrencyToBeNotified(lCurrencyId);
                            long lStartBlock = FLocalCacheDB.ReadBlockHeight(lCurrencyId) - lCurrency.MinConfirmations;
                            var lTransactionRecords = FLocalCacheDB.ReadTransactionRecords(lCurrencyId, lStartBlock);
                            Log.Write(LogLevel.Debug, "For currency '{0}' ID:{1} found {2} unconfirmed.", lCurrency.Name, lCurrencyId, lTransactionRecords.Count);

                            if (!lTransactionRecords.Any())
                            {
                                var lTx = FLocalCacheDB.ReadLastTransactionRecord(lCurrencyId);
                                if (lTx != null)
                                {
                                    Log.Write(LogLevel.Debug, "Found a last transaction record ID:{0} looking for new ones.", lTx.TransactionRecordId);
                                    lTransactionRecords.Add(lTx);
                                }
                                else
                                    Log.Write(LogLevel.Debug, "Found no transaction records.", lCurrency.Name, lCurrencyId, lTransactionRecords.Count);
                            }
                            FPandoraObjectNotifier.AddUnconfirmedTransactions(lCurrencyId, lTransactionRecords.ToArray());
                        }
                    }
                }
            }
            catch
            {
                FPandoraObjectNotifier.Dispose();
                throw;
            }
            FPandoraObjectNotifier.Run();
        }

        private void PandoraObjectNotifier_OnUpgradeFileReady(object aSender, string aFileName)
        {
            Log.Write(LogLevel.Debug, "Upgrade file ready {0}", aFileName);
            if (AutoUpdate)
                OnUpgradeFileReady?.Invoke(this, aFileName);
        }

        private void PandoraObjectNotifier_OnSentTransaction(object aSender, string aTxData, string aTxId, long aCurrencyId, DateTime aStartTime, DateTime aEndTime)
        {
            if (FLocalCacheDB == null) return; // Note: if the thread sent out this message before the object was terminated
            Log.Write(LogLevel.Debug, "Transaction Sent with TxID {0} - CurrencyId: {1}", aTxId, aCurrencyId);
            FLocalCacheDB.WriteTransactionSentData(aTxData, aTxId, aCurrencyId, aStartTime, aEndTime);
        }

        private void PandoraObjectNotifier_OnUpdatedTransaction(object aSender, TransactionRecord aTransactionRecord, ClientTokenTransactionItem aTokenTransactionItem)
        {
            if (FLocalCacheDB == null) return; // Note: if the thread sent out this message before the object was terminated
            Log.Write(LogLevel.Debug, "Updated Transaction TxId {0}\r\n        ID: {1}, bock: {2}, Valid: {3}", aTransactionRecord.TxId, aTransactionRecord.TransactionRecordId, aTransactionRecord.Block, aTransactionRecord.Valid);
            FLocalCacheDB.Write(aTransactionRecord);
            OnUpdatedTransaction?.Invoke(this, aTransactionRecord);
        }

        private void PandoraObjectNotifier_OnUpdatedCurrency(object aSender, CurrencyItem aCurrencyItem)
        {
            if (FLocalCacheDB == null) return; // Note: if the thread sent out this message before the object was terminated
            Log.Write(LogLevel.Debug, "Updated currency writen {0}  ID: {1} ", aCurrencyItem.Name, aCurrencyItem.Id);
            //Intercept
            var lOldChainParams = GetCurrency(aCurrencyItem.Id)?.ChainParamaters;
            if (lOldChainParams != null && lOldChainParams.Version < aCurrencyItem.ChainParamaters.Version)
                OnUpdatedCurrencyParams?.Invoke(aCurrencyItem, lOldChainParams);
            FLocalCacheDB.Write(aCurrencyItem);
            OnUpdatedCurrency?.Invoke(this, aCurrencyItem);
        }

        private void PandoraObjectNotifier_OnNewTransaction(object aSender, TransactionRecord aTransactionRecord, ClientTokenTransactionItem aTokenTransaction)
        {
            if (FLocalCacheDB == null) return; // Note: if the thread sent out this message before the object was terminated
            if (aTokenTransaction != null)
            {
                Log.Write(LogLevel.Debug, "*Writing NEW TokenTx for txid {0}\r\n        ID: {1}, Block: {2}, Valid: {3}, CurrencyId:{4}, TokenAddress: {5}", aTransactionRecord.TxId, aTransactionRecord.TransactionRecordId, aTransactionRecord.Block, aTransactionRecord.Valid, aTransactionRecord.CurrencyId, aTokenTransaction.TokenAddress);
                FLocalCacheDB.Write(aTokenTransaction);
            }
            Log.Write(LogLevel.Debug, "*Writing NEW TXid {0}\r\n        ID: {1}, Block: {2}, Valid: {3}, CurrencyId:{4}", aTransactionRecord.TxId, aTransactionRecord.TransactionRecordId, aTransactionRecord.Block, aTransactionRecord.Valid, aTransactionRecord.CurrencyId);
            FLocalCacheDB.Write(aTransactionRecord);
            OnNewTransaction?.Invoke(this, aTransactionRecord, aTokenTransaction);
        }

        private void PandoraObjectNotifier_OnNewCurrency(object aSender, CurrencyItem aCurrencyItem)
        {
            if (FLocalCacheDB == null) return; // Note: if the thread sent out this message before the object was terminated
            Log.Write(LogLevel.Debug, "*Writing NEW currency {0}  ID: {1}", aCurrencyItem.Name, aCurrencyItem.Id);
            FLocalCacheDB.Write(aCurrencyItem);
            OnNewCurrency?.Invoke(this, aCurrencyItem);
        }

        private void PandoraObjectNotifier_OnErrorEvent(object sender, Exception e, ref bool aIsHandled)
        {
            if (FLocalCacheDB == null) return; // Note: if the thread sent out this message before the object was terminated
            FInternalErrors.Add($"ObjectNotifier: {e.Message}\r\n{e.Source}");
            Log.Write(LogLevel.Error, FInternalErrors.Last());
            aIsHandled = true;
        }

        private void PandoraObjectNotifier_OnCurrencyStatusChange(object aSender, CurrencyStatusItem aCurrencyStatusItem)
        {
            if (FLocalCacheDB == null) return; // Note: if the thread sent out this message before the object was terminated
            Log.Write(LogLevel.Debug, "Updated Status of currency writen {0}  ID: {1} ", aCurrencyStatusItem.Status, aCurrencyStatusItem.StatusId);
            FLocalCacheDB.Write(aCurrencyStatusItem);
            OnCurrencyStatusChange?.Invoke(this, aCurrencyStatusItem);
        }

        private void PandoraObjectNotifier_OnBlockHeightChange(object aSender, long aCurrencyId, long aBlockHeight)
        {
            if (FLocalCacheDB == null) return; // Note: if the thread sent out this message before the object was terminated
            Log.Write(LogLevel.Debug, "Updated block {0} ID: {1} ", aBlockHeight, aCurrencyId);
            FLocalCacheDB.WriteBlockHeight(aCurrencyId, aBlockHeight);
            OnBlockHeightChange?.Invoke(this, aCurrencyId, aBlockHeight);
        }

        public bool DirectCheckAddress(long aCurrencyId, string aAddress)
        {
            CheckConnected();
            return FPandoraWalletServiceAccess.CheckAddress(aCurrencyId, aAddress);
        }

        public List<CurrencyAccount> DirectGetMonitoredAcccounts(long aCurrencyId, long aStartAddressId)
        {
            CheckConnected();
            Log.Write(LogLevel.Debug, "A call to DirectGetMonitoredAcccounts");
            return JsonConvert.DeserializeObject<List<CurrencyAccount>>(FPandoraWalletServiceAccess.GetMonitoredAcccounts(aCurrencyId, aStartAddressId));
        }

        public UserStatus DirectGetUserStatus()
        {
            CheckConnected();
            Log.Write(LogLevel.Debug, "A call to DirectGetUserStatus");
            FCurrentStatus = JsonConvert.DeserializeObject<UserStatus>(FPandoraWalletServiceAccess.GetUserStatus(), FConverter);
            return FCurrentStatus;
        }

        public CurrencyItem DirectGetCurrency(long aCurrencyId)
        {
            CheckConnected();
            Log.Write(LogLevel.Warning, "A direct call to GetCurrency should not be made.");
            return JsonConvert.DeserializeObject<CurrencyItem>(FPandoraWalletServiceAccess.GetCurrency(aCurrencyId), FConverter);
        }

        public ClientCurrencyTokenItem DirectGetCurrencyToken(long aCurrencyID, string aAddress)
        {
            CheckConnected();
            return JsonConvert.DeserializeObject<ClientCurrencyTokenItem>(FPandoraWalletServiceAccess.GetCurrencyToken(aCurrencyID, aAddress));
        }

        private byte[] DirectGetCurrencyIcon(long aCurrencyId)
        {
            CheckConnected();
            Log.Write(LogLevel.Warning, "A direct call to GetCurrencyIcon should not be made.");
            return JsonConvert.DeserializeObject<byte[]>(FPandoraWalletServiceAccess.GetCurrencyIcon(aCurrencyId), FConverter);
        }

        public CurrencyStatusItem DirectGetCurrencyStatus(long aCurrencyId)
        {
            CheckConnected();
            Log.Write(LogLevel.Warning, "A direct call to DirectGetCurrencyStatus should not be made.");
            return JsonConvert.DeserializeObject<CurrencyStatusItem>(FPandoraWalletServiceAccess.GetLastCurrencyStatus(aCurrencyId), FConverter);
        }

        public IEnumerable<TransactionRecord> DirectGetTransactionRecords(long aCurrencyId, long aStartTxRecordId)
        {
            CheckConnected();
            Log.Write(LogLevel.Warning, "A direct call to DirectGetCurrencyStatus should not be made.");
            return JsonConvert.DeserializeObject<List<TransactionRecord>>(FPandoraWalletServiceAccess.GetTransactionRecords(aCurrencyId, aStartTxRecordId), FConverter);
        }

        public string DirectCreateTransaction(CurrencyTransaction aSendTx)
        {
            CheckConnected();
            return FPandoraWalletServiceAccess.CreateTransaction(aSendTx);
        }

        internal void SetDefaultCurrency(long aCurrencyId)
        {
            DefualtCurrencyItem = GetCurrency(aCurrencyId);
            FLocalCacheDB.WritePrimaryCurrencyId(aCurrencyId);
        }

        public void Dispose()
        {
            Logoff();
        }

        private void CheckConnected()
        {
            if (!Connected)
                throw new InvalidOperationException("Control is not connected.");
        }

        public UserStatus GetUserStatus()
        {
            CheckConnected();
            return FCurrentStatus;
        }

        /// <summary>
        /// Gets currencies from the Cache new currencies that are later
        /// added to the case will be sent to you via the OnNewCurrency event
        /// and should be hooked before this request.
        /// </summary>
        /// <returns></returns>
        public List<CurrencyItem> GetCurrencies()
        {
            CheckConnected();
            FLocalCacheDB.ReadCurrencies(out List<CurrencyItem> lReturningList);
            return lReturningList;
        }

        public IEnumerable<ClientCurrencyTokenItem> GetCurrencyTokens()
        {
            CheckConnected();
            IEnumerable<IClientCurrencyToken> lTokens = FLocalCacheDB.ReadCurrencyTokens();
            return lTokens.Select(lToken => new ClientCurrencyTokenItem(lToken));
        }

        public ClientCurrencyTokenItem GetCurrencyToken(string aTokenAddress)
        {
            CheckConnected();
            return FLocalCacheDB.ReadCurrencyTokens(aTokenAddress).Select(lToken => new ClientCurrencyTokenItem(lToken)).FirstOrDefault();
        }

        public ClientCurrencyTokenItem GetCurrencyToken(long aTokenID)
        {
            CheckConnected();
            return FLocalCacheDB.ReadCurrencyTokens(aTokenID: aTokenID).Select(lToken => new ClientCurrencyTokenItem(lToken)).FirstOrDefault();
        }

        public void RegisterNewCurrencyToken(ClientCurrencyTokenItem aCurrencyToken)
        {
            CheckConnected();
            if (aCurrencyToken == null) throw new ArgumentNullException(nameof(aCurrencyToken), "Currency token can't be null");
            FLocalCacheDB.Write(aCurrencyToken);
        }

        public CurrencyItem GetCurrency(long aCurrencyId)
        {
            CheckConnected();
            return FLocalCacheDB.ReadCurrency(aCurrencyId);
        }

        public CurrencyStatusItem GetCurrencyStatus(long aCurrencyId)
        {
            CheckConnected();
            return FLocalCacheDB.ReadCurrencyStatus(aCurrencyId);
        }

        public List<CurrencyAccount> GetMonitoredAccounts(long aCurrencyId)
        {
            CheckConnected();
            return FLocalCacheDB.ReadMonitoredAccounts(aCurrencyId);
        }

        public List<string> GetMonitoredAddresses(long aCurrencyId)
        {
            var lResult = new List<string>();
            foreach (var lAccount in GetMonitoredAccounts(aCurrencyId))
                lResult.Add(lAccount.Address);
            return lResult;
        }

        public List<CurrencyItem> GetDisplayedCurrencies()
        {
            CheckConnected();
            return FLocalCacheDB.ReadDisplayedCurrencies();
        }

        public IEnumerable<ClientCurrencyTokenItem> GetDisplayedCurrencyTokens()
        {
            CheckConnected();
            return FLocalCacheDB.ReadDisplayedTokens().Select(lToken => new ClientCurrencyTokenItem(lToken));
        }

        public void SetDisplayedCurrencyToken(long aTokenID, bool aDisplayed)
        {
            CheckConnected();
            FLocalCacheDB.WriteDisplayedTokenId(aTokenID, aDisplayed);
        }

        public void SetDisplayedCurrency(long aCurrencyId, bool aDisplayed)
        {
            CheckConnected();
            FLocalCacheDB.WriteDisplayedCurrencyId(aCurrencyId, aDisplayed);
            if (FPandoraObjectNotifier == null) Log.Write(LogLevel.Warning, "ObjectNotifier is null at set display currency.");
            FPandoraObjectNotifier?.AddCurrencyToBeNotified(aCurrencyId);
        }

        public CurrencyItem GetDefaultCurrency()
        {
            if (DefualtCurrencyItem == null)
            {
                var lId = FLocalCacheDB.ReadPrimaryCurrencyId();
                if (lId != 0)
                    DefualtCurrencyItem = GetCurrency(lId);
            }
            return DefualtCurrencyItem;
        }

        public TransactionRecordList GetTransactionRecords(long aCurrencyId)
        {
            CheckConnected();
            var lResult = new TransactionRecordList(aCurrencyId);
            foreach (var lTxRecord in FLocalCacheDB.ReadTransactionRecords(aCurrencyId))
                lResult.AddTransactionRecord(lTxRecord);
            return lResult;
        }

        public IEnumerable<ClientTokenTransactionItem> GetTokenTransactionRecords(long aTokenID, string aTokenAddress)
        {
            CheckConnected();
            return FLocalCacheDB.ReadTokenTransactions(aTokenAddress).Select(lTokenTx => new ClientTokenTransactionItem(lTokenTx));
        }

        public byte[] GetCurrencyIcon(long aCurrencyId)
        {
            return GetCurrency(aCurrencyId).Icon;
        }

        internal decimal GetDefaultCurrencyPrice(long id)
        {
            return 0;
        }

        public long GetTransactionFee(CurrencyTransaction aCurrencyTransaction)
        {
            CurrencyItem lCurrency = GetCurrency(aCurrencyTransaction.CurrencyId);
#if DEBUG
            lCurrency = this.DirectGetCurrency(aCurrencyTransaction.CurrencyId);
            FLocalCacheDB.Write(lCurrency);
#endif
            string aTxWithNoFee = DirectCreateTransaction(aCurrencyTransaction);

            long lFeePerKb = lCurrency.FeePerKb;
            decimal lKBSize = ((decimal) aTxWithNoFee.Length / 2) / 1024;

            long lTxFee = Convert.ToInt64(lKBSize * lFeePerKb);

            return lTxFee;
        }

        public void Logoff()
        {
            CacheHelper.Dispose();
            FPandoraObjectNotifier?.Terminate();
            try
            {
                FPandoraWalletServiceAccess?.Logoff();
                FPandoraObjectNotifier?.ActiveThread.Join(3000);
            }
            catch { } // I dont care I am disconnecting anyways
            FPandoraWalletServiceAccess?.Dispose();
            FPandoraWalletServiceAccess = null;
            FLocalCacheDB?.Dispose();
            FLocalCacheDB = null;
            DefualtCurrencyItem = null;
            ClearErrors();
        }

        public bool DirectCheckTransactionSent(long aSendTxHandle, out string TxID)
        {
            CheckConnected();
            if (FPandoraWalletServiceAccess.IsTransactionSent(aSendTxHandle))
            {
                TxID = FPandoraWalletServiceAccess.GetTransactionId(aSendTxHandle);
                return true;
            }
            TxID = string.Empty;
            return false;
        }

        public long GetBlockHeight(long aCurrencyID)
        {
            CheckConnected();
            var lResult = FLocalCacheDB.ReadBlockHeight(aCurrencyID);
            if (lResult == 0)
            {
                lResult = (long) FPandoraWalletServiceAccess.GetBlockHeight(aCurrencyID);
                FLocalCacheDB.WriteBlockHeight(aCurrencyID, lResult);
            }
            return lResult;
        }

        public long DirectSendTransaction(long aCurrencyId, string aSignedTxData)
        {
            CheckConnected();
            return FPandoraWalletServiceAccess.SendTransaction(aCurrencyId, aSignedTxData);
        }

        internal void AddMonitoredAccount(string aAddress, long aCurrencyId)
        {
            CheckConnected();
            FLocalCacheDB.WriteMonitoredAccount(DirectAddMonitoredAccount(aAddress, aCurrencyId));
        }

        internal CurrencyAccount DirectAddMonitoredAccount(string aAddress, long aCurrencyId)
        {
            return new CurrencyAccount(FPandoraWalletServiceAccess.AddMonitoredAccount(aCurrencyId, aAddress), aCurrencyId, aAddress);
        }

        internal void WriteKeyValue(string aKeyName, string aValue)
        {
            FLocalCacheDB.WriteKeyValue(aKeyName, aValue);
        }

        internal void WriteAtomicKeyValue(Dictionary<string, string> aDictionary)
        {
            FLocalCacheDB.WriteAtomicKeyValue(aDictionary);
        }

        internal string ReadKeyValue(string aKeyName)
        {
            return FLocalCacheDB.ReadKeyValue(aKeyName);
        }

        internal string GetCoinAddress(long aCurrencyID)
        {
            return GetMonitoredAccounts(aCurrencyID).Last().Address;
        }

        internal TransactionUnit[] GetUnspentOutputs(long aCurrencyId)
        {
            //TODO: I need do this on the DB side so that the
            //      when the user has many transactions this is faster.
            var lTransacitons = GetTransactionRecords(aCurrencyId);
            var lCurrency = GetCurrency(aCurrencyId);
            var lUnspentOutputs = new Dictionary<long, TransactionUnit>();
            var lAddresses = GetMonitoredAddresses(aCurrencyId);

            for (var i = lTransacitons.Count - 1; i >= 0; i--)
            {
                var lTransaction = lTransacitons[i];
                foreach (var lOutput in lTransaction.Outputs)
                    if (lAddresses.Contains(lOutput.Address))
                        lUnspentOutputs.Add(lOutput.Id, lOutput);
                if (lTransaction.Inputs != null)
                    foreach (var lInput in lTransaction.Inputs)
                        if (lUnspentOutputs.ContainsKey(lInput.Id))
                            lUnspentOutputs.Remove(lInput.Id); // spent
            }
            return lUnspentOutputs.Values.ToArray();
        }

        internal bool CheckTransactionHandle(long lTxHandle, out string lTxID)
        {
            bool lResult;
            lTxID = null;
            if (lResult = FPandoraWalletServiceAccess.IsTransactionSent(lTxHandle))
                lTxID = FPandoraWalletServiceAccess.GetTransactionId(lTxHandle);
            return lResult;
        }

        private PandoraWalletServiceAccess FTempServiceAccess;

        internal void SetDataPath(string aDataPath)
        {
            if (FTempServiceAccess == null) throw new InvalidOperationException("The object must be in Restore Mode.");
            DataPath = aDataPath;
        }

        internal void BeginBackupRestore(out string lDBCopyFileName)
        {
            lDBCopyFileName = null;
            if (FTempServiceAccess != null) return;
            CheckConnected();
            StopDataUpdating();
            FTempServiceAccess = FPandoraWalletServiceAccess;
            FPandoraWalletServiceAccess = null;
            lDBCopyFileName = FLocalCacheDB.CreateDBFileCopy();
            FLocalCacheDB.Dispose();
            FLocalCacheDB = null;
        }

        public void StopDataUpdating()
        {
            if (FPandoraObjectNotifier != null)
            {
                FPandoraObjectNotifier.Dispose();
                FPandoraObjectNotifier = null;
            }
        }

        public void StartDataUpdating()
        {
            if (FPandoraObjectNotifier == null)
                CreatePandoraObjectNotifier(FPandoraWalletServiceAccess);
        }

        internal void EndBackupRestore(string lDBCopyFileName)
        {
            if (FTempServiceAccess == null) throw new InvalidOperationException("BackupRestoreStart Process not called first.");
            PandoraWalletServiceAccess lServerAccess = FTempServiceAccess;
            ConnectToDatabase(lServerAccess);
            FPandoraWalletServiceAccess = FTempServiceAccess;
            FTempServiceAccess = null;
            if (!string.IsNullOrEmpty(lDBCopyFileName) && File.Exists(lDBCopyFileName))
                File.Delete(lDBCopyFileName);
        }

        internal void ValidLocalDBFile(string aWalletTempFilePath)
        {
            LocalCacheDB lLocalCachDB = new LocalCacheDB(aWalletTempFilePath);
            var lAccounts = lLocalCachDB.ReadMonitoredAccounts(1);
            foreach (var lAccount in lAccounts)
                if (!ValidateDefaultAddressExists(lAccount.Address))
                    throw new Exception($"The file does not belong to user {UserName} ({Email}).");
        }
    }
}