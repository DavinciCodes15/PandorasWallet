﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Binance.Net.Objects;

namespace Binance.Net.Converters
{
    public class OrderTypeConverter : JsonConverter
    {
        private readonly bool quotes;

        public OrderTypeConverter()
        {
            quotes = true;
        }

        public OrderTypeConverter(bool useQuotes)
        {
            quotes = useQuotes;
        }

        private readonly Dictionary<OrderType, string> values = new Dictionary<OrderType, string>()
        {
            { OrderType.Limit, "LIMIT" },
            { OrderType.Market, "MARKET" },
            { OrderType.LimitMaker, "LIMIT_MAKER" },
            { OrderType.StopLoss, "STOP_LOSS" },
            { OrderType.StopLossLimit, "STOP_LOSS_LIMIT" },
            { OrderType.TakeProfit, "TAKE_PROFIT" },
            { OrderType.TakeProfitLimit, "TAKE_PROFIT_LIMIT" }
        };

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(OrderType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if(reader.TokenType == JsonToken.StartArray)
            {
                var result = new List<OrderType>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.String)
                        result.Add(values.Single(v => v.Value == (string)reader.Value).Key);
                    if (reader.TokenType == JsonToken.EndArray)
                        break;
                }
                return result.ToArray();
            }
            else
                return values.Single(v => v.Value == (string)reader.Value).Key;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (quotes)
                writer.WriteValue(values[(OrderType)value]);
            else
                writer.WriteRawValue(values[(OrderType)value]);
        }
    }
}
