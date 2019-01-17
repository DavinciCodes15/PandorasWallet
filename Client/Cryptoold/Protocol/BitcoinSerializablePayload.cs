using Pandora.Client.Crypto.Currencies;
using Pandora.Client.Crypto.Protocol.Payloads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Protocol
{
    public class BitcoinSerializablePayload<T> : Payload where T : ICoinSerializable, new()
    {
        public BitcoinSerializablePayload()
        {
        }

        public BitcoinSerializablePayload(T obj)
        {
            _Object = obj;
        }

        private T _Object = new T();

        public T Object
        {
            get
            {
                return _Object;
            }
            set
            {
                _Object = value;
            }
        }

        public override void ReadWriteCore(CoinStream stream)
        {
            if (!stream.Serializing)
                _Object = default(T);
            stream.ReadWrite(ref _Object);
        }
    }
}