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
using Pandora.Client.Crypto.Currencies.Controls;
using Pandora.Client.Universal;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Pandora.Client.PandorasWallet.Wallet
{
    internal class KeyManager
    {
        private string FEncryptedPassword;
        private string FEncryptedSalt;
        private string FEncryptedRootSeed;

        /// <summary>
        /// This object manages all crypto fucntions and keeps the
        /// key data encrypted until needed for use.
        /// </summary>
        /// <param name="aSalt"></param>
        public KeyManager(string aSalt)
        {
            FEncryptedSalt = Encryption.EncryptText(aSalt);
        }

        public string GetSalt()
        {
            return Encryption.DecryptText(FEncryptedSalt);
        }

        public void SetPassword(string aPassword)
        {
            if (aPassword == null)
                FEncryptedPassword = null;
            else
                FEncryptedPassword = Encryption.EncryptText(aPassword, GetSalt());
        }

        public bool IsPasswordSet { get => FEncryptedPassword != null; }

        public string GetPassword()
        {
            if (!IsPasswordSet) throw new InvalidOperationException("Password not set.");
            return Encryption.DecryptText(FEncryptedPassword, GetSalt());
        }

        public void SetRootSeed(byte[] aRootSeed)
        {
            if (aRootSeed == null || aRootSeed.Length != 32) throw new ArgumentException("Invalid root seed size.");
            EncryptedRootSeed = Encryption.EncryptText(Convert.ToBase64String(aRootSeed), GetPassword() + GetSalt());
            System.Diagnostics.Debug.Assert(GetSecretRootSeed() == BitConverter.ToString(aRootSeed).Replace("-", string.Empty).ToLower(), "Keys are not equal. (EC1023)");
        }

        public string EncryptText(string aText)
        {
            return Encryption.EncryptText(aText, string.Concat(GetSalt(), GetPassword()));
        }

        public string DecryptText(string aText)
        {
            return Encryption.DecryptText(aText, string.Concat(GetSalt(), GetPassword()));
        }

        public string EncryptedRootSeed
        {
            get
            {
                if (FEncryptedRootSeed == null) throw new InvalidOperationException("EncryptedEntropy not set.");
                return FEncryptedRootSeed;
            }
            set => FEncryptedRootSeed = value;
        }

        internal byte[] GetRootSeed()
        {
            return Convert.FromBase64String(Encryption.DecryptText(EncryptedRootSeed, GetPassword() + GetSalt()));
        }

        internal string GetSecretRootSeed()
        {
            return BitConverter.ToString(GetRootSeed()).Replace("-", string.Empty).ToLower();
        }

        public IClientCurrencyAdvocacy GetCurrencyAdvocacy(long aCurrencyId, ChainParams aChainParams)
        {
            IClientCurrencyAdvocacy lAdvocacy = CurrencyControl.GetClientCurrencyAdvocacy(aCurrencyId, aChainParams, GetSecretRootSeed);
            return lAdvocacy;
        }

        internal void UpdatePassword(string lPassword)
        {
            var lRootSeed = GetRootSeed();
            SetPassword(lPassword);
            SetRootSeed(lRootSeed);
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

        public static bool CheckPassword(string aPassword, string aPasswordHash)
        {
            return CryptSharp.Crypter.CheckPassword(aPassword, aPasswordHash);
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
            return ClientCurrencyAdvocacy.HexStringToByteArray(aHex);
        }
    }
}