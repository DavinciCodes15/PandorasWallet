using Pandora.Client.PandorasWallet.SystemBackup;
using Pandora.Client.PandorasWallet.Dialogs.Contracts;
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
    public partial class NewBackupProcessWizard : Form, IBackupWindow, IRestoreWindow
    {
        public enum WizardPhases
        {
            Initial = 0, Selector = 1, RestoreByWords = 2, RestoreByFile = 3, BackupUsingFile = 4, BackupUsingWords = 5, End = 6
        }

        private bool FIsRestore;

        public bool IsRestore
        {
            get => FIsRestore;
            set
            {
                FIsRestore = value;
                var lTextType = value ? "restore" : "backup";
                lblTypeProcess.Text = string.Concat("You are about to do a wallet ", lTextType);
                lblFinish.Text = $"You have succesfully completed the {lTextType} process.";
            }
        }

        private WizardPhases FPhase;

        public event Func<string[]> On12WordsNeeded;

        public event Action<string> OnBackupByFileNeeded;

        public event ExecuteRestoreDelegate OnExecuteRestore;

        public event EventHandler OnFinishRestore;

        public WizardPhases WizardPhase
        {
            get => FPhase;
            set
            {
                tabStepsControl.SelectTab((int) value);
                FPhase = value;
            }
        }

        public string Words => txtBoxRecoveryWordsRestore.Text;

        public string RestoreFilePath => txtBoxRestoreFilePath.Text;

        public NewBackupProcessWizard(bool aIsRestore = false)
        {
            InitializeComponent();
            IsRestore = aIsRestore;
            Utils.ChangeFontUtil.ChangeDefaultFontFamily(this);
            //Hide tabs in wizard
            tabStepsControl.Appearance = TabAppearance.FlatButtons;
            tabStepsControl.ItemSize = new Size(0, 1);
            tabStepsControl.SizeMode = TabSizeMode.Fixed;

            //Set Initial Phase
            WizardPhase = WizardPhases.Initial;

            //Clear Controls
            txtBoxRecoveryWordsRestore.Text = lblWords.Text = string.Empty;
        }

        public void SetRecoveryPhraseAutoCompleteWords(string[] aSetOfWords)
        {
            if (aSetOfWords == null || aSetOfWords.Length == 0) throw new Exception("Autocomplete list of words can not be empty");
            autoCompleteMenu.Items = aSetOfWords;
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            try
            {
                GoNextPhase();
            }
            catch (Exception ex)
            {
                var lTextType = IsRestore ? "restore" : "backup";
                this.StandardErrorMsgBox($"Error in {lTextType} process", ex.Message);
                GoPreviousPhase();
            }
        }

        private void GoNextPhase()
        {
            switch (WizardPhase)
            {
                case WizardPhases.Initial:
                    WizardPhase = WizardPhases.Selector;
                    btnBack.Enabled = true;
                    break;

                case WizardPhases.Selector:
                    if (IsRestore)
                        WizardPhase = CheckIfByWords() ? WizardPhases.RestoreByWords : WizardPhases.RestoreByFile;
                    else
                        WizardPhase = CheckIfByWords() ? WizardPhases.BackupUsingWords : WizardPhases.BackupUsingFile;
                    break;

                case WizardPhases.RestoreByFile:
                    DoRestoreByFile();
                    DoFinishProcess();
                    break;

                case WizardPhases.RestoreByWords:
                    DoRestoreByWords();
                    DoFinishProcess();
                    break;

                case WizardPhases.BackupUsingFile:
                    DoBackupByFile();
                    DoFinishProcess();
                    break;

                case WizardPhases.BackupUsingWords:
                    DoFinishProcess();
                    break;

                case WizardPhases.End:
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    break;
            }

            if (WizardPhase == WizardPhases.BackupUsingWords)
                DoBackupByWords();
        }

        private void DoFinishProcess()
        {
            WizardPhase = WizardPhases.End;
            lblContinue.Text = "Click finish to exit";
            btnNext.Text = "Finish";
            btnBack.Enabled = false;
            this.FormClosed -= NewBackupProcessWizard_FormClosed;
            this.FormClosed += NewBackupProcessWizard_FormClosed;
        }

        private void NewBackupProcessWizard_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (IsRestore)
            {
                OnFinishRestore?.Invoke(this, EventArgs.Empty);
            }
        }

        private void DoRestoreByFile()
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                OnExecuteRestore.Invoke(true);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void DoRestoreByWords()
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                OnExecuteRestore.Invoke(false);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void DoBackupByFile()
        {
            var lFilePath = txtBoxBackupSelectedDir.Text;
            this.SetWaitCursor();
            try
            {
                OnBackupByFileNeeded.Invoke(lFilePath);
            }
            finally
            {
                this.SetArrowCursor();
            }
        }

        private void DoBackupByWords()
        {
            //  this.SetWaitCursor();
            var lWords = On12WordsNeeded.Invoke();
            try
            {
                lblWords.Text = string.Join(" ", lWords);
            }
            finally
            {
                this.SetArrowCursor();
            }
        }

        private bool CheckIfByWords()
        {
            bool lIsByWord = radioBtnWords.Checked;
            bool lIsByFile = radioBtnFile.Checked;
            if (lIsByWord && lIsByFile) throw new Exception("Error, both options can not be selected at same time");
            return lIsByWord;
        }

        private void GoPreviousPhase()
        {
            switch (WizardPhase)
            {
                case WizardPhases.Selector:
                    WizardPhase = WizardPhases.Initial;
                    btnBack.Enabled = false;
                    break;

                case WizardPhases.RestoreByFile:
                case WizardPhases.RestoreByWords:
                case WizardPhases.BackupUsingFile:
                case WizardPhases.BackupUsingWords:
                    WizardPhase = WizardPhases.Selector;
                    break;
            }
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            try
            {
                GoPreviousPhase();
            }
            catch (Exception ex)
            {
                var lTextType = IsRestore ? "restore" : "backup";
                this.StandardErrorMsgBox($"Error in {lTextType} process", ex.Message);
            }
        }

        private void BtnRestoreOpenFileDialog_Click(object sender, EventArgs e)
        {
            if (openFileDlg.ShowDialog() == DialogResult.OK)
                txtBoxRestoreFilePath.Text = openFileDlg.FileName;
        }

        private void BtnBackupFolder_Click(object sender, EventArgs e)
        {
            DlgFolderBrowser.FileName = $"PWBACKUP_{DateTime.Now.ToString("yyyyMMddTHHmmss")}.bkp";
            if (DlgFolderBrowser.ShowDialog() == DialogResult.OK)
                txtBoxBackupSelectedDir.Text = DlgFolderBrowser.FileName;
        }

        private void BtnCopyWords_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(lblWords.Text);
            WizardToolTip.Show("Copied.", this);
        }

        private void TxtBoxRecoveryWordsRestore_KeyPress(object sender, KeyPressEventArgs e)
        {
            var lLetter = e.KeyChar.ToString();
            if (lLetter != "\b")
                e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(lLetter, "^[a-zA-Z ]*$");
        }

        public bool Execute()
        {
            bool lResult;
            if (ParentWindow == null)
                lResult = this.ShowDialog() == DialogResult.OK;
            else
                lResult = this.ShowDialog((IWin32Window) ParentWindow) == DialogResult.OK;
            return lResult;
        }

        public object ParentWindow { get; set; }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}