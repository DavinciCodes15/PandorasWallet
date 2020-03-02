﻿using Pandora.Client.Exchange.JKrof.Converters;
using Pandora.Client.Exchange.JKrof.Interfaces;
using Newtonsoft.Json;

namespace Bitfinex.Net.Objects
{
    /// <summary>
    /// Base for order book
    /// </summary>
    [JsonConverter(typeof(ArrayConverter))]
    public class BitfinexOrderBookBase { }

    /// <summary>
    /// Order book entry
    /// </summary>
    public class BitfinexOrderBookEntry: BitfinexOrderBookBase, ISymbolOrderBookEntry
    {
        /// <summary>
        /// The price of this entry
        /// </summary>
        [ArrayProperty(0)]
        public decimal Price { get; set; }
        /// <summary>
        /// The amount of orders for this price
        /// </summary>
        [ArrayProperty(1)]
        public int Count { get; set; }
        /// <summary>
        /// The total quantity for this price
        /// </summary>
        [ArrayProperty(2)]
        public decimal Quantity { get; set; }
    }

    /// <summary>
    /// Raw order book entry
    /// </summary>
    [JsonConverter(typeof(ArrayConverter))]
    public class BitfinexRawOrderBookEntry : BitfinexOrderBookBase, ISymbolOrderBookEntry
    {
        /// <summary>
        /// The id of this order
        /// </summary>
        [ArrayProperty(0)]
        public long OrderId { get; set; }
        /// <summary>
        /// The price for this order
        /// </summary>
        [ArrayProperty(1)]
        public decimal Price { get; set; }
        /// <summary>
        /// The quantity of this order
        /// </summary>
        [ArrayProperty(2)]
        public decimal Quantity { get; set; }
    }
}
