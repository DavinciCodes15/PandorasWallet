using Pandora.Client.Crypto;
using Pandora.Client.ClientLib;
using System;

namespace Pandora.Client.Crypto.Test
{
    internal class FakePandoraServer : IPandoraServer
    {
        public string Username { get; private set; }

        public string Email => throw new NotImplementedException();

        public bool Connected => throw new NotImplementedException();

        //public ICurrencyAccountList MonitoredAccounts => throw new NotImplementedException();

        public string DataPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event TransactionEvent OnTransactions;

        public string CreateTransaction(uint aCurrencyId, CurrencyTransaction aSendTx)
        {
            return FakeTransaction;
        }

        public string FakeTransaction { get; set; }

        public CurrencyItem GetCurrency(uint aCurrencyId)
        {
            throw new NotImplementedException();
        }

        public byte[] GetCurrencyIcon(uint aCurrencyId)
        {
            throw new NotImplementedException();
        }

        public byte[] GetCurrencyLib(uint aCurrencyId, string aLibVersion)
        {
            throw new NotImplementedException();
        }

        public CurrencyItem[] GetCurrencyList()
        {
            throw new NotImplementedException();
        }

        public CurrencyStatusItem GetCurrencyStatus(uint aCurrencyId)
        {
            throw new NotImplementedException();
        }

        public string GetExtendedTransactionInfo(uint aCurrencyId, string aTxId)
        {
            throw new NotImplementedException();
        }

        public ulong GetTransactionFee(uint aCurrencyId, CurrencyTransaction aCurrencyTransaction)
        {
            return FakeTransactionFee;
        }

        public ulong FakeTransactionFee { get; set; }

        public TransactionRecord[] GetTransactions(uint aCurrencyId, string aAddress, bool aMonitored = true)
        {
            throw new NotImplementedException();
        }

        public bool Logoff()
        {
            throw new NotImplementedException();
        }

        public bool Logon(string aEmail, string aUserName, string aPassword)
        {
            throw new NotImplementedException();
        }

        public long SendTransaction(ulong aCurrencyID, string aSignedTxData)
        {
            return 0;
        }

        public UserStatus GetUserStatus()
        {
            throw new NotImplementedException();
        }

        public TransactionRecord[] GetTransactions(uint aCurrencyId, string aAddress)
        {
            throw new NotImplementedException();
        }

        public string CreateTransaction(CurrencyTransaction aSendTx)
        {
            throw new NotImplementedException();
        }

        public ulong GetTransactionFee(uint aCurrencyId, string aTxWithNoFee)
        {
            throw new NotImplementedException();
        }
    }
}