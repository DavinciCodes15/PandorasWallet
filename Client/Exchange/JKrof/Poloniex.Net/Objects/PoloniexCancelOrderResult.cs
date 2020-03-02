using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.JKrof.Poloniex.Net.Objects
{
    public class PoloniexCancelOrderResult
    {
        public int success { get; set; }
        public decimal amount { get; set; }
        public string message { get; set; }
        public string clientOrderId { get; set; }
    }
}
