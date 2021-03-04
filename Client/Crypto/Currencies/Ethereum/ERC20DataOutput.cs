using System.Numerics;

namespace Pandora.Client.Crypto.Currencies.Ethereum
{
    public class ERC20DataOutput
    {
        public string ContractAddress { get; set; }
        public BigInteger AmountSent { get; set; }
        public string DestinationAddress { get; set; }
        public string OriginAddress { get; set; }
    }
}