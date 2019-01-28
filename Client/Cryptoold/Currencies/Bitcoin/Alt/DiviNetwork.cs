﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandora.Client.Crypto.Currencies.DataEncoders;

namespace Pandora.Client.Crypto.Currencies.Bitcoin.Alt
{
    public class DiviNetwork : BitcoinNetwork
    {
        protected DiviNetwork(int aNet)
            : base(aNet)
        {
        }

        public override string NetworkName
        {
            get { return "Divi"; }
        }

        public new static DiviNetwork GetMainNet()
        {
            return GetNetworks()[MainNet];
        }

        private static List<DiviNetwork> FNetworks;

        public new static DiviNetwork[] GetNetworks()
        {
            if (FNetworks == null)
            {

                FNetworks = new List<DiviNetwork>();

                FNetworks.Add(new DiviNetwork(Network.MainNet));
                FNetworks.Add(new DiviNetwork(Network.TestNet));
            }
            return FNetworks.ToArray();
        }

        protected override void InitMainNet()
        {

            SetBase58Prefixes(Base58Type.PUBKEY_ADDRESS, new byte[] { (30) });
            SetBase58Prefixes(Base58Type.SCRIPT_ADDRESS, new byte[] { (13) });
            SetBase58Prefixes(Base58Type.SECRET_KEY, new byte[] { (212) });
            SetBase58Prefixes(Base58Type.EXT_PUBLIC_KEY, new byte[] { (0x02), (0x2D), (0x25), (0x33) });
            SetBase58Prefixes(Base58Type.EXT_SECRET_KEY, new byte[] { (0x02), (0x21), (0x31), (0x2B) });
        }

        protected override void InitTestNet()
        {
            InitMainNet();
            SetBase58Prefixes(Base58Type.PUBKEY_ADDRESS, new byte[] { (139) });
            SetBase58Prefixes(Base58Type.SCRIPT_ADDRESS, new byte[] { (19) });
            SetBase58Prefixes(Base58Type.SECRET_KEY, new byte[] { (239) });
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