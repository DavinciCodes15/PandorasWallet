using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.CustomAPI.Poloniex.Net.Objects
{
    public class PoloniexOrderTradeResult : List<PoloniexOrderTrade>, IPoloniexErrorCapable
    {
        public string error { get; set; }
    }
}