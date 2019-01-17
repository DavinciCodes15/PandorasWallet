using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandora.Client.Crypto.Currencies.DataEncoders;

namespace Pandora.Client.Crypto.Currencies.Bitcoin.Alt
{
    public class BitcloudNetwork : BitcoinNetwork
    {
        protected BitcloudNetwork(int aNet)
            : base(aNet)
        {
        }

        public override string NetworkName
        {
            get { return "Bitcloud"; }
        }

        public new static BitcloudNetwork GetMainNet()
        {
            return GetNetworks()[MainNet];
        }

        private static List<BitcloudNetwork> FNetworks;

        public new static BitcloudNetwork[] GetNetworks()
        {
            if (FNetworks == null)
            {

                FNetworks = new List<BitcloudNetwork>();

                FNetworks.Add(new BitcloudNetwork(Network.MainNet));
                FNetworks.Add(new BitcloudNetwork(Network.TestNet));
            }
            return FNetworks.ToArray();
        }

        protected override void InitMainNet()
        {

            SetBase58Prefixes(Base58Type.PUBKEY_ADDRESS, new byte[] { (25) });
            SetBase58Prefixes(Base58Type.SCRIPT_ADDRESS, new byte[] { (5) });
            SetBase58Prefixes(Base58Type.SECRET_KEY, new byte[] { (153) });
            SetBase58Prefixes(Base58Type.EXT_PUBLIC_KEY, new byte[] { (0x04), (0x88), (0xB2), (0x1E) });
            SetBase58Prefixes(Base58Type.EXT_SECRET_KEY, new byte[] { (0x04), (0x88), (0xAD), (0xE4) });
        }

        protected override void InitTestNet()
        {
            InitMainNet();
            SetBase58Prefixes(Base58Type.PUBKEY_ADDRESS, new byte[] { (139) });
            SetBase58Prefixes(Base58Type.SCRIPT_ADDRESS, new byte[] { (19) });
            SetBase58Prefixes(Base58Type.SECRET_KEY, new byte[] { (239) });
            SetBase58Prefixes(Base58Type.EXT_PUBLIC_KEY, new byte[] { (0x3a), (0x80), (0x61), (0xa0) });
            SetBase58Prefixes(Base58Type.EXT_SECRET_KEY, new byte[] { (0x3a), (0x80), (0x58), (0x37) });
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
