using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandora.Client.Crypto.Currencies.DataEncoders;

namespace Pandora.Client.Crypto.Currencies.Bitcoin.Alt
{
    public class EspersNetwork : BitcoinNetwork
    {
        protected EspersNetwork(int aNet)
            : base(aNet)
        {
        }

        public override string NetworkName
        {
            get { return "Espers"; }
        }

        public new static EspersNetwork GetMainNet()
        {
            return GetNetworks()[MainNet];
        }

        private static List<EspersNetwork> FNetworks;

        public new static EspersNetwork[] GetNetworks()
        {
            if (FNetworks == null)
            {

                FNetworks = new List<EspersNetwork>();

                FNetworks.Add(new EspersNetwork(Network.MainNet));
                FNetworks.Add(new EspersNetwork(Network.TestNet));
            }
            return FNetworks.ToArray();
        }

        protected override void InitMainNet()
        {

            SetBase58Prefixes(Base58Type.PUBKEY_ADDRESS, new byte[] { (33) });
            SetBase58Prefixes(Base58Type.SCRIPT_ADDRESS, new byte[] { (92) });
            SetBase58Prefixes(Base58Type.SECRET_KEY, new byte[] { (144) });
            SetBase58Prefixes(Base58Type.EXT_PUBLIC_KEY, new byte[] { (0x04), (0x88), (0xB2), (0x1E) });
            SetBase58Prefixes(Base58Type.EXT_SECRET_KEY, new byte[] { (0x04), (0x88), (0xAD), (0xE4) });
        }

        protected override void InitTestNet()
        {
            InitMainNet();
            SetBase58Prefixes(Base58Type.PUBKEY_ADDRESS, new byte[] { (34) });
            SetBase58Prefixes(Base58Type.SCRIPT_ADDRESS, new byte[] { (94) });
            SetBase58Prefixes(Base58Type.SECRET_KEY, new byte[] { (143) });
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
