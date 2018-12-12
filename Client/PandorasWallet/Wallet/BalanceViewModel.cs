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
using System.Collections.Generic;

namespace Pandora.Client.PandorasWallet.Wallet
{
    public class BalanceViewModel
    {
        private decimal FConfirmed;
        private decimal FUnconfirmed;
        private decimal FUnconfirmedReceived;
        private decimal FUnconfirmedSent;
        private ulong FCoin;
        private ushort FPrecision;

        public ulong Confirmed => (ulong)(FConfirmed * FCoin);

        public bool IsEmpty { get; private set; }

        public ulong Unconfirmed => (ulong)(Math.Abs(FUnconfirmed) * FCoin);

        public bool NegativeUnconfirmed => FUnconfirmed < 0;

        public string UnconfirmedSent => string.Format(new System.Globalization.NumberFormatInfo() { NumberDecimalDigits = FPrecision }, "{0:F}", FUnconfirmedSent);
        public string UnconfirmedReceived => string.Format(new System.Globalization.NumberFormatInfo() { NumberDecimalDigits = FPrecision }, "{0:F}", FUnconfirmedReceived);

        public ulong Total
        {
            get
            {
                if (NegativeUnconfirmed)
                {
                    return Confirmed - Unconfirmed;
                }
                else
                {
                    return Confirmed + Unconfirmed;
                }
            }
        }

        public uint CurrencyID { get; private set; }

        private Dictionary<string, Tuple<ulong, ulong>> FConfirmedBalanceByAddress;

        private Dictionary<string, Tuple<ulong, ulong>> FUnConfirmedBalanceByAddress;

        public BalanceViewModel()
        {
            FCoin = 100000000;
            FPrecision = 8;
            FConfirmed = 0;
            FUnconfirmed = 0;
            IsEmpty = true;
        }

        public BalanceViewModel(uint aCurrencyId, Dictionary<string, Tuple<ulong, ulong>> aConfirmedBalanceByAddress, Dictionary<string, Tuple<ulong, ulong>> aUnconfirmedBalanceByAddress)
        {
            FConfirmedBalanceByAddress = aConfirmedBalanceByAddress;
            FUnConfirmedBalanceByAddress = aUnconfirmedBalanceByAddress;
            IsEmpty = false;
            SetValues();
        }

        public void SetCoinPrecision(ushort aPrecision)
        {
            FCoin = (ulong)Math.Pow(10, aPrecision);
            FPrecision = aPrecision;
        }

        public void SetNewData(Dictionary<string, Tuple<ulong, ulong>> aConfirmedBalanceByAddress, Dictionary<string, Tuple<ulong, ulong>> aUnconfirmedBalanceByAddress)
        {
            FConfirmedBalanceByAddress = aConfirmedBalanceByAddress;
            FUnConfirmedBalanceByAddress = aUnconfirmedBalanceByAddress;
            IsEmpty = false;
            SetValues();
        }

        private void SetValues()
        {
            FConfirmed = 0;
            FUnconfirmedReceived = 0;
            FUnconfirmedSent = 0;
            FUnconfirmed = 0;

            foreach (Tuple<ulong, ulong> it in FConfirmedBalanceByAddress.Values)
            {
                FConfirmed = it.Item1 > it.Item2 ? FConfirmed + ((decimal)(it.Item1 - it.Item2) / FCoin) : FConfirmed - ((decimal)(it.Item2 - it.Item1) / FCoin);
            }

            foreach (Tuple<ulong, ulong> it in FUnConfirmedBalanceByAddress.Values)
            {
                FUnconfirmedReceived += (decimal)it.Item1 / FCoin;
                FUnconfirmedSent += (decimal)it.Item2 / FCoin;

                FUnconfirmed = it.Item1 > it.Item2 ? FUnconfirmed + ((decimal)(it.Item1 - it.Item2) / FCoin) : FUnconfirmed - ((decimal)(it.Item2 - it.Item1) / FCoin);
            }

            if (FConfirmed < 0 && FUnconfirmed < 0)
            {
                throw new Exception("Invalid Balance");
            }

            if (FConfirmed < 0)
            {
                FConfirmed = 0;
                Utils.PandoraLog.GetPandoraLog().Write("Wallet Error: Invalid confirmed balance detected. Please review transactions.", DateTime.Now);
            }
        }

        public override string ToString()
        {
            decimal lSathoshiBalance = Total;
            decimal lBalance = lSathoshiBalance / FCoin;

            return string.Format(new System.Globalization.NumberFormatInfo() { NumberDecimalDigits = FPrecision }, "{0:F}", lBalance);
        }

        public string ConfirmedToString()
        {
            return string.Format(new System.Globalization.NumberFormatInfo() { NumberDecimalDigits = FPrecision }, "{0:F}", FConfirmed);
        }

        public string UnconfirmedToString()
        {
            return string.Format(new System.Globalization.NumberFormatInfo() { NumberDecimalDigits = FPrecision }, "{0:F}", FUnconfirmed);
        }
    }
}