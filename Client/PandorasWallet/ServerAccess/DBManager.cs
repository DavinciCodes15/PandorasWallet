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
        internal class DBManager : IDisposable

        {
            private SQLiteConnection FDBConnection;
            private List<string> TableNames;

            private Dictionary<string, TimeSpan> FLifeTimes = new Dictionary<string, TimeSpan>();

            public DBManager(string aDataPath, string aSQLiteFile)
            {
                try
                {
                    FDBConnection = new SQLiteConnection("Data Source=" + Path.Combine(aDataPath, aSQLiteFile + ".sqlite" + ";PRAGMA journal_mode=WAL;"));
                    FDBConnection.Open();

                    CreateTables();
                    GetTableNames();
                }
                catch
                {
                    throw;
                }
            }

            private void GetTableNames()
            {
                TableNames = new List<string>();

                try
                {
                    using (SQLiteCommand command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type = 'table' and name not like 'Exchange%' and name not like 'sqlite%' ORDER BY 1", FDBConnection))
                    {
                        using (SQLiteDataReader rdr = command.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                TableNames.Add(rdr.GetString(0));
                            }
                        }
                    }
                }
                catch
                {
                    throw;
                }
            }

            private void CreateTables()
            {
                SQLiteCommand CreateMonitoredAccounts = new SQLiteCommand("CREATE TABLE IF NOT EXISTS MonitoredAccounts (id BIGINT, currencyid INT, address VARCHAR(100),PRIMARY KEY (id,currencyId))", FDBConnection);
                SQLiteCommand CreateCurrencyData = new SQLiteCommand("CREATE TABLE IF NOT EXISTS Currencies (id INT PRIMARY KEY, name VARCHAR(100), ticker VARCHAR(50), precision INT, MinConfirmations INT, livedate DATETIME, Icon BLOB, IconSize int, FeePerKb int, ChainParams BLOB)", FDBConnection);
                SQLiteCommand CreateCurrencyStatusData = new SQLiteCommand("CREATE TABLE IF NOT EXISTS CurrenciesStatus (id BIGINT, currencyid INT PRIMARY KEY, statustime DATETIME, status VARCHAR(50), extinfo VARCHAR(255), blockheight INT)", FDBConnection);
                SQLiteCommand CreateTxTable = new SQLiteCommand("CREATE TABLE IF NOT EXISTS Tx (internalid BIGINT, currencyid BIGINT, id VARCHAR(100), dattime DATETIME, block BIGINT, TxFee BIGINT, PRIMARY KEY(internalid, currencyid))", FDBConnection);
                SQLiteCommand CreateInputsTxTable = new SQLiteCommand("CREATE TABLE IF NOT EXISTS TxIn (internalid BIGINT, id BIGINT, address varchar(100), ammount BIGINT, PRIMARY KEY (internalid,id,address,ammount))", FDBConnection);
                SQLiteCommand CreateOutputsTxTable = new SQLiteCommand("CREATE TABLE IF NOT EXISTS TxOut (internalid BIGINT, id BIGINT, address varchar(100), ammount BIGINT, PRIMARY KEY (internalid,id,address,ammount))", FDBConnection);
                SQLiteCommand CreateExtTxTable = new SQLiteCommand("CREATE TABLE IF NOT EXISTS TxExt (internalid BIGINT PRIMARY KEY, extdata varchar(100))", FDBConnection);
                SQLiteCommand CreateDataRetrievalTable = new SQLiteCommand("CREATE TABLE IF NOT EXISTS DataReferences (type VARCHAR(100) PRIMARY KEY, lastdataretrieval DATETIME INTEGER)", FDBConnection);

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

                        lTransaction.Commit();

                        CreateMonitoredAccounts.Dispose();
                        CreateCurrencyData.Dispose();
                        CreateCurrencyStatusData.Dispose();
                        CreateTxTable.Dispose();
                        CreateInputsTxTable.Dispose();
                        CreateOutputsTxTable.Dispose();
                        CreateExtTxTable.Dispose();
                        CreateDataRetrievalTable.Dispose();
                    }
                    catch
                    {
                        lTransaction.Rollback();
                        throw;
                    }
                }
            }

            public Dictionary<string, long[]> GetSavedCheckpoints()
            {
                Dictionary<string, long[]> lCheckpoints = new Dictionary<string, long[]>();
                foreach (string it in TableNames)
                {
                    switch (it)
                    {
                        case "MonitoredAccounts":

                            string qry1 = "SELECT Max(id), currencyid FROM " + it + " GROUP BY currencyId";
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

                            lCheckpoints.Add(it, LList.ToArray());
                            lCheckpoints.Add(it + "Index", LIndex.ToArray());
                            break;

                        case "Tx":

                            string qry3alt = "SELECT Min(internalid), currencyid FROM " + it + " WHERE block == 0 GROUP BY currencyId";

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

                            string qry3 = "SELECT Max(internalid), currencyid FROM " + it + " WHERE block != 0 GROUP BY currencyId";

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

                            lCheckpoints.Add(it, lListFinalResult.Select(x => x.Item1).ToArray());
                            lCheckpoints.Add(it + "Index", lListFinalResult.Select(x => x.Item2).ToArray());
                            break;

                        case "CurrenciesStatus":
                            string qry4 = "SELECT Max(id), currencyid FROM " + it + " GROUP BY currencyId";
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

                            lCheckpoints.Add(it, LList3.ToArray());
                            lCheckpoints.Add(it + "Index", LIndex3.ToArray());
                            break;

                        case "Currencies":

                            lCheckpoints.Add(it, new long[1]);
                            string qry5 = "SELECT Id FROM " + it;
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

                            lCheckpoints[it] = lCoinIds.ToArray();

                            break;

                        case "DataReferences":
                        case "TxIn":
                        case "TxOut":
                        case "TxExt":
                            break;

                        default:

                            lCheckpoints.Add(it, new long[1]);
                            string qry2 = "SELECT COUNT(id), Max(id) FROM " + it + " limit 1";

                            using (SQLiteCommand command = new SQLiteCommand(qry2, FDBConnection))
                            {
                                using (SQLiteDataReader rdr = command.ExecuteReader())
                                {
                                    while (rdr.Read())
                                    {
                                        if (rdr.GetInt32(0) > 0)
                                        {
                                            lCheckpoints[it][0] = rdr.GetInt32(1);
                                        }
                                        else
                                        {
                                            lCheckpoints[it][0] = 0;
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
                    lTransaction.Commit();
                }
                return true;
            }

            public bool Write(CurrencyItem aCurrItem)
            {
                using (SQLiteCommand WriteCurrItem = new SQLiteCommand("INSERT OR REPLACE INTO Currencies (id, name, ticker, precision, MinConfirmations,livedate,Icon,IconSize,FeePerKb, ChainParams) VALUES (@id,@name,@ticker,@precision,@MinConfirmations,@LiveDate,@Icon,@IconSize,@FeePerKb, @ChainParams)", FDBConnection))
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

                    lTransaction.Commit();
                }

                return true;
            }

            public bool Write(TransactionRecord aTxRecord, ulong aCurrencyId)
            {
                using (SQLiteCommand WriteTxRecord = new SQLiteCommand("INSERT OR REPLACE INTO Tx (internalid, currencyid, id, dattime, block, TxFee) VALUES (@internalid, @currencyid, @id, @dattime, @block, @TxFee)", FDBConnection))
                {
                    WriteTxRecord.Parameters.Add(new SQLiteParameter("internalid", aTxRecord.TransactionRecordId));
                    WriteTxRecord.Parameters.Add(new SQLiteParameter("currencyid", aCurrencyId));
                    WriteTxRecord.Parameters.Add(new SQLiteParameter("id", aTxRecord.TxId));
                    WriteTxRecord.Parameters.Add(new SQLiteParameter("dattime", aTxRecord.TxDate));
                    WriteTxRecord.Parameters.Add(new SQLiteParameter("block", aTxRecord.Block));
                    WriteTxRecord.Parameters.Add(new SQLiteParameter("TxFee", aTxRecord.TxFee));

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

            public bool Write(List<TransactionRecord> aTxRecord, uint aCurrencyId)
            {
                if (!aTxRecord.Any())
                {
                    return false;
                }
                using (SQLiteTransaction lTransaction = FDBConnection.BeginTransaction())
                {
                    foreach (TransactionRecord it in aTxRecord)
                    {
                        if (!Write(it, aCurrencyId))
                        {
                            lTransaction.Rollback();
                            return false;
                        }
                    }

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

            public bool Read(out List<CurrencyAccount> aCurrencyAccountsList, bool fWithWhere = false, uint aCurrencyId = 0)
            {
                aCurrencyAccountsList = new List<CurrencyAccount>();

                string qrywhere = fWithWhere ? (" WHERE currencyId = " + aCurrencyId) : string.Empty;
                string qry = "SELECT id, currencyid, address FROM MonitoredAccounts" + qrywhere;

                try
                {
                    using (SQLiteCommand command = new SQLiteCommand(qry, FDBConnection))
                    {
                        using (SQLiteDataReader rdr = command.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                aCurrencyAccountsList.Add(new CurrencyAccount(Convert.ToUInt32(rdr.GetInt64(0)), Convert.ToUInt32(rdr.GetInt32(1)), rdr.GetString(2)));
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

            public bool Read(out List<CurrencyItem> aCurrencyList, bool fWithWhere = false, uint aId = 0)
            {
                aCurrencyList = new List<CurrencyItem>();

                string qrywhere = fWithWhere ? (" WHERE id = " + aId) : string.Empty;
                string qry = "SELECT id, name, ticker, precision, MinConfirmations, livedate, Icon, IconSize,FeePerKb, ChainParams FROM Currencies" + qrywhere;

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

                                aCurrencyList.Add(new CurrencyItem(Convert.ToUInt32(rdr.GetInt32(0)), rdr.GetString(1), rdr.GetString(2), Convert.ToUInt16(rdr.GetInt16(3)), rdr.GetDateTime(5), Convert.ToUInt16(rdr.GetInt16(4)), lIcon, rdr.GetInt32(8), lParams));
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

            public bool Read(out List<CurrencyStatusItem> aCurrencyStatusList, bool fWithWhere = false, uint aCurrencyId = 0)
            {
                aCurrencyStatusList = new List<CurrencyStatusItem>();

                string qrywhere = fWithWhere ? (" WHERE currencyId = " + aCurrencyId) : string.Empty;
                string qry = "SELECT id, currencyid, statustime, status, extinfo, blockheight FROM CurrenciesStatus" + qrywhere;

                try
                {
                    using (SQLiteCommand command = new SQLiteCommand(qry, FDBConnection))
                    {
                        using (SQLiteDataReader rdr = command.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                aCurrencyStatusList.Add(new CurrencyStatusItem(Convert.ToInt64(rdr.GetInt64(0)), Convert.ToUInt32(rdr.GetInt32(1)), rdr.GetDateTime(2), (CurrencyStatus)Enum.Parse(typeof(CurrencyStatus), rdr.GetString(3)), rdr.GetString(4), Convert.ToUInt64(rdr.GetInt64(5))));
                            }
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Universal.Log.Write(Universal.LogLevel.Error, "Read CurrencyStatus cache exception: " + ex.Message + " on " + ex.Source);
                    return false;
                }
            }

            public bool Read(out List<TransactionRecord> aTxList, uint aCurrencyId)
            {
                aTxList = new List<TransactionRecord>();

                string qrywhere = " WHERE currencyid = " + aCurrencyId;
                string qry = "SELECT internalid, currencyid, id, dattime, block, TxFee FROM Tx" + qrywhere;

                try
                {
                    using (SQLiteCommand command = new SQLiteCommand(qry, FDBConnection))
                    {
                        using (SQLiteDataReader rdr = command.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                aTxList.Add(new TransactionRecord(Convert.ToUInt64(rdr.GetInt64(0)), rdr.GetString(2), rdr.GetDateTime(3), Convert.ToUInt64(rdr.GetInt64(4))));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Universal.Log.Write(Universal.LogLevel.Error, "Read Tx cache exception: " + ex.Message + " on " + ex.Source);
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
                                    it.AddInput(Convert.ToUInt64(rdr.GetInt64(3)), rdr.GetString(2), Convert.ToUInt64(rdr.GetInt64(1)));
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
                                    it.AddOutput(Convert.ToUInt64(rdr.GetInt64(3)), rdr.GetString(2), Convert.ToUInt64(rdr.GetInt64(1)));
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
                uint count = 0;
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

                switch (count)
                {
                    case 0:
                        InsertNewRetrievalTime(aType);
                        return true;

                    case 1:

                        if (FLifeTimes[aType] < (DateTime.Now - lLastDataRetrieval))
                        {
                            InsertNewRetrievalTime(aType);
                            return true;
                        }

                        return false;

                    default:

                        break;
                }

                return true;
            }

            private void InsertNewRetrievalTime(string aType)
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
                    foreach (string it in TableNames)
                    {
                        string qry = "DELETE FROM " + it;
                        using (SQLiteCommand command = new SQLiteCommand(qry, FDBConnection))
                        {
                            {
                                command.ExecuteNonQuery();
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
        }
    }
}