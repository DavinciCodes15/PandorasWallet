using Pandora.Client.Crypto.Currencies;
using Pandora.Client.Crypto.Protocol.Payloads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Protocol
{
    public class Payload : ICoinSerializable
    {
        public virtual string Command
        {
            get
            {
                return PayloadAttribute.GetCommandName(this.GetType());
            }
        }

        #region ICoinSerializable Members

        public void ReadWrite(CoinStream stream)
        {
            using (stream.SerializationTypeScope(SerializationType.Network))
            {
                ReadWriteCore(stream);
            }
        }

        public virtual void ReadWriteCore(CoinStream stream)
        {
        }

        #endregion ICoinSerializable Members

        public override string ToString()
        {
            return this.GetType().Name;
        }
    }
}