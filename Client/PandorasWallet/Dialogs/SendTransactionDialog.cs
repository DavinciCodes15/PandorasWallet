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
        private Size FNormalSize = new Size(949, 417);
        private Size FExpandedSize = new Size(949, 476);
        public EventHandler OnConfirmButtonClick;
        public EventHandler OnCancelButtonClick;
        private bool FEthereumStyleEnabled;
        private decimal FTotalAmounToSend;
        private SendTransactionInfo FTxInfo;

        public decimal TotalAmountToSend
        {
            get => FTotalAmounToSend;
            private set
            {
                lblAmount.Text = $"{value} {FTxInfo.BalanceTicker} ≈ {FTxInfo.FiatSymbol} {Math.Round(FTxInfo.FiatPrice * value, 2)}";
                FTotalAmounToSend = value;
            }
        }

        public object[] AdvancedTxOptions { get; internal set; }

        public SendTransactionDialog(SendTransactionInfo aTxInfo, bool aUseEthereumLayout = false)
        {
            InitializeComponent();
            FEthereumStyleEnabled = aUseEthereumLayout;
            FTxInfo = aTxInfo;
            numericCustomFee.Value = aTxInfo.TotalFee;
            if (aTxInfo.Nonce >= 0)
                numericNonce.Value = aTxInfo.Nonce + 1;
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
            this.Size = FNormalSize;
            FillDialogWithInfo(FTxInfo);
            chckBoxSubsFee.Visible = !FTxInfo.IsSentAll && !IsToken();
            chckBoxSubsFee.Checked = FTxInfo.IsSentAll;
            CalculateDiscountedAndAmount(chckBoxSubsFee.Checked);
            bool lOkPressed = ShowDialog() == DialogResult.OK;
            AdvancedTxOptions = new object[] { GetTotalFee(), $"0x{Convert.ToInt32(numericNonce.Value).ToString("X")}" };
            return lOkPressed;
        }

        private bool IsToken()
        {
            return FTxInfo.BalanceTicker != FTxInfo.FeeTicker;
        }

        private void FillDialogWithInfo(SendTransactionInfo aInfo)
        {
            lblFromAddress.Text = aInfo.FromAddress;
            lblToAddress.Text = aInfo.ToAddress;
            if (FEthereumStyleEnabled)
            {
                lblFeeRateTitle.Text = "Gas Price:";
            }
            lblBalanceBefore.Text = $"{aInfo.CurrentBalance} {aInfo.BalanceTicker}";
        }

        private decimal GetTotalFee()
        {
            bool lIsCustomFeeEnabled = numericCustomFee.Value != FTxInfo.TotalFee;
            lblTxFeeRate.Text = lIsCustomFeeEnabled ? $"CUSTOM" : $"{FTxInfo.FeeRate.ToString("G8")} {FTxInfo.FeeTicker}";
            return lIsCustomFeeEnabled ? numericCustomFee.Value : FTxInfo.TotalFee;
        }

        private void CalculateDiscountedAndAmount(bool isSubsFromAmountChecked)
        {
            decimal lAmountDiscounted;
            decimal lBalanceAfter;
            if (isSubsFromAmountChecked)
            {
                lAmountDiscounted = FTxInfo.AmountToSend;
                TotalAmountToSend = IsToken() ? FTxInfo.AmountToSend : FTxInfo.AmountToSend - GetTotalFee();
            }
            else
            {
                lAmountDiscounted = IsToken() ? FTxInfo.AmountToSend : FTxInfo.AmountToSend + GetTotalFee();
                TotalAmountToSend = FTxInfo.AmountToSend;
            }
            var lTotalFee = GetTotalFee();
            lblTxFee.Text = $"{lTotalFee} {FTxInfo.FeeTicker} ≈ {FTxInfo.FiatSymbol} {Math.Round(FTxInfo.FiatPrice * lTotalFee, 2)}";
            lblDiscounted.Text = $"-{lAmountDiscounted} {FTxInfo.BalanceTicker}";
            lBalanceAfter = FTxInfo.CurrentBalance - lAmountDiscounted;
            lblBalanceAfter.Text = $"{lBalanceAfter} {FTxInfo.BalanceTicker}";
            btnOK.Enabled = (FEthereumStyleEnabled ? TotalAmountToSend >= 0 : TotalAmountToSend > 0) && lBalanceAfter >= 0;
            if (lblWarning.Visible = !btnOK.Enabled)
                lblWarning.Text = "Invalid transaction amounts.";
        }

        private void checkSubtFee_CheckedChanged(object sender, EventArgs e)
        {
            CalculateDiscountedAndAmount(((CheckBox) sender).Checked);
        }

        private void checkBoxAdvanced_CheckedChanged(object sender, EventArgs e)
        {
            groupBoxAdvanced.Visible = ((CheckBox) sender).Checked;
            this.Size = groupBoxAdvanced.Visible ? FExpandedSize : FNormalSize;
            lblNonce.Visible = numericNonce.Visible = FEthereumStyleEnabled;
        }

        public class SendTransactionInfo
        {
            public string FromAddress { get; set; }
            public string ToAddress { get; set; }
            public decimal AmountToSend { get; set; }
            public decimal CurrentBalance { get; set; }
            public string BalanceTicker { get; set; }
            public decimal TotalFee { get; set; }
            public string FeeTicker { get; set; }
            public decimal FeeRate { get; set; }
            public bool IsSentAll { get; set; }
            public int Nonce { get; set; }
            public decimal FiatPrice { get; set; }
            public string FiatSymbol { get; set; }
        }

        private void numericCustomFee_ValueChanged(object sender, EventArgs e)
        {
            CalculateDiscountedAndAmount(chckBoxSubsFee.Checked);
        }
    }
}