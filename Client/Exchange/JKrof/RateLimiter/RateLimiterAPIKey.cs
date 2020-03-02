﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Pandora.Client.Exchange.JKrof.Interfaces;
using Pandora.Client.Exchange.JKrof.Objects;

namespace Pandora.Client.Exchange.JKrof.RateLimiter
{
    /// <summary>
    /// Limits the amount of requests per time period to a certain limit, counts the request per API key.
    /// </summary>
    public class RateLimiterAPIKey: IRateLimiter
    {
        internal Dictionary<string, RateLimitObject> history = new Dictionary<string, RateLimitObject>();

        private readonly int limitPerKey;
        private readonly TimeSpan perTimePeriod;
        private readonly object historyLock = new object();

        /// <summary>
        /// Create a new RateLimiterAPIKey. This rate limiter limits the amount of requests per time period to a certain limit, counts the request per API key.
        /// </summary>
        /// <param name="limitPerApiKey">The amount to limit to</param>
        /// <param name="perTimePeriod">The time period over which the limit counts</param>
        public RateLimiterAPIKey(int limitPerApiKey, TimeSpan perTimePeriod)
        {
            limitPerKey = limitPerApiKey;
            this.perTimePeriod = perTimePeriod;
        }

        /// <inheritdoc />
        public CallResult<double> LimitRequest(RestClient client, string url, RateLimitingBehaviour limitBehaviour)
        {
            if(client.authProvider?.Credentials?.Key == null)
                return new CallResult<double>(0, null);

            var key = client.authProvider.Credentials.Key.GetString();

            int waitTime;
            RateLimitObject rlo;
            lock (historyLock)
            {
                if (history.ContainsKey(key))
                    rlo = history[key];
                else
                {
                    rlo = new RateLimitObject();
                    history.Add(key, rlo);
                }
            }

            var sw = Stopwatch.StartNew();
            lock (rlo.LockObject)
            {
                sw.Stop();
                waitTime = rlo.GetWaitTime(DateTime.UtcNow, limitPerKey, perTimePeriod);
                if (waitTime != 0)
                {
                    if (limitBehaviour == RateLimitingBehaviour.Fail)
                        return new CallResult<double>(waitTime, new RateLimitError($"endpoint limit of {limitPerKey} reached on api key " + key));

                    Thread.Sleep(Convert.ToInt32(waitTime));
                    waitTime += (int)sw.ElapsedMilliseconds;
                }

                rlo.Add(DateTime.UtcNow);
            }

            return new CallResult<double>(waitTime, null);
        }
    }
}
