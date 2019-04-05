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
using System.Text.RegularExpressions;

namespace Pandora.Client.PandorasWallet.Dialogs
{
    public partial class LoginExchanger : BaseDialog
    {
        public event EventHandler OnOkBtnClick;

        public event EventHandler OnCancelBtnClick;

        public LoginExchanger()
        {
            InitializeComponent();
        }

        public string ExchageKey => Regex.Replace(txtKey.Text, " ", "");

        public string ExchangeSecret => Regex.Replace(txtSecret.Text, " ", "");

        public bool OkButtonEnable { get => btnOK.Enabled; set => btnOK.Enabled = value; }

        public bool SaveCredentials => checkSaveCredentials.Checked;

        private void CheckTxtBoxes()
        {
            if (string.IsNullOrWhiteSpace(ExchageKey) || string.IsNullOrWhiteSpace(ExchangeSecret))
            {
                throw new Exception("Please fill all boxes to continue");
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                CheckTxtBoxes();
                OnOkBtnClick?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardUnhandledErrorMsgBox(ex.Message);
            }
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

        private void LoginExchanges_Shown(object sender, EventArgs e)
        {
            txtKey.Text = string.Empty;
            txtSecret.Text = string.Empty;
            checkSaveCredentials.Checked = false;
        }
    }
}