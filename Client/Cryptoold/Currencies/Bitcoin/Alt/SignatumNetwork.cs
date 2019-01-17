using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandora.Client.Crypto.Currencies.DataEncoders;

namespace Pandora.Client.Crypto.Currencies.Bitcoin.Alt
{
    public class SignatumNetwork : BitcoinNetwork
    {
        protected SignatumNetwork(int aNet)
            : base(aNet)
        {
        }

        public override string NetworkName
        {
            get { return "Signatum"; }
        }

        public new static SignatumNetwork GetMainNet()
        {
            return GetNetworks()[MainNet];
        }

        private static List<SignatumNetwork> FNetworks;

        public new static SignatumNetwork[] GetNetworks()
        {
            if (FNetworks == null)
            {

                FNetworks = new List<SignatumNetwork>();

                FNetworks.Add(new SignatumNetwork(Network.MainNet));
                FNetworks.Add(new SignatumNetwork(Network.TestNet));
            }
            return FNetworks.ToArray();
        }

        protected override void InitMainNet()
        {

            SetBase58Prefixes(Base58Type.PUBKEY_ADDRESS, new byte[] { (25) });
            SetBase58Prefixes(Base58Type.SCRIPT_ADDRESS, new byte[] { (125) });
            SetBase58Prefixes(Base58Type.SECRET_KEY, new byte[] { (63+128) });
            SetBase58Prefixes(Base58Type.EXT_PUBLIC_KEY, new byte[] { (0x04), (0x88), (0xC2), (0x1E) });
            SetBase58Prefixes(Base58Type.EXT_SECRET_KEY, new byte[] { (0x04), (0x88), (0xB2), (0xDD) });
        }

        protected override void InitTestNet()
        {
            InitMainNet();
            SetBase58Prefixes(Base58Type.PUBKEY_ADDRESS, new byte[] { (65) });
            SetBase58Prefixes(Base58Type.SCRIPT_ADDRESS, new byte[] { (196) });
            SetBase58Prefixes(Base58Type.SECRET_KEY, new byte[] { (65+128) });
            SetBase58Prefixes(Base58Type.EXT_PUBLIC_KEY, new byte[] { (0x04), (0x35), (0x87), (0xCF) });
            SetBase58Prefixes(Base58Type.EXT_SECRET_KEY, new byte[] { (0x04), (0x35), (0x83), (0x94) });
        }

        public override Network[] Networks
        {
            get
            {
                return FNetworks.ToArray<Network>();
            }
        }
    }
}
