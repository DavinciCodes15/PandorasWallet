using System;

namespace Pandora.Client.PandorasWallet.Dialogs
{
    public partial class PrivKeyDialog : BaseDialog
    {
        public PrivKeyDialog()
        {
            InitializeComponent();
        }

        public event EventHandler OnOkBtnClick;

        public string CoinName
        {
            get => lblSelectedCoin.Text;
            set => lblSelectedCoin.Text = value;
        }

        public string PrivKey { get => txtBoxPrivkey.Text; set => txtBoxPrivkey.Text = value; }

        private void btnOK_Click(object sender, System.EventArgs e)
        {
            try
            {
                OnOkBtnClick?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardUnhandledErrorMsgBox(ex.Message);
            }
        }
    }
}