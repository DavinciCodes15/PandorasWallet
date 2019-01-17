using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandora.Client.Crypto.Currencies.DataEncoders;

namespace Pandora.Client.Crypto.Currencies.Bitcoin.Alt
{
    public class NovacoinNetwork : BitcoinNetwork
    {
        protected NovacoinNetwork(int aNet)
            : base(aNet)
        {
        }

        public override string NetworkName
        {
            get { return "Novacoin"; }
        }

        public new static NovacoinNetwork GetMainNet()
        {
            return GetNetworks()[MainNet];
        }

        private static List<NovacoinNetwork> FNetworks;

        public new static NovacoinNetwork[] GetNetworks()
        {
            if (FNetworks == null)
            {

                FNetworks = new List<NovacoinNetwork>();

                FNetworks.Add(new NovacoinNetwork(Network.MainNet));
                FNetworks.Add(new NovacoinNetwork(Network.TestNet));
            }
            return FNetworks.ToArray();
        }

        protected override void InitMainNet()
        {

            SetBase58Prefixes(Base58Type.PUBKEY_ADDRESS, new byte[] { (8) });
            SetBase58Prefixes(Base58Type.SCRIPT_ADDRESS, new byte[] { (20) });
            SetBase58Prefixes(Base58Type.SECRET_KEY, new byte[] { (136) });
            SetBase58Prefixes(Base58Type.EXT_PUBLIC_KEY, new byte[] { (0x07), (0xE8), (0xF8), (0x9C) });
            SetBase58Prefixes(Base58Type.EXT_SECRET_KEY, new byte[] { (0x04), (0x74), (0xA1), (0x37) });
        }

        protected override void InitTestNet()
        {
            InitMainNet();
            SetBase58Prefixes(Base58Type.PUBKEY_ADDRESS, new byte[] { (111) });
            SetBase58Prefixes(Base58Type.SCRIPT_ADDRESS, new byte[] { (196) });
            SetBase58Prefixes(Base58Type.SECRET_KEY, new byte[] { (239) });
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
