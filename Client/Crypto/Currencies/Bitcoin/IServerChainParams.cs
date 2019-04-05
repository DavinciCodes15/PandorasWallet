namespace Pandora.Client.Crypto.Currencies
{
    public interface IServerChainParams : IChianParams
    {
        int Port { get; set; }
        string GenesisBlockHash { get; set; }
        string GenesisMerkleRoot { get; set; }
        bool UseTxScanner { get; set; }
        long GenesisHeight { get; set; }
        long MaxP2PVersion { get; set; }
        long Magic { get; set; }
        string DNSSeed { get; set; }
        bool X11 { get; set; }
        bool POS { get; set; }
        ProtocolFlags ProtocolFlags { get; set; }
    }
}