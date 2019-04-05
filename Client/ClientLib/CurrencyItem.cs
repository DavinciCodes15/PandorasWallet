using System;
using Pandora.Client.Crypto.Currencies;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.ClientLib
{
    [Serializable]
    public class CurrencyItem 
    {

        public CurrencyItem()
        { ChainParamaters = null;  }
        public CurrencyItem(long aId, string aName, string aTicker, ushort aPrecision, DateTime aLiveDate, int aMinConfirmations, byte[] aIcon, int aFeePerKb, ChainParams aChainParams, CurrencyStatus aStatus)
        {
            Id = aId;
            Name = aName;
            Ticker = aTicker;
            Precision = aPrecision;
            MinConfirmations = aMinConfirmations;
            LiveDate = aLiveDate;
            Icon = aIcon;
            FeePerKb = aFeePerKb;
            ChainParamaters = aChainParams;
            CurrentStatus = aStatus;
        }

        public long Id { get; private set; }

        public string Name { get; private set; }

        public string Ticker { get; private set; }

        public ushort Precision { get;  private set; }

        public DateTime LiveDate { get;  private set; }

        public int MinConfirmations { get; private set; }

        public byte[] Icon { get;  private set; }

        public int FeePerKb { get; private set; }

        public ChainParams ChainParamaters { get; set; }

        public CurrencyStatus CurrentStatus { get; set; }
    }
}