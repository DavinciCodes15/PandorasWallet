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

using Pandora.Client.Crypto.Currencies;
using System.Numerics;

namespace Pandora.Client.ClientLib
{
    public class TransactionUnit : ITransactionUnit
    {
        public TransactionUnit()
        {
        }

        public TransactionUnit(long aId, BigInteger aAmount, string aAddress, int aIndex = -1, string aParentTxID = null, string aScript = null)
        {
            Id = aId;
            Amount = aAmount;
            Address = aAddress;
            Index = aIndex;
            TxID = aParentTxID;
            Script = aScript;
        }

        public long Id { get; private set; }

        public BigInteger Amount { get; private set; }

        public string Address { get; private set; }

        public string TxID { get; private set; }

        public int Index { get; private set; }

        public string Script { get; private set; }
    }
}