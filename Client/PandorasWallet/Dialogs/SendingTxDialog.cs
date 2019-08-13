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

namespace Pandora.Client.PandorasWallet.Dialogs
{
    public partial class SendingTxDialog : BaseDialog
    {
        public SendingTxDialog()
        {
            InitializeComponent();
            Utils.ChangeFontUtil.ChangeDefaultFontFamily(this);
        }

        public string Status { get => StatusLabel.Text; set => StatusLabel.Text = value; }

        private void SendingTxDialog_Shown(object sender, EventArgs e)
        {
            TxId.Text = string.Empty;
            TxId.Visible = false;
            btnOK.Visible = false;
            TxIdLabel.Visible = false;

            StatusPictureBox.Image = Properties.Resources.Waiting;

            Status = "In Progress...";

            StatusPictureBox.Refresh();
        }

        public void Response(string aMessage, string aTxID = null)
        {
            if (string.IsNullOrEmpty(aTxID))
            {
                StatusPictureBox.Image = Properties.Resources.cancel;
                this.Text = "Error Sending Transaction";

            }
            else
            {
                StatusPictureBox.Image = Properties.Resources.ok;
                this.Text = "Transaction Sent";
                TxIdLabel.Visible = true;
                TxId.Text = aTxID;
                TxId.Visible = true;
            }

            StatusLabel.Text = aMessage;
            btnOK.Visible = true;
            btnOK.Focus();
        }

    }
}