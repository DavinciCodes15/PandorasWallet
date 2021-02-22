using Pandora.Client.Crypto.Currencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.ClientLib.Contracts
{
    public interface ICurrencyItem : ICurrencyIdentity, ICurrencyAmountFormatter
    {
        ushort Precision { get; }

        DateTime LiveDate { get; }

        int MinConfirmations { get; }

        byte[] Icon { get; }

        long FeePerKb { get; }

        ChainParams ChainParamaters { get; }

        CurrencyStatus CurrentStatus { get; set; }

        ICurrencyItem CopyFrom(ICurrencyItem aDestinationItem);
    }

    public interface ICurrencyAmountFormatter
    {
        decimal AmountToDecimal(BigInteger aAmount);

        long AmountToLong(decimal aAmount);

        BigInteger AmountToBigInteger(decimal aAmount);
    }
}