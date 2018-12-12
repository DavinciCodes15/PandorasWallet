using Pandora.Client.Crypto.Currencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Protocol.Payloads
{
    /// <summary>
    /// Ask for the block hashes (inv) that happened since BlockLocators
    /// </summary>
    [Payload("getblocks")]
    public class GetBlocksPayload : Payload
    {
        public GetBlocksPayload(BlockLocator locator)
        {
            BlockLocators = locator;
        }

        public GetBlocksPayload()
        {
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

        private uint256 _HashStop = uint256.Zero;

        public uint256 HashStop
        {
            get
            {
                return _HashStop;
            }
            set
            {
                _HashStop = value;
            }
        }

        public override void ReadWriteCore(CoinStream stream)
        {
            stream.ReadWrite(ref version);
            stream.ReadWrite(ref blockLocators);
            stream.ReadWrite(ref _HashStop);
        }
    }
}