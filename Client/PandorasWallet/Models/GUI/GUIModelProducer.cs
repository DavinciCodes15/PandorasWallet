using Pandora.Client.ClientLib;
using Pandora.Client.ClientLib.Contracts;
using Pandora.Client.PandorasWallet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.PandorasWallet.Dialogs.Models
{
    public static class GUIModelProducer
    {
        public static GUITransaction CreateFrom(TransactionRecord aTransactionRecord, ICurrencyAmountFormatter aCurrencyFormatter, IEnumerable<string> aOwnedAddresses)
        {
            return new GUITransaction
            {
                RecordId = aTransactionRecord.TransactionRecordId,
                TxDate = aTransactionRecord.TxDate,
                TxId = aTransactionRecord.TxId,
                BlockNumber = aTransactionRecord.Block,
                Amount = aCurrencyFormatter.AmountToDecimal(aTransactionRecord.GetValue(aOwnedAddresses.ToArray(), out TransactionDirection lTxType, out string lToAddress, out string lFromAddress)),
                Fee = aCurrencyFormatter.AmountToDecimal(aTransactionRecord.TxFee),
                TxType = lTxType,
                From = lFromAddress,
                ToAddress = lToAddress
            };
        }

        public static GUITransaction CreateFrom(IClientTokenTransaction aTokenTx, ICurrencyAmountFormatter aTokenFormatter, TransactionRecord aParentTX, IEnumerable<string> aUserAddresses)
        {
            return new GUITransaction
            {
                RecordId = aTokenTx.GetRecordID(),
                TxDate = aParentTX.TxDate,
                TxId = aParentTX.TxId,
                BlockNumber = aParentTX.Block,
                Amount = aTokenFormatter.AmountToDecimal(aTokenTx.GetValue(aUserAddresses, out TransactionDirection lTxDirection)),
                Fee = aTokenFormatter.AmountToDecimal(aParentTX.TxFee),
                TxType = lTxDirection,
                From = aTokenTx.From,
                ToAddress = aTokenTx.To
            };
        }

        public static GUICurrency CreateFrom(ICurrencyItem aCurrency)
        {
            return new GUICurrency(aCurrency ?? throw new ArgumentException(nameof(aCurrency)));
        }

        public static GUIToken CreateFrom(IClientCurrencyToken aToken, IGUICurrency aParentCurrency)
        {
            return new GUIToken(aToken ?? throw new ArgumentException(nameof(aToken)), aParentCurrency ?? throw new ArgumentException(nameof(aParentCurrency)));
        }

        public static GUIAccount CreateFrom(string aAddress, string aName)
        {
            return new GUIAccount()
            {
                Address = aAddress,
                Name = aName
            };
        }
    }
}