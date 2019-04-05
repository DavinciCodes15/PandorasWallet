using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace Pandora.Client.Exchange
{
    public class ExchangeTxDBHandler : IDisposable
    {
        private string FConnectionString;

        public ExchangeTxDBHandler(string aDataPath, string aSQLiteFile)
        {
            FConnectionString = "Data Source=" + Path.Combine(aDataPath, aSQLiteFile + ".exchange" + ";PRAGMA journal_mode=WAL;");

            SQLiteConnection lDBConnection = new SQLiteConnection("Data Source=" + Path.Combine(aDataPath, aSQLiteFile + ".sqlite" + ";PRAGMA journal_mode=WAL;"));
            lDBConnection.Open();

            lDBConnection.Close();

            CreateTables();
        }

        private void CreateTables()
        {
            using (SQLiteConnection lConnection = new SQLiteConnection(FConnectionString))
            {
                lConnection.Open();
                SQLiteCommand lCreateTransactionTable = new SQLiteCommand("CREATE TABLE IF NOT EXISTS ExchangeTx (InternalID integer PRIMARY KEY autoincrement, ID varchar(255) , Market VARCHAR(100), Quantity numeric, OpenTime Datetime, Rate FLOAT, Stop FLOAT, Completed INT, Cancelled INT, Status INT, CoinTXID varchar(255), BaseTicker varchar(255), Name varchar(255))", lConnection);
                SQLiteCommand lCreateLogTable = new SQLiteCommand("CREATE TABLE IF NOT EXISTS ExchangeLog (ID integer PRIMARY KEY autoincrement, TransactionID int, TransactionTime datetime, Message varchar(255), MessageLevel int)", lConnection);

                lCreateTransactionTable.ExecuteNonQuery();
                lCreateLogTable.ExecuteNonQuery();

                lCreateLogTable.Dispose();
                lCreateTransactionTable.Dispose();

                string lQuery = "PRAGMA table_info(ExchangeTx)";

                SQLiteCommand lReadColumns = new SQLiteCommand(lQuery, lConnection);

                List<string> lColumns = new List<string>();
                using (SQLiteDataReader rdr = lReadColumns.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        lColumns.Add(rdr.GetString(1));
                    }
                }

                lReadColumns.Dispose();

                if (!lColumns.Contains("Stop"))
                {
                    lQuery = "ALTER TABLE ExchangeTx ADD Stop FLOAT DEFAULT 0 NOT NULL";
                    SQLiteCommand lAddColumn = new SQLiteCommand(lQuery, lConnection);
                    lAddColumn.ExecuteNonQuery();
                    lAddColumn.Dispose();
                }

                lConnection.Close();
            }
        }

        public bool WriteTransaction(MarketOrder aMarketTransaction)
        {
            using (SQLiteConnection lConnection = new SQLiteConnection(FConnectionString))
            {
                lConnection.Open();
                SQLiteCommand lCommand = new SQLiteCommand("INSERT OR REPLACE INTO ExchangeTx (ID,Market, Quantity, OpenTime, Rate, Stop, Completed, Cancelled, Status, CoinTXID, BaseTicker, Name) VALUES (@Id, @Market, @Quantity, @Opentime, @Rate, @Stop, @Completed, @Cancelled, @Status, @CoinTxID, @BaseTicker, @Name)", lConnection);
                lCommand.Parameters.Add(new SQLiteParameter("@Id", aMarketTransaction.ID.ToString()));
                lCommand.Parameters.Add(new SQLiteParameter("@Market", aMarketTransaction.Market));
                lCommand.Parameters.Add(new SQLiteParameter("@Quantity", aMarketTransaction.SentQuantity));
                lCommand.Parameters.Add(new SQLiteParameter("@Opentime", aMarketTransaction.OpenTime));
                lCommand.Parameters.Add(new SQLiteParameter("@Rate", aMarketTransaction.Rate));
                lCommand.Parameters.Add(new SQLiteParameter("@Stop", aMarketTransaction.StopPrice));
                lCommand.Parameters.Add(new SQLiteParameter("@Completed", aMarketTransaction.Completed));
                lCommand.Parameters.Add(new SQLiteParameter("@Cancelled", aMarketTransaction.Cancelled));
                lCommand.Parameters.Add(new SQLiteParameter("@Status", aMarketTransaction.Status));
                lCommand.Parameters.Add(new SQLiteParameter("@CoinTxID", aMarketTransaction.CoinTxID));
                lCommand.Parameters.Add(new SQLiteParameter("@BaseTicker", aMarketTransaction.BaseTicker));
                lCommand.Parameters.Add(new SQLiteParameter("@Name", aMarketTransaction.Name));

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

        public bool UpdateTransaction(MarketOrder aMarketTransaction, OrderStatus aStatus)
        {
            using (SQLiteConnection lConnection = new SQLiteConnection(FConnectionString))
            {
                lConnection.Open();
                SQLiteCommand lCommand = new SQLiteCommand("Update ExchangeTx SET ID = @Id, Market = @Market, Quantity = @Quantity, OpenTime = @Opentime, Rate = @Rate, Stop = @Stop, Completed = @Completed, Cancelled = @Cancelled, Status = @Status, CoinTXID = @CoinTxID, BaseTicker = @BaseTicker, Name = @Name WHERE InternalID = " + aMarketTransaction.InternalID, lConnection);
                lCommand.Parameters.Add(new SQLiteParameter("@Id", aMarketTransaction.ID.ToString()));
                lCommand.Parameters.Add(new SQLiteParameter("@Market", aMarketTransaction.Market));
                lCommand.Parameters.Add(new SQLiteParameter("@Quantity", aMarketTransaction.SentQuantity));
                lCommand.Parameters.Add(new SQLiteParameter("@Opentime", aMarketTransaction.OpenTime));
                lCommand.Parameters.Add(new SQLiteParameter("@Rate", aMarketTransaction.Rate));
                lCommand.Parameters.Add(new SQLiteParameter("@Stop", aMarketTransaction.StopPrice));
                lCommand.Parameters.Add(new SQLiteParameter("@Completed", aMarketTransaction.Completed));
                lCommand.Parameters.Add(new SQLiteParameter("@Cancelled", aMarketTransaction.Cancelled));
                lCommand.Parameters.Add(new SQLiteParameter("@Status", (int)aStatus));
                lCommand.Parameters.Add(new SQLiteParameter("@CoinTxID", aMarketTransaction.CoinTxID));
                lCommand.Parameters.Add(new SQLiteParameter("@BaseTicker", aMarketTransaction.BaseTicker));
                lCommand.Parameters.Add(new SQLiteParameter("@Name", aMarketTransaction.Name));

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

        public bool WriteOrderLog(int aOrderID, string aMessage, OrderMessage.OrderMessageLevel aMessageLevel)
        {
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

        public bool ReadOrderLogs(int aOrderID, out List<OrderMessage> aMessages)
        {
            string lQry = "SELECT TransactionTime, Message, MessageLevel FROM ExchangeLog where TransactionID = '" + aOrderID + "'";
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
                            lListMessages.Add(new OrderMessage { Time = rdr.GetDateTime(0), Message = rdr.GetString(1), Level = (OrderMessage.OrderMessageLevel)rdr.GetInt32(2) });
                        }
                    }
                    aMessages = lListMessages;
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

        public bool ReadTransactions(out MarketOrder[] aMarketOrders, string aBaseTicker = null)
        {
            string lQry = "SELECT InternalID, ID, Market, Quantity, OpenTime, Rate, Completed, Cancelled, Status, CoinTxID, BaseTicker, Name, Stop FROM ExchangeTx ";
            if (aBaseTicker != null)
            {
                lQry += "where BaseTicker = '" + aBaseTicker + "'";
            }

            aMarketOrders = null;
            List<MarketOrder> lListOrders = new List<MarketOrder>();

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

                            MarketOrder lOrder = new MarketOrder
                            {
                                InternalID = rdr.GetInt32(0),
                                ID = rdr.GetGuid(1),
                                Market = rdr.GetString(2),
                                SentQuantity = rdr.GetDecimal(3),
                                OpenTime = rdr.GetDateTime(4),
                                Rate = lRate,
                                Completed = rdr.GetBoolean(6),
                                Cancelled = rdr.GetBoolean(7),
                                Status = (OrderStatus)rdr.GetInt32(8),
                                CoinTxID = rdr.GetString(9),
                                BaseTicker = rdr.GetString(10),
                                Name = rdr.GetString(11),
                                StopPrice = lStop == 0 ? lRate : lStop
                            };

                            lListOrders.Add(lOrder);
                        }
                    }
                    aMarketOrders = lListOrders.ToArray();
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

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    FConnectionString = null;

                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ExchangeTxDBHandler() {
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
}