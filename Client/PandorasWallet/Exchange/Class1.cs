using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.PandorasWallet.Exchange
{
    internal abstract class APIBase
    {
        public APIBase(string aAccountName)
        {
            AccountName = aAccountName;
        }

        public APIBase(string aAccountName, string aAPIKey, string aAPISecret)
        {
            AccountName = aAccountName;
            APIKey = aAPIKey;
            APISecret = aAPISecret;
        }

        public string AccountName { get; private set; }
        public string APIKey { get; private set; }
        protected string APISecret { get; private set; }

        /// <summary>
        /// The currrency pair will be displayed as string the way math operations are cared out
        /// or in simple terms the (Currency it is priced in)/(Currency You want)
        /// Example BTC/LTC = 0.1  is priced in btc for each LTC you have.
        /// So fi the exchange has it backwords it must be placed correctly.
        /// </summary>
        /// Returns an array of supported currency pairs all upper case and 
        /// formated TICKER/TICKER example USD/BTC
        /// <returns></returns>
        public abstract string[] GetCurrencyPairs();

    }
}
