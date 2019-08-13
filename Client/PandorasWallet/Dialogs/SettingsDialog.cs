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
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet.Dialogs
{
    public partial class SettingsDialog : BaseDialog
    {
        public EventHandler OnChangeDefaultCoinClick;
        public EventHandler OnShowPrivateKey;

        public long CurrencyId { get; private set; }

        public string DataPath { get => txtDataPath.Text; set => txtDataPath.Text = value; }

        public bool EncryptWallet { get => checkEncryptWallet.Checked; set => checkEncryptWallet.Checked = value; }

        public void SetDefaultCurrency(long aCurrencyId, string aName, Image aImage)
        {
            this.imgBoxDefaultCoin.Image = aImage;
            this.lblDefaultCoin.Text = aName;
            CurrencyId = aCurrencyId;
        }

        public SettingsDialog()
        {
            InitializeComponent();
            Utils.ChangeFontUtil.ChangeDefaultFontFamily(this);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
 
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
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

        private void SettingsDialogDummy_Shown(object sender, EventArgs e)
        {
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

        private void checkEncryptWallet_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void txtDataPath_Leave(object sender, EventArgs e)
        {
        }

        private void btnPrivKey_Click(object sender, EventArgs e)
        {
            try
            {
                OnShowPrivateKey?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        //private void BtnDefault_Click(object sender, EventArgs e)
        //{
        //    DataPath = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "Pandora's Wallet");
        //}
    }
}