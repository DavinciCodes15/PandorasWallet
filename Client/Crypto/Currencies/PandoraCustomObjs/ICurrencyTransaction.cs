using System.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Currencies
{
    public interface ICurrencyTransaction
    {
        /// <summary>
        /// What was spent from what accounts and how much.
        /// </summary>
        ITransactionUnit[] Inputs { get; }

        /// <summary>
        /// Where it was spent to, what accounts and how much.
        /// </summary>
        ITransactionUnit[] Outputs { get; }

        /// <summary>
        /// Adds accounts to take the currency from and includes them into inputs.
        /// </summary>
        /// <param name="aAmount">This value is irrelavent as all coins are taken from the address.</param>
        /// <param name="aAddress">Address that the amount is comming from.</param>
        void AddInput(BigInteger aAmount, string aAddress, long aId = 0);

        /// <summary>
        /// Where to send the coins to
        /// </summary>
        /// <param name="aAmount">Amount to send to what address.</param>
        /// <param name="aAddress">The address that will get the coins.</param>
        void AddOutput(BigInteger aAmount, string aAddress, int aIndex = -1, long aId = 0, string aTxID = null);

        /// <summary>
        /// The amount of fee that will used to send the coins.
        /// </summary>
        long TxFee { get; set; }

        long CurrencyId { get; set; }

        ///// <summary>
        ///// Some cases a currency is built on top of another currency
        ///// such as XCP or REP the under pinging currency that will be send
        ///// and its fee.
        ///// </summary>
        //ICurrencySendTransaction RootCurrency { get; set; }
    }
}