using System;
using System.Text;

namespace Pandora.Client.Crypto.Currencies.Controls
{
    public class CurrencyControl
    {
        private static CurrencyControl FControl;
        private static ClientCurrencyAdvocacy FClientCurrencyAdvocacy;

        protected CurrencyControl()
        {
        }

        public static CurrencyControl GetCurrencyControl()
        {
            if (FControl == null)
            {
                FControl = new CurrencyControl();
            }

            return FControl;
        }

        public string GenerateRootSeed(string aEmail, string aUsername, Guid aEntropy)
        {
            string lString = aEmail + aUsername;
            byte[] lStringdBytes = Encoding.ASCII.GetBytes(lString);
            byte[] lSalt = aEntropy.ToByteArray();
            byte[] lHash = Crypto.SCrypt.ComputeDerivedKey(lStringdBytes, lSalt, 16384, 8, 1, null, 16);

            string s = BitConverter.ToString(lHash);

            return s.Replace("-", "");
        }

        public IClientCurrencyAdvocacy GetClientCurrencyAdvocacy(uint aCurrencyId, ChainParams aChainParams)
        {
            if (aChainParams == null)
            {
                throw new ArgumentNullException(nameof(aChainParams), "Chain Parameters can not be null");
            }

            return new ClientCurrencyAdvocacy(aCurrencyId, aChainParams);
        }
    }
}