using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandora.Client.Crypto.Currencies.DataEncoders;

namespace Pandora.Client.Crypto.Currencies.Bitcoin.Alt
{
    public class DogecoinNetwork : BitcoinNetwork
    {
        private static List<DogecoinNetwork> FNetworks;

        protected DogecoinNetwork(int aNet)
            : base(aNet)
        {
        }

        public override string NetworkName
        {
            get { return "Dogecoin"; }
        }

        public new static DogecoinNetwork GetMainNet()
        {
            return GetNetworks()[MainNet];
        }

        public new static DogecoinNetwork[] GetNetworks()
        {
            if (FNetworks == null)
            {

                FNetworks = new List<DogecoinNetwork>();

                FNetworks.Add(new DogecoinNetwork(Network.MainNet));
                FNetworks.Add(new DogecoinNetwork(Network.TestNet));
            }
            return FNetworks.ToArray();
        }

        protected override void InitMainNet()
        {

            SetBase58Prefixes(Base58Type.PUBKEY_ADDRESS, new byte[] { (30) });
            SetBase58Prefixes(Base58Type.SCRIPT_ADDRESS, new byte[] { (22) });
            SetBase58Prefixes(Base58Type.SECRET_KEY, new byte[] { (158) });
            SetBase58Prefixes(Base58Type.EXT_PUBLIC_KEY, new byte[] { (0x02), (0xfa), (0xca), (0xfd) });
            SetBase58Prefixes(Base58Type.EXT_SECRET_KEY, new byte[] { (0x02), (0xfa), (0xc3), (0x98) });
        }

        protected override void InitTestNet()
        {
            InitMainNet();
            SetBase58Prefixes(Base58Type.PUBKEY_ADDRESS, new byte[] { (113) });
            SetBase58Prefixes(Base58Type.SCRIPT_ADDRESS, new byte[] { (196) });
            SetBase58Prefixes(Base58Type.SECRET_KEY, new byte[] { (241) });
            SetBase58Prefixes(Base58Type.EXT_PUBLIC_KEY, new byte[] { (0x04), (0x35), (0x87), (0xcf) });
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
