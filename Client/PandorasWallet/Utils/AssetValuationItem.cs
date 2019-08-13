using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.PandorasWallet.Utils
{
    public class AssetValuationItem : ICloneable
    {
        public int ValuationId { get; set; }
        public DateTime Date { get; set; }
        public double BitcoinUSDPrice { get; set; }

        public double PricePerCoin { get; set; }
        public double Marketcap { get; set; }
        public bool Modified { get; set; }

        public override bool Equals(object obj)
        {
            AssetValuationItem lItem = obj as AssetValuationItem;
            return ValuationId == lItem.ValuationId &&
                Date == lItem.Date &&
                BitcoinUSDPrice == lItem.BitcoinUSDPrice &&
                PricePerCoin == lItem.PricePerCoin &&
                Marketcap == lItem.Marketcap &&
                Modified == lItem.Modified;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public object Clone()
        {
            AssetValuationItem lResult = new AssetValuationItem();
            lResult.ValuationId = ValuationId;
            lResult.Date = Date;
            lResult.BitcoinUSDPrice = BitcoinUSDPrice;
            lResult.PricePerCoin = PricePerCoin;
            lResult.Marketcap = Marketcap;
            lResult.Modified = Modified;
            return lResult;
        }
    }
}
