using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Currencies.Ethereum
{
    public enum ERC20Methods : long
    {
        Transfer = 0xa9059cbb,
        TransferFrom = 0x23b872dd,
        TotalSupply = 0x18160ddd,
        BalanceOf = 0x70a08231,
        Allowance = 0xdd62ed3e,
        Approve = 0x095ea7b3,
        Name = 0x06fdde03,
        Symbol = 0x95d89b41,
        Decimals = 0x313ce567
    }

    public static class ERC20TokenDecoder
    {
        public static string GetHex(this ERC20Methods aERC20Method)
        {
            return $"0x{(long) aERC20Method:X8}";
        }

        public static ERC20DataOutput TryDecode(string aHex)
        {
            ERC20DataOutput lResult = null;
            var lRawHex = aHex.Replace("0x", string.Empty);
            var lMethod = (ERC20Methods) long.Parse(lRawHex.Substring(0, 8), System.Globalization.NumberStyles.HexNumber);
            switch (lMethod)
            {
                case ERC20Methods.Transfer:
                    lResult = new ERC20DataOutput()
                    {
                        DestinationAddress = string.Concat("0x", (lRawHex.Substring(8, 64).TrimStart('0'))),
                        AmountSent = BigInteger.Parse(lRawHex.Substring(72, 64), System.Globalization.NumberStyles.HexNumber)
                    };
                    break;

                case ERC20Methods.TransferFrom:
                    lResult = new ERC20DataOutput()
                    {
                        OriginAddress = lRawHex.Substring(8, 64).TrimStart('0'),
                        DestinationAddress = lRawHex.Substring(64, 64).TrimStart('0'),
                        AmountSent = BigInteger.Parse(lRawHex.Substring(136, 64), System.Globalization.NumberStyles.HexNumber)
                    };
                    break;
            }
            return lResult;
        }

        public static string Encode(ERC20Methods aERC20Method, params string[] aParams)
        {
            string lResult;
            switch (aERC20Method)
            {
                case ERC20Methods.Transfer:
                    var lHexAddress = BigInteger.Parse(aParams[0].Replace("0x", string.Empty), System.Globalization.NumberStyles.HexNumber);
                    var lDestinationAmount = BigInteger.Parse(aParams[1]);
                    lResult = string.Concat(aERC20Method.GetHex(), lHexAddress.ToString("X64"), lDestinationAmount.ToString("X64"));
                    break;

                default:
                    throw new Exception("ERC20 Method not supported");
            }
            return lResult;
        }

        public class ERC20DataOutput
        {
            public BigInteger AmountSent { get; set; }
            public string DestinationAddress { get; set; }
            public string OriginAddress { get; set; }
        }
    }
}