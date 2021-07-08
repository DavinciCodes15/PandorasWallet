using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.PriceSource.Contracts
{
    public interface ICurrencyPrice
    {
        string Id { get; }
        string Name { get; }
        string Reference { get; }
        decimal Price { get; }
    }
}