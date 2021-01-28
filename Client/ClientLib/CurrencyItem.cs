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
using Pandora.Client.ClientLib.Contracts;
using Pandora.Client.Crypto.Currencies;

namespace Pandora.Client.ClientLib
{
    [Serializable]
    public class CurrencyItem : ICloneable, ICurrencyItem
    {
        public CurrencyItem()
        {
        }

        public long Id { get; set; }

        public string Name { get; set; }

        public string Ticker { get; set; }

        public ushort Precision { get; set; }

        public DateTime LiveDate { get; set; }

        public int MinConfirmations { get; set; }

        public byte[] Icon { get; set; }

        public long FeePerKb { get; set; }

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

        public ICurrencyItem CopyFrom(ICurrencyItem aCurrencyItem)
        {
            Id = aCurrencyItem.Id;
            Name = aCurrencyItem.Name;
            Ticker = aCurrencyItem.Ticker;
            Precision = aCurrencyItem.Precision;
            MinConfirmations = aCurrencyItem.MinConfirmations;
            LiveDate = aCurrencyItem.LiveDate;
            Icon = aCurrencyItem.Icon;
            FeePerKb = aCurrencyItem.FeePerKb;
            ChainParamaters = aCurrencyItem.ChainParamaters;
            CurrentStatus = aCurrencyItem.CurrentStatus;
            return this;
        }

        public object Clone()
        {
            return (new CurrencyItem().CopyFrom(this));
        }
    }
}