#if !NOSOCKET

using Pandora.Client.Crypto.Currencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Protocol.Payloads
{
    /// <summary>
    /// An available peer address in the bitcoin network is announce (unsollicited or after a getaddr)
    /// </summary>
    [Payload("addr")]
    public class AddrPayload : Payload, ICoinSerializable
    {
        private NetworkAddress[] addr_list = new NetworkAddress[0];

        public NetworkAddress[] Addresses
        {
            get
            {
                return addr_list;
            }
        }

        public AddrPayload()
        {
        }

        public AddrPayload(NetworkAddress address)
        {
            addr_list = new NetworkAddress[] { address };
        }

        public AddrPayload(NetworkAddress[] addresses)
        {
            addr_list = addresses.ToArray();
        }

        #region ICoinSerializable Members

        public override void ReadWriteCore(CoinStream stream)
        {
            stream.ReadWrite(ref addr_list);
        }

        #endregion ICoinSerializable Members

        public override string ToString()
        {
            return Addresses.Length + " address(es)";
        }
    }
}

#endif