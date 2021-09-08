using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.Exchangers
{
    /// <summary>
    /// This is the list of exchanges that are declared and available, the value assigned to each element represents the id of the item.
    /// Please be careful to not modify elements that were already added.
    /// </summary>
    public enum AvailableExchangesList
    {
        Bittrex = 1001,
        Poloniex = 1003,
        Bitfinex = 1006
    }
}