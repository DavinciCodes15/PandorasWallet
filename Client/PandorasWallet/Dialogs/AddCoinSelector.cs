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
using Pandora.Client.PandorasWallet.Dialogs.Contracts;
using Pandora.Client.PandorasWallet.Dialogs.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static Pandora.Client.PandorasWallet.CurrencyView;

namespace Pandora.Client.PandorasWallet.Dialogs
{
    public partial class AddCoinSelector : BaseDialog, ICoinSelectorWindow
    {
        private bool FOkPressed;

        public event EventHandler OnOkClickButton;

        public event EventHandler OnCancelClickButton;

        public long[] SelectedCurrencyIds => lstViewAddCurrency.CheckedCurrencyIds;

        private List<GUICurrency> FCurrencyViewItems;

        public bool ShowMaintenanceWarning
        {
            get => lblMaintenanceWarning.Visible || pictureWarning.Visible;
            set
            {
                lblMaintenanceWarning.Visible = pictureWarning.Visible = value;
                lstViewAddCurrency.Height = value ? 300 : 337;
            }
        }

        public AddCoinSelector()
        {
            InitializeComponent();
            ConfigureCurrencyView();
            FCurrencyViewItems = new List<GUICurrency>();
            Utils.ChangeFontUtil.ChangeDefaultFontFamily(this);
        }

        private void ConfigureCurrencyView()
        {
            lstViewAddCurrency.SetVisualOptions((VisualOptionFlags.CurrencyNameVisible | VisualOptionFlags.TickerColunmVisible | VisualOptionFlags.IconVisible | VisualOptionFlags.UseCheckBoxes), new string[] { "Status" });
            lstViewAddCurrency.ChangeColumnWidth(0, 160);
            lstViewAddCurrency.ChangeColumnWidth(1, 60);
            lstViewAddCurrency.ChangeColumnWidth(2, 80);
            lstViewAddCurrency.OnItemChecked += LstViewAddCurrency_OnItemChecked;
        }

        private void LstViewAddCurrency_OnItemChecked(object sender, ItemCheckedEventArgs e)
        {
            btnOK.Enabled = lstViewAddCurrency.CheckedCurrencyIds.Count() > 0;
            var lCheckedCurrencies = lstViewAddCurrency.CheckedCurrencyIds;
            ShowMaintenanceWarning = FCurrencyViewItems.Any(lCurrencyView => lCheckedCurrencies.Contains(lCurrencyView.Id) &&
                                                                             lCurrencyView.CurrentStatus == ClientLib.CurrencyStatus.Maintenance);
            lstViewAddCurrency.Refresh();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                OnOkClickButton?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
            FOkPressed = true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                this.SetWaitCursor();
                OnCancelClickButton?.Invoke(sender, e);
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

        public void AddCurrencies(IEnumerable<GUICurrency> aListOfCurrencyModel)
        {
            foreach (GUICurrency lCurrencyModel in aListOfCurrencyModel)
            {
                AddCurrency(lCurrencyModel);
            }
        }

        public void AddCurrency(GUICurrency lCurrencyModel)
        {
            lstViewAddCurrency.AddCurrency(lCurrencyModel.Id, lCurrencyModel.Name, lCurrencyModel.Ticker, Universal.SystemUtils.BytesToIcon(lCurrencyModel.Icon), new string[] { lCurrencyModel.CurrentStatus.ToString() });
            if (!FCurrencyViewItems.Contains(lCurrencyModel))
                FCurrencyViewItems.Add(lCurrencyModel);
        }

        private void AddCoinSelector_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (FOkPressed)
            {
                FOkPressed = false;
                e.Cancel = !lstViewAddCurrency.CheckedCurrencyIds.Any();
            }
        }

        public void Clear()
        {
            lstViewAddCurrency.ClearCurrencies();
            FCurrencyViewItems.Clear();
        }

        private void AddCoinSelector_Shown(object sender, EventArgs e)
        {
            btnOK.Enabled = false;
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            if (btnSelectAll.Text == "Select all")
            {
                lstViewAddCurrency.CheckAllItems();
                btnSelectAll.Text = "Unselect all";
            }
            else
            {
                lstViewAddCurrency.CheckAllItems(false);
                btnSelectAll.Text = "Select all";
            }
        }
    }
}