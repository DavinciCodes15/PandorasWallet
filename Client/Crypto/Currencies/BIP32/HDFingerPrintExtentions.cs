
namespace Pandora.Client.Crypto.Currencies.BIP32
{
    public static class HDFingerPrintExtentions
    {
        public static void ReadWrite(this CoinStream serializable, ref HDFingerprint lFingerprint)
        {
            var bytes = lFingerprint.ToBytes();
            serializable.ReadWrite(ref bytes);
            if (!serializable.Serializing)
                lFingerprint = new HDFingerprint(bytes);
        }

    }
}
