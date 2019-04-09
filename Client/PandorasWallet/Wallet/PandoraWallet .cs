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
using Pandora.Client.Crypto.Currencies.Controls;
using Pandora.Client.PandorasWallet.ServerAccess;
using Pandora.Client.PandorasWallet.Utils;
using Pandora.Client.Universal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pandora.Client.PandorasWallet.Wallet
{
    public class PandoraWallet : IDisposable
    {
        private PandorasServer FWalletPandoraServer;

        private SettingsManager FUserSettings;

        private PandorasEncryptor FKeyManager;

        private DirectoryInfo FDataFolder;

        private string FSeed;

        private bool FTestnet;

        private object StatusLock = new object();

        //With Locking
        private Dictionary<uint, ulong> FBlockHeigthCache = new Dictionary<uint, ulong>();

        private Dictionary<uint, IClientCurrencyAdvocacy> FCurrencyAdvocacies;
        private Dictionary<uint, TransactionUnit[]> FUnspent;

        //Concurrent
        private ConcurrentDictionary<uint, List<string>> FAddresses;

        private ConcurrentDictionary<uint, BalanceViewModel> FBalance;
        private ConcurrentDictionary<uint, List<TransactionViewModel>> FTransactions;

        public Dictionary<uint, BalanceViewModel> BalancesByCurrency
        {
            get
            {
                foreach (uint it in FUserCoins)
                {
                    if (!FBalance.ContainsKey(it))
                    {
                        BalanceViewModel lBalance = new BalanceViewModel();

                        lBalance.SetCoinPrecision(FWalletPandoraServer.GetCurrency(it).Precision);

                        while (!FBalance.TryAdd(it, lBalance)) ;
                    }

                    GetBalanceModels(it);
                }

                return new Dictionary<uint, BalanceViewModel>(FBalance);
            }
        }

        public ConcurrentDictionary<uint, List<TransactionViewModel>> TransactionsByCurrency
        {
            get
            {
                foreach (uint it in FUserCoins)
                {
                    if (!FTransactions.ContainsKey(it))
                    {
                        //This needs to be added
                        while (!FTransactions.TryAdd(it, new List<TransactionViewModel>()))
                        {
                        }
                    }
                }

                return FTransactions;
            }
        }

        public event Action OnNewTxData;

        public event Action OnNewCurrencyStatusData;

        private CurrencyItem[] FFullListOfCurencies;

        private List<uint> FUserCoins
        {
            get
            {
                FUserSettings.GetSetting("SelectedCoins", out List<uint> lvalue);
                return lvalue;
            }
            set
            {
                FUserSettings.AddSetting("SelectedCoins", value);
                FUserSettings.SaveSettings();
            }
        }

        public Dictionary<uint, CurrencyStatusItem> CoinStatus
        {
            get
            {
                Dictionary<uint, CurrencyStatusItem> lStatus = new Dictionary<uint, CurrencyStatusItem>();

                foreach (uint it in FUserCoins)
                {
                    lStatus[it] = FWalletPandoraServer.GetCurrencyStatus(it);
                }

                return lStatus;
            }
        }

        public List<CurrencyItem> UserCoins
        {
            get
            {
                List<CurrencyItem> lCurrencies = new List<CurrencyItem>();

                foreach (uint it in FUserCoins)
                {
                    lCurrencies.Add(FWalletPandoraServer.GetCurrency(it));
                }

                return lCurrencies;
            }
        }

        private uint FPreviousCurrency = 0;
        private CurrencyItem FCurrencyItemSelected;

        public CurrencyItem ActiveCurrencyItem
        {
            get
            {
                if (FPreviousCurrency != ActiveCurrencyID)
                {
                    FCurrencyItemSelected = UserCoins.Where(x => x.Id == ActiveCurrencyID).FirstOrDefault();
                }

                return FCurrencyItemSelected;
            }
        }

        public List<CurrencyItem> UnselectedCoins
        {
            get
            {
                List<CurrencyItem> lAvaliableCoins = new List<CurrencyItem>();

                CurrencyCount = (uint)FWalletPandoraServer.CurrencyIds.Length;

                foreach (uint it in FWalletPandoraServer.CurrencyIds)
                {
                    if (!FUserCoins.Contains(it))
                    {
                        if (FWalletPandoraServer.GetCurrencyStatus(it).Status != CurrencyStatus.Disabled)
                        {
                            lAvaliableCoins.Add(FWalletPandoraServer.GetCurrency(it));
                        }
                    }
                }

                return lAvaliableCoins;
            }
        }

        public bool UsingFullCoinList;
        public uint CurrencyCount;

        private string DefaultCoinSetting => (InstanceId + "DCoin");

        public ulong Coin
        {
            get
            {
                if (ActiveCurrencyID == 0)
                {
                    return 100000000;
                }

                ushort lPrecision = FWalletPandoraServer.GetCurrency(ActiveCurrencyID).Precision;
                ulong lCoinValue = (ulong)(Math.Pow(10, lPrecision));
                return lCoinValue;
            }
        }

        public ushort Precision => FWalletPandoraServer.GetCurrency(ActiveCurrencyID).Precision;

        public uint AddressNumberSetting
        {
            get
            {
                FUserSettings.GetSetting("AddressNumber", out uint lvalue);
                return lvalue;
            }
            set
            {
                FUserSettings.AddSetting("AddressNumber", value);
                FUserSettings.SaveSettings();
            }
        }

        public uint DefaultCoin
        {
            get
            {
                FUserSettings.GetSetting("DefaultCoin", out uint lvalue);
                return lvalue;
            }

            set
            {
                if (DefaultCoin != 0)
                {
                    if (!FUserCoins.Contains(DefaultCoin))
                    {
                        throw new InvalidOperationException("Default coin should be a selected coin");
                    }
                }

                FUserSettings.AddSetting("DefaultCoin", value);
                FUserSettings.SaveSettings();
            }
        }

        public string InstanceId => FWalletPandoraServer.InstanceId;

        public ulong BlockHeight
        {
            get
            {
                if (ActiveCurrencyID != 0)
                {
                    return FWalletPandoraServer.GetBlockHeight(ActiveCurrencyID);
                }

                return 0;
            }
        }

        public uint ActiveCurrencyID
        {
            get;
            set;
        }

        public CurrencyStatus ActiveCurrencyStatus
        {
            get;
            set;
        }

        public bool Connected => FWalletPandoraServer.Connected;

        public bool Encrypted => FKeyManager.isPasswordSet;

        public string DataFolder => FDataFolder.FullName;

        public string Username => FWalletPandoraServer.Username;

        public string Email => FWalletPandoraServer.Email;

        public event CurrencyUpdatedHandle OnCurrencyItemUpdated;

        public delegate void CurrencyUpdatedHandle(ulong aCurrencyID);

        private PandoraWallet(bool aisTestnet)
        {
            string lSettingsFolderPath = Properties.Settings.Default.DataPath;

            string lDefaultDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).ToString(), "PandorasWallet");
            DirectoryInfo lDefaultFolder = Directory.CreateDirectory(lDefaultDataPath);

            if (string.IsNullOrEmpty(lSettingsFolderPath))
            {
                FDataFolder = lDefaultFolder;
            }
            else
            {
                FDataFolder = Directory.CreateDirectory(lSettingsFolderPath);
            }

            string ServerName = Properties.Settings.Default.ServerName;
            int Port = Properties.Settings.Default.Port;
            bool EncryptedConnection = Properties.Settings.Default.EncyptedConnection;

            FWalletPandoraServer = new PandorasServer(FDataFolder.FullName, ServerName, Port, EncryptedConnection);

            FWalletPandoraServer.OnCurrencyItemUpdated += FWalletPandoraServer_OnCurrencyItemUpdated;

            FWalletPandoraServer.OnCurrencyFullListExpired += CurrencyFullListExpiredHandler;
            FWalletPandoraServer.OnCurrencyStatusFullListExpired += FWalletPandoraServer_OnCurrencyStatusFullListExpired;
            FWalletPandoraServer.OnMonitoredAccountsFullListExpired += FWalletPandoraServer_OnMonitoredAccountsFullListExpired;

            FBalance = new ConcurrentDictionary<uint, BalanceViewModel>();
            FBalance.TryAdd(0, new BalanceViewModel());
            FTransactions = new ConcurrentDictionary<uint, List<TransactionViewModel>>();
            FTransactions.TryAdd(0, new List<TransactionViewModel>());
            FCurrencyAdvocacies = new Dictionary<uint, IClientCurrencyAdvocacy>();
            FAddresses = new ConcurrentDictionary<uint, List<string>>();
            FUnspent = new Dictionary<uint, TransactionUnit[]>();

            FTestnet = aisTestnet;
        }

        private void FWalletPandoraServer_OnMonitoredAccountsFullListExpired()
        {
            FWalletPandoraServer.GetMonitoredAccounts();
        }

        private void FWalletPandoraServer_OnCurrencyStatusFullListExpired()
        {
            FWalletPandoraServer.GetCurrencyStatus(true);
        }

        private void FWalletPandoraServer_OnCurrencyItemUpdated(ulong obj)
        {
            OnCurrencyItemUpdated?.Invoke(obj);
        }

        private void OnTransactionsEventHandler(uint[] aIds, bool aisConfirmationUpdate)
        {
            foreach (uint it in aIds)
            {
                if (!FBalance.ContainsKey(it))
                {
                    while (!FBalance.TryAdd(it, new BalanceViewModel())) ;
                }

                GetBalanceModels(it, true);
            }

            if (aisConfirmationUpdate)
            {
                foreach (uint lCurrencyId in aIds)
                {
                    lock (FBlockHeigthCache)
                    {
                        if (!FBlockHeigthCache.ContainsKey(lCurrencyId))
                        {
                            FBlockHeigthCache.Add(lCurrencyId, FWalletPandoraServer.GetBlockHeight(lCurrencyId));
                            OnNewTxData();
                            return;
                        }

                        if (FBlockHeigthCache[lCurrencyId] < FWalletPandoraServer.GetBlockHeight(lCurrencyId))
                        {
                            FBlockHeigthCache[lCurrencyId] = FWalletPandoraServer.GetBlockHeight(lCurrencyId);
                            OnNewTxData();
                            return;
                        }
                    }
                }
                return;
            }

            OnNewTxData();
        }

        public void SaveExchangeCredentials(string aExchangeName, string aKey, string aSecret)
        {
            KeyInventoryItem lInventoryItem = aExchangeName.GetEnumFromDescription<KeyInventoryItem>();

            FKeyManager.AddOrReplaceInventoryItem(lInventoryItem, aKey, aSecret);

            FKeyManager.OverwriteWallet();
        }

        public bool TryGetExchangeKeyPair(string aExchangeName, out string aKey, out string aSecret)
        {
            KeyInventoryItem lInventoryItem = aExchangeName.GetEnumFromDescription<KeyInventoryItem>();

            Tuple<string, string> lKeyPair = FKeyManager.GetExchangeKeyPair(lInventoryItem);

            if (lKeyPair == null)
            {
                aKey = string.Empty;
                aSecret = string.Empty;
                return false;
            }

            aKey = lKeyPair.Item1;
            aSecret = lKeyPair.Item2;

            return true;
        }

        public string[] GetPrivKey()
        {
            if (FSeed == null)
            {
                throw new Exception("Can't execute if seed is not set");
            }
            IClientCurrencyAdvocacy lAdvocacy = GetAdvocacy(ActiveCurrencyID);

            List<string> lPrivKeys = new List<string>();
            for (uint it = 1; it <= AddressNumberSetting; it++)
            {
                lPrivKeys.Add(lAdvocacy.GetPrivateKey(it));
            }
            if (!lPrivKeys.Any())
                throw new Exception("Failed to retrieve private key specified address");
            return lPrivKeys.ToArray();
        }

        public bool CheckIfHaveKeys(string aExchangeName)
        {
            return FKeyManager.Inventory.Contains(aExchangeName);
        }

        public void ChangeWalletPassword(string aNewPassword)
        {
            FKeyManager.OverwriteWallet(aNewPassword);
        }

        public void DecryptWallet(string aPassword, string aGuid = "", bool aOnlyPasswordChecking = false)
        {
            if (!Encrypted)

            {
                FKeyManager.CreateEncryptedWalletKeys(aPassword, aGuid);
            }

            if (aOnlyPasswordChecking || FSeed != null)
            {
                if (FKeyManager.CheckPassword(aPassword))
                {
                    return;
                }

                throw new Wallet.ClientExceptions.WrongPasswordException("Failed to decrypt. Wrong password");
            }

            if (!FKeyManager.TryToDecryptWalletKey(aPassword))
            {
                throw new Wallet.ClientExceptions.WrongPasswordException("Failed to decrypt. Wrong password");
            }
        }

        public string SettingsFile => FUserSettings.SettingsPath;
        public string DBFileName => FWalletPandoraServer.DBFileName;

        public void ChangeDataFolder(string aNewDataPath)
        {
            FileInfo[] lFiles = FDataFolder.GetFiles();
            DirectoryInfo lDataFolder = Directory.CreateDirectory(aNewDataPath);

            try
            {
                foreach (FileInfo it in lFiles)
                {
                    it.CopyTo(Path.Combine(lDataFolder.FullName, it.Name));
                }

                FDataFolder = lDataFolder;

                FWalletPandoraServer.DataPath = lDataFolder.FullName;

                FUserSettings = new SettingsManager(SettingsFile);
                FKeyManager = new PandorasEncryptor(FDataFolder, InstanceId, FUserSettings);

                Properties.Settings.Default.DataPath = lDataFolder.FullName;
            }
            catch (Exception ex)
            {
                foreach (string it in lFiles.Select(x => x.Name))
                {
                    try
                    {
                        File.Delete(Path.Combine(lDataFolder.FullName, it));
                    }
                    catch
                    {
                        continue;
                    }
                }

                throw new Exception("Failed to change data folder. Details: " + ex.Message);
            }
        }

        public void InitializeRootSeed(bool aForceSeed = false)
        {
            UserStatus lUserStatus = FWalletPandoraServer.GetUserStatus();
            DateTime lLockDate = new DateTime(2019, 4, 8, 23, 59, 59);
            string lEmail = FWalletPandoraServer.Email;
            string lUsername = FWalletPandoraServer.Username;
            if (FSeed == null || aForceSeed)
            {
                FSeed = FKeyManager.GetSecretRootSeed(lEmail.ToLower(), lUsername.ToLower());
                if (lUserStatus.StatusDate <= lLockDate)
                {
                    string lStatusMessage = lUserStatus.ExtendedInfo;

                    if (lUserStatus.ExtendedInfo.Contains("Actual Case is:"))
                    {
                        int lStart = lStatusMessage.IndexOf(":", StringComparison.Ordinal) + 1;
                        string[] lSubstring = lStatusMessage.Substring(lStart, lStatusMessage.Length - lStart).Split(new string[] { "***" }, StringSplitOptions.RemoveEmptyEntries);
                        lEmail = lSubstring[0].Trim();
                        lUsername = lSubstring[1].Trim();
                        FSeed = FKeyManager.GetSecretRootSeed(lEmail, lUsername);
                    }
                    else
                    {
                        FSeed = FKeyManager.GetSecretRootSeed(lEmail, lUsername);
                        CurrencyItem lCurrency = FWalletPandoraServer.GetCurrency(1);
                        IClientCurrencyAdvocacy lAdvocacy = CurrencyControl.GetCurrencyControl().GetClientCurrencyAdvocacy((uint)lCurrency.Id, lCurrency.ChainParamaters);
                        lAdvocacy.RootSeed = FSeed;
                        string lComparationAddress = lAdvocacy.GetAddress(1);
                        string lAddress = GetCoinAddress(1);

                        if (lComparationAddress != lAddress)
                        {
                            string lMessage = string.Format("Please contact support at support@davincicodes.net about Ontime Issue #1344");
                            throw new Exception(lMessage);
                        }
                        FWalletPandoraServer.MarkOldUser(lEmail, lUsername);
                    }
                }
            }
        }

        public static PandoraWallet TryToGetNewWalletInstance(string aEmail, string aUsername, string aPassword, bool aisTestnet = false)
        {
            PandoraWallet lWallet = new PandoraWallet(aisTestnet);

            if (!lWallet.Logon(aEmail, aUsername, aPassword))
            {
                lWallet.Dispose();
                throw new ClientExceptions.InvalidOperationException("Invalid Logon For: " + aEmail + ", and Username: " + aUsername);
            }

            Properties.Settings.Default.Email = aEmail;
            Properties.Settings.Default.Username = aUsername;

            Properties.Settings.Default.Save();

            return lWallet;
        }

        public void AddNewSelectedCurrency(uint aId)
        {
            if (!Connected)
            {
                throw new InvalidOperationException("Needs to have a connection to use this method");
            }

            List<uint> lSelectedCoins = FUserCoins;

            if (!lSelectedCoins.Contains(aId))
            {
                lSelectedCoins.Add(aId);
            }

            FUserCoins = lSelectedCoins;
        }

        public CurrencyItem[] GetFullListOfCurrencies()
        {
            if (!UsingFullCoinList)
            {
                throw new InvalidOperationException("Set UsingFullCoinList first to use this method");
            }

            uint lTimeoutCount = 0;

            FFullListOfCurencies = FWalletPandoraServer.GetCurrencyList();

            while (FFullListOfCurencies.Count() == 0)
            {
                if (lTimeoutCount > 10)
                {
                    System.Threading.Thread.Sleep(1000);

                    if (lTimeoutCount > 20)
                    {
                        throw new ClientExceptions.ConnectionTimeoutException("Failed to retrive data from server");
                    }
                }

                //FWalletPandoraServer.RefreshData();

                FFullListOfCurencies = FWalletPandoraServer.GetCurrencyList();

                lTimeoutCount++;
            }

            CurrencyCount = (uint)FWalletPandoraServer.CurrencyIds.Length;

            return FFullListOfCurencies.Where(x => FWalletPandoraServer.GetCurrencyStatus((uint)x.Id).Status != CurrencyStatus.Disabled).ToArray();
        }

        private void CurrencyFullListExpiredHandler()
        {
            if (UsingFullCoinList)
            {
                FFullListOfCurencies = FWalletPandoraServer.GetCurrencyList();
                CurrencyCount = (uint)FWalletPandoraServer.CurrencyIds.Length;
            }
        }

        public void InitTxTracking()
        {
            FWalletPandoraServer.OnTransactions += OnTransactionsEventHandler;
            FWalletPandoraServer.StartTxUpdatingTask();
        }

        public void InitCurrencyStatusUpdating()
        {
            FWalletPandoraServer.OnCurrencyStatus += OnCurrencyStatusEventHandler;
            FWalletPandoraServer.StartCurrencyStatusUpdatingTask();
        }

        public void InitCurrencyItemUpdating()
        {
            FWalletPandoraServer.StartCurrencyItemUpdatingTask();
        }

        private void OnCurrencyStatusEventHandler(uint[] aIds)
        {
            lock (StatusLock)
            {
                foreach (uint it in aIds)
                {
                    CoinStatus[it] = FWalletPandoraServer.GetCurrencyStatus(it);
                }
            }

            OnNewCurrencyStatusData?.Invoke();
        }

        public bool CheckIfUserHasAccounts()
        {
            bool lIsNewAccount = !FWalletPandoraServer.IsNewAccount();

            //if (!FUserCoins.Contains(1) && !FAddresses.ContainsKey(1))
            //{
            //    SetCoinAccountData(1);
            //}

            return lIsNewAccount;
        }

        private IClientCurrencyAdvocacy GetAdvocacy(uint aCurrencyId)
        {
            if (string.IsNullOrEmpty(FSeed))
            {
                throw new InvalidOperationException("A seed must be assign before asking for an advocacy");
            }

            IClientCurrencyAdvocacy lAdvocacy;

            lock (FCurrencyAdvocacies)
            {
                if (!FCurrencyAdvocacies.ContainsKey(aCurrencyId))
                {
                    CurrencyItem lCurrency = FWalletPandoraServer.GetCurrency(aCurrencyId);
                    FCurrencyAdvocacies.Add(aCurrencyId, CurrencyControl.GetCurrencyControl().GetClientCurrencyAdvocacy(aCurrencyId, lCurrency.ChainParamaters));

                    FCurrencyAdvocacies[aCurrencyId].RootSeed = FSeed;
                }

                lAdvocacy = FCurrencyAdvocacies[aCurrencyId];
            }

            if (!FAddresses.ContainsKey(aCurrencyId))
            {
                while (!FAddresses.TryAdd(aCurrencyId, new List<string>())) ;
            }

            if (FAddresses[aCurrencyId].Count < AddressNumberSetting)
            {
                FAddresses[aCurrencyId].Clear();

                for (uint it = 1; it <= AddressNumberSetting; it++)
                {
                    FAddresses[aCurrencyId].Add(lAdvocacy.GetAddress(it));
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(string.Format("Currency: {0}, Address: {1}, PrivateKey: {2}", aCurrencyId, lAdvocacy.GetAddress(it), lAdvocacy.GetPrivateKey(it)));
#endif
                }
            }

            return lAdvocacy;
        }

        public void TrySetAccountAddresses(uint aCurrencyId, IClientCurrencyAdvocacy aAdvocacy)
        {
            List<CurrencyAccount> lCurrenciesAccounts;

            List<string> lAdvocacyAddresses = new List<string>();

            for (uint it = 1; it <= AddressNumberSetting; it++)
            {
                lAdvocacyAddresses.Add(aAdvocacy.GetAddress(it));
            }

            List<string> lNotinServer = null;
            ushort lExceptionCounter = 0;

            while (true)
            {
                if (lExceptionCounter > 5)
                {
                    throw new ClientExceptions.AddressSynchException("Error synching addresses with server");
                }

                lCurrenciesAccounts = FWalletPandoraServer.MonitoredAccounts.GetById(aCurrencyId).ToList();

                lNotinServer = lAdvocacyAddresses.Where(address => !lCurrenciesAccounts.Exists(x => x.Address == address)).ToList();

                if (lCurrenciesAccounts.Any())
                {
                    if (lNotinServer.Any())
                    {
                        throw new ClientExceptions.AddressSynchException("Bad wallet file for account. Try Restoring Wallet");
                    }

                    break;
                }

                foreach (string lAddress in lNotinServer)
                {
                    FWalletPandoraServer.MonitoredAccounts.AddCurrencyAccount(aCurrencyId, lAddress);
                }

                System.Threading.Thread.Sleep(1000);
                lExceptionCounter++;
            }

            FAddresses[aCurrencyId] = lAdvocacyAddresses;
        }

        public void SetCoinAccountData(IEnumerable<CurrencyItem> aListOfCurrencies)
        {
            foreach (CurrencyItem it in aListOfCurrencies)
            {
                SetCoinAccountData((uint)it.Id);
            }
        }

        public void SetCoinAccountData(uint aCurrencyId)
        {
            if (string.IsNullOrEmpty(FSeed) && !FAddresses.ContainsKey(aCurrencyId))
            {
                List<CurrencyAccount> lCurrenciesAccounts = JsonConvert.DeserializeObject<List<CurrencyAccount>>(FWalletPandoraServer.FServerAccess.GetMonitoredAcccounts(aCurrencyId, 0));

                if (!lCurrenciesAccounts.Any())
                    throw new ClientExceptions.AddressSynchException("Found incoherence between client and server. Try Restoring Wallet");
                FWalletPandoraServer.AddMonitoredAccount(aCurrencyId, lCurrenciesAccounts[0].Address);
                while (!FAddresses.TryAdd(aCurrencyId, lCurrenciesAccounts.Select(x => x.Address).ToList())) ;

                return;
            }

            IClientCurrencyAdvocacy lAdvocacy = GetAdvocacy(aCurrencyId);

            TrySetAccountAddresses(aCurrencyId, lAdvocacy);
        }

        public string GetCoinAddress(uint aCurrencyId)
        {
            if (aCurrencyId == 0)
            {
                return string.Empty;
            }

            return FAddresses[aCurrencyId].Last();
        }

        public bool Logon(string aEmail, string aUsername, string aPassword)
        {
            if (FWalletPandoraServer.Logon(aEmail, aUsername, aPassword))
            {
                try
                {
                    FUserSettings = new SettingsManager(Path.Combine(FDataFolder.FullName, InstanceId + ".settings"));
                    FUserSettings.CreateUserSettings();
                }
                catch (Exception ex)
                {
                    throw new ClientExceptions.SettingsFailureException("Error on settings: " + ex.Message);
                }

                FKeyManager = new PandorasEncryptor(FDataFolder, InstanceId, FUserSettings);

                UserStatus lUserStatus = FWalletPandoraServer.GetUserStatus();

                if (!lUserStatus.Active)
                {
                    throw new ClientExceptions.UserNotActiveException(lUserStatus.ExtendedInfo, lUserStatus.StatusDate);
                }

                FWalletPandoraServer.ClearMemoryCache();

                return true;
            }

            return false;
        }

        public BalanceViewModel GetBalance(uint aCurrencyId)
        {
            {
                if (!BalancesByCurrency.TryGetValue(aCurrencyId, out BalanceViewModel lBalance))
                    throw new Exception("Failed to retrieve balance data");

                return lBalance;
            }
        }

        private void GetBalanceModels(uint aCoinID, bool aEventFlag = false)
        {
            if (((!FAddresses.ContainsKey(aCoinID)) || FAddresses[aCoinID] == null) || (!aEventFlag && !FBalance[aCoinID].IsEmpty))
            {
                return;
            }

            Dictionary<string, Tuple<ulong, ulong>> LConfirmedBalances = new Dictionary<string, Tuple<ulong, ulong>>();
            Dictionary<string, Tuple<ulong, ulong>> LUnconfirmedBalances = new Dictionary<string, Tuple<ulong, ulong>>();

            TransactionRecord[] lTransactions = FWalletPandoraServer.GetTransactions(aCoinID);

            ulong lBalance = 0;

            foreach (uint it in FUserCoins)
            {
                if (!FTransactions.ContainsKey(it))
                {
                    while (!FTransactions.TryAdd(it, new List<TransactionViewModel>()))
                    {
                    }
                }
            }

            ulong lPTotalConfirmedBalance = 0;
            ulong lPTotalUnconfirmedBalance = 0;

            ulong lNTotalConfirmedBalance = 0;
            ulong lNTotalUnconfirmedBalance = 0;

            List<string> lAccounts = new List<string>(FAddresses[aCoinID]);

            bool lOneShot = true;

            foreach (string lAddress in lAccounts)
            {
                foreach (TransactionRecord lTransaction in lTransactions)
                {
                    List<string> lTransactionAddresses = new List<string>();

                    if (lTransaction.Inputs != null)
                    {
                        lTransactionAddresses.AddRange(lTransaction.Inputs.Select(x => x.Address).ToArray());
                    }

                    if (lTransaction.Outputs != null)
                    {
                        lTransactionAddresses.AddRange(lTransaction.Outputs.Select(x => x.Address).ToArray());
                    }

                    if (!lAccounts.Exists(x => lTransactionAddresses.Contains(x)))
                    {
                        continue;
                    }

                    if (lOneShot)
                    {
                        if (!FTransactions.ContainsKey(aCoinID))
                        {
                            FTransactions[aCoinID] = new List<TransactionViewModel>();
                        }

                        if ((!FTransactions[aCoinID].Exists(x => x.TransactionID == lTransaction.TxId)))
                        {
                            FTransactions[aCoinID].Add(new TransactionViewModel(lTransaction, lAccounts));
                        }

                        if (aCoinID != 0)
                        {
                            TransactionViewModel lTransactionViewModel = FTransactions[aCoinID].Find(x => x.TransactionID == lTransaction.TxId);

                            if (lTransaction.Block != 0 && lTransactionViewModel.Block == 0)
                            {
                                lTransactionViewModel.Set(lTransaction, lAccounts);
                            }

                            lTransactionViewModel.SetBlockHeight(FWalletPandoraServer.GetBlockHeight(aCoinID), FWalletPandoraServer.GetCurrency(aCoinID).MinConfirmations);

                            if (!lTransactionViewModel.isConfirmed)
                            {
                                FWalletPandoraServer.CheckIfConfirmed(Convert.ToUInt32(aCoinID), lTransaction.Block);
                            }
                        }
                    }

                    lBalance = 0;

                    if (lTransaction.Block > 0)
                    {
                        if (ScanTransaction(lAddress, lTransaction, out lBalance))
                        {
                            lPTotalConfirmedBalance += lBalance;
                        }
                        else
                        {
                            lNTotalConfirmedBalance += lBalance;
                        }
                    }
                    else
                    {
                        if (ScanTransaction(lAddress, lTransaction, out lBalance))
                        {
                            lPTotalUnconfirmedBalance += lBalance;
                        }
                        else
                        {
                            lNTotalUnconfirmedBalance += lBalance;
                        }
                    }
                }

                lOneShot = false;

                LConfirmedBalances.Add(lAddress, new Tuple<ulong, ulong>(lPTotalConfirmedBalance, lNTotalConfirmedBalance));
                LUnconfirmedBalances.Add(lAddress, new Tuple<ulong, ulong>(lPTotalUnconfirmedBalance, lNTotalUnconfirmedBalance));
            }

            List<TransactionUnit> lUnspent = GetUnspentOutputs(aCoinID, lTransactions);

            FBalance[aCoinID].SetNewData(LConfirmedBalances, LUnconfirmedBalances);

            ulong lUnspentBalance = 0;

            foreach (TransactionUnit it in lUnspent)
            {
                lUnspentBalance += it.Amount;
            }

            if (FBalance[aCoinID].Total != lUnspentBalance)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("Consistency problems on balances");
#endif
                Log.Write(LogLevel.Error, "Consistency problems on balances on {0} for user: {1} ({2}) ", ActiveCurrencyItem.Name, Username, Email);
            }

            lock (FUnspent)
                FUnspent[aCoinID] = lUnspent.ToArray();
        }

        public bool ScanTransaction(string aAddress, TransactionRecord aTransactionRecord, out ulong aBalance)
        {
            ulong lTotalInputs = 0;
            ulong lTotalOutputs = 0;

            TransactionUnit[] lInput = aTransactionRecord.Inputs;
            if (lInput != null)
            {
                foreach (TransactionUnit input in lInput)
                {
                    if (aAddress == input.Address)
                    {
                        lTotalInputs = lTotalInputs + input.Amount;
                    }
                }
            }

            TransactionUnit[] lOutput = aTransactionRecord.Outputs;

            if (lOutput != null)
            {
                foreach (TransactionUnit output in lOutput)
                {
                    if (aAddress == output.Address)
                    {
                        lTotalOutputs = lTotalOutputs + output.Amount;
                    }
                }
            }

            if (lTotalOutputs >= lTotalInputs)
            {
                aBalance = lTotalOutputs - lTotalInputs;
                return true;
            }
            else
            {
                aBalance = lTotalInputs - lTotalOutputs;
                return false;
            }
        }

        public List<TransactionUnit> GetUnspentOutputs(uint aCurrencyID, TransactionRecord[] aTransactionRecordArray)
        {
            if (aCurrencyID == 0)
            {
                return null;
            }

            List<TransactionUnit> aInputList = new List<TransactionUnit>();
            List<TransactionUnit> aOutputList = new List<TransactionUnit>();

            foreach (TransactionRecord it in aTransactionRecordArray)
            {
                if (it.Inputs != null)
                {
                    aInputList.AddRange(it.Inputs);
                }

                aOutputList.AddRange(it.Outputs);
            }

            return aOutputList.Where(x => !(aInputList.Exists(y => y.Id == x.Id)) && FAddresses[aCurrencyID].Contains(x.Address)).ToList();
        }

        public ulong CalculateTxFee(string aToAddress, decimal aAmount, uint aCurrencyID)
        {
            ulong lAmountToSend = (ulong)(aAmount * Coin);
            return CalculateTxFee(aToAddress, lAmountToSend, aCurrencyID);
        }

        public ulong CalculateTxFee(string aToAddress, ulong aAmount, uint aCurrencyID)
        {
            CurrencyTransaction lCurrencyTransaction;

            ulong lBalance = FBalance[aCurrencyID].Total;

            if (lBalance < aAmount)
            {
                throw new ClientExceptions.InsufficientFundsException("The amount given is higher than the balance");
            }

            if (aAmount == 0)
            {
                throw new ClientExceptions.InvalidOperationException("The amount to send can't be empty");
            }

            lCurrencyTransaction = CreateNewCurrencyTransaction(aCurrencyID, aAmount, aToAddress);

            ulong lTxFee = FWalletPandoraServer.GetTransactionFee(aCurrencyID, lCurrencyTransaction);

            return lTxFee;
        }

        public string PrepareNewTransaction(string aToAddress, decimal aAmount, uint aCurrencyID, ulong aTxFee = 0)
        {
            ulong lAmountToSend = (ulong)(aAmount * Coin);
            return PrepareNewTransaction(aToAddress, lAmountToSend, aCurrencyID, aTxFee);
        }

        public string PrepareNewTransaction(string aToAddress, ulong aAmount, uint aCurrencyID, ulong aTxFee = 0)
        {
            CurrencyTransaction lCurrencyTransaction;

            if (!FWalletPandoraServer.VerifyAddress(aCurrencyID, aToAddress))
            {
                throw new ClientExceptions.InvalidAddressException("Address provided not valid. Please verify");
            }

            ulong lBalance = FBalance[aCurrencyID].Total;

            ulong lTxFee = (aTxFee == 0) ? CalculateTxFee(aToAddress, aAmount, aCurrencyID) : aTxFee;

            if (lBalance < (aAmount + lTxFee))
            {
                throw new ClientExceptions.InsufficientFundsException("The amount plus TxFee is higher than the balance");
            }

            lCurrencyTransaction = CreateNewCurrencyTransaction(aCurrencyID, aAmount, aToAddress, lTxFee);

            if (lCurrencyTransaction.TxFee == 0)
            {
                throw new ClientExceptions.InvalidOperationException("You can't send a transaction with no fee");
            }

            string TxData = FWalletPandoraServer.CreateTransaction(aCurrencyID, lCurrencyTransaction);

            SetCoinAccountData(aCurrencyID);

            string lSignedTx;
            lock (FCurrencyAdvocacies)
                lSignedTx = FCurrencyAdvocacies[aCurrencyID].SignTransaction(TxData, lCurrencyTransaction);
            return lSignedTx;
        }

        public long SendNewTransaction(string aSignedTx, uint aCurrencyID)
        {
            return FWalletPandoraServer.SendTransaction(aCurrencyID, aSignedTx);
        }

        public bool CheckTransactionHandle(long aHandle, out string aTxId)
        {
            return FWalletPandoraServer.CheckTransactionSent(aHandle, out aTxId);
        }

        private CurrencyTransaction CreateNewCurrencyTransaction(uint aCurrencyID, ulong aAmountToSend, string aToAddress, ulong aTxFee = 0)
        {
            CurrencyTransaction lCurrecyTransaction = new CurrencyTransaction();
            lock (FUnspent)
                lCurrecyTransaction.AddInput(FUnspent[aCurrencyID]);

            lCurrecyTransaction.AddOutput(aAmountToSend, aToAddress);

            ulong lChangeAmount = FBalance[aCurrencyID].Total - aAmountToSend - aTxFee;

            if (lChangeAmount > 0)
            {
                lCurrecyTransaction.AddOutput(lChangeAmount, FAddresses[aCurrencyID].First());
            }

            lCurrecyTransaction.TxFee = aTxFee;

            lCurrecyTransaction.CurrencyId = aCurrencyID;

            return lCurrecyTransaction;
        }

        public bool Close()
        {
            FUserSettings.SaveSettings();

            return FWalletPandoraServer.Logoff();
        }

        public void CreateBackup(string aPath)
        {
            string lCopyFile = Path.Combine(FDataFolder.FullName, InstanceId + "_Copy" + ".exchange");
            BackUp lBackUp = new BackUp();
            string lBackupFile = Path.ChangeExtension(aPath, ".bkp");
            string lSecret = Directory.EnumerateFiles(FDataFolder.FullName).Where((x) => x.Contains(InstanceId + ".secret")).ToList().First();
            string lSettings = Directory.EnumerateFiles(FDataFolder.FullName).Where((x) => x.Contains(InstanceId + ".settings")).ToList().First();
            string lExchange = Directory.EnumerateFiles(FDataFolder.FullName).Where((x) => x.Contains(InstanceId + ".exchange")).ToList().First();

            File.Copy(lExchange, lCopyFile);

            lBackUp.Secret = File.ReadAllBytes(lSecret);
            lBackUp.LengthSecret = lBackUp.Secret.Length;
            lBackUp.Setting = File.ReadAllBytes(lSettings);
            lBackUp.LengthSetting = lBackUp.Setting.Length;
            lBackUp.Exchange = File.ReadAllBytes(Path.Combine(FDataFolder.FullName, InstanceId + "_Copy" + ".exchange"));
            lBackUp.LengthExchange = lBackUp.Exchange.Length;
            lBackUp.Version = 112;
            if (File.Exists(lBackupFile))
            {
                File.Delete(lBackupFile);
                WriteBackupFile(lBackupFile, lBackUp);
            }
            else
            {
                WriteBackupFile(lBackupFile, lBackUp);
            }
            File.Delete(lCopyFile);
        }

        public void WriteBackupFile(string aPath, BackUp aBackUp)
        {
            using (FileStream lFileStream = new FileStream(aPath, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter lBinaryWriter = new BinaryWriter(lFileStream))
                {
                    lBinaryWriter.Write(aBackUp.Version);
                    lBinaryWriter.Write(aBackUp.LengthSecret);
                    lBinaryWriter.Write(aBackUp.Secret);
                    lBinaryWriter.Write(aBackUp.LengthSetting);
                    lBinaryWriter.Write(aBackUp.Setting);
                    lBinaryWriter.Write(aBackUp.LengthExchange);
                    lBinaryWriter.Write(aBackUp.Exchange);
                }
            }
        }

        public bool RestoreInformationFromFile(string aPath)
        {
            FUserSettings.DeleteSettings();
            BackUp lBackup = new BackUp();
            string lPathSecret = Path.Combine(FDataFolder.FullName, InstanceId + ".secret");
            string lPathSettings = Path.Combine(FDataFolder.FullName, InstanceId + ".settings");
            string lPathExchange = Path.Combine(FDataFolder.FullName, InstanceId + ".exchange");

            using (FileStream lFileStream = new FileStream(aPath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader lBinaryReader = new BinaryReader(lFileStream))
                {
                    lBackup.Version = lBinaryReader.ReadInt32();
                    lBackup.LengthSecret = lBinaryReader.ReadInt32();
                    lBackup.Secret = lBinaryReader.ReadBytes(lBackup.LengthSecret);
                    lBackup.LengthSetting = lBinaryReader.ReadInt32();
                    lBackup.Setting = lBinaryReader.ReadBytes(lBackup.LengthSetting);
                    lBackup.LengthExchange = lBinaryReader.ReadInt32();
                    lBackup.Exchange = lBinaryReader.ReadBytes(lBackup.LengthExchange);
                }
            }
            if (File.Exists(lPathSecret))
            {
                File.Delete(lPathSecret);
            }

            using (FileStream lFileStream = new FileStream(lPathSecret, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter lBinaryWriter = new BinaryWriter(lFileStream))
                {
                    lBinaryWriter.Write(lBackup.Secret);
                }
            }
            if (File.Exists(lPathSettings))
            {
                File.Delete(lPathSettings);
            }
            using (FileStream lFileStream = new FileStream(lPathSettings, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter lBinaryWriter = new BinaryWriter(lFileStream))
                {
                    lBinaryWriter.Write(lBackup.Setting);
                }
            }

            if (File.Exists(lPathExchange))
            {
                File.Delete(lPathExchange);
            }
            using (FileStream lFileStream = new FileStream(lPathExchange, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter lBinaryWriter = new BinaryWriter(lFileStream))
                {
                    lBinaryWriter.Write(lBackup.Exchange);
                }
            }

            FUserSettings.LoadSettings();
            return true;
        }

        public struct BackUp
        {
            public int Version;
            public byte[] Secret;
            public byte[] Setting;

            public byte[] Exchange;

            public int LengthSetting;
            public int LengthSecret;

            public int LengthExchange;
        }

        public uint[] Get12NumbersOf11Bits()
        {
            string l32ByteHex = FKeyManager.UserGuid.ToString().Replace("-", "");
            List<uint> lResult = new List<uint>();
            uint lLast4BitSum = 0;
            ulong lCurrentNum = 0;
            int lRemainder = 0;
            if (string.IsNullOrEmpty(l32ByteHex) || l32ByteHex.Length != 32)
            {
                throw new InvalidOperationException("String must have a length of 32.");
            }

            string lHex;
            for (int i = 0; i < 11; i++)
            {
                if (i == 10)
                {
                    lHex = l32ByteHex.Substring(i * 3, 2);
                }
                else
                {
                    lHex = l32ByteHex.Substring(i * 3, 3);
                }
                uint lNum = uint.Parse(lHex, System.Globalization.NumberStyles.HexNumber);
                lCurrentNum |= lNum << lRemainder;
                lResult.Add((uint)(lCurrentNum & 2047));
                lCurrentNum >>= 11;
                lRemainder++;
                lLast4BitSum ^= (lNum & 0xf) ^ ((lNum >> 4) & 0xf) ^ (lNum >> 8);
            }
            lCurrentNum |= (lLast4BitSum << 7);
            lResult.Add((uint)lCurrentNum);
            return lResult.ToArray();
        }

        public string GetHexPassCode(uint[] aNumbers)
        {
            ushort lReminder = 11;
            ushort lMoveNumber = 0;
            uint lBitsToTake = 0;
            uint lBitsTaken = 0;
            uint lNum = 0;
            string lHex = "";
            string SumAllNumbers = "";
            uint lLast8Bits = 0;
            uint lLast4BitSum = 0;
            for (int i = 0; i < 10; i++)
            {
                lBitsToTake = lBitsToTake + (uint)Math.Pow(2, i);
                lBitsTaken = aNumbers[i + 1] & lBitsToTake;
                lNum = (aNumbers[i] >> lMoveNumber) | lBitsTaken << lReminder;
                if (Convert.ToString(lNum, 16).Length == 2)
                {
                    SumAllNumbers = SumAllNumbers + "0" + Convert.ToString(lNum, 16);
                }

                if (Convert.ToString(lNum, 16).Length == 1)
                {
                    SumAllNumbers = SumAllNumbers + "00" + Convert.ToString(lNum, 16);
                }

                if (Convert.ToString(lNum, 16).Length == 3)
                {
                    SumAllNumbers = SumAllNumbers + Convert.ToString(lNum, 16);
                }

                lLast4BitSum ^= (lNum & 0xf) ^ ((lNum >> 4) & 0xf) ^ (lNum >> 8);
                lReminder--;
                lMoveNumber++;
            }
            uint lLastBit = aNumbers[10] >> 10;
            lLast8Bits = ((aNumbers[11] & 127) << 1) | lLastBit;
            lLast4BitSum ^= (lLast8Bits & 0xf) ^ ((lLast8Bits >> 4) & 0xf);
            SumAllNumbers = SumAllNumbers + Convert.ToString(lLast8Bits, 16);
            if (lLast4BitSum != (aNumbers[11] >> 7) && (((lLast8Bits >> 1) | (lLast4BitSum << 7)) != aNumbers[11]))
            {
                throw new ArgumentOutOfRangeException("One or more words are incorrect");
            }

            lHex = SumAllNumbers.ToString();
            return lHex;
        }

        public void Dispose()
        {
            FWalletPandoraServer?.Dispose();
        }
    }
}