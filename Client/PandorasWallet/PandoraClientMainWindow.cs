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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet
{
    public partial class PandoraClientMainWindow : Form
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
        private string FCoinStatus;

        private Dictionary<long, ListViewItem> FCurrencyLookup = new Dictionary<long, ListViewItem>();

        private List<ListViewItem> FListViewBackup = new List<ListViewItem>();

        public event EventHandler OnConnect;

        public event EventHandler OnAddCurrencyBtnClick;

        public event EventHandler OnDisconnect;

        public event FormClosingEventHandler OnWalletFormClosing;

        public event EventHandler OnTransactionSend;

        public event EventHandler OnExhangeCurrencySelectionChanged;

        public event EventHandler OnFormLoad;

        public event EventHandler OnSendAllMenuClick;

        public event EventHandler OnTransactionViewSelectionChanged;

        public event EventHandler OnSearchBoxTextChanged;

        public event EventHandler OnBackupClick;

        public event EventHandler OnTransactionNameChanged;

        //public event EventHandler OnTotalReceivedChanged;

        public event EventHandler OnLabelEstimatePriceClick;

        public event EventHandler OnPriceChanged;

        public event EventHandler OnCheckAllOrderHistory;

        public event EventHandler OnSettingsMenuClick;

        public event EventHandler OnTxtQuantityLeave;

        public event EventHandler OnTxtTotalLeave;

        public PandoraClientMainWindow()
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

            lstCoinAvailable.LargeImageList = lstViewCurrencies.ImageList;
            lstCoinAvailable.SmallImageList = lstViewCurrencies.ImageList;
        }

        private void OnSendAllMenuClickEvent(object sender, EventArgs e)
        {
            try
            {
                OnSendAllMenuClick?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
            }
        }

        public void SetUserStatus(UserStatuses aStatus, string aEmail = null, string aUsername = null)
        {
            if (aUsername != null)
            {
                toolStripStatusUsername.Text = aUsername;
            }
            else
            {
                toolStripStatusUsername.Text = " - ";
            }

            if (aEmail != null)
            {
                toolStripStatusEmail.Text = aEmail;
            }
            else
            {
                toolStripStatusEmail.Text = " - ";
            }

            toolStripConnectionStatus.Text = aStatus.GetEnumDescription();
        }

        public CurrencyView CurrencyViewControl => lstViewCurrencies;

        public StatusControl StatusControlExchange => statsctrlExchage;
        public StatusControl StatusControlOrderHistory => statscntrlTradeHistory;

        public string FormName { get => Text; set => Text = value; }

        public string BalanceToolTip { set => toolTipBalance.SetToolTip(pictureBoxBalanceInfo, value); }

        public string ReceiveAddress { get => txtBoxReceiveAddress.Text; set => txtBoxReceiveAddress.Text = value; }

        public string ToSendAddress { get => TxtBoxSendToAddress.Text; set => TxtBoxSendToAddress.Text = value; }

        public decimal ToSendAmount { get => Convert.ToDecimal(QuickAmountTextBox.Text); set => QuickAmountTextBox.Text = value.ToString(); }

        public bool CoinImageVisibility { get => picCoinImage.Visible; set => picCoinImage.Visible = value; }

        public string UnconfirmedBalance { get => lblUnconfirmed.Text; set => lblUnconfirmed.Text = value; }

        public Color UnconfirmedBalanceColor { get => lblUnconfirmed.ForeColor; set => lblUnconfirmed.ForeColor = lblNameUnconfirmed.ForeColor = value; }

        public Color ConfirmedBalanceColor { get => lblConfirmed.ForeColor; set => lblConfirmed.ForeColor = lblNameConfirmed.ForeColor = value; }

        public string Confirmed { get => lblConfirmed.Text; set => lblConfirmed.Text = value; }

        public bool SearchByTicker { get => checkIsTicker.Checked; set => checkIsTicker.Checked = value; }

        public string SearchBar { get => txtBoxSearchCoin.Text; set => txtBoxSearchCoin.Text = value; }

        public void SetTxSendAreaUsability(bool aEnabled)
        {
            QuickSendButton.Enabled = aEnabled;
            TxtBoxSendToAddress.Enabled = aEnabled;
            QuickAmountTextBox.Enabled = aEnabled;
        }

        public void AddOrUpdateCurrencyToListView(CurrencyItem aCoin)
        {
            uint lId = Convert.ToUInt32(aCoin.Id);
        }

        public long SelectedCurrencyId { get => lstViewCurrencies.SelectedCurrencyId; set => lstViewCurrencies.SelectedCurrencyId = value; }

        public string TotalCoins { get => lblTotal.Text; set => lblTotal.Text = value; }

        public string CoinName { get => lblCoinName.Text; set => lblCoinName.Text = value; }

        public string NotesBox { get => txtNotesBox.Text; set => txtNotesBox.Text = value; }

        public bool CheckAllOrderHistoryEnabled { set => chckOrderHistory.Enabled = value; }

        public Image CoinImage
        {
            get => picCoinImage.Image;
            set
            {
                picCoinImage.Image = value;
                picCoinImage.Visible = true;
            }
        }

        public string CoinStatus
        {
            get => FCoinStatus;
            set
            {
                FCoinStatus = value;
                lblStatus.Text = FCoinStatus;
            }
        }

        public ListView TransactionView => listTransactions;

        private void MainWallet_Shown(object sender, EventArgs e)
        {
            if (!FShown)
            {
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
            try
            {
                OnDisconnect?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
            }
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
                this.StandardErrorMsgBox(ex.Message);
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
                this.StandardErrorMsgBox(ex.Message);
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
                OnTransactionSend?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
            }
        }

        private void QuickAmmountTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
            /*
            if (string.IsNullOrWhiteSpace(QuickAmountTextBox.Text))
            {
                QuickAmountTextBox.Text = "0";
            }
            */
        }

        private void listTransactions_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            try
            {
                OnTransactionViewSelectionChanged?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
            }
        }

        private void connectToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                OnConnect?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
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
                this.StandardErrorMsgBox(ex.Message);
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
                this.StandardErrorMsgBox(ex.Message);
            }
        }

        private void lstViewCurrencies_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                OnExhangeCurrencySelectionChanged?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
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
                            DateTime.Parse(((ListViewItem)x).SubItems[col].Text);
                    System.DateTime secondDate =
                            DateTime.Parse(((ListViewItem)y).SubItems[col].Text);
                    // Compare the two dates.
                    returnVal = DateTime.Compare(firstDate, secondDate);
                }
                // If neither compared object has a valid date format, compare
                // as a string.
                catch
                {
                    // Compare the two items as a string.
                    returnVal = string.Compare(((ListViewItem)x).SubItems[col].Text,
                                ((ListViewItem)y).SubItems[col].Text);
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
                OnSearchBoxTextChanged?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
            }
        }

        //-------------------------------------------------------------------------Exchanges and Trade History-------------------------------------------------------------------------

        public string SelectedExchange { get => cbExchange.GetItemText(cbExchange.SelectedItem) == "--Select--" ? string.Empty : cbExchange.GetItemText(cbExchange.SelectedItem); set => cbExchange.SelectedIndex = (Convert.ToInt32(value)); }

        public ListView.ListViewItemCollection ExchangeCoinItems => lstCoinAvailable.Items;

        public string LabelCoinExchange { set => lblExchange.Text = string.Format("Exchange {0} On", value); }

        //public string LabelCoinExchange { set { lblExchange.Text = string.Format("Exchange {0} On", lblCoinName.Text); } }
        public string LabelCoinQuantity { get => lblQuantity.Text; set => lblQuantity.Text = value; }

        public string LabelTotalCoinReceived { get => lblTotalReceived.Text; set => lblTotalReceived.Text = value; }

        //public string LabelTotalCoinReceived { set { string.Format("{0} ({1})", lSelectedCurrencyToExchange.SubItems[0], lSelectedCurrencyToExchange.SubItems[1]); } }
        public string LabelPriceInCoin { set => lblEstimatePriceCoin.Text = string.Format("Estmated Current price in ({0}):", value); }

        public string LabelEstimatePrice { get => lblEstimatePrice.Text; set => lblEstimatePrice.Text = value; }

        public decimal ExchangeTargetPrice { get => string.IsNullOrWhiteSpace(txtPrice.Text) ? 0 : Convert.ToDecimal(txtPrice.Text); set => txtPrice.Text = value.ToString(); }
        public decimal ExchangeQuantity { get => string.IsNullOrWhiteSpace(txtQuantity.Text) ? 0 : Convert.ToDecimal(txtQuantity.Text); set => txtQuantity.Text = value.ToString(); }
        public decimal ExchangeTotalReceived { get => string.IsNullOrWhiteSpace(txtTotal.Text) ? 0 : Convert.ToDecimal(txtTotal.Text); set => txtTotal.Text = value.ToString(); }
        public string ExchangeTransactionName { get => txtTransactionName.Text; set => txtTransactionName.Text = value; }

        public bool ExchangeTargetPriceEnabled { get => txtPrice.Enabled; set => txtPrice.Enabled = value; }
        public bool ExchangeQuantityEnabled { get => txtQuantity.Enabled; set => txtQuantity.Enabled = value; }
        public bool ExchangeTotalReceivedEnabled { get => txtTotal.Enabled; set => txtTotal.Enabled = value; }
        public bool ExchangeTransactionNameEnabled { get => txtTransactionName.Enabled; set => txtTransactionName.Enabled = value; }
        public bool ExchangeButtonEnabled { get => btnExchange.Enabled; set => btnExchange.Enabled = value; }

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

        public void AddCoinExchangeTo(long aCurrencyId, string aCurrencyName, string aCurrencySymbol, decimal aPrice)
        {
            ListViewItem lSelectedCurrency = null;
            if (SelectedExchangeMarket != null)
            {
                lSelectedCurrency = SelectedExchangeMarket;
            }

            for (int it = 0; it < lstCoinAvailable.Items.Count; it++)
            {
                if (lstCoinAvailable.Items[it].ImageKey == aCurrencyId.ToString())
                {
                    lstCoinAvailable.Items.RemoveAt(it);
                    break;
                }
            }

            ListViewItem item = lstCoinAvailable.Items.Add(string.Format("{0}", aCurrencyName));
            item.SubItems.Add(aCurrencySymbol);
            item.SubItems.Add(aPrice.ToString());

            item.ImageKey = aCurrencyId.ToString();
            if (lSelectedCurrency != null)
            {
                item.Selected = (lSelectedCurrency.SubItems[1].Text == item.SubItems[1].Text);
                item.Focused = (lSelectedCurrency.SubItems[1].Text == item.SubItems[1].Text);
            }
        }

        public void ClearListExchangeTo()
        {
            lstCoinAvailable.Items.Clear();
        }

        public ListViewItem SelectedExchangeMarket
        {
            get
            {
                if (lstCoinAvailable.Items.Count > 0 && lstCoinAvailable.SelectedItems.Count > 0)
                {
                    return lstCoinAvailable.SelectedItems[0];
                }
                else
                {
                    return null;
                }
            }
        }

        public void ExchangeSelectCurrency(string aTicker)
        {
            try
            {
                if (aTicker == null)
                {
                    return;
                }

                if (lstCoinAvailable.Items.Count > 0)
                {
                    lstCoinAvailable.Items[0].Selected = true;
                    lstCoinAvailable.Items[0].Focused = true;
                    //lstCoinAvailable.Select();
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
                OnExhangeCurrencySelectionChanged?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
            }
        }

        public bool AllOrderHistoryChecked => chckOrderHistory.Checked;

        public void AddOrderHistory(int aInternalID, string aTrasactionName, string aSold, string aReceived, string aPrice, string aExchange, string aDateTime, string aStatus)
        {
            //if (SelectedOrderHistory != null)
            //    for (int it = 0; it < lstOrderHistory.Items.Count; it++)
            //    {
            //        if ((int)lstOrderHistory.Items[it].Tag == aInternalID)
            //        {
            //            lstOrderHistory.Items.RemoveAt(it);
            //            break;
            //        }
            //    }

            ListViewItem lSelectedOrder = null;
            if (FOrderHistorySelected != null)
            {
                lSelectedOrder = FOrderHistorySelected;
            }

            ListViewItem item = lstOrderHistory.Items.Add(string.Format("{0}", aTrasactionName));
            item.SubItems.Add(aSold);
            item.SubItems.Add(aReceived);
            item.SubItems.Add(aPrice);
            item.SubItems.Add(aExchange);
            item.SubItems.Add(aDateTime);
            item.SubItems.Add(aStatus);
            item.Tag = aInternalID;

            if (lSelectedOrder != null)
            {
                item.Selected = (lSelectedOrder.Tag.ToString() == item.Tag.ToString());
                item.Focused = (lSelectedOrder.Tag.ToString() == item.Tag.ToString());
            }
        }

        public void ClearOrderHistory()
        {
            lstOrderHistory.Items.Clear();
        }

        private ListViewItem FOrderHistorySelected;

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

        public event EventHandler OnOrderHistorySelectionChanged;

        private void lstOrderHistory_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                OnOrderHistorySelectionChanged?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
            }
        }

        private void cbExchanges_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                OnExhangeCurrencySelectionChanged?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
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
                this.StandardErrorMsgBox(ex.Message);
            }
        }

        //public event EventHandler OnExchangeQuantityTxtChanged;

        //private void txtQuantity_TextChanged(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        OnExchangeQuantityTxtChanged?.Invoke(sender, e);
        //    }
        //    catch (Exception ex)
        //    {
        //        this.StandardErrorMsgBox(ex.Message);
        //    }
        //}

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
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
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
                this.StandardErrorMsgBox(ex.Message);
            }
        }

        //private void txtTotal_TextChanged(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        OnTotalReceivedChanged?.Invoke(sender, e);
        //    }
        //    catch (Exception ex)
        //    {
        //        this.StandardErrorMsgBox(ex.Message);
        //    }
        //}

        private void lblEstimatePrice_Click(object sender, EventArgs e)
        {
            try
            {
                OnLabelEstimatePriceClick?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
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
                this.StandardErrorMsgBox(ex.Message);
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
                this.StandardErrorMsgBox(ex.Message);
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
                this.StandardErrorMsgBox(ex.Message);
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
                this.StandardErrorMsgBox(ex.Message);
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
                this.StandardErrorMsgBox(ex.Message);
            }
        }
    }
}