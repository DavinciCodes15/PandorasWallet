using Pandora.Client.Exchange.Exchangers.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandora.Client.Exchange.Exchangers;

namespace Pandora.Client.Exchangers.Contracts
{
    public interface IPandoraExchangeFactory
    {
        IPandoraExchanger GetPandoraExchange(AvailableExchangesList aExchangeElement);

        IEnumerable<IPandoraExchanger> GetPandoraExchanges();
    }
}