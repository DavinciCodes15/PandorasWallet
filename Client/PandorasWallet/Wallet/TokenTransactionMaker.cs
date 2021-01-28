using Pandora.Client.ClientLib;
using Pandora.Client.ClientLib.Contracts;
using Pandora.Client.Crypto.Currencies;
using Pandora.Client.Crypto.Currencies.Ethereum;
using Pandora.Client.PandorasWallet.ServerAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.PandorasWallet.Wallet
{
    internal class TokenTransactionMaker : BaseTransactionMaker
    {
        private ICurrencyToken FToken;

        public TokenTransactionMaker(ICurrencyItem aCurrencyItem, KeyManager aKeyManager, ServerConnection aServerConnection) : base(aKeyManager, aServerConnection)
        {
            FToken = aCurrencyItem as ICurrencyToken ?? throw new ArgumentException(nameof(aCurrencyItem), $"Currency item is not a token. Argument must implement {nameof(ICurrencyToken)}");
            FCurrencyItem = aServerConnection.GetCurrency(FToken.ParentCurrencyID);
        }

        public override string CreateSignedTransaction(string aToAddress, decimal aAmount, decimal aTxFee)
        {
            var lParentCurrency = FServerConnection.GetCurrency(FToken.ParentCurrencyID);
            var lParentCurrencyAdvocacy = FKeyManager.GetCurrencyAdvocacy(lParentCurrency.Id, lParentCurrency.ChainParamaters);
            var lFromAddress = lParentCurrencyAdvocacy.GetAddress(1);
            var lTransaction = PrepareNewTransaction(lFromAddress, aToAddress, aAmount, aTxFee);
            var lNonce = FServerConnection.DirectCreateTransaction(lTransaction);
            return SignTransactionData(lNonce, lTransaction, lParentCurrencyAdvocacy);
        }

        private CurrencyTransaction PrepareNewTransaction(string aFromAddress, string aToAddress, decimal aAmount, decimal aTxFee)
        {
            var lInput = new TransactionUnit[1] { new TransactionUnit(0, 0, aFromAddress) };
            var lTokenTxData = ERC20TokenDecoder.Encode(ERC20Methods.Transfer, aToAddress, (FToken as ICurrencyAmountFormatter).AmountToBigInteger(aAmount).ToString());
            var lOutput = new TransactionUnit[1] { new TransactionUnit(0, 0, FToken.ContractAddress, aScript: lTokenTxData) };
            return new CurrencyTransaction(lInput, lOutput, FCurrencyItem.AmountToLong(aTxFee), FToken.ParentCurrencyID);
        }

        public override void SendRawTransaction(string aRawTX, DelegateOnSendTransactionCompleted aTxSentEventDelegate)
        {
            string lRawTxToSend = aRawTX.Contains("0x") ? aRawTX : string.Concat("0x", aRawTX);
            FServerConnection.DirectSendNewTransaction(aRawTX, FToken.ParentCurrencyID, aTxSentEventDelegate);
        }
    }
}