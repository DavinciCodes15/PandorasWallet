using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.Models
{
    public class CandlestickPoint
    {
        public decimal Open { get; internal set; }
        public decimal Close { get; internal set; }
        public decimal High { get; internal set; }
        public decimal Low { get; internal set; }
        public DateTime TimeStamp { get; internal set; }
    }
}