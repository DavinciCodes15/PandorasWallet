using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Pandora.Client.PandorasWallet.Dialogs
{
    public partial class PrivKeyDialog : BaseDialog, IDisposable
    {
        public PrivKeyDialog()
        {
            InitializeComponent();
            Utils.ChangeFontUtil.ChangeDefaultFontFamily(this);
        }

        public event EventHandler OnOkBtnClick;

        public string TitleMessage
        {
            get => lblTitle.Text;
            set => lblTitle.Text = $"Private keys for {value}:";
        }

        public void SetPrivateKeys(IDictionary<string, string> aKeys)
        {
            var lTextBuilder = new StringBuilder();
            int lCounter = 1;
            if (aKeys.Any())
                foreach (var lKeyPair in aKeys)
                {
                    lTextBuilder.AppendLine();
                    lTextBuilder.AppendLine("----------------------------------------------------------------------------");
                    lTextBuilder.AppendLine();
                    lTextBuilder.AppendLine($"{lCounter}. Public Address:");
                    lTextBuilder.AppendLine($"\t{lKeyPair.Key}");
                    lTextBuilder.AppendLine($"{lCounter}. Private Key:");
                    lTextBuilder.AppendLine($"\t{lKeyPair.Value}");
                    lTextBuilder.AppendLine();
                    lCounter++;
                }
            else
                lTextBuilder.Append("No private keys were found"); //This should never happend
            txtBoxPrivkey.Text = lTextBuilder.ToString();
            lTextBuilder.Clear();
        }

        private void btnOK_Click(object sender, System.EventArgs e)
        {
            try
            {
                OnOkBtnClick?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        public new void Dispose()
        {
            txtBoxPrivkey.Text = string.Empty;
            base.Dispose();
        }
    }
}