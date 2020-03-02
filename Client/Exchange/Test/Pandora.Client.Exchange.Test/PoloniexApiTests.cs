using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pandora.Client.Exchange.Exchanges;
using Pandora.Client.Exchange.JKrof.Poloniex.Net;
using Pandora.Client.Exchange.Objects;

namespace Pandora.Client.Exchange.Test
{
    [TestClass]
    public class PoloniexApiTests
    {
        [TestMethod]
        public void GetTicker()
        {
            var lExchange = new PoloniexExchange();
            lExchange.SetCredentials("DWZIXV0S-9HI7DJWA-BX5W5KYO-GOFLNX6Q", "a62a7e513f14da8b50d4284e4c89dbaca5c017c8fa66b9c27f863bdae03fe0de64d3092ab8c2100bdd0c25b6deebfe89e00ffb7c04b9875f8efa7a65a4b87553");
            var lMarkets = lExchange.GetMarketCoins("Bitecoin", "BTC");
            var ltcmarket = lMarkets.Where(lMarket => lMarket.DestinationCurrencyInfo.Ticker == "LTC").FirstOrDefault();
            var lDepositAddress = lExchange.GetDepositAddress(ltcmarket);
            var lTxFee = lExchange.GetTransactionsFee("Litecoin", "LTC");
            var lBalance = lExchange.GetBalance(ltcmarket);
            //var lOrder = lExchange.GetOrderStatus("335840205668");
            var lOrdergood = lExchange.GetOrderStatus("335835568310");
            var lTestOrder = new UserTradeOrder
            {
                Rate = 0.006M,
                SentQuantity = 0.0009M,
                ID = lOrdergood.ID

            };
            //lExchange.PlaceOrder(lTestOrder, ltcmarket);

            //lExchange.WithdrawOrder(ltcmarket, lTestOrder, "MCD9iHAqv9oLHzmyVwMx631dYJDqv93MxB", lTxFee);
            lExchange.RefundOrder(ltcmarket, lTestOrder, "32GTiTeC8Jo7gihJfAYRTPU151MHRoQAzD");
        }
    }
}
