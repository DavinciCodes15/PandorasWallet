using Pandora.Client.Crypto.Currencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Protocol.Payloads
{
    [Payload("sendcmpct")]
    public class SendCmpctPayload : Payload
    {
        public SendCmpctPayload()
        {
        }

        public SendCmpctPayload(bool preferHeaderAndIDs)
        {
            PreferHeaderAndIDs = preferHeaderAndIDs;
        }

        private byte _PreferHeaderAndIDs;

        public bool PreferHeaderAndIDs
        {
            get
            {
                return _PreferHeaderAndIDs == 1;
            }
            set
            {
                _PreferHeaderAndIDs = value ? (byte)1 : (byte)0;
            }
        }

        private ulong _Version = 1;

        public ulong Version
        {
            get
            {
                return _Version;
            }
            set
            {
                _Version = value;
            }
        }

        public override void ReadWriteCore(CoinStream stream)
        {
            stream.ReadWrite(ref _PreferHeaderAndIDs);
            stream.ReadWrite(ref _Version);
        }
    }
}