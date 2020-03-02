using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.JKrof.Poloniex.Net.Objects
{
    public class PoloniexOrderPlaceResult
    {
        public long orderNumber { get; set; }
        public PoloniexOrderTradeLite[] resultingTrades { get; set; }
        public decimal fee { get; set; }
        public string clientOrderId { get; set; }
        public string currencyPair { get; set; }
    }
}
