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
    public partial class ConnectionSettingsDialog : BaseDialog
    {

        public EventHandler OnOkButtonClick;
        public EventHandler OnCancelButtonClick;


        public string DefaultServer { get; set; }

        public int DefaultPort { get; set; }

        public string ServerName
        {
            get => txbServerName.Text;
            set => txbServerName.Text = value;
        }

        public int PortNumber
        {
            get => (int)nudPortNumber.Value;
            set => nudPortNumber.Value = value;
        }

        public bool EncryptConnection
        {
            get => chbEncryptConnection.Checked;
            set => chbEncryptConnection.Checked = value;
        }

        public ConnectionSettingsDialog()
        {
            InitializeComponent();
            Utils.ChangeFontUtil.ChangeDefaultFontFamily(this);
            nudPortNumber.Maximum = int.MaxValue;
        }

        private void ConnectionSettingsDialog_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ServerName))
            {
                ServerName = DefaultServer;
            }

            if (PortNumber == 0)
            {
                PortNumber = DefaultPort;
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            try
            {
                OnOkButtonClick?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                OnCancelButtonClick?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }

        private void ConnectionSettingsDialog_FormClosing(object sender, FormClosingEventArgs e)
        {

            string s = "";
            if (DialogResult == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(ServerName) || PortNumber == 0)
                {
                    s += "You must enter a Server Name and Port. \n";
                }             

                e.Cancel = s != "";
                if (e.Cancel)
                {
                    this.StandardErrorMsgBox("Settings Error", s);
                }
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}

