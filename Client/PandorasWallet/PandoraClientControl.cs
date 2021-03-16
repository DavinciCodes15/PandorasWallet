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

using Pandora.Client.ClientLib;
using Pandora.Client.PandorasWallet.Dialogs;
using Pandora.Client.PandorasWallet.Wallet;
using Pandora.Client.Universal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Pandora.Client.PandorasWallet.ServerAccess;
using Pandora.Client.PandorasWallet.Controlers;
using System.Text;
using Pandora.Client.PandorasWallet.SystemBackup;
using Pandora.Client.SystemBackup;
using Pandora.Client.Crypto.Currencies;
using System.Numerics;
using Pandora.Client.PandorasWallet.Dialogs.Models;
using Pandora.Client.PandorasWallet.Models;
using Pandora.Client.ClientLib.Contracts;
using System.Text.RegularExpressions;
using Pandora.Client.PandorasWallet.Wallet.TransactionMaker;
using Nethereum.Signer;

namespace Pandora.Client.PandorasWallet
{
    public delegate bool PasswordValidationDelegate(string aPassword);

    internal class PandoraClientControl : IDisposable

    {
        private delegate void DelegateSendTransaction(CurrencyItem lSelectedCoin, string aToAddress, decimal aAmount, decimal aTxFee);

        private delegate void DelegateDisplayCurrency(CurrencyItem aCurrency, Exception ex, CancellationToken aCancelToken);

        private delegate void DelegateDisplayCurrencyToken(ClientCurrencyTokenItem aCurrencyToken, Exception ex, CancellationToken aCancelToken);

        private static PandoraClientControl FPandoraClientControl;
        private PandoraEnchangeControl FExchangeControl;

        private ServerConnection FServerConnection
        {
            get;
            set;
        }

        private string FSettingsFile;
        private KeyManager FKeyManager;
        private CancellationTokenSource FCancellationTokenSource;

        private DefaultCurrencySelectorDialog DefaultCoinSelectorDialog { get; set; }
        private AddCoinSelector AddCoinSelectorDialog { get; set; }

        public CoreSettings Settings { get; private set; }
        public AppMainForm AppMainForm { get; private set; }
        public string DefaultPath { get; private set; }

        private CryptoCurrencyAdvocacy FCryptoCurrencyAdvocacy;

        /*************************************************************************************************************************\
         *                                                                                                                       *
         *          Initialization methods                                                                                       *
         *                                                                                                                       *
        \*************************************************************************************************************************/

        #region Initialization methods

        private PandoraClientControl(AppMainForm aMainWindow)
        {
            LoadSettings();
            CreateLog();

            AppMainForm = aMainWindow;
            AppMainForm.OnConnect += MainForm_OnConnect;
            AppMainForm.FormClosing += MainForm_FormClosing;
            AppMainForm.OnTransactionSend += AppMainForm_OnTransactionSend;
            AppMainForm.OnSendAllMenuClick += AppMainForm_OnSendAllMenuClick;
            AppMainForm.OnSelectedCurrencyChanged += AppMainForm_OnSelectedCurrencyChanged;
            AppMainForm.OnAddCurrencyBtnClick += AppMainForm_OnAddCurrencyBtnClick;
            AppMainForm.OnAddTokenBtnClick += AppMainForm_OnAddTokenBtnClick;
            AppMainForm.OnBackupClick += MainForm_OnBackupClick;
            AppMainForm.OnSettingsMenuClick += MainForm_OnSettingsMenuClick;
            AppMainForm.OnChangePassword += AppMainForm_OnChangePassword;
            AppMainForm.OnRemoveCurrencyRequested += AppMainForm_OnRemoveCurrencyRequested;
            AppMainForm.TickerQuantity = string.Empty;
            AppMainForm.TickerTotalReceived = string.Empty;
            AppMainForm.OnClearWalletCache += AppMainForm_OnClearWalletCache;
            AppMainForm.OnSignMessage += AppMainForm_OnSignMessage;
        }

        private void SetDefaultCurrencyTokens()
        {
            var lUserTokens = FServerConnection.GetCurrencyTokens();
            var lDefaultTokens = ClientCurrencyTokenItem.DefaultInventory.GetTokens();

            foreach (var lDefaultToken in lDefaultTokens)
            {
                var lUserDefaultToken = lUserTokens.FirstOrDefault(lUserToken => string.Equals(lUserToken.ContractAddress, lDefaultToken.ContractAddress, StringComparison.OrdinalIgnoreCase));
                if (lUserDefaultToken == null)
                    FServerConnection.RegisterNewCurrencyToken(lDefaultToken);
                else if (lUserDefaultToken.Id != lDefaultToken.Id)
                {
                    Log.Write(LogLevel.Warning, $"Found default token already added, updating token data. Default Token: {lDefaultToken.Name} - ID: {lDefaultToken.Id}. User Token: {lUserDefaultToken.Name} - ID: {lUserDefaultToken.Id}");
                    lUserDefaultToken.Name = lDefaultToken.Name;
                    lUserDefaultToken.Ticker = lDefaultToken.Ticker;
                    lUserDefaultToken.Precision = lDefaultToken.Precision;
                    lUserDefaultToken.Icon = lDefaultToken.Icon;
                    FServerConnection.RegisterNewCurrencyToken(lUserDefaultToken);
                }
            }
        }

        private void AppMainForm_OnClearWalletCache(object sender, EventArgs e)
        {
            try
            {
                string lMessage = "Do you want clear your wallet cache?\nPlease be patient, your balances will be reseted and reloaded.";
                string lTitle = "Clear Wallet Cache";
                bool msgbox = AppMainForm.StandardWarningMsgBoxAsk(lTitle, lMessage);
                if (msgbox)
                {
                    FServerConnection.ClearCacheWallet();
                    Log.Write(LogLevel.Info, "************* Cleared cache **************");
                }
            }
            catch (Exception ex)
            {
                AppMainForm.StandardExceptionMsgBox(ex, "Error");
                Log.Write(LogLevel.Error, "Error: " + ex.Message);
            }
        }

        public void AppMainForm_OnSignMessage(object sender, EventArgs e)
        {
            var lCurrencies = AppMainForm.Currencies.Where(lGUICurrency => !(lGUICurrency is IGUICurrencyToken));
            SignMessageDialog lSignMessage = new SignMessageDialog(lCurrencies);
            lSignMessage.OnSingMessageNeeded += SignMessage_OnSingMessageNeeded;
            lSignMessage.OnVerifyMessageNeeded += SignMessage_OnVerifyMessageNeeded;
            lSignMessage.Execute();
        }

        private bool SignMessage_OnVerifyMessageNeeded(IGUICurrency aGUICurrency, string aMessage, string aSignature, out string aAddress)
        {
            bool lResult = false;
            aAddress = null;
            if (FromUserDecryptWallet(false))
            {
                var lCurrency = FServerConnection.GetCurrency(aGUICurrency.Id);
                var lAdvocacy = FKeyManager.GetCurrencyAdvocacy(lCurrency.Id, lCurrency.ChainParamaters);
                lResult = lAdvocacy.VerifyMessage(aMessage, aSignature, out aAddress);
            }
            return lResult;
        }

        private string SignMessage_OnSingMessageNeeded(IGUICurrency aGUICurrency, string aMessage, out string aAddress)
        {
            string lResult = null;
            aAddress = null;
            if (FromUserDecryptWallet(false))
            {
                var lCurrency = FServerConnection.GetCurrency(aGUICurrency.Id);
                var lAdvocacy = FKeyManager.GetCurrencyAdvocacy(lCurrency.Id, lCurrency.ChainParamaters);
                aAddress = lAdvocacy.GetAddress(1);
                lResult = lAdvocacy.SignMessage(aMessage, aAddress);
            }

            return lResult;
        }

        private void AppMainForm_OnAddTokenBtnClick(object sender, EventArgs e)
        {
            var lUserTokens = FServerConnection.GetCurrencyTokens();
            var lVisibleTokens = FServerConnection.GetDisplayedCurrencyTokens().Select(lToken => lToken.Id);
            var lAddTokenDialog = new AddTokenDialog();
            lAddTokenDialog.OnTokenAddressChanged += AddTokenDialog_OnTokenAddressChanged;

            //We add all currencies that are ethereum like
            foreach (var lEthereumCurrency in FServerConnection.GetCurrencies().Where(lCurrency => lCurrency.ChainParamaters.Capabilities.HasFlag(CapablityFlags.EthereumProtocol)))
                lAddTokenDialog.AddParentCurrency(lEthereumCurrency);

            //Now we add all registered tokens, disabling those already at the main interface
            foreach (var lToken in lUserTokens)
            {
                var lCurrency = FServerConnection.GetCurrency(lToken.ParentCurrencyID);
                if (lCurrency != null)
                {
                    var lParentGUICurrency = AppMainForm.GetCurrency(lToken.ParentCurrencyID) ?? GUIModelProducer.CreateFrom(lCurrency);
                    lAddTokenDialog.AddTokenItem(GUIModelProducer.CreateFrom(lToken, lParentGUICurrency), lVisibleTokens.Contains(lToken.Id));
                }
            }

            if (lAddTokenDialog.Execute() && FromUserDecryptWallet(Settings.RequestWalletPassword))
            {
                try
                {
                    var lDialogToken = lAddTokenDialog.SelectedToken;
                    var lAddedTokenItem = lUserTokens.SingleOrDefault(lToken => lToken.ContractAddress == lDialogToken.ContractAddress);
                    if (lAddedTokenItem == null)
                    {
                        long lMinTokenID = -1000;
                        if (lUserTokens.Any() && lUserTokens.Min(lToken => lToken.Id) < -1000)
                            lMinTokenID = lUserTokens.Min(lToken => lToken.Id);

                        lAddedTokenItem = new ClientCurrencyTokenItem()
                        {
                            ContractAddress = lDialogToken.ContractAddress,
                            ParentCurrencyID = lDialogToken.ParentCurrency.Id,
                            Name = lDialogToken.Name,
                            Ticker = lDialogToken.Ticker,
                            Precision = lDialogToken.Precision,
                            Icon = Globals.IconToBytes(lDialogToken.Icon),
                            Id = lMinTokenID - 3 //Token Ids are negatives
                        };
                        FServerConnection.RegisterNewCurrencyToken(lAddedTokenItem);
                    }
                    FServerConnection.SetDisplayedCurrencyToken(lAddedTokenItem.Id, true);
                    AppMainForm.BeginInvoke(new DelegateDisplayCurrencyToken(Event_DisplayCurrencyToken), new object[] { lAddedTokenItem, null, FCancellationTokenSource.Token });
                }
                catch (Exception ex)
                {
                    AppMainForm.BeginInvoke(new DelegateDisplayCurrencyToken(Event_DisplayCurrencyToken), new object[] { null, ex, FCancellationTokenSource.Token });
                }
            }
        }

        private ICurrencyToken AddTokenDialog_OnTokenAddressChanged(AddTokenDialog aDialog, long aCurrencyID, string aAddress)
        {
            ICurrencyToken lResult = null;
            try
            {
                if (aCurrencyID >= 0)
                    lResult = FServerConnection.DirectGetCurrencyToken(aCurrencyID, aAddress);
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Error, $"Exception thrown when reaching for token information ({aAddress}). Details: {ex}");
            }
            return lResult;
        }

        private Dictionary<long, string> FLastAddresses = new Dictionary<long, string>();
        private Dictionary<long, decimal> FLastSentValue = new Dictionary<long, decimal>();
        private long FPrevSelectedCurrencyID = -1;

        private void AppMainForm_OnRemoveCurrencyRequested(long aCurrencyID)
        {
            RemoveCurrency(aCurrencyID);
        }

        private void AppMainForm_OnSelectedCurrencyChanged(object sender, EventArgs e)
        {
            if (FPrevSelectedCurrencyID > 0)
            {
                FLastAddresses[FPrevSelectedCurrencyID] = AppMainForm.ToSendAddress;
                FLastSentValue[FPrevSelectedCurrencyID] = AppMainForm.ToSendAmount;
            }
            if (FLastAddresses.TryGetValue(AppMainForm.SelectedCurrencyId, out string lAddress))
                AppMainForm.ToSendAddress = lAddress;
            else
                AppMainForm.ToSendAddress = string.Empty;
            if (FLastSentValue.TryGetValue(AppMainForm.SelectedCurrencyId, out decimal lAmountValue))
                AppMainForm.ToSendAmount = lAmountValue;
            else
                AppMainForm.ToSendAmount = 0;
            FPrevSelectedCurrencyID = AppMainForm.SelectedCurrencyId;
        }

        private void CreateLog()
        {
            PandoraLog lLog = PandoraLog.GetPandoraLog();
            Log.LogLevelFlag = Settings.LogLevelFlags;
            lLog.FileName = Settings.LogFileName;
            lLog.LineLength = Settings.LogLineLength;
            lLog.MaxSize = Settings.LogMaxSize;
            lLog.Active = true;
            Log.SystemLog = lLog;
            Log.Write("************************************************************************************************************************");
            Log.Write("Pandora's Wallet Started.");
            Log.Write("************************************************************************************************************************");
            Log.WriteAppEvent("Pandora Client log started.", System.Diagnostics.EventLogEntryType.Information, 6005);
        }

        private void LoadSettings()
        {
            DefaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString(), "Pandora's Wallet");

            var lSettingsPath = Path.Combine(DefaultPath, "Settings"); // settings path is fixed here but the user can change the data path to where ever.
            try
            {
                if (!Directory.Exists(DefaultPath))
                    Directory.CreateDirectory(DefaultPath);
                if (!Directory.Exists(lSettingsPath))
                    Directory.CreateDirectory(lSettingsPath);
            }
            catch (Exception e)
            {
                throw new Exception($"Error creating folder '{lSettingsPath}' : {e.Message}");
            }

            FSettingsFile = Path.Combine(lSettingsPath, "PandorasWallet.ini");
            try
            {
                if (!File.Exists(FSettingsFile))
                    File.WriteAllText(FSettingsFile, ""); // create empty settings file to set up defaults
                Settings = CoreSettings.LoadSettings(FSettingsFile);
            }
            catch (Exception e)
            {
                throw new Exception($"Error loading settings from '{lSettingsPath}' : {e.Message} : {e.Source}");
            }
            if (Settings.DataPath == "") // we will ignore old data path and create the new one.
                Settings.DataPath = DefaultPath;
            if (Settings.LogFileName == "")
                Settings.LogFileName = Path.Combine(Settings.DataPath, "debug.log");
            CoreSettings.SaveSettings(Settings, FSettingsFile);
        }

        public static PandoraClientControl GetExistingInstance()
        {
            return FPandoraClientControl;
        }

        public static PandoraClientControl GetInstance()
        {
            return FPandoraClientControl;
        }

        public static PandoraClientControl GetController(AppMainForm aPandoraClientMainWindow)
        {
            if (FPandoraClientControl == null)
            {
                FPandoraClientControl = new PandoraClientControl(aPandoraClientMainWindow);
            }

            return FPandoraClientControl;
        }

        #endregion Initialization methods

        /*************************************************************************************************************************\
         *                                                                                                                       *
         *         From User Request Code                                                                                        *
         *                                                                                                                       *
        \*************************************************************************************************************************/

        #region Form User Request Code

        public bool FromUserDecryptWallet(bool aRequirePassword)
        {
            if (!FKeyManager.IsPasswordSet || aRequirePassword)
            {
                string lPassword = FromUserGetPassword(new PasswordValidationDelegate(ValidatePasswordHash));
                if (lPassword == null) return false;
                FKeyManager.SetPassword(lPassword);
                lPassword = null;
            }
            return true;
        }

        private void FromUserSendAmount(decimal aAmountToSend, bool aSubtractFee)
        {
            var lSelectedCurrency = AppMainForm.SelectedCurrency;
            TransactionUnit[] lUnspent;
            bool lIsToken = lSelectedCurrency.Id < 0;
            //If form id is less than 0, then it is a token
            if (lIsToken)
            {
                lUnspent = FServerConnection.GetUnspentOutputs(((GUIToken) lSelectedCurrency).ParentCurrency.Id);
            }
            else
            {
                lUnspent = FServerConnection.GetUnspentOutputs(lSelectedCurrency.Id);
            }
            if (string.IsNullOrWhiteSpace(AppMainForm.ToSendAddress))
                AppMainForm.StandardErrorMsgBox("Send Error", $"Please provide a valid {AppMainForm.SelectedCurrency.Name} address!");
            else if (aAmountToSend < 0)
                AppMainForm.StandardErrorMsgBox("Send Error", $"The amount '{aAmountToSend}' is an invalid amount for {AppMainForm.SelectedCurrency.Name}!");
            else if (!lUnspent.Any())
                AppMainForm.StandardErrorMsgBox("Send Error", $"No balance for {AppMainForm.SelectedCurrency.Name}!");
            else
            {
                var lTxFee = CalculateTxFee(AppMainForm.ToSendAddress, aAmountToSend, AppMainForm.SelectedCurrency, out decimal lFeePerKb);
                ExecuteSendTxDialog(aAmountToSend, AppMainForm.SelectedCurrency, lTxFee, lFeePerKb, AppMainForm.SelectedCurrency.Balances.Total, AppMainForm.ToSendAddress, lUnspent[0].Address, aSubtractFee);
            }
        }

        public string FromUserGetNewPassword(PasswordValidationDelegate aValidationHandler)
        {
            Log.Write(LogLevel.Info, "Getting new password from user {0}.", FServerConnection.UserName);
            var lDlg = new PasswordDialog
            {
                ParentWindow = AppMainForm
            };
            lDlg.OnVerifyPassword += aValidationHandler;
            if (!lDlg.Execute(true)) return null;
            return lDlg.Password;
        }

        public string FromUserGetPassword(PasswordValidationDelegate aValidationHandler)
        {
            Log.Write(LogLevel.Info, "Getting password from user {0}.", FServerConnection.UserName);
            PasswordDialog lDlg = new PasswordDialog();
            lDlg.OnVerifyPassword += aValidationHandler;
            lDlg.ParentWindow = AppMainForm;
            if (!lDlg.Execute()) return null;
            return lDlg.Password;
        }

        public long FromUserGetDefaultCurrencyId(long aCurrentDefaultCurrencyId = 0)
        {
            //
            //NOTE: The dialog is field property because it is used in
            //      ServerConnection_OnNewCurrency to update it's currency.
            //
            DefaultCoinSelectorDialog = new DefaultCurrencySelectorDialog();
            long lResult = 0;
            var lDialogList = new List<DefaultCurrencySelectorDialog.DialogCurrencyItem>();
            foreach (var lCurrencyItem in FServerConnection.GetCurrencies())
                lDialogList.Add(new DefaultCurrencySelectorDialog.DialogCurrencyItem() { CurrencyID = lCurrencyItem.Id, CurrencyName = lCurrencyItem.Name, CurrencySymbol = lCurrencyItem.Ticker, CurrencyIcon = SystemUtils.BytesToIcon(lCurrencyItem.Icon) });
            DefaultCoinSelectorDialog.AddDialogCurrencyItems(lDialogList);
            //TODO:suppot following code
            //DefaultCoinSelectorDialog.SelectedCurrencyID = aCurrentDefaultCurrencyId;
            //so that the current Default coin is selected.
            if (DefaultCoinSelectorDialog.Execute()) lResult = DefaultCoinSelectorDialog.SelectedCurrencyID;
            DefaultCoinSelectorDialog = null;
            return lResult;
        }

        private bool ValidateNewPassword(string aPassword)
        {
            return (aPassword.Length > 8);
        }

        #endregion Form User Request Code

        /*************************************************************************************************************************\
         *                                                                                                                       *
         *          Login Event code                                                                                             *
         *                                                                                                                       *
        \*************************************************************************************************************************/

        #region Login Event Code

        private void MainForm_OnConnect(object sender, EventArgs e)
        {
            var lConnectDialog = new ConnectDialog
            {
                ParentWindow = AppMainForm
            };

            lConnectDialog.findUsersClick += ConnectDialog_findUsersClick;

            lConnectDialog.OnOkClick += new EventHandler(delegate (Object o, EventArgs a)
            {
                ConnectDialog lDlg = o as ConnectDialog;
                var lConnection = new ServerConnection(Settings.DataPath, AppMainForm);
                lConnection.AutoUpdate = Settings.AutoUpdate;
                Log.Write(LogLevel.Debug, "Connecting to server {0} with port {1} and encryption {2}, user '{3} - {4}'", Settings.ServerName, Settings.Port, Settings.EncryptedConnection, lDlg.Username, lDlg.Email);
                //NOTE:  Do not change below to lowercase ever because it used for
                //       older user that had the wrong case.
                Settings.UserName = lDlg.Username;
                Settings.Email = lDlg.Email;
                // --------------------------------------------
                lDlg.UserConnected = lConnection.LogIn(Settings.ServerName, Settings.Port, Settings.EncryptedConnection, lDlg.Email, lDlg.Username, lDlg.Password);
                if (lDlg.UserConnected)
                {
                    FServerConnection?.Dispose();
                    AppMainForm.ClearAll();
                    FExchangeControl?.Dispose();
                    FCancellationTokenSource?.Cancel();
                    FTask?.Wait();

                    FCancellationTokenSource = new CancellationTokenSource();
                    FServerConnection = lConnection;
                    FExchangeControl = new PandoraEnchangeControl(FServerConnection);
                    FExchangeControl.GetKeyManagerMethod += Exchange_GetKeyManager;
                    FExchangeControl.SetKeyManagerPassword += () => FromUserDecryptWallet(Settings.RequestWalletPassword);
                    FTransactionMakerFactory = new TransactionMakerFactory(FServerConnection);
                    Log.Write(LogLevel.Debug, "user Connected and is New Account = {0}", FServerConnection.NewAccount);
                }
                else
                    Log.Write(LogLevel.Debug, "Failed to connect");
            });

            lConnectDialog.OnCallSettingDialog += (s, a) =>
            {
                using (ConnectionSettingsDialog lConnectionSettingsDialog = new ConnectionSettingsDialog() { ParentWindow = lConnectDialog })
                {
                    lConnectionSettingsDialog.ServerName = Settings.ServerName;
                    lConnectionSettingsDialog.PortNumber = Settings.Port;
                    lConnectionSettingsDialog.EncryptConnection = Settings.EncryptedConnection;

                    if (lConnectionSettingsDialog.Execute())
                    {
                        Settings.ServerName = lConnectionSettingsDialog.ServerName;
                        Settings.Port = lConnectionSettingsDialog.PortNumber;
                        Settings.EncryptedConnection = lConnectionSettingsDialog.EncryptConnection;
                        CoreSettings.SaveSettings(Settings, FSettingsFile);
                    }
                }
            };

            Log.Write(LogLevel.Debug, "OnConnect called to log into server");
            //AppMainForm.Connected = FServerConnection != null && FServerConnection.Connected;
            // Create login dialog and setup
            lConnectDialog.Username = Settings.UserName;
            lConnectDialog.Email = Settings.Email;
            lConnectDialog.UserConnected = false;
            lConnectDialog.SavePassword = Settings.SaveLogingPasswords;
            lConnectDialog.ClearAccounts();
            var lLoginAccounts = LoginHistoryToLoginAccounts(Settings.LoginHistory);
            foreach (var lLoginAccount in lLoginAccounts)
                lConnectDialog.AddLoginAccount(lLoginAccount);

            if (lConnectDialog.Execute())
            {
                AppMainForm.Connected = false; // App in bad state if an exception is thrown in the middle of setup so it will shutdown.
                FKeyManager = null;
                //MainForm.ClearAll();
                Log.Write(LogLevel.Debug, "User: {0} - {1} connected.", Settings.Email, Settings.UserName);
                UpdateLoginHistory(FServerConnection.Email, FServerConnection.UserName, lConnectDialog.Password, lConnectDialog.SavePassword);
                Settings.SaveLogingPasswords = lConnectDialog.SavePassword;
                // Go and get the correct case for the username and email for old verion of
                // the software for obtaining the key if no old verion exists it returns the default
                //
                CoreSettings.SaveSettings(Settings, FSettingsFile);
                Log.Write(LogLevel.Info, "App Settings saved.");
                //TODO: The following line updates dialog boxes so that they have new currencies that the DB does not have
                //      We need to support update Currency so it changes to icon name or ticker on the fly.
                //      where ever it is displayed.
                FServerConnection.OnNewCurrency += ServerConnection_OnNewCurrency;
                if (FServerConnection.NewAccount)
                    BuildNewAccount();
                else if (FServerConnection.GetDefaultCurrency() == null)
                    if (!ExecuteRestoreWizard())
                        throw new ClientExceptions.LoginFailedException("Restore required to continue.");
                LoadCurrentUser();

                FServerConnection.OnNewTransaction += ServerConnection_OnNewTransaction;
                FServerConnection.OnUpdatedTransaction += ServerConnection_OnUpdatedTransaction;
                FServerConnection.OnBlockHeightChange += ServerConnection_OnBlockHeightChange;
                FServerConnection.OnCurrencyStatusChange += ServerConnection_OnCurrencyStatusChange;
                FServerConnection.OnUpdatedCurrency += ServerConnection_OnUpdatedCurrency;
                FServerConnection.OnUpdatedCurrencyParams += ServerConnection_OnUpdatedCurrencyParams;
                FServerConnection.OnUpgradeFileReady += ServerConnection_OnUpgradeFileReady;
            }
            else
                Log.Write(LogLevel.Debug, "Not connected.. last User: {0} - {1} ", lConnectDialog.Username, lConnectDialog.Email);
            CoreSettings.SaveSettings(Settings, FSettingsFile);
            AppMainForm.Connected = FServerConnection != null && FServerConnection.Connected;
            if (!AppMainForm.Connected)
                AppMainForm.Close();
        }

        private void ResetConnection(object s, EventArgs e)
        {
            //TODO: The following line updates dialog boxes so that they have new currencies that the DB does not have
            //      We need to support update Currency so it changes to icon name or ticker on the fly.
            //      where ever it is displayed.
            FServerConnection.OnNewCurrency += ServerConnection_OnNewCurrency;
            if (FServerConnection.NewAccount)
                BuildNewAccount();
            else if (FServerConnection.GetDefaultCurrency() == null)
                if (!ExecuteRestoreWizard())
                    throw new ClientExceptions.LoginFailedException("Restore required to continue.");
            LoadCurrentUser();

            FServerConnection.OnNewTransaction += ServerConnection_OnNewTransaction;
            FServerConnection.OnUpdatedTransaction += ServerConnection_OnUpdatedTransaction;
            FServerConnection.OnBlockHeightChange += ServerConnection_OnBlockHeightChange;
            FServerConnection.OnCurrencyStatusChange += ServerConnection_OnCurrencyStatusChange;
            FServerConnection.OnUpdatedCurrency += ServerConnection_OnUpdatedCurrency;
            FServerConnection.OnUpdatedCurrencyParams += ServerConnection_OnUpdatedCurrencyParams;
            FServerConnection.OnUpgradeFileReady += ServerConnection_OnUpgradeFileReady;
        }

        private void ServerConnection_OnUpgradeFileReady(object aSender, string aFileName)
        {
            Program.UpgradeFileName = aFileName;
        }

        private void ServerConnection_OnUpdatedCurrencyParams(CurrencyItem aCurrencyItem, Crypto.Currencies.IChainParams aOldChainParams)
        {
            var lMonitoredAccounts = FServerConnection.GetMonitoredAccounts(aCurrencyItem.Id);
            if (!lMonitoredAccounts.Any()) return;
            try
            {
                FServerConnection.CacheHelper.ReconstructCurrencyData(aCurrencyItem, GetAddressesFromChainParams);
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Critical, $"Fatal error when reconstructing currency data for {aCurrencyItem.Name} (id: {aCurrencyItem.Id}). Exception: {ex}");
                AppMainForm.Invoke(new Action(() => AppMainForm.StandardErrorMsgBox("Critical Error", $"{ex.Message}{Environment.NewLine}Unable to update coin data. If error persist, please contact support@davincicodes.net. The application will close.")));
                AppMainForm.Close();
            }
            RefreshCurrencyView(aCurrencyItem);
        }

        private void RefreshCurrencyView(CurrencyItem aCurrencyItem)
        {
            try
            {
                var lFormCurrency = AppMainForm.GetCurrency(aCurrencyItem.Id);
                if (lFormCurrency != null)
                {
                    var lAddresses = FServerConnection.GetMonitoredAddresses(aCurrencyItem.Id);
                    lFormCurrency.Transactions.ClearTransactions();
                    var lTxRecords = FServerConnection.GetTransactionRecords(aCurrencyItem.Id).Select(lTx => GUIModelProducer.CreateFrom(lTx, aCurrencyItem, lAddresses));
                    foreach (var lTx in lTxRecords)
                        lFormCurrency.Transactions.AddTransaction(lTx);
                    lFormCurrency.Balances.UpdateBalance();
                    AppMainForm.BeginInvoke((Action<long>) AppMainForm.RefreshTransactions, aCurrencyItem.Id);
                    var lAppMainFormAccounts = new List<GUIAccount>();
                    int lIndex = 0;
                    foreach (var lAddress in lAddresses)
                        lAppMainFormAccounts.Add(GUIModelProducer.CreateFrom(lAddress, $"{lIndex++}"));
                    lFormCurrency.Addresses = lAppMainFormAccounts.ToArray();
                    AppMainForm.BeginInvoke((Action) (() => AppMainForm.UpdateCurrency(lFormCurrency.Id)));
                }
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Error, $"Exception thrown when performing refreshcurrencyview of {aCurrencyItem.Id}. Details: {ex}");
            }
        }

        private IEnumerable<CurrencyAccount> GetAddressesFromChainParams(IEnumerable<CurrencyAccount> aServerMonitoresAccounts, long aCurrencyID, IChainParams aCurrencyChainParams)
        {
            List<CurrencyAccount> lResult = new List<CurrencyAccount>();
            if (FromUserDecryptWallet(Settings.RequestWalletPassword))
            {
                var lAdvocacy = FKeyManager.GetCurrencyAdvocacy(aCurrencyID, (ChainParams) aCurrencyChainParams);
                var lAddress1 = lAdvocacy.GetAddress(0);
                var lAddress2 = lAdvocacy.GetAddress(1);
                var lServerAddress = aServerMonitoresAccounts.FirstOrDefault();
                if (lServerAddress != null)
                {
                    if (lServerAddress.Address == lAddress1 || lServerAddress.Address == lAddress2)
                        lResult.Add(FServerConnection.DirectAddMonitoredAccount(lServerAddress.Address == lAddress1 ? lAddress2 : lAddress1, aCurrencyID));
                    lResult.Add(lServerAddress);
                }
                else
                {
                    lResult.Add(FServerConnection.DirectAddMonitoredAccount(lAdvocacy.GetAddress(0), aCurrencyID));
                    lResult.Add(FServerConnection.DirectAddMonitoredAccount(lAdvocacy.GetAddress(1), aCurrencyID));
                }
                return lResult;
            }
            else
                throw new Exception("Wallet is encrypted. Unable to continue operation.");
        }

        private void ServerConnection_OnUpdatedCurrency(object aSender, CurrencyItem aCurrencyItem)
        {
            var lDisplayedCurrency = AppMainForm.GetCurrency(aCurrencyItem.Id);
            if (lDisplayedCurrency != null)
            {
                lDisplayedCurrency.CopyFrom(aCurrencyItem);
                AppMainForm.BeginInvoke((Func<long, bool>) AppMainForm.UpdateCurrency, lDisplayedCurrency.Id);
            }
        }

        private void ServerConnection_OnCurrencyStatusChange(object aSender, CurrencyStatusItem aCurrencyStatusItem)
        {
            var lDisplayedCurrency = AppMainForm.GetCurrency(aCurrencyStatusItem.CurrencyId);
            if (lDisplayedCurrency != null)
            {
                lDisplayedCurrency.CurrentStatus = aCurrencyStatusItem.Status;
                lDisplayedCurrency.StatusDetails.StatusMessage = aCurrencyStatusItem.ExtendedInfo;
                lDisplayedCurrency.StatusDetails.StatusTime = aCurrencyStatusItem.StatusTime;
                AppMainForm.BeginInvoke((Func<long, bool>) AppMainForm.UpdateCurrency, lDisplayedCurrency.Id);
            }
        }

        private class DisplayCurrenciesArgs : EventArgs
        {
            public DisplayCurrenciesArgs() : base()
            {
            }

            public long LastSelectedCurrencyId;
            public int Index;
            public object[] ItemsToDisplay;
        }

        private void LoadCurrentUser()
        {
            if (FKeyManager == null)
            {
                FKeyManager = new KeyManager(FServerConnection.ReadKeyValue("SALT"))
                {
                    EncryptedRootSeed = FServerConnection.ReadKeyValue("EncryptedRootSeed")
                };
            }
            var lDefaultCurrency = FServerConnection.GetDefaultCurrency();
            // if the blow code is not done because of an error the next time
            // the user logs in they will need to repeat the following and enter the wallet password.
            // if we do not have 2 address for this account we must create it.
            if (FServerConnection.DefaultBitcoinAddress == null || FServerConnection.DefaultBitcoinAddress.Length < 2)
            {
                Log.Write(LogLevel.Warning, "Something went wrong we need to set the default currency from the Encrypted root seed.");
                if (!FromUserDecryptWallet(Settings.RequestWalletPassword)) throw new Exception("You must decrypt the wallet to continue.");
                var lAdvacacy = FKeyManager.GetCurrencyAdvocacy(1, FServerConnection.GetCurrency(1).ChainParamaters);
                // we add this to the server side to indicate the user is created
                // and this is the locked in address key and it must be the same
                // on recovery for this account.
                FServerConnection.AddMonitoredAccount(lAdvacacy.GetAddress(0), 1);
                FServerConnection.AddMonitoredAccount(lAdvacacy.GetAddress(1), 1);
            }
            else
            {
                // You have all the accounts stored on the server then we need to make sure
                // the addresses we have locally is correct on the server.
                var lBitcoinMonitoredAccounts = FServerConnection.GetMonitoredAccounts(1);
                foreach (var lCurrencyAccount in lBitcoinMonitoredAccounts)
                    if (!FServerConnection.ValidateDefaultAddressExists(lCurrencyAccount.Address))
                        throw new Exception($"The server has inconsistant information with the local database.  Please contact support@davincicodes.net.");
            }
            if (FServerConnection.GetMonitoredAccounts(lDefaultCurrency.Id).Count < 2)
            {
                if (!FromUserDecryptWallet(Settings.RequestWalletPassword)) throw new Exception("You must decrypt the wallet to continue.");
                var lAdvacacy = FKeyManager.GetCurrencyAdvocacy(lDefaultCurrency.Id, lDefaultCurrency.ChainParamaters);
                // Currency to be displayed.
                FServerConnection.AddMonitoredAccount(lAdvacacy.GetAddress(0), lDefaultCurrency.Id);
                FServerConnection.AddMonitoredAccount(lAdvacacy.GetAddress(1), lDefaultCurrency.Id);
            }
            FServerConnection.SetDisplayedCurrency(lDefaultCurrency.Id, true);

            //Now we load defaults tokens if it is necessary

            SetDefaultCurrencyTokens();

            // Now display the default currency first and
            // send a message to do the rest of the currencies.

            var lItemsToDisplay = new List<object>();
            lItemsToDisplay.AddRange(FServerConnection.GetDisplayedCurrencies().Where(lCurrency => lCurrency.Id != lDefaultCurrency.Id));
            lItemsToDisplay.AddRange(FServerConnection.GetDisplayedCurrencyTokens());

            var lArgs = new DisplayCurrenciesArgs
            {
                ItemsToDisplay = lItemsToDisplay.ToArray(),
                LastSelectedCurrencyId = lDefaultCurrency.Id
            };
            FServerConnection.SetDefaultCurrency(lDefaultCurrency.Id);

            DisplayCurrency(lDefaultCurrency);
            string lValue = FServerConnection.ReadKeyValue("LastSelectedCurrencyId");
            long.TryParse(lValue, out long lCurrencyId);
            AppMainForm.SetUserStatus(AppMainForm.UserStatuses.Connected, Settings.Email, Settings.UserName);
            AppMainForm.SelectedCurrencyId = lDefaultCurrency.Id;
            AppMainForm.BeginInvoke(new EventHandler(Display_NextCurrency), this, lArgs);
            try
            {
                FExchangeControl.StartProcess();
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Error, $"Error at pre initialization of exchange process. Exception: {ex}");
            }
        }

        private void BuildNewAccount()
        {
            Log.Write(LogLevel.Info, "Building new account.");
            string lPassword = FromUserGetNewPassword(new PasswordValidationDelegate(ValidateNewPassword));
            if (lPassword == null) throw new Exception("Password was not set. Application will close.");
            var lDefaultCurrencyId = FromUserGetDefaultCurrencyId();
            if (lDefaultCurrencyId < 1) throw new Exception("Default currency was not set. Application will close.");
#if DEBUG
            if (MessageBox.Show("You sure you want create this account?", "confirmation", MessageBoxButtons.YesNo) != DialogResult.Yes) throw new Exception("test new user setup");
#endif
            FKeyManager = new KeyManager(KeyManager.GenerateSalt());
            FKeyManager.SetPassword(lPassword);
            FKeyManager.SetRootSeed(KeyManager.GenerateRootSeed());
            PostUserInfo(FKeyManager.EncryptedRootSeed, KeyManager.HashPassword(FKeyManager.GetPassword()), FKeyManager.GetSalt(), lDefaultCurrencyId);
        }

        public void PostUserInfo(string aEncryptedRootSeed, string aPasswordHash, string aSalt, long aDefaultCurrencyId)
        {
            var lDictionary = new Dictionary<string, string>
            {
                { "SALT", aSalt },
                { "EncryptedRootSeed", aEncryptedRootSeed },
                { "PasswordHash", aPasswordHash },
                { "UserName", FServerConnection.UserName },
                { "Email", FServerConnection.Email }
            };
            FServerConnection.WriteAtomicKeyValue(lDictionary);
            FServerConnection.SetDefaultCurrency(aDefaultCurrencyId);
        }

        private void ServerConnection_OnNewCurrency(object aSender, CurrencyItem aCurrencyItem)
        {
#if !DEBUG
                if (aCurrencyItem.CurrentStatus == CurrencyStatus.Disabled) return;
#endif
            DefaultCoinSelectorDialog?.AddDialogCurrencyItem(new DefaultCurrencySelectorDialog.DialogCurrencyItem() { CurrencyID = aCurrencyItem.Id, CurrencyName = aCurrencyItem.Name, CurrencySymbol = aCurrencyItem.Ticker, CurrencyIcon = SystemUtils.BytesToIcon(aCurrencyItem.Icon) });
            AddCoinSelectorDialog?.AddCurrency(GUIModelProducer.CreateFrom(aCurrencyItem));
        }

        private void ServerConnection_OnBlockHeightChange(object aSender, long aCurrencyId, long aBlockHeight)
        {
            var lFormCurrency = AppMainForm.GetCurrency(aCurrencyId);
            if (lFormCurrency != null)
            {
                lFormCurrency.BlockHeight = aBlockHeight;
                AppMainForm.UpdateCurrency(lFormCurrency.Id);
            }
        }

        private void ServerConnection_OnUpdatedTransaction(object aSender, TransactionRecord aTransactionRecord, IEnumerable<ClientTokenTransactionItem> aTokenTransactions)
        {
            var lItem = GUIModelProducer.CreateFrom(aTransactionRecord,
            FServerConnection.GetCurrency(aTransactionRecord.CurrencyId),
            FServerConnection.GetMonitoredAddresses(aTransactionRecord.CurrencyId)
            );
            var lFormCurrency = AppMainForm.GetCurrency(aTransactionRecord.CurrencyId);
            lFormCurrency.Transactions.UpdateTransaction(lItem);
            if (!aTransactionRecord.Valid)
            {
                lFormCurrency.Transactions.RemoveTransaction(lItem);
                AppMainForm.RemoveTransaction(lItem);
            }
            AppMainForm.UpdateCurrency(lFormCurrency.Id);
        }

        private void ServerConnection_OnNewTransaction(object aSender, TransactionRecord aTransactionRecord, IEnumerable<ClientTokenTransactionItem> aTokenTransactions)
        {
            if (!aTransactionRecord.Valid) return;
            var lAddresses = FServerConnection.GetMonitoredAddresses(aTransactionRecord.CurrencyId);
            var lCurrency = FServerConnection.GetCurrency(aTransactionRecord.CurrencyId);
            var lFormTransaction = GUIModelProducer.CreateFrom(aTransactionRecord, lCurrency, lAddresses);
            AddNewFormTransaction(lFormTransaction, aTransactionRecord.CurrencyId);
            if (aTokenTransactions != null)
            {
                foreach (var lTokenTx in aTokenTransactions)
                {
                    var lToken = FServerConnection.GetCurrencyToken(lTokenTx.TokenAddress);
                    if (lToken != null)
                    {
                        lFormTransaction = GUIModelProducer.CreateFrom(lTokenTx, lToken, aTransactionRecord, lAddresses);
                        AddNewFormTransaction(lFormTransaction, lToken.Id);
                    }
                }
            }
        }

        private void AddNewFormTransaction(GUITransaction aFormTransaction, long aCurrencyID)
        {
            if (AppMainForm.SelectedCurrency.Id == aCurrencyID)
            {
                if (AppMainForm.SelectedCurrency.Transactions.FindTransaction(aFormTransaction.RecordId) == null)
                {
                    AppMainForm.SelectedCurrency.Transactions.AddTransaction(aFormTransaction);
                    AppMainForm.AddTransaction(aFormTransaction);
                    AppMainForm.UpdateCurrency(AppMainForm.SelectedCurrencyId);
                }
                else
                    Log.Write(LogLevel.Error, "transaction {0} - {1} found as new but already exits.", aFormTransaction.TxId, aFormTransaction.RecordId);
            }
            else
            {
                var lFormCurrency = AppMainForm.GetCurrency(aCurrencyID);
                if (lFormCurrency != null)
                {
                    lFormCurrency.Transactions.AddTransaction(aFormTransaction);
                    AppMainForm.UpdateCurrency(lFormCurrency.Id);
                }
            }
        }

        private void ServerConnection_SendTransactionCompleted(object aSender, string aErrorMsg, string aTxId)
        {
            FSendingTxDialog.Response(aErrorMsg, aTxId);
        }

        #endregion Login Event Code

        /*************************************************************************************************************************\
         *                                                                                                                       *
         *          Close Event code                                                                                             *
         *                                                                                                                       *
        \*************************************************************************************************************************/

        #region Close Event Code

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            FExchangeControl?.StopProcess();
            FCancellationTokenSource?.Cancel();
            FTask?.Wait();
            FServerConnection?.Logoff();
        }

        public void Dispose()
        {
            (Log.SystemLog as PandoraLog).Active = false;
            FExchangeControl?.Dispose();
            FServerConnection?.Dispose();
        }

        #endregion Close Event Code

        /*************************************************************************************************************************\
         *                                                                                                                       *
         *          Send Coins Event code                                                                                        *
         *                                                                                                                       *
        \*************************************************************************************************************************/

        #region Send Coins Event Code

        private void AppMainForm_OnTransactionSend(object sender, EventArgs e)
        {
            FromUserSendAmount(AppMainForm.ToSendAmount, false);
        }

        private int CalculateEthTransactionNonce(long aCurrencyID)
        {
            int lResult = -1;
            var lCurrency = FServerConnection.GetCurrency(aCurrencyID);
            if (lCurrency.ChainParamaters.Capabilities.HasFlag(CapablityFlags.EthereumProtocol))
            {
                var lMonitoredAddresses = FServerConnection.GetMonitoredAddresses(aCurrencyID);
                var lTransactionOutputs = FServerConnection.GetTransactionRecords(aCurrencyID).Where(lTx => lTx.Inputs != null).SelectMany(lTx => lTx.Outputs).ToArray();
                if (lTransactionOutputs.Any())
                    lResult = lTransactionOutputs.Where(lTxOut => !string.IsNullOrEmpty(lTxOut.Script)).Max(lTxOut => lTxOut.Index);
            }
            return lResult;
        }

        private string ExecuteSendTxDialog(decimal aAmount, ICurrencyItem aSelectedCoin, decimal aTxFee, decimal aFeePerKb, decimal aBalance, string aAddress, string aFromAddress, bool aSubtractFee)
        {
            string lTicker = aSelectedCoin.Ticker;
            bool lIsEthereum = aSelectedCoin.ChainParamaters.Capabilities.HasFlag(CapablityFlags.EthereumProtocol);
            bool lIsToken = aSelectedCoin.Id < 0;
            int lNonce = CalculateEthTransactionNonce(lIsToken ? ((ICurrencyToken) aSelectedCoin).ParentCurrencyID : aSelectedCoin.Id);

            var lSendTxDialogInfo = new SendTransactionDialog.SendTransactionInfo
            {
                BalanceTicker = aSelectedCoin.Ticker,
                FeeTicker = lIsToken ? FServerConnection.GetCurrency(((ICurrencyToken) aSelectedCoin).ParentCurrencyID).Ticker : aSelectedCoin.Ticker,
                AmountToSend = aAmount,
                CurrentBalance = aBalance,
                FeeRate = aFeePerKb,
                FromAddress = aFromAddress,
                ToAddress = aAddress,
                TotalFee = aTxFee,
                IsSentAll = aSubtractFee,
                Nonce = lNonce
            };

            var lSendTransactionDlg = new SendTransactionDialog(lSendTxDialogInfo, lIsEthereum)
            {
                ParentWindow = AppMainForm
            };
            string lRetunedTxID = null;

            if (lSendTransactionDlg.Execute() && FromUserDecryptWallet(true))
            {
                var lAdvancedTxOptions = lSendTransactionDlg.AdvancedTxOptions;
                FSendingTxDialog = new SendingTxDialog();
                FSendingTxDialog.ParentWindow = AppMainForm;
                Task.Run(() =>
                {
                    Event_SendTransaction(aSelectedCoin, aAddress, lSendTransactionDlg.TotalAmountToSend, aTxFee, lAdvancedTxOptions);
                });
                FSendingTxDialog.Execute();
            }
            return lRetunedTxID;
        }

        private void Event_SendTransaction(ICurrencyItem aSelectedCoin, string aToAddress, decimal aAmount, decimal aTxFee, params object[] aExtOptions)
        {
            try
            {
                var lTx = GetNewSignedTransaction(aSelectedCoin, aToAddress, aAmount, aTxFee, aExtOptions);
                SendNewTransaction(lTx, aSelectedCoin, new DelegateOnSendTransactionCompleted(ServerConnection_SendTransactionCompleted));
            }
            catch (Exception ex)
            {
                AppMainForm.BeginInvoke(new DelegateOnSendTransactionCompleted(ServerConnection_SendTransactionCompleted), new object[] { this, ex.Message, null });
            }
        }

        private SendingTxDialog FSendingTxDialog;
        private Task FTask;
        private TransactionMakerFactory FTransactionMakerFactory;

        internal string GetNewSignedTransaction(ICurrencyItem aSelectedCoin, string aToAddress, decimal aAmount, decimal aTxFee, params object[] aExtParams)
        {
            var lTransactionMaker = FTransactionMakerFactory.GetMaker(aSelectedCoin, FKeyManager);
            return lTransactionMaker.CreateSignedTransaction(aToAddress, aAmount, aTxFee, aExtParams);
        }

        internal void SendNewTransaction(string aRawTX, ICurrencyItem aCurrency, DelegateOnSendTransactionCompleted aTxSentEventDelegate)
        {
            var lTransactionMaker = FTransactionMakerFactory.GetMaker(aCurrency, FKeyManager);
            lTransactionMaker.SendRawTransaction(aRawTX, aTxSentEventDelegate);
        }

        internal decimal CalculateTxFee(string aToAddress, decimal aAmount, ICurrencyItem aCurrency, out decimal aFeePerKb)
        {
            decimal lResult;

            if (aCurrency.ChainParamaters.Capabilities.HasFlag(CapablityFlags.EthereumProtocol))
            {
                long lGasLimit;
                ICurrencyAmountFormatter lAmountFormatter;
                if (aCurrency.Id < 0)
                {
                    lGasLimit = 60000;
                    lAmountFormatter = FServerConnection.GetCurrency((aCurrency as ICurrencyToken).ParentCurrencyID);
                }
                else
                {
                    lGasLimit = 21000;
                    lAmountFormatter = aCurrency;
                }
                long lFeePerKb = FServerConnection.EstimateCurrencyTxFee(((ICurrencyIdentity) lAmountFormatter).Id);
                aFeePerKb = lAmountFormatter.AmountToDecimal(lFeePerKb);
                lResult = lAmountFormatter.AmountToDecimal(lFeePerKb * lGasLimit);
            }
            else
            {
                int lInputCount = FServerConnection.GetUnspentOutputs(aCurrency.Id).Count();
                var lUserAccounts = FServerConnection.GetMonitoredAccounts(aCurrency.Id);
                int lOutputCount = lUserAccounts.Any(lUserAccount => string.Equals(lUserAccount.Address, aToAddress)) ? 1 : 2;

                //Note: Estimation formulas taken from https://btc.network/estimate
                double lEstimatedTxByteSize;
                if (aCurrency.ChainParamaters.Capabilities.HasFlag(CapablityFlags.SegWitSupport))
                    lEstimatedTxByteSize = (lInputCount * 101.25) + (lOutputCount * 31) + 10;
                else
                    lEstimatedTxByteSize = (lInputCount * 146) + (lOutputCount * 33) + 10;

                long lFeePerKb = FServerConnection.EstimateCurrencyTxFee(aCurrency.Id);
                long lTxFee = Convert.ToInt64(lEstimatedTxByteSize * lFeePerKb / 1024);
                aFeePerKb = aCurrency.AmountToDecimal(lFeePerKb);
                lResult = aCurrency.AmountToDecimal(lTxFee);
            }

            return lResult;
        }

        internal decimal CalculateTxFee(string aToAddress, decimal aAmount, long aCurrencyId)
        {
            return CalculateTxFee(aToAddress, aAmount, FServerConnection.GetCurrency(aCurrencyId), out _);
        }

        public decimal GetBalance(long aCurrencyId)
        {
            var lOutputs = FServerConnection.GetUnspentOutputs(aCurrencyId);
            BigInteger lTotal = 0;
            foreach (var lOutput in lOutputs)
                lTotal += lOutput.Amount;
            return GetCurrency(aCurrencyId).AmountToDecimal(lTotal);
        }

        public CurrencyItem GetCurrency(long aCurrencyId)
        {
            return FServerConnection.GetCurrency(aCurrencyId);
        }

        private void AppMainForm_OnSendAllMenuClick(object sender, EventArgs e)
        {
            FromUserSendAmount(AppMainForm.SelectedCurrency.Balances.Total, true);
        }

        #endregion Send Coins Event Code

        /*************************************************************************************************************************\
         *                                                                                                                       *
         *          Sign Message code                                                                                           *
         *                                                                                                                       *
        \*************************************************************************************************************************/

        #region SignerMessage

        public void SignMessage(string pMsgToEncrypt)
        {
            try
            {
                //obtain msg from gui  -- class signermessage
                string Lmsg = "first message";
                //who send (private key)

                //really first try using privatekey, i dont really sure if use that, i will use mine.
                string privateKey = "0xb5b1870957d373ef0eeffecc6e4812c0fd08f554b37b233526acc331bf1544f7";

                if (Lmsg == null) { Lmsg = "Testing signature message if its null"; }
                else
                {
                    var signer1 = new EthereumMessageSigner();
                    var signature1 = signer1.EncodeUTF8AndSign(Lmsg, new EthECKey(privateKey));

                    //verify msg
                    var addressRec1 = signer1.EncodeUTF8AndEcRecover(Lmsg, signature1);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion SignerMessage

        /*************************************************************************************************************************\
         *                                                                                                                       *
         *          Reset wallet code                                                                                            *
         *                                                                                                                       *
        \*************************************************************************************************************************/

        #region Reset wallet

        private void ConnectDialog_findUsersClick(object sender, EventArgs e)
        {
            try
            {
                FindUsers();
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Critical, $"Fatal error when calling findusers method. Exception: {ex}");
                AppMainForm.StandardErrorMsgBox("Critical Error", $"{ex.Message}{Environment.NewLine}Unable to call method to findusers. If error persist, please contact support@davincicodes.net. The application will close.");
                AppMainForm.Close();
            }
        }

        public void DirectoryCopySelectedFiles(string lSourceDirName, string lDestDirName, FileInfo[] pFiles)
        {
            try
            {
                // Get the subdirectories for the specified directory.
                DirectoryInfo lDir = new DirectoryInfo(lSourceDirName);
                if (!lDir.Exists)
                {
                    throw new DirectoryNotFoundException(
                        "Source directory does not exist or could not be found: "
                        + lSourceDirName);
                }
                DirectoryInfo[] lDirs = lDir.GetDirectories();
                // If the destination directory doesn't exist, create it.
                Directory.CreateDirectory(lDestDirName);

                // Get the files in the directory and copy them to the new location.
                // FileInfo[] files = lDir.GetFiles();

                //travel pFiles
                foreach (FileInfo file in pFiles)
                {
                    string tempPath = Path.Combine(lDestDirName, file.Name);
                    file.CopyTo(tempPath, false);
                }
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Critical, $"Fatal error when creating backup by file. Exception: {ex}");
                AppMainForm.StandardErrorMsgBox("Critical Error", $"{ex.Message}{Environment.NewLine}Unable to backup wallet data. If error persist, please contact support@davincicodes.net. The application will close.");
                AppMainForm.Close();
            }
        }

        public void FindUsers()
        {
            try
            {
                //find potential users according to the files.
                string lFolderPath = Settings.DataPath;
                DirectoryInfo lDir = new DirectoryInfo(lFolderPath);
                //all that have the structure..., in the root path.
                FileInfo[] lFiles = lDir.GetFiles("*.sqlite=*", SearchOption.TopDirectoryOnly);

                var lListHash = new List<HashInfoUser>();

                foreach (var file in lFiles)
                {
                    //split filename into hash and then username-email.
                    string[] lNameFileDirectoryUsers = file.Name.Split(new String[] { ".sqlite=" }, StringSplitOptions.None);

                    HashInfoUser lUserHash = new HashInfoUser();

                    lUserHash.Hash = lNameFileDirectoryUsers[0];
                    lUserHash.UserNameEmail = lNameFileDirectoryUsers[1];

                    //lUserHash.Username = file.Name.Split(new String[] { ".txt" }, StringSplitOptions.None);
                    string[] lNameUser = lUserHash.UserNameEmail.Split(new String[] { ".txt" }, StringSplitOptions.None);

                    //set username without extension
                    lUserHash.Username = lNameUser[0];

                    //select user to delete after
                    lListHash.Add(lUserHash);
                }

                var lFindUserHash = new FindUsersToResetWalletDialog(lListHash);

                //if the list has something, add it to var
                if (lListHash.Count() != 0)
                {
                    if (lFindUserHash.Execute())
                    {
                        var lHashUserSelected = lFindUserHash.SelectedUserHash;

                        //get files with hash before all name
                        FileInfo[] lFilesUsersHashToCopy = lDir.GetFiles(@lHashUserSelected + "*", SearchOption.TopDirectoryOnly);

                        string lPath = Settings.DataPath;
                        string lDateTimeNow = DateTime.UtcNow.ToString("yyyyMMddHHmm");

                        //final path to decide where and how name copy directory files
                        string lFinalPath = string.Concat(lPath, "_bkp_", lDateTimeNow);

                        // last warning
                        string lMessage = "\nBefore pressing ok, you need to be completely sure and understand the risks of resetting your wallet. \n\nIf you do not want to continue, press cancel.  ";
                        string lTitle = "                   WARNING";
                        // bool msgbox = AppMainForm.StandardAskMsgBox(lTitle, lMessage);
                        bool msgbox = AppMainForm.StandardWarningMsgBoxAsk(lTitle, lMessage);

                        //bool msgbox = AppMainForm.StandardWarningBox(lTitle, lMessage);
                        //last msgbox for the user warning his operation
                        if (msgbox)
                        {
                            DirectoryCopySelectedFiles(lPath, lFinalPath, lFilesUsersHashToCopy);

                            //compare two directories
                            if (CompareDirs(lFinalPath, lHashUserSelected))
                            {
                                //if the same files in root path, and on new file backup, delete files on rootpath.
                                deleteFilesRootPath(lHashUserSelected);

                                Log.Write(LogLevel.Critical, $"Generated Backup Wallet.");

                                string lMsg = "Reset wallet process completed.  Now the app will close.";
                                string lTitle2 = " Process Completed ";
                                AppMainForm.StandardInfoMsgBox(lTitle2, lMsg);

                                Application.Exit();
                            }
                        }
                    }
                }
                else
                {
                    AppMainForm.StandardInfoMsgBox("Attention", "No user has been found to restart wallet. ");
                }
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Critical, $"Fatal error when Finding User. Exception: {ex}");
                AppMainForm.StandardErrorMsgBox("Critical Error", $"{ex.Message}{Environment.NewLine}Unable to find user. If error persist, please contact support@davincicodes.net. The application will close.");
                AppMainForm.Close();
            }
        }

        public bool CompareDirs(string pNewPath, string pHashUserSelected)
        {
            System.IO.DirectoryInfo dir1 = new System.IO.DirectoryInfo(Settings.DataPath);
            System.IO.DirectoryInfo dir2 = new System.IO.DirectoryInfo(pNewPath);

            // Take a snapshot of the file system.
            IEnumerable<System.IO.FileInfo> list1 = dir1.GetFiles(@pHashUserSelected + "*", SearchOption.TopDirectoryOnly);
            IEnumerable<System.IO.FileInfo> list2 = dir2.GetFiles("*.*", SearchOption.TopDirectoryOnly);

            //A custom file comparer defined below
            FileCompare myFileCompare = new FileCompare();

            // This query determines whether the two folders contain
            // identical file lists, based on the custom file comparer
            // that is defined in the FileCompare class.
            // The query executes immediately because it returns a bool.
            bool areIdentical = list1.SequenceEqual(list2, myFileCompare);

            if (areIdentical == true)
            {
                Console.WriteLine("the two folders are the same");
                return true;
            }
            else
            {
                Console.WriteLine("The two folders are not the same");
            }

            // Find the common files. It produces a sequence and doesn't
            // execute until the foreach statement.
            var queryCommonFiles = list1.Intersect(list2, myFileCompare);

            if (queryCommonFiles.Any())
            {
                Console.WriteLine("The following files are in both folders:");
                foreach (var v in queryCommonFiles)
                {
                    Console.WriteLine(v.FullName); //shows which items end up in result list
                }
            }
            else
            {
                Console.WriteLine("There are no common files in the two folders.");
            }

            // Find the set difference between the two folders.
            // For this example we only check one way.
            var queryList1Only = (from file in list1
                                  select file).Except(list2, myFileCompare);

            Console.WriteLine("The following files are in list1 but not list2:");
            foreach (var v in queryList1Only)
            {
                Console.WriteLine(v.FullName);
            }

            // Keep the console window open in debug mode.
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
            return false;
        }

        // This implementation defines a  comparison
        // between two FileInfo objects. It only compares the name
        // of the files being compared and their length in bytes.
        private class FileCompare : System.Collections.Generic.IEqualityComparer<System.IO.FileInfo>
        {
            public FileCompare()
            {
            }

            public bool Equals(System.IO.FileInfo f1, System.IO.FileInfo f2)
            {
                return (f1.Name == f2.Name &&
                        f1.Length == f2.Length);
            }

            // Return a hash that reflects the comparison criteria. According to the
            // rules for IEqualityComparer<T>, if Equals is true, then the hash codes must
            // also be equal. Because equality as defined here is a simple value equality, not
            // reference identity, it is possible that two or more objects will produce the same
            // hash code.
            public int GetHashCode(System.IO.FileInfo fi)
            {
                string s = $"{fi.Name}{fi.Length}";
                return s.GetHashCode();
            }
        }

        public void deleteFilesRootPath(string pUserHashFileToRemove)
        {
            try
            {
                string[] filePaths = Directory.GetFiles(Settings.DataPath, @pUserHashFileToRemove + "*");
                foreach (string filePath in filePaths)
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Critical, $"Fatal error when deletting rootpath. Exception: {ex}");
                AppMainForm.StandardErrorMsgBox("Critical Error", $"{ex.Message}{Environment.NewLine}Unable to delete files. If error persist, please contact support@davincicodes.net. The application will close.");
                AppMainForm.Close();
            }
        }

        #endregion Reset wallet

        private void MainForm_OnBackupClick(object sender, EventArgs e)
        {
            CallBackupWindows();
        }

        private void CallBackupWindows()
        {
            NewBackupProcessWizard lWizard = new NewBackupProcessWizard();
            lWizard.SetRecoveryPhraseAutoCompleteWords(WordBackupProcessor.EnglishWordList);
            BackupController lBackupController = new BackupController(lWizard);
            lBackupController.OnSaveFile += BackupController_OnSaveFile;
            lBackupController.OnRootSeedNeeded += BackupController_OnRootSeedNeeded;
            lBackupController.GetObjectToBackup += BackupController_GetObjectToBackup;
            lBackupController.ExecuteBackup(AppMainForm);
            lBackupController.Dispose();
        }

        private BasePandoraBackupObject BackupController_GetObjectToBackup()
        {
            BasePandoraBackupObject lObjectToBackup = null;
            try
            {
                FExchangeControl.BeginBackupRestore(out string lExchangeDBCopyFileName);
                FServerConnection.BeginBackupRestore(out string lDBCopyFileName);
                lObjectToBackup = new BasePandoraBackupObject
                {
                    ExchangeData = new BackupSerializableObject(File.ReadAllBytes(lExchangeDBCopyFileName)),
                    WalletData = new BackupSerializableObject(File.ReadAllBytes(lDBCopyFileName)),
                    BackupDate = DateTime.UtcNow,
                    OwnerData = new[] { Settings.UserName, Settings.Email },
                    PasswordHash = string.Empty //This is only used by restore by old file method, but could be a good idea to have code to check it
                };
                FServerConnection.EndBackupRestore(lDBCopyFileName);
                FExchangeControl.EndBackupRestore(lExchangeDBCopyFileName);
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Critical, $"Fatal error when getting backup by file. Exception: {ex}");
                AppMainForm.StandardErrorMsgBox("Critical Error", $"{ex.Message}{Environment.NewLine}Unable to copy wallet data. If error persist, please contact support@davincicodes.net. The application will close.");
                AppMainForm.Close();
            }
            return lObjectToBackup;
        }

        private string BackupController_OnRootSeedNeeded()
        {
            FromUserDecryptWallet(Settings.RequestWalletPassword);
            return FKeyManager.GetSecretRootSeed();
        }

        private void BackupController_OnSaveFile(byte[] aFile, string aPath)
        {
            CreateBackFileIfExists(aPath);
            using (var lStream = new FileStream(aPath, FileMode.CreateNew))
                lStream.Write(aFile, 0, aFile.Length);
        }

        private void MainForm_OnSettingsMenuClick(object sender, EventArgs e)
        {
            SettingsDialog SettingsDlg = new SettingsDialog();
            SettingsDlg.OnChangeDefaultCoinClick += SettingsDialog_OnChangeDefaultCoinClick;
            SettingsDlg.OnGetPrivateKey += SettingsDialog_OnGetPrivateKey;
            SettingsDlg.DataPath = Settings.DataPath;
            SettingsDlg.RequestWalletPassword = Settings.RequestWalletPassword;
            SettingsDlg.AutoUpdate = Settings.AutoUpdate;
            SettingsDlg.SetDefaultCurrency(FServerConnection.DefualtCurrencyItem.Id, FServerConnection.DefualtCurrencyItem.Name, SystemUtils.BytesToIcon(FServerConnection.DefualtCurrencyItem.Icon).ToBitmap());
            SettingsDlg.SetActiveCurrency(AppMainForm.SelectedCurrencyId, AppMainForm.SelectedCurrency.Name, SystemUtils.BytesToIcon(AppMainForm.SelectedCurrency.Icon).ToBitmap());
            if (SettingsDlg.Execute())
            {
                if (Settings.DataPath != SettingsDlg.DataPath)
                {
                    string lExchangeFileName = Path.GetFileName(FExchangeControl.SqLiteDbFileName);
                    string lServerConnectionSqLiteDbFileName = Path.GetFileName(FServerConnection.SqLiteDbFileName);
                    string lLogFileName = Path.GetFileName(Settings.LogFileName);
                    lExchangeFileName = Path.GetFileName(FExchangeControl.SqLiteDbFileName);
                    lExchangeFileName = Path.Combine(SettingsDlg.DataPath, lExchangeFileName);
                    lServerConnectionSqLiteDbFileName = Path.Combine(SettingsDlg.DataPath, lServerConnectionSqLiteDbFileName);
                    lLogFileName = Path.Combine(SettingsDlg.DataPath, lLogFileName);
                    // need to call restore here to set
                    Settings.DataPath = SettingsDlg.DataPath;
                    try
                    {
                        CopyDataFileToNewFolder(lServerConnectionSqLiteDbFileName, lExchangeFileName);
                        Settings.LogFileName = lLogFileName;
                        (Log.SystemLog as PandoraLog).Active = false;
                        CreateLog();
                        CoreSettings.SaveSettings(Settings, FSettingsFile);
                    }
                    catch (Exception ex)
                    {  // if this fails its not recoverable
                        Log.Write(LogLevel.Critical, "{0} Error moving data path to '{1}'", ex.Message, Settings.DataPath);
                        AppMainForm.StandardErrorMsgBox("Critical Error", $"{ex.Message}{Environment.NewLine}Unable to move data path please contact support@davincicodes.net. The application will close.");
                        AppMainForm.Close();
                        return;
                    }
                }
                Settings.RequestWalletPassword = SettingsDlg.RequestWalletPassword;
                Settings.AutoUpdate = SettingsDlg.AutoUpdate;
                if (!Settings.AutoUpdate)
                    Program.UpgradeFileName = null;
                else
                    Program.UpgradeFileName = FServerConnection.UpgradeFileName;
                FServerConnection.AutoUpdate = Settings.AutoUpdate;
                CoreSettings.SaveSettings(Settings, FSettingsFile);
                if (FServerConnection.DefualtCurrencyItem.Id != SettingsDlg.DefaultCurrencyId)
                {
                    // Do you have the currency? if not add it. and then select it.
                    if (!FServerConnection.GetDisplayedCurrencies().Any(lCurrency => lCurrency.Id == SettingsDlg.DefaultCurrencyId))
                    {
                        FromUserDecryptWallet(Settings.RequestWalletPassword);
                        AddNewCurrencyForDisplay(SettingsDlg.DefaultCurrencyId, FCancellationTokenSource.Token);
                        Event_DisplayCurrency(FServerConnection.GetCurrency(SettingsDlg.DefaultCurrencyId), null, FCancellationTokenSource.Token);
                    }
                    FServerConnection.SetDefaultCurrency(SettingsDlg.DefaultCurrencyId);
                }
            }
        }

        public void RemoveCurrency(long aCurrencyID)
        {
            var lDefaultCurrency = FServerConnection.GetDefaultCurrency().Id;
            if (aCurrencyID == lDefaultCurrency)
                throw new InvalidOperationException("You can not remove your default coin");
            if (aCurrencyID > 0)
                FServerConnection.SetDisplayedCurrency(aCurrencyID, false);
            else
                FServerConnection.SetDisplayedCurrencyToken(aCurrencyID, false);
            AppMainForm.RemoveCurrency(aCurrencyID, lDefaultCurrency);
        }

        private void CopyDataFileToNewFolder(string aServerConnectionFile, string aExchangeFile)
        {
            try
            {
                FExchangeControl.BeginBackupRestore(out string lExchangeDBCopyFileName);
                FServerConnection.BeginBackupRestore(out string lDBCopyFileName);

                Log.Write(LogLevel.Debug, "copying '{0}' for Exchange file", aServerConnectionFile);
                CreateBackFileIfExists(aExchangeFile);
                File.Copy(FExchangeControl.SqLiteDbFileName, aExchangeFile);
                Log.Write(LogLevel.Debug, "copying '{0}' for ServerConnection file", aServerConnectionFile);
                CreateBackFileIfExists(aServerConnectionFile);
                File.Copy(FServerConnection.SqLiteDbFileName, aServerConnectionFile);

                FServerConnection.SetDataPath(Settings.DataPath);
                FServerConnection.EndBackupRestore(lDBCopyFileName);
                FExchangeControl.EndBackupRestore(lExchangeDBCopyFileName);
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Critical, "{0} Error moving data path to '{1}'", ex.Message, Settings.DataPath);
                AppMainForm.StandardErrorMsgBox("Critical Error", $"{ex.Message}{Environment.NewLine}Unable to copy wallet data please contact support@davincicodes.net. The application will close.");
                AppMainForm.Close();
                return;
            }
        }

        private void CreateBackFileIfExists(string aFileName)
        {
            var lFileName = aFileName;
            var lIndex = 0;
            while (File.Exists(aFileName))
            {
                lFileName = aFileName + ".bak" + lIndex++;
                if (!File.Exists(lFileName))
                    File.Move(aFileName, lFileName);
            }
        }

        private IDictionary<string, string> SettingsDialog_OnGetPrivateKey(long aCurrencyID)
        {
            var lResult = new Dictionary<string, string>();
            if (FromUserDecryptWallet(true))
            {
                var lAdvocacy = FKeyManager.GetCurrencyAdvocacy(AppMainForm.SelectedCurrencyId, AppMainForm.SelectedCurrency.ChainParamaters);
                var lCurrencyAccounts = FServerConnection.GetMonitoredAccounts(aCurrencyID);
                foreach (var lAccount in lCurrencyAccounts)
                {
                    for (var lCounter = 0; lCounter < 100; lCounter++)
                        if (lAdvocacy.GetAddress(lCounter) == lAccount.Address)
                        {
                            lResult.Add(lAccount.Address, lAdvocacy.GetPrivateKey(lCounter));
                            break;
                        }
                }
            }
            else lResult = null;
            return lResult;
        }

        private void SettingsDialog_OnChangeDefaultCoinClick(object sender, EventArgs e)
        {
            var lSettingsDlg = (sender as SettingsDialog);
            var lCurrencyId = FromUserGetDefaultCurrencyId(lSettingsDlg.DefaultCurrencyId);
            if (lCurrencyId > 0)
            {
                var lCurrency = FServerConnection.GetCurrency(lCurrencyId);
                lSettingsDlg.SetDefaultCurrency(lCurrency.Id, lCurrency.Name, SystemUtils.BytesToIcon(lCurrency.Icon).ToBitmap());
            }
        }

        private void AppMainForm_OnChangePassword(object sender, EventArgs e)
        {
            if (FromUserDecryptWallet(true))
            {
                string lPassword = FromUserGetNewPassword(new PasswordValidationDelegate(ValidateNewPassword));
                if (lPassword != null)
                {
                    try
                    {
                        FKeyManager.UpdatePassword(lPassword);
                        PostUserInfo(FKeyManager.EncryptedRootSeed, KeyManager.HashPassword(lPassword), FKeyManager.GetSalt(), FServerConnection.DefualtCurrencyItem.Id);
                    }
                    catch (Exception ex)
                    {
                        AppMainForm.StandardExceptionMsgBox(ex);
                        AppMainForm.Close();
                    }
                }
            }
        }

        private void AppMainForm_OnAddCurrencyBtnClick(object sender, EventArgs e)
        {
            //
            //NOTE: The dialog is field property because it is used in
            //      ServerConnection_OnNewCurrency to update it's currency.
            //
            AddCoinSelectorDialog = new AddCoinSelector();
            var lCurrencies = FServerConnection.GetCurrencies();
            var lDisplayed = FServerConnection.GetDisplayedCurrencies();
            foreach (var lCurrency in lCurrencies)
            {
                // for the release version we will not show disabled coins.
#if !DEBUG
                if (lCurrency.CurrentStatus == CurrencyStatus.Disabled) continue;
#endif
                if (!lDisplayed.Any(lDisplayedCurrency => lDisplayedCurrency.Id == lCurrency.Id))
                    AddCoinSelectorDialog.AddCurrency(GUIModelProducer.CreateFrom(lCurrency));
            }
            Application.UseWaitCursor = true;
            Application.DoEvents();
            FTask?.Wait();
            Application.UseWaitCursor = false;
            if (AddCoinSelectorDialog.Execute() && FromUserDecryptWallet(Settings.RequestWalletPassword))
            {
                var lIds = AddCoinSelectorDialog.SelectedCurrencyIds;
                CancellationToken lToken = FCancellationTokenSource.Token;
                FTask = Task.Run(() =>
                {
                    foreach (var lCurrencyId in lIds)
                    {
                        try
                        {
                            AddNewCurrencyForDisplay(lCurrencyId, lToken);

                            AppMainForm.BeginInvoke(new DelegateDisplayCurrency(Event_DisplayCurrency), new object[] { FServerConnection.GetCurrency(lCurrencyId), null, lToken });
                        }
                        catch (Exception ex)
                        {
                            AppMainForm.BeginInvoke(new DelegateDisplayCurrency(Event_DisplayCurrency), new object[] { null, ex, lToken });
                        }
                    }
                }, lToken);
            }
            AddCoinSelectorDialog = null;
        }

        private void Event_DisplayCurrency(CurrencyItem aCurrnency, Exception ex, CancellationToken aCancelToken)
        {
            if (aCancelToken.IsCancellationRequested) return;
            if (ex != null)
                AppMainForm.StandardExceptionMsgBox(ex, "Error Adding Currency");
            else
                DisplayCurrency(aCurrnency);
        }

        private void Event_DisplayCurrencyToken(ClientCurrencyTokenItem aToken, Exception ex, CancellationToken aCancelToken)
        {
            if (aCancelToken.IsCancellationRequested) return;
            if (ex != null)
                AppMainForm.StandardExceptionMsgBox(ex, "Error Adding Currency Token");
            else
                DisplayToken(aToken);
        }

        private void AddNewCurrencyForDisplay(long lCurrencyId, CancellationToken aToken)
        {
            var lServer = FServerConnection;
            if (aToken.IsCancellationRequested) return;
            var lCurrency = lServer.GetCurrency(lCurrencyId);
            if (aToken.IsCancellationRequested) return;
            // Get any addresses stored on the server.
            var lServerCurrencyAccounts = lServer.DirectGetMonitoredAcccounts(lCurrencyId, 0);
            if (aToken.IsCancellationRequested) return;
            Log.Write(LogLevel.Debug, "Adding {0} Coin.", lCurrency.Name);
            Log.Write(LogLevel.Debug, "The Server has {0} adddresses", lServerCurrencyAccounts.Count);
            foreach (var lAccount in lServerCurrencyAccounts)
                Log.Write(LogLevel.Debug, "{0} - {1}", lAccount.Address, lAccount.Id);
            var lLocalCurrencyAccounts = lServer.GetMonitoredAccounts(lCurrencyId);
            if (aToken.IsCancellationRequested) return;
            Log.Write(LogLevel.Debug, "Adding {0} Coin.", lCurrency.Name);
            Log.Write(LogLevel.Debug, "The local DB has {0} adddresses", lServerCurrencyAccounts.Count);
            foreach (var lAccount in lLocalCurrencyAccounts)
                Log.Write(LogLevel.Debug, "{0} - {1}", lAccount.Address, lAccount.Id);
            // Generate the addressed and make sure everything matches.
            List<string> lAddresses = new List<string>();
            var lAdvacacy = FKeyManager.GetCurrencyAdvocacy(lCurrencyId, lCurrency.ChainParamaters);
            if (aToken.IsCancellationRequested) return;
            bool lServerValid = true;
            bool lLocalValid = true;
            Log.Write(LogLevel.Debug, "The Generated addresses are..");
            int lCount = lServerCurrencyAccounts.Count > 2 ? lServerCurrencyAccounts.Count : 2;
            for (int i = 0; i < lCount; i++)
            {
                if (aToken.IsCancellationRequested) return;
                lAddresses.Add(lAdvacacy.GetAddress(i));
                if (lServerCurrencyAccounts.Count > i) lServerValid = lServerCurrencyAccounts[i].Address == lAddresses[i] && lServerValid;
                if (lLocalCurrencyAccounts.Count > i) lLocalValid = lLocalCurrencyAccounts[i].Address == lAddresses[i] && lLocalValid;
                Log.Write(LogLevel.Debug, "{0} - {1}", lAddresses[i], i);
            }
            if (!lServerValid)
                throw new InvalidDataException("Server address do not match the locally generated addresses please email support@davincicodes.net.");
            if (!lLocalValid)
                throw new InvalidDataException("Local DB address do not match the locally generated addresses please email support@davincicodes.net.");
            // if the local counts don't match up wiht the server count then store them at the server.
            if (lServerCurrencyAccounts.Count != lLocalCurrencyAccounts.Count || lServerCurrencyAccounts.Count == 0)
                for (int i = 0; i < lCount; i++)
                {
                    if (aToken.IsCancellationRequested) return;
                    lServer.AddMonitoredAccount(lAddresses[i], lCurrency.Id);
                }
            // Save in the loacl Db the
            lServer.SetDisplayedCurrency(lCurrency.Id, true);
            //Make know exchange about the new currency
            FExchangeControl.LoadNewCurrencyData(lCurrency);
        }

        private bool Exchange_GetKeyManager(out KeyManager aKeyManagar)
        {
            aKeyManagar = FKeyManager;
            return true;
        }

        private void Display_NextCurrency(object sender, EventArgs e)
        {
            var lArgs = e as DisplayCurrenciesArgs;
            if (lArgs.Index < lArgs.ItemsToDisplay.Count())
                try
                {
                    var lItem = lArgs.ItemsToDisplay[lArgs.Index++];
                    var lCurrency = lItem as CurrencyItem;
                    if (lCurrency != null)
                    {
                        Log.Write(LogLevel.Debug, "Displaying currency {0} from startup call", lCurrency.Name);
                        DisplayCurrency(lCurrency);
                    }
                    else
                    {
                        var lToken = lItem as ClientCurrencyTokenItem;
                        if (lToken != null)
                        {
                            Log.Write(LogLevel.Debug, "Displaying token {0} from startup call", lToken.Name);
                            DisplayToken(lToken);
                        }
                    }
                    AppMainForm.BeginInvoke(new EventHandler(Display_NextCurrency), this, lArgs);
                }
                catch (Exception ex)
                {
                    Log.Write(LogLevel.Error, ex.Message);
                    var lMsgResult = MessageBox.Show($"{ex.Message}\r\nDo you wish to continue?", "Error", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Error);
                    if (lMsgResult == DialogResult.Cancel)
                        AppMainForm.Close();
                    else if (lMsgResult == DialogResult.Yes)
                        AppMainForm.BeginInvoke(new EventHandler(Display_NextCurrency), this, lArgs);
                }
            else
                AppMainForm.SelectedCurrencyId = lArgs.LastSelectedCurrencyId;
        }

        private void DisplayCurrency(CurrencyItem aCurrency)
        {
            var lServer = FServerConnection;
            if (FServerConnection == null) return;
            // read existing transactions.
            Log.Write(LogLevel.Debug, "Reading transactions, blockheight and accounts for {0}", aCurrency.Name);
            var lTransactionRecordList = lServer.GetTransactionRecords(aCurrency.Id);
            var lBlockHeight = lServer.GetBlockHeight(aCurrency.Id);
            var lAccounts = lServer.GetMonitoredAccounts(aCurrency.Id);
            var lCurrencyStatus = lServer.GetCurrencyStatus(aCurrency.Id);
            Log.Write(LogLevel.Debug, "Displaying currency {0}", aCurrency.Name);
            var lAddresses = new List<string>();
            var lAppMainFormAccounts = new List<GUIAccount>();
            int lIndex = 0;
            foreach (var lAccount in lAccounts)
            {
                lAddresses.Add(lAccount.Address);
                lAppMainFormAccounts.Add(GUIModelProducer.CreateFrom(lAccount.Address, $"{lIndex++}"));
            }
            var lAppMainFormCurrency = GUIModelProducer.CreateFrom(aCurrency);
            lAppMainFormCurrency.BlockHeight = lBlockHeight;
            lAppMainFormCurrency.Addresses = lAppMainFormAccounts.ToArray();
            foreach (TransactionRecord lTransactionRecord in lTransactionRecordList)
                lAppMainFormCurrency.Transactions.AddTransaction(GUIModelProducer.CreateFrom(lTransactionRecord, aCurrency, lAddresses));
            lAppMainFormCurrency.StatusDetails.StatusMessage = lCurrencyStatus.ExtendedInfo;
            lAppMainFormCurrency.StatusDetails.StatusTime = lCurrencyStatus.StatusTime;
            AppMainForm.AddCurrency(lAppMainFormCurrency);
        }

        private void DisplayToken(ClientCurrencyTokenItem aTokenItem)
        {
            var lServer = FServerConnection;
            if (FServerConnection == null) return;
            // read existing transactions.
            Log.Write(LogLevel.Debug, "Reading transactions, blockheight and accounts for {0}", aTokenItem.Name);
            var lParentTransactionRecordList = lServer.GetTransactionRecords(aTokenItem.ParentCurrencyID);
            var lTokenTransactionRecordList = lServer.GetTokenTransactionRecords(aTokenItem.Id, aTokenItem.ContractAddress);
            var lBlockHeight = lServer.GetBlockHeight(aTokenItem.ParentCurrencyID);

            var lCurrencyStatus = lServer.GetCurrencyStatus(aTokenItem.ParentCurrencyID);
            Log.Write(LogLevel.Debug, "Displaying currency {0}", aTokenItem.Name);
            var lAddresses = new List<string>();
            var lAppMainFormAccounts = new List<GUIAccount>();
            var lParentCurrency = AppMainForm.GetCurrency(aTokenItem.ParentCurrencyID);
            if (lParentCurrency == null)
            {
                if (!lServer.GetMonitoredAccounts(aTokenItem.ParentCurrencyID).Any())
                    AddNewCurrencyForDisplay(aTokenItem.ParentCurrencyID, FCancellationTokenSource.Token); //This will only execute the first time ever a token is added to the wallet
                var lServerParentCurrency = FServerConnection.GetCurrency(aTokenItem.ParentCurrencyID);
                DisplayCurrency(lServerParentCurrency);
                lParentCurrency = AppMainForm.GetCurrency(aTokenItem.ParentCurrencyID);
            }
            var lAppMainFormCurrency = GUIModelProducer.CreateFrom(aTokenItem, lParentCurrency);
            lAppMainFormCurrency.BlockHeight = lBlockHeight;
            lAppMainFormCurrency.Addresses = lParentCurrency.Addresses.ToArray();
            foreach (ClientTokenTransactionItem lTokenTxRecord in lTokenTransactionRecordList)
            {
                var lParentTx = lParentTransactionRecordList.SingleOrDefault((lTx) => string.Equals(lTx.TxId, lTokenTxRecord.ParentTransactionID, StringComparison.OrdinalIgnoreCase));
                if (lParentTx != null)
                    lAppMainFormCurrency.Transactions.AddTransaction(GUIModelProducer.CreateFrom(lTokenTxRecord, aTokenItem, lParentTx, lParentCurrency.Addresses.Select(lAccount => lAccount.Address)));
                else
                    Log.Write(LogLevel.Error, $"Missing parent transaction for token transaction with id {lTokenTxRecord.GetRecordID()}, Parent TXID: {lTokenTxRecord.ParentTransactionID}, Contract Address: {lTokenTxRecord.TokenAddress}");
            }
            lAppMainFormCurrency.StatusDetails.StatusMessage = lCurrencyStatus.ExtendedInfo;
            lAppMainFormCurrency.StatusDetails.StatusTime = lCurrencyStatus.StatusTime;
            AppMainForm.AddCurrency(lAppMainFormCurrency);
        }

        public void RestoreLocalCacheDB(string aRestoreFileName)
        {
            LoadCurrentUser();
            // ServerConnection.RestoreLocalCasheDB(aRestoreFileName, FServerConnection);
        }

        private bool ValidatePasswordHash(string aPassword)
        {
            return KeyManager.CheckPassword(aPassword, FServerConnection.ReadKeyValue("PasswordHash"));
        }

        private void UpdateLoginHistory(string aEmail, string aUserName, string aPassword, bool aSavePassword)
        {
            var lLoginAccounts = LoginHistoryToLoginAccounts(Settings.LoginHistory);
            LoginAccount lAccount = null;
            foreach (var lLoginAccount in lLoginAccounts)
                if (lLoginAccount.Email == aEmail && lLoginAccount.UserName == aUserName)
                    lAccount = lLoginAccount;
            if (lAccount == null)
            {
                lAccount = new LoginAccount() { Email = aEmail, UserName = aUserName, Password = aSavePassword ? aPassword : "" };
            }
            else
            {
                lLoginAccounts.Remove(lAccount);
                lAccount.Password = aSavePassword ? aPassword : "";
            }
            lLoginAccounts.Insert(0, lAccount);
            Settings.LoginHistory = LoginAccountsToLoginHistory(lLoginAccounts);
        }

        private List<LoginAccount> LoginHistoryToLoginAccounts(string aHistory)
        {
            var lResult = new List<LoginAccount>();
            if (aHistory != "")
            {
                try
                {
                    var lAccounts = aHistory.Split(':');
                    foreach (string lAccount in lAccounts)
                    {
                        var lArray = lAccount.Split(',');
                        lResult.Add(new LoginAccount() { Email = lArray[0], UserName = lArray[1], Password = Encryption.DecryptText(lArray[2], lArray[0]) });
                    }
                }
                catch (Exception e)
                {
                    Log.Write(LogLevel.Error, "Someone messed with the settings file LoginHistory...\n{0}", e);
                }
            }
            return lResult;
        }

        private string LoginAccountsToLoginHistory(List<LoginAccount> aLoginAccounts)
        {
            var lResult = new StringBuilder();
            foreach (var lLoginAccount in aLoginAccounts)
            {
                if (lResult.ToString() != "")
                    lResult.Append(":");
                var lPassword = string.IsNullOrEmpty(lLoginAccount.Password) ? string.Empty : Encryption.EncryptText(lLoginAccount.Password, lLoginAccount.Email.ToLower());
                lResult.Append($"{lLoginAccount.Email},{lLoginAccount.UserName},{lPassword}");
            }
            return lResult.ToString();
        }

        /*************************************************************************************************************************\
         *
         *
         *
         *          EXCHANGE CONTROLLER METHODS
         *
         *
         *
        \*************************************************************************************************************************/

        #region Exchange Controller Methods

        private PandoraEnchangeControl GetNewPandoraExchangeController()
        {
            if (FServerConnection == null) throw new Exception("A server connection must be set before continuing");
            var lExchangeControl = new PandoraEnchangeControl(FServerConnection);
            lExchangeControl.GetKeyManagerMethod = (out KeyManager lKeyManager) =>
            {
                lKeyManager = null;
                if (FromUserDecryptWallet(Settings.RequestWalletPassword))
                    lKeyManager = FKeyManager;
                return lKeyManager != null;
            };
            return lExchangeControl;
        }

        #endregion Exchange Controller Methods

        /*************************************************************************************************************************\
         *                                                                                                                       *
         *          RESTORE CONTROLLER METHODS                                                                                   *
         *                                                                                                                       *
        \*************************************************************************************************************************/

        #region Restore Controller Methods

        private bool ExecuteRestoreWizard()
        {
            NewBackupProcessWizard lWizard = new NewBackupProcessWizard(aIsRestore: true);
            lWizard.SetRecoveryPhraseAutoCompleteWords(WordBackupProcessor.EnglishWordList);
            RestoreController lRestoreControl = new RestoreController(lWizard)
            {
                PasswordDialogCallMethod = FromUserGetPassword
            };
            lRestoreControl.SetUserData(FServerConnection.Email, FServerConnection.Email);
            lRestoreControl.OnGetBytesFromFilePath += RestoreControl_OnGetBytesFromFilePath;
            lRestoreControl.OnRecoveryObjectRestored += RestoreControl_OnRecoveryBackupObjectCreated;
            lRestoreControl.OnWalletUncryptPasscodeRestored += RestoreControl_OnWalletUncryptedPasscodeRestored;
            lRestoreControl.OnFinishRestore += LRestoreControl_OnFinishRestore;
            bool lResult = lRestoreControl.ExecuteRestore(AppMainForm);
            lRestoreControl.Dispose();
            return lResult;
        }

        private void LRestoreControl_OnFinishRestore(object sender, EventArgs e)
        {
            var lRemoteMonitoredAddresses = FServerConnection.GetMonitoredAccounts(1);

            if (lRemoteMonitoredAddresses.Count == 1)
            {
                AppMainForm.StandardWarningMsgBox("Warning a new backup is needed", "With this version you will have 24 words, and the backup file change internally. Your previous passphrase and backup files will not work with this version");

                if (AppMainForm.StandardAskMsgBox("Please create a new backup", "Do you want create the backup right now?"))
                {
                    CallBackupWindows();
                }
            }
        }

        private byte[] RestoreControl_OnGetBytesFromFilePath(string aPath)
        {
            byte[] lResultBytes = null;
            if (File.Exists(aPath))
                try
                {
                    lResultBytes = File.ReadAllBytes(aPath);
                }
                catch (Exception Ex)
                {
                    throw new Exception($"{Ex.Message}{Environment.NewLine}Unable to read '{aPath}' file.");
                }
            return lResultBytes;
        }

        private void RestoreUsingWalletTempFiles(string aWalletTempFilePath, string aExchangeTempFilePath)
        {
            //TODO: Put code in here to switch temp files to current application folder and also tell the client to switch between them
            // the SqlLite Db for the wallet and the Exchange must be closed
            // Then copy the current the current files and reopen.
            FServerConnection?.ValidLocalDBFile(aWalletTempFilePath);
            if (!string.IsNullOrEmpty(aExchangeTempFilePath) && File.Exists(aExchangeTempFilePath))
                FExchangeControl?.ValidLocalDBFile(aExchangeTempFilePath);
            Log.Write(LogLevel.Debug, "Restoring '{0}' for ServerConnection file", aWalletTempFilePath);
            string lDBCopyFileName = string.Empty;
            string lExchangeDBCopyFileName = string.Empty;
            FServerConnection?.BeginBackupRestore(out lDBCopyFileName);
            FExchangeControl?.BeginBackupRestore(out lExchangeDBCopyFileName);
            try
            {
                Log.Write(LogLevel.Debug, "Restoring '{0}' for Exchange file", aWalletTempFilePath);

                CreateBackFileIfExists(FServerConnection.SqLiteDbFileName);
                File.Copy(aWalletTempFilePath, FServerConnection.SqLiteDbFileName);
                if (!string.IsNullOrEmpty(aExchangeTempFilePath) && File.Exists(aExchangeTempFilePath))
                {
                    CreateBackFileIfExists(FExchangeControl?.SqLiteDbFileName);
                    File.Copy(aExchangeTempFilePath, FExchangeControl.SqLiteDbFileName);
                }
            }
            finally
            {
                FServerConnection?.EndBackupRestore(lDBCopyFileName);
                FExchangeControl?.EndBackupRestore(lExchangeDBCopyFileName);
            }
        }

        private void RestoreControl_OnRecoveryBackupObjectCreated(PandorasBackupObject aBackupObj)
        {
            //TODO: read data to move to new format for the client and validate the restore file with the remote database.
            try
            {
                if (aBackupObj.IsOldBackupData)
                {
                    //This is only allowed to continue if the user does not created their 24 words or new backup file already
                    var lRemoteMonitoredAddresses = FServerConnection.DirectGetMonitoredAcccounts(1, 0);
                    if (lRemoteMonitoredAddresses.Count > 1)
                        throw new BackupExceptions.BadRecoveryFile("Please use a newer version of backup file");
                    //Add here code to recover from old backup file
                    //Even if this part takes a while, it will be normally done one time in the lifetime of the user
                    //When old backup file style is active, "PasswordHash" is the plain text password for the user
                    var lUserPassword = aBackupObj.PasswordHash;
                    var lOldRootSeed = aBackupObj.WalletSeeds.First();
                    var lUserDefaultCoin = aBackupObj.DefaultCoinID;
                    string lExchangeTempFilePath = Path.GetTempFileName();
                    var lExchangeRestoredData = aBackupObj.ExchangeData as BackupSerializableObject;
                    RestoreUsingRootSeed(lOldRootSeed, lUserPassword, lUserDefaultCoin);
                    var lEncryptedKey = FKeyManager.EncryptText(aBackupObj.ExchangeKeys["Bittrex"][0]);
                    var lEncryptedSecret = FKeyManager.EncryptText(aBackupObj.ExchangeKeys["Bittrex"][1]);
                    File.WriteAllBytes(lExchangeTempFilePath, lExchangeRestoredData.Data);
                    FExchangeControl.BeginBackupRestore(out string lExchangeDBCopyFileName);
                    try
                    {
                        Log.Write(LogLevel.Debug, "Restoring '{0}' for Exchange file", lExchangeTempFilePath);
                        CreateBackFileIfExists(FExchangeControl?.SqLiteDbFileName);
                        File.Copy(lExchangeTempFilePath, FExchangeControl.SqLiteDbFileName);
                    }
                    finally
                    {
                        FExchangeControl.EndBackupRestore(lExchangeDBCopyFileName);
                    }
                    FExchangeControl.SaveRestoredKeyAndSecret(lEncryptedKey, lEncryptedSecret);
                }
                else
                {
                    string lWalletTempFilePath = Path.GetTempFileName();
                    string lExchangeTempFilePath = Path.GetTempFileName();
                    var lWalletRestoredData = aBackupObj.WalletData as BackupSerializableObject;
                    var lExchangeRestoredData = aBackupObj.ExchangeData as BackupSerializableObject;
                    File.WriteAllBytes(lWalletTempFilePath, lWalletRestoredData.Data);
                    File.WriteAllBytes(lExchangeTempFilePath, lExchangeRestoredData.Data);
                    RestoreUsingWalletTempFiles(lWalletTempFilePath, lExchangeTempFilePath);
                }
            }
            catch (BackupExceptions.BadRecoveryFile ex)
            {
                Universal.Log.Write(LogLevel.Error, $"An exception occurred while restoring backup data from file. Exception: {ex}");
                throw new Exception($"Failed to restore. {ex.Message}");
            }
            catch (Exception ex)
            {
                Universal.Log.Write(LogLevel.Error, $"An exception occurred while restoring backup data from file. Exception: {ex}");
                throw new Exception("Failed to read file data.");
            }
        }

        private void RestoreControl_OnWalletUncryptedPasscodeRestored(string aPasscode)
        {
            //This is only allowed to continue if the user does not created their 24 words or new backup file already
            var lRemoteMonitoredAddresses = FServerConnection.DirectGetMonitoredAcccounts(1, 0);
            if (lRemoteMonitoredAddresses.Count > 1 && aPasscode.Length == 32)
                throw new Exception("Please use a 24 words backup phrase to continue");
            RestoreUsingRootSeed(aPasscode);
        }

        private void RestoreUsingRootSeed(string aPasscode, string aPassword = null, long aDefaultCurrencyID = 0)
        {
            Log.Write(LogLevel.Debug, "Passcode restore started.");
            var lReplacedWithNewSeed = aPasscode.Length == 32;
            if (lReplacedWithNewSeed)
            {
                Log.Write(LogLevel.Debug, "Old pascode used to restore wallet.");
                string lEmail = FServerConnection.Email;
                string lUserName = FServerConnection.UserName;
                DateTime lDateBeforeBadEmailUserCase = new DateTime(2019, 4, 8, 23, 59, 59);
                // date we replaced all users
                //accounts with the same case.
                UserStatus lUserStatus = FServerConnection.GetUserStatus();
                if (lUserStatus.StatusDate <= lDateBeforeBadEmailUserCase)
                {
                    string lStatusMessage = lUserStatus.ExtendedInfo;
                    Log.Write(LogLevel.Debug, "User existed before case sensitivity issue.");

                    if (lUserStatus.ExtendedInfo.Contains("Actual Case is:"))
                    {
                        Log.Write(LogLevel.Debug, "User has an account with diffrent case.");
                        int lStart = lStatusMessage.IndexOf(":", StringComparison.Ordinal) + 1;
                        string[] lSubstring = lStatusMessage.Substring(lStart, lStatusMessage.Length - lStart).Split(new string[] { "***" }, StringSplitOptions.RemoveEmptyEntries);
                        lEmail = lSubstring[0].Trim();
                        lUserName = lSubstring[1].Trim();
                    }
                }
                aPasscode = CurrencyControl.GenerateRootSeed(lEmail, lUserName, new Guid(aPasscode).ToByteArray());
                FServerConnection.GetUserStatus();
                aPasscode += BitConverter.ToString(KeyManager.GenerateRootSeed(16)).Replace("-", string.Empty).ToLower();
                this.FServerConnection.WriteKeyValue("ReplacedWithNewSeed", lReplacedWithNewSeed.ToString());  // write down the fact that a new seed was created.
            }
            //Verify passcode entered with the bitcoin addresses stored in
            string lPassword = (aPassword == null) ? FromUserGetNewPassword(new PasswordValidationDelegate(ValidateNewPassword)) : aPassword;
            if (lPassword == null) throw new Exception("You must provide a password.");
            Log.Write(LogLevel.Debug, "Generating key manager.");
            var lKey = new KeyManager(KeyManager.GenerateSalt());
            lKey.SetPassword(lPassword);
            lKey.SetRootSeed(KeyManager.HexStringToByteArray(aPasscode));
            Log.Write(LogLevel.Debug, "Loading bitcoin currency.");
            var lCurrency = this.FServerConnection.GetCurrency(1);
            if (lCurrency == null)
            {
                var s = "Unable to load currencies from server." + Environment.NewLine;
                if (FServerConnection.Errors.Any())
                    s = s + FServerConnection.Errors[FServerConnection.Errors.Length - 1];
                FServerConnection.ClearErrors();
                throw new Exception(s);
            }
            var lBitcoinAdvocacy = lKey.GetCurrencyAdvocacy(1, lCurrency.ChainParamaters);
            Log.Write(LogLevel.Info, "Address 0 = {0} and 1 = {1}", lBitcoinAdvocacy.GetAddress(0), lBitcoinAdvocacy.GetAddress(1));
            if (!this.FServerConnection.ValidateDefaultAddressExists(lBitcoinAdvocacy.GetAddress(0)))
                throw new Exception("The server has inconsistant information with the private key provided.");
            if (!lReplacedWithNewSeed && !this.FServerConnection.ValidateDefaultAddressExists(lBitcoinAdvocacy.GetAddress(1)))
                throw new Exception("The server has inconsistant information with the private keys provided.");
            else if (lReplacedWithNewSeed && FServerConnection.DirectGetMonitoredAcccounts(1, 0).Count == 2)
                throw new Exception("You have already recovered from your 12 passphrase you must use the new 24 word passphrase.");
            var lDefaultCurrencyId = aDefaultCurrencyID == 0 ? FromUserGetDefaultCurrencyId() : aDefaultCurrencyID;
            if (lDefaultCurrencyId < 1) throw new Exception("Default currency was not set.");
            Log.Write(LogLevel.Info, "Default currency Id selected is {0}", lDefaultCurrencyId);
#if DEBUG
            if (MessageBox.Show("You sure you want restore this account?", "confirmation", MessageBoxButtons.YesNo) != DialogResult.Yes) throw new Exception("test new user setup");
#endif
            FKeyManager = lKey;
            Log.Write(LogLevel.Info, "Saving the account info.");
            PostUserInfo(FKeyManager.EncryptedRootSeed, KeyManager.HashPassword(FKeyManager.GetPassword()), FKeyManager.GetSalt(), lDefaultCurrencyId);
        }

        #endregion Restore Controller Methods

        //------------------------------------------END OF RESTORE CONTROL CODE ------------------------------------------
    }
}