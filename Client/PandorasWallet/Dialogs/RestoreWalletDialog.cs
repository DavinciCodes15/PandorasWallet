﻿//   Copyright 2017-2019 Davinci Codes
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
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet.Dialogs
{
    public partial class RestoreWalletDialog : BaseDialog
    {
        public event EventHandler OnRestoreBtnClick;

        public event EventHandler OnCancelBtnClick;

        public string Info { get => richTextBoxInfo.Text; set => richTextBoxInfo.Text = value; }

        public string OkBtnName { get => btnOK.Name; set => btnOK.Name = value; }

        public string InitialErrorMessage { get; set; }

        public bool Restored { get; set; }

        public RestoreWalletDialog()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                OnRestoreBtnClick?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
            }
        }

        public void ErrorDialog(string aErrorMessage)
        {
            this.StandardErrorMsgBox(aErrorMessage);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                OnCancelBtnClick?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
            }
        }

        private void RestoreWalletDialog_Shown(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(InitialErrorMessage))
            {
                ErrorDialog(InitialErrorMessage);
            }
        }

        private void RestoreWalletDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            InitialErrorMessage = string.Empty;

            if (Restored)
            {
                DialogResult = DialogResult.OK;
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}