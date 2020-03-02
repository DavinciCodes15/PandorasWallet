﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Bitfinex.Net.Objects;
using Newtonsoft.Json;

namespace Bitfinex.Net.Converters
{
    internal class OrderStatusConverter: JsonConverter
    {
        private readonly Dictionary<string, OrderStatus> mapping = new Dictionary<string, OrderStatus>
        {
            { "PARTIALLY FILLED",OrderStatus.PartiallyFilled },
            { "EXECUTED", OrderStatus.Executed },
            { "CANCELED", OrderStatus.Canceled },
            { "ACTIVE", OrderStatus.Active }
        };
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString().ToUpper());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return OrderStatus.Unknown;

            var statusString = reader.Value.ToString();
            return ParseString(statusString);
        }

        public OrderStatus FromString(string status)
        {
            return ParseString(status);
        }

        private OrderStatus ParseString(string status)
        {
            var wasSplit = status.Split(new[] { " was: " }, StringSplitOptions.RemoveEmptyEntries);
            var split = wasSplit[0].Split(new[] { " @ " }, StringSplitOptions.RemoveEmptyEntries);

            if(TryParseSubstring(split[0], out var result))
               return result;

            split = wasSplit[1].Split(new[] { " @ " }, StringSplitOptions.RemoveEmptyEntries);
            if (TryParseSubstring(split[0], out result))
                return result;

            Debug.WriteLine($"Couldn't deserialize order status: {status}");
            return OrderStatus.Unknown;
        }

        private bool TryParseSubstring(string data, out OrderStatus status)
        {
            status = OrderStatus.Unknown;
            var result = mapping.SingleOrDefault(m => m.Key == data);
            if (result.Equals(default(KeyValuePair<string, OrderStatus>)))
            {
                result = mapping.FirstOrDefault(m => data.Contains(m.Key));
                if (result.Equals(default(KeyValuePair<string, OrderStatus>)))
                    return false;
            }

            status = result.Value;
            return true;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(OrderStatus);
        }
    }
}
