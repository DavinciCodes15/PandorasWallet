using System;
using System.Collections.Generic;
using Bittrex.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pandora.Client.Exchange;

namespace Pandora.Client.Exchange.Test
{
    [TestClass]
    public class UnitTest1
    {
        private PandoraExchanger.ExchangeMarket[] asd;

        [TestMethod]
        public void TestMethod1()
        {
            var lInstance = PandoraExchanger.GetInstance();
            lInstance.OnMarketPricesChanging += LInstance_OnMarketPricesChanging;
            asd = lInstance.GetMarketCoins("LTC");

            lInstance.StartMarketPriceUpdating();

            while (true)
            {
                System.Threading.Thread.Sleep(10000);
            }
        }

        private void LInstance_OnMarketPricesChanging()
        {
            System.Diagnostics.Debug.WriteLine(asd[0].MarketName + "|" + asd[0].Price);
            System.Diagnostics.Debug.WriteLine(asd[1].MarketName + "|" + asd[1].Price);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var lInstance = new ExchangeTxDBHandler("C:\\Users\\LValles\\AppData\\Local\\PandorasWallet\\", "36774389AC9EF1A75B52E607D71061B8");
            Guid lID = Guid.NewGuid();
            //lInstance.WriteOrderLog((lID, "Test Message 1");
            //lInstance.WriteOrderLog(lID, "Test Message 2");
            List<string> aMessages;
            //lInstance.ReadOrderLogs(lID, out aMessages);
        }

        [TestMethod]
        public void GetMoney()
        {
            using (var lClient = new BittrexClient())
            {
                lClient.SetApiCredentials("ad452277eedc4f76a25190ac2723338d", "e1959228b4a944d099e11124a1a9b1c3");

                var lResult = lClient.GetOpenOrders("BTC-LTC");
            }
        }
    }
}