using System;
using System.Text;
using System.Collections.Generic;
using System.Reflection;

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
            if (FControl == null) FControl = new CurrencyControl();
            return FControl;
        }

        public string GenerateRootSeed(string aEmail, string aUsername, Guid aEntropy)
        {
            string lString = aEmail + aUsername;
            byte[] lStringdBytes = Encoding.ASCII.GetBytes(lString);
            byte[] lSalt = aEntropy.ToByteArray();
            var lHash = Crypto.SCrypt.ComputeDerivedKey(lStringdBytes, lSalt, 16384, 8, 1, null, 16);

            var s = BitConverter.ToString(lHash);

            return s.Replace("-", "");
        }

        //public IServerCurrencyAdvocacy GetCryptoCurrency(ulong aCurrencyId, string aAssemblyFileName = null)
        //{
        //    ICurrencyControls lCurrencyControls;
        //    if (!FCurrencyControlList.TryGetValue(aCurrencyId, out lCurrencyControls))
        //    {
        //        Assembly lAssembly;
        //        // TODO: for now just load the last lib but future verions check to see if we are loading the
        //        //       right Assembly version first.
        //        if (string.IsNullOrEmpty(aAssemblyFileName))
        //            lAssembly = System.Reflection.Assembly.GetExecutingAssembly();
        //        else
        //            lAssembly = Assembly.LoadFrom(aAssemblyFileName);  // TODO: Assembly must be signed!!!!
        //        var lClassName = String.Format("Pandora.Client.Crypto.Currencies.Controls.ID_{0:00000}.CurrencyControls", aCurrencyId);
        //        Type T = lAssembly.GetType(lClassName, true);
        //        lCurrencyControls = (ICurrencyControls)Activator.CreateInstance(T);
        //        FCurrencyControlList.Add(aCurrencyId, lCurrencyControls);
        //    }
        //    return lCurrencyControls.GetCurrencyAdvocacy();
        //}

        public IClientCurrencyAdvocacy GetClientCurrencyAdvocacy(uint aCurrencyId, ChainParams aChainParams)
        {
            if (aChainParams == null) throw new Exception("You must provide the ChainParams to create the Advocacy now.");
            return new ClientCurrencyAdvocacy(aCurrencyId, aChainParams);
        }

        /// <summary>
        /// This method is not secure.
        /// Use GenerateRootSeed(string aEmail, string aUsername, string aPassword, string aEntropy) instead.
        /// </summary>
        /// <param name="aEmail"></param>
        /// <param name="aUsername"></param>
        /// <param name="aPassword"></param>
        /// <returns></returns>
        [Obsolete]
        public string GenerateRootSeed(string aEmail, string aUsername, string aPassword)
        {
            byte[] lbuff = new byte[16];
            Random rnd = new Random((aEmail + aUsername + aPassword).GetHashCode());
            rnd.NextBytes(lbuff);
            var s = BitConverter.ToString(lbuff);
            return s.Replace("-", "");
            //return "618AF711D8A040F6B97D41ABE455D92F";
        }
    }

}