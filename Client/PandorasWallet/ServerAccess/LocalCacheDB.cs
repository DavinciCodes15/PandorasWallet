using Pandora.Client.ClientLib;
using Pandora.Client.Crypto.Currencies;
using Pandora.Client.PandorasWallet.Models;
using Pandora.Client.Universal;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Pandora.Client.PandorasWallet.ServerAccess
{
    internal class LocalCacheDB : IDisposable

    {
        private const int VERSION = 10019;

        protected SQLiteConnection FSQLiteConnection;

        public string FileName { get; private set; }

        public LocalCacheDB(string aFileName)
        {
            try
            {
                FileName = aFileName;

                FSQLiteConnection = new SQLiteConnection("Data Source=" + FileName + ";PRAGMA journal_mode=WAL;");
                FSQLiteConnection.Open();

                CreateTables();
            }
            catch
            {
                throw;
            }
        }

        protected virtual string[] GetTableNames()
        {
            List<string> lTableNames = new List<string>();

            try
            {
                using (SQLiteCommand command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type = 'table' and name not like 'Exchange%' and name not like 'sqlite%' ORDER BY 1", FSQLiteConnection))
                {
                    using (SQLiteDataReader rdr = command.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            lTableNames.Add(rdr.GetString(0));
                        }
                    }
                }

                return lTableNames.ToArray();
            }
            catch
            {
                throw;
            }
        }

        protected virtual void CreateTables()
        {
            using (SQLiteTransaction lTransaction = FSQLiteConnection.BeginTransaction())
            {
                try
                {
                    var lVersion = GetVersion();
                    Pandora.Client.Universal.Log.Write(Universal.LogLevel.Debug, "Opening database with Version {0}.", lVersion);
                    if (lVersion > VERSION)
                        throw new Exception($"The wallet data file belongs to a newer verion of Pandor's Wallet ({VERSION})");
                    if (lVersion < 10013)
                    {
                        ExecuteQuery("DROP TABLE IF EXISTS TxTable");
                        ExecuteQuery("DROP TABLE IF EXISTS TxIn");
                        ExecuteQuery("DROP TABLE IF EXISTS TxOut");
                        ExecuteQuery("DROP TABLE IF EXISTS TxExt");
                    }
                    ExecuteQuery("DROP TABLE IF EXISTS Version");
                    ExecuteQuery("CREATE TABLE IF NOT EXISTS MonitoredAccounts (id BIGINT, currencyid INT, address VARCHAR(100), PRIMARY KEY (id,currencyId))");
                    ExecuteQuery("CREATE TABLE IF NOT EXISTS Currencies (id INT PRIMARY KEY, name VARCHAR(100), ticker VARCHAR(50), precision INT, MinConfirmations INT, livedate DATETIME, Icon BLOB, IconSize int, FeePerKb int, ChainParams BLOB, status VARCHAR(50))");
                    ExecuteQuery("CREATE TABLE IF NOT EXISTS CurrenciesStatus (id BIGINT, currencyid INT PRIMARY KEY, statustime DATETIME, status VARCHAR(50), extinfo VARCHAR(255), blockheight INT)");
                    ExecuteQuery("CREATE TABLE IF NOT EXISTS TxTable (internalid BIGINT, currencyid BIGINT, id VARCHAR(100), dattime DATETIME, block BIGINT, TxFee BIGINT, Valid BOOLEAN, PRIMARY KEY(internalid, currencyid))");
                    ExecuteQuery("CREATE TABLE IF NOT EXISTS TxIn (internalid BIGINT, id BIGINT, address varchar(100), ammount BIGINT, ExAmount BIGINT DEFAULT 0, PRIMARY KEY (internalid,id,address,ammount))");
                    ExecuteQuery("CREATE TABLE IF NOT EXISTS TxOut (internalid BIGINT, id BIGINT, address varchar(100), ammount BIGINT, nindex INT, ExAmount BIGINT DEFAULT 0, PRIMARY KEY (internalid,id,address,ammount))");
                    ExecuteQuery("CREATE TABLE IF NOT EXISTS TxExt (internalid BIGINT PRIMARY KEY, extdata varchar(100))");

                    ExecuteQuery("CREATE TABLE IF NOT EXISTS BlockHeight (blockcount BIGINT, currencyid BIGINT PRIMARY KEY)");
                    ExecuteQuery("CREATE TABLE IF NOT EXISTS CurrencyToken (id int PRIMARY KEY, name VARCHAR(100), symbol VARCHAR(50), precision INT, parentcurrency INT, Icon BLOB, IconSize int, address VARCHAR(300))");

                    // KeyValue Table is a lookup table of data where the data must string with no greater than 500 and the key must be no more than 25char
                    ExecuteQuery("CREATE TABLE IF NOT EXISTS KeyValue (datavalue VARCHAR(500), keyname varchar(32) PRIMARY KEY)");
                    // this table contians all currencies the user has but currency with primaryid = 0 is the main crurrency for pricing
                    if (lVersion == 10011)
                    {
                        WritePrimaryCurrencyId(Convert.ToInt64(ExecuteQueryValue("SELECT currencyid FROM PrimaryCurrency where primaryid = 0")));
                        ExecuteQuery("DROP TABLE IF EXISTS PrimaryCurrency");
                    }
                    // Added tables to support token transactions. (Starting from version 10016)
                    ExecuteQuery("CREATE TABLE IF NOT EXISTS CurrencyVisible (CurrencyId INT PRIMARY KEY, Visible BOOLEAN)");
                    ExecuteQuery("CREATE TABLE IF NOT EXISTS TokenVisible (TokenID INT PRIMARY KEY, Visible BOOLEAN)");
                    ExecuteQuery("CREATE TABLE IF NOT EXISTS SentTransactions (TxId VARCHAR(100) PRIMARY KEY, CurrencyId BIGINT, TxData BLOB, SentTime DATETIME, ResponseTime DATETIME )");
                    ExecuteQuery("CREATE TABLE IF NOT EXISTS TokenTx (currencyTxID varchar(100), Txfrom varchar(100), TxTo varchar(100), amount BIGINT, examount BIGINT, tokenaddress varchar(100), valid BOOLEAN)");
                    if (0 < lVersion)
                    {
                        if (lVersion < 10014)
                        {
                            try
                            {
                                ExecuteQuery("ALTER TABLE TXOut ADD COLUMN ExAmount BIGINT DEFAULT 0");
                                ExecuteQuery("ALTER TABLE TXIn ADD COLUMN ExAmount BIGINT DEFAULT 0");
                            }
                            catch (Exception ex)
                            {
                                Universal.Log.Write(Universal.LogLevel.Warning, $"Exception thrown when altering table on application of DB Version 10014. {ex}");
                            }
                        }

                        if (lVersion < 10017)
                        {
                            //Remove past ethereum transactions from cache in case there was already a token in the address
                            ExecuteQuery("delete from TXIN where internalid in (select internalid from txtable where currencyid = 10194 or currencyid = 10196);");
                            ExecuteQuery("delete from TXOUT where internalid in (select internalid from txtable where currencyid = 10194 or currencyid = 10196);");
                            ExecuteQuery("delete from TXEXT where internalid in (select internalid from txtable where currencyid = 10194 or currencyid = 10196);");
                            ExecuteQuery("delete from TXTABLE where currencyid = 10194 or currencyid = 10196;");
                        }
                        if (lVersion < 10019)
                        {
                            try
                            {
                                ExecuteQuery("ALTER TABLE TokenTx ADD COLUMN valid BOOLEAN DEFAULT TRUE");
                            }
                            catch (Exception ex)
                            {
                                Universal.Log.Write(Universal.LogLevel.Warning, $"Exception thrown when altering table on application of DB Version 10014. {ex}");
                            }
                        }
                    }

                    WriteKeyValue("Version", VERSION.ToString());
                    lTransaction.Commit();
                }
                catch
                {
                    lTransaction.Rollback();
                    throw;
                }
            }
        }

        public void ExecuteQuery(string aQuery)
        {
            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand(aQuery, FSQLiteConnection))
                lSQLiteCommand.ExecuteNonQuery();
        }

        public object ExecuteQueryValue(string aQuery)
        {
            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand(aQuery, FSQLiteConnection))
            using (SQLiteDataReader lReader = lSQLiteCommand.ExecuteReader())
                if (lReader.Read())
                    return Convert.ToInt32(lReader.GetValue(0));
                else
                    return null;
        }

        public void WritePrimaryCurrencyId(long aCurrencyId)
        {
            WriteKeyValue("_PrimaryCurrency", aCurrencyId.ToString());
        }

        public long ReadPrimaryCurrencyId()
        {
            return Convert.ToInt64(ReadKeyValue("_PrimaryCurrency"));
        }

        public void WriteKeyValue(string aKey, string aValue)
        {
            if (aKey.Length > 32) throw new InvalidDataException("The Key argument is greater than 32");
            if (aValue.Length > 500) throw new InvalidDataException("The Value argument is greater than 500");

            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand("INSERT OR REPLACE INTO KeyValue (datavalue, keyname) VALUES (@Datavalue, @Keyname)", FSQLiteConnection))
            {
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("Datavalue", aValue));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("Keyname", aKey));
                lSQLiteCommand.ExecuteNonQuery();
            }
        }

        public string ReadKeyValue(string aKey)
        {
            string lQuery = $"SELECT datavalue FROM KeyValue where keyname = @Key";

            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand(lQuery, FSQLiteConnection))
            {
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("Key", aKey));
                using (SQLiteDataReader lSQLiteDataReader = lSQLiteCommand.ExecuteReader())
                    if (lSQLiteDataReader.Read())
                        return lSQLiteDataReader.GetString(0);
                    else
                        return null;
            }
        }

        private int GetVersion()
        {
            int lResult = -1;
            var lTables = GetTableNames();
            if (lTables.Any())
            {
                if (lTables.Contains("Version"))
                {
                    Pandora.Client.Universal.Log.Write(Universal.LogLevel.Debug, "Opening pre version");
                    string lQuery = "SELECT NVersion FROM Version";
                    using (SQLiteCommand lSQLiteCommand = new SQLiteCommand(lQuery, FSQLiteConnection))
                    using (SQLiteDataReader lReader = lSQLiteCommand.ExecuteReader())
                        if (lReader.Read())
                            lResult = Convert.ToInt32(lReader.GetInt32(0));
                    if (lResult != 10003) lResult = -1;
                }
                else if (lTables.Contains("KeyValue"))
                {
                    var lVal = this.ReadKeyValue("Version");
                    if (lVal != null)
                        try
                        {
                            lResult = Convert.ToInt32(lVal);
                        }
                        catch
                        {
                            throw new Exception($"Verion value found in DB is '{lVal}' this is not correct.");
                        }
                }
            }
            else
                lResult = 0;
            if (lResult == -1)
                throw new InvalidOperationException("Database is not valid.");
            return lResult;
        }

        public virtual void WriteMonitoredAccount(CurrencyAccount aCurrencyAccount)
        {
            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand("INSERT OR REPLACE INTO MonitoredAccounts (id, currencyid, address) VALUES (@Id, @currencyId, @address)", FSQLiteConnection))
            {
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("Id", aCurrencyAccount.Id));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("currencyId", aCurrencyAccount.CurrencyId));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("address", aCurrencyAccount.Address));
                lSQLiteCommand.ExecuteNonQuery();
            }
        }

        public virtual void Write(IEnumerable<CurrencyAccount> aCurrencyList)
        {
            if (!aCurrencyList.Any()) return;
            using (SQLiteTransaction lSQLiteTransaction = FSQLiteConnection.BeginTransaction())
                try
                {
                    foreach (CurrencyAccount lCurrencyAccount in aCurrencyList)
                        WriteMonitoredAccount(lCurrencyAccount);
                    lSQLiteTransaction.Commit();
                }
                catch
                {
                    lSQLiteTransaction.Rollback();
                    throw;
                }
        }

        public virtual void Write(CurrencyItem aCurrencyItem)
        {
            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand("INSERT OR REPLACE INTO Currencies (id, name, ticker, precision, MinConfirmations,livedate,Icon,IconSize,FeePerKb, ChainParams, Status) VALUES (@id,@name,@ticker,@precision,@MinConfirmations,@LiveDate,@Icon,@IconSize,@FeePerKb, @ChainParams,@Status)", FSQLiteConnection))
            {
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("id", aCurrencyItem.Id));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("name", aCurrencyItem.Name));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("ticker", aCurrencyItem.Ticker));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("precision", aCurrencyItem.Precision));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("MinConfirmations", aCurrencyItem.MinConfirmations));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("LiveDate", aCurrencyItem.LiveDate));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("Icon", aCurrencyItem.Icon));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("IconSize", aCurrencyItem.Icon.Count()));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("FeePerKb", aCurrencyItem.FeePerKb));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("Status", aCurrencyItem.CurrentStatus));
                DBChainParams lChainParamsCopy = new DBChainParams();
                using (MemoryStream lMemoryStream = new MemoryStream())
                {
                    BinaryFormatter lBinaryFormatter = new BinaryFormatter();

                    aCurrencyItem.ChainParamaters.CopyTo(lChainParamsCopy);

                    lBinaryFormatter.Serialize(lMemoryStream, lChainParamsCopy);

                    lSQLiteCommand.Parameters.Add(new SQLiteParameter("ChainParams", Convert.ToBase64String(lMemoryStream.ToArray())));
                }

                lSQLiteCommand.ExecuteNonQuery();
            }
        }

        public virtual void WriteBlockHeight(long aCurrencyId, long aBlockHeight)
        {
            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand("INSERT OR REPLACE INTO BlockHeight (blockcount , currencyid) VALUES (@height,@id)", FSQLiteConnection))
            {
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("height", aBlockHeight));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("id", aCurrencyId));
                lSQLiteCommand.ExecuteNonQuery();
            }
        }

        internal void WriteTransactionSentData(string aTxData, string aTxId, long aCurrencyId, DateTime aStartTime, DateTime aEndTime)
        {
            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand("INSERT OR REPLACE INTO SentTransactions (TxId, CurrencyId, TxData, SentTime, ResponseTime) VALUES (@TxId, @CurrencyId, @TxData, @SentTime, @ResponseTime)", FSQLiteConnection))
            {
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("TxId", aTxId));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("CurrencyId", aCurrencyId));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("TxData", aTxData));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("SentTime", aStartTime));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("ResponseTime", aEndTime));
                lSQLiteCommand.ExecuteNonQuery();
            }
        }

        public virtual void Write(List<CurrencyItem> aCurrItemList)
        {
            if (!aCurrItemList.Any()) return;

            using (SQLiteTransaction lSQLiteTransaction = FSQLiteConnection.BeginTransaction())
                try
                {
                    foreach (CurrencyItem lCurrencyItem in aCurrItemList)
                        Write(lCurrencyItem);
                    lSQLiteTransaction.Commit();
                }
                catch
                {
                    lSQLiteTransaction.Rollback();
                    throw;
                }
        }

        public virtual void Write(CurrencyStatusItem aCurrencyStatusItem)
        {
            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand("INSERT OR REPLACE INTO CurrenciesStatus (id, currencyid, statustime, status, extinfo, blockheight) VALUES (@StatusId,@currencyid,@statustime,@status,@extinfo,@blockheight)", FSQLiteConnection))
            {
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("StatusId", aCurrencyStatusItem.StatusId));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("currencyid", aCurrencyStatusItem.CurrencyId));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("statustime", aCurrencyStatusItem.StatusTime));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("status", aCurrencyStatusItem.Status.ToString()));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("extinfo", aCurrencyStatusItem.ExtendedInfo));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("blockheight", aCurrencyStatusItem.BlockHeight));

                lSQLiteCommand.ExecuteNonQuery();
            }
        }

        public bool Write(IClientCurrencyToken aCurrencyToken)
        {
            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand("INSERT OR REPLACE INTO CurrencyToken (id, name, symbol, precision, parentcurrency, Icon, IconSize, address) VALUES (@id, @name, @symbol, @precision, @parentcurrency, @icon, @iconsize, @address)", FSQLiteConnection))
            {
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("id", aCurrencyToken.Id));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("name", aCurrencyToken.Name));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("symbol", aCurrencyToken.Ticker));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("precision", aCurrencyToken.Precision));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("parentcurrency", aCurrencyToken.ParentCurrencyID));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("icon", aCurrencyToken.Icon));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("iconsize", aCurrencyToken.Icon.Count()));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("address", aCurrencyToken.ContractAddress));

                try
                {
                    lSQLiteCommand.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex)
                {
                    Universal.Log.Write(Universal.LogLevel.Error, "Write TxExt Cache exception: " + ex.Message + " on " + ex.Source);
                    return false;
                }
            }
        }

        public bool Write(IClientTokenTransaction aTokenTx)
        {
            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand("INSERT OR REPLACE INTO TokenTx (currencyTxID, TxFrom, TxTo, amount, examount, tokenaddress, valid) VALUES (@txid, @from, @to, @amount, @examount, @tokenaddress,@valid)", FSQLiteConnection))
            {
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("txid", aTokenTx.ParentTransactionID));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("from", aTokenTx.From));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("to", aTokenTx.To));
                long lAmount = 0;
                long lExAmount = 0;
                if (aTokenTx.Amount > long.MaxValue)
                {
                    var lAmounts = SystemUtils.ConvertHexToDoubleLong(aTokenTx.Amount.ToString("X"));
                    lAmount = lAmounts.Item1;
                    if (lAmounts.Item2.HasValue)
                        lExAmount = lAmounts.Item2.Value;
                }
                else
                    lAmount = (long)aTokenTx.Amount;

                lSQLiteCommand.Parameters.Add(new SQLiteParameter("amount", lAmount));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("examount", lExAmount));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("tokenaddress", aTokenTx.TokenAddress));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("valid", aTokenTx.Valid));

                try
                {
                    lSQLiteCommand.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex)
                {
                    Universal.Log.Write(Universal.LogLevel.Error, "Write TxExt Cache exception: " + ex.Message + " on " + ex.Source);
                    return false;
                }
            }
        }

        public bool UpdateTokenTransactions(string aParentTxId, bool aValid)
        {
            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand($"UPDATE TokenTx set valid = @valid where currencyTxID = @txid", FSQLiteConnection))
            {
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("txid", aParentTxId));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("valid", aValid));

                try
                {
                    lSQLiteCommand.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex)
                {
                    Universal.Log.Write(Universal.LogLevel.Error, $"Update token tx {aParentTxId} exception: {ex.Message} on {ex.Source}");
                    return false;
                }
            }
        }

        public IEnumerable<ITokenTransaction> ReadTokenTransactionsByCurrencyTxID(string aTransactionID)
        {
            var lTokenTransactionList = new List<ITokenTransaction>();
            string qry = $"SELECT currencyTxID, TxFrom, TxTo, amount, examount, tokenaddress, valid from TokenTx where currencyTxID = '{aTransactionID}'";
            using (SQLiteCommand command = new SQLiteCommand(qry, FSQLiteConnection))
            using (SQLiteDataReader rdr = command.ExecuteReader())
                while (rdr.Read())
                {
                    long lAmount = rdr.GetInt64(3);
                    long lExAmount = rdr.GetInt64(4);
                    lTokenTransactionList.Add(new DBTokenTransactionItem
                    {
                        ParentTransactionID = rdr.GetString(0),
                        From = rdr.GetString(1),
                        To = rdr.GetString(2),
                        Amount = SystemUtils.ConvertDoubleLongToBigInteger(lAmount, lExAmount),
                        TokenAddress = rdr.GetString(5),
                        Valid = rdr.GetBoolean(6)
                    });
                }
            return lTokenTransactionList;
        }

        public IEnumerable<ITokenTransaction> ReadTokenTransactions(string aTokenAddress)
        {
            var lTokenTransactionList = new List<ITokenTransaction>();
            string qry = $"SELECT currencyTxID, TxFrom, TxTo, amount, examount, tokenaddress, valid from TokenTx where tokenaddress = '{aTokenAddress}' and valid = 1";
            using (SQLiteCommand command = new SQLiteCommand(qry, FSQLiteConnection))
            using (SQLiteDataReader rdr = command.ExecuteReader())
                while (rdr.Read())
                {
                    long lAmount = rdr.GetInt64(3);
                    long lExAmount = rdr.GetInt64(4);
                    lTokenTransactionList.Add(new DBTokenTransactionItem
                    {
                        ParentTransactionID = rdr.GetString(0),
                        From = rdr.GetString(1),
                        To = rdr.GetString(2),
                        Amount = SystemUtils.ConvertDoubleLongToBigInteger(lAmount, lExAmount),
                        TokenAddress = rdr.GetString(5),
                        Valid = rdr.GetBoolean(6)
                    });
                }
            return lTokenTransactionList;
        }

        public struct DBTokenTransactionItem : ITokenTransaction
        {
            public string From { get; set; }

            public string To { get; set; }

            public BigInteger Amount { get; set; }

            public string TokenAddress { get; set; }

            public string ParentTransactionID { get; set; }

            public bool Valid { get; set; }
        }

        public struct DBCurrencyTokenItem : IClientCurrencyToken
        {
            public byte[] Icon { get; set; }

            public long Id { get; set; }

            public string ContractAddress { get; set; }

            public string Name { get; set; }

            public string Ticker { get; set; }

            public ushort Precision { get; set; }

            public long ParentCurrencyID { get; set; }
        }

        internal void WriteAtomicKeyValue(Dictionary<string, string> aDictionary)
        {
            using (SQLiteTransaction lSQLiteTransaction = FSQLiteConnection.BeginTransaction())
                try
                {
                    foreach (var lKeyValue in aDictionary)
                        WriteKeyValue(lKeyValue.Key, lKeyValue.Value);
                    lSQLiteTransaction.Commit();
                }
                catch
                {
                    lSQLiteTransaction.Rollback();
                    throw;
                }
        }

        public virtual void Write(List<CurrencyStatusItem> aCurrencyStatusItemList)
        {
            if (!aCurrencyStatusItemList.Any()) return;
            using (SQLiteTransaction lSQLiteTransaction = FSQLiteConnection.BeginTransaction())
                try
                {
                    foreach (CurrencyStatusItem it in aCurrencyStatusItemList)
                        Write(it);
                    lSQLiteTransaction.Commit();
                }
                catch
                {
                    lSQLiteTransaction.Rollback();
                    throw;
                }
        }

        public virtual void Write(TransactionRecord aTransactionRecord)
        {
            using (SQLiteTransaction lSQLiteTransaction = FSQLiteConnection.BeginTransaction())
                try
                {
                    WriteTransaction(aTransactionRecord, FSQLiteConnection);
                    lSQLiteTransaction.Commit();
                }
                catch
                {
                    lSQLiteTransaction.Rollback();
                    throw;
                }
        }

        protected void WriteTransaction(TransactionRecord aTransactionRecord, SQLiteConnection aSQLConnection)
        {
            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand("INSERT OR REPLACE INTO TxTable (internalid, currencyid, id, dattime, block, TxFee, Valid) VALUES (@internalid, @currencyid, @id, @dattime, @block, @TxFee, @Valid)", aSQLConnection))
            {
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("internalid", aTransactionRecord.TransactionRecordId));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("currencyid", aTransactionRecord.CurrencyId));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("id", aTransactionRecord.TxId));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("dattime", aTransactionRecord.TxDate));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("block", aTransactionRecord.Block));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("TxFee", aTransactionRecord.TxFee));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("Valid", aTransactionRecord.Valid));

                lSQLiteCommand.ExecuteNonQuery();
            }
            if (aTransactionRecord.Inputs != null)
                foreach (TransactionUnit lTransactionUnit in aTransactionRecord.Inputs)
                    using (SQLiteCommand lSQLiteCommand = new SQLiteCommand("INSERT OR REPLACE INTO TxIn (internalid, id, address, ammount, ExAmount) VALUES (@internalid, @id, @address, @ammount, @ExAmount)", aSQLConnection))
                    {
                        lSQLiteCommand.Parameters.Add(new SQLiteParameter("internalid", aTransactionRecord.TransactionRecordId));
                        lSQLiteCommand.Parameters.Add(new SQLiteParameter("id", lTransactionUnit.Id));
                        lSQLiteCommand.Parameters.Add(new SQLiteParameter("address", lTransactionUnit.Address));
                        long lAmount = 0;
                        long lExAmount = 0;
                        if (lTransactionUnit.Amount > long.MaxValue)
                        {
                            var lAmounts = SystemUtils.ConvertHexToDoubleLong(lTransactionUnit.Amount.ToString("X"));
                            lAmount = lAmounts.Item1;
                            if (lAmounts.Item2.HasValue)
                                lExAmount = lAmounts.Item2.Value;
                        }
                        else
                            lAmount = (long)lTransactionUnit.Amount;

                        lSQLiteCommand.Parameters.Add(new SQLiteParameter("ammount", lAmount));
                        lSQLiteCommand.Parameters.Add(new SQLiteParameter("ExAmount", lExAmount));

                        lSQLiteCommand.ExecuteNonQuery();
                    }
            if (aTransactionRecord.Outputs == null) throw new InvalidDataException($"Invalid tx '{aTransactionRecord.TxId}' it contains no outputs.");
            foreach (TransactionUnit lTransactionUnit in aTransactionRecord.Outputs)
            {
                using (SQLiteCommand lSQLiteCommand = new SQLiteCommand("INSERT OR REPLACE INTO TxOut (internalid, id, address, ammount, nindex, ExAmount) VALUES (@internalid, @id, @address, @ammount, @nindex, @ExAmount)", aSQLConnection))
                {
                    lSQLiteCommand.Parameters.Add(new SQLiteParameter("internalid", aTransactionRecord.TransactionRecordId));
                    lSQLiteCommand.Parameters.Add(new SQLiteParameter("id", lTransactionUnit.Id));
                    lSQLiteCommand.Parameters.Add(new SQLiteParameter("address", lTransactionUnit.Address));
                    long lAmount = 0;
                    long lExAmount = 0;
                    if (lTransactionUnit.Amount > long.MaxValue)
                    {
                        var lAmounts = SystemUtils.ConvertHexToDoubleLong(lTransactionUnit.Amount.ToString("X"));
                        lAmount = lAmounts.Item1;
                        if (lAmounts.Item2.HasValue)
                            lExAmount = lAmounts.Item2.Value;
                    }
                    else
                        lAmount = (long)lTransactionUnit.Amount;

                    lSQLiteCommand.Parameters.Add(new SQLiteParameter("ammount", lAmount));
                    lSQLiteCommand.Parameters.Add(new SQLiteParameter("ExAmount", lExAmount));
                    lSQLiteCommand.Parameters.Add(new SQLiteParameter("nindex", lTransactionUnit.Index));

                    lSQLiteCommand.ExecuteNonQuery();
                }
                if (!string.IsNullOrEmpty(lTransactionUnit.Script))
                    Write(lTransactionUnit.Id, lTransactionUnit.Script);
            }
        }

        public virtual void Write(IEnumerable<TransactionRecord> aTransactionRecordList, long aCurrencyId)
        {
            if (!aTransactionRecordList.Any()) return;
            foreach (TransactionRecord lTransactionRecord in aTransactionRecordList)
                Write(lTransactionRecord);
        }

        public void Write(long aInternalId, string aExtInfo)
        {
            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand("INSERT OR REPLACE INTO TxExt (internalid, extdata) VALUES (?,?)", FSQLiteConnection))
            {
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("internalid", aInternalId));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("extdata", aExtInfo));
                lSQLiteCommand.ExecuteNonQuery();
            }
        }

        public void WriteDisplayedCurrencyId(long aCurrencyId, bool aIsVisible)
        {
            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand("INSERT OR REPLACE INTO CurrencyVisible (CurrencyId, Visible) VALUES (?,?)", FSQLiteConnection))
            {
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("CurrencyId", aCurrencyId));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("Visible", aIsVisible));
                lSQLiteCommand.ExecuteNonQuery();
            }
        }

        public void WriteDisplayedTokenId(long aTokenId, bool aIsVisible)
        {
            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand("INSERT OR REPLACE INTO TokenVisible (TokenID, Visible) VALUES (?,?)", FSQLiteConnection))
            {
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("TokenID", aTokenId));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("Visible", aIsVisible));
                lSQLiteCommand.ExecuteNonQuery();
            }
        }

        public List<CurrencyAccount> ReadMonitoredAccounts(long aCurrencyId)
        {
            var lResult = new List<CurrencyAccount>();

            string lQuery = $"SELECT id, currencyid, address FROM MonitoredAccounts WHERE currencyId = {aCurrencyId} ORDER BY id ASC";

            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand(lQuery, FSQLiteConnection))
            {
                using (SQLiteDataReader lSQLiteDataReader = lSQLiteCommand.ExecuteReader())
                    while (lSQLiteDataReader.Read())
                        lResult.Add(new CurrencyAccount(Convert.ToUInt32(lSQLiteDataReader.GetInt64(0)), Convert.ToUInt32(lSQLiteDataReader.GetInt32(1)), lSQLiteDataReader.GetString(2)));
            }
            return lResult;
        }

        public List<CurrencyItem> ReadDisplayedCurrencies()
        {
            var lResult = new List<CurrencyItem>();

            string lQuery = "SELECT c1.id, name, ticker, precision, MinConfirmations, livedate, Icon, IconSize, FeePerKb, ChainParams, CASE WHEN cs.status IS NULL THEN c1.status ELSE cs.status END as 'Status' " +
                "FROM Currencies c1 inner join CurrencyVisible c2 on c1.id = c2.CurrencyId left join CurrenciesStatus cs on c2.CurrencyId = cs.currencyid " +
                "WHERE c2.Visible = true";

            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand(lQuery, FSQLiteConnection))
            using (SQLiteDataReader lSQLiteDataReader = lSQLiteCommand.ExecuteReader())
                while (lSQLiteDataReader.Read())
                    SqlReadCurrencyIntoList(lResult, lSQLiteDataReader);
            return lResult;
        }

        public IEnumerable<IClientCurrencyToken> ReadDisplayedTokens()
        {
            var lResult = new List<IClientCurrencyToken>();

            string lQuery = "SELECT CT.id, CT.name, CT.symbol, CT.precision, CT.parentcurrency, CT.Icon, CT.IconSize, CT.address from CurrencyToken CT inner join TokenVisible TV on CT.id = TV.tokenid " +
                            "WHERE TV.Visible = true";

            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand(lQuery, FSQLiteConnection))
            using (SQLiteDataReader lSQLiteDataReader = lSQLiteCommand.ExecuteReader())
                while (lSQLiteDataReader.Read())
                    SqlReadCurrencyTokensIntoList(lResult, lSQLiteDataReader);
            return lResult;
        }

        public long ReadBlockHeight(long aCurrencyId)
        {
            string lQuery = $"SELECT blockcount FROM BlockHeight where currencyid = {aCurrencyId}";

            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand(lQuery, FSQLiteConnection))
            using (SQLiteDataReader lSQLiteDataReader = lSQLiteCommand.ExecuteReader())
                if (lSQLiteDataReader.Read())
                    return lSQLiteDataReader.GetInt64(0);
                else
                    return 0;
        }

        public IEnumerable<CurrencyItem> ReadCurrencies(long? aId = null)
        {
            var lCurrencyList = new List<CurrencyItem>();

            string lWhere = aId.HasValue ? (" WHERE id = " + aId.Value) : string.Empty;
            string lQuery = "SELECT id, name, ticker, precision, MinConfirmations, livedate, Icon, IconSize, FeePerKb, ChainParams, Status FROM Currencies" + lWhere;

            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand(lQuery, FSQLiteConnection))
            using (SQLiteDataReader lSQLiteDataReader = lSQLiteCommand.ExecuteReader())
                while (lSQLiteDataReader.Read())
                    SqlReadCurrencyIntoList(lCurrencyList, lSQLiteDataReader);
            return lCurrencyList;
        }

        private static void SqlReadCurrencyTokensIntoList(List<IClientCurrencyToken> aList, SQLiteDataReader aReader)
        {
            byte[] lIcon = new byte[aReader.GetInt32(6)];
            aReader.GetBytes(5, 0, lIcon, 0, lIcon.Length);
            aList.Add(new DBCurrencyTokenItem
            {
                Id = aReader.GetInt32(0),
                Name = aReader.GetString(1),
                Ticker = aReader.GetString(2),
                Precision = Convert.ToUInt16(aReader.GetInt32(3)),
                ParentCurrencyID = aReader.GetInt64(4),
                Icon = lIcon,
                ContractAddress = aReader.GetString(7)
            });
        }

        public IEnumerable<IClientCurrencyToken> ReadCurrencyTokens(string aContractAddress = null, long? aTokenID = null)
        {
            var lCurrencyTokenList = new List<IClientCurrencyToken>();
            string qry = "SELECT id, name, symbol, precision, parentcurrency, Icon, IconSize, address from CurrencyToken WHERE TRUE";
            if (!string.IsNullOrEmpty(aContractAddress))
                qry += $" AND ADDRESS = '{aContractAddress}'";
            if (aTokenID.HasValue)
                qry += $" AND id = {aTokenID.Value}";
            try
            {
                using (SQLiteCommand command = new SQLiteCommand(qry, FSQLiteConnection))
                using (SQLiteDataReader lSQLiteDataReader = command.ExecuteReader())
                    while (lSQLiteDataReader.Read())
                        SqlReadCurrencyTokensIntoList(lCurrencyTokenList, lSQLiteDataReader);
            }
            catch (Exception ex)
            {
                Universal.Log.Write(Universal.LogLevel.Error, "Read CurrencyList cache exception: " + ex.Message + " on " + ex.Source);
            }

            return lCurrencyTokenList;
        }

        internal CurrencyItem ReadCurrency(long aCurrencyId)
        {
            var lCurrencies = ReadCurrencies(aCurrencyId);
            if (lCurrencies.Count() > 1)
                Universal.Log.Write(LogLevel.Error, $"Duplicated currency with id {aCurrencyId} found"); // I will leave this error message as a precaution
            return lCurrencies.FirstOrDefault();
        }

        private static void SqlReadCurrencyIntoList(IList<CurrencyItem> aCurrencyList, SQLiteDataReader aSQLiteDataReader)
        {
            byte[] lIcon = new byte[aSQLiteDataReader.GetInt32(7)];

            byte[] lChainParams = Convert.FromBase64String(aSQLiteDataReader.GetString(9));

            DBChainParams lChainParamsObject;

            using (MemoryStream ms = new MemoryStream(lChainParams))
            {
                BinaryFormatter lBf = new BinaryFormatter();
                lBf.Binder = DBChainParams.GetSerializationBinder();
                lChainParamsObject = (DBChainParams)lBf.Deserialize(ms);
            }

            aSQLiteDataReader.GetBytes(6, 0, lIcon, 0, aSQLiteDataReader.GetInt32(7));

            ChainParams lParams = new ChainParams();
            lParams.CopyFrom(lChainParamsObject);

            aCurrencyList.Add(new CurrencyItem
            {
                Id = Convert.ToUInt32(aSQLiteDataReader.GetInt32(0)),
                Name = aSQLiteDataReader.GetString(1),
                Ticker = aSQLiteDataReader.GetString(2),
                Precision = Convert.ToUInt16(aSQLiteDataReader.GetInt16(3)),
                LiveDate = aSQLiteDataReader.GetDateTime(5),
                MinConfirmations = Convert.ToUInt16(aSQLiteDataReader.GetInt16(4)),
                Icon = lIcon,
                FeePerKb = aSQLiteDataReader.GetInt64(8),
                ChainParamaters = lParams,
                CurrentStatus = (CurrencyStatus)Enum.Parse(typeof(CurrencyStatus), aSQLiteDataReader.GetString(10))
            });
        }

        #region ClearCacheWallet

        public void ClearCacheWallet()
        {
            try
            {
                ExecuteQuery("DELETE FROM TxTable where true;");
                ExecuteQuery("DELETE FROM TxIn where true;");
                ExecuteQuery("DELETE FROM TxOut where true;");
                ExecuteQuery("DELETE FROM TxExT where true;");
                ExecuteQuery("DELETE FROM TokenTx where true;");
            }
            catch (Exception ex)
            {
                Universal.Log.Write(Universal.LogLevel.Error, "Error during truncating tables Txs: " + ex.Message);
                throw;
            }
        }

        #endregion ClearCacheWallet

        public CurrencyStatusItem ReadCurrencyStatus(long aCurrencyId)
        {
            string lQuery = $"SELECT id, currencyid, statustime, status, extinfo, blockheight FROM CurrenciesStatus WHERE currencyid = {aCurrencyId} order by id desc limit 1";

            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand(lQuery, FSQLiteConnection))
            using (SQLiteDataReader lSQLiteDataReader = lSQLiteCommand.ExecuteReader())
                if (lSQLiteDataReader.Read())
                    return new CurrencyStatusItem(lSQLiteDataReader.GetInt64(0), lSQLiteDataReader.GetInt32(1), lSQLiteDataReader.GetDateTime(2), (CurrencyStatus)Enum.Parse(typeof(CurrencyStatus), lSQLiteDataReader.GetString(3)), lSQLiteDataReader.GetString(4), lSQLiteDataReader.GetInt64(5));
                else
                    return null;
        }

        public virtual IEnumerable<TransactionRecord> ReadTransactionRecords(long aCurrencyId)
        {
            var lQuery = $"SELECT internalid, currencyid, id, dattime, block, TxFee, Valid FROM TxTable WHERE currencyid = {aCurrencyId} and Valid = 1 order by internalid desc";
            return InternalReadTransactionRecords(lQuery);
        }

        public virtual IEnumerable<TransactionRecord> ReadTransactionRecords(string aTransactionID)
        {
            var lQuery = $"SELECT internalid, currencyid, id, dattime, block, TxFee, Valid FROM TxTable WHERE id = '{aTransactionID}' order by internalid desc";
            return InternalReadTransactionRecords(lQuery);
        }

        public virtual IEnumerable<TransactionRecord> ReadTransactionRecords(long aCurrencyId, long aMaxBlockHeight)
        {
            var lQuery = $"SELECT internalid, currencyid, id, dattime, block, TxFee, Valid FROM TxTable WHERE currencyid = {aCurrencyId} and Valid = true and (block > {aMaxBlockHeight} or block = 0) order by internalid desc";
            return InternalReadTransactionRecords(lQuery);
        }

        public virtual TransactionRecord ReadLastTransactionRecord(long aCurrencyId)
        {
            var lQuery = $"SELECT internalid, currencyid, id, dattime, block, TxFee, Valid FROM TxTable WHERE currencyid = {aCurrencyId} order by internalid desc limit 1";

            var lResult = InternalReadTransactionRecords(lQuery);
            if (lResult.Any())
                return lResult[0];
            else
                return null;
        }

        private List<TransactionRecord> InternalReadTransactionRecords(string aQuery)
        {
            var aTxList = new List<TransactionRecord>();

            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand(aQuery, FSQLiteConnection))
            using (SQLiteDataReader lSQLiteDataReader = lSQLiteCommand.ExecuteReader())
                while (lSQLiteDataReader.Read())
                    aTxList.Add(new TransactionRecord(
                        lSQLiteDataReader.GetInt64(0),
                        lSQLiteDataReader.GetInt64(1),
                        lSQLiteDataReader.GetString(2),
                        lSQLiteDataReader.GetDateTime(3),
                        lSQLiteDataReader.GetInt64(4),
                        lSQLiteDataReader.GetBoolean(6)));

            foreach (TransactionRecord lTransactionRecord in aTxList)
            {
                aQuery = $"SELECT internalid, id, address, ammount, ExAmount FROM TxIn WHERE internalid = {lTransactionRecord.TransactionRecordId}";

                using (SQLiteCommand lSQLiteCommand = new SQLiteCommand(aQuery, FSQLiteConnection))
                using (SQLiteDataReader lSQLiteDataReader = lSQLiteCommand.ExecuteReader())
                    while (lSQLiteDataReader.Read())
                    {
                        long lAmount = lSQLiteDataReader.GetInt64(3);
                        long lExAmount = lSQLiteDataReader.GetInt64(4);
                        lTransactionRecord.AddInput(SystemUtils.ConvertDoubleLongToBigInteger(lAmount, lExAmount), lSQLiteDataReader.GetString(2), lSQLiteDataReader.GetInt64(1));
                    }

                aQuery = $"SELECT TxO.internalid, TxO.id, TxO.address, TxO.ammount, TxO.nindex, TxO.ExAmount, TxE.extdata FROM TxOut TxO LEFT JOIN TxExt TxE ON TxO.id = TxE.internalid WHERE TxO.internalid =  {lTransactionRecord.TransactionRecordId}";

                using (SQLiteCommand lSQLiteCommand = new SQLiteCommand(aQuery, FSQLiteConnection))
                using (SQLiteDataReader lSQLiteDataReader = lSQLiteCommand.ExecuteReader())
                    while (lSQLiteDataReader.Read())
                    {
                        long lAmount = lSQLiteDataReader.GetInt64(3);
                        long lExAmount = lSQLiteDataReader.GetInt64(5);
                        var lOutputScript = lSQLiteDataReader.IsDBNull(6) ? null : lSQLiteDataReader.GetString(6);
                        lTransactionRecord.AddOutput(SystemUtils.ConvertDoubleLongToBigInteger(lAmount, lExAmount), lSQLiteDataReader.GetString(2), lSQLiteDataReader.GetInt32(4), lSQLiteDataReader.GetInt64(1), lTransactionRecord.TxId, lOutputScript);
                    }
            }
            return aTxList;
        }

        public string CreateDBFileCopy()
        {
            string lQuery = "BEGIN IMMEDIATE;"; //THIS QUERY FORCES ALL WRITING OPERATIONS TO BE PAUSED
            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand(lQuery, FSQLiteConnection))
                lSQLiteCommand.ExecuteNonQuery();
            string lCopyFileName = string.Concat(this.FileName, "_copy");
            if (File.Exists(lCopyFileName))
                File.Delete(lCopyFileName);
            File.Copy(this.FileName, lCopyFileName);
            lQuery = "ROLLBACK;"; //THIS RELEASES THE BEGIN IMMEDIATE LOCK OF WRITING
            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand(lQuery, FSQLiteConnection))
                lSQLiteCommand.ExecuteNonQuery();
            return lCopyFileName;
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    FSQLiteConnection.Close();

                    SQLiteConnection.ClearPool(FSQLiteConnection);
                    FSQLiteConnection.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support

        [Serializable]
        public class DBChainParams : IChainParams
        {
            public long ForkFromId { get; set; }
            public ChainParams.NetworkType Network { get; set; }
            public string NetworkName { get; set; }
            public byte[] PublicKeyAddress { get; set; }
            public byte[] ScriptAddress { get; set; }
            public byte[] SecretKey { get; set; }
            public byte[] ExtPublicKey { get; set; }
            public byte[] ExtSecretKey { get; set; }
            public byte[] EncryptedSecretKeyNoEc { get; set; }
            public byte[] EncryptedSecretKeyEc { get; set; }
            public byte[] PasspraseCode { get; set; }
            public byte[] ConfirmationCode { get; set; }
            public byte[] StealthAddress { get; set; }
            public byte[] AssetId { get; set; }
            public byte[] ColoredAddress { get; set; }
            public string Encoder { get; set; }
            public CapablityFlags Capabilities { get; set; }
            public long Version { get; set; }

            public static System.Runtime.Serialization.SerializationBinder GetSerializationBinder()
            {
                return new ChainParamsBinder();
            }

            private class ChainParamsBinder : System.Runtime.Serialization.SerializationBinder
            {
                private List<Type> FTypesHandled;

                public ChainParamsBinder() : base()
                {
                    FTypesHandled = new List<Type>();
                    FTypesHandled.Add(typeof(DBChainParams));
                    FTypesHandled.Add(typeof(ChainParams.NetworkType));
                    FTypesHandled.Add(typeof(CapablityFlags));
                }

                public override Type BindToType(string assemblyName, string aTypeName)
                {
                    Type lResult = null;
                    string lTypeName = aTypeName.ToLowerInvariant();
                    if (lTypeName.Contains(@"networktype"))
                        lResult = typeof(ChainParams.NetworkType);
                    else if (lTypeName.Contains(@"chainparams"))
                        lResult = typeof(DBChainParams);
                    else if (lTypeName.Contains(@"capablityflags"))
                        lResult = typeof(CapablityFlags);
                    return lResult;
                }
            }
        }
    }
}