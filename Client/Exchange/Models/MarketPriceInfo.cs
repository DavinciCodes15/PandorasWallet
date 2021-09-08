using Pandora.Client.Exchange.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.Models
{
    public class MarketPriceInfo : IMarketPriceInfo, IEquatable<IMarketPriceInfo>
    {
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }
        public decimal Last { get; set; }

        public bool Equals(IMarketPriceInfo other)
        {
            return Bid == other.Bid && Ask == other.Ask && Last == other.Last;
        }

        public static bool operator ==(MarketPriceInfo lhs, MarketPriceInfo rhs)
        {
            return Equals(lhs, rhs);
        }

        public static bool operator !=(MarketPriceInfo lhs, MarketPriceInfo rhs)
        {
            return !Equals(lhs, rhs);
        }
    }
}