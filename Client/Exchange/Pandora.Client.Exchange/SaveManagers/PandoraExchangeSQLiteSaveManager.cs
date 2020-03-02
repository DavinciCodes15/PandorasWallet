using Pandora.Client.Exchange.Objects;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace Pandora.Client.Exchange.SaveManagers
{
    public partial class PandoraExchangeSQLiteSaveManager : IPandoraSaveManager
    {
        private const int CURRENT_VERSION = 104;
        private const int VERSION_ID = 1; //This should not change unless you want to record more than one version inside the db
        private const string VERSION_NOTE = "ADDED ORDER EXCHANGE ID FOR MULTIPLE EXCHANGES";
        private string FConnectionString;
        private bool FReadOnly;

        public bool Initialized => !string.IsNullOrEmpty(FConnectionString);

        public int Version => GetVersion();

        public string DBFilePath { get; set; }

        [Obsolete("Public ctor deprecated, please use Factory instead.")]
        public PandoraExchangeSQLiteSaveManager(string aFilePath, string aUsername, string aEmail)
        {
            Initialize(aFilePath, aUsername, aEmail);
            FReadOnly = false;
        }

        public static string BuildPath(string aDataPath, string aInstanceID)
        {
            return Path.Combine(aDataPath, $"{aInstanceID}.exchange");
        }

        private PandoraExchangeSQLiteSaveManager(string aFilePath)
        {
            Initialize(aFilePath);
            FReadOnly = true;
        }

        /// <summary>
        /// Instantiate this object in read only mode
        /// </summary>
        /// <param name="aFilePath">Full path of exchange file to read</param>
        /// <returns></returns>
        public static PandoraExchangeSQLiteSaveManager GetReadOnlyInstance(string aFilePath)
        {
            return new PandoraExchangeSQLiteSaveManager(aFilePath);
        }

        private PandoraExchangeSQLiteSaveManager()
        {
            FReadOnly = false;
        }

        public static PandoraExchangeSQLiteSaveManager GetNewSaveManager()
        {
            return new PandoraExchangeSQLiteSaveManager();
        }

        public void Initialize(params string[] aInitializeParamethers)
        {
            string lUsername = string.Empty;
            string lEmail = string.Empty;
            string lDataPath = aInitializeParamethers[0];
            if (aInitializeParamethers.Length > 1)
            {
                lUsername = aInitializeParamethers[1];
                if (aInitializeParamethers.Length >= 3) lEmail = aInitializeParamethers[2];
                FConnectionString = $"Data Source={lDataPath};PRAGMA journal_mode=WAL;";
                CreateVersionTableIfNeeded();
                ApplyVersionChangesIfNeeded(lUsername, lEmail);
            }
            else
            {
                FConnectionString = $"Data Source={lDataPath};PRAGMA journal_mode=WAL;";
            }
            DBFilePath = lDataPath;
        }

        private void CreateVersionTableIfNeeded()
        {
            if (!Initialized && !FReadOnly)
                throw new Exception("Save manager not initialized");
            using (SQLiteConnection lConnection = new SQLiteConnection(FConnectionString))
            {
                lConnection.Open();
                using (var lSqliteTransaction = lConnection.BeginTransaction())
                    try
                    {
                        using (SQLiteCommand lCreateVersionTable = new SQLiteCommand("CREATE TABLE IF NOT EXISTS Version (ID PRIMARY KEY, NVersion INT, username varchar, email varchar, note varchar)", lConnection))
                            lCreateVersionTable.ExecuteNonQuery();
                        lSqliteTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Universal.Log.Write(Universal.LogLevel.Error, $"Exchange SQLite exception when creating version table. Exception {ex}");
                        throw;
                    }
                lConnection.Close();
            }
        }

        private void ApplyVersionChangesIfNeeded(string aUsername = null, string aEmail = null)
        {
            if (!Initialized && !FReadOnly)
                throw new Exception("Save manager not initialized");
            using (SQLiteConnection lConnection = new SQLiteConnection(FConnectionString))
            {
                lConnection.Open();
                var lCurrentFileVersion = Version;
                if (lCurrentFileVersion < CURRENT_VERSION)
                {
                    CreateInitialDBTables(lConnection);
                    Version101DoAlterExchangeTx(lConnection);
                    Version102DoAddExchangeProfileTable(lConnection);
                    Version103DoAddExchangeKeyValueTable(lConnection);
                    Version104DoAddOrderExchangeIDRelationshipTable(lConnection);
                    WriteVersion(VERSION_ID, CURRENT_VERSION, VERSION_NOTE, aUsername, aEmail, lConnection);
                }
                else if (lCurrentFileVersion > CURRENT_VERSION)
                    throw new Exception("Exchange file version out of range");
                lConnection.Close();
            }
        }

        private void CreateInitialDBTables(SQLiteConnection aOpenConnection)
        {
            if (!Initialized && !FReadOnly)
                throw new Exception("Save manager not initialized");
            using (var lSqliteTransaction = aOpenConnection.BeginTransaction())
                try
                {
                    using (SQLiteCommand lCreateTransactionTable = new SQLiteCommand("CREATE TABLE IF NOT EXISTS ExchangeTx (InternalID integer PRIMARY KEY autoincrement, ID varchar(255) , ProfileID INT,Market VARCHAR(100), Quantity numeric, OpenTime Datetime, Rate FLOAT, Stop FLOAT, Completed INT, Cancelled INT, Status INT, CoinTXID varchar(255), BaseTicker varchar(255), Name varchar(255))", aOpenConnection))
                        lCreateTransactionTable.ExecuteNonQuery();
                    using (SQLiteCommand lCreateLogTable = new SQLiteCommand("CREATE TABLE IF NOT EXISTS ExchangeLog (ID integer PRIMARY KEY autoincrement, TransactionID int, TransactionTime datetime, Message varchar(255), MessageLevel int)", aOpenConnection))
                        lCreateLogTable.ExecuteNonQuery();
                    lSqliteTransaction.Commit();
                }
                catch (Exception ex)
                {
                    Universal.Log.Write(Universal.LogLevel.Error, $"Exchange SQLite exception when implementing version 102. Exception {ex}");
                    lSqliteTransaction.Rollback();
                    throw;
                }
        }

        private void Version104DoAddOrderExchangeIDRelationshipTable(SQLiteConnection aOpenConnection)
        {
            using (var lSqliteTransaction = aOpenConnection.BeginTransaction())
                try
                {
                    using (SQLiteCommand lCreateProfileTable = new SQLiteCommand("CREATE TABLE IF NOT EXISTS Relationship_ExchangeID_OrderID (InternalOrderID integer PRIMARY KEY, ExchangeID integer)", aOpenConnection))
                        lCreateProfileTable.ExecuteNonQuery();

                    using (SQLiteCommand lInsertOldOrdersData = new SQLiteCommand($"INSERT INTO Relationship_ExchangeID_OrderID SELECT internalid, {(int)Exchanges.AvailableExchangesList.Bittrex} FROM ExchangeTx", aOpenConnection))
                        lInsertOldOrdersData.ExecuteNonQuery();

                    lSqliteTransaction.Commit();
                }
                catch (Exception ex)
                {
                    Universal.Log.Write(Universal.LogLevel.Error, $"Exchange SQLite exception when implementing version 104. Exception {ex}");
                    lSqliteTransaction.Rollback();
                    throw;
                }
        }

        private void Version103DoAddExchangeKeyValueTable(SQLiteConnection aOpenConnection)
        {
            using (var lSqliteTransaction = aOpenConnection.BeginTransaction())
                try
                {
                    // KeyValue Table is a lookup table of data with relationship with the profiles
                    using (SQLiteCommand lCreateKeyValueTable = new SQLiteCommand("CREATE TABLE IF NOT EXISTS KeyValue (keyname varchar(32) NOT NULL, profileid INT, datavalue varchar(500), PRIMARY KEY(keyname, profileid))", aOpenConnection))
                        lCreateKeyValueTable.ExecuteNonQuery();
                    lSqliteTransaction.Commit();
                }
                catch (Exception ex)
                {
                    Universal.Log.Write(Universal.LogLevel.Error, $"Exchange SQLite exception when implementing version 102. Exception {ex}");
                    lSqliteTransaction.Rollback();
                    throw;
                }
        }

        private void Version102DoAddExchangeProfileTable(SQLiteConnection aOpenConnection)
        {
            using (var lSqliteTransaction = aOpenConnection.BeginTransaction())
                try
                {
                    using (SQLiteCommand lCreateProfileTable = new SQLiteCommand("CREATE TABLE IF NOT EXISTS ExchangeProfile (ProfileID integer PRIMARY KEY, Name string, ExchangeID int)", aOpenConnection))
                        lCreateProfileTable.ExecuteNonQuery();
                    using (SQLiteCommand lCreateProfileTable = new SQLiteCommand("CREATE TABLE IF NOT EXISTS ExchangeList (ExchangeID integer PRIMARY KEY, Name string)", aOpenConnection))
                        lCreateProfileTable.ExecuteNonQuery();
                    lSqliteTransaction.Commit();
                }
                catch (Exception ex)
                {
                    Universal.Log.Write(Universal.LogLevel.Error, $"Exchange SQLite exception when implementing version 102. Exception {ex}");
                    lSqliteTransaction.Rollback();
                    throw;
                }
        }

        private void Version101DoAlterExchangeTx(SQLiteConnection aOpenConnection)
        {
            using (var lSqliteTransaction = aOpenConnection.BeginTransaction())
                try
                {
                    string lQuery = "PRAGMA table_info(ExchangeTx)";
                    SQLiteCommand lReadColumns = new SQLiteCommand(lQuery, aOpenConnection);
                    List<string> lColumns = new List<string>();
                    using (SQLiteDataReader rdr = lReadColumns.ExecuteReader())
                        while (rdr.Read())
                            lColumns.Add(rdr.GetString(1));
                    lReadColumns.Dispose();
                    if (!lColumns.Contains("Stop"))
                    {
                        lQuery = "ALTER TABLE ExchangeTx ADD Stop FLOAT DEFAULT 0 NOT NULL";
                        SQLiteCommand lAddColumn = new SQLiteCommand(lQuery, aOpenConnection);
                        lAddColumn.ExecuteNonQuery();
                        lAddColumn.Dispose();
                    }
                    if (!lColumns.Contains("ProfileID"))
                    {
                        lQuery = "ALTER TABLE ExchangeTx ADD ProfileID INT DEFAULT 10 NOT NULL";
                        SQLiteCommand lAddColumn = new SQLiteCommand(lQuery, aOpenConnection);
                        lAddColumn.ExecuteNonQuery();
                        lAddColumn.Dispose();
                    }
                    lSqliteTransaction.Commit();
                }
                catch (Exception ex)
                {
                    Universal.Log.Write(Universal.LogLevel.Error, $"Exchange SQLite exception when implementing version 101. Exception {ex}");
                    lSqliteTransaction.Rollback();
                    throw;
                }
        }

        private void WriteVersion(int aVersionID, int aVersion, string aVersionNote, string aUsername, string aEmail, SQLiteConnection aOpenConnection)
        {
            using (var lSqliteTransaction = aOpenConnection.BeginTransaction())
                try
                {
                    using (SQLiteCommand lWriteVersion = new SQLiteCommand("INSERT OR REPLACE INTO Version (ID, NVersion, username, email, note) VALUES (@ID, @Version, @Username, @Email, @Note)", aOpenConnection))
                    {
                        lWriteVersion.Parameters.Add(new SQLiteParameter("@ID", aVersionID));
                        lWriteVersion.Parameters.Add(new SQLiteParameter("@Version", aVersion));
                        lWriteVersion.Parameters.Add(new SQLiteParameter("@Username", aUsername));
                        lWriteVersion.Parameters.Add(new SQLiteParameter("@Email", aEmail));
                        lWriteVersion.Parameters.Add(new SQLiteParameter("@Note", aVersionNote));
                        lWriteVersion.ExecuteNonQuery();
                        lSqliteTransaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    Universal.Log.Write(Universal.LogLevel.Error, $"Exchange SQLite exception when implementing writing version. Exception {ex}");
                    lSqliteTransaction.Rollback();
                    throw;
                }
        }

        public void GetUserData(out string aUsername, out string aEmail)
        {
            if (!Initialized)
                throw new Exception("Save manager not initialized");
            aUsername = aEmail = null;
            using (SQLiteConnection lConnection = new SQLiteConnection(FConnectionString))
            {
                lConnection.Open();
                SQLiteCommand lCommand = new SQLiteCommand("SELECT username, email from Version limit 1", lConnection);
                using (SQLiteDataReader rdr = lCommand.ExecuteReader())
                    if (rdr.Read() && !rdr.IsDBNull(0))
                    {
                        aUsername = rdr.GetString(0);
                        aEmail = rdr.GetString(1);
                    }
                lConnection.Close();
            }
        }

        private int GetVersion()
        {
            if (!Initialized)
                throw new Exception("Save manager not initialized");
            using (SQLiteConnection lConnection = new SQLiteConnection(FConnectionString))
            {
                int lVersion = 100;
                lConnection.Open();
                SQLiteCommand lCommand = new SQLiteCommand("SELECT Max(NVersion) from Version", lConnection);
                using (SQLiteDataReader rdr = lCommand.ExecuteReader())
                    if (rdr.Read() && !rdr.IsDBNull(0))
                        lVersion = rdr.GetInt32(0);
                lConnection.Close();
                return lVersion;
            }
        }

        public void RemoveKeyValue(string aKey, int aProfileID)
        {
            if (aKey.Length > 32) throw new InvalidDataException("The Key argument is greater than 32");
            if (!Initialized && !FReadOnly) throw new Exception("Save manager not initialized");
            using (SQLiteConnection lConnection = new SQLiteConnection(FConnectionString))
            {
                lConnection.Open();
                using (var lSqliteTransaction = lConnection.BeginTransaction())
                    try
                    {
                        using (SQLiteCommand lSQLiteCommand = new SQLiteCommand($"DELETE FROM KeyValue WHERE keyname = '{aKey}' and profileid = {aProfileID}", lConnection))
                            lSQLiteCommand.ExecuteNonQuery();                        
                        lSqliteTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Universal.Log.Write(Universal.LogLevel.Error, $"Exchange SQLite exception when deleting key value. Key: {aKey},  ProfileID: {aProfileID}. Exception {ex}");
                        lSqliteTransaction.Rollback();
                        throw;
                    }
                    finally
                    {
                        lConnection.Close();
                    }
            }
        }

        public void WriteKeyValue(string aKey, string aValue, int aProfileID)
        {
            if (aKey.Length > 32) throw new InvalidDataException("The Key argument is greater than 32");
            if (aValue.Length > 500) throw new InvalidDataException("The Value argument is greater than 500");
            if (!Initialized && !FReadOnly) throw new Exception("Save manager not initialized");
            using (SQLiteConnection lConnection = new SQLiteConnection(FConnectionString))
            {
                lConnection.Open();
                using (var lSqliteTransaction = lConnection.BeginTransaction())
                    try
                    {
                        WriteKeyValue(lConnection, aKey, aValue, aProfileID);
                        lSqliteTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Universal.Log.Write(Universal.LogLevel.Error, $"Exchange SQLite exception when writing key value. Key: {aKey}, Value: {aValue}, ProfileID: {aProfileID}. Exception {ex}");
                        lSqliteTransaction.Rollback();
                        throw;
                    }
                    finally
                    {
                        lConnection.Close();
                    }
            }
        }

        private void WriteKeyValue(SQLiteConnection aOpenConnection, string aKey, string aValue, int aProfileID)
        {
            if (aKey.Length > 32) throw new InvalidDataException("The Key argument is greater than 32");
            if (aValue.Length > 500) throw new InvalidDataException("The Value argument is greater than 500");
            using (SQLiteCommand lSQLiteCommand = new SQLiteCommand("INSERT OR REPLACE INTO KeyValue (datavalue, keyname, profileid) VALUES (@Datavalue, @Keyname, @Profileid)", aOpenConnection))
            {
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("@Datavalue", aValue));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("@Keyname", aKey));
                lSQLiteCommand.Parameters.Add(new SQLiteParameter("@Profileid", aProfileID));
                lSQLiteCommand.ExecuteNonQuery();
            }
        }

        public void WriteKeyValues(Dictionary<string, string> aKeyValues, int aProfileID)
        {
            if (!Initialized && !FReadOnly) throw new Exception("Save manager not initialized");
            using (SQLiteConnection lConnection = new SQLiteConnection(FConnectionString))
            {
                lConnection.Open();
                using (var lSqliteTransaction = lConnection.BeginTransaction())
                    try
                    {
                        foreach (var lKeyValue in aKeyValues)
                            WriteKeyValue(lConnection, lKeyValue.Key, lKeyValue.Value, aProfileID);
                        lSqliteTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Universal.Log.Write(Universal.LogLevel.Error, $"Exchange SQLite exception when writing key multiple values. ProfileID: {aProfileID}. Exception {ex}");
                        lSqliteTransaction.Rollback();
                        throw;
                    }
                    finally
                    {
                        lConnection.Close();
                    }
            }
        }

        public string ReadKeyValue(string aKey, int aProfileID)
        {
            if (!Initialized)
                throw new Exception("Save manager not initialized");
            using (SQLiteConnection lConnection = new SQLiteConnection(FConnectionString))
            {
                lConnection.Open();
                string lQuery = $"SELECT datavalue FROM KeyValue where keyname = '{aKey}' and profileid = {aProfileID}";
                using (SQLiteCommand lSQLiteCommand = new SQLiteCommand(lQuery, lConnection))
                using (SQLiteDataReader lSQLiteDataReader = lSQLiteCommand.ExecuteReader())
                    if (lSQLiteDataReader.Read())
                        return lSQLiteDataReader.GetString(0);
                    else
                        return null;
            }
        }

        public void WriteExchange(string aID, string aExchangeName)
        {
            if (!Initialized && !FReadOnly)
                throw new Exception("Save manager not initialized");
            using (SQLiteConnection lConnection = new SQLiteConnection(FConnectionString))
            {
                lConnection.Open();
                SQLiteCommand lCommand = new SQLiteCommand("INSERT OR REPLACE INTO ExchangeList (ExchangeID, Name) VALUES (@ExchangeID, @Name)", lConnection);
                lCommand.Parameters.Add(new SQLiteParameter("@ExchangeID", aID));
                lCommand.Parameters.Add(new SQLiteParameter("@Name", aExchangeName));
                lCommand.ExecuteNonQuery();
                lConnection.Close();
            }
        }

        public IEnumerable<Tuple<int, string>> ReadExchanges()
        {
            if (!Initialized)
                throw new Exception("Save manager not initialized");
            var lResult = new List<Tuple<int, string>>();
            using (SQLiteConnection lConnection = new SQLiteConnection(FConnectionString))
            {
                lConnection.Open();
                SQLiteCommand lCommand = new SQLiteCommand("SELECT ExchangeID, Name from ExchangeList", lConnection);
                try
                {
                    using (SQLiteDataReader rdr = lCommand.ExecuteReader())
                        while (rdr.Read())
                            lResult.Add(new Tuple<int, string>(rdr.GetInt32(0), rdr.GetString(1)));
                    return lResult;
                }
                finally
                {
                    lConnection.Close();
                }
            }
        }

        public bool SaveProfile(PandoraExchangeProfile aProfile)
        {
            if (!Initialized && !FReadOnly)
                throw new Exception("Save manager not initialized");
            using (SQLiteConnection lConnection = new SQLiteConnection(FConnectionString))
            {
                lConnection.Open();
                SQLiteCommand lCommand = new SQLiteCommand("INSERT OR REPLACE INTO ExchangeProfile (ProfileID, Name, ExchangeID) VALUES (@ProfileID, @Name, @ExchangeID)", lConnection);
                lCommand.Parameters.Add(new SQLiteParameter("@ProfileID", aProfile.ProfileID));
                lCommand.Parameters.Add(new SQLiteParameter("@Name", aProfile.Name));
                lCommand.Parameters.Add(new SQLiteParameter("@ExchangeID", (int)aProfile.ExchangeID));
                try
                {
                    lCommand.ExecuteNonQuery();
                    return true;
                }
                catch
                {
                    return false;
                }
                finally
                {
                    lConnection.Close();
                }
            }
        }

        public PandoraExchangeProfile[] LoadProfiles()
        {
            List<PandoraExchangeProfile> lProfiles = new List<PandoraExchangeProfile>();
            if (!Initialized)
                throw new Exception("Save manager not initialized");
            using (SQLiteConnection lConnection = new SQLiteConnection(FConnectionString))
            {
                lConnection.Open();
                SQLiteCommand lCommand = new SQLiteCommand("SELECT ProfileID, Name, ExchangeID from ExchangeProfile", lConnection);
                try
                {
                    using (SQLiteDataReader rdr = lCommand.ExecuteReader())
                        while (rdr.Read())
                            lProfiles.Add(new PandoraExchangeProfile
                            {
                                ProfileID = rdr.GetInt32(0),
                                Name = rdr.GetString(1),
                                ExchangeID = (uint)rdr.GetInt32(2)
                            });
                }
                finally
                {
                    lConnection.Close();
                }
            }
            return lProfiles.ToArray();
        }

        public void DeleteProfile(int aProfileID)
        {
            if (!Initialized && !FReadOnly)
                throw new Exception("Save manager not initialized");
            using (SQLiteConnection lConnection = new SQLiteConnection(FConnectionString))
            {
                lConnection.Open();
                SQLiteCommand lCommand = new SQLiteCommand("DELETE from ExchangeProfile where ProfileID = @ProfileID;", lConnection);
                try
                {
                    lCommand.ExecuteNonQuery();
                }
                finally
                {
                    lConnection.Close();
                }
            }
        }

        public long? WriteOrder(UserTradeOrder aMarketTransaction)
        {
            if (!Initialized && !FReadOnly)
                throw new Exception("Save manager not initialized");
            using (SQLiteConnection lConnection = new SQLiteConnection(FConnectionString))
            {
                SQLiteTransaction lTransaction;
                lConnection.Open();
                lTransaction = lConnection.BeginTransaction();
                try
                {                    
                    SQLiteCommand lCommand1 = new SQLiteCommand("INSERT OR REPLACE INTO ExchangeTx (ID,Market, Quantity, OpenTime, Rate, Stop, Completed, Cancelled, Status, CoinTXID, BaseTicker, Name, ProfileID) VALUES (@Id, @Market, @Quantity, @Opentime, @Rate, @Stop, @Completed, @Cancelled, @Status, @CoinTxID, @BaseTicker, @Name, @ProfileID); " +
                        "SELECT InternalID FROM ExchangeTx where CoinTXID = @CoinTxID;", lConnection);
                    lCommand1.Parameters.Add(new SQLiteParameter("@Id", aMarketTransaction.ID));
                    lCommand1.Parameters.Add(new SQLiteParameter("@Market", aMarketTransaction.ExchangeMarketName));
                    lCommand1.Parameters.Add(new SQLiteParameter("@Quantity", aMarketTransaction.SentQuantity));
                    lCommand1.Parameters.Add(new SQLiteParameter("@Opentime", aMarketTransaction.OpenTime));
                    lCommand1.Parameters.Add(new SQLiteParameter("@Rate", aMarketTransaction.Rate));
                    lCommand1.Parameters.Add(new SQLiteParameter("@Stop", aMarketTransaction.StopPrice));
                    lCommand1.Parameters.Add(new SQLiteParameter("@Completed", aMarketTransaction.Completed));
                    lCommand1.Parameters.Add(new SQLiteParameter("@Cancelled", aMarketTransaction.Cancelled));
                    lCommand1.Parameters.Add(new SQLiteParameter("@Status", aMarketTransaction.Status));
                    lCommand1.Parameters.Add(new SQLiteParameter("@CoinTxID", aMarketTransaction.CoinTxID));
                    lCommand1.Parameters.Add(new SQLiteParameter("@BaseTicker", aMarketTransaction.BaseCurrency.Ticker));
                    lCommand1.Parameters.Add(new SQLiteParameter("@Name", aMarketTransaction.Name));
                    lCommand1.Parameters.Add(new SQLiteParameter("@ProfileID", aMarketTransaction.ProfileID));
                    var lId = lCommand1.ExecuteScalar() as long?;
                    if (lId == null)
                        throw new Exception("Failed to retrieve order id. Operation failed when writing order");

                    SQLiteCommand lCommand2 = new SQLiteCommand("INSERT OR REPLACE INTO Relationship_ExchangeID_OrderID (InternalOrderID, ExchangeID) Values (@InternalID, @ExchangeID)",lConnection);
                    lCommand2.Parameters.Add(new SQLiteParameter("@InternalID", lId));
                    lCommand2.Parameters.Add(new SQLiteParameter("@ExchangeID", aMarketTransaction.PandoraExchangeID));
                    lCommand2.ExecuteNonQuery();

                    lTransaction.Commit();
                    return lId;
                }
                catch (Exception ex)
                {
                    lTransaction.Rollback();
                    Universal.Log.Write(Universal.LogLevel.Error, $"Error writing exchange order from db. Exception details: {ex}");
                    return null;                    
                }
                finally
                {
                    lConnection.Close();
                }
            }
        }

        public bool UpdateOrder(UserTradeOrder aMarketTransaction, OrderStatus aStatus)
        {
            if (!Initialized && !FReadOnly)
                throw new Exception("Save manager not initialized");
            using (SQLiteConnection lConnection = new SQLiteConnection(FConnectionString))
            {
                lConnection.Open();
                var lTransaction = lConnection.BeginTransaction();
                SQLiteCommand lCommand1 = new SQLiteCommand("Update ExchangeTx SET ID = @Id, Market = @Market, Quantity = @Quantity, OpenTime = @Opentime, Rate = @Rate, Stop = @Stop, Completed = @Completed, Cancelled = @Cancelled, Status = @Status, CoinTXID = @CoinTxID, BaseTicker = @BaseTicker, Name = @Name, ProfileID = @ProfileID WHERE InternalID = @InternalID", lConnection);
                lCommand1.Parameters.Add(new SQLiteParameter("@InternalID", aMarketTransaction.InternalID));
                lCommand1.Parameters.Add(new SQLiteParameter("@Id", aMarketTransaction.ID));
                lCommand1.Parameters.Add(new SQLiteParameter("@Market", aMarketTransaction.ExchangeMarketName));
                lCommand1.Parameters.Add(new SQLiteParameter("@Quantity", aMarketTransaction.SentQuantity));
                lCommand1.Parameters.Add(new SQLiteParameter("@Opentime", aMarketTransaction.OpenTime));
                lCommand1.Parameters.Add(new SQLiteParameter("@Rate", aMarketTransaction.Rate));
                lCommand1.Parameters.Add(new SQLiteParameter("@Stop", aMarketTransaction.StopPrice));
                lCommand1.Parameters.Add(new SQLiteParameter("@Completed", aMarketTransaction.Completed));
                lCommand1.Parameters.Add(new SQLiteParameter("@Cancelled", aMarketTransaction.Cancelled));
                lCommand1.Parameters.Add(new SQLiteParameter("@Status", (int)aStatus));
                lCommand1.Parameters.Add(new SQLiteParameter("@CoinTxID", aMarketTransaction.CoinTxID));
                lCommand1.Parameters.Add(new SQLiteParameter("@BaseTicker", aMarketTransaction.BaseCurrency.Ticker));
                lCommand1.Parameters.Add(new SQLiteParameter("@Name", aMarketTransaction.Name));
                lCommand1.Parameters.Add(new SQLiteParameter("@ProfileID", aMarketTransaction.ProfileID));

                SQLiteCommand lCommand2 = new SQLiteCommand("Update Relationship_ExchangeID_OrderID SET ExchangeID = @ExchangeID WHERE InternalOrderID = @InternalID", lConnection);
                lCommand2.Parameters.Add(new SQLiteParameter("@InternalID", aMarketTransaction.InternalID));
                lCommand2.Parameters.Add(new SQLiteParameter("@ExchangeID", aMarketTransaction.PandoraExchangeID));

                try
                {
                    lCommand1.ExecuteNonQuery();
                    lCommand2.ExecuteNonQuery();
                    lTransaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    lTransaction.Rollback();
                    Universal.Log.Write(Universal.LogLevel.Error, $"Error updating exchange order from db. Exception {ex}");
                    return false;
                }
                finally
                {
                    lConnection.Close();
                }
            }
        }

        public bool WriteOrderLog(int aOrderID, string aMessage, OrderMessage.OrderMessageLevel aMessageLevel)
        {
            if (!Initialized && !FReadOnly)
                throw new Exception("Save manager not initialized");
            using (SQLiteConnection lConnection = new SQLiteConnection(FConnectionString))
            {
                lConnection.Open();
                SQLiteCommand lCommand = new SQLiteCommand("INSERT OR REPLACE INTO ExchangeLog (TransactionID, TransactionTime, Message, MessageLevel) VALUES (@Id, @Time, @Message, @MessageLevel)", lConnection);
                lCommand.Parameters.Add(new SQLiteParameter("@Id", aOrderID));
                lCommand.Parameters.Add(new SQLiteParameter("@Time", DateTime.UtcNow));
                lCommand.Parameters.Add(new SQLiteParameter("@Message", aMessage));
                lCommand.Parameters.Add(new SQLiteParameter("@MessageLevel", aMessageLevel));

                try
                {
                    lCommand.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex)
                {
                    Universal.Log.Write(Universal.LogLevel.Error, $"Error writing order logs from db. Exception {ex}");
                    return false;
                }
                finally
                {
                    lConnection.Close();
                }
            }
        }

        public bool ReadOrderLogs(int aOrderID, out List<OrderMessage> aMessages)
        {
            if (!Initialized)
                throw new Exception("Save manager not initialized");
            string lQry = "SELECT ID, TransactionTime, Message, MessageLevel FROM ExchangeLog where TransactionID = '" + aOrderID + "'";
            aMessages = null;
            List<OrderMessage> lListMessages = new List<OrderMessage>();

            using (SQLiteConnection lConnection = new SQLiteConnection(FConnectionString))
            using (SQLiteCommand command = new SQLiteCommand(lQry, lConnection))
            {
                try
                {
                    lConnection.Open();

                    using (SQLiteDataReader rdr = command.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            lListMessages.Add(new OrderMessage
                            {
                                ID = rdr.GetInt32(0),
                                Time = rdr.GetDateTime(1),
                                Message = rdr.GetString(2),
                                Level = (OrderMessage.OrderMessageLevel)rdr.GetInt32(3)
                            });
                        }
                    }
                    aMessages = lListMessages;
                    return true;
                }
                catch (Exception ex)
                {
                    Universal.Log.Write(Universal.LogLevel.Error, $"Error reading order logs from db. Exception {ex}");
                    return false;
                }
                finally
                {
                    lConnection.Close();
                }
            }
        }

        public bool ReadOrders(out UserTradeOrder[] aMarketOrders, string aBaseTicker = null, int aExchangeID = -1)
        {
            if (!Initialized)
                throw new Exception("Save manager not initialized");
            string lQry = "SELECT A.InternalID, A.ID, A.Market, A.Quantity, A.OpenTime, A.Rate, A.Completed, A.Cancelled, A.Status, A.CoinTxID, A.BaseTicker, A.Name, A.Stop, A.ProfileID, B.ExchangeID FROM ExchangeTx A " +
                "inner join Relationship_ExchangeID_OrderID B ON A.InternalID = B.InternalOrderID WHERE 1";
            if (aBaseTicker != null)
                lQry += $" AND A.BaseTicker = '{aBaseTicker}' ";
            if(aExchangeID >0)
                lQry += $" AND B.ExchangeID = {aExchangeID}";


            aMarketOrders = null;
            List<UserTradeOrder> lListOrders = new List<UserTradeOrder>();

            using (SQLiteConnection lConnection = new SQLiteConnection(FConnectionString))
            using (SQLiteCommand command = new SQLiteCommand(lQry, lConnection))
            {
                try
                {
                    lConnection.Open();

                    using (SQLiteDataReader rdr = command.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            decimal lRate = rdr.GetDecimal(5);
                            decimal lStop = rdr.GetDecimal(12);

                            UserTradeOrder lOrder = new UserTradeOrder
                            {
                                InternalID = rdr.GetInt32(0),
                                ID = rdr[1] as string,
                                ExchangeMarketName = rdr.GetString(2),
                                SentQuantity = rdr.GetDecimal(3),
                                OpenTime = rdr.GetDateTime(4),
                                Rate = lRate,
                                Completed = rdr.GetBoolean(6),
                                Cancelled = rdr.GetBoolean(7),
                                Status = (OrderStatus)rdr.GetInt32(8),
                                CoinTxID = rdr.GetString(9),
                                Name = rdr.GetString(11),
                                StopPrice = lStop == 0 ? lRate : lStop,
                                ProfileID = rdr.GetInt32(13),
                                PandoraExchangeID = rdr.GetInt32(14)
                            };
                            lOrder.BaseCurrency.Ticker = rdr.GetString(10);
                            lListOrders.Add(lOrder);
                        }
                    }
                    aMarketOrders = lListOrders.ToArray();
                    return true;
                }
                catch (Exception ex)
                {
                    Universal.Log.Write(Universal.LogLevel.Error, $"Error reading enchange orders from db. Exception {ex}");
                    return false;
                }
                finally
                {
                    lConnection.Close();
                }
            }
        }
        public string CreateDBFileCopy()
        {
            using (SQLiteConnection lConnection = new SQLiteConnection(FConnectionString))
            {
                lConnection.Open();
                string lQuery = "BEGIN IMMEDIATE;"; //THIS QUERY FORCES ALL WRITING OPERATIONS TO BE PAUSED
                using (SQLiteCommand lSQLiteCommand = new SQLiteCommand(lQuery, lConnection))
                    lSQLiteCommand.ExecuteNonQuery();
                string lCopyFileName = string.Concat(this.DBFilePath, "_copy");
                if (File.Exists(lCopyFileName))
                    File.Delete(lCopyFileName);
                File.Copy(this.DBFilePath, lCopyFileName);
                lQuery = "ROLLBACK;"; //THIS RELEASES THE BEGIN IMMEDIATE LOCK OF WRITING
                using (SQLiteCommand lSQLiteCommand = new SQLiteCommand(lQuery, lConnection))
                    lSQLiteCommand.ExecuteNonQuery();
                lConnection.Close();
                return lCopyFileName;
            }
        }

        public void Dispose()
        {
            FConnectionString = null;
        }
    }
}