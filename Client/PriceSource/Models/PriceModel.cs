using Pandora.Client.PriceSource.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.PriceSource.Models
{
    internal class PriceModel : ICurrencyPrice
    {
        public string Name { get; set; }

        public string Reference { get; set; }

        public decimal Price { get; set; }

        public string Id => $"{Name}-{Reference}".ToLowerInvariant();
    }
}