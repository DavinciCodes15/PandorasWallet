using System;
using System.Collections.Generic;
using System.Text;

namespace Pandora.Client.Crypto.Currencies
{
    public delegate ICryptoCurrencyAdvocacy DelegateGetClientCurrencyAdvocacy(long aCurrencyId, ChainParams aChainParams, Func<string> aGetRootSeed);

    public class CurrencyControl
    {
        private static CurrencyControl FControl;
        List<DelegateGetClientCurrencyAdvocacy> FEvents = new List<DelegateGetClientCurrencyAdvocacy>();
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

        public void AddCurrencyAdvocacy(DelegateGetClientCurrencyAdvocacy aGetAdvocacy)
        {
            FEvents.Add(aGetAdvocacy);
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

        public static ICryptoCurrencyAdvocacy GetClientCurrencyAdvocacy(long aCurrencyId, ChainParams aChainParams, Func<string> aGetRootSeed)
        {
            if (aChainParams == null)
                throw new ArgumentNullException(nameof(aChainParams), "Chain Parameters can not be null");

            ICryptoCurrencyAdvocacy lResult = null;
            foreach (var lEvent in FControl.FEvents)
            {
                lResult = lEvent(aCurrencyId, aChainParams, aGetRootSeed);
                if (lResult != null)
                    break;
            }
            if (lResult == null)
                lResult = new CryptoCurrencyAdvocacy(aCurrencyId, aChainParams, aGetRootSeed);
            return lResult;
        }
    }
}