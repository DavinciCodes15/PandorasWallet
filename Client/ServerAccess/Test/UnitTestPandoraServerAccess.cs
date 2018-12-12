using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pandora.Client.ClientLib;
using Pandora.Client.Crypto.Currencies.Controls;

namespace Pandora.Client.ServerAccess.Test
{
    [TestClass]
    public class UnitTestPandoraServerAccess
    {
        public PandoraJsonConverter FJsonConverter = new PandoraJsonConverter();

        [TestMethod]
        public void TestLogon()
        {
            using (var lServerAccess = new ServerAccess.PandoraWalletServiceAccess("localhost", 20159, false))
            if (lServerAccess.Logon("dj@davinCicodes.net", "Dj", "test"))
                return;//lServerAccess.Logoff();
            else
                throw new Exception("Basic Logon and log off failed.");
        }

        [TestMethod]
        public void TestNoUserStatus()
        {
            var lServerAccess = new ServerAccess.PandoraWalletServiceAccess("localhost", 20159, false);
            if (lServerAccess.Logon("dj@davinCicodes.net", "Dj", "test"))
            {
                var s = lServerAccess.GetUserStatus();
                var lStat = Newtonsoft.Json.JsonConvert.DeserializeObject<UserStatus>(s,FJsonConverter);
                Assert.AreEqual(lStat.Active, false);
                Assert.AreEqual(lStat.ExtendedInfo, "User has no status.");
                Assert.IsTrue(lStat.StatusDate > DateTime.Parse("2018-06-21"));
            }
            else
                throw new Exception("Basic Logon and log off failed.");
        }

        [TestMethod]
        public void TestGetCurrencyList()
        {
            using (var lServerAccess = new ServerAccess.PandoraWalletServiceAccess("localhost", 20159, false))
            {
                if (lServerAccess.Logon("dj@davinCicodes.net", "Dj", "test"))
                {
                    CurrencyItem[] lList = null;
                    string s = lServerAccess.GetCurrencyList(0);
                    lList = Newtonsoft.Json.JsonConvert.DeserializeObject<CurrencyItem[]>(s,FJsonConverter);
                    var lLength = lList.Length;
                    Assert.AreEqual(lLength > 0, true);
                    Assert.AreEqual(lList[0].Name, "Bitcoin");
                    Assert.AreEqual(lList[0].Ticker, "BTC");
                    Assert.AreEqual(lList[0].MinConfirmations, 6);
                    Assert.AreEqual(lList[0].Precision, 8);
                    Assert.AreEqual(lList[0].Id, 1);
                    Assert.IsTrue(lList[0].LiveDate > DateTime.Parse("2017-12-31"));

                    Assert.AreEqual(lList[1].Name, "Litecoin");
                    Assert.AreEqual(lList[1].Ticker, "LTC");
                    Assert.AreEqual(lList[1].MinConfirmations, 24);
                    Assert.AreEqual(lList[1].Precision, 8);
                    Assert.AreEqual(lList[1].Id, 2);
                    Assert.IsTrue(lList[1].LiveDate > DateTime.Parse("2017-12-31"));

                    Assert.AreEqual(lList[2].Name, "Dash");
                    Assert.AreEqual(lList[2].Ticker, "DASH");
                    Assert.AreEqual(lList[2].MinConfirmations, 6);
                    Assert.AreEqual(lList[2].Precision, 8);
                    Assert.AreEqual(3, lList[2].Id);
                    Assert.IsTrue(lList[2].LiveDate > DateTime.Parse("2017-12-31"));

                }
                else
                    throw new Exception("Basic Logon and log off failed.");
            }
        }

        [TestMethod]
        public void TestGetCurrencyIcon()
        {
            using (var lServerAccess = new ServerAccess.PandoraWalletServiceAccess("localhost", 20159, false))
                if (lServerAccess.Logon("dj@davinCicodes.net", "Dj", "test"))
                {
                    var lList = Newtonsoft.Json.JsonConvert.DeserializeObject<byte[]>(lServerAccess.GetCurrencyIcon(1));
                    Assert.AreEqual(lList.Length > 0, true);
                    using (MemoryStream lStream = new MemoryStream(lList))
                        new System.Drawing.Icon(lStream, 64, 64);
                    lServerAccess.Logoff();
                }
                else
                    throw new Exception("Basic Logon and log off failed.");
        }

        [TestMethod]
        public void TestGetCurrencyStatusList()
        {
            using (var lServerAccess = new ServerAccess.PandoraWalletServiceAccess("localhost", 20159, false))
            {
                if (lServerAccess.Logon("dj@davinCicodes.net", "Dj", "test"))
                {
                    CurrencyStatusItem[] lList = null;
                    string s = lServerAccess.GetCurrencyStatusList(1,0);
                    lList = Newtonsoft.Json.JsonConvert.DeserializeObject<CurrencyStatusItem[]>(s,FJsonConverter);
                    var lLength = lList.Length;
                    Assert.AreEqual(lLength > 0, true);
                    Assert.AreEqual(lList[0].StatusId, 1);
                    Assert.AreEqual((ulong)1, lList[0].CurrencyId);
                    Assert.AreEqual(lList[0].BlockHeight, (ulong)0);
                    Assert.AreEqual(lList[0].Status, CurrencyStatus.Active);
                    Assert.AreEqual(lList[0].ExtendedInfo, "The coin is working fine with 3 servers running with consensus.");
                    Assert.IsTrue(lList[0].StatusTime > DateTime.Parse("2018-06-1"));

                    s = lServerAccess.GetCurrencyStatusList(2, 0);
                    lList = Newtonsoft.Json.JsonConvert.DeserializeObject<CurrencyStatusItem[]>(s,FJsonConverter);
                    lLength = lList.Length;
                    Assert.AreEqual(lLength > 0, true);
                    Assert.AreEqual(lList[0].StatusId, 2);
                    Assert.AreEqual(lList[0].CurrencyId, (ulong)2);
                    Assert.AreEqual(lList[0].BlockHeight, (ulong)0);
                    Assert.AreEqual(lList[0].Status, CurrencyStatus.Maintenance);
                    Assert.AreEqual(lList[0].ExtendedInfo, "The servers are under maintaince and should be back up soon.");
                    Assert.IsTrue(lList[0].StatusTime > DateTime.Parse("2018-06-1"));

                    s = lServerAccess.GetCurrencyStatusList(3, 0);
                    lList = Newtonsoft.Json.JsonConvert.DeserializeObject<CurrencyStatusItem[]>(s,FJsonConverter);
                    lLength = lList.Length;
                    Assert.AreEqual(lLength > 0, true);
                    Assert.AreEqual(3, lList[0].StatusId);
                    Assert.AreEqual(lList[0].CurrencyId, (ulong)3);
                    Assert.AreEqual(lList[0].BlockHeight, (ulong)0);
                    Assert.AreEqual(lList[0].Status, CurrencyStatus.Disabled);
                    Assert.AreEqual(lList[0].ExtendedInfo, "Coin may not come back up.");
                    Assert.IsTrue(lList[0].StatusTime > DateTime.Parse("2018-06-1"));
                }
                else
                    throw new Exception("Basic Logon and log off failed.");
            }
        }

        [TestMethod]
        public void TestAddGetRemoveCurrencyAccount()
        {
            using (var lServerAccess = new ServerAccess.PandoraWalletServiceAccess("localhost", 20159, false))
                if (lServerAccess.Logon("dj@dj.com", "davinci", "test"))
                {
                    var lControl = CurrencyControl.GetCurrencyControl();
                    var lRootSeed = lControl.GenerateRootSeed("dj@dj.com", "davinci", "test");
                    var lAdvocacy = lControl.GetCryptoCurrency(1, "");
                    lAdvocacy.RootSeed = lRootSeed;
                    lServerAccess.AddMonitoredAccount(1, lAdvocacy.GetAddress(10));
                    lServerAccess.AddMonitoredAccount(1, lAdvocacy.GetAddress(11));
                    var lList = Newtonsoft.Json.JsonConvert.DeserializeObject<CurrencyAccount[]>(lServerAccess.GetMonitoredAcccounts(1, 0));
                    foreach (var lItem in lList)
                        Assert.IsTrue(lServerAccess.RemoveMonitoredAcccounts(lItem.Id));
                    lList = Newtonsoft.Json.JsonConvert.DeserializeObject<CurrencyAccount[]>(lServerAccess.GetMonitoredAcccounts(1, 0));
                    Assert.IsTrue(lList.Length == 0, "Not all accounts were removed.");
                    lServerAccess.Logoff();
                }
                else
                    throw new Exception("Basic Logon and log off failed.");
        }

        [TestMethod]
        public void TestAddCreateTransaction()
        {
            using (var lServerAccess = new ServerAccess.PandoraWalletServiceAccess("localhost", 20159, false))
                if (lServerAccess.Logon("dj@dj.com", "davinci", "test"))
                {
                    var lControl = CurrencyControl.GetCurrencyControl();
                    var lRootSeed = lControl.GenerateRootSeed("dj@dj.com", "davinci", "test");
                    var lAdvocacy = lControl.GetCryptoCurrency(1, "");
                    lAdvocacy.RootSeed = lRootSeed;
                    lServerAccess.AddMonitoredAccount(1, lAdvocacy.GetAddress(10));
                    var lTransaction = new CurrencyTransaction();
                    var s = lServerAccess.GetTransactionRecords(1, 0);
                    var lTxList = Newtonsoft.Json.JsonConvert.DeserializeObject<TransactionRecord[]>(s,new Pandora.Client.ClientLib.PandoraJsonConverter());
                    var lTx = lTxList[0];
                    lTransaction.AddInput(lTx.Outputs[0]);
                    lTransaction.AddOutput(lTx.Outputs[0].Amount - 1000, "mzqD8sEGXfRMvcnNnkoGn6wyMN9AGUxaAB");
                    lTransaction.CurrencyId = 1;
                    lTransaction.TxFee = 1000;
                    var lTxData = lServerAccess.CreateTransaction(lTransaction);

                    lTxData = lAdvocacy.SignTransaction(lTxData, lTransaction);

                    //lServerAccess.SendTransaction(1, lTxData);
                    lServerAccess.Logoff();
                }
                else
                    throw new Exception("Basic Logon and log off failed.");
        }

        [TestMethod]
        public void TestSendTransaction()
        {
            using (var lServerAccess = new ServerAccess.PandoraWalletServiceAccess("localhost", 20159, false))
                if (lServerAccess.Logon("dj@dj.com", "davinci", "test"))
                {
                    var lTransaction = new CurrencyTransaction();
                    lTransaction.CurrencyId = 3;
                    var lControl = CurrencyControl.GetCurrencyControl();
                    var lRootSeed = lControl.GenerateRootSeed("dj@dj.com", "davinci", "test");
                    var lAdvocacy = lControl.GetCryptoCurrency(lTransaction.CurrencyId, "");
                    lAdvocacy.RootSeed = lRootSeed;
                    var lAddress = lAdvocacy.GetAddress(10);
                    lServerAccess.AddMonitoredAccount(lTransaction.CurrencyId, lAddress);
                    
                    var s = lServerAccess.GetTransactionRecords(lTransaction.CurrencyId, 0);
                    var lTxList = Newtonsoft.Json.JsonConvert.DeserializeObject<TransactionRecord[]>(s, new Pandora.Client.ClientLib.PandoraJsonConverter());
                    var lTx = lTxList[0];
                    lTransaction.AddInput(lTx.Outputs[0]);
                    lTransaction.AddOutput(lTx.Outputs[0].Amount - 1000, "yPueC9mAxBj7yrPxj4M2vNY3rn4Yz3thrY");
                    lTransaction.TxFee = 1000;
                    var lTxData = lServerAccess.CreateTransaction(lTransaction);

                    lTxData = lAdvocacy.SignTransaction(lTxData, lTransaction);

                    var lHandle = lServerAccess.SendTransaction(lTransaction.CurrencyId, lTxData);
                    System.Threading.Thread.Sleep(5000);
                    if (lServerAccess.IsTransactionSent(lHandle))
                    {
                        var lTxId = lServerAccess.GetTransactionId(lHandle);

                    }
                    lServerAccess.Logoff();
                }
                else
                    throw new Exception("Basic Logon and log off failed.");
        }

        [TestMethod]
        public void TestGetBlockHeight()
        {
            using (var lServerAccess = new ServerAccess.PandoraWalletServiceAccess("localhost", 20159, false))
                if (lServerAccess.Logon("test@test.com", "test", "test"))
                {
                    var lVal = lServerAccess.GetBlockHeight(1);
                    Assert.IsTrue(lVal > 0);
                }
        }

        [TestMethod]
        public void TestGetTransactionRecords()
        {
            using (var lServerAccess = new ServerAccess.PandoraWalletServiceAccess("localhost", 20159, false))
                if (lServerAccess.Logon("test@test.com", "test", "test"))
                {
                    var s = lServerAccess.GetTransactionRecords(1, 0);
                    var lTXRecList = Newtonsoft.Json.JsonConvert.DeserializeObject<TransactionRecord[]>(s,FJsonConverter);
                    Assert.IsTrue(lTXRecList.Length > 0, "No transaction records found.");
                    Assert.IsTrue(lTXRecList[0].TransactionRecordId != 0,"Deserialization may have failed.");
                    lServerAccess.Logoff();
                }
                else
                    throw new Exception("Basic Logon and log off failed.");
        }

        //[TestMethod]
        //public void TestGetTransactionFee()
        //{
        //    using (var lServerAccess = new ServerAccess.PandoraWalletServiceAccess("localhost", 20159, false))
        //        if (lServerAccess.Logon("test@test.com", "test", "test"))
        //        {
        //            try
        //            {
        //                var lFee = lServerAccess.GetTransactionFee(Newtonsoft.Json.JsonConvert.SerializeObject(new CurrencyTransaction()));
        //                throw new Exception("should not have returned anything.");
        //            }
        //            catch(Exception e)
        //            {
        //                if (!e.Message.Contains("Service Timeout."))
        //                    throw;
        //            }
        //            lServerAccess.Logoff();
        //        }
        //        else
        //            throw new Exception("Basic Logon and log off failed.");
        //}

        [TestMethod]
        public void TestCreateTransaction()
        {
            //using (var lServerAccess = new ServerAccess.PandoraWalletServiceAccess("localhost", 20159, false))
            //    if (lServerAccess.Logon("test@test.com", "test", "test"))
            //    {
            //        try
            //        {
            //            var lDataTosign = lServerAccess.CreateTransaction(Newtonsoft.Json.JsonConvert.SerializeObject(new CurrencyTransaction()));
            //            throw new Exception("should not have returned anything.");
            //        }
            //        catch (Exception e)
            //        {
            //            if (!e.Message.Contains("Service Timeout."))
            //                throw;
            //        }
            //        lServerAccess.Logoff();
            //    }
            //    else
            //        throw new Exception("Basic Logon and log off failed.");
        }


        [TestInitialize]
        public void TestInitialize()
        {
            var lString = Environment.ExpandEnvironmentVariables(@"%PW_RootPath%Server\Wallet");
            (new IisExpressWebServer(lString, 20159)).Start();
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
            var webHostStartInfo = InitializeIisExpress(FFullPath,FPort);
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
