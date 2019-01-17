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
using System.IO;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet.Dialogs
{
    public partial class SettingsDialog : BaseDialog
    {
        private string FCachedDatapath;
        public EventHandler OnOkButtonClick;
        public EventHandler OnCancelButtonClick;
        public EventHandler OnChangeDefaultCoinClick;

        public string DefaultPath { get; set; }

        public string DefaultDefaultCoin { get; set; }

        public string DefaultServer { get; set; }

        public int DefaultPort { get; set; }

        public Image DefaultcoinImage { get => imgBoxDefaultCoin.Image; set => imgBoxDefaultCoin.Image = value; }

        public string DataPath
        {
            get => txtDataPath.Text;
            set
            {
                if (!Directory.Exists(value))
                {
                    throw new ArgumentException("Bad Datapath");
                }
                txtDataPath.Text = value;
                FCachedDatapath = value;
            }
        }

        public bool ConnectionSettingsVisible
        {
            get => groupConnection.Visible;
            set
            {
                groupConnection.Visible = value;
                Size = value ? new Size(444, 378) : new Size(444, 234);
            }
        }

        public string ServerName
        {
            get => txtServerName.Text;
            set => txtServerName.Text = value;
        }

        public int PortNumber
        {
            get => (int)numPort.Value;
            set => numPort.Value = value;
        }

        public bool EncryptConnection
        {
            get => checkEncrypted.Checked;
            set => checkEncrypted.Checked = value;
        }

        public bool EncryptWallet
        {
            get => checkEncryptWallet.Checked;
            set => checkEncryptWallet.Checked = value;
        }

        public string DefaultCoin
        {
            get => lblDefaultCoin.Text;
            set => lblDefaultCoin.Text = value;
        }

        public SettingsDialog()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                OnOkButtonClick?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                OnCancelButtonClick?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
            }
        }

        private void btnChangeDefaultCoin_Click(object sender, EventArgs e)
        {
            try
            {
                OnChangeDefaultCoinClick?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
            }
        }

        private void btnResetDefaults_Click(object sender, EventArgs e)
        {
            try
            {
                DataPath = DefaultPath;
            }
            finally
            {
                DefaultCoin = DefaultDefaultCoin;
                ServerName = DefaultServer;
                PortNumber = DefaultPort;
            }
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            if (SelectFolderDialog.ShowDialog() == DialogResult.OK)
            {
                DataPath = SelectFolderDialog.SelectedPath;
            }
        }

        private void SettingsDialogDummy_Shown(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ServerName))
            {
                ServerName = DefaultServer;
            }

            if (PortNumber == 0)
            {
                PortNumber = DefaultPort;
            }

            if (DefaultCoin.Contains("No Default Chosen") || string.IsNullOrWhiteSpace(DefaultCoin))
            {
                DefaultCoin = DefaultDefaultCoin;
            }

            if (string.IsNullOrWhiteSpace(DataPath))
            {
                DataPath = DefaultPath;
            }

#if DEBUG
            Size = new Size(444, 373);
            groupConnection.Visible = true;
#else
            Size = new Size(444, 232);
            groupConnection.Visible = false;
#endif
        }

        private void SettingsDialogDummy_FormClosing(object sender, FormClosingEventArgs e)
        {
            string s = "";
            if (DialogResult == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(ServerName) || PortNumber == 0)
                {
                    s += "You must enter a Server Name and Port. \n";
                }

                if (string.IsNullOrEmpty(DataPath))
                {
                    s += "You must specify a datapath. \n";
                }

                if (DefaultCoin.Contains("No Default Chosen") || string.IsNullOrWhiteSpace(DefaultCoin))
                {
                    s += "You must choose a default coin \n";
                }

                e.Cancel = s != "";
                if (e.Cancel)
                {
                    this.StandardErrorMsgBox(s);
                }
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void checkEncryptWallet_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkEncryptWallet.Checked)
            {
                this.StandardInfoMsgBox("Beware that disabling wallet encryptation could compromise wallet security.");
            }
        }

        private void txtDataPath_Leave(object sender, EventArgs e)
        {
            try
            {
                DataPath = txtDataPath.Text;
            }
            catch
            {
                this.StandardErrorMsgBox("Invalid DataPath");
                txtDataPath.Focus();
                txtDataPath.Text = FCachedDatapath;
            }
        }
    }
}