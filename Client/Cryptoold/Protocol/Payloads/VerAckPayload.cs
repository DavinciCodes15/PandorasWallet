using Pandora.Client.Crypto.Currencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Protocol.Payloads
{
    [Payload("verack")]
    public class VerAckPayload : Payload, ICoinSerializable
    {
        #region ICoinSerializable Members

        public override void ReadWriteCore(CoinStream stream)
        {
        }

        #endregion ICoinSerializable Members
    }
}