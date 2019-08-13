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
using Pandora.Client.ClientLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet.Wallet
{
    public enum TransactionViewType
    {
        none,
        debt,
        credit,
        both
    }

    public class TransactionViewModel
    {
        public bool IsEmpty { get; private set; }

        public long Block
        {
            get
            {
                if (FRecord != null)
                {
                    return FRecord.Block;
                }

                return 0;
            }
        }

        public TransactionViewType TransactionType { get; private set; }

        private TransactionRecord FRecord;

        private List<string> FAddresses;

        private long FBlockHeight;

        public int MinConfirmations { get; private set; }

        private long FTxFee = 0;

        public long TxFee
        {
            get
            {
                if (TransactionType == TransactionViewType.debt || TransactionType == TransactionViewType.both)
                {
                    if (FTxFee == 0)
                    {
                        FTxFee = (long)FRecord.Inputs.Select(x => x.Amount).Aggregate((x, y) => x + y) - (long)FRecord.Outputs.Select(x => x.Amount).Aggregate((x, y) => x + y);
                    };
                    return FTxFee;
                }
                else
                {
                    return 0;
                }
            }
        }

        public bool Valid => FRecord.Valid;

        public TransactionUnit ChangeOutput
        {
            get
            {
                if (TransactionType == TransactionViewType.debt || TransactionType == TransactionViewType.both)
                {
                    return FRecord.Outputs.Where(x => FAddresses.Contains(x.Address)).FirstOrDefault();
                }
                else
                {
                    return null;
                }
            }
        }

        public TransactionViewModel()
        {
            IsEmpty = true;
        }

        public TransactionViewModel(TransactionRecord aRecord, IEnumerable<string> aListofAddresses)
        {
            Set(aRecord, aListofAddresses);
        }

        public void Set(TransactionRecord aRecord, IEnumerable<string> aListofAddresses)
        {
            FRecord = aRecord;
            FAddresses = aListofAddresses.ToList();

            List<TransactionUnit> lInputs = FRecord.Inputs != null ? FRecord.Inputs.ToList() : null;
            List<TransactionUnit> lOutputs = FRecord.Outputs.ToList();

            decimal lInputsSum = FRecord.Inputs != null ? lInputs.Where(x => FAddresses.Contains(x.Address)).Sum(x => (decimal)x.Amount) : 0;
            decimal lOutputsSum = lOutputs.Where(x => FAddresses.Contains(x.Address)).Sum(x => (decimal)x.Amount);

            decimal lResult = lOutputsSum - lInputsSum;

            if (lResult < 0)
            {
                FDebt = lResult;
                TransactionType = TransactionViewType.debt;
            }
            else if (lResult > 0)
            {
                FCredit = lResult;
                TransactionType = TransactionViewType.credit;
            }
            else
            {
                TransactionType = TransactionViewType.none;
            }

            if (TransactionType == TransactionViewType.debt && !lOutputs.Exists(x => !FAddresses.Contains(x.Address)))
            {
                TransactionType = TransactionViewType.both;
            }

            IsEmpty = false;
        }

        public void SetBlockHeight(long aBlockHeigth, int aMinConfirmations)
        {
            FBlockHeight = aBlockHeigth;
            MinConfirmations = aMinConfirmations;
        }

        public bool IsMultiple(bool Outputs = false)
        {
            if (Outputs)
            {
                if (FRecord.Inputs != null)
                {
                    return FRecord.Inputs.Count() > 1;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return FRecord.Outputs.Count() > 1;
            }
        }

        public string GetSimpleInputAddress()
        {
            if (FRecord.Inputs != null)
            {
                return FRecord.Inputs.First().Address;
            }
            else
            {
                return "External Address";
            }
        }

        public string GetSimpleOutputAddress()
        {
            return TransactionViewType.credit == TransactionType ? FRecord.Outputs.Where(x => FAddresses.Contains(x.Address)).First().Address : FRecord.Outputs.First().Address;
        }

        public ComboBox GetMultiInputAddress()
        {
            ComboBox lCombo = new ComboBox();

            foreach (TransactionUnit it in FRecord.Inputs)
            {
                lCombo.Items.Add(it.Address);
            }

            return lCombo;
        }

        public ComboBox GetMultiOutputAddress()
        {
            ComboBox lCombo = new ComboBox();

            foreach (TransactionUnit it in FRecord.Outputs)
            {
                lCombo.Items.Add(it.Address);
            }

            return lCombo;
        }

        public DateTime Date => FRecord.TxDate;

        private decimal FCredit;

        public decimal Credit
        {
            get
            {
                if (TransactionType == TransactionViewType.none)
                {
                    return -1;
                }

                return FCredit;
            }
        }

        private decimal FDebt;

        public decimal Debt
        {
            get
            {
                if (TransactionType == TransactionViewType.none)
                {
                    return -1;
                }

                return FDebt;
            }
        }

        public long Confirmation
        {
            get
            {
                long lBlock = FRecord.Block;

                if (lBlock == 0 || FBlockHeight < 0 || lBlock > FBlockHeight)
                {
                    return 0;
                }

                return FBlockHeight - lBlock + 1;
            }
        }

        public bool isConfirmed => Confirmation >= MinConfirmations;

        public string TransactionID => FRecord.TxId;
    }
}