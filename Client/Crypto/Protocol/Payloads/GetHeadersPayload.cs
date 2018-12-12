using Pandora.Client.Crypto.Currencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Protocol.Payloads
{
    /// <summary>
    /// Ask block headers that happened since BlockLocators
    /// </summary>
    [Payload("getheaders")]
    public class GetHeadersPayload : Payload
    {
        public GetHeadersPayload()
        {
        }

        public GetHeadersPayload(BlockLocator locator)
        {
            BlockLocators = locator;
        }

        private uint version;

        public uint Version
        {
            get
            {
                return version;
            }
            set
            {
                version = value;
            }
        }

        private BlockLocator blockLocators;

        public BlockLocator BlockLocators
        {
            get
            {
                return blockLocators;
            }
            set
            {
                blockLocators = value;
            }
        }

        private uint256 hashStop = uint256.Zero;

        public uint256 HashStop
        {
            get
            {
                return hashStop;
            }
            set
            {
                hashStop = value;
            }
        }

        public override void ReadWriteCore(CoinStream stream)
        {
            stream.ReadWrite(ref version);
            stream.ReadWrite(ref blockLocators);
            stream.ReadWrite(ref hashStop);
        }
    }
}