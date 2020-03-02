using Pandora.Client.Exchange.Exchanges;
using Pandora.Client.Exchange.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.Objects
{
    public class ExchangeMarket
    {
        private IPandoraExchanger FExchangeInstance;
        public ExchangeMarket(IPandoraExchanger aExchangeInstance)
        {
            FExchangeInstance = aExchangeInstance;
        }

        public MarketPriceInfo Prices => FExchangeInstance.GetMarketPrice(MarketName);
        public CurrencyInfo BaseCurrencyInfo { get; set; }
        public CurrencyInfo DestinationCurrencyInfo { get; set; }
        public string MarketName { get; set; }
        public decimal MinimumTrade { get; set; }
        public bool MinTradeIsBaseCurrency { get; set; }
        public bool IsSell { get; set; }


        public class CurrencyInfo
        {
            public string Ticker { get; set; }
            public string Name { get; set; }
            public long? WalletID { get; set; }
        }
    }
}
