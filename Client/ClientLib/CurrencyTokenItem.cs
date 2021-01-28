using Pandora.Client.ClientLib.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Pandora.Client.ClientLib
{
    public class CurrencyTokenItem : ICurrencyToken
    {
        public CurrencyTokenItem()
        {
        }

        public CurrencyTokenItem(ICurrencyToken aToken)
        {
            ContractAddress = aToken.ContractAddress;
            ParentCurrencyID = aToken.ParentCurrencyID;
            Name = aToken.Name;
            Ticker = aToken.Ticker;
            Precision = aToken.Precision;
        }

        public string ContractAddress { get; set; }
        public string Name { get; set; }
        public string Ticker { get; set; }
        public ushort Precision { get; set; }
        public long ParentCurrencyID { get; set; }
    }
}