﻿using Pandora.Client.Crypto.Currencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Protocol.Payloads
{
    /// <summary>
    /// A merkle block received after being asked with a getdata message
    /// </summary>
    [Payload("merkleblock")]
    public class MerkleBlockPayload : BitcoinSerializablePayload<MerkleBlock>
    {
        public MerkleBlockPayload()
        {
        }

        public MerkleBlockPayload(MerkleBlock block)
            : base(block)
        {
        }
    }
}