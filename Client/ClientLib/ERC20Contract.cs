using System;
using System.Numerics;

namespace Pandora.Client.ClientLib
{
    //TODO: The whole idea of having this class is to put it inside a factory, but right now only ERC20 will be supported
    public class ERC20Contract : ITokenContract
    {
        private const string FNameMethod = @"0x06fdde03";
        private const string FSymbolMethod = @"0x95d89b41";
        private const string FDecimalsMethod = @"0x313ce567";

        public string GetBalanceMethodCode(string aAddress)
        {
            throw new NotImplementedException();
        }

        public string GetBalanceTransferMethodCode(string aDestinationAddress, BigInteger aAmount)
        {
            throw new System.NotImplementedException();
        }

        public string GetNameMethodCode()
        {
            return FNameMethod;
        }

        public string GetPrecisionMethodCode()
        {
            return FDecimalsMethod;
        }

        public string GetSymbolMethodCode()
        {
            return FSymbolMethod;
        }
    }
}