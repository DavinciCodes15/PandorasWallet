using Pandora.Client.ClientLib;
using Pandora.Client.ClientLib.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.PandorasWallet.Models
{
    public interface IClientCurrencyToken : ICurrencyToken

    {
        byte[] Icon { get; }
        int ID { get; }
    }

    public class ClientCurrencyTokenItem : CurrencyTokenItem, IClientCurrencyToken, ICurrencyAmountFormatter
    {
        public ClientCurrencyTokenItem()
        {
        }

        public ClientCurrencyTokenItem(ICurrencyToken aToken) : base(aToken)
        {
        }

        public ClientCurrencyTokenItem(IClientCurrencyToken aToken) : base(aToken)
        {
            Icon = aToken.Icon;
            ID = aToken.ID;
        }

        public byte[] Icon { get; set; }
        public int ID { get; set; }

        public BigInteger AmountToBigInteger(decimal aAmount)
        {
            return new BigInteger(aAmount * Convert.ToDecimal(Math.Pow(10, Precision)));
        }

        public decimal AmountToDecimal(BigInteger aAmount)
        {
            return (decimal) aAmount / Convert.ToDecimal(Math.Pow(10, Precision));
        }

        public long AmountToLong(decimal aAmount)
        {
            return Convert.ToInt64(aAmount * Convert.ToDecimal(Math.Pow(10, Precision)));
        }
    }
}