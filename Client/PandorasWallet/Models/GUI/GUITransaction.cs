using Pandora.Client.ClientLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.PandorasWallet.Dialogs.Models
{
    public class GUITransaction
    {
        public GUITransaction()
        {
        }

        public long RecordId { get; set; }
        public DateTime TxDate { get; set; }
        public string From { get; set; }
        public string ToAddress { get; set; }
        public decimal Amount { get; set; }
        public long BlockNumber { get; set; }
        public string TxId { get; set; }
        public decimal Fee { get; internal set; }
        public TransactionDirection TxType { get; set; }
        public IGUICurrency ParentCurrency { get; set; }

        // fixed off by one bug.
        public long Confirmations { get => ParentCurrency == null ? 0 : BlockNumber == 0 ? 0 : ParentCurrency.BlockHeight + 1 - BlockNumber; }

        public bool Confirmed { get => ParentCurrency == null ? false : Confirmations >= ParentCurrency.MinConfirmations; }

        public GUITransaction CopyFrom(GUITransaction aSource)
        {
            RecordId = aSource.RecordId;
            TxDate = aSource.TxDate;
            From = aSource.From;
            ToAddress = aSource.ToAddress;
            Amount = aSource.Amount;
            BlockNumber = aSource.BlockNumber;
            TxId = aSource.TxId;
            Fee = aSource.Fee;
            TxType = aSource.TxType;
            ParentCurrency = aSource.ParentCurrency;
            return this;
        }
    }
}