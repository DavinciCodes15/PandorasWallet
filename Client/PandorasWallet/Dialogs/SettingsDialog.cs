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
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet.Dialogs
{
    public partial class SettingsDialog : BaseDialog
    {
        public EventHandler OnChangeDefaultCoinClick;
        public Func<long, IDictionary<string, string>> OnGetPrivateKey;

        public long DefaultCurrencyId { get; private set; }

        public long ActiveCurrencyId { get; private set; }

        public string DataPath { get => txtDataPath.Text; set => txtDataPath.Text = value; }

        public bool RequestWalletPassword { get => checkEncryptWallet.Checked; set => checkEncryptWallet.Checked = value; }

        public bool AutoUpdate { get => cbAutoUpdate.Checked; set => cbAutoUpdate.Checked = value; }

        public string SelectedFiat { get => (string) cmboBoxFiatCurrency.SelectedItem; set => cmboBoxFiatCurrency.SelectedItem = value; }

        public string PrivateKeyBtnText { get => btnPrivKey.Text; set => btnPrivKey.Text = $"View {FCurrencyName = value} private key..."; }

        private string FCurrencyName;

        public void SetDefaultCurrency(long aCurrencyId, string aName, Image aImage)
        {
            this.imgBoxDefaultCoin.BackgroundImage = aImage;
            this.lblDefaultCoin.Text = aName;
            DefaultCurrencyId = aCurrencyId;
        }

        public void SetActiveCurrency(long aCurrencyId, string aName, Image aImage)
        {
            this.imgCurrentCoin.BackgroundImage = aImage;
            lblSelectedCoin.Text = aName;
            PrivateKeyBtnText = aName;
            ActiveCurrencyId = aCurrencyId;
        }

        public SettingsDialog()
        {
            InitializeComponent();
            Utils.ChangeFontUtil.ChangeDefaultFontFamily(this);
        }

        private void btnChangeDefaultCoin_Click(object sender, EventArgs e)
        {
            try
            {
                OnChangeDefaultCoinClick?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            if (SelectFolderDialog.ShowDialog() == DialogResult.OK)
                DataPath = SelectFolderDialog.SelectedPath;
        }

        private void SettingsDialogDummy_FormClosing(object sender, FormClosingEventArgs e)
        {
            string lErrorMsg = "";
            if (DialogResult == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(DataPath) || !Directory.Exists(DataPath))
                {
                    lErrorMsg = "You must specify a valid path";
                    txtDataPath.SelectAll();
                    txtDataPath.Focus();
                }
                e.Cancel = lErrorMsg != "";
                if (e.Cancel)
                    this.StandardErrorMsgBox("Settings Error", lErrorMsg);
            }
        }

        private void btnPrivKey_Click(object sender, EventArgs e)
        {
            try
            {
                var lPrivateKey = OnGetPrivateKey?.Invoke(ActiveCurrencyId);
                if (lPrivateKey != null)
                    using (PrivKeyDialog lPrivateKeyDialog = new PrivKeyDialog())
                    {
                        lPrivateKeyDialog.TitleMessage = FCurrencyName;
                        lPrivateKeyDialog.SetPrivateKeys(lPrivateKey);
                        lPrivateKeyDialog.Execute();
                    }
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        public void SetFiatCurrencies(IEnumerable<string> aFiatCurrencies)
        {
            if (aFiatCurrencies == null || !aFiatCurrencies.Any()) throw new ArgumentException(nameof(aFiatCurrencies), "FiatCurrencies can not be empty or null");
            cmboBoxFiatCurrency.Items.Clear();
            cmboBoxFiatCurrency.Items.AddRange(aFiatCurrencies.ToArray());
        }
    }
}