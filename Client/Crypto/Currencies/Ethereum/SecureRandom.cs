

namespace Nethereum.HdWallet
{
    public class SecureRandom : Pandora.Client.Crypto.Currencies.IRandom
    {
        private static readonly Org.BouncyCastle.Security.SecureRandom SecureRandomInstance =
            new Org.BouncyCastle.Security.SecureRandom();

        public void GetBytes(byte[] output)
        {
            SecureRandomInstance.NextBytes(output);
        }
    }
}