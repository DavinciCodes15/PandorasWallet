//   Copyright 2017-2019 Davinci Codes
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Also use the software for non-commercial purposes.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
#if DEBUG
#define TESTING
#endif

using Pandora.Client.ClientLib;
using Pandora.Client.Crypto.Currencies;
using Pandora.Client.Universal;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Pandora.Client.PandorasWallet.ServerAccess
{
    public partial class PandorasCache
    {
        public class DBManager : IDisposable

        {
            private SQLiteConnection FDBConnection;
            private List<string> TableNames;

            private Dictionary<string, TimeSpan> FLifeTimes = new Dictionary<string, TimeSpan>();

            public string FileName { get; private set; }

            public DBManager(string aDataPath, string aSQLiteFile)
            {
                try
                {
                    FileName = Path.Combine(aDataPath, aSQLiteFile + ".sqlite");

                    FDBConnection = new SQLiteConnection("Data Source=" + FileName + ";PRAGMA journal_mode=WAL;");
                    FDBConnection.Open();

                    CreateTables();
                    TableNames = new List<string>();
                    TableNames.AddRange(GetTableNames());
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Retrieves a list of monitored address accounts from specified files, if exists
            /// </summary>
            /// <param name="aFilename">SqlLite File name with or without extention</param>
            /// <returns>Currency account array of users coin data</returns>
            /// <exception cref="Pandora.Client.PandorasWallet.ClientExceptions.CacheDBException">Throw when there is an SQLite problem when perfoming query</exception>
            public static CurrencyAccount[] GetUserAddressesFromFile(string aDataPath, string aSQLiteFilename)
            {
                List<CurrencyAccount> lResult = new List<CurrencyAccount>();
                if (!aSQLiteFilename.Contains(".sqlite"))
                    aSQLiteFilename = string.Concat(aSQLiteFilename, ".sqlite");
                string lFilePath = Path.Combine(aDataPath, aSQLiteFilename);
                using (SQLiteConnection lConnection = new SQLiteConnection(string.Format("Data Source={0};PRAGMA journal_mode=WAL;", lFilePath)))
                {
                    lConnection.Open();
                    if (ReadCurrencyAccount(out List<CurrencyAccount> lCurrencyAccountsLists, lConnection))
                        lResult.AddRange(lCurrencyAccountsLists);
                    else throw new Pandora.Client.PandorasWallet.ClientExceptions.CacheDBException(string.Format("Error retrieving User addresses from file. Connection string: {0}", lConnection.ConnectionString));
                    lConnection.Close();
                }

                return lResult.ToArray();
            }

            private string[] GetTableNames()
            {
                List<string> lTableNames = new List<string>();

                try
                {
                    using (SQLiteCommand command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type = 'table' and name not like 'Exchange%' and name not like 'sqlite%' ORDER BY 1", FDBConnection))
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

            private const int VERSION = 10002;

            private void CreateTables()
            {
                string[] lTables = GetTableNames();

                if (lTables.Any())
                {
                    if (!lTables.Contains("Version"))
                    {
                        TableNames = new List<string>(lTables);
                        ClearAll(); // clears data from all tables
                    }
                    else if (GetVersion() >= VERSION)
                    {
                        return;
                    }
                }
                SQLiteCommand CreateMonitoredAccounts = new SQLiteCommand("CREATE TABLE IF NOT EXISTS MonitoredAccounts (id BIGINT, currencyid INT, address VARCHAR(100),PRIMARY KEY (id,currencyId))", FDBConnection);
                SQLiteCommand CreateCurrencyData = new SQLiteCommand("CREATE TABLE IF NOT EXISTS Currencies2 (id INT PRIMARY KEY, name VARCHAR(100), ticker VARCHAR(50), precision INT, MinConfirmations INT, livedate DATETIME, Icon BLOB, IconSize int, FeePerKb int, ChainParams BLOB, status VARCHAR(50))", FDBConnection);
                SQLiteCommand CreateCurrencyStatusData = new SQLiteCommand("CREATE TABLE IF NOT EXISTS CurrenciesStatus (id BIGINT, currencyid INT PRIMARY KEY, statustime DATETIME, status VARCHAR(50), extinfo VARCHAR(255), blockheight INT)", FDBConnection);
                SQLiteCommand CreateTxTable = new SQLiteCommand("CREATE TABLE IF NOT EXISTS TxTable (internalid BIGINT, currencyid BIGINT, id VARCHAR(100), dattime DATETIME, block BIGINT, TxFee BIGINT, Valid BOOLEAN, PRIMARY KEY(internalid, currencyid))", FDBConnection);
                SQLiteCommand CreateInputsTxTable = new SQLiteCommand("CREATE TABLE IF NOT EXISTS TxIn (internalid BIGINT, id BIGINT, address varchar(100), ammount BIGINT, PRIMARY KEY (internalid,id,address,ammount))", FDBConnection);
                SQLiteCommand CreateOutputsTxTable = new SQLiteCommand("CREATE TABLE IF NOT EXISTS TxOut (internalid BIGINT, id BIGINT, address varchar(100), ammount BIGINT, PRIMARY KEY (internalid,id,address,ammount))", FDBConnection);
                SQLiteCommand CreateExtTxTable = new SQLiteCommand("CREATE TABLE IF NOT EXISTS TxExt (internalid BIGINT PRIMARY KEY, extdata varchar(100))", FDBConnection);
                SQLiteCommand CreateDataRetrievalTable = new SQLiteCommand("CREATE TABLE IF NOT EXISTS DataReferences (type VARCHAR(100) PRIMARY KEY, lastdataretrieval DATETIME INTEGER)", FDBConnection);
                SQLiteCommand CreateVersionTable = new SQLiteCommand("CREATE TABLE IF NOT EXISTS Version (NVersion BIGINT PRIMARY KEY)", FDBConnection);
                SQLiteCommand WriteVersion = new SQLiteCommand("DELETE FROM VERSION ; INSERT OR REPLACE INTO Version (NVersion) VALUES (@Version)", FDBConnection);
                WriteVersion.Parameters.Add(new SQLiteParameter("Version", VERSION));

                using (SQLiteTransaction lTransaction = FDBConnection.BeginTransaction())
                {
                    try
                    {
                        CreateMonitoredAccounts.ExecuteNonQuery();
                        CreateCurrencyData.ExecuteNonQuery();
                        CreateCurrencyStatusData.ExecuteNonQuery();
                        CreateTxTable.ExecuteNonQuery();
                        CreateInputsTxTable.ExecuteNonQuery();
                        CreateOutputsTxTable.ExecuteNonQuery();
                        CreateExtTxTable.ExecuteNonQuery();
                        CreateDataRetrievalTable.ExecuteNonQuery();
                        CreateVersionTable.ExecuteNonQuery();
                        WriteVersion.ExecuteNonQuery();

                        lTransaction.Commit();

                        CreateMonitoredAccounts.Dispose();
                        CreateCurrencyData.Dispose();
                        CreateCurrencyStatusData.Dispose();
                        CreateTxTable.Dispose();
                        CreateInputsTxTable.Dispose();
                        CreateOutputsTxTable.Dispose();
                        CreateExtTxTable.Dispose();
                        CreateDataRetrievalTable.Dispose();
                        CreateVersionTable.Dispose();
                        WriteVersion.Dispose();
                    }
                    catch
                    {
                        lTransaction.Rollback();
                        throw;
                    }
                }
            }

            private int GetVersion()
            {
                string lQuery = "SELECT NVersion FROM Version";
                using (SQLiteCommand lSQLiteCommand = new SQLiteCommand(lQuery, FDBConnection))
                using (SQLiteDataReader lReader = lSQLiteCommand.ExecuteReader())
                    if (lReader.Read())
                        return Convert.ToInt32(lReader.GetInt32(0));
                    else
                        return 0;
            }

            public Dictionary<string, long[]> GetSavedCheckpoints()
            {
                Dictionary<string, long[]> lCheckpoints = new Dictionary<string, long[]>();
                foreach (string lTableName in TableNames.Where(table => table != "Version"))
                {
                    switch (lTableName)
                    {
                        case "MonitoredAccounts":

                            string qry1 = "SELECT Max(id), currencyid FROM " + lTableName + " GROUP BY currencyId";
                            List<long> LList = new List<long>();
                            List<long> LIndex = new List<long>();
                            using (SQLiteCommand command = new SQLiteCommand(qry1, FDBConnection))
                            {
                                using (SQLiteDataReader rdr = command.ExecuteReader())
                                {
                                    while (rdr.Read())
                                    {
                                        LList.Add(rdr.GetInt64(0));
                                        LIndex.Add(rdr.GetInt64(1));
                                    }
                                }
                            }

                            lCheckpoints.Add(lTableName, LList.ToArray());
                            lCheckpoints.Add(lTableName + "Index", LIndex.ToArray());
                            break;

                        case "TxTable":

                            string qry3alt = "SELECT Min(internalid), currencyid FROM " + lTableName + " WHERE block == 0 GROUP BY currencyId";

                            List<Tuple<long, long>> lListResults1 = new List<Tuple<long, long>>();

                            using (SQLiteCommand command = new SQLiteCommand(qry3alt, FDBConnection))
                            {
                                using (SQLiteDataReader rdr = command.ExecuteReader())
                                {
                                    while (rdr.Read())
                                    {
                                        lListResults1.Add(new Tuple<long, long>(rdr.GetInt64(0) - 1, rdr.GetInt64(1)));
                                    }
                                }
                            }

                            string qry3 = "SELECT Max(internalid), currencyid FROM " + lTableName + " WHERE block != 0 GROUP BY currencyId";

                            List<Tuple<long, long>> lListResults2 = new List<Tuple<long, long>>();

                            using (SQLiteCommand command = new SQLiteCommand(qry3, FDBConnection))
                            {
                                using (SQLiteDataReader rdr = command.ExecuteReader())
                                {
                                    while (rdr.Read())
                                    {
                                        lListResults2.Add(new Tuple<long, long>(rdr.GetInt64(0), rdr.GetInt64(1)));
                                    }
                                }
                            }

                            List<Tuple<long, long>> lListFinalResult = lListResults2.Where(x => !lListResults2.Exists(y => y.Item1 == x.Item1)).ToList();
                            lListFinalResult.AddRange(lListResults1);

                            lCheckpoints.Add(lTableName, lListFinalResult.Select(x => x.Item1).ToArray());
                            lCheckpoints.Add(lTableName + "Index", lListFinalResult.Select(x => x.Item2).ToArray());
                            break;

                        case "CurrenciesStatus":
                            string qry4 = "SELECT Max(id), currencyid FROM " + lTableName + " GROUP BY currencyId";
                            List<long> LList3 = new List<long>();
                            List<long> LIndex3 = new List<long>();
                            using (SQLiteCommand command = new SQLiteCommand(qry4, FDBConnection))
                            {
                                using (SQLiteDataReader rdr = command.ExecuteReader())
                                {
                                    while (rdr.Read())
                                    {
                                        LList3.Add(rdr.GetInt64(0));
                                        LIndex3.Add(rdr.GetInt64(1));
                                    }
                                }
                            }

                            lCheckpoints.Add(lTableName, LList3.ToArray());
                            lCheckpoints.Add(lTableName + "Index", LIndex3.ToArray());
                            break;

                        case "Currencies2":

                            lCheckpoints.Add(lTableName, new long[1]);
                            string qry5 = "SELECT Id FROM " + lTableName;
                            List<long> lCoinIds = new List<long>();
                            using (SQLiteCommand command = new SQLiteCommand(qry5, FDBConnection))
                            {
                                using (SQLiteDataReader rdr = command.ExecuteReader())
                                {
                                    while (rdr.Read())
                                    {
                                        lCoinIds.Add(rdr.GetInt32(0));
                                    }
                                }
                            }

                            lCheckpoints[lTableName] = lCoinIds.ToArray();

                            break;

                        case "DataReferences":
                        case "TxIn":
                        case "TxOut":
                        case "TxExt":
                        case "Tx":
                        case "Currencies":
                            break;

                        default:

                            lCheckpoints.Add(lTableName, new long[1]);
                            string qry2 = "SELECT COUNT(id), Max(id) FROM " + lTableName + " limit 1";

                            using (SQLiteCommand command = new SQLiteCommand(qry2, FDBConnection))
                            {
                                using (SQLiteDataReader rdr = command.ExecuteReader())
                                {
                                    while (rdr.Read())
                                    {
                                        if (rdr.GetInt32(0) > 0)
                                        {
                                            lCheckpoints[lTableName][0] = rdr.GetInt32(1);
                                        }
                                        else
                                        {
                                            lCheckpoints[lTableName][0] = 0;
                                        }
                                    }
                                }
                            }

                            break;
                    }
                }

                return lCheckpoints;
            }

            public bool Write(CurrencyAccount aCurr)
            {
                using (SQLiteCommand WriteCurrAcc = new SQLiteCommand("INSERT OR REPLACE INTO MonitoredAccounts (id, currencyid, address) VALUES (@Id, @currencyId, @address)", FDBConnection))
                {
                    WriteCurrAcc.Parameters.Add(new SQLiteParameter("Id", aCurr.Id));
                    WriteCurrAcc.Parameters.Add(new SQLiteParameter("currencyId", aCurr.CurrencyId));
                    WriteCurrAcc.Parameters.Add(new SQLiteParameter("address", aCurr.Address));

                    try
                    {
                        WriteCurrAcc.ExecuteNonQuery();
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            public bool Write(List<CurrencyAccount> aCurrList)
            {
                if (!aCurrList.Any())
                {
                    return false;
                }
                using (SQLiteTransaction lTransaction = FDBConnection.BeginTransaction())
                {
                    foreach (CurrencyAccount it in aCurrList)
                    {
                        if (!Write(it))
                        {
                            lTransaction.Rollback();
                            return false;
                        }
                    }
                    InsertNewRetrievalTime("MonitoredAccounts");
                    lTransaction.Commit();
                }
                return true;
            }

            public bool Write(CurrencyItem aCurrItem)
            {
                using (SQLiteCommand WriteCurrItem = new SQLiteCommand("INSERT OR REPLACE INTO Currencies2 (id, name, ticker, precision, MinConfirmations,livedate,Icon,IconSize,FeePerKb, ChainParams, Status) VALUES (@id,@name,@ticker,@precision,@MinConfirmations,@LiveDate,@Icon,@IconSize,@FeePerKb, @ChainParams,@Status)", FDBConnection))
                {
                    WriteCurrItem.Parameters.Add(new SQLiteParameter("id", aCurrItem.Id));
                    WriteCurrItem.Parameters.Add(new SQLiteParameter("name", aCurrItem.Name));
                    WriteCurrItem.Parameters.Add(new SQLiteParameter("ticker", aCurrItem.Ticker));
                    WriteCurrItem.Parameters.Add(new SQLiteParameter("precision", aCurrItem.Precision));
                    WriteCurrItem.Parameters.Add(new SQLiteParameter("MinConfirmations", aCurrItem.MinConfirmations));
                    WriteCurrItem.Parameters.Add(new SQLiteParameter("LiveDate", aCurrItem.LiveDate));
                    WriteCurrItem.Parameters.Add(new SQLiteParameter("Icon", aCurrItem.Icon));
                    WriteCurrItem.Parameters.Add(new SQLiteParameter("IconSize", aCurrItem.Icon.Count()));
                    WriteCurrItem.Parameters.Add(new SQLiteParameter("FeePerKb", aCurrItem.FeePerKb));
                    WriteCurrItem.Parameters.Add(new SQLiteParameter("Status", aCurrItem.CurrentStatus));

                    MyCacheChainParams lChainParamsCopy = new MyCacheChainParams();

                    using (MemoryStream ms = new MemoryStream())
                    {
                        BinaryFormatter lBf = new BinaryFormatter();

                        aCurrItem.ChainParamaters.CopyTo(lChainParamsCopy);

                        lBf.Serialize(ms, lChainParamsCopy);

                        WriteCurrItem.Parameters.Add(new SQLiteParameter("ChainParams", Convert.ToBase64String(ms.ToArray())));
                    }

                    try
                    {
                        WriteCurrItem.ExecuteNonQuery();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Universal.Log.Write(Universal.LogLevel.Error, "Write CurrencyItem cache exception: " + ex.Message + " on " + ex.Source);
                        return false;
                    }
                }
            }

            public bool Write(List<CurrencyItem> aCurrItemList)
            {
                if (!aCurrItemList.Any())
                {
                    return false;
                }

                using (SQLiteTransaction lTransaction = FDBConnection.BeginTransaction())
                {
                    foreach (CurrencyItem it in aCurrItemList)
                    {
                        if (!Write(it))
                        {
                            lTransaction.Rollback();
                            return false;
                        }
                    }
                    InsertNewRetrievalTime("CurrencyItem");
                    lTransaction.Commit();
                }
                return true;
            }

            public bool Write(CurrencyStatusItem aCurrStatusItem)
            {
                using (SQLiteCommand WriteCurrStatusItem = new SQLiteCommand("INSERT OR REPLACE INTO CurrenciesStatus (id, currencyid, statustime, status, extinfo, blockheight) VALUES (@StatusId,@currencyid,@statustime,@status,@extinfo,@blockheight)", FDBConnection))
                {
                    WriteCurrStatusItem.Parameters.Add(new SQLiteParameter("StatusId", aCurrStatusItem.StatusId));
                    WriteCurrStatusItem.Parameters.Add(new SQLiteParameter("currencyid", aCurrStatusItem.CurrencyId));
                    WriteCurrStatusItem.Parameters.Add(new SQLiteParameter("statustime", aCurrStatusItem.StatusTime));
                    WriteCurrStatusItem.Parameters.Add(new SQLiteParameter("status", aCurrStatusItem.Status.ToString()));
                    WriteCurrStatusItem.Parameters.Add(new SQLiteParameter("extinfo", aCurrStatusItem.ExtendedInfo));
                    WriteCurrStatusItem.Parameters.Add(new SQLiteParameter("blockheight", aCurrStatusItem.BlockHeight));

                    try
                    {
                        WriteCurrStatusItem.ExecuteNonQuery();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Universal.Log.Write(Universal.LogLevel.Error, "Write Currency Status Cache exception: " + ex.Message + " on " + ex.Source);
                        return false;
                    }
                }
            }

            public bool Write(List<CurrencyStatusItem> aCurrStatusItemList)
            {
                if (!aCurrStatusItemList.Any())
                {
                    return false;
                }

                using (SQLiteTransaction lTransaction = FDBConnection.BeginTransaction())
                {
                    foreach (CurrencyStatusItem it in aCurrStatusItemList)
                    {
                        if (!Write(it))
                        {
                            lTransaction.Rollback();
                            return false;
                        }
                    }
                    InsertNewRetrievalTime("CurrencyStatus");
                    lTransaction.Commit();
                }

                return true;
            }

            public bool WriteTransactionRecord(TransactionRecord aTxRecord, long aCurrencyId)
            {
                using (SQLiteCommand WriteTxRecord = new SQLiteCommand("INSERT OR REPLACE INTO TxTable (internalid, currencyid, id, dattime, block, TxFee, Valid) VALUES (@internalid, @currencyid, @id, @dattime, @block, @TxFee, @Valid)", FDBConnection))
                {
                    WriteTxRecord.Parameters.Add(new SQLiteParameter("internalid", aTxRecord.TransactionRecordId));
                    WriteTxRecord.Parameters.Add(new SQLiteParameter("currencyid", aCurrencyId));
                    WriteTxRecord.Parameters.Add(new SQLiteParameter("id", aTxRecord.TxId));
                    WriteTxRecord.Parameters.Add(new SQLiteParameter("dattime", aTxRecord.TxDate));
                    WriteTxRecord.Parameters.Add(new SQLiteParameter("block", aTxRecord.Block));
                    WriteTxRecord.Parameters.Add(new SQLiteParameter("TxFee", aTxRecord.TxFee));
                    WriteTxRecord.Parameters.Add(new SQLiteParameter("Valid", aTxRecord.Valid));

                    try
                    {
                        WriteTxRecord.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Universal.Log.Write(Universal.LogLevel.Error, "Write TX Cache exception: " + ex.Message + " on " + ex.Source);
                        return false;
                    }
                }

                foreach (TransactionUnit it in aTxRecord.Inputs)
                {
                    using (SQLiteCommand WriteTxIn = new SQLiteCommand("INSERT OR REPLACE INTO TxIn (internalid, id, address, ammount) VALUES (@internalid, @id, @address, @ammount)", FDBConnection))
                    {
                        WriteTxIn.Parameters.Add(new SQLiteParameter("internalid", aTxRecord.TransactionRecordId));
                        WriteTxIn.Parameters.Add(new SQLiteParameter("id", it.Id));
                        WriteTxIn.Parameters.Add(new SQLiteParameter("address", it.Address));
                        WriteTxIn.Parameters.Add(new SQLiteParameter("ammount", it.Amount));

                        try
                        {
                            WriteTxIn.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            Universal.Log.Write(Universal.LogLevel.Error, "Write TX Cache exception: " + ex.Message + " on " + ex.Source);
                            return false;
                        }
                    }
                }

                foreach (TransactionUnit it in aTxRecord.Outputs)
                {
                    using (SQLiteCommand WriteTxOut = new SQLiteCommand("INSERT OR REPLACE INTO TxOut (internalid, id, address, ammount) VALUES (@internalid, @id, @address, @ammount)", FDBConnection))
                    {
                        WriteTxOut.Parameters.Add(new SQLiteParameter("internalid", aTxRecord.TransactionRecordId));
                        WriteTxOut.Parameters.Add(new SQLiteParameter("id", it.Id));
                        WriteTxOut.Parameters.Add(new SQLiteParameter("address", it.Address));
                        WriteTxOut.Parameters.Add(new SQLiteParameter("ammount", it.Amount));

                        try
                        {
                            WriteTxOut.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            Universal.Log.Write(Universal.LogLevel.Error, "Write TX Cache exception: " + ex.Message + " on " + ex.Source);
                            return false;
                        }
                    }
                }

                return true;
            }

            public bool WriteTransactionRecords(List<TransactionRecord> aTxRecord, long aCurrencyId)
            {
                if (!aTxRecord.Any())
                {
                    return false;
                }
                using (SQLiteTransaction lTransaction = FDBConnection.BeginTransaction())
                {
                    foreach (TransactionRecord lTransactionRecord in aTxRecord)
                    {
                        if (!WriteTransactionRecord(lTransactionRecord, aCurrencyId))
                        {
                            lTransaction.Rollback();
                            return false;
                        }
                    }
                    InsertNewRetrievalTime("Tx");
                    lTransaction.Commit();
                }

                return true;
            }

            public bool Write(string aInternalId, string aExtInfo)
            {
                using (SQLiteCommand WriteTxExt = new SQLiteCommand("INSERT OR REPLACE INTO TxExt (internalid, extdata) VALUES (?,?)", FDBConnection))
                {
                    WriteTxExt.Parameters.Add(new SQLiteParameter("internalid", aInternalId));
                    WriteTxExt.Parameters.Add(new SQLiteParameter("extdata", aExtInfo));

                    try
                    {
                        WriteTxExt.ExecuteNonQuery();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Universal.Log.Write(Universal.LogLevel.Error, "Write TxExt Cache exception: " + ex.Message + " on " + ex.Source);
                        return false;
                    }
                }
            }

            public bool Read(out List<CurrencyAccount> aCurrencyAccountsList, long? aCurrencyId = null)
            {
                return DBManager.ReadCurrencyAccount(out aCurrencyAccountsList, FDBConnection, aCurrencyId);
            }

            private static bool ReadCurrencyAccount(out List<CurrencyAccount> aCurrencyAccountsList, SQLiteConnection aConnection, long? aCurrencyId = null)
            {
                aCurrencyAccountsList = new List<CurrencyAccount>();

                string qrywhere = aCurrencyId.HasValue ? (" WHERE currencyId = " + aCurrencyId.Value) : string.Empty;
                string qry = "SELECT id, currencyid, address FROM MonitoredAccounts" + qrywhere;

                try
                {
                    using (SQLiteCommand command = new SQLiteCommand(qry, aConnection))
                    {
                        using (SQLiteDataReader rdr = command.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                aCurrencyAccountsList.Add(new CurrencyAccount(rdr.GetInt64(0), rdr.GetInt32(1), rdr.GetString(2)));
                            }
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Universal.Log.Write(Universal.LogLevel.Error, "Read CurrencyAccount exception: " + ex.Message + " on " + ex.Source);
                    return false;
                }
            }

            public bool Read(out List<CurrencyItem> aCurrencyList, long? aId = null)
            {
                aCurrencyList = new List<CurrencyItem>();

                string qrywhere = aId.HasValue ? (" WHERE id = " + aId.Value) : string.Empty;
                string qry = "SELECT id, name, ticker, precision, MinConfirmations, livedate, Icon, IconSize, FeePerKb, ChainParams, Status FROM Currencies2" + qrywhere;

                try
                {
                    using (SQLiteCommand command = new SQLiteCommand(qry, FDBConnection))
                    {
                        using (SQLiteDataReader rdr = command.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                byte[] lIcon = new byte[rdr.GetInt32(7)];

                                byte[] lChainParams = Convert.FromBase64String(rdr.GetString(9));

                                MyCacheChainParams lChainParamsObject;

                                using (MemoryStream ms = new MemoryStream(lChainParams))
                                {
                                    BinaryFormatter lBf = new BinaryFormatter();

                                    lChainParamsObject = (MyCacheChainParams)lBf.Deserialize(ms);
                                }

                                rdr.GetBytes(6, 0, lIcon, 0, rdr.GetInt32(7));

                                ChainParams lParams = new ChainParams();
                                lParams.CopyFrom(lChainParamsObject);

                                aCurrencyList.Add(new CurrencyItem(
                                    Convert.ToInt32(rdr.GetInt32(0)),
                                    rdr.GetString(1),
                                    rdr.GetString(2),
                                    (ushort)rdr.GetInt16(3),
                                    rdr.GetDateTime(5),
                                    Convert.ToInt16(rdr.GetInt16(4)),
                                    lIcon,
                                    rdr.GetInt32(8),
                                    lParams,
                                    (CurrencyStatus)Enum.Parse(typeof(CurrencyStatus), rdr.GetString(10))
                                    ));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Universal.Log.Write(Universal.LogLevel.Error, "Read CurrencyList cache exception: " + ex.Message + " on " + ex.Source);
                    return false;
                }
                return true;
            }

            public bool Read(out List<CurrencyStatusItem> aCurrencyStatusList, long? aCurrencyId = null)
            {
                aCurrencyStatusList = new List<CurrencyStatusItem>();

                string qrywhere = aCurrencyId.HasValue ? (" WHERE currencyId = " + aCurrencyId.Value) : string.Empty;
                string qry = "SELECT id, currencyid, statustime, status, extinfo, blockheight FROM CurrenciesStatus" + qrywhere;

                try
                {
                    using (SQLiteCommand command = new SQLiteCommand(qry, FDBConnection))
                    {
                        using (SQLiteDataReader rdr = command.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                aCurrencyStatusList.Add(new CurrencyStatusItem(Convert.ToInt64(rdr.GetInt64(0)), rdr.GetInt32(1), rdr.GetDateTime(2), (CurrencyStatus)Enum.Parse(typeof(CurrencyStatus), rdr.GetString(3)), rdr.GetString(4), rdr.GetInt64(5)));
                            }
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(LogLevel.Error, "Read CurrencyStatus cache exception: " + ex.Message + " on " + ex.Source);
                    return false;
                }
            }

            public bool Read(out List<TransactionRecord> aTxList, long aCurrencyId)
            {
                aTxList = new List<TransactionRecord>();

                string qrywhere = " WHERE currencyid = " + aCurrencyId;
                string qry = string.Format("SELECT internalid, currencyid, id, dattime, block, TxFee, Valid FROM TxTable WHERE currencyid = {0} and Valid = 1", aCurrencyId);

                try
                {
                    using (SQLiteCommand lSqlCommand = new SQLiteCommand(qry, FDBConnection))
                    {
                        using (SQLiteDataReader lSqLiteReader = lSqlCommand.ExecuteReader())
                            while (lSqLiteReader.Read())
                                aTxList.Add(new TransactionRecord(
                                    lSqLiteReader.GetInt64(0),
                                    lSqLiteReader.GetInt64(1),
                                    lSqLiteReader.GetString(2),
                                    lSqLiteReader.GetDateTime(3),
                                    Convert.ToInt64(lSqLiteReader.GetInt64(4)),
                                    lSqLiteReader.GetBoolean(6)));
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(LogLevel.Error, "Read Tx cache exception: {0}", ex);
                    return false;
                }

                foreach (TransactionRecord it in aTxList)
                {
                    string qrywhereTxIn = " WHERE internalid = " + it.TransactionRecordId;
                    string qryTxIn = "SELECT internalid, id, address, ammount FROM TxIn" + qrywhereTxIn;

                    try
                    {
                        using (SQLiteCommand command = new SQLiteCommand(qryTxIn, FDBConnection))
                        {
                            using (SQLiteDataReader rdr = command.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                    it.AddInput(Convert.ToInt64(rdr.GetInt64(3)), rdr.GetString(2), Convert.ToInt64(rdr.GetInt64(1)));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Universal.Log.Write(Universal.LogLevel.Error, "Read Tx cache exception: " + ex.Message + " on " + ex.Source);
                        return false;
                    }

                    string qrywhereTxOut = " WHERE internalid = " + it.TransactionRecordId;
                    string qryTxOut = "SELECT internalid, id, address, ammount FROM TxOut" + qrywhereTxOut;

                    try
                    {
                        using (SQLiteCommand command = new SQLiteCommand(qryTxOut, FDBConnection))
                        {
                            using (SQLiteDataReader rdr = command.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                    it.AddOutput(rdr.GetInt64(3), rdr.GetString(2), aId: rdr.GetInt32(1));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Universal.Log.Write(Universal.LogLevel.Error, "Read Tx cache exception: " + ex.Message + " on " + ex.Source);
                        return false;
                    }
                }

                return true;
            }

            public void AddOrReplaceLifeTimes(string aKey, double aSeconds)
            {
                if (FLifeTimes.ContainsKey(aKey))
                {
                    FLifeTimes[aKey] = TimeSpan.FromSeconds(aSeconds);
                }
                else
                {
                    FLifeTimes.Add(aKey, TimeSpan.FromSeconds(aSeconds));
                }
            }

            public bool CheckLastRetrieval(string aType)
            {
                if (!FLifeTimes.ContainsKey(aType))
                {
                    throw new ArgumentException("No lifetime set for type provided");
                }

                string qryWhere = " WHERE type = '" + aType + "'";
                string qry = "SELECT lastdataretrieval FROM DataReferences" + qryWhere;
                long count = 0;
                DateTime lLastDataRetrieval = new DateTime();

                using (SQLiteCommand command = new SQLiteCommand(qry, FDBConnection))
                {
                    using (SQLiteDataReader rdr = command.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            lLastDataRetrieval = rdr.GetDateTime(0);
                            count++;
                        }
                    }
                }

                bool lResult = false;

                switch (count)
                {
                    case 0:
                        lResult = true;
                        break;

                    case 1:

                        if (FLifeTimes[aType] < (DateTime.Now - lLastDataRetrieval))
                        {
                            lResult = true;
                        }

                        break;
                }

                return lResult;
            }

            public void InsertNewRetrievalTime(string aType)
            {
                using (SQLiteCommand Write = new SQLiteCommand("INSERT OR REPLACE INTO DataReferences (type, lastdataretrieval) VALUES (@type, @lastdataretrieval)", FDBConnection))
                {
                    Write.Parameters.Add(new SQLiteParameter("type", aType));
                    Write.Parameters.Add(new SQLiteParameter("lastdataretrieval", DateTime.Now));

                    Write.ExecuteNonQuery();
                }
            }

            public bool ClearAll()
            {
                try
                {
                    foreach (string lTableName in TableNames)
                    {
                        string qry = "DELETE FROM " + lTableName;
                        using (SQLiteCommand lSqLiteCommand = new SQLiteCommand(qry, FDBConnection))
                        {
                            {
                                lSqLiteCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }
                catch
                {
                    return false;
                }

                return true;
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
                        FDBConnection.Close();

                        SQLiteConnection.ClearPool(FDBConnection);
                        FDBConnection.Dispose();
                        TableNames.Clear();
                    }

                    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // TODO: set large fields to null.

                    disposedValue = true;
                }
            }

            // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
            // ~DBManager() {
            //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            //   Dispose(false);
            // }

            // This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
                // TODO: uncomment the following line if the finalizer is overridden above.
                // GC.SuppressFinalize(this);
            }

            #endregion IDisposable Support
        }

        [Serializable]
        public class MyCacheChainParams : IChianParams
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
        }
    }
}