using System.Collections.Generic;
using System;

namespace Pandora.Client.Crypto.Currencies
{
    [Serializable]
    public class ChainParams : IChianParams
    {
        public enum NetworkType { MainNet = 0, TestNet = 1};

        public ChainParams()
        {
        }

        public ChainParams(NetworkType aNetwork)
        {
            Init();
            if (aNetwork != NetworkType.MainNet)
            {
                Network = aNetwork;
                PublicKeyAddress = new byte[] { (111) };
                ScriptAddress = new byte[] { (196) };
                SecretKey = new byte[] { (239) };
                ExtPublicKey = new byte[] { (0x04), (0x35), (0x87), (0xCF) };
                ExtSecretKey = new byte[] { (0x04), (0x35), (0x83), (0x94) };
                StealthAddress = new byte[] { 0x2b };
                AssetId = new byte[] { 115 };
                Encoder = "tb";
            }
        }
        private void Init()
        {
            NetworkName = "Bitcoin";
            Network = 0;
            PublicKeyAddress = new byte[] { (0) };
            ScriptAddress = new byte[] { (5) };
            SecretKey = new byte[] { (128) };
            ExtPublicKey = new byte[] { (0x04), (0x88), (0xB2), (0x1E) };
            ExtSecretKey = new byte[] { (0x04), (0x88), (0xAD), (0xE4) };
            ForkFromId = 0;
            EncryptedSecretKeyNoEc = new byte[] { 0x01, 0x42 };
            EncryptedSecretKeyEc = new byte[] { 0x01, 0x43 };
            PasspraseCode = new byte[] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2 };
            ConfirmationCode = new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9A };
            StealthAddress = new byte[] { 0x2a };
            AssetId = new byte[] { 23 };
            ColoredAddress = new byte[] { 0x13 };
            Encoder = "bc";
        }

        public virtual void CopyTo(IChianParams aDest)
        {
            aDest.NetworkName = this.NetworkName;
            aDest.Network = (ChainParams.NetworkType)this.Network;
            aDest.PublicKeyAddress = this.PublicKeyAddress;
            aDest.ScriptAddress = this.ScriptAddress;
            aDest.SecretKey = this.SecretKey;
            aDest.ExtPublicKey = this.ExtPublicKey;
            aDest.ExtSecretKey = this.ExtSecretKey;
            aDest.ForkFromId = this.ForkFromId;
            aDest.EncryptedSecretKeyNoEc = this.EncryptedSecretKeyNoEc;
            aDest.EncryptedSecretKeyEc = this.EncryptedSecretKeyEc;
            aDest.PasspraseCode = this.PasspraseCode;
            aDest.ConfirmationCode = this.ConfirmationCode;
            aDest.StealthAddress = this.StealthAddress;
            aDest.AssetId = this.AssetId;
            aDest.ColoredAddress = this.ColoredAddress;
            aDest.Encoder = this.Encoder;
        }

        public virtual void CopyFrom(IChianParams aSource)
            {
                this.NetworkName = aSource.NetworkName;
            this.Network = (ChainParams.NetworkType)aSource.Network;
            this.PublicKeyAddress = aSource.PublicKeyAddress;
            this.ScriptAddress = aSource.ScriptAddress;
            this.SecretKey = aSource.SecretKey;
            this.ExtPublicKey = aSource.ExtPublicKey;
            this.ExtSecretKey = aSource.ExtSecretKey;
            this.ForkFromId = aSource.ForkFromId;
            this.EncryptedSecretKeyNoEc = aSource.EncryptedSecretKeyNoEc;
            this.EncryptedSecretKeyEc = aSource.EncryptedSecretKeyEc;
            this.PasspraseCode = aSource.PasspraseCode;
            this.ConfirmationCode = aSource.ConfirmationCode;
            this.StealthAddress = aSource.StealthAddress;
            this.AssetId = aSource.AssetId;
            this.ColoredAddress = aSource.ColoredAddress;
            this.Encoder = aSource.Encoder;
         }


        public long ForkFromId { get; set; }
        public NetworkType Network { get; set; }
        public string NetworkName { get; set; }
        public byte[] PublicKeyAddress { get; set; }
        public byte[] ScriptAddress { get; set; }
        public byte[] SecretKey { get; set; }
        public byte[] ExtPublicKey { get; set; }
        public byte[] ExtSecretKey { get; set; }
        public byte[] EncryptedSecretKeyNoEc { get; set; }
        public byte[] EncryptedSecretKeyEc { get; set; }
        public byte[] PasspraseCode { get; set; }
        public byte[] ConfirmationCode { get; set; }
        public byte[] StealthAddress { get; set; }
        public byte[] AssetId { get; set; }
        public byte[] ColoredAddress { get; set; }
        public string Encoder { get; set; }


    }

    public interface IChianParams
    {
        long ForkFromId { get; set; }
        ChainParams.NetworkType Network { get; set; }
        string NetworkName { get; set; }
        byte[] PublicKeyAddress { get; set; }
        byte[] ScriptAddress { get; set; }
        byte[] SecretKey { get; set; }
        byte[] ExtPublicKey { get; set; }
        byte[] ExtSecretKey { get; set; }
        byte[] EncryptedSecretKeyNoEc { get; set; }
        byte[] EncryptedSecretKeyEc { get; set; }
        byte[] PasspraseCode { get; set; }
        byte[] ConfirmationCode { get; set; }
        byte[] StealthAddress { get; set; }
        byte[] AssetId { get; set; }
        byte[] ColoredAddress { get; set; }
        string Encoder { get; set; }
    }
}
