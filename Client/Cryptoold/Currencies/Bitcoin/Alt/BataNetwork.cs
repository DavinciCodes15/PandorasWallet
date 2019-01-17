using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandora.Client.Crypto.Currencies.DataEncoders;

namespace Pandora.Client.Crypto.Currencies.Bitcoin.Alt
{
    public class BataNetwork : BitcoinNetwork
    {
        protected BataNetwork(int aNet)
            : base(aNet)
        {
        }

        public override string NetworkName
        {
            get { return "Bata"; }
        }

        public new static BataNetwork GetMainNet()
        {
            return GetNetworks()[MainNet];
        }

        private static List<BataNetwork> FNetworks;

        public new static BataNetwork[] GetNetworks()
        {
            if (FNetworks == null)
            {

                FNetworks = new List<BataNetwork>();

                FNetworks.Add(new BataNetwork(Network.MainNet));
                FNetworks.Add(new BataNetwork(Network.TestNet));
            }
            return FNetworks.ToArray();
        }

        protected override void InitMainNet()
        {

            SetBase58Prefixes(Base58Type.PUBKEY_ADDRESS, new byte[] { (25) });
            SetBase58Prefixes(Base58Type.SCRIPT_ADDRESS, new byte[] { (85) });
            SetBase58Prefixes(Base58Type.SECRET_KEY, new byte[] { (188) });
            SetBase58Prefixes(Base58Type.EXT_PUBLIC_KEY, new byte[] { (0xA4), (0x0C), (0x86), (0xFA) });
            SetBase58Prefixes(Base58Type.EXT_SECRET_KEY, new byte[] { (0xA4), (0x0B), (0x91), (0xBD) });
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
