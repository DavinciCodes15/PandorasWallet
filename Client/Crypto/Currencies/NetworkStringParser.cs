using Pandora.Client.Crypto.Currencies.DataEncoders;

namespace Pandora.Client.Crypto.Currencies
{
    /// <summary>
    /// This class provide a hook for additionnal string format in altcoin network
    /// </summary>
    public class NetworkStringParser
    {
        /// <summary>
        /// Try to parse a string
        /// </summary>
        /// <param name="str">The string to parse</param>
        /// <param name="result">The result</param>
        /// <returns>True if it was possible to parse the string</returns>
        public virtual bool TryParse<T>(string str, Network network, out T result) where T : ICryptoCurrencyString
        {
            result = default(T);
            return false;
        }

        public virtual Base58CheckEncoder GetBase58CheckEncoder()
        {
            return (Base58CheckEncoder)Encoders.Base58Check;
        }

        public virtual CoinPubKeyAddress CreateP2PKH(KeyId keyId, Network network)
        {
            return new CoinPubKeyAddress(keyId, network);
        }

        //public virtual CoinPubKeyAddress CreateP2SH(ScriptId scriptId, Network network)
        //{
        //    return new CoinPubKeyAddress(scriptId, network);
        //}

        public virtual CoinAddress CreateP2WPKH(WitKeyId witKeyId, Network network)
        {
            return new CoinWitPubKeyAddress(witKeyId, network);
        }

        public virtual CoinAddress CreateP2WSH(WitScriptId scriptId, Network network)
        {
            return new CoinWitScriptAddress(scriptId, network);
        }
    }
}

