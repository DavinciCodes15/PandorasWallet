using Pandora.Client.Crypto.Currencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Protocol.Payloads
{
    /// <summary>
    /// Load a bloomfilter in the peer, used by SPV clients
    /// </summary>
    [Payload("filterload")]
    public class FilterLoadPayload : BitcoinSerializablePayload<BloomFilter>
    {
        public FilterLoadPayload()
        {
        }

        public FilterLoadPayload(BloomFilter filter)
            : base(filter)
        {
        }
    }
}