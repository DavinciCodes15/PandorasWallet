﻿using Bittrex.Net;
using CryptoExchange.Net.Authentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Pandora.Client.Exchange.Test
{
    [TestClass]
    public class BittrexApiTests
    {
        private BittrexClient FClient;

        [TestMethod]
        public void TestSocket()
        {
            var lSocketClient = new BittrexSocketClient();
            var lResult = lSocketClient.SubscribeToSymbolTickerUpdatesAsync((lBittrexSummaries) =>
            {
            });
            while (true) System.Threading.Thread.Sleep(1000);
        }

        [TestInitialize()]
        public void Init()
        {
            FClient = new Bittrex.Net.BittrexClient(new Bittrex.Net.Objects.BittrexClientOptions() { ApiCredentials = new ApiCredentials("ca427b6783a246e9a3ad327ce3a6e89c", "4d8cc6a23ffc4d65b3c2aca1d92265a9") });
        }

        [TestMethod]
        public void PlaceGetOrderTest()
        {
            //decimal lAmountDogeToBuy = Convert.ToDecimal(0.00055564) / Convert.ToDecimal(0.00000050);
            ////This should be O
            //JKrof.Objects.WebCallResult<Bittrex.Net.Objects.BittrexGuid> lOrder = FClient.PlaceOrder(Bittrex.Net.Objects.OrderSide.Buy, "BTC-DOGE", lAmountDogeToBuy, Convert.ToDecimal(0.00000050));
            //Assert.IsTrue(lOrder.Success && lOrder.Data != null);
            ////This should trow an error
            //JKrof.Objects.WebCallResult<Bittrex.Net.Objects.BittrexGuid> lOrder2 = FClient.PlaceOrder(Bittrex.Net.Objects.OrderSide.Buy, "BTC-DOGE", (decimal)0.00000001, Convert.ToDecimal(0.00000050));
            //Assert.IsTrue(!lOrder2.Success && lOrder2.Data == null);
            ////This should return first order made
            var lGetOrder = FClient.GetOpenOrdersAsync(/*new Guid("4a67c489-bf81-40c5-8d04-cea70f54fe32")*/);
            //Assert.IsTrue(lOrder.Data.Uuid == lGetOrder.Data.OrderUuid);
        }

        [TestMethod]
        public void GetBalanceTest()
        {
            var lResult = FClient.GetSymbolsAsync().Result;
            Assert.IsTrue(lResult.Data.Count() > 0);
        }

        [TestMethod]
        public void WithdrawCoinTest()
        {
            Assert.Fail();
            //PLEASE VERIFY THIS ADDRESS BEFORE CONTINUING
            string lAddress = "3PG82uZhDwSSK3kuN6XyjQmNchtrXdG6jb";

            var lWithdraw = FClient.WithdrawAsync("BTC", (decimal) 0.003, lAddress).Result;

            Assert.IsTrue(lWithdraw.Success && lWithdraw.Data.Id != string.Empty);
        }

        [TestMethod]
        public void GetMarkets()
        {
            var lResult = FClient.GetSymbolsAsync().Result;
            Assert.IsTrue(lResult.Data.Count() > 0);
        }
    }
}