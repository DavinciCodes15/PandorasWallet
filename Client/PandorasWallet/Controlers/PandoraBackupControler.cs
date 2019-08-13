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

using Pandora.Client.PandorasWallet.Dialogs;
using Pandora.Client.PandorasWallet.Utils;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet.Controlers
{
    public delegate void WizardSendPathDelegate(object sender, string aPath);

    internal class PandoraBackupControler : IDisposable
    {
        private BaseWizzard BackupWizard { get; set; }
        private string F12Words;

        public event WizardSendPathDelegate OnCreateBackup;

        public uint[] PasscodeEntrapyAs12Numbers { get; private set; }

        public PandoraBackupControler()
        {
            BackupInitialize();
        }

        private void BackupInitialize()
        {
            BackupWizard = new BaseWizzard
            {
                StepOneLabel = "Select your backup method",
                WizardName = "BackupWalletWizard",
                FileOptionName = "Backup to a file. \r\nThis option will generate a backup file in a selected destination, \r\nHere you will save your personal account recovery detail(selected coins, addresses and passwords).  \r\nDO NOT SHARE THIS FILE WITH ANYONE.",
                TwelvesWordsOptionName = "Backup using 12 Words. \r\nThis option will generate 12 random words to recover your coin addresses. \r\nMake sure to write them down or store them in a safe place",
                IntroductionLabel = "This wizard will help you backup your wallet",
                CopyButtonVisibility = true
            };
            BackupWizard.OnDialogClosing += BackupWizard_OnDialogClosing;
            BackupWizard.OnFileBtnClick += BackupWizard_OnFileBtnClick;
            BackupWizard.OnNextBtnClick += BackupWizard_OnNextBtnClick;
            BackupWizard.OnBackBtnClick += BackupWizard_OnBackBtnClick;
            BackupWizard.OnTextChange += txtFolderBackupTextChange;
            BackupWizard.OnCancelBtnClick += BackupWizard_OnCancelBtnClick;
            BackupWizard.OnButtonCopyClick += BackupWizard_OnButtonCopyClick;
        }

        public bool Execute()
        {
            if (PasscodeEntrapyAs12Numbers == null || PasscodeEntrapyAs12Numbers.Length == 12)
                throw new InvalidOperationException("You must set the property PasscodeEntrapyAs12Numbers first");
            if (OnCreateBackup == null)
                throw new InvalidOperationException("You must set the property OnCreateBackup first");
            return BackupWizard.Execute();
        }

        private void BackupWizard_OnButtonCopyClick(object sender, EventArgs e)
        {
            Clipboard.SetText(F12Words);
            BackupWizard.CopiLabelVisibility = true;
            //Task.Run(() =>
            //{
            //    Thread.Sleep(500);
            //    BackupWizard.BeginInvoke(new MethodInvoker(delegate () { BackupWizard.CopiLabelVisibility = false; }));
            //});
        }

        private void BackupWizard_OnDialogClosing(object sender, EventArgs e)
        {
            BackupWizard.SelectedTabIndex = 0;
            BackupWizard.CancelVisibility = true;
            BackupWizard.NextButtonText = "Next";
            BackupWizard.BackVisibility = true;
            BackupWizard.FolderPath = "";
            BackupWizard.OnNextBtnClick -= BackupWizard_OnCancelBtnClick;
            BackupWizard.NextButtonEnabled = true;
        }

        private void BackupWizard_OnNextBtnClick(object sender, EventArgs e)
        {
            switch (BackupWizard.SelectedTabIndex)
            {
                case (int)WizzardProcess.Introduction:
                    BackupWizard.SelectedTabIndex = (int)WizzardProcess.OptionChoose;

                    break;

                case (int)WizzardProcess.OptionChoose:
                    if (!BackupWizard.UseWords)
                    {
                        BackupWizard.FileLabel = "Step 2. Select the destination folder for your backup file.";
                        if (string.IsNullOrEmpty(BackupWizard.FolderPath))
                        {
                            BackupWizard.NextButtonEnabled = false;
                        }
                        else
                        {
                            BackupWizard.NextButtonText = "Backup";
                            BackupWizard.NextButtonEnabled = true;
                        }
                        BackupWizard.SelectedTabIndex = (int)WizzardProcess.File;
                    }
                    else
                    {
                        F12Words = "";
                        BackupWizard.CopiLabelVisibility = false;
                        BackupWizard.TwelveWordsLabel = "Step 2. Store your 12 words displayed here";
                        BackupWizard.TwelveWordsControl.ReadOnlyTextBoxes = true;
                        string[] Words = null; // FWordList.GetWords(PasscodeEntrapyAs12Numbers);

                        for (int i = 0; i < 12; i++)
                        {
                            string lLookingFor = "Word" + (i + 1).ToString();

                            PropertyInfo prop = BackupWizard.TwelveWordsControl.GetType().GetProperty(lLookingFor, BindingFlags.Public | BindingFlags.Instance);
                            prop.SetValue(BackupWizard.TwelveWordsControl, Words[i]);

                            if (string.IsNullOrEmpty(F12Words))
                            {
                                F12Words = Words[i];
                            }
                            else
                            {
                                if (i % 4 == 0)
                                {
                                    F12Words = string.Format("{0}{1}{2}", F12Words, Environment.NewLine, Words[i]);
                                }
                                else
                                {
                                    F12Words = string.Format("{0}, {1}", F12Words, Words[i]);
                                }
                            }
                        }

                        BackupWizard.SelectedTabIndex = (int)WizzardProcess.TwelveWords;
                    }
                    break;

                case (int)WizzardProcess.File:
                    string lBackupFile = Path.ChangeExtension(BackupWizard.FolderPath, ".bkp");
                    try
                    {
                        OnCreateBackup(this, lBackupFile);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    BackupWizard.FinalLabel = "You have backed up your personal configurations and addressess successfully";
                    SetupFinishTab(BackupWizard);
                    BackupWizard.OnNextBtnClick += BackupWizard_OnCancelBtnClick;
                    BackupWizard.SelectedTabIndex = (int)WizzardProcess.Finish;
                    break;

                case (int)WizzardProcess.TwelveWords:
                    BackupWizard.FinalLabel = "You have backed up your addresses succesfully";
                    SetupFinishTab(BackupWizard);
                    BackupWizard.OnNextBtnClick += BackupWizard_OnCancelBtnClick;
                    BackupWizard.SelectedTabIndex = (int)WizzardProcess.Finish;
                    break;
            }
        }

        private void BackupWizard_OnBackBtnClick(object sender, EventArgs e)
        {
            BackupWizard.NextButtonText = "Next";
            OnBackClick(BackupWizard);
        }

        private void SetupFinishTab(BaseWizzard aBaseWizzard)
        {
            aBaseWizzard.BackVisibility = false;
            aBaseWizzard.CancelVisibility = false;
            aBaseWizzard.NextButtonText = "Finish";
        }

        private void BackupWizard_OnFileBtnClick(object sender, EventArgs e)
        {
            SaveFileDialog lSaveFileDialog = new SaveFileDialog
            {
                Filter = "Backup File (*.bkp)|*.bkp",
                DefaultExt = "bkp",
                AddExtension = true
            };
            if (lSaveFileDialog.ShowDialog() == DialogResult.OK)
            {
                BackupWizard.FolderPath = lSaveFileDialog.FileName;
            }
        }

        private void txtFolderBackupTextChange(object sender, EventArgs e)
        {
            TextBox ltextBox = sender as TextBox;
            if (!string.IsNullOrEmpty(ltextBox.Text))
            {
                BackupWizard.NextButtonEnabled = true;
                BackupWizard.NextButtonText = "Backup";
            }
            else
            {
                BackupWizard.NextButtonEnabled = false;
                BackupWizard.NextButtonText = "Next";
            }
        }

        private void OnBackClick(BaseWizzard aBaseWizard)
        {
            switch (aBaseWizard.SelectedTabIndex)
            {
                case (int)WizzardProcess.OptionChoose:
                    aBaseWizard.SelectedTabIndex = (int)WizzardProcess.Introduction;
                    break;

                case (int)WizzardProcess.File:
                    aBaseWizard.NextButtonEnabled = true;
                    aBaseWizard.CancelButtonEnabled = true;
                    aBaseWizard.SelectedTabIndex = (int)WizzardProcess.OptionChoose;
                    break;

                case (int)WizzardProcess.TwelveWords:
                    aBaseWizard.NextButtonEnabled = true;
                    aBaseWizard.CancelButtonEnabled = true;
                    aBaseWizard.SelectedTabIndex = (int)WizzardProcess.OptionChoose;
                    break;
            }
        }

        private void BackupWizard_OnCancelBtnClick(object sender, EventArgs e)
        {
            BackupWizard.Close();
        }

        public void Dispose()
        {
            BackupWizard.Dispose();
            BackupWizard = null;
        }
    }
}