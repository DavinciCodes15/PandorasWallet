using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.ClientLib
{

    public class TransactionRecord : CurrencyTransaction
    {
        public TransactionRecord()
        {

        }
        public TransactionRecord(ulong aTransactionRecordId, string aTxId, DateTime aTxDate, ulong aBlock)
        {
            this.TransactionRecordId = aTransactionRecordId;
            this.TxId = aTxId;
            this.TxDate = aTxDate;
            this.Block = aBlock;
        }
        public ulong TransactionRecordId { get; private set; }

        public string TxId { get; private set; }

        public DateTime TxDate { get; private set; }

        public ulong Block { get; private set; }
    }
}