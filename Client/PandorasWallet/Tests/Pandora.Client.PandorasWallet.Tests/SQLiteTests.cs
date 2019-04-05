using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SQLite;

namespace Pandora.Client.PandorasWallet.Tests
{
    [TestClass]
    public class SQLiteTests
    {
        [TestMethod]
        public void TestOpenCloseConnection()
        {
            SQLiteConnection lConnection = new SQLiteConnection(string.Format("Data Source={0};PRAGMA journal_mode=WAL;", "asdasd"));

            lConnection.Open();
            lConnection.Close();
        }
    }
}