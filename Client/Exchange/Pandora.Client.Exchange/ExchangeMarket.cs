using Pandora.Client.Exchange.Exchanges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange
{
    public class ExchangeMarket2
    {
        private readonly IPandoraExchange FParentExchange;

        public ExchangeMarket2(IPandoraExchange aParentExchange)
        {
            FParentExchange = aParentExchange;
        }

        /// <summary>
        /// Gets the relative price of the market
        /// </summary>
        public decimal Price => BasePrice > 0 ? Math.Round(IsSell ? 1 / BasePrice : BasePrice, 7) : 0;

        /// <summary>
        /// Returns current base price for the market. If the current price can not be retrieved, it will return 0 instead.
        /// </summary>
        public decimal BasePrice
        {
            get
            {
                decimal lPrice = 0;
                try
                {
                    lock (FParentExchange)
                        lPrice = FParentExchange.GetMarketPrice(MarketName);
                }
                catch (Exception ex)
                {
                    Universal.Log.Write(Universal.LogLevel.Error, $"Base price for market {MarketName} error. Exception: {ex.ToString()}");
                }

                return lPrice;
            }
        }

        public string BaseTicker { get; set; }
        public string CoinName { get; set; }
        public string CoinTicker { get; set; }
        public string MarketName { get; set; }
        public decimal MinimumTrade { get; set; }
        public bool IsSell { get; set; }
    }
}