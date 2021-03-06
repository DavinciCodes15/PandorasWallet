﻿using Pandora.Client.ClientLib;
using Pandora.Client.ClientLib.Contracts;
using Pandora.Client.Crypto.Currencies;
using Pandora.Client.PandorasWallet.ServerAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.PandorasWallet.Wallet.TransactionMaker
{
    internal class EthereumTransactioMaker : BaseTransactionMaker
    {
        internal EthereumTransactioMaker(ICurrencyItem aCurrencyItem, KeyManager aKeyManager, ServerConnection aServerConnection) : base(aCurrencyItem, aKeyManager, aServerConnection)
        {
        }

        public override string CreateSignedTransaction(string aToAddress, decimal aAmount, decimal aTxFee, params object[] aExtParams)
        {
            string lNonce = null;
            if (aExtParams != null && aExtParams.Any())
            {
                var lCustomTxFee = aExtParams.FirstOrDefault() as decimal?;
                if (lCustomTxFee.HasValue && lCustomTxFee.Value != aTxFee && lCustomTxFee.Value > 0)
                    aTxFee = lCustomTxFee.Value;
                if (aExtParams.Length >= 2)
                    lNonce = aExtParams[1] as string;
            }
            var lEthAdvocacy = FKeyManager.GetCurrencyAdvocacy(FCurrencyItem.Id, FCurrencyItem.ChainParamaters);
            var lAddress = lEthAdvocacy.GetAddress(1);
            var lTransaction = PrepareNewTransaction(lAddress, aToAddress, aAmount, aTxFee);
            if (string.IsNullOrEmpty(lNonce))
                lNonce = FServerConnection.DirectCreateTransaction(lTransaction);
            return SignTransactionData(lNonce, lTransaction, lEthAdvocacy);
        }

        private CurrencyTransaction PrepareNewTransaction(string aFromAddress, string aToAddress, decimal aAmount, decimal aTxFee)
        {
            var lInput = new TransactionUnit[1] { new TransactionUnit(0, 0, aFromAddress) };
            var lOutput = new TransactionUnit[1] { new TransactionUnit(0, FCurrencyItem.AmountToBigInteger(aAmount), aToAddress) };
            return new CurrencyTransaction(lInput, lOutput, FCurrencyItem.AmountToLong(aTxFee), FCurrencyItem.Id);
        }
    }
}