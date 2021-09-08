using Pandora.Client.ClientLib.Contracts;
using Pandora.Client.Exchange.Models;
using System;

namespace Pandora.Client.Exchange.Contracts
{
    public enum MarketDirection
    {
        Buy, Sell
    }

    public interface IExchangeMarket : IEquatable<IExchangeMarket>
    {
        /// <summary>
        /// Current prices of the market, valued in the Market base currency
        /// </summary>
        IMarketPriceInfo Prices { get; }

        /// <summary>
        /// Relative currency used to sell trade in the market
        /// </summary>
        ICurrencyIdentity SellingCurrencyInfo { get; }

        /// <summary>
        /// Relative currency to buy in the market
        /// </summary>
        ICurrencyIdentity BuyingCurrencyInfo { get; }

        /// <summary>
        /// Base currency used as symbol to value market prices, it must be one of the two relatives currencies
        /// </summary>
        ICurrencyIdentity MarketBaseCurrencyInfo { get; }

        /// <summary>
        /// Minimum amount to place a trading order, valued in market base currency.
        /// </summary>
        decimal MinimumTrade { get; }

        /// <summary>
        /// Direction which the remote exchange market is trading relative to the current structure
        /// </summary>
        MarketDirection MarketDirection { get; }

        /// <summary>
        /// This is an special id used to diferentiate markets
        /// </summary>
        string MarketID { get; }

        /// <summary>
        /// ID of the exchange owner of this market
        /// </summary>
        int ExchangeID { get; }
    }
}