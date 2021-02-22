using Pandora.Client.ClientLib;
using Pandora.Client.ClientLib.Contracts;
using Pandora.Client.PandorasWallet.Models.CurrencyToken.DefaultTokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.PandorasWallet.Models
{
    public interface IClientCurrencyToken : ICurrencyToken
    {
        byte[] Icon { get; }
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
            Id = aToken.Id;
        }

        public byte[] Icon { get; set; }
        public long Id { get; set; }

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

        public static class DefaultInventory
        {
            public static IEnumerable<IClientCurrencyToken> GetTokens()
            {
                var lDefaultCoinsTypes = Assembly.GetExecutingAssembly().GetTypes().Where(lType => lType.IsClass && lType.IsSubclassOf(typeof(DefaultToken)));
                return lDefaultCoinsTypes.Select(lType => (IClientCurrencyToken) Activator.CreateInstance(lType));
            }
        }
    }
}