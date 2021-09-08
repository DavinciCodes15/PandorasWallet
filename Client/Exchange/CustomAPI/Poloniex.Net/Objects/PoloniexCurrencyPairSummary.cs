using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.CustomAPI.Poloniex.Net.Objects
{
    public class PoloniexCurrencyPairSummary
    {
        public int id { get; set; }
        public decimal last { get; set; }
        public decimal lowestAsk { get; set; }
        public decimal highestBid { get; set; }
        public decimal percentChange { get; set; }
        public decimal baseVolume { get; set; }
        public decimal quoteVolume { get; set; }
        public int isFrozen { get; set; }
        public decimal high24hr { get; set; }
        public decimal low24hr { get; set; }
    }
}