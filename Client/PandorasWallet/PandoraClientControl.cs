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
using Pandora.Client.Crypto.Currencies.Controls;
using Pandora.Client.PandorasWallet.Controlers;
using System.Text;
using Pandora.Client.PandorasWallet.SystemBackup;
using Pandora.Client.SystemBackup;
using Pandora.Client.Crypto.Currencies;

namespace Pandora.Client.PandorasWallet
{
    public delegate bool PasswordValidationDelegate(string aPassword);

    internal class PandoraClientControl : IDisposable
    {
        private delegate void DelegateSendTransaction(CurrencyItem lSelectedCoin, string aToAddress, decimal aAmount, decimal aTxFee);

        private delegate void DelegateDisplayCurrency(CurrencyItem aCurrency, Exception ex, CancellationToken aToken);

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
            //AppMainForm.OnExhangeCurrencySelectionChanged += MainForm_OnCurrencySelectionChanged;
            //AppMainForm.OnFormLoad += MainForm_OnFormLoad;
            //AppMainForm.OnTransactionViewSelectionChanged += MainForm_OnTransactionViewSelectionChanged;
            //AppMainForm.OnCurrencySearch += AppMainForm_OnSearchBoxTextChanged;
            AppMainForm.OnBackupClick += MainForm_OnBackupClick;
            AppMainForm.OnSettingsMenuClick += MainForm_OnSettingsMenuClick;
            AppMainForm.OnChangePassword += AppMainForm_OnChangePassword;
            AppMainForm.LabelCoinQuantity = "";
            AppMainForm.LabelTotalCoinReceived = "";
        }

        private Dictionary<long, string> FLastAddresses = new Dictionary<long, string>();
        private Dictionary<long, decimal> FLastSentValue = new Dictionary<long, decimal>();
        private long FPrevSelectedCurrencyID = -1;

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
            var lUnspent = FServerConnection.GetUnspentOutputs(AppMainForm.SelectedCurrencyId);
            if (!lUnspent.Any())
                aAmountToSend = 0;
            if (string.IsNullOrWhiteSpace(AppMainForm.ToSendAddress))
                AppMainForm.StandardErrorMsgBox("Send Error", $"Please provide a valid {AppMainForm.SelectedCurrency.Name} address!");
            else if (aAmountToSend <= 0)
                AppMainForm.StandardErrorMsgBox("Send Error", $"The amount '{aAmountToSend}' is an invalid amount for {AppMainForm.SelectedCurrency.Name}!");
            else
            {
                var lTxFee = CalculateTxFee(AppMainForm.ToSendAddress, aAmountToSend, AppMainForm.SelectedCurrency);
                if (aAmountToSend <= lTxFee)
                    AppMainForm.StandardErrorMsgBox("Send Transaction Error", $"The {AppMainForm.SelectedCurrency.Name} amount must be higher than the transaction fees.\r\nCurrent Transaction Fee : {lTxFee}");
                else
                    ExecuteSendTxDialog(aAmountToSend, FServerConnection.GetCurrency(AppMainForm.SelectedCurrency.Id), lTxFee, AppMainForm.SelectedCurrency.Balance, AppMainForm.ToSendAddress, lUnspent[0].Address, aSubtractFee);
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

            lConnectDialog.OnOkClick += new EventHandler(delegate (Object o, EventArgs a)
            {
                ConnectDialog lDlg = o as ConnectDialog;
                var lConnection = new ServerConnection(Settings.DataPath, AppMainForm);
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
            AppMainForm.Connected = FServerConnection != null && FServerConnection.Connected;
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
            }
            else
                Log.Write(LogLevel.Debug, "Not connected.. last User: {0} - {1} ", lConnectDialog.Username, lConnectDialog.Email);
            CoreSettings.SaveSettings(Settings, FSettingsFile);
            AppMainForm.Connected = (FServerConnection == null || !FServerConnection.Connected);
            if (AppMainForm.Connected)
                AppMainForm.Close();
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
                    lFormCurrency.ClearTransactions();
                    var lTxRecords = FServerConnection.GetTransactionRecords(aCurrencyItem.Id).Select(lTx => CreateFromTransaction(aCurrencyItem, lAddresses, lTx));
                    foreach (var lTx in lTxRecords)
                        lFormCurrency.AddTransaction(lTx);
                    lFormCurrency.UpdateBalance();
                    AppMainForm.BeginInvoke((Action<long>)AppMainForm.RefreshTransactions, aCurrencyItem.Id);
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
            if (FromUserDecryptWallet(false))
            {
                var lAdvocacy = FKeyManager.GetCurrencyAdvocacy(aCurrencyID, (ChainParams)aCurrencyChainParams);
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
                var lNewCurrencyToShow = aCurrencyItem.CopyTo(lDisplayedCurrency);
                var lAsyncHandle = AppMainForm.BeginInvoke((Func<AppMainForm.Currency, bool>)AppMainForm.UpdateCurrency, lNewCurrencyToShow);
                if (!(bool)AppMainForm.EndInvoke(lAsyncHandle))
                    Log.Write(LogLevel.Error, $"Unable to update currency info. CurrencyID: {aCurrencyItem.Id}.");
            }
        }

        private void ServerConnection_OnCurrencyStatusChange(object aSender, CurrencyStatusItem aCurrencyStatusItem)
        {
            var lDisplayedCurrency = AppMainForm.GetCurrency(aCurrencyStatusItem.CurrencyId);
            if (lDisplayedCurrency != null && lDisplayedCurrency.CurrentStatus != aCurrencyStatusItem.Status)
            {
                lDisplayedCurrency.CurrentStatus = aCurrencyStatusItem.Status;
                var lAsyncHandle = AppMainForm.BeginInvoke((Func<AppMainForm.Currency, bool>)AppMainForm.UpdateCurrency, lDisplayedCurrency);
                if (!((bool)AppMainForm.EndInvoke(lAsyncHandle)))
                    Log.Write(LogLevel.Error, $"Unable to update currency status. CurrencyID: {aCurrencyStatusItem.CurrencyId}. New status: {aCurrencyStatusItem.Status.ToString()}");
            }
        }

        private class DisplayCurrenciesArgs : EventArgs
        {
            public DisplayCurrenciesArgs() : base()
            {
            }

            public long LastSelectedCurrencyId;
            public int Index;
            public List<CurrencyItem> Currencies;
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
                if (!FromUserDecryptWallet(false)) throw new Exception("You must decrypt the wallet to continue.");
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
                if (!FromUserDecryptWallet(false)) throw new Exception("You must decrypt the wallet to continue.");
                var lAdvacacy = FKeyManager.GetCurrencyAdvocacy(lDefaultCurrency.Id, lDefaultCurrency.ChainParamaters);
                // Currency to be displayed.
                FServerConnection.AddMonitoredAccount(lAdvacacy.GetAddress(0), lDefaultCurrency.Id);
                FServerConnection.AddMonitoredAccount(lAdvacacy.GetAddress(1), lDefaultCurrency.Id);
                FServerConnection.SetDisplayedCurrency(lDefaultCurrency.Id, true);
            }

            // Now display the default currency first and
            // send a message to do the rest of the currencies.
            var lArgs = new DisplayCurrenciesArgs
            {
                Currencies = FServerConnection.GetDisplayedCurrencies(),
                LastSelectedCurrencyId = lDefaultCurrency.Id
            };
            // remove the defualt from the list.
            foreach (var lCurrency in lArgs.Currencies)
                if (lCurrency.Id == lDefaultCurrency.Id)
                {
                    lArgs.Currencies.Remove(lCurrency);
                    break;
                }
            FServerConnection.SetDefaultCurrency(lDefaultCurrency.Id);

            DisplayCurrency(lDefaultCurrency);
            string lValue = FServerConnection.ReadKeyValue("LastSelectedCurrencyId");
            long.TryParse(lValue, out long lCurrencyId);
            AppMainForm.SetUserStatus(AppMainForm.UserStatuses.Connected, Settings.Email, Settings.UserName);
            AppMainForm.SelectedCurrencyId = lDefaultCurrency.Id;
            AppMainForm.BeginInvoke(new EventHandler(Display_NextCurrency), this, lArgs);
            FExchangeControl.StartProcess();
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
            AddCoinSelectorDialog?.AddCurrency(aCurrencyItem.Id, aCurrencyItem.Name, aCurrencyItem.Ticker, Globals.BytesToIcon(aCurrencyItem.Icon), aCurrencyItem.CurrentStatus.ToString());
        }

        private void ServerConnection_OnBlockHeightChange(object aSender, long aCurrencyId, long aBlockHeight)
        {
            var lFormCurrency = AppMainForm.GetCurrency(aCurrencyId);
            if (lFormCurrency != null)
            {
                var lUnconfirmedBalance = lFormCurrency.UnconfirmedBalance;
                lFormCurrency.BlockHeight = aBlockHeight;
                if (AppMainForm.SelectedCurrency.Id == lFormCurrency.Id)
                {
                    foreach (var lTrans in AppMainForm.SelectedCurrency.Transactions)
                        if (!lTrans.Confirmed)
                            AppMainForm.UpdateTransaction(lTrans);
                }
                lFormCurrency.UpdateBalance();
                if (lUnconfirmedBalance != lFormCurrency.UnconfirmedBalance)
                    AppMainForm.UpdateCurrency(lFormCurrency);
            }
        }

        private void ServerConnection_OnUpdatedTransaction(object aSender, TransactionRecord aTransactionRecord)
        {
            if (AppMainForm.SelectedCurrency.Id == aTransactionRecord.CurrencyId)
            {
                var lFormTransaction = AppMainForm.SelectedCurrency.FindTransaction(aTransactionRecord.TransactionRecordId);
                if (lFormTransaction != null)
                {
                    if (!aTransactionRecord.Valid)
                        AppMainForm.RemoveTransaction(lFormTransaction);  // note this is wrong.
                    else
                    {
                        var lItem = CreateFromTransaction(
                        FServerConnection.GetCurrency(aTransactionRecord.CurrencyId),
                        FServerConnection.GetMonitoredAddresses(aTransactionRecord.CurrencyId),
                        aTransactionRecord);
                        lItem.ParrentCurrency = lFormTransaction.ParrentCurrency;
                        lFormTransaction.CopyFrom(lItem);
                        AppMainForm.UpdateTransaction(lFormTransaction);
                    }
                }
            }
        }

        private void ServerConnection_OnNewTransaction(object aSender, TransactionRecord aTransactionRecord)
        {
            if (!aTransactionRecord.Valid) return;
            var lAddresses = FServerConnection.GetMonitoredAddresses(aTransactionRecord.CurrencyId);
            var lCurrency = FServerConnection.GetCurrency(aTransactionRecord.CurrencyId);
            var lFormTransaction = CreateFromTransaction(lCurrency,
                                                         lAddresses,
                                                         aTransactionRecord);
            if (AppMainForm.SelectedCurrency.Id == aTransactionRecord.CurrencyId)
            {
                if (AppMainForm.SelectedCurrency.FindTransaction(aTransactionRecord.TransactionRecordId) == null)
                {
                    AppMainForm.SelectedCurrency.AddTransaction(lFormTransaction);
                    AppMainForm.AddTransaction(lFormTransaction);
                    AppMainForm.UpdateCurrency(AppMainForm.SelectedCurrency);
                }
                else
                    Log.Write(LogLevel.Error, "transaction {0} - {1} found as new but already exits.", aTransactionRecord.TxId, aTransactionRecord.TransactionRecordId);
            }
            else
            {
                var lFormCurrency = AppMainForm.GetCurrency(aTransactionRecord.CurrencyId);
                if (lFormCurrency != null)
                {
                    lFormCurrency.AddTransaction(lFormTransaction);
                    AppMainForm.UpdateCurrency(lFormCurrency);
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

        private string ExecuteSendTxDialog(decimal aAmount, CurrencyItem lSelectedCoin, decimal aTxFee, decimal aBalance, string aAddress, string aFromAddress, bool aSubtractFee)
        {
            string lTicker = lSelectedCoin.Ticker;
            var lSendTransactionDlg = new SendTransactionDialog();
            lSendTransactionDlg.ParentWindow = AppMainForm;

            lSendTransactionDlg.SubstractFee = aSubtractFee;
            lSendTransactionDlg.Ticker = lTicker;
            lSendTransactionDlg.Amount = aAmount;
            lSendTransactionDlg.ToAddress = aAddress;
            lSendTransactionDlg.Balance = aBalance;
            lSendTransactionDlg.TxFee = aTxFee;
            lSendTransactionDlg.FromAddress = aFromAddress;
            lSendTransactionDlg.TxFeeRate = lSelectedCoin.AmountToDecimal(lSelectedCoin.FeePerKb);

            string lRetunedTxID = null;

            if (lSendTransactionDlg.Execute() && FromUserDecryptWallet(Settings.RequestWalletPassword))
            {
                FSendingTxDialog = new SendingTxDialog();
                FSendingTxDialog.ParentWindow = AppMainForm;
                Task.Run(() =>
                {
                    Event_SendTransaction(lSelectedCoin, aAddress, lSendTransactionDlg.Amount, lSendTransactionDlg.TxFee);
                });
                FSendingTxDialog.Execute();
            }
            return lRetunedTxID;
        }

        private void Event_SendTransaction(CurrencyItem lSelectedCoin, string aToAddress, decimal aAmount, decimal aTxFee)
        {
            try
            {
                string lTx = CreateSignedTransaction(aToAddress, aAmount, aTxFee, lSelectedCoin);
                FServerConnection.DirectSendNewTransaction(lTx, lSelectedCoin.Id, new DelegateOnSendTransactionCompleted(ServerConnection_SendTransactionCompleted));
            }
            catch (Exception ex)
            {
                AppMainForm.BeginInvoke(new DelegateOnSendTransactionCompleted(ServerConnection_SendTransactionCompleted), new object[] { this, ex.Message, null });
            }
        }

        private SendingTxDialog FSendingTxDialog;
        private Task FTask;

        internal string PrepareNewTransaction(string aToAddress, decimal aAmount, decimal aTxFee, CurrencyItem aCurrencyItem, TransactionUnit[] aUnspentOutputs, out CurrencyTransaction aCurrencyTransaction)
        {
            if (!FServerConnection.DirectCheckAddress(aCurrencyItem.Id, aToAddress))
                throw new ClientExceptions.InvalidAddressException("Address provided not valid. Please verify");
            var lTxOutputs = new List<TransactionUnit>();
            lTxOutputs.Add(new TransactionUnit(0, aCurrencyItem.AmountToLong(aAmount), aToAddress));
            long lTotal = 0;
            foreach (var lOutput in aUnspentOutputs)
                lTotal += lOutput.Amount;
            var lSendTotal = aCurrencyItem.AmountToDecimal(lTotal);
            if (lSendTotal < (aAmount + aTxFee))
                throw new InvalidOperationException($"The amount to send '{aAmount + aTxFee}' is greater than the balance of transactions '{aCurrencyItem.AmountToDecimal(lTotal)}'.");
            else if (lSendTotal > (aAmount + aTxFee))
                lTxOutputs.Add(new TransactionUnit(0, lTotal - aCurrencyItem.AmountToLong(aAmount + aTxFee), FServerConnection.GetCoinAddress(aCurrencyItem.Id)));
            aCurrencyTransaction = new CurrencyTransaction(aUnspentOutputs, lTxOutputs.ToArray(), aCurrencyItem.AmountToLong(aTxFee), aCurrencyItem.Id);
            return FServerConnection.DirectCreateTransaction(aCurrencyTransaction);
        }

        internal string SignTransactionData(string aTxData, CurrencyItem aCurrencyItem, CurrencyTransaction aCurrencyTransaction)
        {
            string lSignedTx = null;
            if (FromUserDecryptWallet(false))
            {
                var lCurrencyAdvocacy = FKeyManager.GetCurrencyAdvocacy(aCurrencyItem.Id, aCurrencyItem.ChainParamaters);
                //TODO: this is done because the object needs the address create.
                //So we should fix this somehow so its a bit more logical.
                lCurrencyAdvocacy.GetAddress(0);
                lCurrencyAdvocacy.GetAddress(1);
                lSignedTx = lCurrencyAdvocacy.SignTransaction(aTxData, aCurrencyTransaction);
            }
            return lSignedTx;
        }

        /// <summary>
        /// Takes all available balance from existing transactions and sends them to the address spesified minus the fee ammount provoided.
        /// </summary>
        /// <param name="aToAddress"></param>
        /// <param name="aAmount"></param>
        /// <param name="aTxFee"></param>
        /// <param name="aCurrencyId"></param>
        /// <returns></returns>
        internal string CreateSignedTransaction(string aToAddress, decimal aAmount, decimal aTxFee, long aCurrencyId)
        {
            return CreateSignedTransaction(aToAddress, aAmount, aTxFee, FServerConnection.GetCurrency(aCurrencyId));
        }

        internal string CreateSignedTransaction(string aToAddress, decimal aAmount, decimal aTxFee, CurrencyItem aCurrency)
        {
            var lUnspents = FServerConnection.GetUnspentOutputs(aCurrency.Id);
            string lData;
            if (aCurrency.ChainParamaters.Capabilities.HasFlag(Crypto.Currencies.CapablityFlags.SupportSegWit))
            {
                Dictionary<string, List<TransactionUnit>> FAddressTypes = new Dictionary<string, List<TransactionUnit>>();
                foreach (var lTxUnit in lUnspents)
                {
                    if (!FAddressTypes.ContainsKey(lTxUnit.Address))
                        FAddressTypes.Add(lTxUnit.Address, new List<TransactionUnit>());
                    FAddressTypes[lTxUnit.Address].Add(lTxUnit);
                }
                //if (FAddressTypes.Count > 1)
                //{
                //    throw new Exception("Send transaction from 2 diffrent address types not supported.");
                //    var lAccounts = FServerConnection.GetMonitoredAccounts(aCurrency.Id);
                //    StringBuilder lResultData = new StringBuilder();
                //    lResultData.AppendLine("Version=1");
                //    lResultData.AppendLine($"Count={FAddressTypes.Count}");
                //    int lIndex = 0;
                //    foreach (var lUnspentList in FAddressTypes.Values)
                //        lResultData.AppendLine($"" + PrepareNewTransaction(aToAddress, aAmount, aTxFee, aCurrency, lUnspents, out CurrencyTransaction lCurrencyTransaction))
                //}
            }
            lData = PrepareNewTransaction(aToAddress, aAmount, aTxFee, aCurrency, lUnspents, out CurrencyTransaction lCurrencyTransaction);
            return SignTransactionData(lData, aCurrency, lCurrencyTransaction);
        }

        internal decimal CalculateTxFee(string aToAddress, decimal aAmount, CurrencyItem aCurrency)
        {
            var lData = PrepareNewTransaction(aToAddress, aAmount, 0, aCurrency, FServerConnection.GetUnspentOutputs(aCurrency.Id), out CurrencyTransaction lCurrencyTransaction);

            //TODO: for segwit this is fine but I want to fix this
            // of not segwit address and include the signature in the total size of the tx
            // thus we need to sign the tx with a bogus key so we don't ask the user for a password here.
            int lFeePerKb = aCurrency.FeePerKb;
            decimal lKBSize = ((decimal)lData.Length / 2) / 1024;
            long lTxFee = Convert.ToInt64(lKBSize * lFeePerKb);
            return aCurrency.AmountToDecimal(lTxFee);
        }

        internal decimal CalculateTxFee(string aToAddress, decimal aAmount, long aCurrencyId)
        {
            return CalculateTxFee(aToAddress, aAmount, FServerConnection.GetCurrency(aCurrencyId));
        }

        public decimal GetBalance(long aCurrencyId)
        {
            var lOutputs = FServerConnection.GetUnspentOutputs(aCurrencyId);
            long lTotal = 0;
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
            FromUserSendAmount(AppMainForm.SelectedCurrency.Balance, true);
        }

        #endregion Send Coins Event Code

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
                FExchangeControl.BeginBackupRestore();
                FServerConnection.BeginBackupRestore();
                lObjectToBackup = new BasePandoraBackupObject
                {
                    ExchangeData = new BackupSerializableObject(File.ReadAllBytes(FExchangeControl.SqLiteDbFileName)),
                    WalletData = new BackupSerializableObject(File.ReadAllBytes(FServerConnection.SqLiteDbFileName)),
                    BackupDate = DateTime.UtcNow,
                    OwnerData = new[] { Settings.UserName, Settings.Email },
                    PasswordHash = string.Empty //This is only used by restore by old file method, but could be a good idea to have code to check it
                };
                FServerConnection.EndBackupRestore();
                FExchangeControl.EndBackupRestore();
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
                Settings.RequestWalletPassword = Settings.RequestWalletPassword;
                CoreSettings.SaveSettings(Settings, FSettingsFile);
                if (FServerConnection.DefualtCurrencyItem.Id != SettingsDlg.DefaultCurrencyId)
                {
                    // Do you have the currency? if not add it. and then select it.
                    if (!FServerConnection.GetDisplayedCurrencies().Any(lCurrency => lCurrency.Id == SettingsDlg.DefaultCurrencyId))
                    {
                        FromUserDecryptWallet(false);
                        AddNewCurrencyForDisplay(SettingsDlg.DefaultCurrencyId, FCancellationTokenSource.Token);
                        Event_DisplayCurrency(FServerConnection.GetCurrency(SettingsDlg.DefaultCurrencyId), null, FCancellationTokenSource.Token);
                    }
                    FServerConnection.SetDefaultCurrency(SettingsDlg.DefaultCurrencyId);
                }
            }
        }

        private void CopyDataFileToNewFolder(string aServerConnectionFile, string aExchangeFile)
        {
            try
            {
                FExchangeControl.BeginBackupRestore();
                FServerConnection.BeginBackupRestore();

                Log.Write(LogLevel.Debug, "copying '{0}' for Exchange file", aServerConnectionFile);
                CreateBackFileIfExists(aExchangeFile);
                File.Copy(FExchangeControl.SqLiteDbFileName, aExchangeFile);
                Log.Write(LogLevel.Debug, "copying '{0}' for ServerConnection file", aServerConnectionFile);
                CreateBackFileIfExists(aServerConnectionFile);
                File.Copy(FServerConnection.SqLiteDbFileName, aServerConnectionFile);

                FServerConnection.SetDataPath(Settings.DataPath);
                FServerConnection.EndBackupRestore();
                FExchangeControl.EndBackupRestore();
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
            if (FromUserDecryptWallet(Settings.RequestWalletPassword))
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
                    AddCoinSelectorDialog.AddCurrency(lCurrency.Id, lCurrency.Name, lCurrency.Ticker, Globals.BytesToIcon(lCurrency.Icon), lCurrency.CurrentStatus.ToString());
            }
            Application.UseWaitCursor = true;
            Application.DoEvents();
            FTask?.Wait();
            Application.UseWaitCursor = false;
            if (AddCoinSelectorDialog.Execute() && FromUserDecryptWallet(false))
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

        private void Event_DisplayCurrency(CurrencyItem aCurrnency, Exception ex, CancellationToken aToken)
        {
            if (aToken.IsCancellationRequested) return;
            if (ex != null)
                AppMainForm.StandardExceptionMsgBox(ex, "Error Adding Currency");
            else
                DisplayCurrency(aCurrnency);
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
            if (lArgs.Index < lArgs.Currencies.Count)
                try
                {
                    var lCurrency = lArgs.Currencies[lArgs.Index++];
                    Log.Write(LogLevel.Debug, "Displaying currency {0} from startup call", lCurrency.Name);
                    DisplayCurrency(lCurrency);
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
                foreach (var lCurrency in lArgs.Currencies)
                    if (lArgs.LastSelectedCurrencyId == lCurrency.Id)
                        AppMainForm.SelectedCurrencyId = lCurrency.Id;
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
            Log.Write(LogLevel.Debug, "Displaying currency {0}", aCurrency.Name);
            var lAddresses = new List<string>();
            var lAppMainFormAccounts = new List<AppMainForm.Accounts>();
            int lIndex = 0;
            foreach (var lAccount in lAccounts)
            {
                lAddresses.Add(lAccount.Address);
                lAppMainFormAccounts.Add(new AppMainForm.Accounts() { Address = lAccount.Address, Name = $"{lIndex++}" });
            }
            var lAppMainFormCurrency = new AppMainForm.Currency(aCurrency);
            lAppMainFormCurrency.BlockHeight = lBlockHeight;
            lAppMainFormCurrency.Addresses = lAppMainFormAccounts.ToArray();
            foreach (TransactionRecord lTransactionRecord in lTransactionRecordList)
                lAppMainFormCurrency.AddTransaction(CreateFromTransaction(aCurrency, lAddresses, lTransactionRecord));
            AppMainForm.AddCurrency(lAppMainFormCurrency);
        }

        private static AppMainForm.Transaction CreateFromTransaction(CurrencyItem aCurrency, List<string> aAddresses, TransactionRecord aTransactionRecord)
        {
            return new AppMainForm.Transaction
            {
                RecordId = aTransactionRecord.TransactionRecordId,
                TxDate = aTransactionRecord.TxDate,
                TxId = aTransactionRecord.TxId,
                BlockNumber = aTransactionRecord.Block,
                Amount = aCurrency.AmountToDecimal(aTransactionRecord.GetValue(aAddresses.ToArray(), out int lTxType, out string lToAddress, out string lFromAddress)),
                Fee = aCurrency.AmountToDecimal(aTransactionRecord.TxFee),
                TxType = (AppMainForm.TransactionType)lTxType,
                From = lFromAddress,
                ToAddress = lToAddress,
            };
        }

        public void RestoreLocalCacheDB(string aRestoreFileName)
        {
            ServerConnection.RestoreLocalCasheDB(aRestoreFileName, FServerConnection);
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
                if (FromUserDecryptWallet(false))
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

            if (lRemoteMonitoredAddresses.Count < 2)
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
            FServerConnection?.BeginBackupRestore();
            FExchangeControl?.BeginBackupRestore();
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
                FServerConnection?.EndBackupRestore();
                FExchangeControl?.EndBackupRestore();
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
                    FExchangeControl.BeginBackupRestore();
                    try
                    {
                        Log.Write(LogLevel.Debug, "Restoring '{0}' for Exchange file", lExchangeTempFilePath);
                        CreateBackFileIfExists(FExchangeControl?.SqLiteDbFileName);
                        File.Copy(lExchangeTempFilePath, FExchangeControl.SqLiteDbFileName);
                    }
                    finally
                    {
                        FExchangeControl.EndBackupRestore();
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