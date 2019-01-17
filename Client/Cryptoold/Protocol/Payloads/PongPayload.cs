using Pandora.Client.Crypto.Currencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Protocol.Payloads
{
    [Payload("pong")]
    public class PongPayload : Payload
    {
        private ulong _Nonce;

        public ulong Nonce
        {
            get
            {
                return _Nonce;
            }
            set
            {
                _Nonce = value;
            }
        }

        public override void ReadWriteCore(CoinStream stream)
        {
            stream.ReadWrite(ref _Nonce);
        }

        public override string ToString()
        {
            return base.ToString() + " : " + Nonce;
        }
    }
}