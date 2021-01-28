using System.Numerics;

namespace Pandora.Client.Crypto.Currencies
{
    public interface ITransactionUnit
    {
        /// <summary>
        ///  Total amount used in a a Tx Unit
        /// </summary>
        BigInteger Amount { get; }

        /// <summary>
        /// Address assosiated with the Tx Unit.
        /// </summary>
        string Address { get; }

        /// <summary>
        /// Parent transaction id
        /// </summary>
        string TxID { get; }

        /// <summary>
        /// Index of output associated to this txunit
        /// </summary>
        int Index { get; }

        /// <summary>
        /// Script of output
        /// </summary>
        string Script { get; }
    }
}