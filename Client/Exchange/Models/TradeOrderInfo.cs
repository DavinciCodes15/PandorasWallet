using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.Models
{
    public class TradeOrderStatusInfo
    {
        public bool Cancelled { get; set; }
        public bool Completed { get; set; }
        public decimal Rate { get; set; }
        public string ID { get; set; }
    }
}