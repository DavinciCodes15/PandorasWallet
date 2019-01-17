using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Pandora.Client.PandorasWallet.ServerAccess;
using System.Text;
using NBitcoin;
using Pandora.Client.Crypto;
using System.Collections.Generic;
using System.Runtime.Caching;
using Newtonsoft.Json;
using System.Diagnostics;
using Pandora.Client.ClientLib;
using System.Linq;

namespace Pandora.Client.PandorasWallet.TestUnit
{
    [TestClass]
    public class ClientTests
    {
        private IisExpressWebServer webService;

        //public void SQLIOTest(out PandorasCache.DBManager DBTEST)
        //{
        //    Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "DBTEST"));

        //    DBTEST = new PandorasCache.DBManager(Path.Combine(Directory.GetCurrentDirectory(), "DBTEST"), "sqtest");

        //    CurrencyAccount testaccount = new CurrencyAccount(25, 99, "1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN3");

        //    Assert.IsTrue(DBTEST.Write(testaccount));

        //    List<CurrencyAccount> returnaccount;

        //    Assert.IsTrue(DBTEST.Read(out returnaccount, true, 99));

        //    foreach (CurrencyAccount it in returnaccount)
        //    {
        //        Assert.IsTrue(it.Address == "1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN3");
        //    }

        //    var dummyIcon = System.Drawing.SystemIcons.WinLogo;

        //    byte[] serializedIcon;

        //    using (var ms = new MemoryStream())
        //    {
        //        dummyIcon.Save(ms);
        //        serializedIcon = ms.ToArray();
        //    }

        //    CurrencyItem testCurrency = new CurrencyItem(99, "TestCoin", "TTC", 3, DateTime.Now, 5, serializedIcon, 50);

        //    Assert.IsTrue(DBTEST.Write(testCurrency));

        //    List<CurrencyItem> textCurrList;

        //    Assert.IsTrue(DBTEST.Read(out textCurrList, true, 99));

        //    foreach (CurrencyItem it in textCurrList)
        //    {
        //        Assert.IsTrue(it.Ticker == "TTC");
        //        Assert.IsTrue(it.Icon.SequenceEqual(serializedIcon));
        //    }

        //    CurrencyStatusItem teststatus = new CurrencyStatusItem(25, 99, DateTime.Now, CurrencyStatus.Disabled, "", 500000);
        //    Assert.IsTrue(DBTEST.Write(teststatus));

        //    List<CurrencyStatusItem> statusreturn;

        //    Assert.IsTrue(DBTEST.Read(out statusreturn, true, 94));

        //    Assert.IsTrue(statusreturn.Count == 0);

        //    Assert.IsTrue(DBTEST.Read(out statusreturn, true, 99));

        //    foreach (CurrencyStatusItem it in statusreturn)
        //    {
        //        Assert.IsTrue(it.CurrencyId == 99 && it.Status == CurrencyStatus.Disabled);
        //    }

        //    TransactionRecord testtx = new TransactionRecord(25000, "asdsa12354t", DateTime.Now, 2598569857);
        //    TransactionRecord testtx2 = new TransactionRecord(25000, "asdsa12354t", DateTime.Now, 25985698574);

        //    testtx.AddInput(10000000000, "zBvBMSEYstWetqTFn5Au4m4GFgaxJaNVN3", 25);
        //    testtx.AddOutput(10000000000, "zJvBMSEYstWetqTFn5Au4m4GFgaxJaNVN3", 15);
        //    testtx2.AddInput(10000000000, "zBvBMSEYstWetqTFn5Au4m4GFgaxJaNVN3", 25);
        //    testtx2.AddOutput(10000000000, "zJvBMSEYstWetqTFn5Au4m4GFgaxJaNVN3", 15);

        //    Assert.IsTrue(DBTEST.Write(testtx, 99));
        //    Assert.IsTrue(DBTEST.Write(testtx2, 99));

        //    List<TransactionRecord> testtxresult;

        //    Assert.IsTrue(DBTEST.Read(out testtxresult, 99));

        //    foreach (TransactionRecord it in testtxresult)
        //    {
        //        Assert.IsTrue(it.Block == 25985698574);
        //        Assert.IsTrue(it.Inputs[0].Amount == 10000000000);
        //        Assert.IsTrue(it.Outputs[0].Amount == 10000000000);
        //        Assert.IsTrue(it.Inputs[0].Id == 25);
        //        Assert.IsTrue(it.Outputs[0].Id == 15);
        //    }
        //}

        //[TestMethod]
        //public void CacheTest()
        //{
        //    PandorasCache.DBManager DBTEST;

        //    SQLIOTest(out DBTEST);

        //    ObjectCache FCache = MemoryCache.Default;

        //    PandorasCache.CachedObject testCacheObj = new PandorasCache.CachedObject(new PandorasCache(new PandorasServer(".")), FCache, DBTEST, typeof(CurrencyAccount));

        //    CurrencyAccount[] result = testCacheObj.Get(99);

        //    Assert.IsTrue(result[0].Address == "1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN3");

        //    result = testCacheObj.Get(99);

        //    Assert.IsTrue(result[0].Address == "1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN3");

        //    PandorasCache.CachedObject testCacheObj2 = new PandorasCache.CachedObject(new PandorasCache(new PandorasServer(".")), FCache, DBTEST, typeof(TransactionRecord));

        //    TransactionRecord result2 = testCacheObj2.Get(99)[0];

        //    Assert.IsTrue(result2.Inputs[0].Address == "zBvBMSEYstWetqTFn5Au4m4GFgaxJaNVN3");

        //    result2 = testCacheObj2.Get(99)[0];

        //    Assert.IsTrue(result2.Inputs[0].Address == "zBvBMSEYstWetqTFn5Au4m4GFgaxJaNVN3");
        //}

        [TestMethod]
        public void FetchingDataTests()
        {
            PandorasServer TestServer = new PandorasServer(Path.Combine(Directory.GetCurrentDirectory(), "DBTEST"), "localhost", 20159);

            Assert.IsTrue(TestServer.Logon("dj@davinCicodes.net", "Dj", "test"));

            List<CurrencyItem> TestCurrencies = TestServer.FetchCurrencies();

            Assert.IsTrue(TestCurrencies[1].Name == "Litecoin");

            List<CurrencyStatusItem> TestStatus = TestServer.FetchCurrencyStatus(2);

            Assert.IsTrue(TestStatus[0].ExtendedInfo == "The coin is working fine with 3 servers running with consensus.");

            List<CurrencyStatusItem> TestStatus2 = TestServer.FetchCurrencyStatus(2);

            Assert.IsTrue(TestStatus2[0].ExtendedInfo == "The servers are under maintaince and should be back up soon.");

            List<CurrencyStatusItem> TestStatus3 = TestServer.FetchCurrencyStatus(3);

            Assert.IsTrue(TestStatus3[0].ExtendedInfo == "Coin may not come back up.");

            CurrencyTransaction TestCurr = new CurrencyTransaction();

            TestCurr.AddInput(10000000000, "zBvBMSEYstWetqTFn5Au4m4GFgaxJaNVN3");
            TestCurr.AddOutput(10000000000, "zJvBMSEYstWetqTFn5Au4m4GFgaxJaNVN3");
            TestCurr.AddInput(10000000000, "zBvBMSEYstWetqTFn5Au4m4GFgaxJaNVN3");
            TestCurr.AddOutput(10000000000, "zJvBMSEYstWetqTFn5Au4m4GFgaxJaNVN3");

            TestCurr.TxFee = 100;

            string jsonstring = JsonConvert.SerializeObject(TestCurr);
            PandoraJsonConverter lJsonConverter = new PandoraJsonConverter();
            var Response = Newtonsoft.Json.JsonConvert.DeserializeObject<CurrencyTransaction>(jsonstring, lJsonConverter);

            Assert.IsTrue(Response.TxFee == 100 && Response.Outputs[1].Address == "zJvBMSEYstWetqTFn5Au4m4GFgaxJaNVN3");
            TestServer.Logoff();
        }

        private TransactionRecord[] Records = new TransactionRecord[0];
        private PandorasServer TestServer;

        [TestMethod]
        public void PandoraServerTest()
        {
            TestServer = new PandorasServer(Path.Combine(Directory.GetCurrentDirectory(), "DBTEST"), "localhost", 20159);

            Assert.IsTrue(TestServer.Logon("quality@test.com", "quality", "quality"));

            TestServer.OnTransactions += NewTransactionTestHandler;

            var TestUserStatus = (TestServer.GetUserStatus());

            Assert.IsTrue(TestUserStatus.Active && TestUserStatus.ExtendedInfo == "Quality Testing User");

            CurrencyItem[] Currencies = TestServer.GetCurrencyList();

            Assert.IsTrue(Currencies.Count() > 0 && Currencies[0].Name == "Bitcoin");

            CurrencyStatusItem Result2 = TestServer.GetCurrencyStatus(1);

            Assert.IsTrue(Result2.Status == CurrencyStatus.Active && Result2.ExtendedInfo == "The coin is working fine with 3 servers running with consensus.");

            CurrencyAccount[] Accounts = TestServer.MonitoredAccounts.GetById(1);

            Assert.AreEqual(Accounts[0].Address, "mk4DoTmTB8PnNgPyvzozTeNGxC1fduJqb5");

            TestServer.StartTxUpdatingTask();

            while (!Records.Any())
            {
                Records = TestServer.GetTransactions(1);
                System.Threading.Thread.Sleep(1000);
            }

            TransactionRecord Record = Records.FirstOrDefault();

            Assert.IsTrue(Record.TxId == "30c8e2f7f5cf8b00f8b8bff127a8ceb726e30ab354dda361199fc435c22275b9");

            Assert.IsTrue(Record.Block == 1355565);

            Assert.IsTrue(Record.Outputs.Count() == 2);

            var Outputs = Record.Outputs.ToList();

            Assert.IsTrue(Outputs.Exists(x => x.Address == "mk4DoTmTB8PnNgPyvzozTeNGxC1fduJqb5" && x.Amount == 150000000));
            Assert.IsTrue(Outputs.Exists(x => x.Address == "2MwqZdwPUrmBACjacg8uwEZ8rVoCUX7GD2h" && x.Amount == 111418554));

            var Transaction = new CurrencyTransaction();

            Transaction.AddInput(150000000, "mk4DoTmTB8PnNgPyvzozTeNGxC1fduJqb5", 10151);
            Transaction.AddOutput(100000000, "mtCFoNLWbgHnB1SG7R7VqiDizGUP8K2uae");
            Transaction.AddOutput(50000000, "mk4DoTmTB8PnNgPyvzozTeNGxC1fduJqb5");

            Transaction.TxFee = 250000;

            Transaction.CurrencyId = 1;

            //var Response = TestServer.CreateTransaction(1, Transaction);

            //Assert.IsTrue(Response == "0100000001b97522c235c49f1961a3dd54b30ae326b7cea827f1bfb8f8008bcff5f7e2c830010000001976a91431ca57b301db0088e2d5912cff743210364b126c88acffffffff0200e1f505000000001976a9148b10583ecc2a455ab21342b3b34943ec711e694888ac80f0fa02000000001976a91431ca57b301db0088e2d5912cff743210364b126c88ac00000000");

            TestServer.Logoff();
        }

        private void NewTransactionTestHandler(uint[] aIds, bool isConfirmationUpdate)
        {
            Records = TestServer.GetTransactions(1);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            var lString = Environment.ExpandEnvironmentVariables(@"%PW_RootPath%Server\Wallet");
            webService = (new IisExpressWebServer(lString, 20159));
            webService.Start();
        }

        [TestCleanup]
        public void EndTest()
        {
            webService.Stop();
        }
    }

    public class IisExpressWebServer
    {
        private int FPort;
        private string FFullPath;
        private static Process _webHostProcess;

        public IisExpressWebServer(string aFullPath, int aPortNumber)
        {
            FFullPath = aFullPath;
            FPort = aPortNumber;
        }

        public void Start()
        {
            var webHostStartInfo = InitializeIisExpress(FFullPath, FPort);
            _webHostProcess = Process.Start(webHostStartInfo);
        }

        public void Stop()
        {
            if (_webHostProcess == null)
                return;
            if (!_webHostProcess.HasExited)
                _webHostProcess.Kill();
            _webHostProcess.Dispose();
        }

        public string BaseUrl
        {
            get { return string.Format("http://localhost:{0}", FPort); }
        }

        private static ProcessStartInfo InitializeIisExpress(string aFullPath, int aPortNumber)
        {
            // todo: grab stdout and/or stderr for logging purposes?
            var key = Environment.Is64BitOperatingSystem ? "programfiles(x86)" : "programfiles";
            var programfiles = Environment.GetEnvironmentVariable(key);

            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Normal,
                ErrorDialog = true,
                LoadUserProfile = true,
                CreateNoWindow = false,
                UseShellExecute = false,
                Arguments = String.Format("/path:\"{0}\" /port:{1}", aFullPath, aPortNumber),
                FileName = string.Format("{0}\\IIS Express\\iisexpress.exe", programfiles)
            };

            return startInfo;
        }
    }
}