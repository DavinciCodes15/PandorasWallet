using System.Collections.Generic;
using Bittrex.Net.Objects;
using Pandora.Client.Exchange.JKrof.Converters;

namespace Bittrex.Net.Converters.V3
{
    internal class DepositAddressStatusConverter : BaseConverter<DepositAddressStatus>
    {
        public DepositAddressStatusConverter() : this(true) { }
        public DepositAddressStatusConverter(bool quotes) : base(quotes) { }

        protected override List<KeyValuePair<DepositAddressStatus, string>> Mapping => new List<KeyValuePair<DepositAddressStatus, string>>
        {
            new KeyValuePair<DepositAddressStatus, string>(DepositAddressStatus.Requested, "REQUESTED"),
            new KeyValuePair<DepositAddressStatus, string>(DepositAddressStatus.Provisioned, "PROVISIONED")
        };
    }
}
