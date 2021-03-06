﻿//   Copyright 2017-2019 Davinci Codes
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
using System.Numerics;

namespace Pandora.Client.ClientLib
{
    public enum TransactionDirection
    { Credit = 0, Debit = 1, Both = 2, Unknown = 3 }

    public class TransactionRecord : CurrencyTransaction
    {
        public TransactionRecord()
        {
        }

        public TransactionRecord(long aTransactionRecordId, long aCurrencyId, string aTxId, DateTime aTxDate, long aBlock, bool aValid)
        {
            CurrencyId = aCurrencyId;
            TransactionRecordId = aTransactionRecordId;
            TxId = aTxId;
            TxDate = aTxDate;
            Block = aBlock;
            Valid = aValid;
        }

        public static IComparer<TransactionRecord> GetTransactionRecordIdComparer()
        {
            return new IDComparer();
        }

        private class IDComparer : IComparer<TransactionRecord>
        {
            public int Compare(TransactionRecord x, TransactionRecord y)
            {
                return Convert.ToInt32((long) x.TransactionRecordId - (long) y.TransactionRecordId);
            }
        }

        public long TransactionRecordId { get; private set; }

        public string TxId { get; private set; }

        public DateTime TxDate { get; private set; }

        public long Block { get; private set; }

        public bool Valid { get; private set; }

        public const string DefaultAddress = "External Address";

        public BigInteger GetValue(string[] aAddresses, out TransactionDirection aTxType, out string aLastToAddress, out string aLastFromAddress)
        {
            BigInteger lResult = 0;
            BigInteger lAllOutputs = 0;
            BigInteger lAllInputs = 0;
            int lOtherAddressCount = 0;
            aTxType = TransactionDirection.Unknown;
            aLastFromAddress = DefaultAddress;
            aLastToAddress = aLastFromAddress;
            string lLastUserToAddress = null;
            string lLastUserFromAddress = null;
            if (Outputs != null)
                foreach (var lOutput in Outputs)
                {
                    if (Array.IndexOf(aAddresses, lOutput.Address) == -1)
                    {
                        lOtherAddressCount++;  // address not found so count the addresses not belonging to you.
                        aLastToAddress = lOutput.Address;
                    }
                    else
                    {
                        lResult += lOutput.Amount;
                        lLastUserToAddress = lOutput.Address;
                    }
                    lAllOutputs += lOutput.Amount;
                }
            else
                lOtherAddressCount++;  // this is a tx but no outputs then the outputs don;t belong to you
            if (Inputs != null)
                foreach (var lInput in Inputs)
                {
                    if (Array.IndexOf(aAddresses, lInput.Address) == -1)
                    {
                        lOtherAddressCount++;
                        aLastFromAddress = lInput.Address;
                    }
                    else
                    {
                        lResult -= lInput.Amount;
                        lLastUserFromAddress = lInput.Address;
                    }
                    lAllInputs += lInput.Amount;
                }
            else
                lOtherAddressCount++;  // no inputs because we don't store external address then 1 our more inputs are not yours.
            this.TxFee = (long) (lAllInputs - lAllOutputs);
            if (lResult < 0)
            {
                aTxType = TransactionDirection.Debit;
                if (lOtherAddressCount == 0)
                    aTxType = TransactionDirection.Both;
                if (aLastFromAddress == DefaultAddress && lLastUserFromAddress != null)
                    aLastFromAddress = lLastUserFromAddress;
                if (aLastToAddress == DefaultAddress && lLastUserToAddress != null)
                    aLastToAddress = lLastUserToAddress;
            }
            else if (lResult > 0)
            {
                aTxType = TYPE_Credit;
                if (lLastUserToAddress != null)
                    aLastToAddress = lLastUserToAddress;
                if (aLastFromAddress == DefaultAddress)
                {
                    this.TxFee = 0; // we don't know what the fee is because we dont have all the inputs.
                    if (lLastUserFromAddress != null)
                        aLastFromAddress = lLastUserFromAddress;
                }
            }
            return lResult;
        }

        public bool IsEqual(TransactionRecord aTransactionRecord)
        {
            return (TransactionRecordId == aTransactionRecord.TransactionRecordId) &&
                (TxId == aTransactionRecord.TxId) &&
                (TxDate == aTransactionRecord.TxDate) &&
                (Block == aTransactionRecord.Block) &&
                (Valid == aTransactionRecord.Valid);
        }

        private const int TYPE_Credit = 0;
        private const int TYPE_Debit = 1;
        private const int TYPE_Both = 2;
        private const int TYPE_Unknown = 3;
    }
}