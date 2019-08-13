using Newtonsoft.Json.Linq;
using Pandora.Client.Universal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.Client.PandorasWallet.Utils
{
    public class OldSettingsReader
    {
        private JObject FSettings;

        private ReaderWriterLockSlim FrwLock { get; }

        public OldSettingsReader(byte[] aSettingsFileByteArray)
        {
            FrwLock = new ReaderWriterLockSlim();
            string lSettings;
            using (MemoryStream lMemStream = new MemoryStream(aSettingsFileByteArray))
            using (StreamReader lStreamReader = new StreamReader(lMemStream))
                lSettings = lStreamReader.ReadToEnd();
            LoadSettings(lSettings);
            FrwLock = new ReaderWriterLockSlim();
        }

        private void LoadSettings(string aSettingString)
        {
            FrwLock.EnterWriteLock();
            FSettings = string.IsNullOrWhiteSpace(aSettingString) ? new JObject() : JObject.Parse(aSettingString);
            FrwLock.ExitWriteLock();
        }

        /// <summary>
        /// Returns a dictionary of all settings
        /// </summary>
        /// <returns></returns>
        public bool GetSettings(out Dictionary<string, object> values)
        {
            return GetSettings<object>(out values);
        }

        /// <summary>
        /// Returns a dictionary of all settings, where all settings are of the same type
        /// </summary>
        /// <typeparam name="TValue">The type of all of the settings</typeparam>
        /// <returns></returns>
        public bool GetSettings<TValue>(out Dictionary<string, TValue> values)
        {
            try
            {
                if (FSettings == null) throw new Exception("Settings not loaded");
                FrwLock.EnterReadLock();

                values = FSettings.ToObject<Dictionary<string, TValue>>();
                FrwLock.ExitReadLock();
                return true;
            }
            catch
            {
                values = null;
                FrwLock.ExitReadLock();
                return false;
            }
        }
    }

    public class OldSecretReader
    {
        public enum KeyInventoryItem
        {
            [Description("GUID")]
            GUIDSeed = 0,

            [Description("Bittrex")]
            BittrexKeyPair = 1
        }

        private static UnicodeEncoding FUE = new UnicodeEncoding();
        private KeyInventoryItem[] FInventory;
        private string FSalt;
        private int FEncryptionEnd;

        public byte[] SecretData { get; private set; }
        public Guid PasscodeEntropy { get; private set; }
        public PandoraKeys.ExchangeKeys[] FExchangeKeys { get; private set; }

        public OldSecretReader(byte[] aSecretData)
        {
            SecretData = aSecretData;
        }

        private static KeyInventoryItem[] GetInventoryFromFile(byte[] aSecretData, out int aEndPosition)
        {
            List<KeyInventoryItem> lInventory = new List<KeyInventoryItem>();
            byte[] lData = GetUncryptedData(aSecretData, out aEndPosition);
            for (int it = 0; it < (lData.Length / 4); it++)
            {
                byte[] lSubArray = new byte[4];
                Array.Copy(lData, (4 * it), lSubArray, 0, 4);
                lInventory.Add((KeyInventoryItem)BitConverter.ToInt32(lSubArray, 0));
            }
            return lInventory.ToArray();
        }

        private static string GetSaltFromFile(byte[] aSecretData, out int aEndPosition, int aOffSet)
        {
            string lSalt;
            lSalt = FUE.GetString(GetUncryptedData(aSecretData, out aEndPosition, aOffSet));
            return lSalt;
        }

        private static string GetSecureHash(string aPassword, string aSalt)
        {
            byte[] lStringdBytes = Encoding.ASCII.GetBytes(aPassword);
            string lHash = CryptSharp.Crypter.Blowfish.Crypt(aPassword, aSalt);
            return lHash;
        }

        private static byte[] GetUncryptedData(byte[] aSecretData, out int aEndPosition, int aOffSet = 0)
        {
            byte[] lOutArray;

            using (MemoryStream ms = new MemoryStream(aSecretData))
            using (MemoryStream msOut = new MemoryStream())
            {
                int lOffset = aOffSet > 0 ? ((-1) * ((int)ms.Length - aOffSet)) - 4 : -4;
                ms.Seek(lOffset, SeekOrigin.End);
                int data;
                while ((data = ms.ReadByte()) != -1)
                {
                    msOut.WriteByte((byte)data);
                    if (aOffSet > 0 && ms.Position == aOffSet)
                        break;
                }
                uint lDataLength = BitConverter.ToUInt32(msOut.ToArray(), 0);
                msOut.SetLength(0);
                int lDataPosition = ((-1) * (int)lDataLength) + lOffset;
                aEndPosition = (int)ms.Seek(lDataPosition, SeekOrigin.End);
                while ((data = ms.ReadByte()) != -1)
                {
                    msOut.WriteByte((byte)data);
                    if (aOffSet > 0 && ms.Position == aOffSet)
                        break;
                }
                lOutArray = msOut.ToArray();
            }
            return lOutArray.Take(lOutArray.Length - 4).ToArray();
        }

        public bool DecryptWalletKey(string aPassword)
        {
            if (SecretData == null) throw new Exception("Secret data is not set");
            if (string.IsNullOrEmpty(FSalt) || FInventory == null)
            {
                FInventory = GetInventoryFromFile(SecretData, out int lInventoryEnd);
                FSalt = GetSaltFromFile(SecretData, out FEncryptionEnd, lInventoryEnd);
            }
            string lPasswordHash = GetSecureHash(aPassword, FSalt);
            try
            {
                PandoraKeys lKeys = DecryptFile(lPasswordHash, SecretData, FEncryptionEnd);
                PasscodeEntropy = lKeys.AccountGuid;
                FExchangeKeys = lKeys.ExchangeCredentials;
            }
            catch (Exception ex)
            {
                Log.Write($"Error decrypting old secret file. Exception: {ex}");
                return false;
            }
            return true;
        }

        ///<summary>
        /// Decrypts a file using Rijndael algorithm.
        ///</summary>
        private static PandoraKeys DecryptFile(string aPassword, byte[] aSecretData, int aEndPosition)
        {
            Tuple<byte[], byte[]> lkeys = GetCryptoKeyAndIV(aPassword);
            byte[] lkey = lkeys.Item1;
            byte[] lIV = lkeys.Item2;
            PandoraKeys lKeys;
            var lSecretBytes = aSecretData.Take(aEndPosition).ToArray();
            using (MemoryStream MemOut = new MemoryStream(lSecretBytes))
            {
                Aes RMCrypto = Aes.Create();
                RMCrypto.Padding = PaddingMode.PKCS7;
                MemOut.Seek(0, SeekOrigin.Begin);
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Binder = new KeysSerializationBinder();
                using (CryptoStream cs = new CryptoStream(MemOut, RMCrypto.CreateDecryptor(lkey, lIV), CryptoStreamMode.Read))
                    lKeys = (PandoraKeys)formatter.Deserialize(cs);
            }
            return lKeys;
        }

        private static Tuple<byte[], byte[]> GetCryptoKeyAndIV(string aPassword)
        {
            byte[] lOriginalkey = FUE.GetBytes(aPassword);
            if (lOriginalkey.Length < 32) throw new CryptographicException("Password lenght should be at least 32 bytes");
            byte[] lIV = lOriginalkey.Take(8).ToArray().Concat(lOriginalkey.Skip(lOriginalkey.Length - 8).ToArray()).ToArray();
            byte[] lKey = lOriginalkey.Skip(lOriginalkey.Length - 32).ToArray();
            return new Tuple<byte[], byte[]>(lKey, lIV);
        }

        [Serializable]
        public class PandoraKeys
        {
            public Guid AccountGuid { get; set; }
            public ExchangeKeys[] ExchangeCredentials { get; set; }

            [Serializable]
            public class ExchangeKeys
            {
                public string ExchangeName;
                public string Key;
                public string Secret;
            }

            [NonSerialized]
            public KeyInventoryItem[] Inventory;
        }

        private class KeysSerializationBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string aTypeName)
            {
                aTypeName = aTypeName.Replace(
            "Pandora.Client.PandorasWallet.Utils.PandoraKeys",
            typeof(PandoraKeys).FullName);

                return Type.GetType(aTypeName);
            }
        }
    }
}