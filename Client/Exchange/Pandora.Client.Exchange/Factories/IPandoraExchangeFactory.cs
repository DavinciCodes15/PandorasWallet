using Pandora.Client.Exchange.Exchanges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.Factories
{
    public interface IPandoraExchangeFactory
    {
        IPandoraExchange GetNewPandoraExchange(params string[] aParams);
    }
}