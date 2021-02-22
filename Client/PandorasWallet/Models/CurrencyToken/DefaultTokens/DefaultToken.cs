using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.PandorasWallet.Models.CurrencyToken.DefaultTokens
{
    internal abstract class DefaultToken : IClientCurrencyToken
    {
        public abstract byte[] Icon { get; }
        public abstract long Id { get; }
        public abstract string ContractAddress { get; }
        public abstract string Name { get; }
        public abstract string Ticker { get; }
        public abstract ushort Precision { get; }
        public abstract long ParentCurrencyID { get; }
    }
}