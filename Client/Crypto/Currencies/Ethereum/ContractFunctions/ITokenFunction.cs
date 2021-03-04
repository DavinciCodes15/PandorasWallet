using System.Collections.Generic;

namespace Pandora.Client.Crypto.Currencies.Ethereum.ContractFunctions
{
    internal interface ITokenFunction
    {
        long GetFunctionHash();

        bool TryDecodeInput(string aHex, out IEnumerable<ERC20DataOutput> aDecodedData);
    }
}