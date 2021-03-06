﻿using Pandora.Client.Crypto.Currencies.DataEncoders;
using System;
using System.Linq;
using System.Text;

namespace Pandora.Client.Crypto.Currencies
{
    /// <summary>
    /// Base58 representaiton of a script hash
    /// </summary>
    public class CoinScriptAddress : CoinAddress
    {
        private const int Base58Type = Network.BASE58_SCRIPT_ADDRESS;

        public CoinScriptAddress(string base58, Network expectedNetwork)
            : base(Validate(base58, ref expectedNetwork), expectedNetwork)
        {
            var decoded = Encoders.Base58Check.DecodeData(base58);
            _Hash = new ScriptId(new uint160(decoded.Skip(expectedNetwork.GetVersionBytes(Type, true).Length).ToArray()));
        }

        private static string Validate(string base58, ref Network expectedNetwork)
        {
            if (base58 == null)
                throw new ArgumentNullException("base58");
            var networks = new[] { expectedNetwork };
            var data = Encoders.Base58Check.DecodeData(base58);
            foreach (var network in networks)
            {
                var versionBytes = network.GetVersionBytes(Base58Type, false);
                if (versionBytes != null && data.StartWith(versionBytes))
                {
                    if (data.Length == versionBytes.Length + 20)
                    {
                        expectedNetwork = network;
                        return base58;
                    }
                }
            }
            throw new FormatException("Invalid CoinScriptAddress " + expectedNetwork.NetworkName);
        }

        public static bool IsValid(string aAddress, Network aNetwork)
        {
            return AddressToBin(aAddress, aNetwork) != null;
        }

        public static byte[] AddressToBin(string aAddress, Network aNetwork)
        {
            byte[] lData = null;
            const int SCRIPT_ADDRESS = 1;
            const int PUBKEY_ADDRESS = 0;
            if (!string.IsNullOrEmpty(aAddress))
            {
                lData = (aNetwork.NetworkStringParser.GetBase58CheckEncoder()).IsValidData(aAddress) ?
                    (aNetwork.NetworkStringParser.GetBase58CheckEncoder()).DecodeData(aAddress) : null;
                if (lData != null)
                {
                    var lScriptVer = aNetwork.GetVersionBytes(SCRIPT_ADDRESS, true);
                    var lPubVer = aNetwork.GetVersionBytes(PUBKEY_ADDRESS, true);

                    if (!(((lScriptVer != null && lData.StartWith(lScriptVer)) && (lData.Length == lScriptVer.Length + 20)) ||
                        ((lPubVer != null && lData.StartWith(lPubVer)) && (lData.Length == lPubVer.Length + 20))))
                        lData = null;
                }
                else if (aNetwork.ChainParams.Capabilities.HasFlag(CapablityFlags.SegWitSupport))
                {
                    int i = Network.BEACH32_WITNESS_PUBKEY_ADDRESS;

                    while (lData == null && i < Network.BEACH32_WITNESS_SCRIPT_ADDRESS)
                    {
                        var encoder = aNetwork.GetBech32Encoder(i, true);
                        if (encoder == null)
                            continue;
                        Int32 type = i;
                        i++;
                        try
                        {
                            byte witVersion;
                            var bytes = encoder.Decode(aAddress, out witVersion);

                            if (witVersion == 0 && bytes.Length == 20 && type == Network.BEACH32_WITNESS_PUBKEY_ADDRESS)
                                lData = bytes;
                            else if (witVersion == 0 && bytes.Length == 32 && type == Network.BEACH32_WITNESS_SCRIPT_ADDRESS)
                                lData = bytes;
                        }
                        catch (Bech32FormatException) { throw; }
                        catch (FormatException) { continue; }
                    }
                }
            }
            return lData;
        }

        public static string AddressToBinString(string aAddress, Network aNetwork)
        {
            var lData = AddressToBin(aAddress, aNetwork);
            if (lData == null) return null;
            return BitConverter.ToString(lData).Replace("-", "");
        }

        public CoinScriptAddress(ScriptId scriptId, Network network)
            : base(NotNull(scriptId) ?? Network.CreateBase58(Base58Type, scriptId.ToBytes(), network), network)
        {
            _Hash = scriptId;
        }

        private static string NotNull(ScriptId scriptId)
        {
            if (scriptId == null)
                throw new ArgumentNullException("scriptId");
            return null;
        }

        private ScriptId _Hash;

        public ScriptId Hash
        {
            get
            {
                return _Hash;
            }
        }

        protected override Script GeneratePaymentScript()
        {
            //throw new NotImplementedException();
            return PayToScriptHashTemplate.Instance.GenerateScriptPubKey((ScriptId)Hash);
        }
    }

    /// <summary>
    /// Base58 representation of a Coin address
    /// </summary>
    public abstract class CoinAddress : BinaryEncoding, IDestination, ICryptoCurrencyString
    {
        /// <summary>
        /// Detect whether the input base58 is a pubkey hash or a script hash
        /// </summary>
        /// <param name="str">The string to parse</param>
        /// <param name="expectedNetwork">The expected network to which it belongs</param>
        /// <returns>A CoinAddress or CoinScriptAddress</returns>
        /// <exception cref="System.FormatException">Invalid format</exception>
        public static CoinAddress Create(string str, Network expectedNetwork = null)
        {
            if (str == null)
                throw new ArgumentNullException("base58");
            return Network.Parse<CoinAddress>(str, expectedNetwork);
        }

        public CoinAddress(string str, Network network)
        {
            this.Type = Network.BASE58_SCRIPT_ADDRESS;
            if (network == null)
                throw new ArgumentNullException("network");
            if (str == null)
                throw new ArgumentNullException("str");
            _Str = str;
            _Network = network;
        }

        private string _Str;

        private Script _ScriptPubKey;

        public Script ScriptPubKey
        {
            get
            {
                if (_ScriptPubKey == null)
                {
                    _ScriptPubKey = GeneratePaymentScript();
                }
                return _ScriptPubKey;
            }
        }

        protected abstract Script GeneratePaymentScript();

        public CoinScriptAddress GetScriptAddress()
        {
            var lCoinScriptAddress = this as CoinScriptAddress;
            if (lCoinScriptAddress != null)
                return lCoinScriptAddress;

            return new CoinScriptAddress(this.ScriptPubKey.Hash, Network);
        }

        //public CoinColoredAddress ToColoredAddress()
        //{
        //    return new CoinColoredAddress(this);
        //}

        private readonly Network _Network;

        public override Network Network
        {
            get
            {
                return _Network;
            }
        }

        public override string ToString()
        {
            return _Str;
        }

        public override bool Equals(object obj)
        {
            CoinAddress item = obj as CoinAddress;
            if (item == null)
                return false;
            return _Str.Equals(item._Str);
        }

        public static bool operator ==(CoinAddress a, CoinAddress b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;
            if (((object)a == null) || ((object)b == null))
                return false;
            return a._Str == b._Str;
        }

        public static bool operator !=(CoinAddress a, CoinAddress b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return _Str.GetHashCode();
        }
    }
}