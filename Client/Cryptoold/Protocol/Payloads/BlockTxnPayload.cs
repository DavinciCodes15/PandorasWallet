using Pandora.Client.Crypto.Currencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Protocol.Payloads
{
    [Payload("blocktxn")]
    public class BlockTxnPayload : Payload
    {
        private uint256 _BlockId;

        public uint256 BlockId
        {
            get
            {
                return _BlockId;
            }
            set
            {
                _BlockId = value;
            }
        }

        private List<Transaction> _Transactions = new List<Transaction>();

        public List<Transaction> Transactions
        {
            get
            {
                return _Transactions;
            }
        }

        public override void ReadWriteCore(CoinStream stream)
        {
            stream.ReadWrite(ref _BlockId);
            stream.ReadWrite(ref _Transactions);
        }
    }
}