﻿using Newtonsoft.Json;

namespace Bitfinex.Net.Objects.SocketObjects
{
    internal class BitfinexAuthentication
    {
        [JsonProperty("event")]
        public string Event { get; set; } = "";
        [JsonProperty("apiKey")]
        public string ApiKey { get; set; } = "";
        [JsonProperty("authPayload")]
        public string Payload { get; set; } = "";
        [JsonProperty("authSig")]
        public string Signature { get; set; } = "";
        [JsonProperty("authNonce")]
        public string Nonce { get; set; } = "";
        [JsonProperty("filter")]
        public string[] Filter { get; set; }
    }
}
