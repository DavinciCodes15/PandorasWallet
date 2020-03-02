using System;
using Pandora.Client.Exchange.JKrof.Converters;
using Newtonsoft.Json;

namespace Bittrex.Net.Objects.V3
{
    internal class BittrexServerTimeV3
    {
        [JsonConverter(typeof(TimestampConverter))]
        public DateTime ServerTime { get; set; }
    }
}
