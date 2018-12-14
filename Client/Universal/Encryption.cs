using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace Pandora.Client.Universal
{
    public class Encryption 
    {
        TripleDESCryptoServiceProvider FAlgorithm = new TripleDESCryptoServiceProvider();
        string FKey = "";
        ICryptoTransform FDecrypt;
        ICryptoTransform FEncrypt;


        public Encryption()
        {
            FAlgorithm.IV = new byte[] { 120, 2, 39, 232, 15, 72, 9, 19 };
            FAlgorithm.Padding = PaddingMode.None;
            FAlgorithm.KeySize = 128;
        }

        public void DecryptDataBlock(byte[] aBuffer, int aOffset, int aCount)
        {
            if (FDecrypt == null)
                FDecrypt = FAlgorithm.CreateDecryptor();
            MemoryStream lMemStream = new MemoryStream();
            lMemStream.Write(aBuffer, aOffset, aCount);
            lMemStream.Position = 0;
            CryptoStream lStream = new CryptoStream(lMemStream, FDecrypt, CryptoStreamMode.Read);
            lStream.Read(aBuffer, aOffset, aCount - (aCount % 8));
        }

        public void EncryptDataBlock(byte[] aBuffer, int aOffset, int aCount)
        {
            if (FEncrypt == null)
                FEncrypt = FAlgorithm.CreateEncryptor();
            MemoryStream lMemStream = new MemoryStream();
            CryptoStream lStream = new CryptoStream(lMemStream, FEncrypt, CryptoStreamMode.Write);
            lStream.Write(aBuffer, aOffset, aCount);
            lStream.Flush();
            lMemStream.Position = 0;
            lMemStream.Read(aBuffer, aOffset, aCount);
        }

        const string KEY_Filler = "S8yd+=UKeC%=k'KaE+k&Jge$*i";
        public static string EncryptText(string aText, string aKey = "")
        {
            aKey += KEY_Filler;
            var lEnc = new Encryption();
            lEnc.Key = aKey;
            var lData = Encoding.ASCII.GetBytes(aText);
            lEnc.EncryptDataBlock(lData, 0, lData.Length);
            return Convert.ToBase64String(lData);
        }

        public static string DecryptText(string aEncryptedText, string aKey = "")
        {
            try
            {
                aKey += KEY_Filler;
                var lEnc = new Encryption();
                lEnc.Key = aKey;
                var lData = Convert.FromBase64String(aEncryptedText);
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
            get
            {
                return FKey;
            }
            set
            {
                byte[] lKey = new byte[FAlgorithm.Key.Length];
                FKey = value;
                if (!String.IsNullOrEmpty(FKey))
                {
                    byte[] lBinKey = Encoding.ASCII.GetBytes(FKey);
                    int lTotal = lBinKey.Length;
                    if (lTotal > lKey.Length)
                        lTotal = lKey.Length;
                    for (int i = 0; i < lTotal; i++)
                        lKey[i] = lBinKey[i];
                }
                FAlgorithm.Key = lKey;
            }
        }
    }
}
