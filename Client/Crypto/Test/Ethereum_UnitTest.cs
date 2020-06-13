using System;
using System.Text;
using System.Collections.Generic;
using Pandora.Client.ClientLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Pandora.Client.Crypto.Currencies.Ethereum.Test
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class Ethereum_UnitTest
    {
        private EthChainParams FEthChainParams;
        public Ethereum_UnitTest()
        {
            var lParms = new ChainParams();

            lParms.Capabilities = CapablityFlags.EthereumProtocol;
            lParms.NetworkName = "Ethereum";
            FEthChainParams = new EthChainParams(lParms);
            EthereumCurrencyAdvocacy.Register();
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion


        [TestMethod]
        public void CreateObjectTest()
        {
            Assert.IsTrue(CurrencyControl.GetClientCurrencyAdvocacy(1000, FEthChainParams, () => "6A41F49D98D83DF2EEB89F8E895D2269") is EthereumCurrencyAdvocacy, "Error EthereumCurrencyAdvocacy not returned");
        }

        [TestMethod]
        public void CreateAddressTest()
        {
            var lAdvocacy = new EthereumCurrencyAdvocacy(1000, FEthChainParams, () => "6A41F49D98D83DF2EEB89F8E895D22697A41F49D98D83DF2EEB89F8E895D2269");
            var lString = lAdvocacy.GetAddress(0);
            Assert.AreEqual("0xFF3225E2c09280797F5848Ad204e9c0C331Af2c0", lString);
        }

        [TestMethod]
        public void GetPrivateKeyTest()
        {
            var lAdvocacy = new EthereumCurrencyAdvocacy(1000, FEthChainParams, () => "6A41F49D98D83DF2EEB89F8E895D22697A41F49D98D83DF2EEB89F8E895D2269");
            var lString = lAdvocacy.GetAddress(0);
            Assert.AreEqual("0xFF3225E2c09280797F5848Ad204e9c0C331Af2c0", lString);
            lString = lAdvocacy.GetPrivateKey(0);
            Assert.AreEqual("BBE1487FBBC49B4390B8A6C723E480944634AD52DC6EA203466576B98FD51132", lString);
        }

        [TestMethod]
        public void SignTxTest()
        {
            var lTx = new CurrencyTransaction();
            var lAdvocacy = new EthereumCurrencyAdvocacy(1000, FEthChainParams, () => "6A41F49D98D83DF2EEB89F8E895D22697A41F49D98D83DF2EEB89F8E895D2269");
            var lString = lAdvocacy.GetAddress(0);
            Assert.AreEqual("0xFF3225E2c09280797F5848Ad204e9c0C331Af2c0", lString);
            // 1 eth is 1 Quintillion 1, or 1000 Quadrillion
            // 1,000,000,000,000,000,000 Wei (1018)
            lTx.AddInput(13689000000000000, lString); // how much I have
            lTx.TxFee = 420000000000000;   // fee to pay
            lTx.AddOutput(1100000000000000, "0x596a2232d098965bc56b762549E045829CF43c8D"); // amount to move
            var ldata = lAdvocacy.SignTransaction("30", lTx);
            Assert.AreEqual("f8728087019945ca2620008704cbd15e72600094596a2232d098965bc56b762549e045829cf43c8d8703e871b540c000801ca0fb1f1e416a5b9d22b2b34242807e0c41994e1be4909cc16370b5187011a109fea03165afb74b0d47138e86e2faec57bce8079bdd7024937d64c8fdc454d2dfc512", ldata);
        }

    }
}
