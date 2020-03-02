using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pandora.Client.Exchange.Exchanges;
using Pandora.Client.Exchange.Objects;

namespace Pandora.Client.Exchange.Test
{
    [TestClass]
    public class BitfinexTests
    {
        [TestMethod]
        public void BitFinex()
        {
            var lExchange = new BitfinexExchange();
            lExchange.SetCredentials("5PrOsHQoroWXpx2ZuSrnzEcsHkd1XNQG9XuQmLohlPl", "IZXRRMk9KXS8jBMYEbm61MGI1uK3T4OCyKRwDphkHhB");
            var lMarkets = lExchange.GetMarketCoins("DASH", "DASH");
            var ltcmarket = lMarkets.Where(lMarket => lMarket.DestinationCurrencyInfo.Ticker == "BTC").FirstOrDefault();
            var lDepositAddress = lExchange.GetDepositAddress(ltcmarket);
            var lTxFee = lExchange.GetTransactionsFee("DASH", "DASH");
            var lBalance = lExchange.GetBalance(ltcmarket);
            var lOrder = lExchange.GetOrderStatus("40154406110");
            var lTestOrder = new UserTradeOrder
            {
                Rate = 0.006M,
                SentQuantity = 0.0009M,
                ID = lOrder.ID

            };

            //lExchange.WithdrawOrder(ltcmarket, lTestOrder, "32GTiTeC8Jo7gihJfAYRTPU151MHRoQAzD", lTxFee);
            lExchange.RefundOrder(ltcmarket, lTestOrder, "MCD9iHAqv9oLHzmyVwMx631dYJDqv93MxB");
            lExchange.PlaceOrder(lTestOrder, ltcmarket);
            //var lExchanges = lExchange.GetMarketCoins("BTC");
        }
    }
}
