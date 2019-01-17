using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandora.Client.Crypto.Currencies.DataEncoders;

namespace Pandora.Client.Crypto.Currencies.Bitcoin.Alt
{
    public class ArcticCoinNetwork : BitcoinNetwork
    {
        protected ArcticCoinNetwork(int aNet)
            : base(aNet)
        {
        }

        public override string NetworkName
        {
            get { return "ArcticCoin"; }
        }

        public new static ArcticCoinNetwork GetMainNet()
        {
            return GetNetworks()[MainNet];
        }

        private static List<ArcticCoinNetwork> FNetworks;

        public new static ArcticCoinNetwork[] GetNetworks()
        {
            if (FNetworks == null)
            {

                FNetworks = new List<ArcticCoinNetwork>();

                FNetworks.Add(new ArcticCoinNetwork(Network.MainNet));
                FNetworks.Add(new ArcticCoinNetwork(Network.TestNet));
            }
            return FNetworks.ToArray();
        }

        protected override void InitMainNet()
        {

            SetBase58Prefixes(Base58Type.PUBKEY_ADDRESS, new byte[] { (23) });
            SetBase58Prefixes(Base58Type.SCRIPT_ADDRESS, new byte[] { (8) });
            SetBase58Prefixes(Base58Type.SECRET_KEY, new byte[] { (176) });
            SetBase58Prefixes(Base58Type.EXT_PUBLIC_KEY, new byte[] { (0x07), (0xE8), (0xF8), (0x9C) });
            SetBase58Prefixes(Base58Type.EXT_SECRET_KEY, new byte[] { (0x04), (0x74), (0xA1), (0x37) });
        }

        protected override void InitTestNet()
        {
            InitMainNet();
            SetBase58Prefixes(Base58Type.PUBKEY_ADDRESS, new byte[] { (83) });
            SetBase58Prefixes(Base58Type.SCRIPT_ADDRESS, new byte[] { (9) });
            SetBase58Prefixes(Base58Type.SECRET_KEY, new byte[] { (239) });
            SetBase58Prefixes(Base58Type.EXT_PUBLIC_KEY, new byte[] { (0x09), (0x72), (0x98), (0xBF) });
            SetBase58Prefixes(Base58Type.EXT_SECRET_KEY, new byte[] { (0x09), (0x62), (0x3A), (0x6F) });
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
