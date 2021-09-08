using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.CustomAPI.Poloniex.Net.Objects
{
    public class PoloniexOrderStatusResult : IPoloniexErrorCapable
    {
        public Dictionary<string, PoloniexOrderStatus> result { get; set; }
        public int success { get; set; }
        public string error { get; set; }
    }
}