using Pandora.Client.ClientLib;
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
    internal abstract class BaseTransactionMaker : ITransactionMaker
    {
        protected ICurrencyItem FCurrencyItem;
        protected ServerConnection FServerConnection;
        protected KeyManager FKeyManager;

        internal BaseTransactionMaker(KeyManager aKeyManager, ServerConnection aServerConnection)
        {
            FServerConnection = aServerConnection;
            FKeyManager = aKeyManager;
        }

        internal BaseTransactionMaker(ICurrencyItem aCurrencyItem, KeyManager aKeyManager, ServerConnection aServerConnection) : this(aKeyManager, aServerConnection)
        {
            FCurrencyItem = aCurrencyItem;
        }

        public abstract string CreateSignedTransaction(string aToAddress, decimal aAmount, decimal aTxFee, params object[] aExtParams);

        public virtual void SendRawTransaction(string aRawTX, DelegateOnSendTransactionCompleted aTxSentEventDelegate)
        {
            FServerConnection.DirectSendNewTransaction(aRawTX, FCurrencyItem.Id, aTxSentEventDelegate);
        }

        protected virtual string SignTransactionData(string aTxData, CurrencyTransaction aCurrencyTransaction, ICryptoCurrencyAdvocacy aCurrencyAdvocacy)
        {
            //TODO: this is done because the object needs the address create.
            //So we should fix this somehow so its a bit more logical.
            aCurrencyAdvocacy.GetAddress(0);
            aCurrencyAdvocacy.GetAddress(1);
            return aCurrencyAdvocacy.SignTransaction(aTxData, aCurrencyTransaction);
        }
    }
}