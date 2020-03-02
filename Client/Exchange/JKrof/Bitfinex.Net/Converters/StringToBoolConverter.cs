using System.Collections.Generic;
using Pandora.Client.Exchange.JKrof.Converters;

namespace Bitfinex.Net.Converters
{
    internal class StringToBoolConverter : BaseConverter<bool>
    {
        public StringToBoolConverter() : this(true) { }
        public StringToBoolConverter(bool quotes) : base(quotes) { }

        protected override List<KeyValuePair<bool, string>> Mapping => new List<KeyValuePair<bool, string>>
        {
            new KeyValuePair<bool, string>(false, "No"),
            new KeyValuePair<bool, string>(true, "Yes"),
            new KeyValuePair<bool, string>(true, "success"),
            new KeyValuePair<bool, string>(true, "error")
        };
    }
}
