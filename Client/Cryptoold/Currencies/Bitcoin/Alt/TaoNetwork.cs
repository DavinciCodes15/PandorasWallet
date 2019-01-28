﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandora.Client.Crypto.Currencies.DataEncoders;

namespace Pandora.Client.Crypto.Currencies.Bitcoin.Alt
{
    public class TaoNetwork : BitcoinNetwork
    {
        protected TaoNetwork(int aNet)
            : base(aNet)
        {
        }

        public override string NetworkName
        {
            get { return "Tao"; }
        }

        public new static TaoNetwork GetMainNet()
        {
            return GetNetworks()[MainNet];
        }

        private static List<TaoNetwork> FNetworks;

        public new static TaoNetwork[] GetNetworks()
        {
            if (FNetworks == null)
            {

                FNetworks = new List<TaoNetwork>();

                FNetworks.Add(new TaoNetwork(Network.MainNet));
                FNetworks.Add(new TaoNetwork(Network.TestNet));
            }
            return FNetworks.ToArray();
        }

        protected override void InitMainNet()
        {

            SetBase58Prefixes(Base58Type.PUBKEY_ADDRESS, new byte[] { (66) });
            SetBase58Prefixes(Base58Type.SCRIPT_ADDRESS, new byte[] { (3) });
            SetBase58Prefixes(Base58Type.SECRET_KEY, new byte[] { (76) });
            SetBase58Prefixes(Base58Type.EXT_PUBLIC_KEY, new byte[] { (0x04), (0x88), (0xB2), (0x1E) });
            SetBase58Prefixes(Base58Type.EXT_SECRET_KEY, new byte[] { (0x04), (0x88), (0xAD), (0xE4) });
        }

        protected override void InitTestNet()
        {
            InitMainNet();
            SetBase58Prefixes(Base58Type.PUBKEY_ADDRESS, new byte[] { (127) });
            SetBase58Prefixes(Base58Type.SCRIPT_ADDRESS, new byte[] { (130) });
            SetBase58Prefixes(Base58Type.SECRET_KEY, new byte[] { (138) });
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