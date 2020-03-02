//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Newtonsoft.Json.Linq;
//using System;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Security.Cryptography;
//using System.Text;

//namespace Pandora.Client.Exchange.Test
//{
//    [TestClass]
//    public class ProxyTests
//    {
//        [TestMethod]
//        public void TestWebProxyWorking()
//        {
//            ICredentials lCredentials = new NetworkCredential("ProxyUser", "CxSUPyHSmr2N3pRtCk");
//            WebProxy lProxy = new WebProxy("http://proxy.davincicodes.net:808/", false, null, lCredentials);

//            var lProxyIP = GetMyIPResponse(lProxy);
//            var lNormalIP = GetMyIPResponse();

//            Assert.AreNotEqual(lNormalIP, lProxyIP);
//            Assert.AreEqual(lProxyIP, "192.119.10.164");
//        }

//        [TestMethod]
//        public void TestProxyObjectWorking()
//        {
//            WebProxy lProxy = PandoraProxy.GetWebProxy();

//            var lProxyIP = GetMyIPResponse(lProxy);
//            var lNormalIP = GetMyIPResponse();

//            Assert.AreNotEqual(lNormalIP, lProxyIP);
//            Assert.AreEqual(lProxyIP, "192.119.10.164");
//        }

//        private string GetMyIPResponse(WebProxy aProxy = null)
//        {
//            HttpWebRequest lTestRequest = (HttpWebRequest)WebRequest.Create("https://api.ipify.org?format=json");
//            lTestRequest.Method = "GET";
//            if (aProxy != null)
//            {
//                lTestRequest.Proxy = aProxy;
//            }

//            var lGetIPResponse = (HttpWebResponse)lTestRequest.GetResponse();
//            string lJsonReponse;
//            using (var lStream = lGetIPResponse.GetResponseStream())
//            {
//                var lReader = new StreamReader(lStream, Encoding.UTF8);
//                lJsonReponse = lReader.ReadToEnd();
//            }

//            var lMyIPObject = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(lJsonReponse);

//            return lMyIPObject["ip"].Value<string>(); ;
//        }

//        [TestMethod]
//        public void TestBittrexWithdrawWithProxy()
//        {
//            var lExchanger = PandoraExchanger.GetInstance();
//            lExchanger.SetCredentials("ad452277eedc4f76a25190ac2723338d", "e1959228b4a944d099e11124a1a9b1c3");
//            var lMarkets = lExchanger.GetMarketCoins("BTC");
//            var lReddmarket = lMarkets.Where(lMarket => lMarket.MarketName == "BTC-RDD").FirstOrDefault();
//            Assert.IsNotNull(lReddmarket);
//            var lOrder = new MarketOrder()
//            {
//                Rate = 1,
//                SentQuantity = 10,
//                Status = OrderStatus.Completed
//            };
//            //Any other address
//            Assert.ThrowsException<Exception>(() => lExchanger.WithdrawOrder(lReddmarket, lOrder, "Rf5Wo9ynHFFUsS2kycuBDPKGXkgXNgRHdW", (decimal)0.1, false));
//            //Proxy address
//            var lResult = lExchanger.WithdrawOrder(lReddmarket, lOrder, "Rf5Wo9ynHFFUsS2kycuBDPKGXkgXNgRHdW", (decimal)0.1);
//            Assert.IsTrue(lResult);
//        }

//        [TestMethod]
//        public void TestPlaceOrderWithProxy()
//        {
//            var lExchanger = PandoraExchanger.GetInstance();
//            lExchanger.SetCredentials("ad452277eedc4f76a25190ac2723338d", "e1959228b4a944d099e11124a1a9b1c3");
//            var lMarkets = lExchanger.GetMarketCoins("RDD");
//            var lReddmarket = lMarkets.Where(lMarket => lMarket.MarketName == "BTC-RDD").FirstOrDefault();
//            Assert.IsNotNull(lReddmarket);
//            var lOrder = new MarketOrder()
//            {
//                Rate = 1,
//                SentQuantity = 1500,
//                Status = OrderStatus.Waiting,
//                Market = "BTC-RDD",
//                BaseTicker = "RDD"
//            };
//            //Any other address
//            Assert.ThrowsException<Exception>(() => lExchanger.PlaceOrder(lOrder, lReddmarket, false));
//            //Proxy address
//            var lResult = lExchanger.PlaceOrder(lOrder, lReddmarket);
//            Assert.IsTrue(lResult);
//        }
//    }
//}