using Pandora.Client.Exchange.JKrof.Objects;

namespace Pandora.Client.Exchange.JKrof.Interfaces
{
    public interface IRateLimiter
    {
        CallResult<double> LimitRequest(string url, RateLimitingBehaviour limitBehaviour);
    }
}
