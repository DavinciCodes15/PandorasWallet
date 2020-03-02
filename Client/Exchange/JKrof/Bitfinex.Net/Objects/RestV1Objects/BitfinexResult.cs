﻿using Bitfinex.Net.Converters;
using Newtonsoft.Json;

namespace Bitfinex.Net.Objects.RestV1Objects
{
    /// <summary>
    /// Result
    /// </summary>
    public class BitfinexResult
    {
        /// <summary>
        /// Result string
        /// </summary>
        public string Result { get; set; } = "";
    }

    /// <summary>
    /// Transfer results
    /// </summary>
    public class BitfinexTransferResult
    {
        /// <summary>
        /// The status of the transfer
        /// </summary>
        [JsonConverter(typeof(StringToBoolConverter)), JsonProperty("status")]
        public bool Success { get; set; }
        /// <summary>
        /// Additional info
        /// </summary>
        public string Message { get; set; } = "";
    }
}
