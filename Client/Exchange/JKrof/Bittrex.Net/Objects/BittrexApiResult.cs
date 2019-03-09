﻿using Newtonsoft.Json;

namespace Bittrex.Net.Objects
{
    /// <summary>
    /// The result of an Api call
    /// </summary>
    /// <typeparam name="T">The result type</typeparam>
    public class BittrexApiResult<T>
    {
        /// <summary>
        /// Whether the Api call was successful
        /// </summary>
        [JsonProperty("success")]
        public bool Success { get; internal set; }
        /// <summary>
        /// The result of the Api call
        /// </summary>
        [JsonProperty("result")]
        public T Result { get; internal set; }

        [JsonProperty("message")]
        public string Message { get; internal set; }
    }
}
