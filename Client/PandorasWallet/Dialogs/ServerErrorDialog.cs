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
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet.Dialogs
{
    public partial class ServerErrorDialog : Form
    {
        static ServerErrorDialog FServerErrorDialog;

        public static ServerErrorDialog GetInstance()
        {
            if (FServerErrorDialog == null)
                FServerErrorDialog = new ServerErrorDialog();
            return FServerErrorDialog;
        }

        public ServerErrorDialog()
        {
            InitializeComponent();
        }

        public string ErrorTitle { get => textBox2.Text; set => textBox2.Text = value; }

        public string ErrorMessage { get => textBox1.Text; set => textBox1.Text = value; }

        public bool Execute()
        {   
            
            if (ParentWindow == null) 
                return ShowDialog() == DialogResult.OK;
            else
                return ShowDialog(ParentWindow) == DialogResult.OK;
        }

        public IWin32Window ParentWindow { get; set; }
    }
}