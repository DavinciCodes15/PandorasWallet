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

using Pandora.Client.Crypto.Currencies;
using Pandora.Client.Universal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Pandora.Client.PandorasWallet.Wallet
{
    internal interface IWalletEncryptionContext
    {
        string MasterPasswordHash { get; }
        string AlternatePasswordHash { get; }
        string EncryptedRootSeed { get; }
        string EncryptedMasterPassword { get; }
        bool IsWalletInitialized { get; }
        bool IsAltPasswordSet { get; }

        string GetSalt();
    }

    internal class KeyManager
    {
        private string FStoredMasterPassword; // This value is encrypted for security
        private readonly WalletEncryptionContext FWalletEncryptionData;

        public bool Unlocked => !string.IsNullOrEmpty(FStoredMasterPassword);

        /// <summary>
        /// This object manages all crypto fucntions and keeps the
        /// key data encrypted until needed for use.
        /// </summary>
        /// <param name="aSalt"></param>
        public KeyManager(WalletEncryptionContext aEncryptionData = null)
        {
            FWalletEncryptionData = aEncryptionData ?? WalletEncryptionContext.GetNewInstance();
        }

        public IWalletEncryptionContext GetCurrentEncryptionContext()
        {
            return FWalletEncryptionData;
        }

        public bool TryUnlock(string aPassword)
        {
            string lMasterPassword = aPassword;
            if (FWalletEncryptionData.IsAltPasswordSet && ComparePasswordToHash(aPassword, FWalletEncryptionData.AlternatePasswordHash))
                lMasterPassword = Encryption.DecryptText(FWalletEncryptionData.EncryptedMasterPassword, aPassword);
            if (ComparePasswordToHash(lMasterPassword, FWalletEncryptionData.MasterPasswordHash))
                FStoredMasterPassword = Encryption.EncryptText(lMasterPassword, FWalletEncryptionData.GetSalt());
            return Unlocked;
        }

        private string GetStoredMasterPassword()
        {
            if (!Unlocked) throw new InvalidOperationException("Key manager is not unlocked.");
            return Encryption.DecryptText(FStoredMasterPassword, FWalletEncryptionData.GetSalt());
        }

        public IWalletEncryptionContext EncryptAndSetNewRootSeed(byte[] aRootSeed, string aPassword)
        {
            if (aRootSeed == null || aRootSeed.Length != 32) throw new ArgumentException("Invalid root seed size.");
            if (string.IsNullOrEmpty(aPassword) || !ValidatePassword(aPassword)) throw new ArgumentException("Invalid password");
            var lSalt = FWalletEncryptionData.GetSalt();
            var lBase64RootSeed = Convert.ToBase64String(aRootSeed);
            FWalletEncryptionData.EncryptedRootSeed = Encryption.EncryptText(lBase64RootSeed, string.Concat(aPassword, lSalt));
            FWalletEncryptionData.EncryptedMasterPassword = null;
            FWalletEncryptionData.MasterPasswordHash = HashPassword(aPassword);
            FWalletEncryptionData.AlternatePasswordHash = null;
            if (!TryUnlock(aPassword))
                throw new Exception("There was an error trying to set provided Root Seed. Please contact support");
            return FWalletEncryptionData;
        }

        public string EncryptText(string aText)
        {
            return Encryption.EncryptText(aText, string.Concat(FWalletEncryptionData.GetSalt(), GetStoredMasterPassword()));
        }

        public string DecryptText(string aText)
        {
            return Encryption.DecryptText(aText, string.Concat(FWalletEncryptionData.GetSalt(), GetStoredMasterPassword()));
        }

        private byte[] GetByteRootSeed()
        {
            var lDescryptedRootSeed = Encryption.DecryptText(FWalletEncryptionData.EncryptedRootSeed, string.Concat(GetStoredMasterPassword(), FWalletEncryptionData.GetSalt()));
            return Convert.FromBase64String(lDescryptedRootSeed);
        }

        public IWalletEncryptionContext SetAltenatePassword(string aPassword)
        {
            if (string.IsNullOrEmpty(aPassword) || !ValidatePassword(aPassword)) throw new ArgumentException("Invalid password");
            var lMasterPassword = GetStoredMasterPassword();
            var lEncryptedMasterPassword = Encryption.EncryptText(lMasterPassword, aPassword);
            var lAltPasswordHash = HashPassword(aPassword);
            FWalletEncryptionData.AlternatePasswordHash = lAltPasswordHash;
            FWalletEncryptionData.EncryptedMasterPassword = lEncryptedMasterPassword;
            return FWalletEncryptionData;
        }

        public string GetStringRootSeed()
        {
            return BitConverter.ToString(GetByteRootSeed()).Replace("-", string.Empty).ToLower();
        }

        public ICryptoCurrencyAdvocacy GetCurrencyAdvocacy(long aCurrencyId, ChainParams aChainParams)
        {
            ICryptoCurrencyAdvocacy lAdvocacy = CurrencyControl.GetClientCurrencyAdvocacy(aCurrencyId, aChainParams, GetStringRootSeed);
            return lAdvocacy;
        }

        internal IWalletEncryptionContext UpdatePassword(string lNewPassword)
        {
            var lRootSeed = GetByteRootSeed();
            return EncryptAndSetNewRootSeed(lRootSeed, lNewPassword);
        }

        public static byte[] ConvertOldRootSeedToNew(byte[] aOldRootSeed)
        {
            if (aOldRootSeed == null || aOldRootSeed.Length != 16) throw new ArgumentException("Invalid OLD root seed size.");
            var lNewBytes = aOldRootSeed.Concat(GenerateRootSeed(16));
            return lNewBytes;
        }

        public static string CreateInstanceId(string aEmail, string aUserName)
        {
            string lUserNameEmailData = string.Concat(aUserName, aEmail);
            // Use input string to calculate MD5 hash
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(lUserNameEmailData);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public static byte[] GenerateRootSeed(int aSize = 32)
        {
            var lData = new byte[aSize];
            var lRandomNumberGenerator = RandomNumberGenerator.Create();
            lRandomNumberGenerator.GetBytes(lData);
            return lData;
        }

        private static CryptSharp.Crypter GetCrypter()
        {
            return CryptSharp.Crypter.Blowfish;
        }

        private bool ComparePasswordToHash(string aPassword, string aPasswordHash)
        {
            return CryptSharp.Crypter.CheckPassword(aPassword, aPasswordHash);
        }

        public bool CheckPassword(string aPassword)
        {
            return FWalletEncryptionData.IsWalletInitialized
                && (ComparePasswordToHash(aPassword, FWalletEncryptionData.MasterPasswordHash)
                || (FWalletEncryptionData.IsAltPasswordSet && ComparePasswordToHash(aPassword, FWalletEncryptionData.AlternatePasswordHash)));
        }

        public static bool ValidatePassword(string aPassword)
        {
            return !string.IsNullOrEmpty(aPassword) && aPassword.Length > 8;
        }

        public static string HashPassword(string aDataString)
        {
            var lCrypter = GetCrypter();
            return lCrypter.Crypt(aDataString);
        }

        public static string GenerateSalt(int aSize = 32)
        {
            return Convert.ToBase64String(GenerateRootSeed(aSize)).Substring(0, aSize);
        }

        public static byte[] HexStringToByteArray(string aHex)
        {
            return CryptoCurrencyAdvocacy.HexStringToByteArray(aHex);
        }

        internal class WalletEncryptionContext : IWalletEncryptionContext
        {
            public string EncryptedSalt { get; }
            public string MasterPasswordHash { get; set; }
            public string AlternatePasswordHash { get; set; }
            public string EncryptedRootSeed { get; set; }
            public string EncryptedMasterPassword { get; set; }
            public bool IsAltPasswordSet => !string.IsNullOrEmpty(AlternatePasswordHash) && !string.IsNullOrEmpty(EncryptedMasterPassword);
            public bool IsWalletInitialized => !string.IsNullOrEmpty(MasterPasswordHash) && !string.IsNullOrEmpty(EncryptedRootSeed);

            public WalletEncryptionContext(string aSalt)
            {
                EncryptedSalt = Encryption.EncryptText(aSalt);
            }

            public string GetSalt()
            {
                return Encryption.DecryptText(EncryptedSalt);
            }

            public static WalletEncryptionContext GetNewInstance()
            {
                return new WalletEncryptionContext(GenerateSalt());
            }
        }
    }
}