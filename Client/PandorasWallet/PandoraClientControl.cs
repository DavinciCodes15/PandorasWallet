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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet
{
    public partial class PandoraClientControl
    {
        private static PandoraClientControl FPandoraClientControl;
        private PandoraWallet FWallet;
        private PandoraWallet FWorkingWallet;

        private bool FStartUpConnected;
        private CancellationTokenSource FEncryptionCancellationtoken;
        private Dictionary<long, string> FSendAddressCache = new Dictionary<long, string>();
        private long FPreviousSelectedCurrency;

        private ConnectDialog ConnectDialog { get; set; }
        private RestoreWalletDialog RestoreDialog { get; set; }
        private BaseWizzard RestoreWizard { get; set; }
        private BaseWizzard BackupWizard { get; set; }
        private DefaultCoinSelector DefaultCoinSelectorDialog { get; set; }
        private AddCoinSelector AddCoinSelectorDialog { get; set; }
        private WalletPasswordDialog EncryptionPasswordDialog { get; set; }
        private SendTransactionDialog TransactionDetailDialog { get; set; }

        public SettingsDialog SettingsDialog { get; set; }
        public SendingTxDialog TrySendTxDialog { get; set; }

        public PandoraClientMainWindow MainForm { get; private set; }

        public InitializingDialog InitializingDialog { get; set; }

        private PandoraClientControl(PandoraClientMainWindow aMainWindow)
        {
            MainForm = aMainWindow;
            MainForm.OnConnect += MainForm_OnConnect;
            MainForm.OnAddCurrencyBtnClick += MainForm_OnAddCurrencyBtnClick;
            MainForm.OnExhangeCurrencySelectionChanged += MainForm_OnCurrencySelectionChanged;
            MainForm.OnSendAllMenuClick += MainForm_OnSendAllMenuClick;
            MainForm.OnFormLoad += MainForm_OnFormLoad;
            MainForm.OnTransactionSend += MainForm_OnTransactionSend;
            MainForm.OnTransactionViewSelectionChanged += MainForm_OnTransactionViewSelectionChanged;
            MainForm.OnSearchBoxTextChanged += MainForm_OnSearchBoxTextChanged;
            MainForm.OnBackupClick += MainForm_OnBackupClick;
            MainForm.OnSettingsMenuClick += MainForm_OnSettingsMenuClick;
            MainForm.OnChangePassword += MainForm_OnChangePassword;

            MainForm.LabelCoinQuantity = "";
            MainForm.LabelTotalCoinReceived = "";
            MainForm.ExchangeTargetPriceEnabled = false;
            MainForm.ExchangeQuantityEnabled = false;
            MainForm.ExchangeTotalReceivedEnabled = false;
            MainForm.ExchangeTransactionNameEnabled = false;

            ConnectDialog = new ConnectDialog();
            ConnectDialog.OnOkClick += ConnectDialog_OnOkClick;

            DefaultCoinSelectorDialog = new DefaultCoinSelector();

            AddCoinSelectorDialog = new AddCoinSelector();

            EncryptionPasswordDialog = new WalletPasswordDialog();
            EncryptionPasswordDialog.OnOkButtonClick += EncryptionPasswordDialog_OnOkButtonClick;
            EncryptionPasswordDialog.OnCancelButtonClick += EncryptionPasswordDialog_OnCancelButtonClick;

            RestoreDialog = new RestoreWalletDialog();
            RestoreDialog.OnRestoreBtnClick += RestoreDialog_OnRestoreBtnClick;

            TransactionDetailDialog = new SendTransactionDialog();

            TrySendTxDialog = new SendingTxDialog();

            SettingsDialog = new SettingsDialog();
            SettingsDialog.OnChangeDefaultCoinClick += SettingsDialog_OnChangeDefaultCoinClick;

            InitializingDialog = new InitializingDialog();

            RestoreInitialize();
            BackupInitialize();
            ExchangeInitialize();
        }

        private void MainForm_OnChangePassword(object sender, EventArgs e)
        {
            if (!EncryptionPasswordDialog.Execute())
            {
                return;
            }
            EncryptionPasswordDialog.OnOkButtonClick -= EncryptionPasswordDialog_OnOkButtonClick;
            EncryptionPasswordDialog.OnOkButtonClick += ChangeWalletPassword;

            if (EncryptionPasswordDialog.Execute(true))
            {
                MainForm.StandardInfoMsgBox("Wallet Password successfully changed");
            }

            EncryptionPasswordDialog.OnOkButtonClick -= ChangeWalletPassword;
            EncryptionPasswordDialog.OnOkButtonClick += EncryptionPasswordDialog_OnOkButtonClick;

            MainForm.SetArrowCursor();
        }

        private void ChangeWalletPassword(object sender, EventArgs e)
        {
            string lResult;
            try
            {
                FWallet.ChangeWalletPassword(EncryptionPasswordDialog.Password);
            }
            catch (Exception ex)
            {
                lResult = ex.Message;
            }
            finally
            {
                EncryptionPasswordDialog.Invoke(new MethodInvoker(() => EncryptionPasswordDialog.SetResult()));
            }
        }

        private void SettingsDialog_OnChangeDefaultCoinClick(object sender, EventArgs e)
        {
            if (ExecuteDefaultCoinSelector(FWallet, true))
            {
                CurrencyItem lCurrencySelected = FWallet.UserCoins.Where(x => x.Id == DefaultCoinSelectorDialog.SelectedCurrencyID).First();

                SettingsDialog.DefaultCoin = lCurrencySelected.Name;
                SettingsDialog.DefaultcoinImage = Universal.SystemUtils.BytesToIcon(lCurrencySelected.Icon).ToBitmap();
            }
        }

        private void MainForm_OnSettingsMenuClick(object sender, EventArgs e)
        {
            CurrencyItem lDefaultCoin = FWallet.UserCoins.Find(x => x.Id == FWallet.DefaultCoin);

            SettingsDialog.DefaultcoinImage = Universal.SystemUtils.BytesToIcon(lDefaultCoin.Icon).ToBitmap();
            SettingsDialog.DefaultCoin = lDefaultCoin.Name;
            SettingsDialog.DataPath = FWallet.DataFolder;
            SettingsDialog.DefaultDefaultCoin = "Bitcoin";
            SettingsDialog.DefaultPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).ToString(), "PandorasWallet");
            SettingsDialog.DefaultPort = 20159;
            SettingsDialog.DefaultServer = "localhost";
            SettingsDialog.PortNumber = Properties.Settings.Default.Port;
            SettingsDialog.ServerName = Properties.Settings.Default.ServerName;
            SettingsDialog.EncryptConnection = Properties.Settings.Default.EncyptedConnection;

            if (SettingsDialog.Execute())
            {
                uint lSelectedDefault = (uint)FWallet.UserCoins.Find(x => x.Name == SettingsDialog.DefaultCoin).Id;
                if (lSelectedDefault != FWallet.DefaultCoin)
                {
                    FWallet.DefaultCoin = lSelectedDefault;
                }

                if (FWallet.DataFolder != SettingsDialog.DataPath)
                {
                    FWallet.ChangeDataFolder(SettingsDialog.DataPath);
                    SetOrderDBHandler(FWallet.DataFolder, FWallet.InstanceId);
                }
#if DEBUG
                Properties.Settings.Default.Port = SettingsDialog.PortNumber;
                Properties.Settings.Default.ServerName = SettingsDialog.ServerName;
                Properties.Settings.Default.EncyptedConnection = SettingsDialog.EncryptConnection;
#endif
            }
        }

        partial void ExchangeInitialize();

        partial void RestoreInitialize();

        partial void BackupInitialize();

        partial void StartupExchangeProcess();

        private void MainForm_OnBackupClick(object sender, EventArgs e)
        {
            BackupWizard.Execute();
        }

        private void MainForm_OnSearchBoxTextChanged(object sender, EventArgs e)
        {
            MainForm.CurrencyViewControl.ExecuteSearch(CoinSearch);
        }

        private bool CoinSearch(ListView aListview, Dictionary<long, ListViewItem> aListviewCache)
        {
            if (string.IsNullOrEmpty(MainForm.SearchBar))
            {
                MainForm.CurrencyViewControl.SelectedCurrencyId = FWallet.ActiveCurrencyID;
                return false;
            }

            List<long> lNotFoundKeys = new List<long>();
            List<long> lFoundKeys = new List<long>();

            foreach (ListViewItem it in aListviewCache.Values)
            {
                if ((MainForm.SearchByTicker ? System.Text.RegularExpressions.Regex.IsMatch(it.SubItems[1].Text, MainForm.SearchBar, System.Text.RegularExpressions.RegexOptions.IgnoreCase) : System.Text.RegularExpressions.Regex.IsMatch(it.Text, MainForm.SearchBar, System.Text.RegularExpressions.RegexOptions.IgnoreCase)))
                {
                    lFoundKeys.Add(Convert.ToInt64(it.ImageKey));
                }
                else
                {
                    lNotFoundKeys.Add(Convert.ToInt64(it.ImageKey));
                }
            }

            foreach (long it in lFoundKeys)
            {
                ListViewItem lSavedItem = aListviewCache[it];
                bool lHaveItem = aListview.Items.Cast<ListViewItem>().Any(x => x.ImageKey == it.ToString());
                if (!lHaveItem)
                {
                    aListview.Items.Add(lSavedItem);
                }
            }

            foreach (long it in lNotFoundKeys)
            {
                ListViewItem lSavedItem = aListviewCache[it];
                bool lHaveItem = aListview.Items.Cast<ListViewItem>().Any(x => x.ImageKey == it.ToString());
                if (lHaveItem)
                {
                    aListview.Items.Remove(lSavedItem);
                }
            }
            MainForm.CurrencyViewControl.SelectedCurrencyId = FWallet.ActiveCurrencyID;
            return true;
        }

        private void MainForm_OnTransactionViewSelectionChanged(object sender, EventArgs e)
        {
            if (MainForm.TransactionView.SelectedIndices.Count > 0)
            {
                TransactionViewModel lSelectedTransactionView = (MainForm.TransactionView.SelectedItems[0].Tag as TransactionViewModel);
                string lTxNote = string.Format("Transaction ID: {1} {0}Transaction Block Number: {2}", Environment.NewLine, lSelectedTransactionView.TransactionID,
                    lSelectedTransactionView.Block);

                if (lSelectedTransactionView.TxFee > 0)
                {
                    lTxNote += string.Format("{0}Transaction Fee: {1}", Environment.NewLine, (decimal)lSelectedTransactionView.TxFee / FWallet.Coin);
                }

                TransactionUnit lChange = lSelectedTransactionView.ChangeOutput;

                if (lChange != null)
                {
                    lTxNote += string.Format("{0}Total change of {1}{2} returned to address: {3}", Environment.NewLine, (decimal)lChange.Amount / FWallet.Coin, FWallet.ActiveCurrencyItem.Ticker, lChange.Address);
                }

                MainForm.NotesBox = lTxNote;
            }
            else
            {
                MainForm.NotesBox = string.Empty;
            }
        }

        private void MainForm_OnTransactionSend(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MainForm.ToSendAddress))
            {
                throw new ClientExceptions.InvalidAddressException("Please add a valid address to continue.");
            }

            if (MainForm.ToSendAmount <= 0)
            {
                MainForm.ToSendAmount = 0;
                throw new ClientExceptions.InsufficientFundsException("Please add a valid ammount to continue.");
            }

            long lSelectedCurrency = MainForm.SelectedCurrencyId;

            decimal lAmount = MainForm.ToSendAmount;

            BalanceViewModel lBalanceModel = FWallet.GetBalance(FWallet.ActiveCurrencyID);
            decimal lBalance = Convert.ToDecimal(lBalanceModel.ToString());

            ulong lTxFee = FWallet.CalculateTxFee(MainForm.ToSendAddress, lAmount, FWallet.ActiveCurrencyID);

            ExecuteSendTxDialog(lAmount, lSelectedCurrency, lTxFee, lBalance, MainForm.ToSendAddress);

            MainForm.ToSendAmount = 0;
        }

        private void MainForm_OnSendAllMenuClick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MainForm.ToSendAddress))
            {
                throw new ClientExceptions.InvalidAddressException("Please add a valid address to continue.");
            }

            long lSelectedCurrency = MainForm.SelectedCurrencyId;

            BalanceViewModel lBalanceModel = FWallet.GetBalance(FWallet.ActiveCurrencyID);
            decimal lBalance = Convert.ToDecimal(lBalanceModel.ToString());

            ulong lTxFee = FWallet.CalculateTxFee(MainForm.ToSendAddress, lBalance, FWallet.ActiveCurrencyID);

            if (lBalance == 0)
            {
                throw new ClientExceptions.InsufficientFundsException("Not enough coins to make Transaction.");
            }

            decimal lAmount = lBalance - (lTxFee / (decimal)FWallet.Coin);

            ExecuteSendTxDialog(lAmount, lSelectedCurrency, lTxFee, lBalance, MainForm.ToSendAddress, false);
        }

        private string ExecuteSendTxDialog(decimal aAmount, long aSelectedCurrencyID, ulong aTxFee, decimal aBalance, string aAddress, bool SubstractFeeVisible = true)
        {
            CurrencyItem lSelectedCoin = FWallet.UserCoins.Where(x => x.Id == aSelectedCurrencyID).FirstOrDefault();

            string lTicker = lSelectedCoin.Ticker;

            TransactionDetailDialog.Ticker = lTicker;
            TransactionDetailDialog.Amount = aAmount;
            TransactionDetailDialog.ToAddress = aAddress;
            TransactionDetailDialog.Balance = aBalance;
            TransactionDetailDialog.TxFee = aTxFee / (decimal)FWallet.Coin;
            TransactionDetailDialog.FromAddress = FWallet.GetCoinAddress((uint)lSelectedCoin.Id);
            TransactionDetailDialog.TxFeeRate = lSelectedCoin.FeePerKb / (decimal)FWallet.Coin;
            TransactionDetailDialog.SubstractFeeVisible = SubstractFeeVisible;

            string lRetunedTxID = null;

            if (TransactionDetailDialog.Execute())
            {
                if (!EncryptionPasswordDialog.Execute())
                {
                    return null;
                }

                FWallet.InitializeRootSeed();

                string lTx;
                if (TransactionDetailDialog.isSubstractFeeChecked)
                {
                    ulong lNewAmount = Convert.ToUInt64((aAmount * FWallet.Coin) - aTxFee);
                    lTx = FWallet.PrepareNewTransaction(aAddress, lNewAmount, (uint)lSelectedCoin.Id, aTxFee);
                }
                else
                {
                    lTx = FWallet.PrepareNewTransaction(aAddress, aAmount, (uint)lSelectedCoin.Id, aTxFee);
                }

                long lTxHandle = FWallet.SendNewTransaction(lTx, (uint)lSelectedCoin.Id);

                System.Threading.Tasks.Task.Run(
                    () =>
                    {
                        try
                        {
                            bool lTimeout = false;
                            uint lCounter = 0;
                            string lTxID;
                            while (!FWallet.CheckTransactionHandle(lTxHandle, out lTxID))
                            {
                                lCounter++;

                                if (lCounter < 61)
                                {
                                    System.Threading.Thread.Sleep(1000);
                                }
                                else
                                {
                                    lTimeout = true;
                                    break;
                                }
#if DEBUG
                                System.Diagnostics.Debug.WriteLine("Checking TX");
#endif
                            }
                            if (lTimeout)
                            {
                                TrySendTxDialog?.BeginInvoke((MethodInvoker)delegate () { TrySendTxDialog.Response("Failed to Send Transaction."); });
                            }
                            else
                            {
                                TrySendTxDialog?.BeginInvoke((MethodInvoker)delegate () { TrySendTxDialog.Response("Transaction Sended", lTxID); });
                                lRetunedTxID = lTxID;
                            }
                        }
                        catch (PandoraServerException ex)
                        {
                            TrySendTxDialog?.BeginInvoke((MethodInvoker)delegate () { TrySendTxDialog.Response(ex.Message); });
                        }
                        catch (Exception ex)
                        {
                            string lError = " Unhandled error on sending transaction. Details: " + ex.Message + " on " + ex.Source;
                            Utils.PandoraLog.GetPandoraLog().Write(lError);
                            TrySendTxDialog?.BeginInvoke((MethodInvoker)delegate () { TrySendTxDialog.Response(lError); });
                        }
                    }
                );

                TrySendTxDialog.Execute();
            }
            return lRetunedTxID;
        }

        private void MainForm_OnFormLoad(object sender, EventArgs e)
        {
            ClearGui();
        }

        private void MainForm_OnCurrencySelectionChanged(object sender, EventArgs e)
        {
            if (MainForm.SelectedCurrencyId == 0 || MainForm.SelectedCurrencyId == FWallet.ActiveCurrencyID)
            {
                return;
            }

            FWallet.ActiveCurrencyID = (uint)MainForm.SelectedCurrencyId;

            ChangeCurrencyDetailData();

            UpdateTransactions(FWallet.ActiveCurrencyID, true);

            UpdateExchange(FWallet.ActiveCurrencyID);

            if (!MainForm.AllOrderHistoryChecked)
            {
                MainForm.ClearOrderHistory();
            }

            if (MainForm.SelectedExchangeMarket != null)
            {
                MainForm.ExchangeTargetPrice = FExchangeSelectedCoin.BasePrice;
            }
        }

        partial void UpdateExchange(uint aCurrency);

        private void ChangeCurrencyDetailData()
        {
            CurrencyItem lCurrencyItem = FWallet.UserCoins.Find(x => x.Id == FWallet.ActiveCurrencyID);

            MainForm.LabelCoinQuantity = string.Format("{0} ({1})", lCurrencyItem.Name, lCurrencyItem.Ticker);

            MainForm.CoinImage = Universal.SystemUtils.BytesToIcon(lCurrencyItem.Icon).ToBitmap();

            MainForm.CoinName = lCurrencyItem.Name;

            CurrencyStatusItem lCurrencyStatus = FWallet.CoinStatus[FWallet.ActiveCurrencyID];

            MainForm.CoinStatus = lCurrencyStatus.Status.ToString();

            BalanceViewModel lBalance = FWallet.GetBalance((uint)lCurrencyItem.Id);

            MainForm.TotalCoins = lBalance.ToString();

            MainForm.UnconfirmedBalance = lBalance.UnconfirmedToString();

            MainForm.UnconfirmedBalanceColor = lBalance.Unconfirmed == 0 ? Color.Gray : Color.Black;

            MainForm.Confirmed = lBalance.ConfirmedToString();

            MainForm.ConfirmedBalanceColor = lBalance.Unconfirmed == 0 ? Color.Gray : Color.Black;

            SetBalanceTooltip(lBalance.UnconfirmedSent, lBalance.UnconfirmedReceived);

            MainForm.SetTxSendAreaUsability(lCurrencyStatus.Status == CurrencyStatus.Active);

            MainForm.ReceiveAddress = FWallet.GetCoinAddress(FWallet.ActiveCurrencyID);

            if (FPreviousSelectedCurrency != 0)
            {
                FSendAddressCache[FPreviousSelectedCurrency] = MainForm.ToSendAddress;

                if (!FSendAddressCache.ContainsKey(lCurrencyItem.Id))
                {
                    FSendAddressCache[lCurrencyItem.Id] = string.Empty;
                }

                MainForm.ToSendAddress = FSendAddressCache[lCurrencyItem.Id];
            }

            FPreviousSelectedCurrency = lCurrencyItem.Id;
        }

        private void SetBalanceTooltip(string aUnconfirmedSent, string aUnconfirmedReceived)
        {
            string lMessage = string.Empty;

            lMessage += "Here you can find details refered to your current balance." + Environment.NewLine;
            lMessage += Environment.NewLine + "Unconfirmed balances:" + Environment.NewLine;
            lMessage += string.Format("{0}Pending Sent: {1} {0}Pending Received: {2}", Environment.NewLine, aUnconfirmedSent, aUnconfirmedReceived);

            MainForm.BalanceToolTip = lMessage;
        }

        public void UpdateTransactions(uint aSelectedCoinId, bool isSelectionChanged = false)
        {
            List<string> lKeys = new List<string>();

            foreach (ListViewItem it in MainForm.TransactionView.Items)
            {
                lKeys.Add(it.Name);
            }

            string lSelected = null;

            if (MainForm.TransactionView.SelectedItems.Count > 0 && !isSelectionChanged)
            {
                lSelected = MainForm.TransactionView.SelectedItems[0].Name;
            }

            MainForm.TransactionView.BeginUpdate();

            foreach (string it in lKeys)
            {
                MainForm.TransactionView.Items.RemoveByKey(it);
            }

            List<TransactionViewModel> lTxViews = FWallet.TransactionsByCurrency[aSelectedCoinId];

            foreach (TransactionViewModel it in lTxViews)
            {
                ListViewItem lItem = new ListViewItem(it.Date.ToString())
                {
                    Name = it.TransactionID,
                    Tag = it
                };

                lItem.SubItems.Add(it.GetSimpleInputAddress());
                lItem.SubItems.Add(it.GetSimpleOutputAddress());

                lItem.SubItems.Add((it.Debt / FWallet.Coin).ToString());
                lItem.SubItems.Add((it.Credit / FWallet.Coin).ToString());

                lItem.SubItems.Add(it.isConfirmed ? string.Format("Confirmed ({0}+)", it.MinConfirmations) : it.Confirmation.ToString());

                switch (it.TransactionType)
                {
                    case TransactionViewType.credit:

                        lItem.ImageKey = "Credit";
                        break;

                    case TransactionViewType.debt:

                        lItem.ImageKey = "Debit";
                        break;

                    case TransactionViewType.both:

                        lItem.ImageKey = "Both";
                        break;

                    case TransactionViewType.none:

                        lItem.ImageKey = "None";
                        break;
                }

                MainForm.TransactionView.Items.Add(lItem);
            }

            if (!string.IsNullOrEmpty(lSelected))
            {
                MainForm.TransactionView.Items[lSelected].Selected = true;
            }

            MainForm.TransactionView.EndUpdate();
        }

        private void ClearGui()
        {
            MainForm.CurrencyViewControl.ClearCurrencies();
            MainForm.SetUserStatus(PandoraClientMainWindow.UserStatuses.NotConnected);
            MainForm.CoinImageVisibility = false;
            MainForm.CoinStatus = PandoraClientMainWindow.UserStatuses.NotConnected.GetEnumDescription();
            MainForm.CoinName = "No Coin Selected";
            MainForm.ReceiveAddress = string.Empty;
            MainForm.ToSendAmount = 0;
            MainForm.ToSendAddress = "";
        }

        private void EncryptionPasswordDialog_OnCancelButtonClick(object sender, EventArgs e)
        {
            FEncryptionCancellationtoken?.Cancel();
        }

        private void EncryptionPasswordDialog_OnOkButtonClick(object sender, EventArgs e)
        {
            FEncryptionCancellationtoken = new CancellationTokenSource();
            Task.Run(() => WalletEncryptionProcess(EncryptionPasswordDialog.Password, FWorkingWallet));
        }

        private void WalletEncryptionProcess(string aPassword, PandoraWallet aWallet)
        {
            string lResult;
            try
            {
                if (string.IsNullOrEmpty(FPasscode))
                {
                    aWallet.DecryptWallet(aPassword);
                }
                else
                {
                    aWallet.DecryptWallet(aPassword, FPasscode);
                }
                lResult = string.Empty;
            }
            catch (Wallet.ClientExceptions.WrongPasswordException ex)
            {
                lResult = ex.Message;
            }

            if (FEncryptionCancellationtoken.IsCancellationRequested)
            {
                lResult = "Process aborted.";
            }

            EncryptionPasswordDialog.Invoke(new MethodInvoker(() => EncryptionPasswordDialog.SetResult(lResult)));
        }

        private void MainForm_OnAddCurrencyBtnClick(object sender, EventArgs e)
        {
            FWorkingWallet = FWallet;

            if (!EncryptionPasswordDialog.Execute())
            {
                return;
            }

            FWallet.InitializeRootSeed();

            List<AddCoinSelector.lstCurrencyViewItem> lItems = new List<AddCoinSelector.lstCurrencyViewItem>();

            foreach (CurrencyItem it in FWallet.UnselectedCoins)
            {
                Icon lIcon = Universal.SystemUtils.BytesToIcon(it.Icon);
                lItems.Add(new AddCoinSelector.lstCurrencyViewItem { CurrencyIcon = lIcon, CurrencyID = it.Id, CurrencyName = it.Name, CurrencySimbol = it.Ticker });
            }
            AddCoinSelectorDialog.AddItemsToShow(lItems);
            bool lAny = false;
            if (AddCoinSelectorDialog.Execute())
            {
                foreach (long it in AddCoinSelectorDialog.SelectedItems)
                {
                    lAny = true;
                    FWallet.AddNewSelectedCurrency((uint)it);
                }

                if (lAny)
                {
                    SetUserCoins(FWallet);
                }
            }
        }

        private void SetUserCoins(PandoraWallet aWallet)
        {
            List<CurrencyItem> lUserCoins = aWallet.UserCoins;
            AddCoinsToCurrencyView(lUserCoins, aWallet);
            aWallet.SetCoinAccountData(lUserCoins);
        }

        private void ConnectDialog_OnOkClick(object sender, EventArgs e)
        {
            FWorkingWallet = PandoraWallet.TryToGetNewWalletInstance(ConnectDialog.Email, ConnectDialog.Username, ConnectDialog.Password);
            ConnectDialog.UserConnected = FWorkingWallet != null;
        }

        public bool Connected => FWallet == null && FWallet.Connected;

        private void MainForm_OnConnect(object sender, EventArgs e)
        {
            try
            {
                ConnectDialog.Username = Properties.Settings.Default.Username;
                ConnectDialog.Email = Properties.Settings.Default.Email;
                ConnectDialog.UserConnected = false;

                do
                {
                    if (!ConnectDialog.Execute())
                    {
                        if (FStartUpConnected)
                        {
                            break;
                        }
                        else
                        {
                            MainForm.Close();
                        }
                    }

                    Task<bool> lInitializeTask = Task.Run(() =>
                    {
                        Thread.Sleep(100);

                        bool lResult = WalletCreationProcess();

                        MainForm.BeginInvoke(new MethodInvoker(() => InitializingDialog.SetInitialized()));

                        return lResult;
                    });

                    InitializingDialog.ShowDialog();

                    lInitializeTask.Wait();

                    if (lInitializeTask.IsFaulted)
                    {
                        throw lInitializeTask.Exception.InnerExceptions[0];
                    }

                    if (!lInitializeTask.Result)
                    {
                        continue;
                    }

                    StartupExchangeProcess();
                }
                while (!FStartUpConnected);

                Utils.PandoraLog.GetPandoraLog().Write("Succesfully logged as " + FWallet.Username);
            }
            catch (Exception ex)
            {
                Utils.PandoraLog.GetPandoraLog().Write("An exception during initialization ocurred. Details: " + ex.Message + " on " + ex.Source);
                throw;
            }
            finally
            {
                if (FWallet == null)
                {
                    Utils.PandoraLog.GetPandoraLog().Write("Fatal Error. Application shutting down.");
                    Application.Exit();
                }
            }
        }

        private delegate bool BoolInvoker();

        private bool WalletCreationProcess(bool aIsRestore = false)
        {
            if (!StartupProcess(aIsRestore))
            {
                return false;
            }

            FWallet = FWorkingWallet;

            MainForm?.Invoke(new MethodInvoker(() => PrepareGUIWithWallet(FWallet)));

            FWallet = FWorkingWallet;

            FStartUpConnected = true;

            FWallet.InitTxTracking();
            FWallet.InitCurrencyStatusUpdating();

            return true;
        }

        private void FWallet_OnCurrencyItemUpdated(ulong aCurrencyID)
        {
            MainForm?.BeginInvoke(new MethodInvoker(() => RefreshInterface()));
        }

        private void PrepareGUIWithWallet(PandoraWallet aWallet)
        {
            MainForm.SetUserStatus(PandoraClientMainWindow.UserStatuses.Connected, aWallet.Email, aWallet.Username);
            MainForm.FormName = string.Format("Pandora's Wallet - {0} - {1}", aWallet.Username, aWallet.Email);
            MainForm.SelectedCurrencyId = aWallet.DefaultCoin;
            MainForm.CoinImageVisibility = true;
        }

        private bool StartupProcess(bool aForceCreation = false)
        {
            if (FWorkingWallet.CheckIfUserHasAccounts() && !aForceCreation)
            {
                if (FWorkingWallet.DefaultCoin == 0 || !FWorkingWallet.Encrypted)
                {
                    if ((bool)MainForm.Invoke(new BoolInvoker(() => !ExecuteRestoringProccess())))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!ExecuteDefaultCoinSelector(FWorkingWallet))
                {
                    return false;
                }

                uint lSelectedID = (uint)DefaultCoinSelectorDialog.SelectedCurrencyID;

                FWorkingWallet.DefaultCoin = lSelectedID;
                FWorkingWallet.AddNewSelectedCurrency(lSelectedID);

                if ((bool)MainForm.Invoke(new BoolInvoker(() => !EncryptionPasswordDialog.Execute(true))))
                {
                    return false;
                }

                FWorkingWallet.InitializeRootSeed(true);
            }

            MainForm?.Invoke(new MethodInvoker(() => MainForm.CurrencyViewControl.ClearCurrencies()));

            MainForm?.Invoke(new MethodInvoker(() => SetUserCoins(FWorkingWallet)));

            if (FWallet != null)
            {
                FWallet.Close();
            }

            FWorkingWallet.OnCurrencyItemUpdated += FWallet_OnCurrencyItemUpdated;
            FWorkingWallet.OnNewTxData += FWallet_OnNewTxData;
            FWorkingWallet.OnNewCurrencyStatusData += FWallet_OnNewCurrencyStatusData;

            return true;
        }

        private void FWallet_OnNewCurrencyStatusData()
        {
            MainForm?.BeginInvoke(new MethodInvoker(() => RefreshInterface()));
        }

        public void RefreshInterface()
        {
            ChangeCurrencyDetailData();

            AddCoinsToCurrencyView(FWorkingWallet.UserCoins, FWallet);

            MainForm.SelectedCurrencyId = FWallet.ActiveCurrencyID;
        }

        private void FWallet_OnNewTxData()
        {
            MainForm?.BeginInvoke(new MethodInvoker(() =>
            {
                UpdateTransactions(FWallet.ActiveCurrencyID, false);
                RefreshInterface();
            }));
        }

        private bool ExecuteRestoringProccess(string aError = null)
        {
            RestoreDialog.InitialErrorMessage = aError;
            if (!RestoreDialog.Execute())
            {
                return false;
            }

            return false;
        }

        private bool ExecuteDefaultCoinSelector(PandoraWallet aWallet, bool aOnlyUserCoins = false)
        {
            List<DefaultCoinSelector.lstCurrencyListItem> lListOfCurrency = new List<DefaultCoinSelector.lstCurrencyListItem>();

            aWallet.UsingFullCoinList = true;

            CurrencyItem[] lCurrencies;
            if (aOnlyUserCoins)
            {
                lCurrencies = aWallet.UserCoins.ToArray();
            }
            else
            {
                lCurrencies = aWallet.GetFullListOfCurrencies();
            }

            foreach (CurrencyItem it in lCurrencies)
            {
                lListOfCurrency.Add(new DefaultCoinSelector.lstCurrencyListItem { CurrencyIcon = Universal.SystemUtils.BytesToIcon(it.Icon), CurrencyID = it.Id, CurrencyName = it.Name, CurrencySimbol = it.Ticker });
            }

            aWallet.UsingFullCoinList = false;

            MainForm.Invoke(new MethodInvoker(() => DefaultCoinSelectorDialog.AddItemsToShow(lListOfCurrency)));

            return (bool)MainForm.Invoke(new BoolInvoker(() => DefaultCoinSelectorDialog.Execute()));
        }

        private void AddCoinsToCurrencyView(List<CurrencyItem> aListCoins, PandoraWallet aWallet)
        {
            foreach (CurrencyItem lItem in aListCoins)
            {
                MainForm.CurrencyViewControl.AddCurrency(lItem.Id, lItem.Name, lItem.Ticker, Globals.BytesToIcon(lItem.Icon), new string[] { aWallet.GetBalance((uint)lItem.Id).ToString(), aWallet.CoinStatus[(uint)lItem.Id].Status.ToString() });
            }
        }

        public static PandoraClientControl GetController(PandoraClientMainWindow aPandoraClientMainWindow)
        {
            if (FPandoraClientControl == null)
            {
                FPandoraClientControl = new PandoraClientControl(aPandoraClientMainWindow);
            }

            return FPandoraClientControl;
        }
    }
}