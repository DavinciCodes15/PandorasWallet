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
using Pandora.Client.ClientLib.Contracts;
using Pandora.Client.Crypto.Currencies;
using Pandora.Client.Exchange.Contracts;
using Pandora.Client.Exchange.Models;
using Pandora.Client.PandorasWallet.Dialogs;
using Pandora.Client.PandorasWallet.Dialogs.Models;
using Pandora.Client.PandorasWallet.Models;
using Pandora.Client.PandorasWallet.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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
        private Dictionary<long, IGUICurrency> FCurrencyLookup = new Dictionary<long, IGUICurrency>();

        public event EventHandler OnConnect;

        public event EventHandler OnChangeExchangeKeysBtnClick;

        public event EventHandler OnAddCurrencyBtnClick;

        public event EventHandler OnAddTokenBtnClick;

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

        public event Action OnPriceChanged;

        public event EventHandler OnCheckAllOrderHistory;

        public event EventHandler OnSettingsMenuClick;

        public event Action OnExchangeQuantityChanged;

        public event Action OnExchangeReceivedChanged;

        public event EventHandler OnSelectedCurrencyChanged;

        public event Action<long> OnRemoveCurrencyRequested;

        public event EventHandler OnExchangeSelectionChanged;

        public AppMainForm()
        {
            InitializeComponent();
            Crypto.Currencies.Ethereum.EthereumCurrencyAdvocacy.Register();
            lstExchangers.SelectedIndex = -1;
            lstExchangers.DisplayMember = "Name";

            Cursor.Current = Cursors.WaitCursor;

            lstViewCurrencies.SetVisualOptions(CurrencyView.VisualOptionFlags.CurrencyNameVisible | CurrencyView.VisualOptionFlags.TickerColunmVisible | CurrencyView.VisualOptionFlags.IconVisible, new string[] { "Total", "Status", "Crypto value", "Fiat Value", "Token of" });

            Cursor.Current = Cursors.Default;

            QuickSendButton.AddMenuItem("Send available balance");
            QuickSendButton.AssingOnClickEvent(OnSendAllMenuClickEvent, 0);

            TransactionView.ListViewItemSorter = new PandoraListViewItemSorter(0, SortOrder.Descending);

            lstOrderHistory.ListViewItemSorter = new PandoraListViewItemSorter(1, SortOrder.Descending);

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

        public string SearchBar { get => txtBoxSearchCoin.Text; set => txtBoxSearchCoin.Text = value; }

        public long SelectedCurrencyId { get => lstViewCurrencies.SelectedCurrencyId; set => lstViewCurrencies.SelectedCurrencyId = value; }

        public string CoinName { get => lblCoinName.Text; set => lblCoinName.Text = value; }

        public string NotesBox { get => txtNotesBox.Text; set => txtNotesBox.Text = value; }

        public bool CheckAllOrderHistoryEnabled { set => chckOrderHistory.Enabled = value; }

        public ListView TransactionView => listTransactions;

        public Image CoinImage
        { get => picCoinImage.Image; set { picCoinImage.Image = value; picCoinImage.Visible = true; } }

        public GUITransaction SelectedTransaction { get; private set; }

        public IEnumerable<IGUICurrency> Currencies { get => FCurrencyLookup.Values; }

        public string CoinStatus
        { get => lblStatus.Text; set { lblStatus.Text = value; } }

        public IGUICurrency GetCurrency(long aCurrenyId)
        {
            if (!FCurrencyLookup.TryGetValue(aCurrenyId, out IGUICurrency lResult))
                Universal.Log.Write(Universal.LogLevel.Warning, "Unable to find currency id {0}", aCurrenyId);
            return lResult;
        }

        public void AddCurrency(IGUICurrency aCurrency)
        {
            if (!FCurrencyLookup.ContainsKey(aCurrency.Id))
            {
                string lMarketValue = aCurrency.MarketPrices.DefaultCoinPrice > 0 ? $"{FormatedAmount(aCurrency.MarketPrices.DefaultCoinValue, aCurrency.Precision)} {aCurrency.MarketPrices.SymbolCurrency}" : "-";
                string lFiatValue = aCurrency.MarketPrices.FiatPrice > 0 ? $"{aCurrency.MarketPrices.FiatValue.ToString("F")}  {aCurrency.MarketPrices.SymbolFiat}" : "-";
                var lGUIToken = aCurrency as GUIToken;
                var lCustomValues = new string[] { FormatedAmount(aCurrency.Balances.Total, aCurrency.Precision), aCurrency.CurrentStatus.ToString(), lMarketValue, lFiatValue, lGUIToken?.ParentCurrency.Name ?? "-" };
                CurrencyViewControl.AddCurrency(aCurrency.Id, aCurrency.Name, aCurrency.Ticker, Globals.BytesToIcon(aCurrency.Icon), lCustomValues);
                aCurrency.MarketPrices.OnPricesChanged += CurrencyMarketPrices_OnPricesChanged;
                FCurrencyLookup.Add(aCurrency.Id, aCurrency);
            }
        }

        public void RemoveCurrency(long aCurrencyIDToRemove, long aDefaultCurrencyID)
        {
            if (FCurrencyLookup.TryGetValue(aCurrencyIDToRemove, out IGUICurrency lGUICurrency))
            {
                if (SelectedCurrencyId != aCurrencyIDToRemove)
                    OnSelectedCurrencyChanged?.Invoke(null, null); //This is only to trigger exchange tab to refresh
                CurrencyViewControl.RemoveCurrency(aCurrencyIDToRemove);
                FCurrencyLookup.Remove(aCurrencyIDToRemove);
                lGUICurrency.MarketPrices.OnPricesChanged -= CurrencyMarketPrices_OnPricesChanged;
                UpdateWalletTotals();
            }
            else
                throw new Exception("Default currency id provided not found");
        }

        private void CurrencyMarketPrices_OnPricesChanged(IGUICurrency aCurrency)
        {
            if (aCurrency.Id == SelectedCurrencyId)
                UpdateCurrencyView();
            UpdateCurrency(aCurrency.Id);
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
            this.ClearExchangeMarketSelector();
            this.ClearOrderHistory();
            ToSendAddress = string.Empty;
            ToSendAmount = 0;
        }

        public static string FormatedAmount(decimal aAmount, ushort aPrecision)
        {
            string lFormatPattern = @"{0:0.000}";
            if (aPrecision > 3)
                lFormatPattern = $"{{0:0.000{new string('#', aPrecision - 3)}}}";
            return string.Format(lFormatPattern, aAmount);
        }

        private void CurrencyListView_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var lId = lstViewCurrencies.SelectedCurrencyId;
                TransactionView.Items.Clear();
                if (FCurrencyLookup.ContainsKey(lId))
                {
                    //The call of OnSelectedCurrencyChanged is async to accelerate a little the operation of counstruccion of the interface
                    var lInvokeResults = new List<IAsyncResult>();
                    foreach (var lDelegate in OnSelectedCurrencyChanged.GetInvocationList())
                        lInvokeResults.Add(this.BeginInvoke(lDelegate));
                    SelectedCurrency = FCurrencyLookup[SelectedCurrencyId];
                    this.CoinImage = Globals.BytesToIcon(SelectedCurrency.Icon).ToBitmap();
                    UpdateCurrencyView();
                    this.NotesBox = "";
                    foreach (var lTransaction in SelectedCurrency.Transactions.TransactionItems)
                        AddTransaction(lTransaction);
                    foreach (var lResult in lInvokeResults)
                        this.EndInvoke(lResult);
                }
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
            finally
            {
                this.SetArrowCursor();
            }
        }

        private void UpdateCurrencyView()
        {
            SetCurrencyTooltip(SelectedCurrency);
            this.lblTotal.SetAmount(SelectedCurrency.Balances.Total, SelectedCurrency.Precision);
            this.lblUnconfirmed.SetAmount(SelectedCurrency.Balances.UnconfirmedBalance, SelectedCurrency.Precision);
            this.lblCurrencyPrice.SetAmount(SelectedCurrency.MarketPrices.DefaultCoinPrice, 8);
            this.lblMarketTickerPrice.Text = $"{SelectedCurrency.MarketPrices.SymbolCurrency ?? "Coin"} Price :";
            this.lblFiatTickerPrice.Text = $"{SelectedCurrency.MarketPrices.SymbolFiat ?? "Fiat"} Price :";
            this.lblMarketTickerValue.Enabled = this.lblFiatTickerValue.Enabled = this.grpBoxWalletTotals.Enabled = this.grpPrices.Enabled =
                this.lblFiatValue.Enabled = this.lblCurrencyValue.Enabled = this.lblTickerFiatTotal.Enabled =
                this.lblTickerCurrencyTotal.Enabled = this.lblCurrencyTotal.Enabled =
                this.lblFiatTotal.Enabled = !string.IsNullOrEmpty(SelectedCurrency.MarketPrices.SymbolCurrency);
            this.lblMarketTickerValue.Text = $"{SelectedCurrency.MarketPrices.SymbolCurrency ?? "Coin"} Value :";
            this.lblFiatTickerValue.Text = $"{SelectedCurrency.MarketPrices.SymbolFiat ?? "Fiat"} Value :";
            this.lblTickerCurrencyTotal.Text = $"{SelectedCurrency.MarketPrices.SymbolCurrency ?? "Coin"} Value :";
            this.lblTickerFiatTotal.Text = $"{SelectedCurrency.MarketPrices.SymbolFiat ?? "Fiat"} Value :";
            this.lblCurrencyValue.SetAmount(SelectedCurrency.MarketPrices.DefaultCoinValue, 8);
            this.lblFiatPrice.SetAmount(SelectedCurrency.MarketPrices.FiatPrice, 2);
            this.lblFiatValue.SetAmount(SelectedCurrency.MarketPrices.FiatValue, 2);
            this.CoinName = SelectedCurrency.Name;
            this.CoinStatus = SelectedCurrency.CurrentStatus.ToString();
            this.ReceiveAddress = SelectedCurrency.LastAddress;
#if !DEBUG
            SetTxSendAreaUsability(SelectedCurrency.CurrentStatus == CurrencyStatus.Active);
#endif
        }

        public void AddTransaction(GUITransaction aTransaction)
        {
            if (aTransaction.TxType == TransactionDirection.Unknown) return;
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

        public bool UpdateTransaction(GUITransaction aTransaction)
        {
            if (aTransaction.ParentCurrency.Id != SelectedCurrencyId) return false;
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

        private void SetCurrencyTooltip(IGUICurrency aSelectedCurrency)
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
            if (FCurrencyLookup.TryGetValue(aCurrencyID, out IGUICurrency lCurrency))
            {
                lCurrency.Balances.UpdateBalance();
                string lMarketValue = lCurrency.MarketPrices.DefaultCoinPrice > 0 ? $"{FormatedAmount(lCurrency.MarketPrices.DefaultCoinValue, lCurrency.Precision)} {lCurrency.MarketPrices.SymbolCurrency}" : "-";
                string lFiatValue = lCurrency.MarketPrices.FiatPrice > 0 ? $"{lCurrency.MarketPrices.FiatValue.ToString("F")}  {lCurrency.MarketPrices.SymbolFiat}" : "-";
                var lCustomValues = new string[] { FormatedAmount(lCurrency.Balances.Total, lCurrency.Precision), lCurrency.CurrentStatus.ToString(), lMarketValue, lFiatValue };
                CurrencyViewControl.UpdateCurrency(lCurrency.Id, lCurrency.Name, lCurrency.Ticker, lCustomValues);
                if (SelectedCurrency.Id == lCurrency.Id)
                    UpdateCurrencyView();
                foreach (var lTx in lCurrency.Transactions.TransactionItems)
                    this.UpdateTransaction(lTx);
                UpdateWalletTotals();
                return true;
            }

            return false;
        }

        private void UpdateWalletTotals()
        {
            lblCurrencyTotal.SetAmount(FCurrencyLookup.Sum(lCurrency => lCurrency.Value.MarketPrices.DefaultCoinValue), 8);
            lblFiatTotal.SetAmount(FCurrencyLookup.Sum(lCurrency => lCurrency.Value.MarketPrices.FiatValue), 2);
        }

        public void UpdateCurrencyMarketPrices(long aCurrencyID, long aBaseCurrencyID, decimal aDefaultCoinPrice, decimal aFiatPrice, string aFiatSymbol)
        {
            if (FCurrencyLookup.TryGetValue(aCurrencyID, out IGUICurrency lCurrency))
                lCurrency.MarketPrices.UpdatePrices(aDefaultCoinPrice, aFiatPrice, FCurrencyLookup[aBaseCurrencyID].Ticker, aFiatSymbol);
        }

        public bool RemoveTransaction(GUITransaction aTransaction)
        {
            ListViewItem lListViewItem = null;
            foreach (var lObj in TransactionView.Items)
            {
                var lTx = (lObj as ListViewItem).Tag as GUITransaction;
                if (lTx != null && lTx.RecordId == aTransaction.RecordId)
                {
                    lListViewItem = lObj as ListViewItem;
                    break;
                }
            }
            if (lListViewItem != null)
            {
                SelectedCurrency.Transactions.RemoveTransaction(aTransaction);
                TransactionView.Items.Remove(lListViewItem);
                //UpdateCurrency(SelectedCurrency);
            }
            return lListViewItem != null;
        }

        public void ClearAllTransactions()
        {
            foreach (var lCurrency in FCurrencyLookup.Values)
            {
                lCurrency.Transactions.ClearTransactions();
                UpdateCurrency(lCurrency.Id);
            }
            TransactionView.Items.Clear();
        }

        public void RefreshTransactions(long aCurrencyID = -1)
        {
            if (SelectedCurrency.Id == aCurrencyID || aCurrencyID < 0)
            {
                TransactionView.Items.Clear();
                foreach (var lTransaction in SelectedCurrency.Transactions.TransactionItems)
                    AddTransaction(lTransaction);
                UpdateCurrency(SelectedCurrencyId);
            }
        }

        private void listTransactions_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            NotesBox = "";
            if (e.IsSelected)
            {
                SelectedTransaction = (GUITransaction)e.Item.Tag;
                NotesBox = $"Transaction ID : {SelectedTransaction.TxId} - To Address: {SelectedTransaction.ToAddress}{Environment.NewLine}" +
                           $"Tx Block Number: {SelectedTransaction.BlockNumber} - Confirmations: {SelectedTransaction.Confirmations}{Environment.NewLine}" +
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
                e.Handled &= (e.KeyChar != (int)CommandKeyCodes.Backspace);
                e.Handled &= (e.KeyChar != (int)CommandKeyCodes.Copy);
                e.Handled &= (e.KeyChar != (int)CommandKeyCodes.Cut);
                e.Handled &= (e.KeyChar != (int)CommandKeyCodes.Undo);
                e.Handled &= (e.KeyChar != (int)CommandKeyCodes.Redo);
                e.Handled |= (e.KeyChar == (int)CommandKeyCodes.Space);
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

        public ExchangeItem SelectedExchange { get => lstExchangers.GetItemText(lstExchangers.SelectedItem) == "--Select--" ? null : (ExchangeItem)lstExchangers.SelectedItem; }

        public bool ExchangeSelectorEnabled { get => lstExchangers.Enabled; set => lstExchangers.Enabled = value; }
        public bool ExchangeMarketSelectorEnabled { get => lstExchangeMarket.Enabled; set => lstExchangeMarket.Enabled = value; }

        public bool ExchangeLoadingHidden { get => !lblLoading.Visible; set => lblLoading.Visible = !value; }

        public ListView.ListViewItemCollection ExchangeCoinItems => lstExchangeMarket.Items;

        public string LabelCoinExchange { set => lblExchange.Text = string.Format("Exchange {0} On", value); }

        public string TickerQuantity { get => txtQuantity.CurrencyTicker; set => txtQuantity.CurrencyTicker = value; }

        public string TickerPrices
        {
            get => txtExchangeTargetPrice.CurrencyTicker;
            set
            {
                txtExchangeTargetPrice.CurrencyTicker = value;
                txtStopPrice.CurrencyTicker = value;
            }
        }

        public string TickerTotalReceived { get => txtTotalReceived.CurrencyTicker; set => txtTotalReceived.CurrencyTicker = value; }

        public string LabelPriceInCoin { set => lblEstimatePriceCoin.Text = string.Format("Estimated Current price in ({0}):", value); }

        public decimal EstimatePrice { get => Convert.ToDecimal(lblEstimatePrice.Text); set => lblEstimatePrice.Text = value.ToString(); }

        public decimal ExchangeTargetPrice { get => txtExchangeTargetPrice.Amount; set => txtExchangeTargetPrice.Amount = value; }
        public decimal ExchangeStopPrice { get => txtStopPrice.Amount; set => txtStopPrice.Amount = value; }
        public decimal ExchangeQuantity { get => txtQuantity.Amount; set => txtQuantity.Amount = value; }
        public decimal ExchangeTotalReceived { get => txtTotalReceived.Amount; set => txtTotalReceived.Amount = value; }
        public string ExchangeTransactionName { get => txtTransactionName.Text; set => txtTransactionName.Text = value; }

        public bool ExchangeTargetPriceEnabled { get => txtExchangeTargetPrice.Enabled; set => txtExchangeTargetPrice.Enabled = value; }
        public bool ExchangeStoptPriceEnabled { get => txtStopPrice.Enabled; set => txtStopPrice.Enabled = value; }
        public bool ExchangeQuantityEnabled { get => txtQuantity.Enabled; set => txtQuantity.Enabled = value; }
        public bool ExchangeTotalReceivedEnabled { get => txtTotalReceived.Enabled; set => txtTotalReceived.Enabled = value; }
        public bool ExchangeTransactionNameEnabled { get => txtTransactionName.Enabled; set => txtTransactionName.Enabled = value; }
        public bool ExchangeButtonEnabled { get => btnExchange.Enabled; set => btnExchange.Enabled = value; }

        public event Action OnStopPriceTextChanged;

        private int FLastStatusOrderID = -1;

        public void ClearExchangeList()
        {
            lstExchangers.Items.Clear();
        }

        public void AddExchange(ExchangeItem aExchangeItem)
        {
            if (!lstExchangers.Items.Contains(aExchangeItem))
                lstExchangers.Items.Add(aExchangeItem);
        }

        public void AddExchange(IEnumerable<ExchangeItem> aExchangeItems)
        {
            foreach (var lExchangeItem in aExchangeItems)
                AddExchange(lExchangeItem);
        }

        public void ChangeExchangeSelection(string aExchangeName)
        {
            if (string.IsNullOrEmpty(aExchangeName)) lstExchangers.SelectedIndex = -1;
            else
                lstExchangers.SelectedIndex = lstExchangers.FindStringExact(aExchangeName);
        }

        public class ExchangeItem
        {
            public int ID { get; set; }
            public string Name { get; set; }
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
            try
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
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        internal void ClearExchangeChart()
        {
            marketPriceChart.Series.Clear();
        }

        public void ClearExchangeMarketSelector()
        {
            lstExchangeMarket.Items.Clear();
            lstExchangeMarket.Enabled = false;
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

        internal void LoadMarketIntervals(IEnumerable<string> aChartIntervals)
        {
            if (aChartIntervals == null || !aChartIntervals.Any()) throw new ArgumentException(nameof(aChartIntervals), "Intervals must have a value");
            cmboBoxChartInterval.Items.Clear();
            cmboBoxChartInterval.Items.AddRange(aChartIntervals.ToArray());
        }

        internal void AddExchangeChartPoint(DateTime timeStamp, decimal open, decimal close, decimal high, decimal low)
        {
            if (!marketPriceChart.Series.Any())
                throw new Exception("Chart must be initialized");
            var lPriceSeries = marketPriceChart.Series["Prices"];
            lPriceSeries.Points.AddXY(timeStamp, high, low, open, close);
        }

        internal void InitializeExchangeChart(string aBaseCurrency, string aChartTitle = null)
        {
            var lChartArea = marketPriceChart.ChartAreas.FirstOrDefault();
            if (!marketPriceChart.Series.Any())
            {
                lChartArea.AxisY.LabelStyle.Format = "0.########";
                lChartArea.AxisY.IsStartedFromZero = false;
                var lPriceSeries = new System.Windows.Forms.DataVisualization.Charting.Series("Prices", 4);
                marketPriceChart.Series.Add(lPriceSeries);
                lPriceSeries.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Candlestick;
                lPriceSeries.CustomProperties = "PriceDownColor=Red, PriceUpColor=Green";
                lChartArea.AxisX.LabelStyle.Format = "MM-dd HH:mm";
                if (!cmboBoxChartInterval.Enabled)
                    cmboBoxChartInterval.SelectedIndex = 0;
                cmboBoxChartInterval.Enabled = true;
            }
            lChartArea.AxisY.Title = aBaseCurrency;
            lblChartTitle.Text = string.IsNullOrEmpty(aChartTitle) ? "Market Price Chart:" : aChartTitle;
        }

        public string ExchangeSelectedChartInterval => cmboBoxChartInterval.SelectedItem?.ToString();

        private Dictionary<int, ExchangeOrderViewModel> FExchangeOrders = new Dictionary<int, ExchangeOrderViewModel>();
        private Dictionary<long, List<int>> FExchangeOrdersByCurrency = new Dictionary<long, List<int>>();
        private Dictionary<int, ListViewItem> FExchangeOrderLstViewItems = new Dictionary<int, ListViewItem>();

        public void AddOrUpdateOrder(ExchangeOrderViewModel aOrder, long aCurrencyID, bool aUpdateListView = true)
        {
            if (FExchangeOrders.TryGetValue(aOrder.ID, out ExchangeOrderViewModel lOrderViewModel))
            {
                if (!object.ReferenceEquals(aOrder, lOrderViewModel))
                    lOrderViewModel.CopyFrom(aOrder);
                AddToOrderLstViewItemCache(aOrder, aCurrencyID);
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
            lstOrderHistory.Sort();
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
            lResult.SubItems.Add(aOrder.Time);
            lResult.SubItems.Add(aOrder.Sold);
            lResult.SubItems.Add(aOrder.Received);
            lResult.SubItems.Add(aOrder.Price);
            lResult.SubItems.Add(aOrder.Stop);
            lResult.SubItems.Add(aOrder.Exchange);
            lResult.SubItems.Add(aOrder.Status);
            return lResult;
        }

        public void SetExchangeLastOrder(int aOrderID)
        {
            if (FLastStatusOrderID >= 0)
                FExchangeOrders[FLastStatusOrderID].OnNewLogAdded -= LastOrder_OnNewLogAdded;
            StatusControlExchange.ClearStatusList();
            if (FExchangeOrders.TryGetValue(aOrderID, out ExchangeOrderViewModel aOrder))
            {
                foreach (var lOrderMessage in aOrder.GetLogs())
                    StatusControlExchange.AddStatus(lOrderMessage.ID, lOrderMessage.Time, lOrderMessage.Message);
                FLastStatusOrderID = aOrderID;
                aOrder.OnNewLogAdded += LastOrder_OnNewLogAdded;
            }
        }

        private void LastOrder_OnNewLogAdded(int aParentOrderID, ExchangeOrderLogViewModel aLog)
        {
            if (FLastStatusOrderID == aParentOrderID)
                StatusControlExchange.AddStatus(aLog.ID, aLog.Time, aLog.Message);
        }

        private void AddToOrderLstViewItemCache(ExchangeOrderViewModel aOrder, long aCurrencyID)
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
                {
                    if (FExchangeOrders[lOrderID].ExchangeID == SelectedExchange.ID)
                        UpdateOrderLstViewWith(lOrderID, FExchangeOrderLstViewItems[lOrderID], false);
                }
            }
            statscntrlTradeHistory.ClearStatusList();
        }

        public void LoadAllCurrencyExchangeOrders()
        {
            ClearOrderHistory();
            foreach (var lOrder in FExchangeOrderLstViewItems)
            {
                if (FExchangeOrders[lOrder.Key].ExchangeID == SelectedExchange.ID)
                    UpdateOrderLstViewWith(lOrder.Key, lOrder.Value, false);
            }
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
            lstOrderHistory.Sort();
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

        public event Action<int> OnSellHalfOnDoubleClick;

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

        public IGUICurrency SelectedCurrency { get; private set; }
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

        private void ExchangeOrder_OnNewLogAdded(int aParentOrderID, ExchangeOrderLogViewModel aLog)
        {
            var lCurrentSelected = SelectedOrderHistory?.Name;
            int lID = lCurrentSelected != null ? -1 : Convert.ToInt32(lCurrentSelected);
            if (lID == aParentOrderID)
                statscntrlTradeHistory.AddStatus(aLog.ID, aLog.Time, aLog.Message);
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

        private void txtReceived_OnAmountChanged()
        {
            try
            {
                OnExchangeReceivedChanged?.Invoke();
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
                    contextMenuOrderMenu.Show(Cursor.Position);
                var lOrderID = Convert.ToInt32(SelectedOrderHistory.Name);
                var lStatus = FExchangeOrders[lOrderID].Status;
                contextMenuOrderMenu.Items[0].Enabled = (lStatus != Exchange.OrderStatus.Interrupted.ToString() && lStatus != Exchange.OrderStatus.Withdrawn.ToString());
                contextMenuOrderMenu.Items[1].Enabled = lStatus == Exchange.OrderStatus.Withdrawn.ToString();
            }
        }

        public class ExchangeOrderViewModel
        {
            public class ExchangeOrderViewModelContextData
            {
                public IExchangeMarket Market { get; set; }
                public decimal TradeComission { get; set; }
                public ushort Precision { get; set; }

                public AppMainForm MainForm { get; set; }

                public decimal GetReceivingQuantity(decimal aSentQuantity, decimal aOrderRate)
                {
                    decimal lRate = Market.MarketDirection == MarketDirection.Sell ? aOrderRate : 1 / aOrderRate;
                    decimal lRawAmount = aSentQuantity * lRate;
                    decimal lQuantity = Math.Round(lRawAmount - (lRawAmount * TradeComission), Precision);
                    return lQuantity;
                }
            }

            public ExchangeOrderViewModel(Exchange.Models.UserTradeOrder aOrder, ExchangeOrderViewModelContextData aContextData)
            {
                ID = aOrder.InternalID;
                FOrderLogs = new Dictionary<int, ExchangeOrderLogViewModel>();
                ContextData = aContextData;
                Name = aOrder.Name;
                Sold = aOrder.SentQuantity.ToString();
                Received = aContextData.GetReceivingQuantity(aOrder.SentQuantity, aOrder.Rate).ToString();
                Price = aOrder.Rate.ToString();
                Stop = aOrder.StopPrice.ToString();
                Exchange = $"{aOrder.Market.SellingCurrencyInfo.Ticker} -> {aOrder.Market.BuyingCurrencyInfo.Ticker}";
                Time = aOrder.OpenTime.ToLocalTime().ToString();
                Status = aOrder.Status.ToString();
                ExchangeID = aOrder.Market.ExchangeID;
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
            public int ExchangeID { get; private set; }
            public int ID { get; private set; }
            private Dictionary<int, ExchangeOrderLogViewModel> FOrderLogs;

            public event Action<int, ExchangeOrderLogViewModel> OnNewLogAdded;

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
                        ContextData.MainForm.BeginInvoke(OnNewLogAdded, ID, aLog);
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
            catch (Exchange.PandoraExchangeExceptions.InvalidExchangeCredentials ex)
            {
                this.StandardInfoMsgBox("Unable to change exchange credentials", ex.Message);
            }
            catch (Exception ex)
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

        private void lblTotal_Click(object sender, EventArgs e)
        {
            try
            {
                DoTotalBalanceClick();
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void labelTotal_Click(object sender, EventArgs e)
        {
            try
            {
                DoTotalBalanceClick();
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void DoTotalBalanceClick()
        {
            decimal lCurrentBalance = SelectedCurrency.Balances.Total;
            ToSendAmount = lCurrentBalance;
            if (tabControl.SelectedTab.Name == "tabExchange" && ExchangeQuantityEnabled)
                ExchangeQuantity = lCurrentBalance;
        }

        private void txtQuantity_OnAmountChanged()
        {
            try
            {
                OnExchangeQuantityChanged?.Invoke();
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void txtStopPrice_OnAmountChanged()
        {
            try
            {
                OnStopPriceTextChanged?.Invoke();
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void txtTargetPrice_OnAmountChanged()
        {
            try
            {
                OnPriceChanged?.Invoke();
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void menuItemSellHalf_Click(object sender, EventArgs e)
        {
            try
            {
                var lSelectedOrder = SelectedOrderHistory;
                if (lSelectedOrder == null) return;
                var lInternalOrderID = Convert.ToInt32(lSelectedOrder.Name);
                DoGUISellHalfOperation(lInternalOrderID);
                OnSellHalfOnDoubleClick?.Invoke(lInternalOrderID);
            }
            catch
            (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
            }
        }

        private void DoGUISellHalfOperation(int aOrderID)
        {
            try
            {
                this.SetWaitCursor();
                var lOrder = GetOrderViewModel(aOrderID);
                if (lOrder == null) throw new Exception("Exchange Order not found");
                tabControl.SelectTab("tabExchange");
                DoExchangeChangeOfMarketOperation(lOrder.ContextData.Market.BuyingCurrencyInfo.Id, lOrder.ContextData.Market.SellingCurrencyInfo.Id);
                var lTargetPrice = Convert.ToDecimal(lOrder.Price) * 2;
                var lQuantityToSpend = Convert.ToDecimal(lOrder.Received) / 2;
                ExchangeTargetPrice = lTargetPrice;
                ExchangeStopPrice = lTargetPrice;
                ExchangeQuantity = lQuantityToSpend;
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
            finally
            {
                this.SetArrowCursor();
            }
        }

        private void DoExchangeChangeOfMarketOperation(long aWalletCurrencyID, long aMarketCurrencyID)
        {
            lstViewCurrencies.SelectedCurrencyId = aWalletCurrencyID;
            do
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(100);
            } while (lstExchangeMarket.Items.Count == 0);

            var lExchangeMarketItem = lstExchangeMarket.Items.Find(aMarketCurrencyID.ToString(), false).FirstOrDefault();
            if (lExchangeMarketItem == null)
                return;
            lExchangeMarketItem.Selected = true;
        }

        private void lstExchangeMarket_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                this.SetWaitCursor();
                if (SelectedExchangeMarket == null) return;
                var lCurrencyIDFrom = SelectedCurrencyId;
                var lCurrencyIDTo = Convert.ToInt32(SelectedExchangeMarket.Name);
                DoExchangeChangeOfMarketOperation(lCurrencyIDTo, lCurrencyIDFrom);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
            finally
            {
                this.SetArrowCursor();
            }
        }

        private void toolStripMenuRemove_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.StandardAskMsgBox("Do you want to remove this coin?", "By removing it you will not loose any of your funds. You can add it back in throught \"Add Currency\""))
                    OnRemoveCurrencyRequested?.Invoke(SelectedCurrencyId);
            }
            catch (InvalidOperationException ex)
            {
                this.StandardInfoMsgBox("Unable to remove currency", ex.Message);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
            finally
            {
                this.SetArrowCursor();
            }
        }

        private void AddTokenBtn_Click(object sender, EventArgs e)
        {
            try
            {
                OnAddTokenBtnClick?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
            }
        }

        public event Func<bool> OnClearWalletCache;

        private void clearWalletCacheToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var lResult = OnClearWalletCache?.Invoke();
                if (lResult.HasValue && lResult.Value)
                {
                    ClearAllTransactions();     //reset all currency amount
                }
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        public event EventHandler OnSignMessage;

        private void signMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OnSignMessage?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        public event Action<string> OnExportTxsMenuClick;

        public event Action<string> OnExportExchangeOrdersMenuClick;

        private void ExportTxtoolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ExportTxFileSaveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                ExportTxFileSaveDialog.FileName = $"{DateTime.Now.ToString("yyyyMMddHHmm")}_PWTransactionsReport";
                if (ExportTxFileSaveDialog.ShowDialog() == DialogResult.OK)
                {
                    OnExportTxsMenuClick?.Invoke(ExportTxFileSaveDialog.FileName);
                    this.StandardInfoMsgBox("Success", "Your wallet transaction report has been exported");
                }
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void exportTradeOrdersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ExportTxFileSaveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                ExportTxFileSaveDialog.FileName = $"{DateTime.Now.ToString("yyyyMMddHHmm")}_PWTradesReport";
                if (ExportTxFileSaveDialog.ShowDialog() == DialogResult.OK)
                {
                    OnExportExchangeOrdersMenuClick?.Invoke(ExportTxFileSaveDialog.FileName);
                    this.StandardInfoMsgBox("Success", "Your wallet trade orders report has been exported");
                }
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void lstExchangers_SelectedIndexChanged(object sender, EventArgs e)
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

        public event Action OnExchangeChartIntervalChanged;

        private void cmboBoxChartInterval_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                OnExchangeChartIntervalChanged?.Invoke();
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }
    }
}