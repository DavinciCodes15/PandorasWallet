using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.JKrof.Poloniex.Net.Objects
{
    public class PoloniexOrderTradeLite
    {
        public decimal amount { get; set; }
        public decimal total { get; set; }
        public decimal fee { get; set; }
        public DateTime date { get; set; }
        public string type { get; set; }
        public decimal rate { get; set; }
        public long tradeID { get; set; }
    }
}
