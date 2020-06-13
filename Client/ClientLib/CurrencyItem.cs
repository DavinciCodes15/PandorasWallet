//   Copyright 2017-2019 Davinci Codes
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Also use the software for non-commercial purposes.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE

using System;
using System.Numerics;
using Pandora.Client.Crypto.Currencies;

namespace Pandora.Client.ClientLib
{
    [Serializable]
    public class CurrencyItem : ICloneable
    {
        public CurrencyItem()
        {
        }

        public CurrencyItem(long aId, string aName, string aTicker, ushort aPrecision, DateTime aLiveDate, int aMinConfirmations, byte[] aIcon, long aFeePerKb, ChainParams aChainParams, CurrencyStatus aStatus)
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

        public ushort Precision { get; private set; }

        public DateTime LiveDate { get; private set; }

        public int MinConfirmations { get; private set; }

        public byte[] Icon { get; private set; }

        public long FeePerKb { get; private set; }

        public ChainParams ChainParamaters { get; set; }

        public CurrencyStatus CurrentStatus { get; set; }

        public decimal AmountToDecimal(BigInteger aAmount)
        {
            return (decimal) aAmount / Convert.ToDecimal(Math.Pow(10, Precision));
        }

        public long AmountToLong(decimal aAmount)
        {
            return Convert.ToInt64(aAmount * Convert.ToDecimal(Math.Pow(10, Precision)));
        }

        public BigInteger AmountToBigInteger(decimal aAmount)
        {
            return new BigInteger(aAmount * Convert.ToDecimal(Math.Pow(10, Precision)));
        }

        public CurrencyItem CopyTo(CurrencyItem aDestinationItem)
        {
            aDestinationItem.Id = Id;
            aDestinationItem.Name = Name;
            aDestinationItem.Ticker = Ticker;
            aDestinationItem.Precision = Precision;
            aDestinationItem.MinConfirmations = MinConfirmations;
            aDestinationItem.LiveDate = LiveDate;
            aDestinationItem.Icon = Icon;
            aDestinationItem.FeePerKb = FeePerKb;
            aDestinationItem.ChainParamaters = ChainParamaters;
            aDestinationItem.CurrentStatus = CurrentStatus;
            return aDestinationItem;
        }

        public object Clone()
        {
            return CopyTo(new CurrencyItem());
        }
    }
}