using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandora.Client.Crypto.Currencies.DataEncoders;

namespace Pandora.Client.Crypto.Currencies.Bitcoin
{
    public class BitcoinNetwork : Network
    {
        protected enum Base58Type
        {
            PUBKEY_ADDRESS,
            SCRIPT_ADDRESS,
            SECRET_KEY,
            EXT_PUBLIC_KEY,
            EXT_SECRET_KEY,
            ENCRYPTED_SECRET_KEY_EC,
            ENCRYPTED_SECRET_KEY_NO_EC,
            PASSPHRASE_CODE,
            CONFIRMATION_CODE,
            STEALTH_ADDRESS,
            ASSET_ID,
            COLORED_ADDRESS,
            MAX_BASE58_TYPES,
        }

        protected enum Bech32Type
        {
            WITNESS_PUBKEY_ADDRESS,
            WITNESS_SCRIPT_ADDRESS
        }

        public BitcoinNetwork(IChianParams aChainparams) : base(aChainparams)
        {

        }

        public BitcoinNetwork(int aNet) : base(new ChainParams())
        {
        }


    //public static BitcoinNetwork GetMainNet()
    //{
    //    return GetNetworks()[MainNet];
    //}

    protected override void SetChainParams()
        {
            SetBase58Prefixes(Base58Type.PUBKEY_ADDRESS, ChainParams.PublicKeyAddress);
            SetBase58Prefixes(Base58Type.SCRIPT_ADDRESS, ChainParams.ScriptAddress);
            SetBase58Prefixes(Base58Type.SECRET_KEY, ChainParams.SecretKey);
            SetBase58Prefixes(Base58Type.EXT_PUBLIC_KEY, ChainParams.ExtPublicKey);
            SetBase58Prefixes(Base58Type.EXT_SECRET_KEY, ChainParams.ExtSecretKey);

            SetBase58Prefixes(Base58Type.ENCRYPTED_SECRET_KEY_NO_EC, ChainParams.EncryptedSecretKeyNoEc);
            SetBase58Prefixes(Base58Type.ENCRYPTED_SECRET_KEY_EC, ChainParams.EncryptedSecretKeyEc);
            SetBase58Prefixes(Base58Type.PASSPHRASE_CODE, ChainParams.PasspraseCode);
            SetBase58Prefixes(Base58Type.CONFIRMATION_CODE, ChainParams.ConfirmationCode);
            SetBase58Prefixes(Base58Type.STEALTH_ADDRESS, ChainParams.StealthAddress);
            SetBase58Prefixes(Base58Type.ASSET_ID, ChainParams.AssetId);
            SetBase58Prefixes(Base58Type.COLORED_ADDRESS, ChainParams.ColoredAddress);

            var encoder = new Bech32Encoder(ChainParams.Encoder);
            bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;
            //if (ChainParams is IServerChainParams)
            //    Port = (ChainParams as ServerChainParams).Port;
        }

        //private static List<BitcoinNetwork> FNetworks;

        //public static BitcoinNetwork[] GetNetworks()
        //{
        //    if (FNetworks == null)
        //    {
        //        FNetworks = new List<BitcoinNetwork>();

        //        FNetworks.Add(new BitcoinNetwork(Network.MainNet));
        //        FNetworks.Add(new BitcoinNetwork(Network.TestNet));
        //    }
        //    return FNetworks.ToArray();
        //}

        protected override void InitMainNet()
        {
            SetChainParams();
        }

        protected override void InitTestNet()
        {
            InitMainNet();
        }

        public override Network[] Networks
        {
            get
            {
                return FNetworks.ToArray<Network>();
            }
        }

        protected void SetBase58Prefixes(Base58Type aType, byte[] aPrefix)
        {
            base58Prefixes[(int)aType] = aPrefix;
        }

    }
}