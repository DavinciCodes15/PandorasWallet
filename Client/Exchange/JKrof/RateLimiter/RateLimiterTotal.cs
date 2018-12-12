﻿using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace CryptoExchange.Net.RateLimiter
{
    /// <summary>
    /// Limits the amount of requests per time period to a certain limit, counts the total amount of requests.
    /// </summary>
    public class RateLimiterTotal: IRateLimiter
    {
        internal List<DateTime> history = new List<DateTime>();

        private readonly int limit;
        private readonly TimeSpan perTimePeriod;
        private readonly object requestLock = new object();

        /// <summary>
        /// Create a new RateLimiterTotal. This rate limiter limits the amount of requests per time period to a certain limit, counts the total amount of requests.
        /// </summary>
        /// <param name="limit">The amount to limit to</param>
        /// <param name="perTimePeriod">The time period over which the limit counts</param>
        public RateLimiterTotal(int limit, TimeSpan perTimePeriod)
        {
            this.limit = limit;
            this.perTimePeriod = perTimePeriod;
        }

        public CallResult<double> LimitRequest(string url, RateLimitingBehaviour limitBehaviour)
        {
            var sw = Stopwatch.StartNew();
            lock (requestLock)
            {
                sw.Stop();
                double waitTime = 0;
                var checkTime = DateTime.UtcNow;
                history.RemoveAll(d => d < checkTime - perTimePeriod);

                if (history.Count >= limit)
                {
                    waitTime = (history.First() - (checkTime - perTimePeriod)).TotalMilliseconds;
                    if (waitTime > 0)
                    {
                        if (limitBehaviour == RateLimitingBehaviour.Fail)
                            return new CallResult<double>(waitTime, new RateLimitError($"total limit of {limit} reached"));

                        Thread.Sleep(Convert.ToInt32(waitTime));
                        waitTime += sw.ElapsedMilliseconds;
                    }
                }

                history.Add(DateTime.UtcNow);
                history.Sort();
                return new CallResult<double>(waitTime, null);
            }
        }
    }
}
