using Pandora.Client.ClientLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.PandorasWallet.Models
{
    public interface IClientTokenTransaction : ITokenTransaction
    {
        long GetRecordID();

        BigInteger GetValue(IEnumerable<string> aAddreses, out TransactionDirection aTxDirection);
    }

    public interface ITokenTransaction
    {
        string ParentTransactionID { get; }
        string From { get; }
        string To { get; }
        BigInteger Amount { get; }
        string TokenAddress { get; }
    }

    public class ClientTokenTransactionItem : IClientTokenTransaction
    {
        public ClientTokenTransactionItem()
        {
        }

        public ClientTokenTransactionItem(ITokenTransaction aClientTokenTransaction)
        {
            From = aClientTokenTransaction.From;
            To = aClientTokenTransaction.To;
            Amount = aClientTokenTransaction.Amount;
            TokenAddress = aClientTokenTransaction.TokenAddress;
            ParentTransactionID = aClientTokenTransaction.ParentTransactionID;
        }

        public string From { get; set; }
        public string To { get; set; }
        public BigInteger Amount { get; set; }
        public string TokenAddress { get; set; }
        public string ParentTransactionID { get; set; }

        private long FRecordID;

        public long GetRecordID()
        {
            if (FRecordID == 0)
            {
                var lUniqueIdentifier = string.Concat(ParentTransactionID, TokenAddress);
                var lHasher = System.Security.Cryptography.MD5.Create();
                var lHashedIdentifier = lHasher.ComputeHash(Encoding.UTF8.GetBytes(lUniqueIdentifier));
                FRecordID = BitConverter.ToInt64(lHashedIdentifier, 0);
            }
            return FRecordID;
        }

        public BigInteger GetValue(IEnumerable<string> aAddreses, out TransactionDirection aTxDirection)
        {
            BigInteger lResult = 0;
            aTxDirection = TransactionDirection.Unknown;
            var lNormalizedAddresses = aAddreses.Select(lAddress => lAddress.ToLowerInvariant());
            if (lNormalizedAddresses.Contains(To.ToLowerInvariant()))
            {
                lResult += Amount;
                aTxDirection = TransactionDirection.Credit;
            }

            if (lNormalizedAddresses.Contains(From.ToLowerInvariant()))
            {
                lResult -= Amount;
                aTxDirection = aTxDirection == TransactionDirection.Credit ? TransactionDirection.Both : TransactionDirection.Debit;
            }

            return lResult;
        }
    }
}