using Pandora.Client.PriceSource.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.PriceSource.SourceAPIs.Contracts
{
    public interface IPriceSourceAPI : IDisposable
    {
        int Id { get; }
        string Name { get; }

        bool TestConnection();

        Task<IEnumerable<ICurrencyPrice>> GetPrices(IEnumerable<string> aCurrencyTicker);

        Task<IEnumerable<ICurrencyPrice>> GetTokenPrices(IEnumerable<string> aContracts);
    }
}