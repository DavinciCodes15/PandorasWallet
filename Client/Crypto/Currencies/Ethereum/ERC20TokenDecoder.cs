using System;
using System.Collections.Generic;
using Nethereum.Contracts.Extensions;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Nethereum.ABI;
using Nethereum.Contracts;
using Pandora.Client.Crypto.Currencies.Ethereum.ContractFunctions;
using System.Reflection;

namespace Pandora.Client.Crypto.Currencies.Ethereum
{
    public static partial class ERC20TokenDecoder
    {
        public static string GetHex(this ERC20Methods aERC20Method)
        {
            return $"0x{(long) aERC20Method:X8}";
        }

        public static bool TryDecode(string aHex, out IEnumerable<ERC20DataOutput> lDecodedOutputs)
        {
            lDecodedOutputs = null;
            foreach (var lContractFunction in GetContractFunctions())
            {
                if (lContractFunction.TryDecodeInput(aHex, out lDecodedOutputs))
                    break;
            }
            return lDecodedOutputs != null && lDecodedOutputs.Any();
        }

        private static IEnumerable<ITokenFunction> GetContractFunctions()
        {
            return Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == "Pandora.Client.Crypto.Currencies.Ethereum.ContractFunctions.Models").Select(t => (ITokenFunction) Activator.CreateInstance(t));
        }

        public static string Encode(ERC20Methods aERC20Method, params string[] aParams)
        {
            string lResult;
            switch (aERC20Method)
            {
                case ERC20Methods.Transfer:
                    var lHexAddress = BigInteger.Parse(string.Concat("0", aParams[0].Replace("0x", string.Empty)), System.Globalization.NumberStyles.HexNumber);
                    var lDestinationAmount = BigInteger.Parse(aParams[1]);
                    lResult = string.Concat(aERC20Method.GetHex(), lHexAddress.ToString("X64"), lDestinationAmount.ToString("X64"));
                    break;

                default:
                    throw new Exception("ERC20 Method not supported");
            }
            return lResult;
        }
    }
}