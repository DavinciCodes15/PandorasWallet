using Pandora.Client.ClientLib.Contracts;

namespace Pandora.Client.Exchange.Contracts
{
    public interface IMarketPriceInfo
    {
        decimal Bid { get; }
        decimal Ask { get; }
        decimal Last { get; }
    }
}