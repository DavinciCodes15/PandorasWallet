using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandora.Client.Crypto.Currencies.DataEncoders;

namespace Pandora.Client.Crypto.Currencies.Bitcoin.Alt
{
    public class BlackcoinNetwork : BitcoinNetwork
    {
        private static List<BlackcoinNetwork> FNetworks;

        protected BlackcoinNetwork(int aNet)
            : base(aNet)
        {
        }

        public override string NetworkName
        {
            get { return "Blackcoin"; }
        }

        public new static BlackcoinNetwork GetMainNet()
        {
            return GetNetworks()[MainNet];
        }

        public new static BlackcoinNetwork[] GetNetworks()
        {
            if (FNetworks == null)
            {

                FNetworks = new List<BlackcoinNetwork>();

                FNetworks.Add(new BlackcoinNetwork(Network.MainNet));
                FNetworks.Add(new BlackcoinNetwork(Network.TestNet));
            }
            return FNetworks.ToArray();
        }

        protected override void InitMainNet()
        {

            SetBase58Prefixes(Base58Type.PUBKEY_ADDRESS, new byte[] { (25) });
            SetBase58Prefixes(Base58Type.SCRIPT_ADDRESS, new byte[] { (85) });
            SetBase58Prefixes(Base58Type.SECRET_KEY, new byte[] { (153) });
            SetBase58Prefixes(Base58Type.EXT_PUBLIC_KEY, new byte[] { (0x04), (0x88), (0xB2), (0x1E) });
            SetBase58Prefixes(Base58Type.EXT_SECRET_KEY, new byte[] { (0x04), (0x88), (0xAD), (0xE4) });
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
