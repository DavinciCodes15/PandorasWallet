using Pandora.Client.ClientLib;
using Pandora.Client.Crypto.Currencies;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Pandora.Client.PandorasWallet.ServerAccess
{
    internal class LocalCacheDB : IDisposable

    {
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

        private const int VERSION = 10013;

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
                    ExecuteQuery("CREATE TABLE IF NOT EXISTS TxIn (internalid BIGINT, id BIGINT, address varchar(100), ammount BIGINT, PRIMARY KEY (internalid,id,address,ammount))");
                    ExecuteQuery("CREATE TABLE IF NOT EXISTS TxOut (internalid BIGINT, id BIGINT, address varchar(100), ammount BIGINT, nindex INT, PRIMARY KEY (internalid,id,address,ammount))");
                    ExecuteQuery("CREATE TABLE IF NOT EXISTS TxExt (internalid BIGINT PRIMARY KEY, extdata varchar(100))");
                    ExecuteQuery("CREATE TABLE IF NOT EXISTS BlockHeight (blockcount BIGINT, currencyid BIGINT PRIMARY KEY)");
                    // KeyValue Table is a lookup table of data where the data must string with no greater than 500 and the key must be no more than 25char
                    ExecuteQuery("CREATE TABLE IF NOT EXISTS KeyValue (datavalue VARCHAR(500), keyname varchar(32) PRIMARY KEY)");
                    // this table contians all currencies the user has but currency with primaryid = 0 is the main crurrency for pricing
                    if (lVersion == 10011)
                    {
                        WritePrimaryCurrencyId(Convert.ToInt64(ExecuteQueryValue("SELECT currencyid FROM PrimaryCurrency where primaryid = 0")));
                        ExecuteQuery("DROP TABLE IF EXISTS PrimaryCurrency");
                    }

                    ExecuteQuery("CREATE TABLE IF NOT EXISTS CurrencyVisible (CurrencyId INT PRIMARY KEY, Visible BOOLEAN)");
                    ExecuteQuery("CREATE TABLE IF NOT EXISTS SentTransactions (TxId VARCHAR(100) PRIMARY KEY, CurrencyId BIGINT, TxData BLOB, SentTime DATETIME, ResponseTime DATETIME )");
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
                PandorasCache.MyCacheChainParams lChainParamsCopy = new PandorasCache.MyCacheChainParams();
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
                    using (SQLiteCommand lSQLiteCommand = new SQLiteCommand("INSERT OR REPLACE INTO TxIn (internalid, id, address, ammount) VALUES (@internalid, @id, @address, @ammount)", aSQLConnection))
                    {
                        lSQLiteCommand.Parameters.Add(new SQLiteParameter("internalid", aTransactionRecord.TransactionRecordId));
                        lSQLiteCommand.Parameters.Add(new SQLiteParameter("id", lTransactionUnit.Id));
                        lSQLiteCommand.Parameters.Add(new SQLiteParameter("address", lTransactionUnit.Address));
                        lSQLiteCommand.Parameters.Add(new SQLiteParameter("ammount", lTransactionUnit.Amount));

                        lSQLiteCommand.ExecuteNonQuery();
                    }

            foreach (TransactionUnit lTransactionUnit in aTransactionRecord.Outputs)
                using (SQLiteCommand lSQLiteCommand = new SQLiteCommand("INSERT OR REPLACE INTO TxOut (internalid, id, address, ammount, nindex) VALUES (@internalid, @id, @address, @ammount, @nindex)", aSQLConnection))
                {
                    lSQLiteCommand.Parameters.Add(new SQLiteParameter("internalid", aTransactionRecord.TransactionRecordId));
                    lSQLiteCommand.Parameters.Add(new SQLiteParameter("id", lTransactionUnit.Id));
                    lSQLiteCommand.Parameters.Add(new SQLiteParameter("address", lTransactionUnit.Address));
                    lSQLiteCommand.Parameters.Add(new SQLiteParameter("ammount", lTransactionUnit.Amount));
                    lSQLiteCommand.Parameters.Add(new SQLiteParameter("nindex", lTransactionUnit.Index));

                    lSQLiteCommand.ExecuteNonQuery();
                }
        }

        public virtual void Write(IEnumerable<TransactionRecord> aTransactionRecordList, long aCurrencyId)
        {
            if (!aTransactionRecordList.Any()) return;
            foreach (TransactionRecord lTransactionRecord in aTransactionRecordList)
                Write(lTransactionRecord);
        }

        public void Write(string aInternalId, string aExtInfo)
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

            string lQuery = "SELECT c2.id, name, ticker, precision, MinConfirmations, livedate, Icon, IconSize, FeePerKb, ChainParams, Status FROM Currencies c2, CurrencyVisible m where c2.id = m.CurrencyId AND m.Visible = true";

            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand(lQuery, FSQLiteConnection))
            using (SQLiteDataReader lSQLiteDataReader = lSQLiteCommand.ExecuteReader())
                while (lSQLiteDataReader.Read())
                    SqlReadCurrencyIntoList(lResult, lSQLiteDataReader);
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

        public void ReadCurrencies(out List<CurrencyItem> aCurrencyList, long? aId = null)
        {
            aCurrencyList = new List<CurrencyItem>();

            string lWhere = aId.HasValue ? (" WHERE id = " + aId.Value) : string.Empty;
            string lQuery = "SELECT id, name, ticker, precision, MinConfirmations, livedate, Icon, IconSize, FeePerKb, ChainParams, Status FROM Currencies" + lWhere;

            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand(lQuery, FSQLiteConnection))
            using (SQLiteDataReader lSQLiteDataReader = lSQLiteCommand.ExecuteReader())
                while (lSQLiteDataReader.Read())
                    SqlReadCurrencyIntoList(aCurrencyList, lSQLiteDataReader);
        }

        internal CurrencyItem ReadCurrency(long aCurrencyId)
        {
            ReadCurrencies(out List<CurrencyItem> lList, aCurrencyId);
            return lList.FirstOrDefault();
        }

        private static void SqlReadCurrencyIntoList(List<CurrencyItem> aCurrencyList, SQLiteDataReader aSQLiteDataReader)
        {
            byte[] lIcon = new byte[aSQLiteDataReader.GetInt32(7)];

            byte[] lChainParams = Convert.FromBase64String(aSQLiteDataReader.GetString(9));

            PandorasCache.MyCacheChainParams lChainParamsObject;

            using (MemoryStream ms = new MemoryStream(lChainParams))
            {
                BinaryFormatter lBf = new BinaryFormatter();

                lChainParamsObject = (PandorasCache.MyCacheChainParams)lBf.Deserialize(ms);
            }

            aSQLiteDataReader.GetBytes(6, 0, lIcon, 0, aSQLiteDataReader.GetInt32(7));

            ChainParams lParams = new ChainParams();
            lParams.CopyFrom(lChainParamsObject);

            aCurrencyList.Add(new CurrencyItem(
                Convert.ToUInt32(aSQLiteDataReader.GetInt32(0)),
                aSQLiteDataReader.GetString(1),
                aSQLiteDataReader.GetString(2),
                Convert.ToUInt16(aSQLiteDataReader.GetInt16(3)),
                aSQLiteDataReader.GetDateTime(5),
                Convert.ToUInt16(aSQLiteDataReader.GetInt16(4)),
                lIcon,
                aSQLiteDataReader.GetInt32(8),
                lParams,
                (CurrencyStatus)Enum.Parse(typeof(CurrencyStatus), aSQLiteDataReader.GetString(10))
                ));
        }

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

        public virtual List<TransactionRecord> ReadTransactionRecords(long aCurrencyId)
        {
            var lQuery = $"SELECT internalid, currencyid, id, dattime, block, TxFee, Valid FROM TxTable WHERE currencyid = {aCurrencyId} and Valid = 1 order by internalid desc";
            return ReadTransactionRecords(lQuery);
        }

        public virtual List<TransactionRecord> ReadTransactionRecords(long aCurrencyId, long aMaxBlockHeight)
        {
            var lQuery = $"SELECT internalid, currencyid, id, dattime, block, TxFee, Valid FROM TxTable WHERE currencyid = {aCurrencyId} and (block > {aMaxBlockHeight} or block = 0) order by internalid desc";
            return ReadTransactionRecords(lQuery);
        }

        public virtual TransactionRecord ReadLastTransactionRecord(long aCurrencyId)
        {
            var lQuery = $"SELECT internalid, currencyid, id, dattime, block, TxFee, Valid FROM TxTable WHERE currencyid = {aCurrencyId} order by internalid desc limit 1";

            var lResult = ReadTransactionRecords(lQuery);
            if (lResult.Any())
                return lResult[0];
            else
                return null;
        }

        private List<TransactionRecord> ReadTransactionRecords(string aQuery)
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
                aQuery = $"SELECT internalid, id, address, ammount FROM TxIn WHERE internalid = {lTransactionRecord.TransactionRecordId}";

                using (SQLiteCommand lSQLiteCommand = new SQLiteCommand(aQuery, FSQLiteConnection))
                using (SQLiteDataReader lSQLiteDataReader = lSQLiteCommand.ExecuteReader())
                    while (lSQLiteDataReader.Read())
                        lTransactionRecord.AddInput(lSQLiteDataReader.GetInt64(3), lSQLiteDataReader.GetString(2), lSQLiteDataReader.GetInt64(1));

                aQuery = $"SELECT internalid, id, address, ammount, nindex FROM TxOut WHERE internalid = {lTransactionRecord.TransactionRecordId}";

                using (SQLiteCommand lSQLiteCommand = new SQLiteCommand(aQuery, FSQLiteConnection))
                using (SQLiteDataReader lSQLiteDataReader = lSQLiteCommand.ExecuteReader())
                    while (lSQLiteDataReader.Read())
                        lTransactionRecord.AddOutput(lSQLiteDataReader.GetInt64(3), lSQLiteDataReader.GetString(2), lSQLiteDataReader.GetInt32(4), lSQLiteDataReader.GetInt64(1), lTransactionRecord.TxId);
            }
            return aTxList;
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

        //private class MyCacheChainParams : PandorasCache.MyCacheChainParams
        //{
        //}
    }
}