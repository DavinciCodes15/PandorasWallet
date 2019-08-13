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
using Pandora.Client.PandorasWallet.Dialogs.Contracts;
using Pandora.Client.Universal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.PandorasWallet.SystemBackup
{
    public delegate void SaveBackupFileDelegate(byte[] aFile, string aPath);

    public class BackupController : IDisposable
    {
        public event SaveBackupFileDelegate OnSaveFile;

        public event Func<string> OnRootSeedNeeded;

        public event Func<BasePandoraBackupObject> GetObjectToBackup;

        private IBackupWindow FBackupWindow;

        public BackupController(IBackupWindow aBackupWindow)
        {
            FBackupWindow = aBackupWindow;
            FBackupWindow.On12WordsNeeded += FBackupWindow_OnWordsNeeded;
            FBackupWindow.OnBackupByFileNeeded += FBackupWindow_OnBackupByFileNeeded;
        }

        public bool ExecuteBackup(object aReferenceForm = null)
        {
            FBackupWindow.ParentWindow = aReferenceForm;
            return FBackupWindow.Execute();
        }

        private void FBackupWindow_OnBackupByFileNeeded(string aPathToSaveTo)
        {
            if (string.IsNullOrEmpty(aPathToSaveTo)) throw new Exception("You must provide a valid path to save your backup file");
            var lObjectToBackup = GetObjectToBackup.Invoke();
            var lBackupFileBytes = FileBackupProcessor.GetBackupFileBytes(lObjectToBackup);
            OnSaveFile.Invoke(lBackupFileBytes, aPathToSaveTo);
        }

        private string[] FBackupWindow_OnWordsNeeded()
        {
            try
            {
                var lRootSeed = OnRootSeedNeeded.Invoke();
                var lWords = WordBackupProcessor.GetWordsFromSeed(lRootSeed);
                return lWords;
            }
            catch (Exception ex)
            {
                Log.Write($"Exception thrown during generation of recovery phrase. Exception: {ex}");
                throw new BackupExceptions.BadRecoveryPhrase($"Failed to recover backup phrase. Error: {ex.Message}");
            }
        }

        public void Dispose()
        {
            FBackupWindow.On12WordsNeeded -= FBackupWindow_OnWordsNeeded;
            FBackupWindow.OnBackupByFileNeeded -= FBackupWindow_OnBackupByFileNeeded;
            FBackupWindow = null;
        }
    }
}