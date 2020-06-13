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
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet.Dialogs
{
    public partial class SendTransactionDialog : BaseDialog
    {
        public EventHandler OnConfirmButtonClick;
        public EventHandler OnCancelButtonClick;

        private decimal FBalance;
        private decimal FTxFee;
        private decimal FTxFeeRate;
        private decimal FAmount;
        private decimal FBalanceAfter;
        private decimal FDiscounted;
        private decimal FCachedAmount;
        public string FromAddress { get => lblFromAddress.Text; set => lblFromAddress.Text = value; }

        public string ToAddress { get => lblToAddress.Text; set => lblToAddress.Text = value; }

        public decimal Amount { get => FAmount; set { FAmount = value; lblAmount.Text = value.ToString() + Ticker; } }

        public decimal Balance { get => FBalance; set { FBalance = value; lblBalanceBefore.Text = value.ToString() + Ticker; } }

        public decimal TxFee { get => FTxFee; set { FTxFee = value; lblTxFee.Text = value.ToString() + Ticker; } }

        public decimal TxFeeRate { get => FTxFeeRate; set { FTxFeeRate = value; lblTxFeeRate.Text = value.ToString() + Ticker; } }

        public bool SubstractFee
        {
            get => chckBoxSubsFee.Checked;
            set => chckBoxSubsFee.Checked = value;
        }

        public string Ticker { get; set; }

        public bool EthereumStyleEnabled { get; private set; }

        public SendTransactionDialog(bool aUseEthereumLayout = false)
        {
            InitializeComponent();
            EthereumStyleEnabled = aUseEthereumLayout;
        }

        private decimal BalanceAfterSend
        {
            get => FBalanceAfter;
            set
            {
                FBalanceAfter = value;
                lblBalanceAfter.Text = value.ToString() + Ticker;
                if (value < 0)
                {
                    lblBalanceAfter.ForeColor = Color.Red;
                    lblWarning.Text = "Not enough coins to send transaction with TxFee.";
                    lblWarning.Visible = true;
                    btnOK.Enabled = false;
                }
                else
                {
                    lblBalanceAfter.ForeColor = Color.Black;
                    lblWarning.Text = "";
                    lblWarning.Visible = false;
                    btnOK.Enabled = true;
                }
            }
        }

        public decimal Discounted
        {
            get => FDiscounted;
            set
            {
                FDiscounted = value;
                lblDiscounted.Text = FDiscounted.ToString() + Ticker;
            }
        }

        public SendTransactionDialog()
        {
            InitializeComponent();
            Utils.ChangeFontUtil.ChangeDefaultFontFamily(this);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            OnConfirmButtonClick?.Invoke(sender, e);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            OnCancelButtonClick?.Invoke(sender, e);
        }

        public new bool Execute()
        {
            if (EthereumStyleEnabled)
                ChangeLayoutToEthereum();
            FCachedAmount = FAmount;
            return ShowDialog() == DialogResult.OK;
        }

        private void ChangeLayoutToEthereum()
        {
            lblFeeRateTitle.Text = "Gas Price:";
        }

        private void checkSubtFee_CheckedChanged(object sender, EventArgs e)
        {
            if (SubstractFee)
                Discounted = TxFee;
            else
                Discounted = 0;
            Amount = FCachedAmount - Discounted;
            BalanceAfterSend = Balance - (Amount + TxFee);
        }

        private void SendTransaction_Shown(object sender, EventArgs e)
        {
            checkSubtFee_CheckedChanged(this, null);
        }
    }
}