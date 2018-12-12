using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.ClientLib
{
    public class TransactionUnit : Client.Crypto.Currencies.Controls.ITransactionUnit
    {
        public TransactionUnit()
        {
        }

        public TransactionUnit(ulong aId, ulong aAmount, string aAddress)
        {
            Id = aId;
            Amount = aAmount;
            Address = aAddress;
        }

        public ulong Id { get; private set; }

        public ulong Amount { get; private set; }

        public string Address { get; private set; }
    }
}