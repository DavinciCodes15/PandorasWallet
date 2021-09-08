using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.CustomAPI.Poloniex.Net.Objects
{
    public class PoloniexChartData
    {
        public int date { get; set; }
        public decimal high { get; set; }
        public decimal low { get; set; }
        public decimal open { get; set; }
        public decimal close { get; set; }
        public double volume { get; set; }
        public double quoteVolume { get; set; }
        public double weightedAverage { get; set; }
    }
}