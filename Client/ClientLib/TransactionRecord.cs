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
        public TransactionRecord(ulong aTransactionRecordId, string aTxId, DateTime aTxDate, ulong aBlock, bool aValid)
        {
            TransactionRecordId = aTransactionRecordId;
            TxId = aTxId;
            TxDate = aTxDate;
            Block = aBlock;
            Valid = aValid;
        }

        public static IComparer<TransactionRecord> GetTransactionRecordIdComparer()
        {
            return new IDComparer();
        }

        private class IDComparer : IComparer<TransactionRecord>
        {
            public int Compare(TransactionRecord x, TransactionRecord y)
            {
                return Convert.ToInt32((long)x.TransactionRecordId - (long)y.TransactionRecordId);
            }
        }


        public ulong TransactionRecordId { get; private set; }

        public string TxId { get; private set; }

        public DateTime TxDate { get; private set; }

        public ulong Block { get; private set; }

        public bool Valid { get; private set; }

        public bool IsEqual(TransactionRecord aTransactionRecord)
        {
            return (TransactionRecordId == aTransactionRecord.TransactionRecordId) &&
                (TxId == aTransactionRecord.TxId) &&
                (TxDate == aTransactionRecord.TxDate) &&
                (Block == aTransactionRecord.Block) &&
                (Valid == aTransactionRecord.Valid);
        }
    }
}