//using System;
//using System.Linq;
//using System.Threading;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace Pandora.Client.Exchange.Test
//{
//    [TestClass]
//    public class ExchangeEntityTests
//    {
//        private PandoraExchangeEntity FExchangeEntity;
//        private bool FMarketPriceUpdateTrigger;
//        private int FOrderCancelledId;

//        [TestInitialize]
//        public void Initialize()
//        {
//            var lProfiler = PandoraExchangeProfileManager.GetProfiler();
//            var lNewProfile = lProfiler.AddNewExchangeProfile(10, "TestProfile");
//            FExchangeEntity = lProfiler.GetProfileExchangeEntity(lNewProfile);
//            FExchangeEntity.SetCredentials("ad452277eedc4f76a25190ac2723338d", "e1959228b4a944d099e11124a1a9b1c3");
//        }

//        [TestMethod]
//        public void TestGetMarkets()
//        {
//            var lBTCMarkets = FExchangeEntity.GetExchangeMarkets("BTC");
//            Assert.IsTrue(lBTCMarkets.Length > 0);
//        }

//        [TestMethod]
//        public void TestProcessMessage()
//        {
//            var lBTCMarkets = FExchangeEntity.GetExchangeMarkets("BTC");
//            Assert.IsTrue(lBTCMarkets.Length > 0);
//        }

//        [TestMethod]
//        public void TestPlaceOrder()
//        {
//            var lMarkets = FExchangeEntity.GetExchangeMarkets("LTC");
//            var lReddmarket = lMarkets.Where(lMarket => lMarket.MarketName == "BTC-LTC").FirstOrDefault();
//            Assert.IsNotNull(lReddmarket);
//            var lOrder = new MarketOrder()
//            {
//                Rate = 1,
//                SentQuantity = 0.051M,
//                Status = OrderStatus.Waiting,
//                Market = "BTC-LTC",
//                BaseTicker = "LTC",
//                MarketInstance = lReddmarket
//            };

//            var lUid = FExchangeEntity.PlaceOrder(lOrder);
//            Assert.IsFalse(string.IsNullOrEmpty(lUid));
//        }

//        [TestMethod]
//        public void TestWithdrawOrder()
//        {
//            var lMarkets = FExchangeEntity.GetExchangeMarkets("BTC");
//            var lBTCMarket = lMarkets.Where(lMarket => lMarket.MarketName == "BTC-RDD").FirstOrDefault();
//            Assert.IsNotNull(lBTCMarket);
//            var lOrder = new MarketOrder()
//            {
//                Rate = (decimal)0.00000027,
//                SentQuantity = (decimal)0.0002,
//                Status = OrderStatus.Waiting,
//                Market = "BTC-RDD",
//                BaseTicker = "BTC",
//                MarketInstance = lBTCMarket,
//                ID = "asdsdawda"
//            };

//            FExchangeEntity.WithdrawOrder(lOrder, "Rkz4LvhCfembr3HzXda4K3oLn7EXQSUL3E");
//        }

//        [TestMethod]
//        public void TestMarketUpdateEvent()
//        {
//            FMarketPriceUpdateTrigger = true;
//            FExchangeEntity.OnMarketPriceUpdating += FExchangeEntity_OnMarketPriceUpdating;
//            int lTimeoutCounter = 0;
//            while (FMarketPriceUpdateTrigger)
//            {
//                if (lTimeoutCounter > 20)
//                    break;
//                Thread.Sleep(1000);
//                lTimeoutCounter++;
//            }
//            Assert.IsFalse(FMarketPriceUpdateTrigger);
//        }

//        private void FExchangeEntity_OnMarketPriceUpdating()
//        {
//            FMarketPriceUpdateTrigger = false;
//        }

//        [TestMethod]
//        public void TestOrderTracking()
//        {
//            FOrderCancelledId = -1;
//            var lMarkets = FExchangeEntity.GetExchangeMarkets("RDD");
//            var lReddmarket = lMarkets.Where(lMarket => lMarket.MarketName == "BTC-RDD").FirstOrDefault();
//            Assert.IsNotNull(lReddmarket);
//            var lOrder = new MarketOrder()
//            {
//                Rate = 1,
//                SentQuantity = 1500,
//                Status = OrderStatus.Waiting,
//                Market = "BTC-RDD",
//                BaseTicker = "RDD",
//                MarketInstance = lReddmarket,
//                InternalID = 9999
//            };

//            var lUid = FExchangeEntity.PlaceOrder(lOrder);
//            lOrder.ID = lUid;
//            FExchangeEntity.OnOrderCancelled += FExchangeEntity_OnOrderCancelled;
//            FExchangeEntity.AddOrderToTrack(lOrder);
//            Thread.Sleep(3000);
//            FExchangeEntity.CancelOrder(lUid);
//            int lTimeoutCounter = 0;
//            while (FOrderCancelledId < 0)
//            {
//                if (lTimeoutCounter > 10)
//                    break;
//                Thread.Sleep(1000);
//                lTimeoutCounter++;
//            }

//            Assert.AreEqual(FOrderCancelledId, 9999);
//        }

//        private void FExchangeEntity_OnOrderCancelled(int[] aInternalID)
//        {
//            FOrderCancelledId = aInternalID.First();
//        }
//    }
//}