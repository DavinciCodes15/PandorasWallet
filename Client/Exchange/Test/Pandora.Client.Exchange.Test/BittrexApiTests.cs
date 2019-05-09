using Bittrex.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Pandora.Client.Exchange.Test
{
    [TestClass]
    public class BittrexApiTests
    {
        private BittrexClient FClient;

        [TestInitialize()]
        public void Init()
        {
            FClient = new Bittrex.Net.BittrexClient(new Bittrex.Net.Objects.BittrexClientOptions() { ApiCredentials = new JKrof.Authentication.ApiCredentials("ad452277eedc4f76a25190ac2723338d", "e1959228b4a944d099e11124a1a9b1c3") });
        }

        [TestMethod]
        public void PlaceGetOrderTest()
        {
            decimal lAmountDogeToBuy = Convert.ToDecimal(0.00055564) / Convert.ToDecimal(0.00000050);
            //This should be OK
            JKrof.Objects.WebCallResult<Bittrex.Net.Objects.BittrexGuid> lOrder = FClient.PlaceOrder(Bittrex.Net.Objects.OrderSide.Buy, "BTC-DOGE", lAmountDogeToBuy, Convert.ToDecimal(0.00000050));
            Assert.IsTrue(lOrder.Success && lOrder.Data != null);
            //This should trow an error
            JKrof.Objects.WebCallResult<Bittrex.Net.Objects.BittrexGuid> lOrder2 = FClient.PlaceOrder(Bittrex.Net.Objects.OrderSide.Buy, "BTC-DOGE", (decimal)0.00000001, Convert.ToDecimal(0.00000050));
            Assert.IsTrue(!lOrder2.Success && lOrder2.Data == null);
            //This should return first order made
            JKrof.Objects.WebCallResult<Bittrex.Net.Objects.BittrexAccountOrder> lGetOrder = FClient.GetOrder(lOrder.Data.Uuid);
            Assert.IsTrue(lOrder.Data.Uuid == lGetOrder.Data.OrderUuid);
        }

        [TestMethod]
        public void GetBalanceTest()
        {
            JKrof.Objects.WebCallResult<Bittrex.Net.Objects.BittrexMarket[]> lResult = FClient.GetMarkets();
            Assert.IsTrue(lResult.Data.Length > 0);
        }

        [TestMethod]
        public void WithdrawCoinTest()
        {
            Assert.Fail();
            //PLEASE VERIFY THIS ADDRESS BEFORE CONTINUING
            string lAddress = "3PG82uZhDwSSK3kuN6XyjQmNchtrXdG6jb";

            JKrof.Objects.WebCallResult<Bittrex.Net.Objects.BittrexGuid> lWithdraw = FClient.Withdraw("BTC", (decimal)0.003, lAddress);

            Assert.IsTrue(lWithdraw.Success && lWithdraw.Data.Uuid != Guid.Empty);
        }

        [TestMethod]
        public void GetMarkets()
        {
            JKrof.Objects.WebCallResult<Bittrex.Net.Objects.BittrexMarket[]> lResult = FClient.GetMarkets();
            Assert.IsTrue(lResult.Data.Length > 0);
        }
    }
}