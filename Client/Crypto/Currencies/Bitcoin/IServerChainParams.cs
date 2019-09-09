namespace Pandora.Client.Crypto.Currencies
{
    public interface IServerChainParams : IChainParams
    {
        int Port { get; set; }
        bool UseTxScanner { get; set; }
        long Version { get; set; }
        long MaxP2PVersion { get; set; }
        long Magic { get; set; }
        bool X11 { get; set; }
        bool POS { get; set; }
        ProtocolFlags ProtocolFlags { get; set; }
    }
}