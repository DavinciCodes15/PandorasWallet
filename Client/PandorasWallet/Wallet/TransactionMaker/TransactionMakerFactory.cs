using Pandora.Client.ClientLib.Contracts;
using Pandora.Client.Crypto.Currencies;
using Pandora.Client.PandorasWallet.ServerAccess;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.PandorasWallet.Wallet.TransactionMaker
{
    public interface ITransactionMaker
    {
        string CreateSignedTransaction(string aToAddress, decimal aAmount, decimal aTxFee, params object[] aExtParams);

        void SendRawTransaction(string aRawTX, DelegateOnSendTransactionCompleted aTxSentEventDelegate);
    }

    public class TransactionMakerFactory
    {
        private enum MakerType
        {
            BitcoinCurrency, EthereumCurrency, Token
        }

        private ConcurrentDictionary<long, ITransactionMaker> FMakers;
        private ServerConnection FServerConnection;
        private KeyManager FKeyManager;

        public TransactionMakerFactory(ServerConnection aServerConnection)
        {
            FServerConnection = aServerConnection ?? throw new ArgumentNullException(nameof(aServerConnection));
            FMakers = new ConcurrentDictionary<long, ITransactionMaker>();
        }

        internal ITransactionMaker GetMaker(ICurrencyItem aCurrencyItem, KeyManager aKeyManager)
        {
            if (!FMakers.TryGetValue(aCurrencyItem.Id, out ITransactionMaker lResult))
            {
                if (aCurrencyItem.ChainParamaters.Capabilities.HasFlag(CapablityFlags.EthereumProtocol))
                {
                    if (aCurrencyItem.Id < 0)
                        lResult = new TokenTransactionMaker(aCurrencyItem, aKeyManager, FServerConnection);
                    else
                        lResult = new EthereumTransactioMaker(aCurrencyItem, aKeyManager, FServerConnection);
                }
                else
                    lResult = new BitcoinTransactionMaker(aCurrencyItem, aKeyManager, FServerConnection);
                FMakers.TryAdd(aCurrencyItem.Id, lResult);
            }
            return lResult;
        }
    }
}