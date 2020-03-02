using System.Collections.Generic;
using Bitfinex.Net.Objects;
using Pandora.Client.Exchange.JKrof.Converters;

namespace Bitfinex.Net.Converters
{
    internal class StatSectionConverter: BaseConverter<StatSection>
    {
        public StatSectionConverter(): this(true) { }
        public StatSectionConverter(bool quotes) : base(quotes) { }

        protected override List<KeyValuePair<StatSection, string>> Mapping => new List<KeyValuePair<StatSection, string>>
        {
            new KeyValuePair<StatSection, string>(StatSection.History, "hist"),
            new KeyValuePair<StatSection, string>(StatSection.Last, "last")
        };
    }
}
