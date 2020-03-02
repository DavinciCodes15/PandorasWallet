﻿using Pandora.Client.Exchange.JKrof.Converters;
using Newtonsoft.Json;
using System.Linq;

namespace Bitfinex.Net.Objects.SocketObjects
{
    [JsonConverter(typeof(ArrayConverter))]
    internal class BitfinexSocketQuery
    {
        [JsonIgnore]
        public string Id { get; set; }
        [JsonIgnore]
        public BitfinexEventType QueryType { get; set; }

        [ArrayProperty(0)]
        public int ChannelId { get; set; }
        [ArrayProperty(1)]
        public string Event { get; set; }
        [ArrayProperty(2)]
        public object Object { get; set; }
        [ArrayProperty(3)]
        public object Request { get; set; }

        public BitfinexSocketQuery(string id, BitfinexEventType type, object request)
        {
            Id = id;
            QueryType = type;
            Request = request;
            Event = BitfinexEvents.EventMapping.Single(k => k.Value == type).Key;
        }
    }
}
