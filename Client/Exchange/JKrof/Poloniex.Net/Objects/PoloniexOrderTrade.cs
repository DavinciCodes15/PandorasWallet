using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.JKrof.Poloniex.Net.Objects
{
    public class PoloniexOrderTrade : PoloniexOrderTradeLite
    {
        public long globalTradeID { get; set; }
        public string currencyPair { get; set; }      
    }
}
