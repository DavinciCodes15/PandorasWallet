﻿//   Copyright 2017-2019 Davinci Codes
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet.Dialogs
{
    public partial class ConnectDialog : BaseDialog
    {
        Stack<Keys> KeysConnectionSettings { get; set; } = new Stack<Keys>();
        public string Email { get => txtEmail.Text; set => txtEmail.Text = value; }
        public string Username { get => txtUsername.Text; set => txtUsername.Text = value; }
        public string Password { get => txtPassword.Text; set => txtPassword.Text = value; }

        private bool FOkPressed;

        public event EventHandler OnOkClick;

        public event EventHandler OnCallSettingDialog;

        public bool UserConnected { get; set; }

        public bool SavePassword { get => cbxSavePassword.Checked; set => cbxSavePassword.Checked = value; }

        public ConnectDialog()
        {
            InitializeComponent();
            Utils.ChangeFontUtil.ChangeDefaultFontFamily(this);
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
                    OnOkClick(this, e);
                }
                finally
                {
                    txtPassword.SelectAll();
                    this.SetArrowCursor();
                }
                if (!UserConnected)
                    this.StandardErrorMsgBox("Login failed", "Email, username or password is incorrect,\nplease check your account at www.pandoraswallet.com");
            }
            catch (ClientExceptions.UserNotActiveException ex)
            {
                string s = string.Format("{0}\n{1}\n", ex.Data["message"], ex.Data["statustime"]);
                this.StandardErrorMsgBox("Login failed: User not active", s);
            }
            catch (Exception ex)
            {
                this.StandardExceptionMsgBox(ex, "Login failed");
            }
        }

        public void ClearAccounts()
        {
            txtEmail.Items.Clear();
        }

        public void AddLoginAccount(LoginAccount aLoginAccount)
        {
            txtEmail.Items.Add(aLoginAccount);
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

        private void txtEmail_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (txtEmail.SelectedItem != null)
            {
                var lLoginAccount = txtEmail.SelectedItem as LoginAccount;
                txtUsername.Text = lLoginAccount.UserName;
                txtPassword.Text = lLoginAccount.Password;
            }
        }

        private void txtEmail_Validating(object sender, CancelEventArgs e)
        {
            if (txtEmail.SelectedItem != null)
            {
                var lLoginAccount = txtEmail.SelectedItem as LoginAccount;
                txtEmail.Text = lLoginAccount.Email;
                txtUsername.Text = lLoginAccount.UserName;
                txtPassword.Text = lLoginAccount.Password;
            }
        }

        private void ConnectDialog_KeyDown(object sender, KeyEventArgs e)
        {
            ValidationEventKeyPress(e.KeyCode);
        }



        private void ValidationEventKeyPress(Keys e)
        {
            switch (e)
            {
                case Keys.Left:
                    KeysConnectionSettings.Clear();
                    KeysConnectionSettings.Push(e);
                    break;
                case Keys.Right:
                    EvaluateKeysPressed(e, Keys.Left);
                    break;
                case Keys.Up:
                    EvaluateKeysPressed(e, Keys.Right);
                    break;
                case Keys.Down:
                    EvaluateKeysPressed(e, Keys.Up);
                    break;
                default:
                    KeysConnectionSettings.Clear();
                    break;
            }

            if (KeysConnectionSettings.Count == 4 && EvaluateCorrectOrder(Keys.Left, Keys.Right, Keys.Up, Keys.Down))
            {
                OnCallSettingDialog?.Invoke(this, EventArgs.Empty);
            }
        }

        private void EvaluateKeysPressed(Keys aKeyPressed, Keys aPreviousKey)
        {
            if (KeysConnectionSettings.Count < 1)
            {
                return;
            }
            if (KeysConnectionSettings.Peek() == aPreviousKey)
            {
                KeysConnectionSettings.Push(aKeyPressed);
            }
            else
            {
                KeysConnectionSettings.Clear();
            }
        }

        private bool EvaluateCorrectOrder(params Keys[] aKeys)
        {
            bool lReturn = true;

            if (aKeys.Length != KeysConnectionSettings.Count)
            {
                return lReturn;
            }

            for (int i = aKeys.Length - 1; i >= 0; i--)
            {
                if (KeysConnectionSettings.Pop() == aKeys[i])
                {
                    continue;
                }
                else
                {
                    lReturn = false;
                    break;
                }
            }

            return lReturn;
        }

        private void ConnectDialog_DoubleClick(object sender, EventArgs e)
        {
            if (Form.ModifierKeys == (Keys.Shift | Keys.Control))
            {
                OnCallSettingDialog?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public class LoginAccount
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public override string ToString()
        {
            return $"{Email} - {UserName}";
        }
    }


}