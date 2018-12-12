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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Pandora.Client.PandorasWallet.Utils
{
    internal enum KeyInventoryItem
    {
        [Description("GUID")]
        GUIDSeed = 0,

        [Description("Bittrex")]
        BittrexKeyPair = 1
    }

    internal class PandorasEncryptor
    {
        private DirectoryInfo FDataFolder;
        private string FInternalID;
        private string FEncryptedFilePath;
        private SettingsManager FSettingsManager;
        private Guid FUserGuid = Guid.Empty;
        private PandoraKeys.ExchangeKeys[] FExchangeKeys;
        private string FPasswordHash;
        private KeyInventoryItem[] FInventory;
        private string FSalt = null;
        private int FEncryptionEnd;

        public Guid UserGuid => FUserGuid;

        public string[] Inventory
        {
            get
            {
                List<string> lStringInventory = new List<string>();

                if (FInventory == null)
                {
                    FInventory = GetInventoryFromFile(FEncryptedFilePath, out int lInventoryEnd);
                }

                foreach (KeyInventoryItem it in FInventory)
                {
                    lStringInventory.Add(it.GetEnumDescription());
                }

                return lStringInventory.ToArray();
            }
        }

        private static UnicodeEncoding FUE = new UnicodeEncoding();
        private bool FPendingChanges;

        public bool isPasswordSet => File.Exists(FEncryptedFilePath);

        public Tuple<string, string> GetExchangeKeyPair(KeyInventoryItem aInventoryItem)
        {
            if (FExchangeKeys == null)
            {
                throw new InvalidOperationException("Wallet must be decrypted before retrieving key pairs");
            }

            PandoraKeys.ExchangeKeys lPair = FExchangeKeys.Where(x => x.ExchangeName == aInventoryItem.GetEnumDescription()).FirstOrDefault();

            if (lPair != null)
            {
                return new Tuple<string, string>(lPair.Key, lPair.Secret);
            }

            return null;
        }

        public void AddOrReplaceInventoryItem(KeyInventoryItem aItem, params string[] aArgs)
        {
            if (FUserGuid == Guid.Empty)
            {
                throw new InvalidOperationException("To add or replace items the wallet must be decrypted");
            }

            switch (aItem)
            {
                case KeyInventoryItem.GUIDSeed:

                    if (aArgs.Count() != 1)
                    {
                        throw new ArgumentOutOfRangeException(nameof(aArgs), "For GUID only one param is needed");
                    }

                    FUserGuid = ParseGUID(aArgs[0]);

                    break;

                case KeyInventoryItem.BittrexKeyPair:

                    if (aArgs.Count() != 2)
                    {
                        throw new ArgumentOutOfRangeException(nameof(aArgs), "For KeyPairs two params are needed");
                    }

                    PandoraKeys.ExchangeKeys lItem = new PandoraKeys.ExchangeKeys
                    {
                        ExchangeName = aItem.GetEnumDescription(),
                        Key = aArgs[0],
                        Secret = aArgs[1]
                    };

                    PandoraKeys.ExchangeKeys lKeyPair = FExchangeKeys.Where(x => x.ExchangeName == aItem.GetEnumDescription()).FirstOrDefault();

                    if (lKeyPair == null)
                    {
                        List<PandoraKeys.ExchangeKeys> lKeyList = FExchangeKeys.ToList();

                        lKeyList.Add(lItem);

                        FExchangeKeys = lKeyList.ToArray();

                        List<KeyInventoryItem> lInventoryKey = FInventory.ToList();

                        lInventoryKey.Add(aItem);

                        FInventory = lInventoryKey.ToArray();
                    }
                    else
                    {
                        FExchangeKeys[Array.IndexOf(FExchangeKeys, lKeyPair)] = lItem;
                    }

                    break;
            }

            FPendingChanges = true;
        }

        public void OverwriteWallet(string aNewPassword = null)
        {
            if (FUserGuid == Guid.Empty)
            {
                throw new InvalidOperationException("To overwrite wallet first needs to be decrypted");
            }

            if (!FPendingChanges && string.IsNullOrEmpty(aNewPassword))
            {
                return;
            }

            string lTempFilePath = Path.Combine(FDataFolder.FullName, "_" + FInternalID + ".secret");

            PandoraKeys lPandoraKeys = new PandoraKeys { AccountGuid = FUserGuid, ExchangeCredentials = FExchangeKeys, Inventory = FInventory };

            File.Copy(FEncryptedFilePath, lTempFilePath);

            File.Delete(FEncryptedFilePath);

            string lPasswordHash;
            string lSalt;
            if (!string.IsNullOrEmpty(aNewPassword))
            {
                lPasswordHash = GetSecureHash(aNewPassword, out lSalt);
            }
            else
            {
                lPasswordHash = FPasswordHash;
                lSalt = FSalt;
            }

            try
            {
                EncryptIntoFile(FEncryptedFilePath, lPasswordHash, lSalt, lPandoraKeys);
                FPasswordHash = lPasswordHash;
                FSalt = lSalt;
                FPendingChanges = false;
            }
            catch
            {
                File.Copy(lTempFilePath, FEncryptedFilePath);
                throw;
            }
            finally
            {
                File.Delete(lTempFilePath);
            }
        }

        public PandorasEncryptor(DirectoryInfo aDataFolder, string aInternalID, SettingsManager aSettingManager)
        {
            FDataFolder = aDataFolder;
            FInternalID = aInternalID;
            FEncryptedFilePath = Path.Combine(FDataFolder.FullName, FInternalID + ".secret");
            FSettingsManager = aSettingManager;
        }

        public void CreateEncryptedWalletKeys(string aPassword, string aGuid = "")
        {
            if (isPasswordSet)
            {
                throw new Wallet.ClientExceptions.EncryptedKeysException("Encrypted key file already exists");
            }

            Guid lGuid = ParseGUID(aGuid);

            FPasswordHash = GetSecureHash(aPassword, out string lSalt);

            PandoraKeys lKeys = new PandoraKeys { AccountGuid = lGuid, ExchangeCredentials = new PandoraKeys.ExchangeKeys[0], Inventory = new KeyInventoryItem[1] { KeyInventoryItem.GUIDSeed } };

            EncryptIntoFile(FEncryptedFilePath, FPasswordHash, lSalt, lKeys);
        }

        private Guid ParseGUID(string aStringGUID)
        {
            Guid lGuid;

            if (string.IsNullOrEmpty(aStringGUID))
            {
                lGuid = Guid.NewGuid();
            }
            else
            {
                try
                {
                    lGuid = Guid.Parse(aStringGUID);
                }
                catch
                {
                    lGuid = Guid.ParseExact(aStringGUID, "N");
                }
            }

            return lGuid;
        }

        public static string GetSecureHash(string aPassword, out string aSalt)
        {
            byte[] lStringdBytes = Encoding.ASCII.GetBytes(aPassword);

            aSalt = CryptSharp.Crypter.Blowfish.GenerateSalt();

            string lHash = CryptSharp.Crypter.Blowfish.Crypt(aPassword, aSalt);

            return lHash;
        }

        public static string GetSecureHash(string aPassword, string aSalt)
        {
            byte[] lStringdBytes = Encoding.ASCII.GetBytes(aPassword);

            string lHash = CryptSharp.Crypter.Blowfish.Crypt(aPassword, aSalt);

            return lHash;
        }

        ///<summary>
        /// Encrypts a file using Rijndael algorithm.
        ///</summary>
        private void EncryptIntoFile(string aEncryptedFilePath, string aPassword, string aSalt, PandoraKeys aKeys)
        {
            try
            {
                using (MemoryStream fsCrypt = new MemoryStream())
                {
                    Tuple<byte[], byte[]> lkeys = GetCryptoKeyAndIV(aPassword);

                    byte[] lkey = lkeys.Item1;
                    byte[] lIV = lkeys.Item2;

                    BinaryFormatter formatter = new BinaryFormatter();

                    formatter.Serialize(fsCrypt, aKeys);

                    fsCrypt.Seek(0, SeekOrigin.Begin);

                    Aes RMCrypto = Aes.Create();

                    RMCrypto.Padding = PaddingMode.PKCS7;

                    using (FileStream lFile = new FileStream(aEncryptedFilePath, FileMode.Create))
                    using (CryptoStream cs = new CryptoStream(lFile,
                            RMCrypto.CreateEncryptor(lkey, lIV),
                            CryptoStreamMode.Write))
                    {
                        cs.Write(fsCrypt.ToArray(), 0, (int)fsCrypt.Length);
                    }

                    byte[] lSalt = FUE.GetBytes(aSalt);
                    uint lSaltLenght = (uint)lSalt.Length;

                    lSalt = lSalt.Concat(BitConverter.GetBytes(lSaltLenght)).ToArray();

                    using (FileStream stream = new FileStream(aEncryptedFilePath, FileMode.Append))
                    {
                        stream.Write(lSalt, 0, lSalt.Length);
                    }

                    byte[] lInventory;
                    if (aKeys.Inventory == null || !aKeys.Inventory.Any())
                    {
                        lInventory = new byte[0];
                    }
                    else
                    {
                        lInventory = aKeys.Inventory.SelectMany(x => BitConverter.GetBytes((int)x)).ToArray();
                    }

                    uint lInventoryLenght = (uint)lInventory.Length;

                    lInventory = lInventory.Concat(BitConverter.GetBytes(lInventoryLenght)).ToArray();

                    using (FileStream stream = new FileStream(FEncryptedFilePath, FileMode.Append))
                    {
                        stream.Write(lInventory, 0, lInventory.Length);
                    }
                }
            }
            catch
            {
                File.Delete(FEncryptedFilePath);
                throw;
            }
        }

        public string GetSecretRootSeed(string aEmail, string aUsername)
        {
            if (FUserGuid == Guid.Empty)
            {
                throw new Wallet.ClientExceptions.InvalidOperationException("Wallet needs to be decrypt before asking for root seed");
            }

            return Crypto.Currencies.Controls.CurrencyControl.GetCurrencyControl().GenerateRootSeed(aEmail, aUsername, FUserGuid);
        }

        public Tuple<string, string> GetExchangeKeys(string aExchange)
        {
            if (FExchangeKeys == null || !FExchangeKeys.Any())
            {
                return null;
            }

            Tuple<string, string> lItem = FExchangeKeys?.Where(x => x.ExchangeName == aExchange).Select(x => new Tuple<string, string>(x.Key, x.Secret)).FirstOrDefault();

            return lItem;
        }

        public bool CheckPassword(string aPassword)
        {
            return CryptSharp.Crypter.CheckPassword(aPassword, FPasswordHash);
        }

        private string GetSaltFromFile(string aEncryptedFile, out int aEndPosition, int aOffSet)
        {
            string lSalt;

            lSalt = FUE.GetString(GetUncryptedData(aEncryptedFile, out aEndPosition, aOffSet));

            return lSalt;
        }

        private KeyInventoryItem[] GetInventoryFromFile(string aEncryptedFile, out int aEndPosition)
        {
            List<KeyInventoryItem> lInventory = new List<KeyInventoryItem>();

            byte[] lData = GetUncryptedData(aEncryptedFile, out aEndPosition);

            for (int it = 0; it < (lData.Length / 4); it++)
            {
                byte[] lSubArray = new byte[4];

                Array.Copy(lData, (4 * it), lSubArray, 0, 4);

                lInventory.Add((KeyInventoryItem)BitConverter.ToInt32(lSubArray, 0));
            }

            return lInventory.ToArray();
        }

        public byte[] GetUncryptedData(string aEncryptedFile, out int aEndPosition, int aOffSet = 0)
        {
            byte[] lOutArray;

            using (FileStream fs = new FileStream(aEncryptedFile, FileMode.Open))
            using (MemoryStream fsOut = new MemoryStream())
            {
                int lOffset = aOffSet > 0 ? ((-1) * ((int)fs.Length - aOffSet)) - 4 : -4;
                fs.Seek(lOffset, SeekOrigin.End);

                int data;
                while ((data = fs.ReadByte()) != -1)
                {
                    fsOut.WriteByte((byte)data);
                    if (aOffSet > 0 && fs.Position == aOffSet)
                    {
                        break;
                    }
                }

                uint lDataLength = BitConverter.ToUInt32(fsOut.ToArray(), 0);

                fsOut.SetLength(0);

                int lDataPosition = ((-1) * (int)lDataLength) + lOffset;

                aEndPosition = (int)fs.Seek(lDataPosition, SeekOrigin.End);

                while ((data = fs.ReadByte()) != -1)
                {
                    fsOut.WriteByte((byte)data);
                    if (aOffSet > 0 && fs.Position == aOffSet)
                    {
                        break;
                    }
                }

                lOutArray = fsOut.ToArray();
            }

            return lOutArray.Take(lOutArray.Length - 4).ToArray();
        }

        public bool TryToDecryptWalletKey(string aPassword)
        {
            if (string.IsNullOrEmpty(FSalt) || FInventory == null)
            {
                FInventory = GetInventoryFromFile(FEncryptedFilePath, out int lInventoryEnd);
                FSalt = GetSaltFromFile(FEncryptedFilePath, out FEncryptionEnd, lInventoryEnd);
            }

            string lPasswordHash = GetSecureHash(aPassword, FSalt);
            try
            {
                PandoraKeys lKeys = DecryptFile(lPasswordHash, FEncryptedFilePath, FEncryptionEnd);

                FUserGuid = lKeys.AccountGuid;

                FExchangeKeys = lKeys.ExchangeCredentials;

                FPasswordHash = lPasswordHash;
            }
            catch
            {
                return false;
            }

            return true;
        }

        private static Tuple<byte[], byte[]> GetCryptoKeyAndIV(string aPassword)
        {
            byte[] lOriginalkey = FUE.GetBytes(aPassword);

            if (lOriginalkey.Length < 32)
            {
                throw new CryptographicException("Password lenght should be at least 32 bytes");
            }

            byte[] lIV = lOriginalkey.Take(8).ToArray().Concat(lOriginalkey.Skip(lOriginalkey.Length - 8).ToArray()).ToArray();
            byte[] lKey = lOriginalkey.Skip(lOriginalkey.Length - 32).ToArray();

            return new Tuple<byte[], byte[]>(lKey, lIV);
        }

        ///<summary>
        /// Decrypts a file using Rijndael algorithm.
        ///</summary>
        private PandoraKeys DecryptFile(string aPassword, string aEncryptedFile, int aEndPosition)
        {
            Tuple<byte[], byte[]> lkeys = GetCryptoKeyAndIV(aPassword);

            byte[] lkey = lkeys.Item1;
            byte[] lIV = lkeys.Item2;

            int lData;

            PandoraKeys lKeys;

            using (FileStream fsCrypt = new FileStream(aEncryptedFile, FileMode.Open))
            {
                using (MemoryStream MemOut = new MemoryStream())
                {
                    while ((lData = fsCrypt.ReadByte()) != -1)
                    {
                        MemOut.WriteByte((byte)lData);
                        if (fsCrypt.Position == aEndPosition)
                        {
                            break;
                        }
                    }

                    Aes RMCrypto = Aes.Create();

                    RMCrypto.Padding = PaddingMode.PKCS7;

                    MemOut.Seek(0, SeekOrigin.Begin);

                    BinaryFormatter formatter = new BinaryFormatter();

                    using (MemoryStream DecryptOut = new MemoryStream())
                    using (CryptoStream cs = new CryptoStream(MemOut,
                        RMCrypto.CreateDecryptor(lkey, lIV),
                        CryptoStreamMode.Read))
                    {
                        lKeys = (PandoraKeys)formatter.Deserialize(cs);
                    }
                }

                return lKeys;
            }
        }
    }

    [Serializable]
    internal class PandoraKeys
    {
        public Guid AccountGuid { get; set; }
        public ExchangeKeys[] ExchangeCredentials { get; set; }

        [Serializable]
        internal class ExchangeKeys
        {
            public string ExchangeName;
            public string Key;
            public string Secret;
        }

        [NonSerialized]
        public KeyInventoryItem[] Inventory;
    }
}