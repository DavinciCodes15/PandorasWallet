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
using Pandora.Client.Universal;
using System;
using System.IO;
using System.Linq;
using Pandora.Client.PandorasWallet.Dialogs.Contracts;
using Pandora.Client.SystemBackup.Interfaces;

namespace Pandora.Client.PandorasWallet.SystemBackup
{
    public class RestoreController : IDisposable
    {
        private IRestoreWindow FRestoreWindow;
        private string FEmail;
        private string FUsername;

        public event BackupDelegates.PasscodeEntropyRestoreDelegate OnWalletUncryptPasscodeRestored;

        public event BackupDelegates.ReadFileDelegate OnGetBytesFromFilePath;

        public event BackupDelegates.RestoredPandoraBackupObject OnRecoveryObjectRestored;

        public event EventHandler OnFinishRestore;

        //The reason for this is that this method are already declared in the parent controller, so I just need the reference to them to continue working
        public BackupDelegates.WalletPasswordNeededDelegate PasswordDialogCallMethod { get; set; }

        /// <summary>
        /// Will only have value if a restore by file was performed. Otherwise, will be null
        /// </summary>
        public PandorasBackupObject RestoredObject { get; private set; }

        public RestoreController(IRestoreWindow aRestoreWindow)
        {
            FRestoreWindow = aRestoreWindow;
            FRestoreWindow.OnExecuteRestore += RestoreWindow_OnExecuteRestore;
            FRestoreWindow.OnFinishRestore += FRestoreWindow_OnFinishRestore;
        }

        private void FRestoreWindow_OnFinishRestore(object sender, EventArgs e)
        {
            OnFinishRestore?.Invoke(sender, e);
        }

        public void SetUserData(string aEmail, string aUsername)
        {
            FEmail = aEmail.Replace(" ", string.Empty).ToLower();
            FUsername = aUsername.Replace(" ", string.Empty).ToLower();
        }

        private void RestoreWindow_OnExecuteRestore(bool aIsByFile)
        {
            if (aIsByFile)
                RestoreByFile();
            else
                RestoreByWords();
        }

        private void RestoreByFile()
        {
            try
            {
                string lPath = FRestoreWindow.RestoreFilePath;
                PandorasBackupObject lRestoredFile = GetRestoredFile(lPath);
                if (!lRestoredFile.IsOldBackupData && !VerifyRetoreFileOwnership(lRestoredFile))
                    throw new BackupExceptions.BadRecoveryFile($"Unable to read recovery file {lPath}");
                RestoredObject = lRestoredFile;
                OnRecoveryObjectRestored?.Invoke(lRestoredFile);
            }
            catch (BackupExceptions.BadRecoveryFile)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Error, $"Exception during RestoreByFileProcess. Exception: {ex}");
                throw ex;
            }
        }

        private PandorasBackupObject GetRestoredFile(string aFilePath)
        {
            if (!File.Exists(aFilePath)) throw new Exception($"Invalid restore file path {aFilePath}");
            byte[] lFileBytes = OnGetBytesFromFilePath.Invoke(aFilePath);
            PandorasBackupObject lRestoredFile = null;
            var lBackupSerializableTypes = GetBackupSerializableClasses();
            try
            {
                lRestoredFile = new PandorasBackupObject(FileBackupProcessor.GetRetoreObjectFromBytes<BasePandoraBackupObject, BackupSerializableObject>(lFileBytes));
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Warning, $"Failed to retore from file. Trying to open it with old backup style. Path: {aFilePath}. Exception thrown: {ex}");
                if (!TryRestoreFromOldBackupFile(lFileBytes, out lRestoredFile))
                    lRestoredFile = null;
            }
            if (lRestoredFile == null || (lRestoredFile.IsOldBackupData && lRestoredFile.ExchangeKeys == null) || (!lRestoredFile.IsOldBackupData && lRestoredFile.WalletData == null))
                throw new BackupExceptions.BadRecoveryFile($"Error reading recovery file {aFilePath}");
            return lRestoredFile;
        }

        private bool VerifyRetoreFileOwnership(BasePandoraBackupObject aPandoraBackupObject)
        {
            bool lResult = false;
            string lEmail = aPandoraBackupObject.OwnerData[0].Replace(" ", string.Empty).ToLower();
            string lUsername = aPandoraBackupObject.OwnerData[1].Replace(" ", string.Empty).ToLower(); ;
            if (lEmail != FEmail || lUsername != FUsername) lResult = true;
            return lResult;
        }

        private string PromptUserForPassword(PasswordValidationDelegate aVerificationDelegate)
        {
            return PasswordDialogCallMethod?.Invoke(aVerificationDelegate);
        }

        private Type[] GetBackupSerializableClasses()
        {
            var lTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(lAssembly => lAssembly.GetTypes())
                .Where(x => typeof(IBackupSerializable).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .ToArray();
            if (lTypes.Length == 0)
                lTypes = new Type[1] { typeof(BackupSerializableObject) };
            return lTypes;
        }

        private bool TryRestoreFromOldBackupFile(byte[] aOldBackupFile, out PandorasBackupObject aResultantBackupObject)
        {
            bool lResult = false;
            aResultantBackupObject = null;
            var lOldRestoreManager = new OldBackupFileConverter();
            lOldRestoreManager.OnOldWalletPasswordNeeded += PromptUserForPassword; ;
            try
            {
                aResultantBackupObject = lOldRestoreManager.ConvertOldFileToNewBackupObject(aOldBackupFile);
                lResult = true;
            }
            catch
            {
                lResult = false;
            }
            return lResult;
        }

        private void RestoreByWords()
        {
            string lWords = FRestoreWindow.Words;
            if (string.IsNullOrEmpty(lWords)) throw new Exception("Empty recovery phrase");
            string[] lSplitedWords = lWords.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            string lPasscode = WordBackupProcessor.GetSeedFromWords(lSplitedWords);
            OnWalletUncryptPasscodeRestored.Invoke(lPasscode);
        }

        public bool ExecuteRestore(object aReferenceForm = null)
        {
            if (string.IsNullOrEmpty(FUsername) || string.IsNullOrEmpty(FEmail)) throw new Exception("Before executing restore wizard you must set username and email into Restore controller");
            if (PasswordDialogCallMethod == null) throw new Exception("You must set PasswordDialogCallMethod and PasswordHashCheckMethod before executing retore process");
            FRestoreWindow.ParentWindow = aReferenceForm;
            return FRestoreWindow.Execute();
        }

        public void Dispose()
        {
            FRestoreWindow.OnExecuteRestore -= RestoreWindow_OnExecuteRestore;
            FRestoreWindow = null;
            RestoredObject = null;
        }
    }
}