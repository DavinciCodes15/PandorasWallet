using Pandora.Client.Crypto.Currencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Protocol.Payloads
{
    [Payload("filteradd")]
    public class FilterAddPayload : Payload
    {
        public FilterAddPayload()
        {
        }

        public FilterAddPayload(byte[] data)
        {
            _Data = data;
        }

        private byte[] _Data;

        public byte[] Data
        {
            get
            {
                return _Data;
            }
            set
            {
                _Data = value;
            }
        }

        public override void ReadWriteCore(CoinStream stream)
        {
            stream.ReadWriteAsVarString(ref _Data);
        }
    }
}