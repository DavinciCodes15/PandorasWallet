using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Contracts.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Currencies.Ethereum.ContractFunctions.Models
{
    [Function("bulksendToken")]
    public class BulksendTokenFunction : FunctionMessage, ITokenFunction
    {
        [Parameter("address", "_token", 1)]
        public string TokenAddress { get; set; }

        [Parameter("address[]", "_to", 2)]
        public List<string> ReceiptAddresses { get; set; }

        [Parameter("uint256[]", "_values", 3)]
        public List<BigInteger> Values { get; set; }

        [Parameter("bytes32", "_uniqueId", 4)]
        public BigInteger UniqueID { get; set; }

        public long GetFunctionHash()
        {
            return (long) BulkSendMethods.BulksendToken;
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
                for (var lCounter = 0; lCounter < lDecodedInput.ReceiptAddresses.Count(); lCounter++)
                {
                    lOutputs.Add(new ERC20DataOutput
                    {
                        ContractAddress = lDecodedInput.TokenAddress,
                        AmountSent = lDecodedInput.Values[lCounter],
                        DestinationAddress = lDecodedInput.ReceiptAddresses[lCounter]
                    });
                }
                lResult = true;
            }
            aDecodedData = lOutputs;
            return lResult;
        }
    }
}