using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Currencies
{
    public interface ICryptoCurrency
    {
        string Name { get; }
        ICryptoCurrencyKey CreateNewKey();

    }
}
