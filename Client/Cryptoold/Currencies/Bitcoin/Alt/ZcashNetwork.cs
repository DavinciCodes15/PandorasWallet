using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandora.Client.Crypto.Currencies.DataEncoders;

namespace Pandora.Client.Crypto.Currencies.Bitcoin.Alt
{
    public class ZcashNetwork : BitcoinNetwork
    {
        protected ZcashNetwork(int aNet)
            : base(aNet)
        {
        }

        public override string NetworkName
        {
            get { return "Zcash"; }
        }

        public new static ZcashNetwork GetMainNet()
        {
            return GetNetworks()[MainNet];
        }

        private static List<ZcashNetwork> FNetworks;

        public new static ZcashNetwork[] GetNetworks()
        {
            if (FNetworks == null)
            {

                FNetworks = new List<ZcashNetwork>();

                FNetworks.Add(new ZcashNetwork(Network.MainNet));
                FNetworks.Add(new ZcashNetwork(Network.TestNet));
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
