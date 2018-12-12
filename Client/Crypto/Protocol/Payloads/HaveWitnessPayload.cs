using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Protocol.Payloads
{
    [Payload("havewitness")]
    public class HaveWitnessPayload : Payload
    {
        public HaveWitnessPayload()
        {
        }
    }
}