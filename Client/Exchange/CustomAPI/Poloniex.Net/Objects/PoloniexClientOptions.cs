using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.CustomAPI.Poloniex.Net.Objects
{
    public class PoloniexClientOptions : RestClientOptions
    {
        public PoloniexClientOptions() : base("https://poloniex.com/")
        {
            BaseAddress = "https://poloniex.com/";
        }

        public PoloniexClientOptions Copy()
        {
            var copy = Copy<PoloniexClientOptions>();
            copy.BaseAddress = BaseAddress;
            return copy;
        }
    }

    public class PoloniexSocketClientOptions : SocketClientOptions
    {
        public PoloniexSocketClientOptions() : base("wss://api2.poloniex.com")
        {
            BaseAddress = "wss://api2.poloniex.com";
        }
    }
}