﻿using CryptoExchange.Net;
using CryptoExchange.Net.Objects;

namespace Bittrex.Net.Objects
{
    public class BittrexClientOptions : ExchangeOptions
    {
        public BittrexClientOptions()
        {
            BaseAddress = "https://api.bittrex.com";
        }

        public string BaseAddressV2 { get; set; } = "https://bittrex.com";
    }

    public class BittrexSocketClientOptions : ExchangeOptions
    {
        public BittrexSocketClientOptions()
        {
            BaseAddress = "https://socket.bittrex.com/signalr";
        }

        public string BaseAddressV2 { get; set; } = "https://bittrex.com";
    }
}
