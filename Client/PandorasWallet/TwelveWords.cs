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
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet
{
    public partial class TwelveWords : UserControl
    {
        public TwelveWords()
        {
            InitializeComponent();
        }

        public event EventHandler OnTextChange;

        public bool ReadOnlyTextBoxes
        {
            set
            {
                txtWord1.ReadOnly = value;
                txtWord2.ReadOnly = value;
                txtWord3.ReadOnly = value;
                txtWord4.ReadOnly = value;
                txtWord5.ReadOnly = value;
                txtWord6.ReadOnly = value;
                txtWord7.ReadOnly = value;
                txtWord8.ReadOnly = value;
                txtWord9.ReadOnly = value;
                txtWord10.ReadOnly = value;
                txtWord11.ReadOnly = value;
                txtWord12.ReadOnly = value;
            }
        }

        public string LabelInformation
        {
            set => lblImportantInformation.Text = value;
        }

        public string Word1 { get => txtWord1.Text; set => txtWord1.Text = value; }
        public string Word2 { get => txtWord2.Text; set => txtWord2.Text = value; }
        public string Word3 { get => txtWord3.Text; set => txtWord3.Text = value; }
        public string Word4 { get => txtWord4.Text; set => txtWord4.Text = value; }
        public string Word5 { get => txtWord5.Text; set => txtWord5.Text = value; }
        public string Word6 { get => txtWord6.Text; set => txtWord6.Text = value; }
        public string Word7 { get => txtWord7.Text; set => txtWord7.Text = value; }
        public string Word8 { get => txtWord8.Text; set => txtWord8.Text = value; }
        public string Word9 { get => txtWord9.Text; set => txtWord9.Text = value; }
        public string Word10 { get => txtWord10.Text; set => txtWord10.Text = value; }
        public string Word11 { get => txtWord11.Text; set => txtWord11.Text = value; }
        public string Word12 { get => txtWord12.Text; set => txtWord12.Text = value; }

        private void WordTextBoxes_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OnTextChange?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void txtBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox lTextBox = sender as TextBox;
            e.Handled = !(char.IsLetter(e.KeyChar) || e.KeyChar == (char)Keys.Back);
            if (e.Handled && !lTextBox.ReadOnly)
            {
                MessageBox.Show("This textbox accepts only alphabetical characters");
            }
        }
    }
}