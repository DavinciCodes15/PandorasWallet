using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandora.Client.Crypto.Currencies.DataEncoders;

namespace Pandora.Client.Crypto.Currencies.Bitcoin.Alt
{
    public class ALQONetwork : BitcoinNetwork
    {
       private static List<ALQONetwork> FNetworks;
       protected ALQONetwork(int aNet)
            : base(aNet)
        {
        }

        public override string NetworkName
        {
            get { return "ALQO"; }
        }

#pragma warning disable 414, CS0108
        public static ALQONetwork GetMainNet()
        {
            return GetNetworks()[MainNet];
        }

        public static ALQONetwork[] GetNetworks()
        {
            if (FNetworks == null)
            {

                FNetworks = new List<ALQONetwork>();

                FNetworks.Add(new ALQONetwork(Network.MainNet));
                FNetworks.Add(new ALQONetwork(Network.TestNet));
            }
            return FNetworks.ToArray();
        }
#pragma warning restore CS0108

        protected override void InitMainNet()
        {

            SetBase58Prefixes(Base58Type.PUBKEY_ADDRESS, new byte[] { (23) });
            SetBase58Prefixes(Base58Type.SCRIPT_ADDRESS, new byte[] { (16) });
            SetBase58Prefixes(Base58Type.SECRET_KEY, new byte[] { (193) });
            SetBase58Prefixes(Base58Type.EXT_PUBLIC_KEY, new byte[] { (0x04), (0x88), (0xB2), (0x1E) });
            SetBase58Prefixes(Base58Type.EXT_SECRET_KEY, new byte[] { (0x04), (0x88), (0xAD), (0xE4) });
        }

        protected override void InitTestNet()
        {
            InitMainNet();
            SetBase58Prefixes(Base58Type.PUBKEY_ADDRESS, new byte[] { (83) });
            SetBase58Prefixes(Base58Type.SCRIPT_ADDRESS, new byte[] { (18) });
            SetBase58Prefixes(Base58Type.SECRET_KEY, new byte[] { (193) });
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
