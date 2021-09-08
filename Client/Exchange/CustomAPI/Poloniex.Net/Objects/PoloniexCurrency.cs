using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.CustomAPI.Poloniex.Net.Objects
{
    public class PoloniexCurrency
    {
        public int id { get; set; }
        public decimal txFee { get; set; }
        public int minConf { get; set; }
        public string depositAddress { get; set; }
        public int disabled { get; set; }
        public int delisted { get; set; }
        public int frozen { get; set; }
        public string name { get; set; }
    }
}