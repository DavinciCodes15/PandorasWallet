using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO.Compression;
using System.IO;
using Pandora.Client.Dialogs;
using Pandora.Client.ServerAccess;
using Pandora.Client.Wallet;
using Pandora.Server.ClientLib;

namespace Pandora.Client
{
    public partial class PandoraClientMainWindow: Form
    {
        private const string STATUS_NotConnected = "Not Connected";
        private const string STATUS_Connected = "Connected";

        private static bool FShown = false;
        private string FCoinStatus;

        private PandoraWallet FWallet = null;

        private List<ListViewItem> FListViewBackup = new List<ListViewItem>();

        //Dialogs
        public RestoreWalletWizard FRestoreWallet;

        public GenerateBackupWizard FGenerateBackup;
        public ConnectDialog FLoginDialog;
        public DefaultCoinSelector FDefaultCoinSelector;
        public SendTransaction FSendTransaction;
        public AddCoinSelector FAddCoinSelector;
        public SendingTxDialog FSendingTxDialog;
        public WalletPasswordDialog FWalletPasswordBox;

        //Arrays

        public CurrencyItem[] FCurrencyItems;
        public SettingsDialog FSettingsDialog;
        public CurrencyStatusItem[] FCurrencyStatuses;
        public CurrencyItem[] FCurrenciesList;

        private uint FSelectedCurrency
        {
            get => FWallet.ActiveCurrency;
            set => FWallet.ActiveCurrency = value;
        }

        private CurrencyStatus FSelectedCurrencyStatus { get => FWallet.ActiveCurrencyStatus; set => FWallet.ActiveCurrencyStatus = value; }

        private BalanceViewModel Balance
        {
            get => FWallet.BalancesByCurrency[FSelectedCurrency];
        }

        public string CoinStatus
        {
            get
            {
                return FCoinStatus;
            }
            set
            {
                FCoinStatus = value;
                lblStatus.Text = FCoinStatus;
            }
        }

        public void OnNewTxDataHandler()
        {
            string lSelected = String.Empty;

            if (listViewCryptoCoins.SelectedIndices.Count >= 1)
            {
                lSelected = listViewCryptoCoins.SelectedItems[0].Name;
            }

            AddOrRenewCoinToListView(FWallet.UserCoins);
        }

        public MainWindow()
        {
            InitializeComponent();
            Text = String.Format("{0} - [{1}]", AboutBox.AssemblyProduct, STATUS_NotConnected);
            picCoinImage.Visible = false;
            lblCoinName.Text = "No Coin Selected";
            CoinStatus = STATUS_NotConnected;

            FSettingsDialog = new SettingsDialog(this);
            FDefaultCoinSelector = new DefaultCoinSelector();
            FWalletPasswordBox = new WalletPasswordDialog();
            FLoginDialog = new ConnectDialog();
            FRestoreWallet = new RestoreWalletWizard();
            FLoginDialog.Username = Properties.Settings.Default.Username;
            FLoginDialog.Email = Properties.Settings.Default.Email;
            FDefaultCoinSelector.RestorePressed = false;

            do
            {
                Cursor.Current = Cursors.WaitCursor;

                if (!FLoginDialog.Execute(out FWallet))
                {
                    Close();
                    return;
                }
                Cursor.Current = Cursors.WaitCursor;
                do
                {
                    if (FWallet.DefaultCoin == 0 && !FDefaultCoinSelector.Execute(FWallet))
                    {
                        if (FDefaultCoinSelector.RestorePressed)
                        {
                            FRestoreWallet = new RestoreWalletWizard();
                            FRestoreWallet.Execute(FWallet);
                            FDefaultCoinSelector.RestorePressed = false;
                            FDefaultCoinSelector.DialogResult = DialogResult.OK;
                        }
                    }
                } while (FWallet.DefaultCoin == 0 && FDefaultCoinSelector.DialogResult != DialogResult.Cancel);

                if (FDefaultCoinSelector.DialogResult == DialogResult.Cancel)
                {
                    FWallet.Close();
                    FWallet = null;
                }

                if (FWallet != null && !FWalletPasswordBox.Execute(FWallet, false, this))
                {
                    FWallet.Close();
                    FWallet = null;
                    Cursor.Current = Cursors.Default;
                    continue;
                }

                if (FWallet != null)
                {
                    Cursor.Current = Cursors.WaitCursor;

                    FWallet.Initialize();
                    FWallet.OnNewTxData += OnNewTxDataHandler;

                    Cursor.Current = Cursors.Default;
                }
            } while (FWallet == null);

            Cursor.Current = Cursors.WaitCursor;

            Text = String.Format("{0} - [{1}]", AboutBox.AssemblyProduct, STATUS_Connected);

            FAddCoinSelector = new AddCoinSelector();

            FSendingTxDialog = new SendingTxDialog(FWallet);

            AddOrRenewCoinToListView(FWallet.UserCoins);

            listViewCryptoCoins.Items[Convert.ToString(FWallet.DefaultCoin)].Selected = true;

            FWallet.InitTxTracking();

            FSendTransaction = new SendTransaction();

            Cursor.Current = Cursors.Default;
        }

        private void AddOrRenewCoinToListView(CurrencyItem aCoin)
        {
            uint lId = Convert.ToUInt32(aCoin.Id);

            bool lAlreadyAdded = listViewCryptoCoins.Items.ContainsKey(Convert.ToString(aCoin.Id));

            listViewCryptoCoins.BeginUpdate();

            if (lAlreadyAdded)
            {
                listViewCryptoCoins.Items.RemoveByKey(Convert.ToString(lId));
            }

            ListViewItem lItem = new ListViewItem();

            imageListCoin.Images.Add(aCoin.Name, Globals.BytesToIcon(aCoin.Icon));
            lItem.Text = aCoin.Name;
            lItem.Tag = aCoin;
            lItem.Name = Convert.ToString(aCoin.Id);
            lItem.SubItems.Add(aCoin.Ticker);
            lItem.SubItems.Add(FWallet.GetBalance(lId).ToString());
            lItem.SubItems.Add(FWallet.CoinStatus[lId].Status.ToString());
            lItem.ImageKey = aCoin.Name;

            listViewCryptoCoins.Items.Add(lItem);

            listViewCryptoCoins.EndUpdate();
        }

        private void AddOrRenewCoinToListView(List<CurrencyItem> aListCoins)
        {
            imageListCoin.Images.Clear();

            ListViewItem lSelectedItem = null;

            if (listViewCryptoCoins.SelectedIndices.Count >= 1)
            {
                lSelectedItem = listViewCryptoCoins.SelectedItems[0];
            }

            foreach (CurrencyItem it in aListCoins)
            {
                AddOrRenewCoinToListView(it);
            }

            listViewCryptoCoins.Items[lSelectedItem != null ? lSelectedItem.Name : FWallet.DefaultCoin.ToString()].Selected = true;

            //ChangeInterfaceSelectedItem();

            FListViewBackup.AddRange(listViewCryptoCoins.Items.Cast<ListViewItem>());
        }

        private uint ChangeInterfaceSelectedItem()
        {
            if (listViewCryptoCoins.SelectedIndices.Count >= 1)
            {
                ListViewItem lSelectedItem = listViewCryptoCoins.SelectedItems[0];

                CurrencyItem lSelectedCurrency = (CurrencyItem)lSelectedItem.Tag;

                Icon lIcon = Globals.BytesToIcon(lSelectedCurrency.Icon);

                uint lSelectedID = Convert.ToUInt32(lSelectedCurrency.Id);
#if DEBUG
                txtBoxReceiveAddress.Text = FWallet.GetCoinAddress(lSelectedID, true);
#else
                txtBoxReceiveAddress.Text = FWallet.GetCoinAddress(lSelectedID);
#endif

                picCoinImage.Image = lIcon.ToBitmap();
                picCoinImage.Visible = true;
                picCoinImage.Refresh();

                lblCoinName.Text = lSelectedCurrency.Name;

                FSelectedCurrencyStatus = FWallet.CoinStatus[lSelectedID].Status;

                string lBalance = FWallet.GetBalance(lSelectedID).ToString(); ;

                lblTotal.Text = lBalance;

                lSelectedItem.SubItems[2].Text = lBalance;

                lblStatus.Text = FSelectedCurrencyStatus.ToString();

                UpdateTransactions(lSelectedID, true);

                return Convert.ToUInt32(lSelectedCurrency.Id);
            }

            return 0;
        }

        public void UpdateTransactions(uint aSelectedCoinId, bool isSelectionChanged = false)
        {
            List<string> lKeys = new List<string>();

            foreach (ListViewItem it in listTransactions.Items)
            {
                lKeys.Add(it.Name);
            }

            string lSelected = null;

            if (listTransactions.SelectedItems.Count > 0 && !isSelectionChanged)
            {
                lSelected = listTransactions.SelectedItems[0].Name;
            }

            listTransactions.BeginUpdate();

            foreach (string it in lKeys)
            {
                listTransactions.Items.RemoveByKey(it);
            }

            foreach (TransactionViewModel it in FWallet.TransactionsByCurrency[aSelectedCoinId])
            {
                ListViewItem lItem = new ListViewItem(it.Date.ToString());

                lItem.Name = it.TransactionID;
                lItem.Tag = it;

                lItem.SubItems.Add(it.GetSimpleInputAddress());
                lItem.SubItems.Add(it.GetSimpleOutputAddress());

                lItem.SubItems.Add((it.Debt / FWallet.Coin).ToString());
                lItem.SubItems.Add((it.Credit / FWallet.Coin).ToString());

                lItem.SubItems.Add(it.isConfirmed ? String.Format("Confirmed ({0}+)", it.MinConfirmations) : it.Confirmation.ToString());

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

                listTransactions.Items.Add(lItem);
            }

            if (!String.IsNullOrEmpty(lSelected))
            {
                listTransactions.Items[lSelected].Selected = true;
            }

            listTransactions.EndUpdate();
        }

        private void MainWallet_Shown(object sender, EventArgs e)
        {
            if (!FShown)
            {
                FShown = true;
            }
        }

        private void MainWallet_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                FWallet.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "An error occured during logoff");
            }
        }

        private void DisconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Globals.Logout = true;
            Close();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aDlg = new AboutBox();
            aDlg.Execute();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FSettingsDialog.Show();
        }

        private void AddCurrencyBtn_Click(object sender, EventArgs e)
        {
            FAddCoinSelector.Execute(FWallet);

            AddOrRenewCoinToListView(FWallet.UserCoins);
        }

        private void listViewCryptoCoins_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            uint lId = ChangeInterfaceSelectedItem();

            if (lId != 0)
            {
                FSelectedCurrency = lId;
            }

            QuickAmmountLabel.Text = String.Format("Ammount ({0}... remaining)", Balance.ToString());
            FTextBoxSendToAddress.Text = String.Empty;
        }

        private void CoinSearchBox_TextChanged(object sender, EventArgs e)
        {
            List<string> lNotFoundKeys = new List<string>();
            List<string> lFoundKeys = new List<string>();

            foreach (ListViewItem it in FListViewBackup)
            {
                if ((isTickerCheckBox.Checked ? Regex.IsMatch((it.Tag as CurrencyItem).Ticker, CoinSearchBox.Text, RegexOptions.IgnoreCase) : Regex.IsMatch((it.Tag as CurrencyItem).Name, CoinSearchBox.Text, RegexOptions.IgnoreCase)))
                {
                    lFoundKeys.Add(it.Name);
                }
                else
                {
                    lNotFoundKeys.Add(it.Name);
                }
            }

            foreach (string it in lFoundKeys)
            {
                if (!listViewCryptoCoins.Items.ContainsKey(it))
                    listViewCryptoCoins.Items.Add(FListViewBackup.Find(x => x.Name == it));
            }

            foreach (string it in lNotFoundKeys)
            {
                if (listViewCryptoCoins.Items.ContainsKey(it))
                    listViewCryptoCoins.Items.RemoveByKey(it);
            }
        }

        private void QuickSendButton_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(FTextBoxSendToAddress.Text))
            {
                MessageBox.Show(this, "Please add a valid address to continue.", "Invalid Transaction", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            if (String.IsNullOrWhiteSpace(QuickAmountTextBox.Text))
            {
                MessageBox.Show(this, "Please add a valid ammount to continue.", "Invalid Transaction", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            ListViewItem lSelectedItem = listViewCryptoCoins.SelectedItems[0];
            CurrencyItem lSelectedCurrency = (CurrencyItem)lSelectedItem.Tag;

            string lTicker = lSelectedCurrency.Ticker;

            FSendTransaction.ToAddress = FTextBoxSendToAddress.Text;
            FSendTransaction.Ammount = QuickAmountTextBox.Text + " " + lTicker;
            FSendTransaction.FromAddress = txtBoxReceiveAddress.Text;
            if (FSendTransaction.Execute())
            {
                if (!FWalletPasswordBox.Execute(FWallet, true, this))
                {
                    return;
                }

                try
                {
                    long lTxHandle = FWallet.SendNewTransaction(FTextBoxSendToAddress.Text, Convert.ToDecimal(QuickAmountTextBox.Text), FWallet.ActiveCurrency);

                    FSendingTxDialog.Execute(lTxHandle);
                }
                catch (ClientExceptions.InsufficientFundsException ex)
                {
                    MessageBox.Show(this, ex.Message, "Insufficient Funds", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    QuickAmountTextBox.Text = "0";
                }
                catch (ClientExceptions.InvalidAddressException ex)
                {
                    MessageBox.Show(this, ex.Message, "Invalid Address", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    QuickAmountTextBox.Text = "0";
                }
                catch (ClientExceptions.InvalidOperationException ex)
                {
                    MessageBox.Show(this, ex.Message, "Invalid Amount", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    QuickAmountTextBox.Text = "0";
                }
            }
        }

        private void QuickAmmountTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void QuickAmountTextBox_TextChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(QuickAmountTextBox.Text))
            {
                UInt64 lAmount = (ulong)(Convert.ToDecimal(QuickAmountTextBox.Text) * FWallet.Coin);

                UInt64 lRemainingBalance = Balance.Confirmed - lAmount;

                QuickAmmountLabel.Text = String.Format("Ammount ({0}... remaining)", Balance.Confirmed > lAmount ? (decimal)lRemainingBalance / (decimal)FWallet.Coin : 0);
            }
            else
            {
                QuickAmmountLabel.Text = String.Format("Ammount ({0}... remaining)", Balance.ConfirmedToString());
            }
        }

        private void listTransactions_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (listTransactions.SelectedIndices.Count > 0)
            {
                TxNotesBox.Text = String.Format("Transaction ID: {0}", (listTransactions.SelectedItems[0].Tag as TransactionViewModel).TransactionID);
            }
            else TxNotesBox.Text = String.Empty;
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            FGenerateBackup = new GenerateBackupWizard(this, FWallet);
            FGenerateBackup.Show();
            this.Hide();
            //try
            //{
            //    //folderBrowserDialog1 = new FolderBrowserDialog();
            //    //if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            //        //FWallet.CreateBackup(folderBrowserDialog1.SelectedPath);
            //        //FWallet.GenerateTwelveWords();
            //}catch(Exception ex)
            //{
            //    MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }
    }
}