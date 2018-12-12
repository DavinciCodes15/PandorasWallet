using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandora.Client.Crypto.Currencies
{
    public interface ICryptoCurrencyKey
    {
        ICryptoCurrency CryptoCurrency { get; }
        string PrivateKey { get; set; }
        string PublicKey { get; }
    }
}
