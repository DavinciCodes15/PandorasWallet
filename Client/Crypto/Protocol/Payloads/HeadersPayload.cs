using Pandora.Client.Crypto.Currencies;
using System.Collections.Generic;
using System.Linq;

namespace Pandora.Client.Crypto.Protocol.Payloads
{
    /// <summary>
    /// Block headers received after a getheaders messages
    /// </summary>
    [Payload("headers")]
    public class HeadersPayload : Payload
    {
        private class BlockHeaderWithTxCount : ICoinSerializable
        {
            public BlockHeaderWithTxCount()
            {
            }

            public BlockHeaderWithTxCount(BlockHeader header)
            {
                _Header = header;
            }

            internal BlockHeader _Header;

            #region ICoinSerializable Members

            public void ReadWrite(CoinStream stream)
            {
                stream.ReadWrite(ref _Header);

                VarInt txCount = new VarInt(0);
                stream.ReadWrite(ref txCount);
            }

            #endregion ICoinSerializable Members
        }

        protected List<BlockHeader> headers = new List<BlockHeader>();

        public HeadersPayload()
        {
        }

        public HeadersPayload(params BlockHeader[] headers)
        {
            Headers.AddRange(headers);
        }

        public List<BlockHeader> Headers => headers;

        public override void ReadWriteCore(CoinStream stream)
        {
            if (stream.Serializing)
            {
                List<BlockHeaderWithTxCount> heardersOff = headers.Select(h => new BlockHeaderWithTxCount(h)).ToList();
                stream.ReadWrite(ref heardersOff);
            }
            else
            {
                headers.Clear();
                List<BlockHeaderWithTxCount> headersOff = new List<BlockHeaderWithTxCount>();
                stream.ReadWrite(ref headersOff);
                headers.AddRange(headersOff.Select(h => h._Header));
            }
        }
    }
}