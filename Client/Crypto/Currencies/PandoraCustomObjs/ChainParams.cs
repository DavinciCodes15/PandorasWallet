using System.Collections.Generic;
using System;

namespace Pandora.Client.Crypto.Currencies
{
    [Serializable]
    public class ChainParams : IChainParams
    {
        public enum NetworkType { MainNet = 0, TestNet = 1 };

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
            Capabilities = CapablityFlags.SegWitSupport;
            Encoder = "bc";
        }

        public virtual void CopyTo(IChainParams aDest)
        {
            aDest.NetworkName = this.NetworkName;
            aDest.Network = (ChainParams.NetworkType)this.Network;
            aDest.PublicKeyAddress = this.PublicKeyAddress ?? aDest.PublicKeyAddress;
            aDest.ScriptAddress = this.ScriptAddress ?? aDest.ScriptAddress;
            aDest.SecretKey = this.SecretKey ?? aDest.SecretKey;
            aDest.ExtPublicKey = this.ExtPublicKey ?? aDest.ExtPublicKey;
            aDest.ExtSecretKey = this.ExtSecretKey ?? aDest.ExtSecretKey;
            aDest.ForkFromId = this.ForkFromId;
            aDest.EncryptedSecretKeyNoEc = this.EncryptedSecretKeyNoEc ?? aDest.EncryptedSecretKeyNoEc;
            aDest.EncryptedSecretKeyEc = this.EncryptedSecretKeyEc ?? aDest.EncryptedSecretKeyEc;
            aDest.PasspraseCode = this.PasspraseCode ?? aDest.PasspraseCode;
            aDest.ConfirmationCode = this.ConfirmationCode ?? aDest.ConfirmationCode;
            aDest.StealthAddress = this.StealthAddress ?? aDest.StealthAddress;
            aDest.AssetId = this.AssetId ?? aDest.AssetId;
            aDest.ColoredAddress = this.ColoredAddress ?? aDest.ColoredAddress;
            aDest.Encoder = this.Encoder;
            aDest.Capabilities = this.Capabilities;
            aDest.Version = this.Version;
        }

        public virtual void CopyFrom(IChainParams aSource)
        {
            this.NetworkName = aSource.NetworkName;
            this.Network = (ChainParams.NetworkType)aSource.Network;
            this.PublicKeyAddress = aSource.PublicKeyAddress ?? this.ColoredAddress;
            this.ScriptAddress = aSource.ScriptAddress ?? this.ScriptAddress;
            this.SecretKey = aSource.SecretKey ?? this.SecretKey;
            this.ExtPublicKey = aSource.ExtPublicKey ?? this.ExtPublicKey;
            this.ExtSecretKey = aSource.ExtSecretKey ?? this.ExtSecretKey;
            this.ForkFromId = aSource.ForkFromId;
            this.EncryptedSecretKeyNoEc = aSource.EncryptedSecretKeyNoEc ?? this.EncryptedSecretKeyNoEc;
            this.EncryptedSecretKeyEc = aSource.EncryptedSecretKeyEc ?? this.EncryptedSecretKeyEc;
            this.PasspraseCode = aSource.PasspraseCode ?? this.PasspraseCode;
            this.ConfirmationCode = aSource.ConfirmationCode ?? this.ConfirmationCode;
            this.StealthAddress = aSource.StealthAddress ?? this.StealthAddress;
            this.AssetId = aSource.AssetId ?? this.AssetId;
            this.ColoredAddress = aSource.ColoredAddress ?? this.ColoredAddress;
            this.Encoder = aSource.Encoder;
            this.Capabilities = aSource.Capabilities;
            this.Version = aSource.Version;
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
        public CapablityFlags Capabilities { get; set; }
        public long Version { get; set; }
    }

    [Flags]
    public enum CapablityFlags
    {
        None = 0, SegWitSupport = 16, EthereumProtocol = 32 
    }
}