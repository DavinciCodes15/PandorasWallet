﻿using Pandora.Client.Exchange.JKrof.Converters;
using Newtonsoft.Json;

namespace Bitfinex.Net.Objects
{
    /// <summary>
    /// User info
    /// </summary>
    [JsonConverter(typeof(ArrayConverter))]
    public class BitfinexUserInfo
    {
        /// <summary>
        /// The id of the user
        /// </summary>
        [ArrayProperty(0)]
        public long Id { get; set; }
        /// <summary>
        /// The email address of the user
        /// </summary>
        [ArrayProperty(1)]
        public string Email { get; set; } = "";
        /// <summary>
        /// The username of the user
        /// </summary>
        [ArrayProperty(2)]
        public string UserName { get; set; } = "";
    }
}
