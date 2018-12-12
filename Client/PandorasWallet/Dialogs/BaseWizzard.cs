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
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet.Dialogs
{
    public partial class BaseWizzard : Form
    {
        public event EventHandler OnDialogClosing;

        public event EventHandler OnNextBtnClick;

        public event EventHandler OnBackBtnClick;

        public event EventHandler OnTabSelectionChanged;

        public event EventHandler OnCancelBtnClick;

        public event EventHandler OnTextChange;

        public event EventHandler OnFileBtnClick;

        public event EventHandler OnButtonCopyClick;

        public bool CopyButtonVisibility { set => btnCopy.Visible = value; }

        public bool CopiLabelVisibility { set => lblCopy.Visible = value; }
        public string WizardName { set => Text = value; }
        public int SelectedTabIndex { get => tabControl1.SelectedIndex; set => tabControl1.SelectedIndex = value; }

        public string FileOptionName { set => rbtnFile.Text = value; }

        public string IntroductionLabel { set => lblIntroductionText.Text = value; }

        public string StepOneLabel { set => lblStep1.Text = value; }

        public string TwelvesWordsOptionName { set => rbtnWords.Text = value; }

        public string NextButtonText { get => btnNext.Text; set => btnNext.Text = value; }

        public bool NextButtonEnabled { get => btnNext.Enabled; set => btnNext.Enabled = value; }

        public bool CancelButtonEnabled { get => btnCancel.Enabled; set => btnCancel.Enabled = value; }

        public bool UseFile { get => rbtnFile.Checked; set => rbtnFile.Checked = value; }

        public Control.ControlCollection TabControls => tabControl1.SelectedTab.Controls;

        public string FinalLabel { get => lbl1.Text; set => lbl1.Text = value; }

        public bool CancelVisibility { get => btnCancel.Visible; set => btnCancel.Visible = value; }

        public bool BackVisibility { get => btnBack.Visible; set => btnBack.Visible = value; }

        public string FileLabel { get => lblFile.Text; set => lblFile.Text = value; }

        public string TwelveWordsLabel { get => lblTwelveWords.Text; set => lblTwelveWords.Text = value; }

        public TwelveWords TwelveWordsControl => twelveWords1;

        public string FolderPath
        {
            get => txtFile?.Text;
            set
            {
                if (txtFile != null)
                {
                    txtFile.Text = value;
                }
            }
        }

        public bool UseWords { get => rbtnWords.Checked; set => rbtnWords.Checked = value; }

        public BaseWizzard()
        {
            InitializeComponent();
            tabControl1.Region = new Region(tabControl1.DisplayRectangle);
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            try
            {
                OnNextBtnClick?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
                return;
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            try
            {
                OnBackBtnClick?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
            }
        }

        protected void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                OnTabSelectionChanged?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                OnCancelBtnClick?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
            }
        }

        protected void tabControl1_Load(object sender, EventArgs e)
        {
        }

        public bool Execute()
        {
            return ShowDialog() == DialogResult.OK;
        }

        protected void RestoreWalletWizard_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (NextButtonText == "Finish")
            {
                DialogResult = DialogResult.OK;
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        protected void FileButton_Click(object sender, EventArgs e)
        {
            try
            {
                OnFileBtnClick?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
            }
        }

        private void txtFile_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OnTextChange?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
            }
        }

        private void BaseWizzard_Shown(object sender, EventArgs e)
        {
            try
            {
                OnDialogClosing?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            try
            {
                OnButtonCopyClick?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                this.StandardErrorMsgBox(ex.Message);
            }
        }
    }
}