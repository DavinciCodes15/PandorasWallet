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
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Pandora.Client.Universal
{
    public class Encryption
    {
        private TripleDESCryptoServiceProvider FAlgorithm = new TripleDESCryptoServiceProvider();
        private string FKey = "";
        private ICryptoTransform FDecrypt;
        private ICryptoTransform FEncrypt;

        public Encryption()
        {
            FAlgorithm.IV = new byte[] { 120, 2, 39, 232, 15, 72, 9, 19 };
            FAlgorithm.Padding = PaddingMode.None;
            FAlgorithm.KeySize = 128;
        }

        public void DecryptDataBlock(byte[] aBuffer, int aOffset, int aCount)
        {
            if (FDecrypt == null)
            {
                FDecrypt = FAlgorithm.CreateDecryptor();
            }

            MemoryStream lMemStream = new MemoryStream();
            lMemStream.Write(aBuffer, aOffset, aCount);
            lMemStream.Position = 0;
            CryptoStream lStream = new CryptoStream(lMemStream, FDecrypt, CryptoStreamMode.Read);
            lStream.Read(aBuffer, aOffset, aCount - (aCount % 8));
        }

        public void EncryptDataBlock(byte[] aBuffer, int aOffset, int aCount)
        {
            if (FEncrypt == null)
            {
                FEncrypt = FAlgorithm.CreateEncryptor();
            }

            MemoryStream lMemStream = new MemoryStream();
            CryptoStream lStream = new CryptoStream(lMemStream, FEncrypt, CryptoStreamMode.Write);
            lStream.Write(aBuffer, aOffset, aCount);
            lStream.Flush();
            lMemStream.Position = 0;
            lMemStream.Read(aBuffer, aOffset, aCount);
        }

        private const string KEY_Filler = "S8yd+=UKeC%=k'KaE+k&Jge$*i";

        public static string EncryptText(string aText, string aKey = "")
        {
            aKey += KEY_Filler;
            Encryption lEnc = new Encryption
            {
                Key = aKey
            };
            byte[] lData = Encoding.ASCII.GetBytes(aText);
            lEnc.EncryptDataBlock(lData, 0, lData.Length);
            return Convert.ToBase64String(lData);
        }

        public static string DecryptText(string aEncryptedText, string aKey = "")
        {
            try
            {
                aKey += KEY_Filler;
                Encryption lEnc = new Encryption
                {
                    Key = aKey
                };
                byte[] lData = Convert.FromBase64String(aEncryptedText);
                lEnc.DecryptDataBlock(lData, 0, lData.Length);
                return Encoding.ASCII.GetString(lData);
            }
            catch (Exception e)
            {
                Log.Write(LogLevel.Error, "Decryption failed : {0} ", e.Message);
                return "";
            }
        }

        public string Key
        {
            get => FKey;
            set
            {
                byte[] lKey = new byte[FAlgorithm.Key.Length];
                FKey = value;
                if (!string.IsNullOrEmpty(FKey))
                {
                    byte[] lBinKey = Encoding.ASCII.GetBytes(FKey);
                    int lTotal = lBinKey.Length;
                    if (lTotal > lKey.Length)
                    {
                        lTotal = lKey.Length;
                    }

                    for (int i = 0; i < lTotal; i++)
                    {
                        lKey[i] = lBinKey[i];
                    }
                }
                FAlgorithm.Key = lKey;
            }
        }
    }
}