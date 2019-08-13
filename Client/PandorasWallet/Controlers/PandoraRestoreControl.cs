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

using Pandora.Client.SystemBackup;
using Pandora.Client.PandorasWallet.Dialogs;
using Pandora.Client.PandorasWallet.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet.Controlers
{
    public enum WizzardProcess
    {
        Introduction,
        OptionChoose,
        File,
        TwelveWords,
        Finish
    }

    public delegate void PasscodeEntropyFromWordsDelegate(object sender, uint[] aWordIndex, out string aPasscodeEntropy);

    public class PandoraClientRestoreControl : IDisposable
    {
        private BaseWizzard RestoreWizard { get; set; }

        //private WordList FWordList;
        //private string F12Words;

        public event WizardSendPathDelegate OnRestoreInformationFromFile;

        //public event PasscodeEntropyFromWordsDelegate OnPasscodeEntropyFromWordsIndex;

        public PandoraClientRestoreControl()
        {
            RestoreInitialize();
        }

        public string PasscodeEntropy { get; private set; }

        public bool ExecuteRestore()
        {
            if (OnRestoreInformationFromFile == null)
                throw new InvalidOperationException("You must set the property OnRestoreInformationFromFile first");

            if (OnRestoreInformationFromFile == null)
                throw new InvalidOperationException("You must set the property OnPasscodeEntropyFromWords first");

            return RestoreWizard.Execute();

            //if (RestoreDialog.Restored)
            //{
            //    WalletCreationProcess(!string.IsNullOrEmpty(FPasscode));

            //    StartupExchangeProcess();
            //}
        }

        private void RestoreInitialize()
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
                        RestoreWizard.TwelveWordsLabel = "Step 2. Enter each of the 12 words in the correct order";
                        RestoreWizard.TwelveWordsControl.LabelInformation = "Insure all words are correct";
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
                        throw new Exception("The specified file does not exist");
                    OnRestoreInformationFromFile(this, RestoreWizard.FolderPath);
                    RestoreWizard.FinalLabel = "You have recovered your personal configurations and addressess successfully.";
                    SetupFinishTab(RestoreWizard);
                    RestoreWizard.OnNextBtnClick += RestoreWizard_OnCancelBtnClick;
                    RestoreWizard.SelectedTabIndex = (int)WizzardProcess.Finish;
                    break;

                    //case (int)WizzardProcess.TwelveWords:
                    //    List<string> lWords = new List<string>();

                    //    WordListlWordList = null;// new WordList();
                    //    for (int i = 1; i < 13; i++)
                    //    {
                    //        if (!CheckIfWordIsValid(i, out string lWord))
                    //        {
                    //            return;
                    //        }

                    //        lWords.Add(lWord);
                    //    }
                    //    string s = "";
                    //    OnPasscodeEntropyFromWordsIndex(this, FWordList.GetNumbers(lWords.ToArray()), out string lPasscode);

                    //    PasscodeEntropy = lPasscode;
                    //    //FWorkingWallet.GetHexPassCode(FWordList.GetNumbers(lWords.ToArray()));
                    //    //if (PasscodeEntropy.Length != 32)
                    //    //    throw new Exception("The recovery process has failed please try again using your twelve word backup phrase, restore by file, or contact us");

                    //    RestoreWizard.FinalLabel = "You have recovered your addresses and crypto assets succesfully.";
                    //    SetupFinishTab(RestoreWizard);
                    //    RestoreWizard.OnNextBtnClick += RestoreWizard_OnCancelBtnClick;
                    //    RestoreWizard.SelectedTabIndex = (int)WizzardProcess.Finish;
                    //    break;
            }
        }

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

        private void RestoreWizerd_OnBackBtnClick(object sender, EventArgs e)
        {
            RestoreWizard.NextButtonText = "Next";
            OnBackClick(RestoreWizard);
        }

        private void RestoreWizard_OnCancelBtnClick(object sender, EventArgs e)
        {
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
                RestoreWizard.StandardWarningMsgBox("One or more boxes were empty, try again");
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

        public void Dispose()
        {
            RestoreWizard.Dispose();
            RestoreWizard = null;
        }
    }
}