using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pandora.Client.Crypto.Currencies.Controls;

namespace Pandora.Client.ClientLib
{
    public class CurrencyTransaction : ICurrencyTransaction
    {
        public CurrencyTransaction()
        {
        }

        public CurrencyTransaction(TransactionUnit[] aInputs, TransactionUnit[] aOutputs, ulong aTxFee, ulong aCurrencyId)
        {
            Inputs = aInputs;
            Outputs = aOutputs;
            TxFee = aTxFee;
            CurrencyId = aCurrencyId;
        }

        public TransactionUnit[] Inputs { get; private set; }

        public TransactionUnit[] Outputs { get; private set; }

        public virtual ulong TxFee { get; set; }

        public ulong CurrencyId { get; set; }

 
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

        public virtual void AddInput(ulong aAmount, string aAddress, ulong aId = 0)
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

        public virtual void AddOutput(ulong aAmount, string aAddress, ulong aId = 0)
        {
            var lList = new List<TransactionUnit>();
            if (Outputs != null)
                lList.AddRange(Outputs);
            lList.Add(new TransactionUnit(aId, aAmount, aAddress));
            Outputs = lList.ToArray();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}