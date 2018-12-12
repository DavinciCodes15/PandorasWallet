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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet
{
    public partial class PandoraClientControl
    {
        private WordList FWordList;
        private string FPasscode;
        private string F12Words;

        public enum WizzardProcess
        {
            Introduction,
            OptionChoose,
            File,
            TwelveWords,
            Finish
        }

        partial void RestoreInitialize()
        {
            RestoreWizard = new BaseWizzard
            {
                CopyButtonVisibility = false,
                CopiLabelVisibility = false
            };
            RestoreWizard.OnFileBtnClick += RestoreWizard_OnFileBtnClick;
            RestoreWizard.OnNextBtnClick += RestoreWizerd_OnNextBtnClick;
            RestoreWizard.OnBackBtnClick += RestoreWizerd_OnBackBtnClick;
            RestoreWizard.OnCancelBtnClick += RestoreWizard_OnCancelBtnClick;
            RestoreWizard.OnDialogClosing += RestoreWizard_OnDialogClosing;
            RestoreWizard.OnTextChange += txtFolderTextChange;
            RestoreWizard.TwelveWordsControl.OnTextChange += TwelveWordsControl_OnTextChange;
        }

        private void RestoreWizard_OnDialogClosing(object sender, EventArgs e)
        {
            RestoreWizard.SelectedTabIndex = 0;
            RestoreWizard.CancelVisibility = true;
            RestoreWizard.NextButtonText = "Next";
            RestoreWizard.BackVisibility = true;
            RestoreWizard.OnNextBtnClick -= RestoreWizard_OnCancelBtnClick;
            RestoreWizard.FolderPath = "";
            for (int i = 0; i < 12; i++)
            {
                string lLookingFor = "Word" + (i + 1).ToString();
                PropertyInfo prop = RestoreWizard.TwelveWordsControl.GetType().GetProperty(lLookingFor, BindingFlags.Public | BindingFlags.Instance);
                prop.SetValue(RestoreWizard.TwelveWordsControl, "");
            }
            RestoreWizard.NextButtonEnabled = true;
        }

        private void RestoreDialog_OnRestoreBtnClick(object sender, EventArgs e)
        {
            RestoreDialog.Restored = RestoreWizard.Execute();

            if (RestoreDialog.Restored)
            {
                WalletCreationProcess(!string.IsNullOrEmpty(FPasscode));
                StartupExchangeProcess();
            }
        }

        private void RestoreWizerd_OnNextBtnClick(object sender, EventArgs e)
        {
            switch (RestoreWizard.SelectedTabIndex)
            {
                case (int)WizzardProcess.Introduction:
                    RestoreWizard.SelectedTabIndex = (int)WizzardProcess.OptionChoose;

                    break;

                case (int)WizzardProcess.OptionChoose:
                    if (!RestoreWizard.UseWords)
                    {
                        RestoreWizard.FileLabel = "Step 2. Select your restoration file";
                        if (string.IsNullOrEmpty(RestoreWizard.FolderPath))
                        {
                            RestoreWizard.NextButtonEnabled = false;
                        }
                        else
                        {
                            RestoreWizard.NextButtonText = "Restore";
                            RestoreWizard.NextButtonEnabled = true;
                        }
                        RestoreWizard.SelectedTabIndex = (int)WizzardProcess.File;
                    }
                    else
                    {
                        RestoreWizard.TwelveWordsLabel = "Step 2. Introduce in each box the words";
                        RestoreWizard.TwelveWordsControl.LabelInformation = "Please fill every textbox display";
                        if (!AreTextBoxesEmpty())
                        {
                            RestoreWizard.NextButtonText = "Restore";
                            RestoreWizard.NextButtonEnabled = true;
                        }
                        else
                        {
                            RestoreWizard.NextButtonText = "Next";
                            RestoreWizard.NextButtonEnabled = false;
                        }
                        RestoreWizard.SelectedTabIndex = (int)WizzardProcess.TwelveWords;
                    }
                    break;

                case (int)WizzardProcess.File:
                    if (!File.Exists(RestoreWizard.FolderPath))
                    {
                        throw new Exception("The specified file does not exist");
                    }
                    if (Path.GetExtension(RestoreWizard.FolderPath) != ".bkp")
                    {
                        throw new Exception("This file is not supported");
                    }
                    FWorkingWallet.RestoreInformationFromFile(RestoreWizard.FolderPath);
                    RestoreWizard.FinalLabel = "You recover your personal configurations and addressess successfully";
                    SetupFinishTab(RestoreWizard);
                    RestoreWizard.OnNextBtnClick += RestoreWizard_OnCancelBtnClick;
                    RestoreWizard.SelectedTabIndex = (int)WizzardProcess.Finish;
                    break;

                case (int)WizzardProcess.TwelveWords:
                    List<string> lWords = new List<string>();

                    FWordList = new WordList();
                    for (int i = 1; i < 13; i++)
                    {
                        if (!CheckIfWordIsValid(i, out string lWord))
                        {
                            return;
                        }

                        lWords.Add(lWord);
                    }

                    FPasscode = FWorkingWallet.GetHexPassCode(FWordList.GetNumbers(lWords.ToArray()));
                    if (FPasscode.Length != 32)
                    {
                        throw new Exception("Something went wrong while trying to restore using twelve words, use restore by file or contact us");
                    }

                    RestoreWizard.FinalLabel = "You recover your addresses succesfully";
                    SetupFinishTab(RestoreWizard);
                    RestoreWizard.OnNextBtnClick += RestoreWizard_OnCancelBtnClick;
                    RestoreWizard.SelectedTabIndex = (int)WizzardProcess.Finish;
                    break;
            }
        }

        private void RestoreWizerd_OnBackBtnClick(object sender, EventArgs e)
        {
            RestoreWizard.NextButtonText = "Next";
            OnBackClick(RestoreWizard);
        }

        private void RestoreWizard_OnCancelBtnClick(object sender, EventArgs e)
        {
            RestoreDialog.Close();
            RestoreWizard.Close();
        }

        private void TwelveWordsControl_OnTextChange(object sender, EventArgs e)
        {
            if (!AreTextBoxesEmpty())
            {
                RestoreWizard.NextButtonText = "Restore";
                RestoreWizard.NextButtonEnabled = true;
            }
            else
            {
                RestoreWizard.NextButtonText = "Next";
                RestoreWizard.NextButtonEnabled = false;
            }
        }

        public bool CheckIfWordIsValid(int aIndex, out string aWord)
        {
            aWord = null;

            string lLookingFor = "Word" + aIndex.ToString();

            PropertyInfo prop = RestoreWizard.TwelveWordsControl.GetType().GetProperty(lLookingFor, BindingFlags.Public | BindingFlags.Instance);
            aWord = prop.GetValue(RestoreWizard.TwelveWordsControl, null).ToString();
            if (string.IsNullOrEmpty(aWord))
            {
                RestoreWizard.StandardInfoMsgBox("One or more boxes were empty, try again");
                return false;
            }
            return true;
        }

        public bool AreTextBoxesEmpty()
        {
            for (int i = 1; i < 13; i++)
            {
                string lLookingFor = "Word" + i.ToString();
                PropertyInfo prop = RestoreWizard.TwelveWordsControl.GetType().GetProperty(lLookingFor, BindingFlags.Public | BindingFlags.Instance);
                string lWord = prop.GetValue(RestoreWizard.TwelveWordsControl, null).ToString();
                if (string.IsNullOrEmpty(lWord))
                {
                    return true;
                }
            }
            return false;
        }

        private void txtFolderTextChange(object sender, EventArgs e)
        {
            TextBox ltextBox = sender as TextBox;
            if (!string.IsNullOrEmpty(ltextBox.Text))
            {
                RestoreWizard.NextButtonEnabled = true;
            }
            else
            {
                RestoreWizard.NextButtonEnabled = false;
            }
        }

        private void RestoreWizard_OnFileBtnClick(object sender, EventArgs e)
        {
            OpenFileDialog lOpenFileDialog = new OpenFileDialog
            {
                Filter = "Backup File(*.bkp)|*.bkp"
            };
            if (lOpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                RestoreWizard.FolderPath = lOpenFileDialog.FileName;
            }
        }

        //-----------------------------------------------------BackupProcess-----------------------------------------------------

        partial void BackupInitialize()
        {
            BackupWizard = new BaseWizzard
            {
                StepOneLabel = "Select your backup method",
                WizardName = "BackupWalletWizard",
                FileOptionName = "Backup to a file. \r\nThis option will generate a backup file in a selected destination, \r\nHere you will safe your personal account recovery detail(selected coins, addresses and passwords).  \r\nDO NOT SHARE THIS FILE WITH ANYONE.",
                TwelvesWordsOptionName = "Backup using 12 Words. \r\nThis option will generate 12 random words to recover your coin addresses. \r\nMake sure to write them down or store them in a save place",
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

        private void BackupWizard_OnButtonCopyClick(object sender, EventArgs e)
        {
            Clipboard.SetText(F12Words);
            BackupWizard.CopiLabelVisibility = true;
            Task.Run(() =>
            {
                Thread.Sleep(500);
                BackupWizard.BeginInvoke(new MethodInvoker(delegate () { BackupWizard.CopiLabelVisibility = false; }));
            });
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
                        BackupWizard.FileLabel = "Step 2. Select the destination for your backup file.";
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
                        FWordList = new WordList();
                        BackupWizard.TwelveWordsLabel = "Step 2. Store your 12 words display here";
                        BackupWizard.TwelveWordsControl.ReadOnlyTextBoxes = true;

                        if (!EncryptionPasswordDialog.Execute())
                        {
                            return;
                        }

                        string[] Words = FWordList.GetWords(FWallet.Get12NumbersOf11Bits());

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
                        FWallet.CreateBackup(BackupWizard.FolderPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    BackupWizard.FinalLabel = "You backup your personal configurations and addressess successfully";
                    SetupFinishTab(BackupWizard);
                    BackupWizard.OnNextBtnClick += BackupWizard_OnCancelBtnClick;
                    BackupWizard.SelectedTabIndex = (int)WizzardProcess.Finish;
                    break;

                case (int)WizzardProcess.TwelveWords:
                    BackupWizard.FinalLabel = "You backup your addresses succesfully";
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

        private void BackupWizard_OnCancelBtnClick(object sender, EventArgs e)
        {
            BackupWizard.Close();
        }

        //--------------------------------------------ShareMethods----------------------------------------------

        private void SetupFinishTab(BaseWizzard aBaseWizzard)
        {
            aBaseWizzard.BackVisibility = false;
            aBaseWizzard.CancelVisibility = false;
            aBaseWizzard.NextButtonText = "Finish";
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
    }
}