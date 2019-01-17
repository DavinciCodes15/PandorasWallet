using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandora.Client.Crypto.Currencies.DataEncoders;

namespace Pandora.Client.Crypto.Currencies.Bitcoin.Alt
{
    public class ZenniesNetwork : BitcoinNetwork
    {
        protected ZenniesNetwork(int aNet)
            : base(aNet)
        {
        }

        public override string NetworkName
        {
            get { return "Zennies"; }
        }

        public new static ZenniesNetwork GetMainNet()
        {
            return GetNetworks()[MainNet];
        }

        private static List<ZenniesNetwork> FNetworks;

        public new static ZenniesNetwork[] GetNetworks()
        {
            if (FNetworks == null)
            {

                FNetworks = new List<ZenniesNetwork>();

                FNetworks.Add(new ZenniesNetwork(Network.MainNet));
                FNetworks.Add(new ZenniesNetwork(Network.TestNet));
            }
            return FNetworks.ToArray();
        }

        protected override void InitMainNet()
        {

            SetBase58Prefixes(Base58Type.PUBKEY_ADDRESS, new byte[] { (80) });
            SetBase58Prefixes(Base58Type.SCRIPT_ADDRESS, new byte[] { (72) });
            SetBase58Prefixes(Base58Type.SECRET_KEY, new byte[] { (142) });
            SetBase58Prefixes(Base58Type.EXT_PUBLIC_KEY, new byte[] { (0x04), (0x73), (0xDC), (0x3B) });
            SetBase58Prefixes(Base58Type.EXT_SECRET_KEY, new byte[] { (0x04), (0x73), (0x6C), (0x1C) });
        }

        protected override void InitTestNet()
        {
            InitMainNet();
            SetBase58Prefixes(Base58Type.PUBKEY_ADDRESS, new byte[] { (120) });
            SetBase58Prefixes(Base58Type.SCRIPT_ADDRESS, new byte[] { (196) });
            SetBase58Prefixes(Base58Type.SECRET_KEY, new byte[] { (239) });
            SetBase58Prefixes(Base58Type.EXT_PUBLIC_KEY, new byte[] { (0x04), (0x35), (0x17), (0xFF) });
            SetBase58Prefixes(Base58Type.EXT_SECRET_KEY, new byte[] { (0x04), (0x35), (0x43), (0x99) });
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
