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
using System.Drawing;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet.Dialogs
{
    public partial class ServerErrorDialog : Form
    {
        public static ServerErrorDialog GetInstance()
        { 
            return new ServerErrorDialog();
        }

        public ServerErrorDialog()
        {
            InitializeComponent();
            Utils.ChangeFontUtil.ChangeDefaultFontFamily(this);
            Shown += ServerErrorDialog_Shown;
        }

        private void ServerErrorDialog_Shown(object sender, System.EventArgs e)
        {
            HideDetails();

            if (lblTitle.Height > 25)
            {
                var lNewSize = (lblTitle.Height - 25);
                this.Height += lNewSize;
                lblMessageEx.Location = new Point(lblMessageEx.Location.X, lblMessageEx.Location.Y + lNewSize);
            }

            if (lblMessageEx.Height > 17)
            {
                var lNewSize = (lblMessageEx.Height - 17);
                this.Height += lNewSize;
            }
        }

        public string ErrorTitle { get => lblTitle.Text; set => lblTitle.Text = value; }

        public string ErrorDetails { get => textBox1.Text; set => textBox1.Text = value; }

        public string ErrorMessage { get => lblMessageEx.Text; set => lblMessageEx.Text = value; }

        public bool Execute()
        {

            if (ParentWindow == null)
                return ShowDialog() == DialogResult.OK;
            else
                return ShowDialog(ParentWindow) == DialogResult.OK;
        }

        public IWin32Window ParentWindow { get; set; }

        private void HideDetails()
        {
            if (textBox1.Visible)
            {
                textBox1.Visible = false;
                Height = Height - textBox1.Height;
                btnShowDetails.Text = "More Details...";
                lblCopied.Visible = false;
                pbCopy.Visible = false;
            }
        }

        private void ShowDetails()
        {
            if (!textBox1.Visible)
            {
                textBox1.Visible = true;
                Height = Height + textBox1.Height;
                btnShowDetails.Text = "Less Details...";
                pbCopy.Visible = true;
            }
        }

        private void PbCopy_Click(object sender, System.EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ErrorDetails))
            {
                Clipboard.SetText(ErrorDetails);
                lblCopied.Visible = true;
            }
        }

        private void BtnShowDetails_Click(object sender, System.EventArgs e)
        {
            if (textBox1.Visible)
            {
                HideDetails();
            }
            else
            {
                ShowDetails();
            }
        }
    }
}