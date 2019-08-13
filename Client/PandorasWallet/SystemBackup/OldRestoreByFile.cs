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


using Newtonsoft.Json.Linq;
using Pandora.Client.PandorasWallet.Utils;
using Pandora.Client.Universal;
using System;
using System.Collections.Generic;
using System.IO;

namespace Pandora.Client.PandorasWallet.SystemBackup
{
    public class OldBackupFileConverter
    {
        public event BackupDelegates.WalletPasswordNeededDelegate OnOldWalletPasswordNeeded;

        public PandorasBackupObject ConvertOldFileToNewBackupObject(byte[] aFileByteArray)
        {
            var lOldBackupObject = RestoreInformationFromOldBackupFile(aFileByteArray);
            var lResultObject = new PandorasBackupObject { IsOldBackupData = true };
            //Read SecretFile
            try
            {
                ProcessOldSecretFile(lOldBackupObject.Secret, out string lPasscode, out Dictionary<string, string[]> lExchangeKeys, out string aUserPassword);
                lResultObject.WalletSeeds = new string[1] { lPasscode };
                lResultObject.ExchangeKeys = lExchangeKeys;
                lResultObject.PasswordHash = aUserPassword;
            }
            catch (Exception ex)
            {
                Log.Write($"Error reading old secret file. Exception: {ex}");
                throw;
            }

            //Read settings file
            try
            {
                ProcessOldSettingsFile(lOldBackupObject.Setting, out long lDefaultCoin, out long[] lSelectedCoins);
                lResultObject.DefaultCoinID = lDefaultCoin;
                lResultObject.SelectedCoins = lSelectedCoins;
            }
            catch (Exception ex)
            {
                Log.Write($"Error reading old settings file. Exception: {ex}");
                lResultObject.DefaultCoinID = 1;
                lResultObject.SelectedCoins = new long[1] { 1 };
            }

            //Load .exchange file to object
            lResultObject.ExchangeData = new BackupSerializableObject() { Data = lOldBackupObject.Exchange };
            return lResultObject;
        }

        private void ProcessOldSecretFile(byte[] aOldSecretFile, out string aPasscode, out Dictionary<string, string[]> aExchangeKeys, out string aUserPassword)
        {
            var lOldSecretReader = new OldSecretReader(aOldSecretFile);
            var lValidation = new PasswordValidationDelegate((aPassword) => lOldSecretReader.DecryptWalletKey(aPassword));
            aUserPassword = OnOldWalletPasswordNeeded.Invoke(lValidation);
            aExchangeKeys = new Dictionary<string, string[]>();
            aPasscode = lOldSecretReader.PasscodeEntropy.ToString("N");
            foreach (var lExchangeKey in lOldSecretReader.FExchangeKeys)
                aExchangeKeys.Add(lExchangeKey.ExchangeName, new string[2] { lExchangeKey.Key, lExchangeKey.Secret });
        }

        private void ProcessOldSettingsFile(byte[] aSettingsFileByteArray, out long lDefaultCoin, out long[] lSelectedCoins)
        {
            var lSettingManager = new OldSettingsReader(aSettingsFileByteArray);
            if (!lSettingManager.GetSettings(out Dictionary<string, object> lOldSettings) || lOldSettings == null) throw new Exception("Failed to read settings file");
            lDefaultCoin = (long)lOldSettings["DefaultCoin"];
            lSelectedCoins = ((JToken)lOldSettings["SelectedCoins"]).ToObject<long[]>();
        }

        private OldBackupAuxiliar RestoreInformationFromOldBackupFile(byte[] aOldRestoreFile)
        {
            OldBackupAuxiliar lOldBackupObject = new OldBackupAuxiliar();

            using (MemoryStream lMemoryStream = new MemoryStream(aOldRestoreFile))
            {
                using (BinaryReader lBinaryReader = new BinaryReader(lMemoryStream))
                {
                    lOldBackupObject.Version = lBinaryReader.ReadInt32();
                    if (lOldBackupObject.Version < 111 || lOldBackupObject.Version > 112) throw new Exception("Bad old recovery file");
                    lOldBackupObject.LengthSecret = lBinaryReader.ReadInt32();
                    lOldBackupObject.Secret = lBinaryReader.ReadBytes(lOldBackupObject.LengthSecret);
                    lOldBackupObject.LengthSetting = lBinaryReader.ReadInt32();
                    lOldBackupObject.Setting = lBinaryReader.ReadBytes(lOldBackupObject.LengthSetting);
                    if (lOldBackupObject.Version >= 112)
                    {
                        lOldBackupObject.LengthExchange = lBinaryReader.ReadInt32();
                        lOldBackupObject.Exchange = lBinaryReader.ReadBytes(lOldBackupObject.LengthExchange);
                    }
                }
            }
            return lOldBackupObject;
        }

        private class OldBackupAuxiliar
        {
            public int Version;
            public byte[] Secret;
            public byte[] Setting;

            public byte[] Exchange;

            public int LengthSetting;
            public int LengthSecret;

            public int LengthExchange;
        }
    }
}