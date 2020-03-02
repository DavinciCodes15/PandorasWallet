﻿namespace Pandora.Client.Exchange.JKrof.Objects
{
    /// <summary>
    /// What to do when a request would exceed the rate limit
    /// </summary>
    public enum RateLimitingBehaviour
    {
        /// <summary>
        /// Fail the request
        /// </summary>
        Fail,
        /// <summary>
        /// Wait till the request can be send
        /// </summary>
        Wait
    }

    /// <summary>
    /// Where the post parameters should be added
    /// </summary>
    public enum PostParameters
    {
        /// <summary>
        /// Post parameters in body
        /// </summary>
        InBody,
        /// <summary>
        /// Post parameters in url
        /// </summary>
        InUri
    }

    /// <summary>
    /// The format of the request body
    /// </summary>
    public enum RequestBodyFormat
    {
        /// <summary>
        /// Form data
        /// </summary>
        FormData,
        /// <summary>
        /// Json
        /// </summary>
        Json
    }

    /// <summary>
    /// Status of the order book
    /// </summary>
    public enum OrderBookStatus
    {
        /// <summary>
        /// Not connected
        /// </summary>
        Disconnected,
        /// <summary>
        /// Connecting
        /// </summary>
        Connecting,
        /// <summary>
        /// Syncing data
        /// </summary>
        Syncing,
        /// <summary>
        /// Data synced, order book is up to date
        /// </summary>
        Synced
    }

    /// <summary>
    /// Order book entry type
    /// </summary>
    public enum OrderBookEntryType
    {
        /// <summary>
        /// Ask
        /// </summary>
        Ask,
        /// <summary>
        /// Bid
        /// </summary>
        Bid
    }

    /// <summary>
    /// Define how array parameters should be send
    /// </summary>
    public enum ArrayParametersSerialization
    {
        /// <summary>
        /// Send multiple key=value for each entry
        /// </summary>
        MultipleValues,
        /// <summary>
        /// Create an []=value array
        /// </summary>
        Array
    }
}
