﻿using System.Collections.Generic;
using Bitfinex.Net.Objects;
using Pandora.Client.Exchange.JKrof.Converters;

namespace Bitfinex.Net.Converters
{
    internal class OrderSideConverter: BaseConverter<OrderSide>
    {
        public OrderSideConverter(): this(true) { }
        public OrderSideConverter(bool quotes) : base(quotes) { }

        protected override List<KeyValuePair<OrderSide, string>> Mapping => new List<KeyValuePair<OrderSide, string>>
        {
            new KeyValuePair<OrderSide, string>(OrderSide.Buy, "buy"),
            new KeyValuePair<OrderSide, string>(OrderSide.Sell, "sell")
        };
    }
}
