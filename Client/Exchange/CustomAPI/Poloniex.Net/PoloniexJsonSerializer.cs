using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pandora.Client.Exchange.CustomAPI.Poloniex.Net.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.CustomAPI.Poloniex.Net
{
    internal class PoloniexJsonSerializer : JsonSerializer
    {
        private static PoloniexJsonSerializer FInstance;

        private PoloniexJsonSerializer()
        {
            var lConverter = new PoloniexJsonConverter();
            Converters.Clear();
            Converters.Add(lConverter);
        }

        public static PoloniexJsonSerializer GetSerializer()
        {
            if (FInstance == null)
                FInstance = new PoloniexJsonSerializer();
            return FInstance;
        }
    }

    internal class PoloniexJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object lResult = null;

            try
            {
                var lSerializer = new JsonSerializer();
                lResult = lSerializer.Deserialize(reader, objectType);
            }
            catch
            {
                JProperty lobj;
                try
                {
                    lobj = JProperty.Load(reader);
                }
                catch
                {
                    lobj = new JProperty("error", reader.Value);
                }

                if (lobj.Name == "error" && typeof(IPoloniexErrorCapable).IsAssignableFrom(objectType))
                {
                    var lErrorObject = (IPoloniexErrorCapable) Activator.CreateInstance(objectType);
                    lErrorObject.error = lobj.Value.ToString();
                    lResult = lErrorObject;
                }
            }
            return lResult;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}