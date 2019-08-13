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


using Pandora.Client.SystemBackup.Interfaces;
using Pandora.Client.Universal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace Pandora.Client.PandorasWallet.SystemBackup
{
    [Serializable]
    public class PandorasBackupObject : BasePandoraBackupObject
    {
        public PandorasBackupObject()
        {
        }

        public PandorasBackupObject(BasePandoraBackupObject aBaseBackupObject)
        {
            base.BackupDate = aBaseBackupObject.BackupDate;
            base.ExchangeData = aBaseBackupObject.ExchangeData;
            base.OwnerData = aBaseBackupObject.OwnerData;
            base.PasswordHash = aBaseBackupObject.PasswordHash;
            base.Version = aBaseBackupObject.Version;
            base.WalletData = aBaseBackupObject.WalletData;
        }

        public bool IsOldBackupData { get; set; }
        public Dictionary<string, string[]> ExchangeKeys { get; set; }
        public long[] SelectedCoins { get; set; }
        public string[] WalletSeeds { get; set; }
        public long DefaultCoinID { get; set; }
    }

    [Serializable]
    public class BasePandoraBackupObject : IBackupObject
    {
        private int FVersion = 1001;

        public int Version
        {
            get => FVersion; set
            {
                if (value <= 1000 || value > FVersion) throw new Exception("Invalid backup version");
                FVersion = value;
            }
        }

        public DateTime BackupDate { get; set; }

        /// <summary>
        /// Array position 0 is Email, and position 1 is Username
        /// </summary>
        public string[] OwnerData { get; set; }

        public string PasswordHash { get; set; }
        public IBackupSerializable ExchangeData { get; set; }
        public IBackupSerializable WalletData { get; set; }
    }

    [Serializable]
    public class BackupSerializableObject : IBackupSerializable
    {
        public virtual byte[] Data { get; set; }
        protected int FVersion = 5001;
        protected MemoryStream FInternalMemStream;

        public int Version
        {
            get => FVersion; set
            {
                if (value <= 5000 || value > FVersion) throw new Exception("Invalid backup serializable version");
                FVersion = value;
            }
        }

        public BackupSerializableObject()
        {
        }

        public BackupSerializableObject(byte[] aData)
        {
            Data = aData;
        }

        public BackupSerializableObject(SerializationInfo info, StreamingContext context)
        {
            Deserialize((byte[])info.GetValue("data", typeof(byte[])));
        }

        public virtual void Deserialize(byte[] aBytesToDeserialize)
        {
            try
            {
                if (aBytesToDeserialize == null)
                    Data = null;
                FInternalMemStream = new MemoryStream(aBytesToDeserialize);
                using (BinaryReader lReader = new BinaryReader(FInternalMemStream, System.Text.Encoding.UTF8, true))
                {
                    Version = lReader.ReadInt32();
                    var lDataLength = lReader.ReadInt32();
                    Data = lReader.ReadBytes(lDataLength);
                }
            }
            catch (Exception ex)
            {
                Log.Write($"Failed to deserialize exchange backup object. Exception thrown: {ex}");
                throw;
            }
        }

        public virtual byte[] Serialize()
        {
            try
            {
                FInternalMemStream = new MemoryStream();
                using (BinaryWriter lWriter = new BinaryWriter(FInternalMemStream, System.Text.Encoding.UTF8, true))
                {
                    lWriter.Write(Version);
                    lWriter.Write(Data != null ? Data.Length : 0);
                    lWriter.Write(Data ?? new byte[0]);
                    return FInternalMemStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Log.Write($"Failed to serialize exchange backup object. Exception thrown: {ex}");
                throw;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("data", Serialize());
        }
    }
}