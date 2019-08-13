using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet.Dialogs
{
    public partial class InitializingDialog : Form
    {
        public InitializingDialog()
        {
            InitializeComponent();
            Utils.ChangeFontUtil.ChangeDefaultFontFamily(this);
        }

        public void SetInitialized()
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }
    }
}