using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pandora.Client.Crypto.Currencies.Bitcoin;
using Pandora.Client.ClientLib;
using Pandora.Client.Crypto.Currencies.Controls;

namespace Pandora.Client.Crypto.Test
{
    [TestClass]
    public class TestCryptoCurrencies
    {
        [TestMethod]
        public void BitcoinTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = BitcoinNetwork.GetNetworks()[BitcoinNetwork.TestNet];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "mrZJWwLk6N4EoWK3Bf77myzdcfLSAXWoEt");
        }

        [TestMethod]
        public void RootKeyGen()
        {
            var s = CurrencyControl.GetCurrencyControl().GenerateRootSeed("test1", "test2", "test3");
            Assert.AreEqual("A586936F27816EB28905632A818C87CD", s);
        }

        [TestMethod]
        public void TestAdvocacyBitcoinTestnet()
        {
            string lData = Pandora.Client.Universal.Encryption.EncryptText("", "Pandora");
            lData = ConvertPassword(lData);
            IServerCurrencyAdvocacy lAdvocacy = CurrencyControl.GetCurrencyControl().GetCryptoCurrency(10001, null);
            lAdvocacy.RootSeed = "618AF711D8A040F6B97D41ABE455D92F";
            var lTestAddress = lAdvocacy.GetAddress(0);
            var lTestKey = lAdvocacy.GetPrivateKey(0);
            Assert.AreEqual(lTestAddress, "mg1Q9u4h63ixPQWtTLho3H51YbJiXG3rEZ");
            Assert.IsTrue(lAdvocacy.IsValidAddress(lTestAddress), "Address created is not valid!?");
            Assert.IsTrue(lAdvocacy.IsValidAddress("2NFrRsQsLEN4xxXrMfpgP7qqLzJTTsFRJW6"), "Segwit address is not valid!?");
        }

        private static string ConvertPassword(string aPassword)
        {
            var lkey = "Pandora";
            if (aPassword == lkey)
                aPassword = "CcMRETWdS243NEx1Agq/U3dlbVg=";
            if (aPassword.EndsWith("="))
                aPassword = Pandora.Client.Universal.Encryption.DecryptText(aPassword, lkey);
            return aPassword;
        }

        [TestMethod]
        public void TestNetAdvocacyBitcoin()
        {
            IServerCurrencyAdvocacy lAdvocacy = CurrencyControl.GetCurrencyControl().GetCryptoCurrency(10001, null);
            lAdvocacy.RootSeed = "618AF711D8A040F6B97D41ABE455D92F";
            var lTestAddress = lAdvocacy.GetAddress(0);
            var lTestKey = lAdvocacy.GetPrivateKey(0);
            Assert.IsTrue(lAdvocacy.IsValidAddress(lTestAddress), "Address created is not valid!?");
            Assert.AreEqual(lTestAddress, "muZkczUieaExLSvmRS649ZTWZn5waA5puT");
            Assert.AreEqual(lTestKey, "cPCNbwfugArLG7NBDjBXxRoUxZsc5BcDthAdGJ8tgPh4j9cY5Cgx");
            ICurrencyTransaction lTxSend = new CurrencyTransaction();
            IPandoraServer lPandoraServer = GetFakePandoraServer();
            //figures out the fee on the server side.
            lTxSend.TxFee = lPandoraServer.GetTransactionFee(lAdvocacy.Id, lTxSend as CurrencyTransaction);
            lTxSend.AddInput(300000000, "muZkczUieaExLSvmRS649ZTWZn5waA5puT");
            lTxSend.AddOutput(280000000, "n4Tm9i7Yp93XPhtoRyjEJFMEKeSLVmZtXM");
            lTxSend.AddOutput(20000000 - lTxSend.TxFee, "muZkczUieaExLSvmRS649ZTWZn5waA5puT");
            // if the user does not set a fee the fee will be set also
            // if not all money is spent from the output address regardless of the currency
            // The IPandoraServer will send change back to the last input address.
            //ICurrencyTransaction lCurrencyTx = lTxSend.Clone();
            var lTxData = lPandoraServer.CreateTransaction(lAdvocacy.Id, lTxSend as CurrencyTransaction);

            // Should return this info....
            //'020000000195253ae6ad09a47c02cb0f9d6ffba70f24a9b6c6eb6731eb726d24aa94c7a1430100000000ffffffff020076b010000000001976a914fbb057bbc267c9deddd75e8dfad6096ad7470c4a88acc01f2e01000000001976a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac00000000'
            //'[{"txid":"43a1c794aa246d72eb3167ebc6b6a9240fa7fb6f9d0fcb027ca409ade63a2595","vout":1,"scriptPubKey":"76a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac"}]' '["cPCNbwfugArLG7NBDjBXxRoUxZsc5BcDthAdGJ8tgPh4j9cY5Cgx"]'
            //
            // This call must remember that change is sent to the last input address
            var lSignedTxData = lAdvocacy.SignTransaction(lTxData, lTxSend);
            // "020000000195253ae6ad09a47c02cb0f9d6ffba70f24a9b6c6eb6731eb726d24aa94c7a143010000006a473044022038063fa5d091fdabd18999db757aaf9a1c2eccb105727de52346b75f8355532102205e18f279ebe1be5b1b385e7d6e5bdfe18f498be96845c2582029c8c4b937fea6012102d7273d559e6196e34452705b0158b7d8af92ce43151b9782ae34c0b9e0025851ffffffff020076b010000000001976a914fbb057bbc267c9deddd75e8dfad6096ad7470c4a88acc01f2e01000000001976a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac00000000"
            Assert.AreEqual(lSignedTxData, "020000000195253ae6ad09a47c02cb0f9d6ffba70f24a9b6c6eb6731eb726d24aa94c7a143010000006a473044022038063fa5d091fdabd18999db757aaf9a1c2eccb105727de52346b75f8355532102205e18f279ebe1be5b1b385e7d6e5bdfe18f498be96845c2582029c8c4b937fea6012102d7273d559e6196e34452705b0158b7d8af92ce43151b9782ae34c0b9e0025851ffffffff020076b010000000001976a914fbb057bbc267c9deddd75e8dfad6096ad7470c4a88acc01f2e01000000001976a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac00000000");
            lPandoraServer.SendTransaction(1, lSignedTxData);
        }

        private IPandoraServer GetFakePandoraServer()
        {
            var lFake = new FakePandoraServer();
            lFake.FakeTransactionFee = 200000;
            lFake.FakeTransaction =
            "020000000195253ae6ad09a47c02cb0f9d6ffba70f2" +
            "4a9b6c6eb6731eb726d24aa94c7a143010000001976" +
            "a9149a1943aaa58e63d72a911efdc99102fb826c508" +
            "f88acffffffff020076b010000000001976a914fbb0" +
            "57bbc267c9deddd75e8dfad6096ad7470c4a88acc01" +
            "f2e01000000001976a9149a1943aaa58e63d72a911e" +
            "fdc99102fb826c508f88ac00000000";
            //"{\"hex\":\"020000000195253ae6ad09a47c02cb0f9d6ffba70f2" +
            //           "4a9b6c6eb6731eb726d24aa94c7a1430100000000ff" +
            //              "ffffff020076b010000000001976a914fbb057bbc26" +
            //              "7c9deddd75e8dfad6096ad7470c4a88acc01f2e0100" +
            //              "0000001976a9149a1943aaa58e63d72a911efdc9910" +
            //              "2fb826c508f88ac00000000\"," +
            // "\"inputs\":[{\"txid\":\"43a1c794aa246d72eb3167ebc6b6a9240fa7fb6f9d0fcb027ca409ade63a2595\"," +
            //              "\"vout\":1,\"scriptPubKey\":\"76a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac\"}]}";
            return lFake;
        }

        [TestMethod]
        public void BitcoinTwoInputTestNetAdvocacy()
        {
            IServerCurrencyAdvocacy lAdvocacy = Pandora.Client.Crypto.Currencies.Controls.CurrencyControl.GetCurrencyControl().GetCryptoCurrency(1, null);
            lAdvocacy.RootSeed = "618AF711D8A040F6B97D41ABE455D92F";
            var lTestAddress = lAdvocacy.GetAddress(0);
            var lTestKey = lAdvocacy.GetPrivateKey(0);
            //Assert.AreEqual(lTestAddress, "muZkczUieaExLSvmRS649ZTWZn5waA5puT");
            //Assert.AreEqual(lTestKey, "cPCNbwfugArLG7NBDjBXxRoUxZsc5BcDthAdGJ8tgPh4j9cY5Cgx");
            CurrencyTransaction lTxSend = new CurrencyTransaction();
            IPandoraServer lPandoraServer = GetFakeTwoInputsPandoraServer();
            //figures out the fee on the server side.
            lTxSend.TxFee = lPandoraServer.GetTransactionFee(lAdvocacy.Id, lTxSend);
            lTxSend.AddInput(13750000, "muZkczUieaExLSvmRS649ZTWZn5waA5puT");
            lTxSend.AddInput(19800000, "muZkczUieaExLSvmRS649ZTWZn5waA5puT");
            lTxSend.AddOutput(33000000, "n1WkSm1uQB8ELxRDzQNKbstMJSHh5U3ixX");
            lTxSend.AddOutput(550000 - lTxSend.TxFee, "muZkczUieaExLSvmRS649ZTWZn5waA5puT");
            // if the user does not set a fee the fee will be set also
            // if not all money is spent from the output address regardless of the currency
            // The IPandoraServer will send change back to the last input address.
            //ICurrencyTransaction lCurrencyTx = lTxSend.Clone();
            var lTxData = lPandoraServer.CreateTransaction(lAdvocacy.Id, lTxSend);

            // Should return this info....
            //'020000000195253ae6ad09a47c02cb0f9d6ffba70f24a9b6c6eb6731eb726d24aa94c7a1430100000000ffffffff020076b010000000001976a914fbb057bbc267c9deddd75e8dfad6096ad7470c4a88acc01f2e01000000001976a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac00000000'
            //'[{"txid":"43a1c794aa246d72eb3167ebc6b6a9240fa7fb6f9d0fcb027ca409ade63a2595","vout":1,"scriptPubKey":"76a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac"}]' '["cPCNbwfugArLG7NBDjBXxRoUxZsc5BcDthAdGJ8tgPh4j9cY5Cgx"]'
            //
            // This call must remember that change is sent to the last input address
            var lSignedTxData = lAdvocacy.SignTransaction(lTxData, lTxSend);
            // "020000000195253ae6ad09a47c02cb0f9d6ffba70f24a9b6c6eb6731eb726d24aa94c7a143010000006a473044022038063fa5d091fdabd18999db757aaf9a1c2eccb105727de52346b75f8355532102205e18f279ebe1be5b1b385e7d6e5bdfe18f498be96845c2582029c8c4b937fea6012102d7273d559e6196e34452705b0158b7d8af92ce43151b9782ae34c0b9e0025851ffffffff020076b010000000001976a914fbb057bbc267c9deddd75e8dfad6096ad7470c4a88acc01f2e01000000001976a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac00000000"
            Assert.AreEqual(lSignedTxData, "0200000002f82377c61093b0ade35b7c2e6e4efba4b92b46a94dc2b6f92d77a7a572130ccf000000006b483045022100be959964f09968b6f5c9e8c32d36b012741ce2cac4dc5b49f0c77b41dada707302207bc6308f33eb21d4f2a83f4f2874a5fff5ff73c5ac35e22494bea4808342db84012102d7273d559e6196e34452705b0158b7d8af92ce43151b9782ae34c0b9e0025851ffffffff9a92a4eca6c8c70b660c39ed05dd0484c9a84a02df73b13b792d6a424f98742a010000006b483045022100e918885059c16d94814625063093bcb2705db9578c2286fcd36139c07f304bd502205fa589b7d02b4089c356598b7cd9717764f5f347772fcbd9cadf2b1f275ad1eb012102d7273d559e6196e34452705b0158b7d8af92ce43151b9782ae34c0b9e0025851ffffffff02408af701000000001976a914db589fb64e6835f5165ff2c2abc1a5313d071c5588ac20a10700000000001976a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac00000000");
            lPandoraServer.SendTransaction(1, lSignedTxData);
        }

        private IPandoraServer GetFakeTwoInputsPandoraServer()
        {
            var lFake = new FakePandoraServer();
            lFake.FakeTransactionFee = 50000;
            lFake.FakeTransaction =
                "{\"hex\":\"0200000002f82377c61093b0ade35b7c2e6e4efba4b92b46a94dc2b6f92d77a7a572130ccf0000000000ffffffff9a92a4eca6c8c70b660c39ed05dd0484c9a84a02df73b13b792d6a424f98742a0100000000ffffffff02408af701000000001976a914db589fb64e6835f5165ff2c2abc1a5313d071c5588ac20a10700000000001976a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac00000000\"," +
                 "\"inputs\":[{\"txid\":\"cf0c1372a5a7772df9b6c24da9462bb9a4fb4e6e2e7c5be3adb09310c67723f8\",\"vout\":0,\"scriptPubKey\":\"76a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac\"}," +
                             "{\"txid\":\"2a74984f426a2d793bb173df024aa8c98404dd05ed390c660bc7c8a6eca4929a\",\"vout\":1,\"scriptPubKey\":\"76a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac\"}]}";
            return lFake;
        }

        [TestMethod]
        public void BitcoinTwoInputOneOutputTestNetAdvocacy()
        {
            IServerCurrencyAdvocacy lAdvocacy = Pandora.Client.Crypto.Currencies.Controls.CurrencyControl.GetCurrencyControl().GetCryptoCurrency(1, null);
            lAdvocacy.RootSeed = "618AF711D8A040F6B97D41ABE455D92F";
            var lTestAddress = lAdvocacy.GetAddress(0);
            var lTestKey = lAdvocacy.GetPrivateKey(0);
            //Assert.AreEqual(lTestAddress, "muZkczUieaExLSvmRS649ZTWZn5waA5puT");
            //Assert.AreEqual(lTestKey, "cPCNbwfugArLG7NBDjBXxRoUxZsc5BcDthAdGJ8tgPh4j9cY5Cgx");
            CurrencyTransaction lTxSend = new CurrencyTransaction();
            IPandoraServer lPandoraServer = GetFakeTwoInputsOneOutputPandoraServer();
            //figures out the fee on the server side.
            lTxSend.TxFee = lPandoraServer.GetTransactionFee(lAdvocacy.Id, lTxSend);
            lTxSend.AddInput(55000000, "muZkczUieaExLSvmRS649ZTWZn5waA5puT");
            lTxSend.AddInput(27500000, "muZkczUieaExLSvmRS649ZTWZn5waA5puT");
            lTxSend.AddOutput(27000000, "n1WkSm1uQB8ELxRDzQNKbstMJSHh5U3ixX");
            lTxSend.AddOutput(55500000 - lTxSend.TxFee, "muZkczUieaExLSvmRS649ZTWZn5waA5puT");
            // if the user does not set a fee the fee will be set also
            // if not all money is spent from the output address regardless of the currency
            // The IPandoraServer will send change back to the last input address.
            //ICurrencyTransaction lCurrencyTx = lTxSend.Clone();
            var lTxData = lPandoraServer.CreateTransaction(lAdvocacy.Id, lTxSend);

            // Should return this info....
            //'020000000195253ae6ad09a47c02cb0f9d6ffba70f24a9b6c6eb6731eb726d24aa94c7a1430100000000ffffffff020076b010000000001976a914fbb057bbc267c9deddd75e8dfad6096ad7470c4a88acc01f2e01000000001976a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac00000000'
            //'[{"txid":"43a1c794aa246d72eb3167ebc6b6a9240fa7fb6f9d0fcb027ca409ade63a2595","vout":1,"scriptPubKey":"76a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac"}]' '["cPCNbwfugArLG7NBDjBXxRoUxZsc5BcDthAdGJ8tgPh4j9cY5Cgx"]'
            //
            // This call must remember that change is sent to the last input address
            var lSignedTxData = lAdvocacy.SignTransaction(lTxData, lTxSend);
            // "020000000195253ae6ad09a47c02cb0f9d6ffba70f24a9b6c6eb6731eb726d24aa94c7a143010000006a473044022038063fa5d091fdabd18999db757aaf9a1c2eccb105727de52346b75f8355532102205e18f279ebe1be5b1b385e7d6e5bdfe18f498be96845c2582029c8c4b937fea6012102d7273d559e6196e34452705b0158b7d8af92ce43151b9782ae34c0b9e0025851ffffffff020076b010000000001976a914fbb057bbc267c9deddd75e8dfad6096ad7470c4a88acc01f2e01000000001976a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac00000000"
            Assert.AreEqual(lSignedTxData, "02000000022d09df2f214ba7588ad29aaf37412ad80e3b2c35bc45718966d5e599df8b0ff3000000006a4730440220712cac2b004c55cf763690fb3d02cad485eb1c0bc7f64579999066a1901fe965022056f078905fd2f0c585a7ff87acb008637136362ba74bfdead8f29111c535cf65012102d7273d559e6196e34452705b0158b7d8af92ce43151b9782ae34c0b9e0025851ffffffff79619eeea93e095b080cf11953533febd62814b2ae257616e3ef672cdfde8511000000006b483045022100b45b8daaa0b55c88e4a456371173075a3217832723aedb581a6c0eb83570740e022072ef7dc38919a25cf3fb9a632c46c040d538e47460b562bda2286949a310e557012102d7273d559e6196e34452705b0158b7d8af92ce43151b9782ae34c0b9e0025851ffffffff02c0fc9b01000000001976a914db589fb64e6835f5165ff2c2abc1a5313d071c5588aca0cf4b03000000001976a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac00000000");
            lPandoraServer.SendTransaction(1, lSignedTxData);
        }

        private IPandoraServer GetFakeTwoInputsOneOutputPandoraServer()
        {
            var lFake = new FakePandoraServer();
            lFake.FakeTransactionFee = 200000;
            lFake.FakeTransaction =
                "{\"hex\":\"02000000022d09df2f214ba7588ad29aaf37412ad80e3b2c35bc45718966d5e599df8b0ff30000000000ffffffff79619eeea93e095b080cf11953533febd62814b2ae257616e3ef672cdfde85110000000000ffffffff02c0fc9b01000000001976a914db589fb64e6835f5165ff2c2abc1a5313d071c5588aca0cf4b03000000001976a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac00000000\"," +
                 "\"inputs\":[{\"txid\":\"f30f8bdf99e5d566897145bc352c3b0ed82a4137af9ad28a58a74b212fdf092d\",\"vout\":0,\"scriptPubKey\":\"76a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac\"}," +
                             "{\"txid\":\"1185dedf2c67efe3167625aeb21428d6eb3f535319f10c085b093ea9ee9e6179\",\"vout\":0,\"scriptPubKey\":\"76a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac\"}]}";
            return lFake;
        }

        [TestMethod]
        public void AllionMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.AllionNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "Beq2pi1Fk2fmHfTg6zo2ZKrm7nSFjEFEXH");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "Pp7SvdegZPDqseromawGdKz8gs98MDc3bvLgVV8wchMvhmc1VNo3");
        }

        /*[TestMethod]
        public void AllionTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.AllionNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "mrZJWwLk6N4EoWK3Bf77myzdcfLSAXWoEt");
        }*/

        [TestMethod]
        public void ALQOMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.ALQONetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "ASpDsP7PcVH8qN3R2jo56x3QEGfRayj428");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "Vaj3DvhmPNxCsqNmLYL6g7g8PoyZuYBPx9dmKCedmMkgggQnzUn3");
        }

        [TestMethod]
        public void ArcticCoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.ArcticCoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "ASpDsP7PcVH8qN3R2jo56x3QEGfRayj428");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "T4sKWBaUQmNsK39hwFcv9A4rARZUiRa3Be3YoUm2RDwau42XHyVx");
        }

        [TestMethod]
        public void ArcticCoinTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.ArcticCoinNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "ab6QwtzgDK5huNQcVunCCUNbzY82E2KVFv");
        }

        [TestMethod]
        public void BataMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.BataNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "BFVRqbhy2rCtUEKb5aTi5CayVHBK15XVDT");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "UqptN7uGWcNX1FJvaAMdhFCh9dyJjSpm8wX7WvvAJxz8H2NA4iP2");
        }

        [TestMethod]
        public void BataTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.BataNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "mrZJWwLk6N4EoWK3Bf77myzdcfLSAXWoEt");
        }

        [TestMethod]
        public void BelaCoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D9FF";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.BelaCoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "PfXpMUHnPEJhu8r3DWYNq9teSRwUWoLKSUiYw2nr8DcyP3qfFvDu");
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "B4Ycx4AcEXDVR6qM2q8Jcb9fcdxeFL5LbD");
        }

        [TestMethod]
        public void BelaCoinTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.BelaCoinNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "mrZJWwLk6N4EoWK3Bf77myzdcfLSAXWoEt");
        }

        [TestMethod]
        public void BitcoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.BitcoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "1C3MDtFmHLcz2PqRU68jx4nJkfjjG3Usog");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "Ky344SHJ1PQGYCWqPcg3voXUDZvAeLZ9NS9Hwg8UrFmRPASQ7KKZ");
        }

        [TestMethod]
        public void BitbayMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.BitBayNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "BFVRqbhy2rCtUEKb5aTi5CayVHBK15XVDT");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "PfXpMUHnPEJhu8r3DWYNq9teSRwUWoLKSUiYw2nr8DcDRSMtCePU");
        }

        [TestMethod]
        public void BitBayTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.BitBayNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "mrZJWwLk6N4EoWK3Bf77myzdcfLSAXWoEt");
        }

        [TestMethod]
        public void BitBeanMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D9FF";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.BitBeanNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "LQmwmvMzXr9gTkZ93qrjKKovxsX89bNKrm1gd3AmKh3KBmQdDphA");
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "2D7MHfcGcZ2DRZmSVboHwqAMmYHterJCHo");
        }

        [TestMethod]
        public void BitcloudMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.BitcloudNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "BFVRqbhy2rCtUEKb5aTi5CayVHBK15XVDT");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "PfXpMUHnPEJhu8r3DWYNq9teSRwUWoLKSUiYw2nr8DcDRSMtCePU");
        }

        [TestMethod]
        public void BitcoinCashMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.BitcoinCashNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "1C3MDtFmHLcz2PqRU68jx4nJkfjjG3Usog");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "Ky344SHJ1PQGYCWqPcg3voXUDZvAeLZ9NS9Hwg8UrFmRPASQ7KKZ");
        }

        [TestMethod]
        public void BitcoinCashTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.BitcoinCashNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "mrZJWwLk6N4EoWK3Bf77myzdcfLSAXWoEt");
        }

        [TestMethod]
        public void BitcoreMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.BitcoreNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "1C3MDtFmHLcz2PqRU68jx4nJkfjjG3Usog");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "Ky344SHJ1PQGYCWqPcg3voXUDZvAeLZ9NS9Hwg8UrFmRPASQ7KKZ");
        }

        [TestMethod]
        public void BlackcoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D9FF";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.BlackcoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "PfXpMUHnPEJhu8r3DWYNq9teSRwUWoLKSUiYw2nr8DcyP3qfFvDu");
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "B4Ycx4AcEXDVR6qM2q8Jcb9fcdxeFL5LbD");
        }

        [TestMethod]
        public void C42CoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.C42CoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "4QkB6ke4xnKzZrx7fSoHq4xbnhoGuBff9k");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "M9g6djAVQcjNLLcypDqCJCHND3XPKgixfy8LRJuEn5o7dozsxJo2");
        }

        [TestMethod]
        public void C42CoinTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.C42CoinNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "mrZJWwLk6N4EoWK3Bf77myzdcfLSAXWoEt");
        }

        [TestMethod]
        public void CannabisCoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D9FF";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.CannabisCoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "Q7Gi4xNUuh47pgtLsjj4DgB7BjYS249VvoawcPq8bet7E3SNVFqg");
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "CGZRuP4UN4c7sQFc768G4xy2W9jUPgmdQP");
        }

        [TestMethod]
        public void CasinoCoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D9FF";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.CasinoCoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "Q7Gi4xNUuh47pgtLsjj4DgB7BjYS249VvoawcPq8bet7E3SNVFqg");
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "CGZRuP4UN4c7sQFc768G4xy2W9jUPgmdQP");
        }

        [TestMethod]
        public void CloakcoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.CloakcoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "C4AdopJYTD8e76bm8R8M3T8YkHhCQ35TS8");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "W2TvwQnTuqhcoPR4zmWn4dxb97aXQnzaSUW9zZgvEo1pXfyPbHH5");
        }

        [TestMethod]
        public void CoinOMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.CoinONetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "UZ2NBGXMZc8ZdsKK8cnQvbHoYyFsa1PCAV");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "W2TvwQnTuqhcoPR4zmWn4dxb97aXQnzaSUW9zZgvEo1pXfyPbHH5");
        }

        [TestMethod]
        public void CrownMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.CrownNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "1C3MDtFmHLcz2PqRU68jx4nJkfjjG3Usog");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "Ky344SHJ1PQGYCWqPcg3voXUDZvAeLZ9NS9Hwg8UrFmRPASQ7KKZ");
        }

        [TestMethod]
        public void CryptoBullionMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.CryptoBullionNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "5ckz45Xw6Kid2ANNjhoFHSmxgDa756PLZw");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "MbQzMDFBw5UnFtfHUT1sgiZpxM8LpwY9AHzj6fwXFX4FUoeijWHj");
        }

        [TestMethod]
        public void CryptoBullionTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.CryptoBullionNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "mrZJWwLk6N4EoWK3Bf77myzdcfLSAXWoEt");
        }

        [TestMethod]
        public void DashMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.DashNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "XmjC48ufF3qaBLS1KySxobU6b1KRKYzp2F");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "XD6yWhffK52ibXXDRNfvS2iV8bBk69APk1VCUCTgAd3WnKbQit3E");
        }

        [TestMethod]
        public void DashTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.DashNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "yXMo55z6gbVeX5MYtpmMqctSsHonoecrKV");
            string lKey = testKey.GetCoinSecret(TestNet).ToString();
            Assert.AreEqual(lKey, "cPQ3XMH9ST6Xhdz6n2VBJ82XqoDaJneqSUHm46azMNRRduZJTCZz");
        }

        [TestMethod]
        public void TestNetAdvocacyDash()
        {
            IServerCurrencyAdvocacy lAdvocacy = Pandora.Client.Crypto.Currencies.Controls.CurrencyControl.GetCurrencyControl().GetCryptoCurrency(3, null);
            lAdvocacy.RootSeed = "618AF711D8A040F6B97D41ABE455D92F";
            var lTestAddress = lAdvocacy.GetAddress(0);
            var lTestKey = lAdvocacy.GetPrivateKey(0);
            Assert.AreEqual(lTestAddress, "yghrPt9BPqcsjhQLvdTbx6spp5ionQsTdN");
            Assert.AreEqual(lTestKey, "cPCNbwfvGdV5FgsNopf56bNKQvECTdDGPgTGkiH5eVbiSzTQxmVy");
            CurrencyTransaction lTxSend = new CurrencyTransaction();
            IPandoraServer lPandoraServer = GetFakeDashPandoraServer();
            //figures out the fee on the server side.
            lTxSend.TxFee = lPandoraServer.GetTransactionFee(lAdvocacy.Id, lTxSend);
            lTxSend.AddInput(8900000000, "yghrPt9BPqcsjhQLvdTbx6spp5ionQsTdN");
            lTxSend.AddOutput(8800000000, "ydbsMfcM2DdbzUD4JCqD6sLqnnTeqTeKmf");
            lTxSend.AddOutput(100000000 - lTxSend.TxFee, "yghrPt9BPqcsjhQLvdTbx6spp5ionQsTdN");
            // if the user does not set a fee the fee will be set also
            // if not all money is spent from the output address regardless of the currency
            // The IPandoraServer will send change back to the last input address.
            var lTxData = lPandoraServer.CreateTransaction(lAdvocacy.Id, lTxSend);
            // Should return this info....
            //'020000000195253ae6ad09a47c02cb0f9d6ffba70f24a9b6c6eb6731eb726d24aa94c7a1430100000000ffffffff020076b010000000001976a914fbb057bbc267c9deddd75e8dfad6096ad7470c4a88acc01f2e01000000001976a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac00000000'
            //'[{"txid":"43a1c794aa246d72eb3167ebc6b6a9240fa7fb6f9d0fcb027ca409ade63a2595","vout":1,"scriptPubKey":"76a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac"}]' '["cPCNbwfugArLG7NBDjBXxRoUxZsc5BcDthAdGJ8tgPh4j9cY5Cgx"]'
            //
            // This call must remember that change is sent to the last input address
            var lSignedTxData = lAdvocacy.SignTransaction(lTxData, lTxSend);
            // "020000000195253ae6ad09a47c02cb0f9d6ffba70f24a9b6c6eb6731eb726d24aa94c7a143010000006a473044022038063fa5d091fdabd18999db757aaf9a1c2eccb105727de52346b75f8355532102205e18f279ebe1be5b1b385e7d6e5bdfe18f498be96845c2582029c8c4b937fea6012102d7273d559e6196e34452705b0158b7d8af92ce43151b9782ae34c0b9e0025851ffffffff020076b010000000001976a914fbb057bbc267c9deddd75e8dfad6096ad7470c4a88acc01f2e01000000001976a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac00000000"
            Assert.AreEqual(lSignedTxData, "010000000190a01974ebaf2d7ad5d0761b2bc585c0d476b33f384b611831c57bc9cbb31878010000006a47304402201db5326b718a47f7b3781706c40a73e0b1d1cfe1fb37093086ea3a0033da5dc702202a4215b14828bdf2a59d821c87ecc6237d47ed9f2c806f6718fdd76611f9b3f4012103daacee80c950b21d9d7961c8d6142e5ecb841dc46219878afe2878140858e761ffffffff020058850c020000001976a914bd95407ffc6bb7e50a9f5bd40962392fb53faf1c88ace092f505000000001976a914df9f527c72b90a3bfb723a18cab87d424618373488ac00000000");
            //lPandoraServer.SendTransaction(lSignedTxData);
        }

        private IPandoraServer GetFakeDashPandoraServer()
        {
            var lFake = new FakePandoraServer();
            lFake.FakeTransactionFee = 20000;
            lFake.FakeTransaction =
                "{\"hex\":\"010000000190a01974ebaf2d7ad5d0761b2bc585c0d476b33f384b611831c57bc9cbb318780100000000ffffffff020058850c020000001976a914bd95407ffc6bb7e50a9f5bd40962392fb53faf1c88ace092f505000000001976a914df9f527c72b90a3bfb723a18cab87d424618373488ac00000000\"," +
                 "\"inputs\":[{\"txid\":\"7818b3cbc97bc53118614b383fb376d4c085c52b1b76d0d57a2dafeb7419a090\"," +
                              "\"vout\":1,\"scriptPubKey\":\"76a914df9f527c72b90a3bfb723a18cab87d424618373488ac\"}]}";
            return lFake;
        }

        [TestMethod]
        public void DiamondMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.DiamondNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "dQTdqf5hBaKqdQPDfr7RbMH7Q4vdFSGegT");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "ZHDoWxiFmDreEmhyASCRaU3Jcfzsmzxa2CD2JZK13KbUixK95a6C");
        }

        [TestMethod]
        public void DigibyteMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.DigibyteNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "DGBSm9CQakXGZQ22Cg8JVpwudoU2aiEnDU");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "Ky344SHJ1PQGYCWqPcg3voXUDZvAeLZ9NS9Hwg8UrFmRPASQ7KKZ");
        }

        //[TestMethod]
        //public void DiviMainNet()
        //{
        //    var mykey = "618AF711D8A040F6B97D41ABE455D92F";
        //    var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.DiviNetwork.GetMainNet();
        //    var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
        //    string address = key.PubKey.GetAddress(MainNet).ToString();
        //    Assert.AreEqual(address, "DGBSm9CQakXGZQ22Cg8JVpwudoU2aiEnDU");
        //    string lKey = key.GetBitcoinSecret(MainNet).ToString();
        //    Assert.AreEqual(lKey, "");
        //}

        [TestMethod]
        public void DNotesMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D9FF";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.DNotesNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "QZ1bnSTBS9oXkEveXxujcCTZw39PXJxgR8TLHksR569F532MxJpV");
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "DUaErhxLVbzkKhfsBM8DXLnPPfWJTibyQW");
        }

        [TestMethod]
        public void DogecoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.DogecoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "DGBSm9CQakXGZQ22Cg8JVpwudoU2aiEnDU");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "QQRyDH6HFztPmiusytWqp2N5gbwjgtgxFgqCjJXKacNmq6T71R11");
        }

        [TestMethod]
        public void DogecoinTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.DogecoinNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "nfEWV9wKWiyzSNbDEVmkkEYCsfrKaUBMwA");
        }

        //[TestMethod]
        //public void DopeCoinMainNet()
        //{
        //    var mykey = "618AF711D8A040F6B97D41ABE455D92F";
        //    var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.DopeCoinNetwork.GetMainNet();
        //    var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
        //    string address = key.PubKey.GetAddress(MainNet).ToString();
        //    Assert.AreEqual(address, "ASpDsP7PcVH8qN3R2jo56x3QEGfRayj428");
        //}

        [TestMethod]
        public void DynamicMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.DynamicNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "DGBSm9CQakXGZQ22Cg8JVpwudoU2aiEnDU");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "MjzcvNc67EPvEQg42XQmUtfKCnKzfMosKjcrf8Hcjzoxm8rUGgRc");
        }

        [TestMethod]
        public void DynamicTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.DynamicNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "DGBSm9CQakXGZQ22Cg8JVpwudoU2aiEnDU");
        }

        [TestMethod]
        public void EarthCoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.EarthCoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "ecUSnyyZK7iU5hoUk77P3j6UHahTN1ugX7");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "ZixhESnxHgc4AKkGpfP6xzKmMybqHFmkWX5QyvMHWkrcZwxaQzBU");
        }

        [TestMethod]
        public void ECoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D9FF";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.ECoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "ZaP4fHS47XgvBojWGazDApEH7YQBSqW2M5THRU1C2H7fFE8fVtxC");
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "e2C2vL8uocGCD9B9fwSf6zPNnSDqt7vvt4");
        }

        /*[TestMethod]
        public void ExclusiveCoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.ExclusiveCoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "EUCFiU6GiHuu1hSHGw8FxCmGXKEriK66nX");
        }*/

        /*[TestMethod]
        public void ExclusiveCoinTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.ExclusiveCoinNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "EUCFiU6GiHuu1hSHGw8FxCmGXKEriK66nX");
        }*/

        //[TestMethod]
        //public void EspersMainNet()
        //{
        //    var mykey = "618AF711D8A040F6B97D41ABE455D92F";
        //    var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.EspersNetwork.GetMainNet();
        //    var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
        //    string address = key.PubKey.GetAddress(MainNet).ToString();
        //    Assert.AreEqual(address, "ASpDsP7PcVH8qN3R2jo56x3QEGfRayj428");
        //}

        [TestMethod]
        public void EverGreenCoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D9FF";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.EverGreenCoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "QrArvmAynTdohGxBe7hXCYeYRuYhC9W8k1hbQfZc43efdhPf6mPK");
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "EHFSpvYuuxvVxZx3EBnrVbKxeg2BynDFjx");
        }

        [TestMethod]
        public void FaircoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.FaircoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "fR9emCa8jUeDia5enwn21ye3YbDLkHvzxx");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "a27xNmWkdzSL7MmovpAtZLWjrr18x6KCqQKg6q3UViN38cN1n8F8");
        }

        [TestMethod]
        public void FaircoinTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.FaircoinNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "mrZJWwLk6N4EoWK3Bf77myzdcfLSAXWoEt");
        }

        [TestMethod]
        public void GambitMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D9FF";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.GambitNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "Rb51nZyUfEDVZs22QVfzBR7yg5YxNErmZDpFCwJ5WSRE3MNwUY7f");
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "GHwTkU3MTsEt3jeUMHTSvDgtoCJuXqT1ur");
        }

        [TestMethod]
        public void GlobalTokenMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.GlobalTokenNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "GUtGe1aiGCEH6s8iQ2nrNq8CfqXaDuHB5k");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "Rb51nZyUfEDVZs22QVfzBR7yg5YxNErmZDpFCwJ5WSQU5jxBcZrt");
        }

        [TestMethod]
        public void GlobalTokenTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.GlobalTokenNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "mrZJWwLk6N4EoWK3Bf77myzdcfLSAXWoEt");
        }

        [TestMethod]
        public void HappyCoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.HappyCoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "Hgu5bLUaPjcuZAYyUHnoqCwZZMJQRyvcUN");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "S2ouW44BBgxuVR4L4irfZwQSRP9usVfx3YgdtJLMysfbvjZe3HZ6");
        }

        /*[TestMethod]
        public void HappyCoinTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.HappyCoinNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "mrZJWwLk6N4EoWK3Bf77myzdcfLSAXWoEt");
        }*/

        [TestMethod]
        public void HoboNickelsMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.HoboNickelsNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "EsXrhaPZRUNmq8aNJMTaSL349pVoTveNAS");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "QzkVVvXsxcYwfnxxCC6Qzik2gLkM2ZmruTKiy7uhYXPcxR9MWCdt");
        }

        [TestMethod]
        public void InfluxcoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.InfluxcoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "iEWsexf9hjtMSc4Fxt7FQrYYx81wkhXoU8");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "b4BNNu33s4rHvysBoLw98ZB9btQho2DHyVgb21U8w4e26vhsJATY");
        }

        [TestMethod]
        public void InfluxcoinTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.InfluxcoinNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "mrZJWwLk6N4EoWK3Bf77myzdcfLSAXWoEt");
        }

        [TestMethod]
        public void IOCoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.IOCoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "bCkzx4Px3DmRuVsxMRL2vjGdrKcMdSV28wJiaTpERYPjPFsarqMG");
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "idrUe4xSQvMEG3CLzJSZtypLadGtUu2FGW");
        }

        [TestMethod]
        public void KomodoMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.KomodoNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "RLKYJQ93tARZ6QCcwG7s3b7WWwCKwLDdpk");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "UqptN7uGWcNX1FJvaAMdhFCh9dyJjSpm8wX7WvvAJxz8H2NA4iP2");
        }

        //[TestMethod]
        //public void KoreMainNet()
        //{
        //    var mykey = "618AF711D8A040F6B97D41ABE455D92F";
        //    var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.KoreNetwork.GetMainNet();
        //    var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
        //    string address = key.PubKey.GetAddress(MainNet).ToString();
        //    Assert.AreEqual(address, "ASpDsP7PcVH8qN3R2jo56x3QEGfRayj428");
        //}

        [TestMethod]
        public void LEOCoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D9FF";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.LEOCoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "NLK9D23gor4U8Uj8EpzLfb3GCX8c12tmyW7NtwfzhuqZr52fhQxz");
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "8FBQ4J5bGFyMh4rjrto5DiFAD7A3HdrJjD");
        }

        [TestMethod]
        public void LEOCoinTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.LEOCoinNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "TkMAD3vn9FCp11395n7mxLkEHxjzCY2V21");
        }

        [TestMethod]
        public void LitecoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.LitecoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "LWGJV6ZbMzs3HCXaeE83E5r4xt71Qm4Vj9");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "T4sKWBaUQmNsK39hwFcv9A4rARZUiRa3Be3YoUm2RDwau42XHyVx");
        }

        [TestMethod]
        public void LitecoinTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.LitecoinNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "mrZJWwLk6N4EoWK3Bf77myzdcfLSAXWoEt");
        }

        [TestMethod]
        public void TestNetAdvocacyLitecoin()
        {
            IServerCurrencyAdvocacy lAdvocacy = Pandora.Client.Crypto.Currencies.Controls.CurrencyControl.GetCurrencyControl().GetCryptoCurrency(2, null);
            lAdvocacy.RootSeed = "618AF711D8A040F6B97D41ABE455D92F";
            var lTestAddress = lAdvocacy.GetAddress(0);
            var lTestKey = lAdvocacy.GetPrivateKey(0);
            Assert.AreEqual(lTestAddress, "muCjBLWVszth2SGsvS4N9uz4FLEVJcGKja");
            Assert.AreEqual(lTestKey, "cPCNbwfuyQAhku7n1mvJXWauBk3uGQQk9Box1WCzASePb4y29STj");
            CurrencyTransaction lTxSend = new CurrencyTransaction();
            IPandoraServer lPandoraServer = GetFakeLitePandoraServer();
            //figures out the fee on the server side.
            lTxSend.TxFee = lPandoraServer.GetTransactionFee(lAdvocacy.Id, lTxSend);
            lTxSend.AddInput(600000000, "muCjBLWVszth2SGsvS4N9uz4FLEVJcGKja");
            lTxSend.AddOutput(580000000, "muZkczUieaExLSvmRS649ZTWZn5waA5puT");
            lTxSend.AddOutput(20000000 - lTxSend.TxFee, "muCjBLWVszth2SGsvS4N9uz4FLEVJcGKja");
            // if the user does not set a fee the fee will be set also
            // if not all money is spent from the output address regardless of the currency
            // The IPandoraServer will send change back to the last input address.
            var lTxData = lPandoraServer.CreateTransaction(lAdvocacy.Id, lTxSend);
            // Should return this info....
            //'020000000195253ae6ad09a47c02cb0f9d6ffba70f24a9b6c6eb6731eb726d24aa94c7a1430100000000ffffffff020076b010000000001976a914fbb057bbc267c9deddd75e8dfad6096ad7470c4a88acc01f2e01000000001976a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac00000000'
            //'[{"txid":"43a1c794aa246d72eb3167ebc6b6a9240fa7fb6f9d0fcb027ca409ade63a2595","vout":1,"scriptPubKey":"76a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac"}]' '["cPCNbwfugArLG7NBDjBXxRoUxZsc5BcDthAdGJ8tgPh4j9cY5Cgx"]'
            //
            // This call must remember that change is sent to the last input address
            var lSignedTxData = lAdvocacy.SignTransaction(lTxData, lTxSend);
            // "020000000195253ae6ad09a47c02cb0f9d6ffba70f24a9b6c6eb6731eb726d24aa94c7a143010000006a473044022038063fa5d091fdabd18999db757aaf9a1c2eccb105727de52346b75f8355532102205e18f279ebe1be5b1b385e7d6e5bdfe18f498be96845c2582029c8c4b937fea6012102d7273d559e6196e34452705b0158b7d8af92ce43151b9782ae34c0b9e0025851ffffffff020076b010000000001976a914fbb057bbc267c9deddd75e8dfad6096ad7470c4a88acc01f2e01000000001976a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac00000000"
            Assert.AreEqual(lSignedTxData, "020000000181ef1f8e49daa2a8659767239cc8eb94fc0d59863f4fcc38946068750fe9c7b9010000006b483045022100fb75f96efc52b874027d356afe50bb46c13ced90c3d738b5543df66ff02c12740220368975551b5e2a0ec0ea3b566bcfe8483e5ef9d17f4cf910d0051ff75b01c553012102af10211c709fa0bddda95b59aa9f344d8adf802c8ec358009d07de98a85f8897ffffffff0200199222000000001976a9149a1943aaa58e63d72a911efdc99102fb826c508f88acc01f2e01000000001976a914961f533c1872ba46e1c5332e8ab8211a6e15c99188ac00000000");
            //lPandoraServer.SendTransaction(lSignedTxData);
        }

        private IPandoraServer GetFakeLitePandoraServer()
        {
            var lFake = new FakePandoraServer();
            lFake.FakeTransactionFee = 200000;
            lFake.FakeTransaction =
                "{\"hex\":\"020000000181ef1f8e49daa2a8659767239cc8eb94fc0d59863f4fcc38946068750fe9c7b90100000000ffffffff0200199222000000001976a9149a1943aaa58e63d72a911efdc99102fb826c508f88acc01f2e01000000001976a914961f533c1872ba46e1c5332e8ab8211a6e15c99188ac00000000\"," +
                 "\"inputs\":[{\"txid\":\"b9c7e90f7568609438cc4f3f86590dfc94ebc89c23679765a8a2da498e1fef81\"," +
                              "\"vout\":1,\"scriptPubKey\":\"76a914961f533c1872ba46e1c5332e8ab8211a6e15c99188ac\"}]}";
            return lFake;
        }

        [TestMethod]
        public void LoMoCoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D9FF";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.LoMoCoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "T4sKWBaUQmNsK39hwFcv9A4rARZUiRa3Be3YoUm2RDxLrfLAkHtr");
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "LKKVbZ2EZfseE53LbUndmUQm6EtLcwKaMU");
        }

        [TestMethod]
        public void MasterSwiscoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.MasterSwiscoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "sVHkJTWn2tYWFaGFXXmaZjoeRiwe5Y37wK");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "eTWsXcKjtbvTLtArX61gSZMMKt2hzeT1if1atTSKE4yPaYGmb5Hc");
        }

        [TestMethod]
        public void MasterSwiscoinTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.MasterSwiscoinNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "mrZJWwLk6N4EoWK3Bf77myzdcfLSAXWoEt");
        }

        [TestMethod]
        public void MemeticMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.MemeticNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "PKdXNrecLG7B1EWBpATGcxkaNQucPuDKQ6");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "PfXpMUHnPEJhu8r3DWYNq9teSRwUWoLKSUiYw2nr8DcDRSMtCePU");
        }

        [TestMethod]
        public void MemeticTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.MemeticNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "PKdXNrecLG7B1EWBpATGcxkaNQucPuDKQ6");
        }

        [TestMethod]
        public void MergecoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.MergecoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "MJwWTKAAnMnnv4okh4ngCLPeDtctjgCnMi");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "TN2aeWJGm5D9G5BF3QQhjWFpfHxnPG7VWXHovPTDQBT1TiMJjHKi");
        }

        [TestMethod]
        public void MonacoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.MonacoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "MJwWTKAAnMnnv4okh4ngCLPeDtctjgCnMi");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "T4sKWBaUQmNsK39hwFcv9A4rARZUiRa3Be3YoUm2RDwau42XHyVx");
        }

        [TestMethod]
        public void MonacoinTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.MonacoinNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "mrZJWwLk6N4EoWK3Bf77myzdcfLSAXWoEt");
        }

        [TestMethod]
        public void MooncoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.MooncoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "2Q4ABD9dQt1cUhFgYM8hQSbfeBWZKsAw6s");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "LQmwmvMzXr9gTkZ93qrjKKovxsX89bNKrm1gd3AmKh2ZEA25f9nY");
        }

        [TestMethod]
        public void NamecoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.NamecoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "N7ciRXkkCiiYYw5vjuTKAawDUu8n9spB89");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "TfBqnq257P3RD7Cn9ZCVKrSoAAN646ewqQY53J9QP8xS2NjmuzMn");
        }

        [TestMethod]
        public void NeosCoinCoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.NeosCoinCoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "NWxKQe42uuBRNNE1mKndeiD17QPiwSKggt");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "TDSx5LwNavJ1HZAUVL1owLALQrm8YqqmM5fgMw77uhhJBP8piGsk");
        }

        [TestMethod]
        public void NeosCoinCoinTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.NeosCoinCoinNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "EUCFiU6GiHuu1hSHGw8FxCmGXKEriK66nX");
        }

        [TestMethod]
        public void NovacoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.NovacoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "4QkB6ke4xnKzZrx7fSoHq4xbnhoGuBff9k");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "M9g6djAVQcjNLLcypDqCJCHND3XPKgixfy8LRJuEn5o7dozsxJo2");
        }

        [TestMethod]
        public void NovacoinTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.NovacoinNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "mrZJWwLk6N4EoWK3Bf77myzdcfLSAXWoEt");
        }

        [TestMethod]
        public void OmniMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.OmniNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "1C3MDtFmHLcz2PqRU68jx4nJkfjjG3Usog");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "Ky344SHJ1PQGYCWqPcg3voXUDZvAeLZ9NS9Hwg8UrFmRPASQ7KKZ");
        }

        [TestMethod]
        public void OmniTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.OmniNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "mrZJWwLk6N4EoWK3Bf77myzdcfLSAXWoEt");
        }

        [TestMethod]
        public void PhoreMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.PhoreNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "PKdXNrecLG7B1EWBpATGcxkaNQucPuDKQ6");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "YPk25zYriJMpPfdMqyq4oRUP84nxmVLD3YUEwqES6T5D2y7KVMu1");
        }

        [TestMethod]
        public void PIVXMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.PIVXNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "DGBSm9CQakXGZQ22Cg8JVpwudoU2aiEnDU");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "YPk25zYriJMpPfdMqyq4oRUP84nxmVLD3YUEwqES6T5D2y7KVMu1");
        }

        [TestMethod]
        public void PIVXTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.PIVXNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "y82C5ygoyR2mheDTsQS3MVcfEnYr32xJ5u");
        }

        [TestMethod]
        public void PrimeCoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.PrimeCoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "ASpDsP7PcVH8qN3R2jo56x3QEGfRayj428");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "PNNZD9Zz2vURx6pW7MkbEohfwZYAqxns7bUHp86f9G6nrn2zDyL5");
        }

        [TestMethod]
        public void PuraMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.PuraNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "PKdXNrecLG7B1EWBpATGcxkaNQucPuDKQ6");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "PDnvdzD5rmZHyaojZHMhSdcBh8LX1YX8x9rAFfkZenM5aSpXSLWk");
        }

        [TestMethod]
        public void PutinCoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.PutinCoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "PKdXNrecLG7B1EWBpATGcxkaNQucPuDKQ6");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "U6vjWK6mdqnq8fF5onPAiNjFuTy3ZMU8KjQTifBgraDZsNLXHKnn");
        }

        /*[TestMethod]
        public void PutinCoinTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.PutinCoinNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "mrZJWwLk6N4EoWK3Bf77myzdcfLSAXWoEt");
        }*/

        //[TestMethod]
        //public void ReddCoinMainNet()
        //{
        //    var mykey = "618AF711D8A040F6B97D41ABE455D92F";
        //    var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.ReddCoinNetwork.GetMainNet();
        //    var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
        //    string address = key.PubKey.GetAddress(MainNet).ToString();
        //    Assert.AreEqual(address, "Rjf9HWSLbLtRuqLhxgTBXiPJ9STGZwixWN");
        //    string lKey = key.GetBitcoinSecret(MainNet).ToString();
        //    Assert.AreEqual(lKey, "");
        //}

        [TestMethod]
        public void SyndicateMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.SyndicateNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "SYLMFj2v1hpBYhct1X7pVxvsQSyA5WQV8n");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "PfXpMUHnPEJhu8r3DWYNq9teSRwUWoLKSUiYw2nr8DcDRSMtCePU");
        }

        [TestMethod]
        public void SyndicateTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.SyndicateNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "gDprjRAi9qZyMSMpqnSezEBcobjEDHF7BC");
        }

        [TestMethod]
        public void TagCoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.TagCoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "TM1ZDwdVS4jwBZu44MnTUDUSfTV3VLjDP1");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "Vaj3DvhmPNxCsqNmLYL6g7g8PoyZuYBPx9dmKCedmMkgggQnzUn3");
        }

        /*[TestMethod]
        public void TagCoinTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.TagCoinNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "mrZJWwLk6N4EoWK3Bf77myzdcfLSAXWoEt");
        }*/

        [TestMethod]
        public void TaoMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.TaoNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "TkMAD3vn9FCp11395n7mxLkEHxjzCY2V21");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "CGtGL3YWuPm7sHptdg9cXkc9GyUFEaTLuTkWr47ZKNaQjx39H4PM");
        }

        [TestMethod]
        public void TaoTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.TaoNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "tHxxGg7MTFUFtSYRaNSDXzMDgjTXVNtHm6");
        }

        [TestMethod]
        public void TransferCoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.TransferCoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "TkMAD3vn9FCp11395n7mxLkEHxjzCY2V21");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "PfXpMUHnPEJhu8r3DWYNq9teSRwUWoLKSUiYw2nr8DcDRSMtCePU");
        }

        [TestMethod]
        public void TransferCoinTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.TransferCoinNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "gDprjRAi9qZyMSMpqnSezEBcobjEDHF7BC");
        }

        [TestMethod]
        public void TrollCoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.TrollCoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "TkMAD3vn9FCp11395n7mxLkEHxjzCY2V21");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "PfXpMUHnPEJhu8r3DWYNq9teSRwUWoLKSUiYw2nr8DcDRSMtCePU");
        }

        [TestMethod]
        public void TrollCoinTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.TrollCoinNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "mrZJWwLk6N4EoWK3Bf77myzdcfLSAXWoEt");
        }

        [TestMethod]
        public void VeriCoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D9FF";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.VeriCoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "WKdC5jWGG9XtkRSc6vJZez9Zdyyq5dY2mMkR7UP7DkY13wo6FgkB");
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "VAkmFwaaBe4vDc7F8i7eSEQ4wLZ6EwPoMY");
        }

        [TestMethod]
        public void VertcoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.VertcoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "Vm3B8bRDh9XC6AjaCsnNNy7ASV2hmnn1Xm");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "Ky344SHJ1PQGYCWqPcg3voXUDZvAeLZ9NS9Hwg8UrFmRPASQ7KKZ");
        }

        [TestMethod]
        public void VertcoinTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.VertcoinNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "Wy3z5vK5pgupYU9qH8nKqLvXKzoXr4AraJ");
        }

        [TestMethod]
        public void ViacoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.ViacoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "Vm3B8bRDh9XC6AjaCsnNNy7ASV2hmnn1Xm");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "WUCpetsASJT2iwTNezhTTAF3tRBUv3okvoNYfvjCiEGxNfZ2bBJp");
        }

        [TestMethod]
        public void ViacoinTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.ViacoinNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "tHxxGg7MTFUFtSYRaNSDXzMDgjTXVNtHm6");
        }

        [TestMethod]
        public void WorldCoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D9FF";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.WorldCoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "WmN5oDaxncHJfyUum9VF3WS2PHanatMDFgconqRPhBo8twQSQy5k");
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "WNmaDGUSKBTYfuXWCy7btcDRprKvMecHTc");
        }

        [TestMethod]
        public void YashCoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.YashCoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "YaQQ2MWEfQmKpCiBNp7bmr1fr1qJjePEan");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "XWGEf2PTfNrzYZYkXXTi2NuTdTb3kyhr4tjTb79s9aYwLytnCXXZ");
        }

        /*[TestMethod]
        public void YashCoinTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.YashCoinNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "mrZJWwLk6N4EoWK3Bf77myzdcfLSAXWoEt");
        }*/

        [TestMethod]
        public void ZcashMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.ZcashNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "t1UuxEDfuFfQad2tKQWws5stE1KvoycEq5u");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "Ky344SHJ1PQGYCWqPcg3voXUDZvAeLZ9NS9Hwg8UrFmRPASQ7KKZ");
        }

        [TestMethod]
        public void ZcashTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.ZcashNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "tmLkhZ4AJeKv5mH5m5FFbwYYycK1dVFUKxa");
        }

        [TestMethod]
        public void ZCoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.ZCoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "aBkoxnhPW8cq5wGXUVSsiM6pN2s5aj723d");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "Y6akwfq4MzXYSdbpjq3HD5HQdCPf6enkifDypvYF7VZnUJim9Lup");
        }

        [TestMethod]
        public void ZCoinTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.ZCoinNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "TM1ZDwdVS4jwBZu44MnTUDUSfTV3VLjDP1");
        }

        //[TestMethod]
        //public void ZenniesMainNet()
        //{
        //    var mykey = "618AF711D8A040F6B97D41ABE455D92F";
        //    var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.ZenniesNetwork.GetMainNet();
        //    var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
        //    string address = key.PubKey.GetAddress(MainNet).ToString();
        //    Assert.AreEqual(address, "ASpDsP7PcVH8qN3R2jo56x3QEGfRayj428");
        //}

        [TestMethod]
        public void ZurcoinMainNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var MainNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.ZurcoinNetwork.GetMainNet();
            var key = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string address = key.PubKey.GetAddress(MainNet).ToString();
            Assert.AreEqual(address, "UxMyANpeGnbSTJTQA37jQiZbBUWpHo7CAh");
            string lKey = key.GetCoinSecret(MainNet).ToString();
            Assert.AreEqual(lKey, "WB3ZWa9N5zckmuRqYqufrp45PYnBFDGJbv8HZ231jGmXp17wR3X5");
        }

        /*[TestMethod]
        public void ZurcoinTestNet()
        {
            var mykey = "618AF711D8A040F6B97D41ABE455D92F";
            var TestNet = Pandora.Client.Crypto.Currencies.Bitcoin.Alt.ZurcoinNetwork.GetMainNet().Networks[1];
            var testKey = new Pandora.Client.Crypto.Currencies.CCKey(Encoding.ASCII.GetBytes(mykey));
            string testAddress = testKey.PubKey.GetAddress(TestNet).ToString();
            Assert.AreEqual(testAddress, "q5G8Poj3momFLyRjP1mfezAvehPytj7BPZ");
        }*/
    }
}