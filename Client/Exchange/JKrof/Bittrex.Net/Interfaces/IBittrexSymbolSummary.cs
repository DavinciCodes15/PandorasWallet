using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.JKrof.Bittrex.Net.Interfaces
{
    public interface IBittrexSymbolSummary
    {
        string Symbol { get; }
        decimal? Bid { get; }
        decimal? Ask { get; }
        decimal? Last { get; }
    }
}
