using System;
using System.Text;

namespace Pandora.Client.Crypto.Currencies.Controls
{
    public class CurrencyControl
    {
        private static CurrencyControl FControl;

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

        [Obsolete("Use only for backward compatiblity")]
        public static string GenerateRootSeed(string aEmail, string aUsername, byte[] aEntropy)
        {
            string lString = aEmail + aUsername;
            byte[] lStringdBytes = Encoding.ASCII.GetBytes(lString);
            byte[] lHash = Crypto.SCrypt.ComputeDerivedKey(lStringdBytes, aEntropy, 16384, 8, 1, null, 16);

            string s = BitConverter.ToString(lHash);

            return s.Replace("-", "");
        }

        public static IClientCurrencyAdvocacy GetClientCurrencyAdvocacy(long aCurrencyId, ChainParams aChainParams, Func<string> aGetRootSeed)
        {
            if (aChainParams == null)
                throw new ArgumentNullException(nameof(aChainParams), "Chain Parameters can not be null");
           return new ClientCurrencyAdvocacy(aCurrencyId, aChainParams, aGetRootSeed);
        }
    }
}