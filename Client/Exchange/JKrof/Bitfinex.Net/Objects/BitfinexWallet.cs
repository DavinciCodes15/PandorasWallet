﻿using Bitfinex.Net.Converters;
using Pandora.Client.Exchange.JKrof.Converters;
using Newtonsoft.Json;

namespace Bitfinex.Net.Objects
{
    /// <summary>
    /// Wallet info
    /// </summary>
    [JsonConverter(typeof(ArrayConverter))]
    public class BitfinexWallet
    {
        /// <summary>
        /// The type of the wallet
        /// </summary>
        [ArrayProperty(0), JsonConverter(typeof(WalletTypeConverter))]
        public WalletType Type { get; set; }

        /// <summary>
        /// the currency
        /// </summary>
        [ArrayProperty(1)]
        public string Currency { get; set; } = "";

        /// <summary>
        /// The current balance
        /// </summary>
        [ArrayProperty(2)]
        public decimal Balance { get; set; }

        /// <summary>
        /// The unsettled interest
        /// </summary>
        [ArrayProperty(3)]
        public decimal UnsettledInterest { get; set; }

        /// <summary>
        /// The available balance
        /// </summary>
        [ArrayProperty(4)]
        public decimal? BalanceAvailable { get; set; }
    }
}
