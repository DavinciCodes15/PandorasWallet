using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Currencies.Ethereum
{
    public interface IEthereumChainParams
    {
        ChainParams.NetworkType Network { get; set; }

        string NetworkName { get; set; }

        long Version { get; set; }

        long ForkFromId { get; set; }
    }

    public class EthChainParams : ChainParams, IEthereumChainParams
    {
        public EthChainParams(ChainParams chainParams)
        {
            this.CopyFrom(chainParams);
            if (!chainParams.Capabilities.HasFlag(CapablityFlags.EthereumProtocol))
                throw new ArgumentException("ChainParams does not support Ethereum.");
        }
    }
}
