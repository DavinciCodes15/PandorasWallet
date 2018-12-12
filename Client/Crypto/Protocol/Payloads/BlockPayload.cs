using Pandora.Client.Crypto.Currencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Protocol.Payloads
{
    /// <summary>
    /// A block received after being asked with a getdata message
    /// </summary>
    [Payload("block")]
    public class BlockPayload : BitcoinSerializablePayload<Block>
    {
        public BlockPayload()
        {
        }

        public BlockPayload(Block block)
            : base(block)
        {
        }
    }
}