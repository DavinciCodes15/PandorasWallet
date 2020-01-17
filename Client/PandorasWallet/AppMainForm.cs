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
using Pandora.Client.PandorasWallet.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet
{
    public partial class AppMainForm : Form
    {
        public enum UserStatuses
        {
            [Description("No Status")]
            Node = 0,

            [Description("Connected")]
            Connected = 1,

            [Description("Not Connected")]
            NotConnected = 2
        }

        private static bool FShown = false;
        private Dictionary<long, Currency> FCurrencyLookup = new Dictionary<long, Currency>();

        public event EventHandler OnConnect;

        public event EventHandler OnChangeExchangeKeysBtnClick;

        public event EventHandler OnAddCurrencyBtnClick;

        public event EventHandler OnDisconnect;

        public event FormClosingEventHandler OnWalletFormClosing;

        public event EventHandler OnTransactionSend;

        public event EventHandler OnExchangeSelectedCurrencyChanged;

        public event EventHandler OnFormLoad;

        public event EventHandler OnSendAllMenuClick;

        public event EventHandler OnTransactionViewSelectionChanged;

        public event EventHandler OnBackupClick;

        public event EventHandler OnTransactionNameChanged;

        public event EventHandler OnLabelEstimatePriceClick;

        public event EventHandler OnPriceChanged;

        public event EventHandler OnCheckAllOrderHistory;

        public event EventHandler OnSettingsMenuClick;

        public event EventHandler OnTxtQuantityLeave;

        public event EventHandler OnTxtTotalLeave;

        public event EventHandler OnSelectedCurrencyChanged;

        public AppMainForm()
        {
            InitializeComponent();
            cbExchange.Items.Add("--Select--");
            cbExchange.SelectedIndex = 0;

            Cursor.Current = Cursors.WaitCursor;

            lstViewCurrencies.SetVisualOptions(CurrencyView.VisualOptionFlags.CurrencyNameVisible | CurrencyView.VisualOptionFlags.TickerColunmVisible | CurrencyView.VisualOptionFlags.IconVisible, new string[] { "Total", "Status" });

            Cursor.Current = Cursors.Default;

            QuickSendButton.AddMenuItem("Send available balance");
            QuickSendButton.AssingOnClickEvent(OnSendAllMenuClickEvent, 0);

            TransactionView.ListViewItemSorter = new PandoraListViewItemSorter(0, SortOrder.Descending);

            lstExchangeMarket.LargeImageList = lstViewCurrencies.ImageList;
            lstExchangeMarket.SmallImageList = lstViewCurrencies.ImageList;
        }

        public void SetUserStatus(UserStatuses aStatus, string aEmail = null, string aUsername = null)
        {
            string lAppTitle = "Pandora's Wallet";
            if (aUsername != null)
            {
                toolStripStatusUsername.Text = aUsername;
                lAppTitle += $" - {aUsername}";
            }
            else
            {
                toolStripStatusUsername.Text = " - ";
            }

            if (aEmail != null)
            {
                toolStripStatusEmail.Text = aEmail;
                lAppTitle += $" - {aEmail}";
            }
            else
            {
                toolStripStatusEmail.Text = " - ";
            }

            toolStripConnectionStatus.Text = aStatus.GetEnumDescription();
            FormName = lAppTitle;
        }

        private CurrencyView CurrencyViewControl => lstViewCurrencies;

        public StatusControl StatusControlExchange => statsctrlExchage;

        public StatusControl StatusControlOrderHistory => statscntrlTradeHistory;

        public string FormName { get => Text; set => Text = value; }

        public string BalanceToolTip { set => toolTipBalance.SetToolTip(pictureBoxBalanceInfo, value); }

        public string ReceiveAddress { get => txtBoxReceiveAddress.Text; set => txtBoxReceiveAddress.Text = value; }

        public string ToSendAddress { get => TxtBoxSendToAddress.Text; set => TxtBoxSendToAddress.Text = value; }

        public decimal ToSendAmount { get => Convert.ToDecimal(QuickAmountTextBox.Text, new System.Globalization.NumberFormatInfo() { NumberDecimalSeparator = ".", NumberGroupSeparator = ",", NumberGroupSizes = new[] { 3 } }); set => QuickAmountTextBox.Text = value.ToString(); }

        public bool CoinImageVisibility { get => picCoinImage.Visible; set => picCoinImage.Visible = value; }

        public string UnconfirmedBalance { get => lblUnconfirmed.Text; set => lblUnconfirmed.Text = value; }

        public Color UnconfirmedBalanceColor { get => lblUnconfirmed.ForeColor; set => lblUnconfirmed.ForeColor = lblNameUnconfirmed.ForeColor = value; }

        //public Color ConfirmedBalanceColor { get => lblConfirmed.ForeColor; set => lblConfirmed.ForeColor = lblNameConfirmed.ForeColor = value; }

        //public string Confirmed { get => lblConfirmed.Text; set => lblConfirmed.Text = value; }

        public string SearchBar { get => txtBoxSearchCoin.Text; set => txtBoxSearchCoin.Text = value; }

        public long SelectedCurrencyId { get => lstViewCurrencies.SelectedCurrencyId; set => lstViewCurrencies.SelectedCurrencyId = value; }

        public string TotalCoins { get => lblTotal.Text; set => lblTotal.Text = value; }

        public string CoinName { get => lblCoinName.Text; set => lblCoinName.Text = value; }

        public string NotesBox { get => txtNotesBox.Text; set => txtNotesBox.Text = value; }

        public bool CheckAllOrderHistoryEnabled { set => chckOrderHistory.Enabled = value; }

        public ListView TransactionView => listTransactions;

        public Image CoinImage { get => picCoinImage.Image; set { picCoinImage.Image = value; picCoinImage.Visible = true; } }

        public Transaction SelectedTransaction { get; private set; }

        public IEnumerable<Currency> Currencies { get => FCurrencyLookup.Values; }
        public string CoinStatus { get => lblStatus.Text; set { lblStatus.Text = value; } }

        public Currency GetCurrency(long aCurrenyId)
        {
            if (!FCurrencyLookup.TryGetValue(aCurrenyId, out Currency lResult))
                Universal.Log.Write(Universal.LogLevel.Warning, "Unable to find currency id {0}", aCurrenyId);
            return lResult;
        }

        public void AddCurrency(Currency aCurrency)
        {
            if (!FCurrencyLookup.ContainsKey(aCurrency.Id))
            {
                var lCustomValues = new string[] { FormatedAmount(aCurrency.Balance, aCurrency.Precision), aCurrency.CurrentStatus.ToString() };
                CurrencyViewControl.AddCurrency(aCurrency.Id, aCurrency.Name, aCurrency.Ticker, Globals.BytesToIcon(aCurrency.Icon), lCustomValues);
                FCurrencyLookup.Add(aCurrency.Id, aCurrency);
            }
        }

        public void SetTxSendAreaUsability(bool aEnabled)
        {
            QuickSendButton.Enabled = aEnabled;
            TxtBoxSendToAddress.Enabled = aEnabled;
            QuickAmountTextBox.Enabled = aEnabled;
        }

        public void ClearAll()
        {
            TransactionView.Items.Clear();
            CurrencyViewControl.ClearCurrencies();
            FCurrencyLookup.Clear();
            this.ClearExchangeList();
            this.ClearListExchangeTo();
            this.ClearOrderHistory();
            ToSendAddress = string.Empty;
            ToSendAmount = 0;
        }

        public static string FormatedAmount(decimal aAmount, ushort aPrecision)
        {
            var lFormat = string.Format("{0}0:#,0.{1}{2}", '{', new string('0', aPrecision), '}');
            return string.Format(lFormat, aAmount);
        }

        private void CurrencyListView_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var lId = lstViewCurrencies.SelectedCurrencyId;
                TransactionView.Items.Clear();
                if (FCurrencyLookup.ContainsKey(lId))
                {
                    SelectedCurrency = FCurrencyLookup[SelectedCurrencyId];
                    this.CoinImage = Globals.BytesToIcon(SelectedCurrency.Icon).ToBitmap();
                    UpdateCurrencyView();
                    this.NotesBox = "";

                    foreach (var lTransaction in SelectedCurrency.Transactions)
                        AddTransaction(lTransaction);
                    OnSelectedCurrencyChanged?.Invoke(sender, e);
                }
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void UpdateCurrencyView()
        {            
            SetCurrencyTooltip(SelectedCurrency);
            this.TotalCoins = FormatedAmount(SelectedCurrency.Balance, SelectedCurrency.Precision);
            this.UnconfirmedBalance = FormatedAmount(SelectedCurrency.UnconfirmedBalance, SelectedCurrency.Precision);
            //this.Confirmed = FormatedAmount(SelectedCurrency.ConfirmedBalance, SelectedCurrency.Precision);
            this.CoinName = SelectedCurrency.Name;
            this.CoinStatus = SelectedCurrency.CurrentStatus.ToString();
            this.ReceiveAddress = SelectedCurrency.LastAddress;
#if !DEBUG
            SetTxSendAreaUsability(SelectedCurrency.CurrentStatus == CurrencyStatus.Active);
#endif
        }

        public void AddTransaction(Transaction aTransaction)
        {
            var lListViewItem = new ListViewItem();
            lListViewItem.Text = aTransaction.TxDate.ToString();
            lListViewItem.SubItems.Add(aTransaction.From);
            lListViewItem.SubItems.Add(aTransaction.ToAddress);
            lListViewItem.SubItems.Add(aTransaction.Amount < 0 ? FormatedAmount(Math.Abs(aTransaction.Amount), SelectedCurrency.Precision) : "-");
            lListViewItem.SubItems.Add(aTransaction.Amount > 0 ? FormatedAmount(aTransaction.Amount, SelectedCurrency.Precision) : "-");
            lListViewItem.SubItems.Add(aTransaction.Confirmed.ToString());
            lListViewItem.Tag = aTransaction;
            lListViewItem.ImageIndex = (int)aTransaction.TxType;
            TransactionView.Items.Add(lListViewItem);
        }

        public bool UpdateTransaction(Transaction aTransaction)
        {
            if (aTransaction.ParrentCurrency.Id != SelectedCurrencyId) return false;
            ListViewItem lListViewItem = null;
            foreach (var lObj in TransactionView.Items)
            {
                if ((lObj as ListViewItem).Tag == aTransaction)
                {
                    lListViewItem = lObj as ListViewItem;
                    break;
                }
            }
            if (lListViewItem != null)
            {
                lListViewItem.Text = aTransaction.TxDate.ToString();
                lListViewItem.SubItems[1].Text = aTransaction.From;
                lListViewItem.SubItems[2].Text = aTransaction.ToAddress;
                lListViewItem.SubItems[3].Text = (aTransaction.Amount < 0 ? FormatedAmount(Math.Abs(aTransaction.Amount), SelectedCurrency.Precision) : "-");
                lListViewItem.SubItems[4].Text = (aTransaction.Amount > 0 ? FormatedAmount(aTransaction.Amount, SelectedCurrency.Precision) : "-");
                lListViewItem.SubItems[5].Text = (aTransaction.Confirmed.ToString());
            }
            return lListViewItem != null;
        }

        private void SetCurrencyTooltip(Currency aSelectedCurrency)
        {
            var lStringBuilder = new System.Text.StringBuilder();
            lStringBuilder.AppendLine($"Selected currency: {aSelectedCurrency.Name}.");
            lStringBuilder.AppendLine($"Blockchain Height: {aSelectedCurrency.BlockHeight}.");
            lStringBuilder.AppendLine($"Minimum transaction confirmations: {aSelectedCurrency.MinConfirmations}.");
            lStringBuilder.AppendLine($"As of {aSelectedCurrency.StatusDetails.StatusTime} the status is {aSelectedCurrency.CurrentStatus}.");
            lStringBuilder.AppendLine(aSelectedCurrency.StatusDetails.StatusMessage);
                       
            coinTooltip.SetToolTip(picCoinImage, lStringBuilder.ToString());
        }

        public bool UpdateCurrency(long aCurrencyID)
        {
            if (FCurrencyLookup.TryGetValue(aCurrencyID, out Currency lCurrency))
            {
                lCurrency.UpdateBalance();
                var lCustomValues = new string[] { FormatedAmount(lCurrency.Balance, lCurrency.Precision), lCurrency.CurrentStatus.ToString() };
                CurrencyViewControl.UpdateCurrency(lCurrency.Id, lCurrency.Name, lCurrency.Ticker, lCustomValues);
                if (SelectedCurrency.Id == lCurrency.Id)
                    UpdateCurrencyView();
                foreach (var lTx in lCurrency.Transactions)
                    this.UpdateTransaction(lTx);
                return true;
            }
            return false;
        }

        public bool RemoveTransaction(Transaction aTransaction)
        {
            ListViewItem lListViewItem = null;
            foreach (var lObj in TransactionView.Items)
            {
                var lTx = (lObj as ListViewItem).Tag as Transaction;
                if (lTx != null && lTx.RecordId == aTransaction.RecordId)
                {
                    lListViewItem = lObj as ListViewItem;
                    break;
                }
            }
            if (lListViewItem != null)
            {
                SelectedCurrency.RemoveTransaction(aTransaction);
                TransactionView.Items.Remove(lListViewItem);
                //UpdateCurrency(SelectedCurrency);
            }
            return lListViewItem != null;
        }

        public void RefreshTransactions(long aCurrencyID = -1)
        {
            if (SelectedCurrency.Id == aCurrencyID || aCurrencyID < 0)
            {
                TransactionView.Items.Clear();
                foreach (var lTransaction in SelectedCurrency.Transactions)
                    AddTransaction(lTransaction);
                UpdateCurrency(SelectedCurrencyId);
            }
        }

        private void listTransactions_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            NotesBox = "";
            if (e.IsSelected)
            {
                SelectedTransaction = (Transaction)e.Item.Tag;
                NotesBox = $"Transaction ID : {SelectedTransaction.TxId} - To Address: {SelectedTransaction.ToAddress}{Environment.NewLine}" +
                           $"Tx Block Number: {SelectedTransaction.BlockNumber} - Confirmations: {SelectedTransaction.Confimations}{Environment.NewLine}" +
                           $"Transaction Fee: {FormatedAmount(SelectedTransaction.Fee, SelectedCurrency.Precision)}";
            }
            else
                SelectedTransaction = null;
        }

        private void MainWallet_Shown(object sender, EventArgs e)
        {
            if (!FShown)
            {
                ChangeFontUtil.ChangeDefaultFontFamily(this);
                FShown = true;
                connectToolStripMenuItem1_Click(this, e);
            }
        }

        private void MainWallet_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                OnWalletFormClosing?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "An error occured during logoff");
            }
        }

        private void DisconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aDlg = new AboutBox();
            aDlg.ShowDialog();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OnSettingsMenuClick?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void AddCurrencyBtn_Click(object sender, EventArgs e)
        {
            try
            {
                OnAddCurrencyBtnClick?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void QuickSendButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtBoxSendToAddress.Text))
            {
                this.StandardInfoMsgBox("Please add a valid address to continue.");
                return;
            }

            if (string.IsNullOrWhiteSpace(QuickAmountTextBox.Text))
            {
                this.StandardInfoMsgBox("Please add a valid ammount to continue.");
                return;
            }
            try
            {
                OnTransactionSend(sender, e);
            }
            catch (SubscriptionOverException ex)
            {
                this.StandardErrorMsgBox("Subscription period over!", ex.Message);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void QuickAmmountTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"[^0-9\s]");

            if (regex.IsMatch(e.KeyChar.ToString()) && (e.KeyChar != '.') && e.KeyChar != 8)
            {
                e.Handled = true;
            }

            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }

            if ((sender as TextBox).Text == "0" && e.KeyChar != '.')
            {
                (sender as TextBox).Text = e.KeyChar.ToString();
                (sender as TextBox).SelectionStart = (sender as TextBox).Text.Length;
                (sender as TextBox).SelectionLength = 0;
                e.Handled = true;
            }
        }

        public bool Connected { get; set; }

        private void connectToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                OnConnect(this, e);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
                if (!Connected)
                    Close();
            }
        }

        private void PandoraClientMainWindow_Load(object sender, EventArgs e)
        {
            try
            {
                OnFormLoad?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void backupWalletKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OnBackupClick?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private class PandoraListViewItemSorter : System.Collections.IComparer
        {
            private int col;
            private SortOrder order;

            public PandoraListViewItemSorter()
            {
                col = 0;
                order = SortOrder.Ascending;
            }

            public PandoraListViewItemSorter(int column, SortOrder order)
            {
                col = column;
                this.order = order;
            }
             
            public int Compare(object x, object y)
            {
                int returnVal;
                // Determine whether the type being compared is a date type.
                try
                {
                    // Parse the two objects passed as a parameter as a DateTime.
                    System.DateTime firstDate =
                            DateTime.Parse(((ListViewItem)x).Text);
                    System.DateTime secondDate =
                            DateTime.Parse(((ListViewItem)y).Text);
                    // Compare the two dates.
                    returnVal = DateTime.Compare(firstDate, secondDate);
                }
                // If neither compared object has a valid date format, compare
                // as a string.
                catch
                {
                    // Compare the two items as a string.
                    returnVal = string.Compare(((ListViewItem)x).Text,
                                ((ListViewItem)y).Text);
                }
                // Determine whether the sort order is descending.
                if (order == SortOrder.Descending)
                {
                    // Invert the value returned by String.Compare.
                    returnVal *= -1;
                }

                return returnVal;
            }
        }

        private void txtBoxSearchCoin_TextChanged(object sender, EventArgs e)
        {
            try
            {
                var lSearchText = txtBoxSearchCoin.Text;
                if (string.IsNullOrEmpty(lSearchText))
                {
                    lstViewCurrencies.ClearFilter();
                    iconSearch.Visible = true;
                }
                else
                {
                    lstViewCurrencies.ApplyFilter(lSearchText);
                    iconSearch.Visible = false;
                }
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private enum CommandKeyCodes
        {
            Copy = 22, Cut = 24, Undo = 26, Redo = 25, Space = 32, Backspace = 8
        }

        private void TxtBoxSendToAddress_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"[^a-zA-Z0-9\s]");
                e.Handled = regex.IsMatch(e.KeyChar.ToString());
                e.Handled &= (e.KeyChar != (int) CommandKeyCodes.Backspace);
                e.Handled &= (e.KeyChar != (int) CommandKeyCodes.Copy);
                e.Handled &= (e.KeyChar != (int) CommandKeyCodes.Cut);
                e.Handled &= (e.KeyChar != (int) CommandKeyCodes.Undo);
                e.Handled &= (e.KeyChar != (int) CommandKeyCodes.Redo);
                e.Handled |= (e.KeyChar == (int) CommandKeyCodes.Space);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void TxtBoxSendToAddress_TextChanged(object sender, EventArgs e)
        {
            try
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"[^a-zA-Z0-9\s]");
                string sanitizedValue = regex.Replace(TxtBoxSendToAddress.Text, string.Empty);
                TxtBoxSendToAddress.Text = sanitizedValue;
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private string FPreviousSendAmount = "0";

        private void QuickAmountTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace((sender as TextBox).Text))
                {
                    (sender as TextBox).Text = "0";
                }
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^([0-9]*[.]{0,1})([0-9]*)$");
                if (!regex.IsMatch((sender as TextBox).Text))
                {
                    (sender as TextBox).Text = FPreviousSendAmount;
                }
                else
                {
                    FPreviousSendAmount = (sender as TextBox).Text;
                }
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        //-------------------------------------------------------------------------Exchanges and Trade History-------------------------------------------------------------------------

        public string SelectedExchange { get => cbExchange.GetItemText(cbExchange.SelectedItem) == "--Select--" ? string.Empty : cbExchange.GetItemText(cbExchange.SelectedItem); set => cbExchange.SelectedIndex = (Convert.ToInt32(value)); }

        public ListView.ListViewItemCollection ExchangeCoinItems => lstExchangeMarket.Items;

        public string LabelCoinExchange { set => lblExchange.Text = string.Format("Exchange {0} On", value); }

        public string LabelCoinQuantity { get => lblQuantity.Text; set => lblQuantity.Text = value; }

        public string LabelTotalCoinReceived { get => lblTotalReceived.Text; set => lblTotalReceived.Text = value; }

        public string LabelPriceInCoin { set => lblEstimatePriceCoin.Text = string.Format("Estimated Current price in ({0}):", value); }

        public string LabelEstimatePrice { get => lblEstimatePrice.Text; set => lblEstimatePrice.Text = value; }

        public decimal ExchangeTargetPrice { get => string.IsNullOrWhiteSpace(txtPrice.Text) ? 0 : Convert.ToDecimal(txtPrice.Text); set => txtPrice.Text = value.ToString(); }
        public decimal ExchangeStopPrice { get => string.IsNullOrWhiteSpace(txtStopPrice.Text) ? 0 : Convert.ToDecimal(txtStopPrice.Text); set => txtStopPrice.Text = value.ToString(); }
        public decimal ExchangeQuantity { get => string.IsNullOrWhiteSpace(txtQuantity.Text) ? 0 : Convert.ToDecimal(txtQuantity.Text); set => txtQuantity.Text = value.ToString(); }
        public decimal ExchangeTotalReceived { get => string.IsNullOrWhiteSpace(txtTotal.Text) ? 0 : Convert.ToDecimal(txtTotal.Text); set => txtTotal.Text = value.ToString(); }
        public string ExchangeTransactionName { get => txtTransactionName.Text; set => txtTransactionName.Text = value; }

        public bool ExchangeTargetPriceEnabled { get => txtPrice.Enabled; set => txtPrice.Enabled = value; }
        public bool ExchangeStoptPriceEnabled { get => txtStopPrice.Enabled; set => txtStopPrice.Enabled = value; }
        public bool ExchangeQuantityEnabled { get => txtQuantity.Enabled; set => txtQuantity.Enabled = value; }
        public bool ExchangeTotalReceivedEnabled { get => txtTotal.Enabled; set => txtTotal.Enabled = value; }
        public bool ExchangeTransactionNameEnabled { get => txtTransactionName.Enabled; set => txtTransactionName.Enabled = value; }
        public bool ExchangeButtonEnabled { get => btnExchange.Enabled; set => btnExchange.Enabled = value; }

        public event EventHandler OnStopPriceTextChanged;

        public void ClearExchangeList()
        {
            cbExchange.Items.Clear();
        }

        public void AddExchanges(IEnumerable<string> aListOfExchanges)
        {
            foreach (string it in aListOfExchanges)
            {
                if (!cbExchange.Items.Contains(it))
                {
                    cbExchange.Items.Add(it);
                }
            }
        }

        /// <summary>
        /// Add or replace a coin item inside coins to exchange listview
        /// </summary>
        /// <param name="aCurrencyId">Currency id of item to update</param>
        /// <param name="aCurrencyName">Currency name of item to update</param>
        /// <param name="aCurrencySymbol">Currency ticker of item to update</param>
        /// <param name="aPrice">Market price in BTC for currency item to update</param>
        public void AddCoinExchangeTo(long aCurrencyId, string aCurrencyName, string aCurrencySymbol, decimal aPrice)
        {
            string lCurrencyIDString = aCurrencyId.ToString();
            ListViewItem lNewItemToAdd = new ListViewItem();
            lNewItemToAdd.Name = lCurrencyIDString;
            lNewItemToAdd.Text = aCurrencyName;
            lNewItemToAdd.SubItems.Add(aCurrencySymbol);
            lNewItemToAdd.SubItems.Add(aPrice.ToString());
            lNewItemToAdd.ImageKey = aCurrencyId.ToString();
            lstExchangeMarket.BeginUpdate();
            bool isSelected = SelectedExchangeMarket != null ? SelectedExchangeMarket.Name == lCurrencyIDString : false;
            var lIndex = lstExchangeMarket.Items.IndexOfKey(lCurrencyIDString);
            if (lIndex >= 0)
                lstExchangeMarket.Items[lIndex] = lNewItemToAdd;
            else
                lstExchangeMarket.Items.Add(lNewItemToAdd);
            lNewItemToAdd.Selected = lNewItemToAdd.Focused = isSelected;
            lstExchangeMarket.EndUpdate();
        }

        public void ClearListExchangeTo()
        {
            lstExchangeMarket.Items.Clear();
        }

        public ListViewItem SelectedExchangeMarket
        {
            get
            {
                if (lstExchangeMarket.Items.Count > 0 && lstExchangeMarket.SelectedItems.Count > 0)
                {
                    return lstExchangeMarket.SelectedItems[0];
                }
                else
                {
                    return null;
                }
            }
        }

        private Dictionary<int, ExchangeOrderViewModel> FExchangeOrders = new Dictionary<int, ExchangeOrderViewModel>();
        private Dictionary<long, List<int>> FExchangeOrdersByCurrency = new Dictionary<long, List<int>>();
        private Dictionary<int, ListViewItem> FExchangeOrderLstViewItems = new Dictionary<int, ListViewItem>();

        public void AddOrUpdateOrder(ExchangeOrderViewModel aOrder, long aCurrencyID, bool aUpdateListView = true)
        {
            if (FExchangeOrders.TryGetValue(aOrder.ID, out ExchangeOrderViewModel lOrderViewModel))
            {
                if (!object.ReferenceEquals(aOrder, lOrderViewModel))
                    lOrderViewModel.CopyFrom(aOrder);
                AddTooOrderLstViewItemCache(aOrder, aCurrencyID);
                if (aUpdateListView)
                    UpdateOrderLstView(aOrder.ID, aCurrencyID);
            }
            else
            {
                FExchangeOrders.Add(aOrder.ID, aOrder);
                if (!FExchangeOrdersByCurrency.ContainsKey(aCurrencyID))
                    FExchangeOrdersByCurrency.Add(aCurrencyID, new List<int>());
                FExchangeOrdersByCurrency[aCurrencyID].Add(aOrder.ID);
                var lOrderLstViewItem = ConstructOrderHistoryLstViewItem(aOrder);
                FExchangeOrderLstViewItems.Add(aOrder.ID, lOrderLstViewItem);
                if ((SelectedCurrencyId == aCurrencyID || AllOrderHistoryChecked) && aUpdateListView)
                    lstOrderHistory.Items.Add(lOrderLstViewItem);
            }
        }

        public ExchangeOrderViewModel GetOrderViewModel(int aOrderId)
        {
            if (FExchangeOrders.TryGetValue(aOrderId, out ExchangeOrderViewModel lOrder))
                return lOrder;
            return null;
        }

        private ListViewItem ConstructOrderHistoryLstViewItem(ExchangeOrderViewModel aOrder)
        {
            ListViewItem lResult = new ListViewItem()
            {
                Tag = aOrder,
                Text = aOrder.Name,
                Name = aOrder.ID.ToString()
            };
            lResult.SubItems.Add(aOrder.Sold);
            lResult.SubItems.Add(aOrder.Received);
            lResult.SubItems.Add(aOrder.Price);
            lResult.SubItems.Add(aOrder.Stop);
            lResult.SubItems.Add(aOrder.Exchange);
            lResult.SubItems.Add(aOrder.Time);
            lResult.SubItems.Add(aOrder.Status);
            return lResult;
        }

        private int FLastStatusOrderID = -1;

        public void SetExchangeLastOrder(int aOrderID)
        {
            if (FLastStatusOrderID >= 0)
                FExchangeOrders[FLastStatusOrderID].OnNewLogAdded -= LastOrder_OnNewLogAdded;
            StatusControlExchange.ClearStatusList();
            if (FExchangeOrders.TryGetValue(aOrderID, out ExchangeOrderViewModel aOrder))
            {
                foreach (var lOrderMessage in aOrder.GetLogs())
                    StatusControlExchange.AddStatus(lOrderMessage.ID, lOrderMessage.Time, lOrderMessage.Message);
                aOrder.OnNewLogAdded += LastOrder_OnNewLogAdded;
            }
        }

        private void LastOrder_OnNewLogAdded(ExchangeOrderLogViewModel aLog)
        {
            StatusControlExchange.AddStatus(aLog.ID, aLog.Time, aLog.Message);
        }

        private void AddTooOrderLstViewItemCache(ExchangeOrderViewModel aOrder, long aCurrencyID)
        {
            var lNewItem = ConstructOrderHistoryLstViewItem(aOrder);
            FExchangeOrderLstViewItems[aOrder.ID] = lNewItem;
        }

        private void UpdateOrderLstView(int aOrderID, long aCurrencyID)
        {
            var lItem = FExchangeOrderLstViewItems[aOrderID];
            if (aCurrencyID == SelectedCurrencyId || AllOrderHistoryChecked)
                UpdateOrderLstViewWith(aOrderID, lItem);
        }

        public void LoadCurrencyExchangeOrders(long aCurrencyID)
        {
            ClearOrderHistory();
            if (FExchangeOrdersByCurrency.TryGetValue(aCurrencyID, out List<int> lOrdersID))
            {
                foreach (var lOrderID in lOrdersID)
                    UpdateOrderLstViewWith(lOrderID, FExchangeOrderLstViewItems[lOrderID], false);
            }
            statscntrlTradeHistory.ClearStatusList();
        }

        public void LoadAllCurrencyExchangeOrders()
        {
            ClearOrderHistory();
            foreach (var lOrder in FExchangeOrderLstViewItems)
                UpdateOrderLstViewWith(lOrder.Key, lOrder.Value, false);
            statscntrlTradeHistory.ClearStatusList();
        }

        private void UpdateOrderLstViewWith(int aID, ListViewItem aItem, bool lRememberSelected = true)
        {
            var lIndexLstViewModel = lstOrderHistory.Items.IndexOfKey(aID.ToString());
            ListViewItem lSelectedOrder = null;
            if (FOrderHistorySelected != null && lRememberSelected) lSelectedOrder = FOrderHistorySelected;
            lstOrderHistory.BeginUpdate();
            if (lIndexLstViewModel >= 0)
                lstOrderHistory.Items.RemoveAt(lIndexLstViewModel);
            lstOrderHistory.Items.Add(aItem);
            lstOrderHistory.EndUpdate();
            if (lSelectedOrder != null)
            {
                aItem.Selected = (lSelectedOrder.Tag.ToString() == aItem.Tag.ToString());
                aItem.Focused = (lSelectedOrder.Tag.ToString() == aItem.Tag.ToString());
            }
        }

        public void ExchangeSelectCurrency(string aName)
        {
            try
            {
                if (aName == null)
                {
                    return;
                }
                int lIndex = -1;
                if (lstExchangeMarket.Items.Count > 0)
                {
                    foreach (object lIt in lstExchangeMarket.Items)
                    {
                        ListViewItem lListViewItem = (ListViewItem)lIt;
                        if (aName.Contains(lListViewItem.Name))
                        {
                            lIndex = lListViewItem.Index;
                            break;
                        }
                    }

                    if (lIndex >= 0)
                    {
                        lstExchangeMarket.Items[lIndex].Selected = true;
                        lstExchangeMarket.Items[lIndex].Focused = true;
                        lstExchangeMarket.Select();
                        lstExchangeMarket.EnsureVisible(lIndex);
                    }
                }
            }
            catch
            {
            }
        }

        private void lstCoinAvailable_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                OnExchangeSelectedCurrencyChanged?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        public bool AllOrderHistoryChecked => chckOrderHistory.Checked;

        public void ClearOrderHistory()
        {
            lstOrderHistory.Items.Clear();
        }

        private ListViewItem FOrderHistorySelected;

        public event Action<int> OnCancelBtnClick;

        public ListViewItem SelectedOrderHistory
        {
            get
            {
                if (lstOrderHistory.Items.Count > 0 && lstOrderHistory.SelectedItems.Count > 0)
                {
                    FOrderHistorySelected = lstOrderHistory.SelectedItems[0];
                    return lstOrderHistory.SelectedItems[0];
                }
                else
                {
                    return null;
                }
            }
        }

        public Currency SelectedCurrency { get; private set; }
        public bool ExchangeChangeKeysEnabled { get => btnExchangeKeys.Enabled; internal set => btnExchangeKeys.Enabled = value; }

        private int FPreviousOrderSelected = -1;

        private void lstOrderHistory_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var lCurrentSelected = SelectedOrderHistory?.Name;
                if (lCurrentSelected != null)
                {
                    int lID = Convert.ToInt32(lCurrentSelected);
                    if (FPreviousOrderSelected == lID) return;
                    if (FPreviousOrderSelected >= 0)
                    {
                        FExchangeOrders[FPreviousOrderSelected].OnNewLogAdded -= ExchangeOrder_OnNewLogAdded;
                        statscntrlTradeHistory.ClearStatusList();
                    }
                    var lOrderView = FExchangeOrders[lID];
                    foreach (var lLog in lOrderView.GetLogs())
                    {
                        statscntrlTradeHistory.AddStatus(lLog.ID, lLog.Time, lLog.Message);
                    }
                    lOrderView.OnNewLogAdded += ExchangeOrder_OnNewLogAdded;
                }
                else
                {
                    statscntrlTradeHistory.ClearStatusList();
                }
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void ExchangeOrder_OnNewLogAdded(ExchangeOrderLogViewModel aLog)
        {
            statscntrlTradeHistory.AddStatus(aLog.ID, aLog.Time, aLog.Message);
        }

        private void cbExchanges_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                OnExchangeSelectedCurrencyChanged?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        public event EventHandler OnExchangeSelectionChanged;

        private void cbExchange_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                OnExchangeSelectionChanged?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void txtQuantity_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(QuickAmountTextBox.Text))
            {
                (sender as TextBox).Text = "0";
            }
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        public event EventHandler OnExchangeBtnClick;

        private void btnExchange_Click(object sender, EventArgs e)
        {
            try
            {
                OnExchangeBtnClick?.Invoke(sender, e);
            }
            catch (ClientExceptions.InvalidOperationException ex)
            {
                this.StandardErrorMsgBox("Invalid Operation", ex.Message);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void txtTransactionName_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OnTransactionNameChanged?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void lblEstimatePrice_Click(object sender, EventArgs e)
        {
            try
            {
                OnLabelEstimatePriceClick?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void txtPrice_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OnPriceChanged?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void chckOrderHistory_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                OnCheckAllOrderHistory?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void txtQuantity_Leave(object sender, EventArgs e)
        {
            try
            {
                OnTxtQuantityLeave?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void txtTotal_Leave(object sender, EventArgs e)
        {
            try
            {
                OnTxtTotalLeave?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        public event EventHandler OnChangePassword;

        private void changePasswordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OnChangePassword?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void txtStopPrice_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OnStopPriceTextChanged?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void OnSendAllMenuClickEvent(object sender, EventArgs e)
        {
            try
            {
                OnSendAllMenuClick?.Invoke(sender, e);
            }
            catch (SubscriptionOverException ex)
            {
                this.StandardErrorMsgBox("Subscription period over!", ex.Message);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void MenuItemCancel_Click(object sender, EventArgs e)
        {
            try
            {
                var lSelectedOrder = SelectedOrderHistory;
                if (lSelectedOrder != null)
                    OnCancelBtnClick?.Invoke(Convert.ToInt32(lSelectedOrder.Name));
            }
            catch
            (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
            }
        }

        private void LstOrderHistory_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (lstOrderHistory.FocusedItem.Bounds.Contains(e.Location))
                    ctxMenuOrderMenu.Show(Cursor.Position);
                var lOrderID = Convert.ToInt32(SelectedOrderHistory.Name);
                var lStatus = FExchangeOrders[lOrderID].Status;
                ctxMenuOrderMenu.Items[0].Enabled = (lStatus != "Interrupted" && lStatus != "Withdrawed");
            }
        }

        public class Accounts
        {
            public string Address { get; set; }
            public string Name { get; set; }
        }

        public enum TransactionType { Credit = 0, Debit = 1, Both = 2, Unknown = 3 }

        public class Transaction
        {
            public Transaction()
            {
            }

            public long RecordId { get; set; }
            public DateTime TxDate { get; set; }
            public string From { get; set; }
            public string ToAddress { get; set; }
            public decimal Amount { get; set; }
            public long BlockNumber { get; set; }
            public string TxId { get; set; }
            public decimal Fee { get; internal set; }
            public TransactionType TxType { get; set; }
            public Currency ParrentCurrency { get; set; }
            // fixed off by one bug.
            public long Confimations { get => ParrentCurrency == null ? 0 : BlockNumber == 0 ? 0 : ParrentCurrency.BlockHeight + 1 - BlockNumber; }
            public bool Confirmed { get => ParrentCurrency == null ? false : Confimations >= ParrentCurrency.MinConfirmations; }

            public Transaction CopyFrom(Transaction aSource)
            {
                RecordId = aSource.RecordId;
                TxDate = aSource.TxDate;
                From = aSource.From;
                ToAddress = aSource.ToAddress;
                Amount = aSource.Amount;
                BlockNumber = aSource.BlockNumber;
                TxId = aSource.TxId;
                Fee = aSource.Fee;
                TxType = aSource.TxType;
                ParrentCurrency = aSource.ParrentCurrency;
                return this;
            }
        }

        public class Currency : CurrencyItem
        {
            public Currency(CurrencyItem aCurrencyItem)
            {
                StatusDetails = new StatusDetailsObject();
                aCurrencyItem.CopyTo(this);
            }

            private Dictionary<long, Transaction> FTransactions = new Dictionary<long, Transaction>();

            //public Currency(CurrencyItem aCurrencyItem,long aBlockHeight, decimal aBalance, decimal aUnconfirmedBalance, decimal aDefaultCurrencyPricePerCoin, Accounts[] aAddresses, Transaction[] aTransactions)
            //{
            //    BlockHeight = aBlockHeight;
            //    aCurrencyItem.CopyTo(this);
            //    Addresses = aAddresses;
            //    foreach (var lTx in aTransactions)
            //        FTransactions.Add(lTx.RecordId, lTx);
            //    Balance = aBalance;
            //    UnconfirmedBalance = aUnconfirmedBalance;
            //    foreach (var lTransaction in aTransactions)
            //        lTransaction.ParrentCurrency = this;
            //}

            public decimal UnconfirmedBalance { get; set; }
            public decimal ConfirmedBalance { get; set; }
            public decimal Balance { get { return UnconfirmedBalance + ConfirmedBalance; } }
            public decimal DefaultCurrencyPricePerCoin { get; set; }
            public long BlockHeight { get; set; }
            public Accounts[] Addresses { get; set; }
            public string LastAddress { get { return Addresses[Addresses.Length - 1].Address; } }
            public Transaction[] Transactions { get => (new List<Transaction>(FTransactions.Values)).ToArray(); }
            public IStatusDetails StatusDetails { get; private set; }

            public interface IStatusDetails
            {
                string StatusMessage { get; set; }
                DateTime StatusTime { get; set; }
            }

            private class StatusDetailsObject : IStatusDetails
            {
                public string StatusMessage { get; set; }
                public DateTime StatusTime { get; set; }
            }

            public void AddTransaction(Transaction aTransaction)
            {
                FTransactions.Add(aTransaction.RecordId, aTransaction);
                aTransaction.ParrentCurrency = this;
                if (!aTransaction.Confirmed)
                    UnconfirmedBalance += aTransaction.Amount;
                else
                    ConfirmedBalance += aTransaction.Amount;
            }

            public void UpdateTransaction(Transaction aTransaction)
            {
                aTransaction.ParrentCurrency = this;
                if (FTransactions.ContainsKey(aTransaction.RecordId))
                    FTransactions[aTransaction.RecordId].CopyFrom(aTransaction);
            }

            public bool RemoveTransaction(Transaction aTransaction)
            {
                var lResult = FTransactions.Remove(aTransaction.RecordId);
                if (lResult)
                {
                    if (!aTransaction.Confirmed)
                        UnconfirmedBalance -= aTransaction.Amount;
                    else
                        ConfirmedBalance -= aTransaction.Amount;
                    return lResult;
                }
                return lResult;
            }

            public void ClearTransactions()
            {
                FTransactions.Clear();
            }

            public void UpdateBalance()
            {
                UnconfirmedBalance = 0;
                ConfirmedBalance = 0;
                foreach (var lTx in Transactions)
                {
                    if (!lTx.Confirmed)
                        UnconfirmedBalance += lTx.Amount;
                    else
                        ConfirmedBalance += lTx.Amount;
                }
            }

            public Transaction FindTransaction(long aRecordId)
            {
                FTransactions.TryGetValue(aRecordId, out Transaction lResult);
                return lResult;
            }

            public bool RemoveTransaction(long aRecordId)
            {
                return FTransactions.Remove(aRecordId);
            }
        }

        public class ExchangeOrderViewModel
        {
            public class ExchangeOrderViewModelContextData
            {
                public Exchange.PandoraExchanger.ExchangeMarket Market { get; set; }
                public decimal ExchangeFee { get; set; }
                public decimal TradeComission { get; set; }
                public ushort Precision { get; set; }

                public AppMainForm MainForm { get; set; }

                public decimal GetReceivingQuantity(decimal aSentQuantity, decimal aOrderRate)
                {
                    decimal lRate = Market.IsSell ? aOrderRate : 1 / aOrderRate;
                    decimal lRawAmount = aSentQuantity * lRate;
                    decimal lQuantity = Math.Round(lRawAmount - (lRawAmount * TradeComission), Precision);
                    return lQuantity;
                }
            }

            public ExchangeOrderViewModel(Exchange.MarketOrder aOrder, ExchangeOrderViewModelContextData aContextData)
            {
                ID = aOrder.InternalID;
                FOrderLogs = new Dictionary<int, ExchangeOrderLogViewModel>();
                ContextData = aContextData;
                Name = aOrder.Name;
                Sold = aOrder.SentQuantity.ToString();
                Received = aContextData.GetReceivingQuantity(aOrder.SentQuantity, aOrder.Rate).ToString();
                Price = aOrder.Rate.ToString();
                Stop = aOrder.StopPrice.ToString();
                Exchange = aOrder.Market;
                Time = aOrder.OpenTime.ToLocalTime().ToString();
                Status = aOrder.Status.ToString();
            }

            public void CopyFrom(ExchangeOrderViewModel aOrderViewModel)
            {
                ID = aOrderViewModel.ID;
                ContextData = aOrderViewModel.ContextData;
                Name = aOrderViewModel.Name;
                Sold = aOrderViewModel.Sold;
                Received = aOrderViewModel.Received;
                Price = aOrderViewModel.Price;
                Stop = aOrderViewModel.Stop;
                Exchange = aOrderViewModel.Exchange;
                Time = aOrderViewModel.Time;
                Status = aOrderViewModel.Status;
            }

            public ExchangeOrderViewModelContextData ContextData { get; private set; }

            public string Name { get; set; }
            public string Sold { get; set; }
            public string Received { get; set; }
            public string Price { get; set; }
            public string Stop { get; set; }
            public string Exchange { get; set; }
            public string Time { get; set; }
            public string Status { get; set; }
            public int ID { get; private set; }
            private Dictionary<int, ExchangeOrderLogViewModel> FOrderLogs;

            public event Action<ExchangeOrderLogViewModel> OnNewLogAdded;

            public IEnumerable<ExchangeOrderLogViewModel> GetLogs()
            {
                return FOrderLogs.Values;
            }

            public void AddLog(IEnumerable<ExchangeOrderLogViewModel> aLogs)
            {
                foreach (var lLog in aLogs)
                    AddLog(lLog);
            }

            public void AddLog(ExchangeOrderLogViewModel aLog)
            {
                if (!FOrderLogs.ContainsKey(aLog.ID))
                {
                    FOrderLogs.Add(aLog.ID, aLog);
                    if (OnNewLogAdded != null)
                        ContextData.MainForm.BeginInvoke(OnNewLogAdded, aLog);
                }
            }
        }

        public class ExchangeOrderLogViewModel
        {
            public int ID { get; set; }
            public string Message { get; set; }
            public string Time { get; set; }
        }

        public bool AskUserDialog(string aTitle, string aMessage)
        {
            return this.StandardAskMsgBox(aTitle, aMessage);
        }

        private void btnExchangeKeys_Click(object sender, EventArgs e)
        {
            try
            {
                OnChangeExchangeKeysBtnClick.Invoke(sender, e);
            }
            catch(Exchange.PandoraExchangeExceptions.InvalidExchangeCredentials ex)
            {
                this.StandardInfoMsgBox("Unable to change exchange credentials", ex.Message);
            }
            catch(Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void pandorasWalletGuideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("Pandora'sWalletGuide 16-01-2020.pdf");
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }
    }
}