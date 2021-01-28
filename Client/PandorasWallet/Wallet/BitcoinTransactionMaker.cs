using Pandora.Client.ClientLib;
using Pandora.Client.ClientLib.Contracts;
using Pandora.Client.Crypto.Currencies;
using Pandora.Client.PandorasWallet.ServerAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.PandorasWallet.Wallet
{
    internal class BitcoinTransactionMaker : BaseTransactionMaker
    {
        internal BitcoinTransactionMaker(ICurrencyItem aCurrencyItem, KeyManager aKeyManager, ServerConnection aServerConnection) : base(aCurrencyItem, aKeyManager, aServerConnection)
        {
        }

        public override string CreateSignedTransaction(string aToAddress, decimal aAmount, decimal aTxFee)
        {
            var lUnspents = FServerConnection.GetUnspentOutputs(FCurrencyItem.Id);
            string lData = PrepareNewTransaction(aToAddress, aAmount, aTxFee, FCurrencyItem, lUnspents, out CurrencyTransaction lCurrencyTransaction);
            return SignTransactionData(lData, lCurrencyTransaction, FKeyManager.GetCurrencyAdvocacy(FCurrencyItem.Id, FCurrencyItem.ChainParamaters));
        }

        private string PrepareNewTransaction(string aToAddress, decimal aAmount, decimal aTxFee, ICurrencyItem aCurrencyItem, TransactionUnit[] aUnspentOutputs, out CurrencyTransaction aCurrencyTransaction)
        {
            if (!FServerConnection.DirectCheckAddress(aCurrencyItem.Id, aToAddress))
                throw new ClientExceptions.InvalidAddressException("Address provided not valid. Please verify");
            var lTxOutputs = new List<TransactionUnit>();
            lTxOutputs.Add(new TransactionUnit(0, aCurrencyItem.AmountToBigInteger(aAmount), aToAddress));
            BigInteger lTotal = 0;
            foreach (var lOutput in aUnspentOutputs)
                lTotal += lOutput.Amount;
            var lSendTotal = aCurrencyItem.AmountToDecimal(lTotal);
            if (lSendTotal < (aAmount + aTxFee))
                throw new InvalidOperationException($"The amount to send '{aAmount + aTxFee}' is greater than the balance of transactions '{aCurrencyItem.AmountToDecimal(lTotal)}'.");
            else if (lSendTotal > (aAmount + aTxFee))
                lTxOutputs.Add(new TransactionUnit(0, lTotal - aCurrencyItem.AmountToBigInteger(aAmount + aTxFee), FServerConnection.GetCoinAddress(aCurrencyItem.Id)));
            aCurrencyTransaction = new CurrencyTransaction(aUnspentOutputs, lTxOutputs.ToArray(), aCurrencyItem.AmountToLong(aTxFee), aCurrencyItem.Id);
            return FServerConnection.DirectCreateTransaction(aCurrencyTransaction);
        }
    }
}