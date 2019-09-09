using Pandora.Client.ClientLib;
using Pandora.Client.Crypto.Currencies;
using Pandora.Client.Universal;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.PandorasWallet.ServerAccess
{
    public class CacheServerAccessHelper : IDisposable
    {
        private ServerConnection FCurrentServerConnection;

        public delegate IEnumerable<CurrencyAccount> OnCurrencyAddressesNeededDelegate(IEnumerable<CurrencyAccount> aServerMonitoredAccounts, long aCurrencyID, IChainParams aCurrencyChainParams);

        public CacheServerAccessHelper(ServerConnection aServerConnection)
        {
            FCurrentServerConnection = aServerConnection;
        }

        public void ReconstructCurrencyData(CurrencyItem aCurrencyItem, OnCurrencyAddressesNeededDelegate aGetAddressFunction)
        {
            using (var lSpareDB = CreateNewSpareDBStore())
            {
                FillDataFromServer(lSpareDB, aCurrencyItem, aGetAddressFunction);
                string lCurrentDBFile = Path.Combine(FCurrentServerConnection.DataPath, $"{FCurrentServerConnection.InstanceId}.sqlite");
                using (var lActiveDB = new LocalCacheDBExtension(lCurrentDBFile))
                    MoveCurrencyDataToActiveDB(aCurrencyItem.Id, lSpareDB, lActiveDB);
            }
        }

        private void MoveCurrencyDataToActiveDB(long aCurrencyID, LocalCacheDB aSpareCacheDB, LocalCacheDBExtension aCurrentCacheDB)
        {
            FCurrentServerConnection.StopDataUpdating();
            try
            {
                aCurrentCacheDB.StartTransaction();
                aCurrentCacheDB.DeleteTransactions(aCurrencyID);
                aCurrentCacheDB.DeleteMonitoredAccounts(aCurrencyID);
                var lNewTransactions = aSpareCacheDB.ReadTransactionRecords(aCurrencyID);
                aCurrentCacheDB.Write(lNewTransactions, aCurrencyID);
                var lMonitoredAccounts = aSpareCacheDB.ReadMonitoredAccounts(aCurrencyID);
                aCurrentCacheDB.Write(lMonitoredAccounts);
                aCurrentCacheDB.EndTransaction(true);
            }
            catch (Exception)
            {
                aCurrentCacheDB.EndTransaction(false);
                throw;
            }
            FCurrentServerConnection.StartDataUpdating();
        }

        private void FillDataFromServer(LocalCacheDBExtension aCacheDB, CurrencyItem aCurrencyItem, OnCurrencyAddressesNeededDelegate aGetAddressFunction)
        {
            try
            {
                aCacheDB.StartTransaction();
                aCacheDB.Write(aCurrencyItem);
                aCacheDB.DeleteMonitoredAccounts(aCurrencyItem.Id);
                IEnumerable<CurrencyAccount> lMonitoredAccounts = FCurrentServerConnection.DirectGetMonitoredAcccounts(aCurrencyItem.Id, 0);
                if (lMonitoredAccounts.Count() < 2 && aGetAddressFunction != null)
                    lMonitoredAccounts = aGetAddressFunction.Invoke(lMonitoredAccounts, aCurrencyItem.Id, aCurrencyItem.ChainParamaters);
                aCacheDB.Write(lMonitoredAccounts);
                aCacheDB.DeleteTransactions(aCurrencyItem.Id);
                var lTransactionRecords = GetTxRecords(aCurrencyItem.Id);
                aCacheDB.Write(lTransactionRecords, aCurrencyItem.Id);
                aCacheDB.EndTransaction(true);
            }
            catch (Exception)
            {
                aCacheDB.EndTransaction(false);
                throw;
            }
        }

        private IEnumerable<TransactionRecord> GetTxRecords(long aCurrencyId)
        {
            List<TransactionRecord> lResult = new List<TransactionRecord>();
            IEnumerable<TransactionRecord> lCurrentBatch;
            long lTxCounter = 0;
            do
            {
                lCurrentBatch = FCurrentServerConnection.DirectGetTransactionRecords(aCurrencyId, lTxCounter);
                lResult.AddRange(lCurrentBatch);
                lTxCounter = lCurrentBatch.Select(lTx => lTx.TransactionRecordId).DefaultIfEmpty().Max();
            } while (lCurrentBatch.Any());
            return lResult;
        }

        private LocalCacheDBExtension CreateNewSpareDBStore()
        {
            string lSpareDBFile = GetSpareDBFileName();
            var lSpareDB = new LocalCacheDBExtension(lSpareDBFile);
            return lSpareDB;
        }

        private string GetSpareDBFileName()
        {
            return Path.Combine(FCurrentServerConnection.DataPath, $"{FCurrentServerConnection.InstanceId}.tmpspare");
        }

        public void Dispose()
        {
            try
            {
                string lDBFileName = GetSpareDBFileName();
                if (File.Exists(lDBFileName))
                    File.Delete(lDBFileName);
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Error, $"Failed to clear cache helper data. Exception {ex}");
            }
        }

        /// <summary>
        /// Extention class for normal localcachedb
        /// </summary>
        ///
        private class LocalCacheDBExtension : LocalCacheDB
        {
            //Note: I decided to create this class because I did not wanted to modify too much base class, as I am not sure what could be the consequences
            private string FConnectionString;

            private SQLiteTransaction FSQLTransaction;

            public bool TransactionActive => FSQLTransaction != null;

            public LocalCacheDBExtension(string aFileName) : base(aFileName)
            {
                FConnectionString = "Data Source=" + FileName + ";PRAGMA journal_mode=WAL;";
            }

            public void StartTransaction()
            {
                FSQLTransaction = FSQLiteConnection.BeginTransaction();
            }

            public void EndTransaction(bool isSuccess)
            {
                if (isSuccess)
                    FSQLTransaction.Commit();
                else
                    FSQLTransaction.Rollback();
            }

            public override void Write(TransactionRecord aTransactionRecord)
            {
                WriteTransaction(aTransactionRecord, FSQLiteConnection);
            }

            public override void Write(IEnumerable<CurrencyAccount> aCurrencyList)
            {
                if (!aCurrencyList.Any()) return;
                foreach (CurrencyAccount lCurrencyAccount in aCurrencyList)
                    WriteMonitoredAccount(lCurrencyAccount);
            }

            public void DeleteTransactions(long aCurrencyID)
            {
                var lCurrentTransactionRecords = ReadTransactionRecords(aCurrencyID);
                var lIds = lCurrentTransactionRecords.Select(lTxRecord => lTxRecord.TransactionRecordId);
                if (lIds.Any())
                    DeleteTransactionsByID(lIds);
            }

            private void DeleteTransactionsByID(IEnumerable<long> aIDs)
            {
                using (SQLiteCommand lSQLiteCommand = new SQLiteCommand($"delete from TxIn where TxIn.internalid in {BuildIdsString(aIDs)} ", FSQLiteConnection))
                    lSQLiteCommand.ExecuteNonQuery();
                using (SQLiteCommand lSQLiteCommand = new SQLiteCommand($"delete from TxOut where TxOut.internalid in {BuildIdsString(aIDs)} ", FSQLiteConnection))
                    lSQLiteCommand.ExecuteNonQuery();
                using (SQLiteCommand lSQLiteCommand = new SQLiteCommand($"delete from TxExt where TxExt.internalid in {BuildIdsString(aIDs)} ", FSQLiteConnection))
                    lSQLiteCommand.ExecuteNonQuery();
                using (SQLiteCommand lSQLiteCommand = new SQLiteCommand($"delete from TxTable where TxTable.internalid in {BuildIdsString(aIDs)} ", FSQLiteConnection))
                    lSQLiteCommand.ExecuteNonQuery();
            }

            private string BuildIdsString(IEnumerable<long> aIDs)
            {
                StringBuilder lBuilder = new StringBuilder();
                lBuilder.Append("(");
                foreach (var lID in aIDs)
                {
                    lBuilder.Append(lID);
                    lBuilder.Append(",");
                }
                lBuilder.Append("-1)");
                return lBuilder.ToString();
            }

            public void DeleteMonitoredAccounts(long aCurrencyID)
            {
                using (SQLiteCommand lSQLiteCommand = new SQLiteCommand($"delete from MonitoredAccounts where MonitoredAccounts.currencyid = @CurrencyId ", FSQLiteConnection))
                {
                    lSQLiteCommand.Parameters.Add(new SQLiteParameter("CurrencyId", aCurrencyID));
                    lSQLiteCommand.ExecuteNonQuery();
                }
            }
        }
    }
}