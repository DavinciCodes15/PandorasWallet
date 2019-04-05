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
using System.Linq;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet.Dialogs
{
    public partial class WalletPasswordDialog : BaseDialog
    {
        private bool FOkPressed;
        private bool FPasswordMatch;
        private bool FIsNewPassword;

        public EventHandler OnCancelButtonClick;
        public EventHandler OnOkButtonClick;

        public string Password { get; private set; }

        public WalletPasswordDialog()
        {
            InitializeComponent();
        }

        public new bool Execute()
        {
            return Execute(false);
        }

        public bool Execute(bool aIsNewPasswordDialog)
        {
            FIsNewPassword = aIsNewPasswordDialog;
            return ShowDialog() == DialogResult.OK;
        }

        private void WalletPasswordBox_Shown(object sender, EventArgs e)
        {
            FPasswordMatch = false;
            unlockPictureBox.Image = Properties.Resources.unlocked;

            passwordBox.Focus();
            passwordBox.Clear();
            passwordBoxCheck.Clear();
            WarningLabel.Hide();

            if (!FIsNewPassword)
            {
                firstLabel.Text = "Insert password used to decrypt wallet";
                passwordBoxCheck.Hide();
                secondLabel.Hide();
                lockPictureBock.Image = Properties.Resources.locked;
                unlockPictureBox.Hide();
                lockPictureBock.Show();
                WarningLabel.Show();
                WarningLabel.Text = "- LOCKED -";
            }
            else
            {
                firstLabel.Text = "Insert new password used to decrypt wallet";
                passwordBox.Show();
                passwordBoxCheck.Show();
                secondLabel.Show();
                lockPictureBock.Hide();
                unlockPictureBox.Show();
            }

            FPasswordMatch = false;
            btnOK.Enabled = true;
        }

        private void WalletPasswordBox_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (FOkPressed)
            {
                FOkPressed = false;
                e.Cancel = !FPasswordMatch;
            }
        }

        private bool CheckPasswordsBoxes()
        {
            if (!FIsNewPassword)
            {
                if (string.IsNullOrWhiteSpace(passwordBox.Text))
                {
                    WarningLabel.Text = "Please enter password to continue";
                    WarningLabel.Show();
                    return false;
                }
                return true;
            }

            if (string.IsNullOrWhiteSpace(passwordBox.Text) || string.IsNullOrWhiteSpace(passwordBoxCheck.Text))
            {
                WarningLabel.Text = "Please fill all boxes to continue";
                WarningLabel.Show();
                return false;
            }

            if (passwordBox.Text.Length < 8)
            {
                WarningLabel.Text = "Password too short to continue. Minimum 8 characters.";
                WarningLabel.Show();
                return false;
            }

            if (!(passwordBox.Text.Any(c => char.IsLetter(c))))
            {
                WarningLabel.Text = "Password must contain at least one letter (a-Z)\n and one digit (0-9)";
                WarningLabel.Show();
                return false;
            }

            if (!(passwordBox.Text == passwordBoxCheck.Text))
            {
                WarningLabel.Text = "Password must match. Please verify";
                WarningLabel.Show();
                return false;
            }

            return true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            FOkPressed = true;

            if (!CheckPasswordsBoxes())
            {
                return;
            }

            lockPictureBock.Image = Properties.Resources.Waiting;

            if (!FIsNewPassword)
            {
                WarningLabel.Text = "Trying to decrypt...";
                WarningLabel.Show();
            }
            else
            {
                WarningLabel.Text = "Encrypting wallet. Please Wait...";
                WarningLabel.Show();
            }

            btnOK.Enabled = false;

            Password = passwordBox.Text;

            try
            {
                OnOkButtonClick?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }

            this.SetWaitCursor();
        }

        public void SetResult(string aError = null)
        {
            this.SetArrowCursor();

            if (string.IsNullOrEmpty(aError))
            {
                FPasswordMatch = true;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                WarningLabel.Text = aError;
                WarningLabel.Show();
                lockPictureBock.Image = Properties.Resources.locked;
                btnOK.Enabled = true;
            }

            Password = null;
        }

        private void passwordBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = (e.KeyChar == (char)Keys.Space);
        }

        private void passwordBoxCheck_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = (e.KeyChar == (char)Keys.Space);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                OnCancelButtonClick?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex);
            }
        }
    }
}