﻿using Pandora.Client.Exchange.JKrof.Objects;

namespace Pandora.Client.Exchange.JKrof.Interfaces
{
    /// <summary>
    /// Rate limiter interface
    /// </summary>
    public interface IRateLimiter
    {
        /// <summary>
        /// Limit the request if needed
        /// </summary>
        /// <param name="client"></param>
        /// <param name="url"></param>
        /// <param name="limitBehaviour"></param>
        /// <returns></returns>
        CallResult<double> LimitRequest(RestClient client, string url, RateLimitingBehaviour limitBehaviour);
    }
}
