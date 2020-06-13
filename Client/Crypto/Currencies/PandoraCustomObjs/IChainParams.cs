namespace Pandora.Client.Crypto.Currencies
{
    public interface IChainParams
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
        CapablityFlags Capabilities { get; set; }
        long Version { get; set; }
    }
}