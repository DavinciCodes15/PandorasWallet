using Pandora.Client.PandorasWallet.Dialogs.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet.Dialogs
{
    public partial class SignMessageDialog : BaseDialog
    {
        public delegate bool OnVerifyMessageDelegate(IGUICurrency aGUICurrency, string aMessage, string aSignature, out string aAddress);

        public delegate string OnSignMessageDelegate(IGUICurrency aGUICurrency, string aMessage, out string aAddress);

        public event OnSignMessageDelegate OnSingMessageNeeded;

        public event OnVerifyMessageDelegate OnVerifyMessageNeeded;

        public long SelectedCurrencyID
        {
            get => SelectedCurrency.Id;
            set
            {
                var lSelectedItem = FCurrencies.FirstOrDefault(lCurrency => lCurrency.Id == value);
                comboBoxCurrency.SelectedItem = lSelectedItem ?? throw new ArgumentException($"Currency id {value} not found");
            }
        }

        private IEnumerable<IGUICurrency> FCurrencies;
        private IGUICurrency SelectedCurrency => comboBoxCurrency.SelectedItem as IGUICurrency;

        public SignMessageDialog(IEnumerable<IGUICurrency> aCurrencies)
        {
            InitializeComponent();
            if (!aCurrencies.Any()) throw new ArgumentException(nameof(aCurrencies), "Collection can not be empty");
            FCurrencies = aCurrencies;
            comboBoxCurrency.DisplayMember = "Name";
            comboBoxCurrency.Items.AddRange(aCurrencies.ToArray());
            comboBoxCurrency.SelectedIndex = 0;
        }

        private void btnSign_Click(object sender, EventArgs e)
        {
            try
            {
                var lMessageToSign = txtBoxMsgToSign.Text;
                if (!string.IsNullOrEmpty(lMessageToSign))
                {
                    string lSignedHash = string.Empty;
                    string aAddress = string.Empty;
                    if (SelectedCurrency != null)
                        lSignedHash = OnSingMessageNeeded.Invoke(SelectedCurrency, lMessageToSign, out aAddress);
                    txtBoxSignature.Text = lSignedHash;
                    txtBoxAddress.Text = aAddress;
                }
                else
                    this.StandardErrorMsgBox("Message to sign is empty", "Please provide a message to sign");
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtBoxSignature.Text))
            {
                Clipboard.SetText(txtBoxSignature.Text);
                toolTipCopy.Active = true;
                toolTipCopy.Show("Signature Copied to clipboard", btnCopy);
                Task.Run(() =>
                 {
                     Task.Delay(3000).Wait();
                     this.BeginInvoke(new Action(() => toolTipCopy.Active = false));
                 });
            }
        }

        private void btnVerifySignature_Click(object sender, EventArgs e)
        {
            try
            {
                var lMessageToVerify = txtBoxMsgToSign.Text;
                var lMessageSignature = txtBoxSignature.Text;
                if (!string.IsNullOrEmpty(lMessageToVerify) && !string.IsNullOrEmpty(lMessageSignature))
                {
                    bool? lVerified = null;
                    string lAddress = string.Empty;
                    if (SelectedCurrency != null)
                        lVerified = OnVerifyMessageNeeded.Invoke(SelectedCurrency, lMessageToVerify, lMessageSignature, out lAddress);
                    if (!lVerified.HasValue) throw new InvalidOperationException("A problem was encounter while verifying signature");
                    if (lVerified.Value)
                    {
                        this.StandardInfoMsgBox("Signature verification succesful", "Signature verified");
                        txtBoxAddress.Text = lAddress;
                    }
                    else
                    {
                        this.StandardErrorMsgBox("Signature verification failed", "Signature invalid");
                        txtBoxAddress.Clear();
                    }
                }
                else
                    this.StandardErrorMsgBox("Message or signature to verify is empty", "Please provide a message/signature");
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void txtBoxSignedMsg_TextChanged(object sender, EventArgs e)
        {
            btnCopy.Enabled = !string.IsNullOrEmpty(txtBoxSignature.Text);
        }

        private void comboBoxCurrency_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtBoxAddress.Clear();
            txtBoxSignature.Clear();
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            panelHelp.Visible = true;
            btnHelp.Enabled = false;
        }

        private void btn_close_help_Click(object sender, EventArgs e)
        {
            panelHelp.Visible = false;
            btnHelp.Enabled = true;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            string LResetText = "";
            txtBoxAddress.Text = LResetText;
            txtBoxMsgToSign.Text = LResetText;
            txtBoxSignature.Text = LResetText;
        }
    }
}