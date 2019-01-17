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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static Pandora.Client.PandorasWallet.CurrencyView;

namespace Pandora.Client.PandorasWallet.Dialogs
{
    public partial class AddCoinSelector : BaseDialog
    {
        private bool FOkPressed;

        public event EventHandler OnOkClickButton;

        public event EventHandler OnCancelClickButton;

        public AddCoinSelector()
        {
            InitializeComponent();
            lstViewAddCurrency.SetVisualOptions((VisualOptionFlags.CurrencyNameVisible | VisualOptionFlags.TickerColunmVisible | VisualOptionFlags.IconVisible | VisualOptionFlags.UseCheckBoxes));
            lstViewAddCurrency.OnItemChecked += LstViewAddCurrency_OnItemChecked;
            lstViewAddCurrency.ChangeColumnWidth(0, 200);
            lstViewAddCurrency.ChangeColumnWidth(1, 95);
        }

        private void LstViewAddCurrency_OnItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (lstViewAddCurrency.CheckedCurrencyIds.Count() > 0)
            {
                btnOK.Enabled = true;
            }
            else
            {
                btnOK.Enabled = false;
            }
        }

        public long[] SelectedItems
        {
            get;
            private set;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            UpdateSelectedCoins();
            try
            {
                this.SetWaitCursor();
                OnOkClickButton?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
            }
            finally
            {
                this.SetArrowCursor();
            }

            FOkPressed = true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            UpdateSelectedCoins();
            try
            {
                this.SetWaitCursor();
                OnCancelClickButton?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
            }
            finally
            {
                this.SetArrowCursor();
            }
        }

        private void UpdateSelectedCoins()
        {
            SelectedItems = new long[lstViewAddCurrency.CheckedCurrencyIds.Count()];
            Array.Copy(lstViewAddCurrency.CheckedCurrencyIds, SelectedItems, SelectedItems.Count());
        }

        public void AddItemsToShow(IEnumerable<lstCurrencyViewItem> aListOfCurrencyItems)
        {
            foreach (lstCurrencyViewItem it in aListOfCurrencyItems)
            {
                lstViewAddCurrency.AddCurrency(it.CurrencyID, it.CurrencyName, it.CurrencySimbol, it.CurrencyIcon);
            }
        }

        private void AddCoinSelector_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (FOkPressed)
            {
                FOkPressed = false;
                e.Cancel = !lstViewAddCurrency.CheckedCurrencyIds.Any();
            }

            UpdateSelectedCoins();

            lstViewAddCurrency.ClearCurrencies();
        }

        public class lstCurrencyViewItem
        {
            public long CurrencyID { get; set; }
            public string CurrencyName { get; set; }
            public string CurrencySimbol { get; set; }
            public Icon CurrencyIcon { get; set; }
        }

        private void AddCoinSelectorDummy_Shown(object sender, EventArgs e)
        {
            btnOK.Enabled = false;
        }
    }
}