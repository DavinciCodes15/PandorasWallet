using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Currencies.Controls
{
    public interface ITransactionUnit
    {
        /// <summary>
        ///  Total amount used in a a Tx Unit
        /// </summary>
        ulong Amount { get; }

        /// <summary>
        /// Address assosiated with the Tx Unit.
        /// </summary>
        string Address { get; }
    }
}
