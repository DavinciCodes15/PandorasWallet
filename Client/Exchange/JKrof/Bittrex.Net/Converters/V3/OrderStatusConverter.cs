using System.Collections.Generic;
using Bittrex.Net.Objects;
using Pandora.Client.Exchange.JKrof.Converters;

namespace Bittrex.Net.Converters.V3
{
    internal class OrderStatusConverter : BaseConverter<OrderStatus>
    {
        public OrderStatusConverter() : this(true) { }
        public OrderStatusConverter(bool quotes) : base(quotes) { }

        protected override List<KeyValuePair<OrderStatus, string>> Mapping => new List<KeyValuePair<OrderStatus, string>>
        {
            new KeyValuePair<OrderStatus, string>(OrderStatus.Open, "OPEN"),
            new KeyValuePair<OrderStatus, string>(OrderStatus.Closed, "CLOSED")
        };
    }
}
