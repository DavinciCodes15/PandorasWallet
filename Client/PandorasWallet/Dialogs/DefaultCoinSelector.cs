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

namespace Pandora.Client.PandorasWallet.Dialogs
{
    public partial class DefaultCoinSelector : BaseDialog
    {
        private bool FOkPressed;
        private List<lstCurrencyListItem> FCoinList;

        public event EventHandler OnOkBtnClick;

        public event EventHandler OnCancelBtnClick;

        public DefaultCoinSelector()
        {
            InitializeComponent();
            lstViewDefaultCoin.SetVisualOptions(CurrencyView.VisualOptionFlags.CurrencyNameVisible | CurrencyView.VisualOptionFlags.IconVisible);
            lstViewDefaultCoin.OnSelectedIndexChanged += LstViewDefaultCoin_OnSelectedIndexChanged;
            lstViewDefaultCoin.ChangeColumnWidth(0, 215);
        }

        private void LstViewDefaultCoin_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                SelectedCurrencyID = lstViewDefaultCoin.SelectedCurrencyId;

                if (SelectedCurrencyID == 0)
                {
                    btnOK.Enabled = false;
                    return;
                }

                btnOK.Enabled = true;

                lstCurrencyListItem lSelected = FCoinList.Where(x => x.CurrencyID == SelectedCurrencyID).First();

                lblCoinName.Text = lSelected.CurrencyName;

                TickerLabel.Text = lSelected.CurrencySimbol;

                picCoinImage.Image = lSelected.CurrencyIcon.ToBitmap();
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        public void AddItemsToShow(IEnumerable<lstCurrencyListItem> aListOfCurrencyItems)
        {
            foreach (lstCurrencyListItem it in aListOfCurrencyItems)
            {
                lstViewDefaultCoin.AddCurrency(it.CurrencyID, it.CurrencyName, it.CurrencySimbol, it.CurrencyIcon);
            }

            FCoinList = aListOfCurrencyItems.ToList();
        }

        public long SelectedCurrencyID
        {
            get;
            private set;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                OnOkBtnClick?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardUnhandledErrorMsgBox(ex.Message);
            }

            FOkPressed = true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                OnCancelBtnClick?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardUnhandledErrorMsgBox(ex.Message);
            }
        }

        private void DefaultCoinSelector_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool lCancel = lstViewDefaultCoin.SelectedCurrencyId == 0;
            if (FOkPressed)
            {
                FOkPressed = false;
                e.Cancel = lCancel;
            }

            SelectedCurrencyID = lstViewDefaultCoin.SelectedCurrencyId;

            if (lCancel)
            {
                return;
            }

            lstViewDefaultCoin.ClearCurrencies();
        }

        public class lstCurrencyListItem
        {
            public long CurrencyID { get; set; }
            public string CurrencyName { get; set; }
            public string CurrencySimbol { get; set; }
            public Icon CurrencyIcon { get; set; }
        }

        private void DefaultCoinSelectorDummy_Shown(object sender, EventArgs e)
        {
            lstCurrencyListItem lCurrencyItem = FCoinList.OrderBy(x => x.CurrencyName).FirstOrDefault();
            lstViewDefaultCoin.SelectedCurrencyId = lCurrencyItem == null ? 0 : lCurrencyItem.CurrencyID;
        }
    }
}