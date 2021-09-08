using Pandora.Client.ClientLib;
using Pandora.Client.ClientLib.Contracts;
using Pandora.Client.Crypto.Currencies;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Pandora.Client.PandorasWallet.Dialogs.Models
{
    public interface IGUICurrency : ICurrencyItem
    {
        IStatusDetails StatusDetails { get; }
        IGUICurrencyTransactional Transactions { get; }
        IGUICurrencyBalance Balances { get; }
        IGUICurrencyMarketPrices MarketPrices { get; }
        long BlockHeight { get; set; }
        IEnumerable<GUIAccount> Addresses { get; set; }
        string LastAddress { get; }
    }

    internal class GUICurrencyMarketPriceHelper : IGUICurrencyMarketPrices
    {
        //Note: This event is not thinked to be used on a multithread enviroment
        public event Action<IGUICurrency> OnPricesChanged;

        private IGUICurrency FGUICurrency;

        public GUICurrencyMarketPriceHelper(IGUICurrency aGUICurrency)
        {
            FGUICurrency = aGUICurrency;
            SymbolCurrency = null;
            SymbolFiat = null;
        }

        public decimal DefaultCoinPrice { get; private set; }

        public decimal FiatPrice { get; private set; }

        public decimal DefaultCoinValue => FGUICurrency.Balances.Total * DefaultCoinPrice;
        public decimal FiatValue => FGUICurrency.Balances.Total * FiatPrice;
        public string SymbolCurrency { get; private set; }
        public string SymbolFiat { get; private set; }

        public void UpdatePrices(decimal aDefaultCoinPrice, decimal aFiatPrice, string aDefaultCurrencyTicker, string aFiatSymbol)
        {
            DefaultCoinPrice = aDefaultCoinPrice;
            FiatPrice = aFiatPrice;
            SymbolCurrency = aDefaultCurrencyTicker ?? throw new ArgumentNullException(nameof(aDefaultCurrencyTicker));
            SymbolFiat = aFiatSymbol ?? throw new ArgumentNullException(nameof(aFiatSymbol));
            OnPricesChanged?.Invoke(FGUICurrency);
        }
    }

    internal class GUICurrencyTxAndBalanceHelper : IGUICurrencyTransactional, IGUICurrencyBalance
    {
        private ConcurrentDictionary<long, GUITransaction> FTransactions = new ConcurrentDictionary<long, GUITransaction>();
        private IGUICurrency FGUIParentCurrency;
        public decimal UnconfirmedBalance { get; set; }
        public decimal ConfirmedBalance { get; set; }
        public decimal Total { get { return UnconfirmedBalance + ConfirmedBalance; } }
        public IEnumerable<GUITransaction> TransactionItems => FTransactions.Values;

        public GUICurrencyTxAndBalanceHelper(IGUICurrency aParentGUICurrency)
        {
            FGUIParentCurrency = aParentGUICurrency;
        }

        public void AddTransaction(GUITransaction aTransaction)
        {
            if (aTransaction.TxType == TransactionDirection.Unknown) return;
            FTransactions.TryAdd(aTransaction.RecordId, aTransaction);
            aTransaction.ParentCurrency = FGUIParentCurrency;
            if (!aTransaction.Confirmed)
                UnconfirmedBalance += aTransaction.Amount;
            else
                ConfirmedBalance += aTransaction.Amount;
        }

        public void UpdateTransaction(GUITransaction aTransaction)
        {
            aTransaction.ParentCurrency = FGUIParentCurrency;
            if (FTransactions.ContainsKey(aTransaction.RecordId))
                FTransactions[aTransaction.RecordId].CopyFrom(aTransaction);
        }

        public bool RemoveTransaction(GUITransaction aTransaction)
        {
            var lResult = FTransactions.TryRemove(aTransaction.RecordId, out _);
            if (lResult)
            {
                if (!aTransaction.Confirmed)
                    UnconfirmedBalance -= aTransaction.Amount;
                else
                    ConfirmedBalance -= aTransaction.Amount;
                return lResult;
            }
            return lResult;
        }

        public void ClearTransactions()
        {
            FTransactions.Clear();
            UpdateBalance();
        }

        public void UpdateBalance()
        {
            UnconfirmedBalance = 0;
            ConfirmedBalance = 0;
            foreach (var lTx in TransactionItems)
            {
                if (!lTx.Confirmed)
                    UnconfirmedBalance += lTx.Amount;
                else
                    ConfirmedBalance += lTx.Amount;
            }
        }

        public GUITransaction FindTransaction(long aRecordId)
        {
            FTransactions.TryGetValue(aRecordId, out GUITransaction lResult);
            return lResult;
        }

        public bool RemoveTransaction(long aRecordId)
        {
            return FTransactions.TryRemove(aRecordId, out _);
        }
    }

    public interface IGUICurrencyBalance
    {
        decimal UnconfirmedBalance { get; }
        decimal ConfirmedBalance { get; }
        decimal Total { get; }

        void UpdateBalance();
    }

    public interface IGUICurrencyMarketPrices
    {
        event Action<IGUICurrency> OnPricesChanged;

        void UpdatePrices(decimal aDefaultCoinPrice, decimal aFiatPrice, string aDefaultCurrencyTicker, string aFiatSymbol);

        decimal DefaultCoinPrice { get; }
        decimal FiatPrice { get; }
        decimal DefaultCoinValue { get; }
        decimal FiatValue { get; }
        string SymbolCurrency { get; }
        string SymbolFiat { get; }
    }

    public interface IGUICurrencyTransactional
    {
        IEnumerable<GUITransaction> TransactionItems { get; }

        void AddTransaction(GUITransaction aTransaction);

        void UpdateTransaction(GUITransaction aTransaction);

        bool RemoveTransaction(GUITransaction aTransaction);

        void ClearTransactions();

        GUITransaction FindTransaction(long aRecordId);

        bool RemoveTransaction(long aRecordId);
    }

    public class GUICurrency : CurrencyItem, IGUICurrency
    {
        private GUICurrencyTxAndBalanceHelper FGUICurrencyHelper;
        private GUICurrencyMarketPriceHelper FGUICurrencyPricesHelper;

        public IStatusDetails StatusDetails { get; private set; }
        public decimal DefaultCurrencyPricePerCoin { get; set; }
        public long BlockHeight { get; set; }
        public IEnumerable<GUIAccount> Addresses { get; set; }
        public string LastAddress { get { return Addresses.Last().Address; } }

        public IGUICurrencyTransactional Transactions => FGUICurrencyHelper;

        public IGUICurrencyBalance Balances => FGUICurrencyHelper;

        public IGUICurrencyMarketPrices MarketPrices => FGUICurrencyPricesHelper;

        public GUICurrency()
        {
            StatusDetails = new StatusDetailsObject();
            FGUICurrencyHelper = new GUICurrencyTxAndBalanceHelper(this);
            FGUICurrencyPricesHelper = new GUICurrencyMarketPriceHelper(this);
        }

        public GUICurrency(ICurrencyItem aCurrency) : this()
        {
            this.CopyFrom(aCurrency);
        }

        private class StatusDetailsObject : IStatusDetails
        {
            public string StatusMessage { get; set; }
            public DateTime StatusTime { get; set; }
        }
    }

    public interface IStatusDetails
    {
        string StatusMessage { get; set; }
        DateTime StatusTime { get; set; }
    }
}