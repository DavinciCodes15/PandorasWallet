using Pandora.Client.ClientLib.Contracts;
using Pandora.Client.Exchange.Contracts;
using Pandora.Client.Exchange.Exchangers.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pandora.Client.Exchange.Models
{
    internal static class ExchangeMarketTools
    {
        internal static bool TryCastToLocalMarket(this IExchangeMarket aExchangeMarket, out ExchangeMarket aMarket)
        {
            aMarket = aExchangeMarket as ExchangeMarket;
            return aMarket != null && !string.IsNullOrEmpty(aMarket.MarketPairID);
        }

        /// <summary>
        /// Search the collection looking for a market with provided id. It works too with the pairID
        /// </summary>
        /// <param name="aListOfMarkets"></param>
        /// <param name="aMarketID"></param>
        /// <returns></returns>
        internal static IExchangeMarket FindByMarketID(this IEnumerable<IExchangeMarket> aListOfMarkets, string aMarketID)
        {
            var lResult = aListOfMarkets.FirstOrDefault(lMarket => string.Equals(lMarket.MarketID, aMarketID, StringComparison.InvariantCultureIgnoreCase));
            if (lResult == null)
            {
                var lExchangeMarkets = aListOfMarkets.OfType<ExchangeMarket>();
                lResult = lExchangeMarkets.FirstOrDefault(lMarket => string.Equals(lMarket.MarketPairID, aMarketID, StringComparison.OrdinalIgnoreCase));
                if (lResult == null)
                {
                    //Bittrex changed order in their market id, so we need to test the id with markets backwards
                    var lTickers = aMarketID.Split('-');
                    if (lTickers.Count() == 2)
                        lResult = lExchangeMarkets.FirstOrDefault(lMarket => string.Equals(lMarket.MarketPairID, $"{lTickers[1]}-{lTickers[0]}", StringComparison.OrdinalIgnoreCase));

                    //In case poloniex does the same as bittrex, I will do this verification
                    lTickers = aMarketID.Split('_');
                    if (lResult == null && lTickers.Count() == 2)
                        lResult = lExchangeMarkets.FirstOrDefault(lMarket => string.Equals(lMarket.MarketPairID, $"{lTickers[1]}_{lTickers[0]}", StringComparison.OrdinalIgnoreCase));
                }
            }
            return lResult;
        }
    }

    internal class ExchangeMarket : IExchangeMarket
    {
        private IPandoraExchanger FExchangeInstance;

        public ExchangeMarket(IPandoraExchanger aExchangeInstance)
        {
            FExchangeInstance = aExchangeInstance;
        }

        internal virtual string MarketPairID { get; set; }
        public IMarketPriceInfo Prices => FExchangeInstance.GetMarketPrice(this);
        public ICurrencyIdentity SellingCurrencyInfo { get; internal set; }
        public ICurrencyIdentity BuyingCurrencyInfo { get; internal set; }
        public ICurrencyIdentity MarketBaseCurrencyInfo { get; internal set; }
        public int ExchangeID => FExchangeInstance.ID;
        public string MarketID => $"{ExchangeID}_{SellingCurrencyInfo.Ticker}_{BuyingCurrencyInfo.Ticker}";
        public decimal MinimumTrade { get; internal set; }
        public MarketDirection MarketDirection => SellingCurrencyInfo.Ticker == MarketBaseCurrencyInfo.Ticker ? MarketDirection.Buy : MarketDirection.Sell;

        public virtual bool Equals(IExchangeMarket other)
        {
            return string.Equals(MarketID, other.MarketID, StringComparison.OrdinalIgnoreCase);
        }

        internal class CurrencyInfo : ICurrencyIdentity
        {
            private long FId = 0;
            private GetWalletIDDelegate FGetWalletIDFunction;
            public string Ticker { get; set; }
            public string Name { get; set; }

            public virtual long Id
            {
                get
                {
                    if (FId == 0)
                        FId = FGetWalletIDFunction?.Invoke(Ticker) ?? 0;
                    return FId;
                }
                set
                {
                    FId = value;
                }
            }

            public CurrencyInfo()
            {
            }

            public CurrencyInfo(GetWalletIDDelegate aGetWalletIDFunction)
            {
                FGetWalletIDFunction = aGetWalletIDFunction;
            }
        }
    }
}