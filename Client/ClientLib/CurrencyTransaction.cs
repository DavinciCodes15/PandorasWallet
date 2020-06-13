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

using System.Collections.Generic;
using Newtonsoft.Json;
using Pandora.Client.Crypto.Currencies;
using System.Numerics;

namespace Pandora.Client.ClientLib
{
    public class CurrencyTransaction : ICurrencyTransaction
    {
        public CurrencyTransaction()
        {
        }

        public CurrencyTransaction(TransactionUnit[] aInputs, TransactionUnit[] aOutputs, long aTxFee, long aCurrencyId)
        {
            Inputs = aInputs;
            Outputs = aOutputs;
            TxFee = aTxFee;
            CurrencyId = aCurrencyId;
        }

        public TransactionUnit[] Inputs { get; private set; }

        public TransactionUnit[] Outputs { get; private set; }

        public virtual long TxFee { get; set; }

        public long CurrencyId { get; set; }

        ITransactionUnit[] ICurrencyTransaction.Inputs => this.Inputs;

        ITransactionUnit[] ICurrencyTransaction.Outputs => this.Outputs;

        public virtual void AddInput(TransactionUnit[] aInputArray)
        {
            var lList = new List<TransactionUnit>();
            if (Inputs != null)
                lList.AddRange(Inputs);
            lList.AddRange(aInputArray);
            Inputs = lList.ToArray();
        }

        public virtual void AddInput(TransactionUnit aInputArray)
        {
            var lList = new List<TransactionUnit>();
            if (Inputs != null)
                lList.AddRange(Inputs);
            lList.Add(aInputArray);
            Inputs = lList.ToArray();
        }

        public virtual void AddInput(BigInteger aAmount, string aAddress, long aId = 0)
        {
            var lList = new List<TransactionUnit>();
            if (Inputs != null)
                lList.AddRange(Inputs);
            lList.Add(new TransactionUnit(aId, aAmount, aAddress));
            Inputs = lList.ToArray();
        }

        public virtual void AddOutput(TransactionUnit[] aOutputArray)
        {
            var lList = new List<TransactionUnit>();
            if (Outputs != null)
                lList.AddRange(Outputs);
            lList.AddRange(aOutputArray);
            Outputs = lList.ToArray();
        }

        public virtual void AddOutput(BigInteger aAmount, string aAddress, int aIndex = -1, long aId = 0, string aTxID = null)
        {
            var lList = new List<TransactionUnit>();
            if (Outputs != null)
                lList.AddRange(Outputs);
            lList.Add(new TransactionUnit(aId, aAmount, aAddress, aIndex, aTxID));
            Outputs = lList.ToArray();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}