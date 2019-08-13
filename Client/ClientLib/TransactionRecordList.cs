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
using System.Collections;
using System.Collections.Generic;

namespace Pandora.Client.ClientLib
{
    public class TransactionRecordList : IEnumerable<TransactionRecord>
    {
        private List<TransactionRecord> FList = new List<TransactionRecord>();
        private Dictionary<string, long> FAddressLookUpTotal = new Dictionary<string, long>();

        /// <summary>
        /// Create a list of transactions with a optional expected currency id
        /// </summary>
        /// <param name="aExpectedCurrencyId">if set to number higher than zero only transactions with this id can be added.</param>
        public TransactionRecordList(long aExpectedCurrencyId = 0)
        {
            ExpectedCurrencyId = aExpectedCurrencyId;
        }

        public long ExpectedCurrencyId { get; private set; }

        public bool TransactionRecordExists(long aTransactionRecordId)
        {
            foreach (var lTx in FList)
                if (lTx.TransactionRecordId == aTransactionRecordId)
                    return true;
            return false;
        }

        public void AddTransactionRecord(TransactionRecord aTransactionRecord)
        {
            if (ExpectedCurrencyId > 0 && aTransactionRecord.CurrencyId != ExpectedCurrencyId) throw new ArgumentOutOfRangeException("Invalid currency Id added to list.");
            if (!TransactionRecordExists(aTransactionRecord.TransactionRecordId))
            {
                FList.Add(aTransactionRecord);
                AddRemoveLookup(aTransactionRecord, FAddressLookUpTotal, true);
            }
        }

        public int Count { get => FList.Count; }

        private void AddRemoveLookup(TransactionRecord aTransactionRecord, Dictionary<string, long> aLookup, bool aAdd)
        {
            long lAddMultiplyer = 1;
            if (!aAdd) lAddMultiplyer = -1;
            long lRemoveMultiplyer = -1;
            if (!aAdd) lRemoveMultiplyer = 1;
            if (aTransactionRecord.Valid)
            {
                foreach (var lOutput in aTransactionRecord.Outputs)
                    if (!aLookup.ContainsKey(lOutput.Address))
                        aLookup.Add(lOutput.Address, lOutput.Amount * lAddMultiplyer);
                    else
                        aLookup[lOutput.Address] += lOutput.Amount * lAddMultiplyer;
                if (aTransactionRecord.Inputs != null)
                    foreach (var lInputs in aTransactionRecord.Inputs)
                        if (!aLookup.ContainsKey(lInputs.Address))
                            aLookup.Add(lInputs.Address, lInputs.Amount * lRemoveMultiplyer);
                        else
                            aLookup[lInputs.Address] += lInputs.Amount * lRemoveMultiplyer;
            }
        }

        public void RemoveTransactionRecord(TransactionRecord aTransactionRecord)
        {
            if (ExpectedCurrencyId > 0 && aTransactionRecord.CurrencyId != ExpectedCurrencyId) throw new ArgumentOutOfRangeException("Invalid currency Id added to list.");
            if (FList.Remove(aTransactionRecord))
                AddRemoveLookup(aTransactionRecord, FAddressLookUpTotal, false);
        }

        public TransactionRecord this[int index] { get => FList[index]; }

        public long GetAddressAmount(string aAddress)
        {
            if (!FAddressLookUpTotal.ContainsKey(aAddress))
                return 0;
            return FAddressLookUpTotal[aAddress];
        }

        public IEnumerator<TransactionRecord> GetEnumerator()
        {
            return FList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return FList.GetEnumerator();
        }
    }
}
