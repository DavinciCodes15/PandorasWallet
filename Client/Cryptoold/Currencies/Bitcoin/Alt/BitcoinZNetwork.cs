using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandora.Crypto.Currencies.DataEncoders;

namespace Pandora.Crypto.Currencies.Bitcoin.Alt
{
    public class BticoinZNetwork : BitcoinNetwork
    {
        protected BticoinZNetwork(int aNet)
            : base(aNet)
        {
        }

        public override string NetworkName
        {
            get { return "BticoinZ"; }
        }

        public new static BticoinZNetwork GetMainNet()
        {
            return GetNetworks()[MainNet];
        }

        private static List<BticoinZNetwork> FNetworks;

        public new static BticoinZNetwork[] GetNetworks()
        {
            if (FNetworks == null)
            {

                FNetworks = new List<BticoinZNetwork>();

                FNetworks.Add(new BticoinZNetwork(Network.MainNet));
                FNetworks.Add(new BticoinZNetwork(Network.TestNet));
            }
            return FNetworks.ToArray();
        }

        protected override void InitMainNet()
        {

            SetBase58Prefixes(Base58Type.PUBKEY_ADDRESS, new byte[] { (0x1C), (0xB8) });
            SetBase58Prefixes(Base58Type.SCRIPT_ADDRESS, new byte[] { (0x1C), (0xBD) });
            SetBase58Prefixes(Base58Type.SECRET_KEY, new byte[] { (0x80) });
            SetBase58Prefixes(Base58Type.EXT_PUBLIC_KEY, new byte[] { (0x04), (0x88), (0xB2), (0x1E) });
            SetBase58Prefixes(Base58Type.EXT_SECRET_KEY, new byte[] { (0x04), (0x88), (0xAD), (0xE4) });
        }

        protected override void InitTestNet()
        {
            InitMainNet();
            SetBase58Prefixes(Base58Type.PUBKEY_ADDRESS, new byte[] { (0x1D), (0x25) });
            SetBase58Prefixes(Base58Type.SCRIPT_ADDRESS, new byte[] { (0x1C), (0xBA) });
            SetBase58Prefixes(Base58Type.SECRET_KEY, new byte[] { (0xEF) });
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
