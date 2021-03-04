using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Currencies.Ethereum.ContractFunctions.Models
{
    [Function("transfer", "bool")]
    public class ERC20TransferFunction : FunctionMessage, ITokenFunction
    {
        [Parameter("address", "_to", 1)]
        public string RecipientAddress { get; set; }

        [Parameter("uint256", "_value", 2)]
        public BigInteger Amount { get; set; }

        public long GetFunctionHash()
        {
            return (long) ERC20Methods.Transfer;
        }

        public bool TryDecodeInput(string aHex, out IEnumerable<ERC20DataOutput> aDecodedData)
        {
            bool lResult = false;
            var lOutputs = new List<ERC20DataOutput>();
            var lRawHex = aHex.Replace("0x", string.Empty);
            long lMethod = long.Parse(lRawHex.Substring(0, 8), System.Globalization.NumberStyles.HexNumber);
            if (lMethod == GetFunctionHash())
            {
                var lDecodedInput = this.DecodeInput(aHex);
                lOutputs.Add(new ERC20DataOutput
                {
                    AmountSent = lDecodedInput.Amount,
                    DestinationAddress = lDecodedInput.RecipientAddress
                });
                lResult = true;
            }
            aDecodedData = lOutputs;
            return lResult;
        }
    }
}