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
using Pandora.Client.PandorasWallet.Wallet;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet.Dialogs
{
    public partial class ConnectDialog : BaseDialog
    {
        public string Email { get => txtEmail.Text; set => txtEmail.Text = value; }
        public string Username { get => txtUsername.Text; set => txtUsername.Text = value; }
        public string Password { get => txtPassword.Text; set => txtPassword.Text = value; }

        private bool FOkPressed;

        public event EventHandler OnOkClick;

        public bool UserConnected { get; set; }

        public ConnectDialog()
        {
            InitializeComponent();
            Name = string.Format("Connect to {0} Server", AboutBox.AssemblyProduct);
        }

        private void ConnectDialog_Shown(object sender, EventArgs e)
        {
            txtEmail.Focus();
            Password = string.Empty;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            FOkPressed = true;

            this.SetWaitCursor();

            try
            {
                try
                {
                    OnOkClick?.Invoke(this, e);
                }
                finally
                {
                    txtPassword.SelectAll();
                    this.SetArrowCursor();
                }
            }
            catch (ClientExceptions.InvalidOperationException)
            {
                this.StandardUnhandledErrorMsgBox("Email, username or password is incorrect,\nplease check your account at www.pandoraswallet.com", "Login failed");
                DialogResult = DialogResult.None;
            }
            catch (ClientExceptions.UserNotActiveException ex)
            {
                string s = string.Format("{0}\n{1}\n", ex.Data["message"], ex.Data["statustime"]);
                this.StandardUnhandledErrorMsgBox(s, "Login failed: User not active");
                DialogResult = DialogResult.None;
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex , "Login failed");
            }
        }

        private void ConnectDialog_Validating(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void ConnectDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (FOkPressed)
            {
                FOkPressed = false;
                e.Cancel = !UserConnected;
            }
        }
    }
}