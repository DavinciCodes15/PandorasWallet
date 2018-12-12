using System;
using System.Collections.Generic;
using System.Linq;
using Pandora.Client.Crypto.Currencies.DataEncoders;
using Base58Type = System.Int32;

using Bech32Type = System.Int32;

namespace Pandora.Client.Crypto.Currencies
{
    public abstract class Network
    {
        public const int MainNet = 0;
        public const int TestNet = 1;

        public const int BASE58_PUBKEY_ADDRESS = 0;
        public const int BASE58_SCRIPT_ADDRESS = 1;
        public const int BASE58_SECRET_KEY = 2;
        public const int BASE58_EXT_PUBLIC_KEY = 3;
        public const int BASE58_EXT_SECRET_KEY = 4;
        public const int BASE58_ENCRYPTED_SECRET_KEY_EC = 5;
        public const int BASE58_ENCRYPTED_SECRET_KEY_NO_EC = 6;
        public const int BASE58_PASSPHRASE_CODE = 7;
        public const int BASE58_CONFIRMATION_CODE = 8;

        public const int BEACH32_WITNESS_PUBKEY_ADDRESS = 30;
        public const int BEACH32_WITNESS_SCRIPT_ADDRESS = 31;


        protected Network(IChianParams aChainParams)
        {
            ChainParams = aChainParams;
            Net = (int)aChainParams.Network;
            NetworkName = aChainParams.NetworkName;
            SetChainParams();
        }

        public IChianParams ChainParams { get; protected set; }


        protected abstract void SetChainParams();

        public int Net
        {
            get;
            private set;
        }

        public virtual string NetworkName { get; protected set; }

        public int Port { get; protected set; }


        protected byte[][] base58Prefixes = new byte[20][];
        protected Bech32Encoder[] bech32Encoders = new Bech32Encoder[2];

        private Base58Type? GetBase58Type(string base58)
        {
            var bytes = Encoders.Base58Check.DecodeData(base58);
            for (int i = 0; i < base58Prefixes.Length; i++)
            {
                var prefix = base58Prefixes[i];
                if (prefix == null)
                    continue;
                if (bytes.Length < prefix.Length)
                    continue;
                if (Utils.ArrayEqual(bytes, 0, prefix, 0, prefix.Length))
                    return (Base58Type)i;
            }
            return null;
        }

        public static Network GetNetworkFromBase58Data(string base58, Base58Type? expectedType = null)
        {
            //foreach (var network in GetNetworks())
            //{
            //    var lBase58Type = network.GetBase58Type(base58);
            //    if (lBase58Type.HasValue)
            //    {
            //        if (expectedType != null && expectedType.Value != lBase58Type.Value)
            //            continue;
            //        //if(lBase58Type.Value == Base58Type.COLORED_ADDRESS)
            //        //{
            //        //    var raw = Encoders.Base58Check.DecodeData(base58);
            //        //    var version = network.GetVersionBytes(lBase58Type.Value, false);
            //        //    if(version == null)
            //        //        continue;
            //        //    raw = raw.Skip(version.Length).ToArray();
            //        //    base58 = Encoders.Base58Check.EncodeData(raw);
            //        //    return GetNetworkFromBase58Data(base58, null);
            //        //}
            //        return network;
            //    }
            //}
            return null;
        }

        public Bech32Encoder GetBech32Encoder(Bech32Type type, bool throws)
        {
            var encoder = bech32Encoders[(int)type];
            if (encoder == null && throws)
                throw new NotImplementedException("The network " + this + " does not have any prefix for bech32 " + Enum.GetName(typeof(Bech32Type), type));
            return encoder;
        }

        public byte[] GetVersionBytes(Base58Type type, bool throws)
        {
            var prefix = base58Prefixes[(int)type];
            if (prefix == null && throws)
                throw new NotImplementedException("The network " + this + " does not have any prefix for base58 " + Enum.GetName(typeof(Base58Type), type));
            return prefix.ToArray();
        }

        public static string CreateBase58(Base58Type type, byte[] bytes, Network network)
        {
            if (network == null)
                throw new ArgumentNullException("network");
            if (bytes == null)
                throw new ArgumentNullException("bytes");
            var versionBytes = network.GetVersionBytes(type, true);
            return Encoders.Base58Check.EncodeData(versionBytes.Concat(bytes));
        }

        public static string CreateBech32(Bech32Type type, byte[] bytes, byte witnessVersion, Network network)
        {
            if (network == null)
                throw new ArgumentNullException("network");
            if (bytes == null)
                throw new ArgumentNullException("bytes");
            var encoder = network.GetBech32Encoder(type, true);
            return encoder.Encode(witnessVersion, bytes);
        }

        private static IEnumerable<IBinaryEncodable> GetCandidates(IEnumerable<Network> networks, string base58)
        {
            if (base58 == null)
                throw new ArgumentNullException("base58");
            foreach (var network in networks)
            {
                var type = network.GetBase58Type(base58);
                if (type.HasValue)
                {
                    //if(type.Value == Base58Type.COLORED_ADDRESS)
                    //{
                    //    var wrapped = CoinColoredAddress.GetWrappedBase58(base58, network);
                    //    var wrappedType = network.GetBase58Type(wrapped);
                    //    if(wrappedType == null)
                    //        continue;
                    //    try
                    //    {
                    //        var inner = network.CreateBase58Data(wrappedType.Value, wrapped);
                    //        if(inner.Network != network)
                    //            continue;
                    //    }
                    //    catch(FormatException) { }
                    //}
                    IBinaryEncodable data = null;
                    try
                    {
                        data = network.CreateBase58Data(type.Value, base58);
                    }
                    catch (FormatException) { }
                    if (data != null)
                        yield return data;
                }
            }
        }

        private IBinaryEncodable CreateBase58Data(Base58Type type, string base58)
        {
            //if(type == Base58Type.EXT_PUBLIC_KEY)
            //    return CreateCoinExtPubKey(base58);
            //if(type == Network.BASE58_EXT_SECRET_KEY)
            //    return CreateCoinExtKey(base58);
            if (type == Network.BASE58_PUBKEY_ADDRESS)
                return new CoinPubKeyAddress(base58, this);
            if (type == Network.BASE58_SCRIPT_ADDRESS)
                return CreateCoinScriptAddress(base58);
            if (type == Network.BASE58_SECRET_KEY)
                return CreateCoinSecret(base58);
            //if(type == Network.BASE58_CONFIRMATION_CODE)
            //    return CreateConfirmationCode(base58);
            if (type == Network.BASE58_ENCRYPTED_SECRET_KEY_EC)
                return CreateEncryptedKeyEC(base58);
            //if(type == Network.BASE58_ENCRYPTED_SECRET_KEY_NO_EC)
            //    return CreateEncryptedKeyNoEC(base58);
            //if(type == Network.BASE58_PASSPHRASE_CODE)
            //    return CreatePassphraseCode(base58);
            //if(type == Network.BASE58_STEALTH_ADDRESS)
            //    return CreateStealthAddress(base58);
            //if(type == Network.BASE58_ASSET_ID)
            //    return CreateAssetId(base58);
            //if(type == Network.BASE58_COLORED_ADDRESS)
            //    return CreateColoredAddress(base58);
            throw new NotSupportedException("Invalid Base58Data type : " + type.ToString());
        }

        internal NetworkStringParser NetworkStringParser
        {
            get;
            set;
        } = new NetworkStringParser();

        public CoinPubKeyAddress CreateCoinAddress(KeyId dest)
        {
            if (dest == null)
                throw new ArgumentNullException("dest");
            return new CoinPubKeyAddress(dest, this);
        }

        private CoinEncryptedSecretEC CreateEncryptedKeyEC(string base58)
        {
            return new CoinEncryptedSecretEC(base58, this);
        }

        private CoinAddress CreateCoinScriptAddress(ScriptId scriptId)
        {
            return new CoinScriptAddress(scriptId, this);
        }

        public CoinScriptAddress CreateCoinScriptAddress(string base58)
        {
            return new CoinScriptAddress(base58, this);
        }

        public CoinSecret CreateCoinSecret(string base58)
        {
            return new CoinSecret(base58, this);
        }

        public CoinSecret CreateCoinSecret(CCKey key)
        {
            return new CoinSecret(key, this);
        }

        public static T Parse<T>(string str, Network expectedNetwork) where T : ICryptoCurrencyString
        {
            if (str == null)
                throw new ArgumentNullException("str");
            var networks =  new[] { expectedNetwork };
            var maybeb58 = true;
            if (maybeb58)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    if (!Base58Encoder.pszBase58Chars.Contains(str[i]))
                    {
                        maybeb58 = false;
                        break;
                    }
                }
            }
            if (maybeb58)
            {
                try
                {
                    Encoders.Base58Check.DecodeData(str);
                }
                catch (FormatException)
                {
                    maybeb58 = false;
                }
                if (maybeb58)
                {
                    foreach (var candidate in GetCandidates(networks, str))
                    {
                        bool rightNetwork = (candidate.Network == expectedNetwork);
                        bool rightType = candidate is T;
                        if (rightNetwork && rightType)
                            return (T)(object)candidate;
                    }
                    throw new FormatException("Invalid base58 string");
                }
            }

            foreach (var network in networks)
            {
                int i = BEACH32_WITNESS_PUBKEY_ADDRESS;
                foreach (var encoder in network.bech32Encoders)
                {
                    if (encoder == null)
                        continue;
                    var type = (Bech32Type)i;
                    try
                    {
                        byte witVersion;
                        var bytes = encoder.Decode(str, out witVersion);
                        object candidate = null;

                        if (witVersion == 0 && bytes.Length == 20 && type == Network.BEACH32_WITNESS_PUBKEY_ADDRESS)
                            candidate = new CoinWitPubKeyAddress(str, network);
                        if (witVersion == 0 && bytes.Length == 32 && type == Network.BEACH32_WITNESS_SCRIPT_ADDRESS)
                            candidate = new CoinWitScriptAddress(str, network);

                        if (candidate is T)
                            return (T)(object)candidate;
                    }
                    catch (Bech32FormatException) { throw; }
                    catch (FormatException) { continue; }
                    i++;
                }
            }

            throw new FormatException("Invalid string");
        }
    }
}