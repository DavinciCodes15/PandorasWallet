using Pandora.Client.Crypto.Currencies.Crypto;
using System;
using System.Linq;

namespace Pandora.Client.Crypto.Currencies
{
    public class ForkIdTransaction : Transaction
    {
        public class FormIdIndexInput : IndexedTxIn
        {
            private ForkIdTransaction FInternalForkTx;

            //public override uint256 GetSignatureHash(ICoin coin, SigHash sigHash = SigHash.All)
            //{
            //    //return Transaction.GetSignatureHash(Transaction.Network, coin.GetScriptCode(), Transaction, (int)Index, sigHash, coin.TxOut.Value, coin.GetHashVersion());

            //}
        }

        public uint256 GetSignatureHash(Script scriptCode, int nIn, SigHash nHashType, TxOut spentOutput, HashVersion sigversion, PrecomputedTransactionData precomputedTransactionData)
        {
            if (UsesForkId(nHashType))
            {
                uint nForkHashType = (uint)nHashType;

                nForkHashType |= (uint)ForkID << 8;
                if (spentOutput?.Value == null || spentOutput.Value == TxOut.NullMoney)
                {
                    throw new ArgumentException("The output being signed with the amount must be provided", nameof(spentOutput));
                }

                uint256 hashPrevouts = uint256.Zero;
                uint256 hashSequence = uint256.Zero;
                uint256 hashOutputs = uint256.Zero;

                if ((nHashType & SigHash.AnyoneCanPay) == 0)
                {
                    hashPrevouts = precomputedTransactionData == null ?
                                   GetHashPrevouts() : precomputedTransactionData.HashPrevouts;
                }

                if ((nHashType & SigHash.AnyoneCanPay) == 0 && ((uint)nHashType & 0x1f) != (uint)SigHash.Single && ((uint)nHashType & 0x1f) != (uint)SigHash.None)
                {
                    hashSequence = precomputedTransactionData == null ?
                                   GetHashSequence() : precomputedTransactionData.HashSequence;
                }

                if (((uint)nHashType & 0x1f) != (uint)SigHash.Single && ((uint)nHashType & 0x1f) != (uint)SigHash.None)
                {
                    hashOutputs = precomputedTransactionData == null ?
                                    GetHashOutputs() : precomputedTransactionData.HashOutputs;
                }
                else if (((uint)nHashType & 0x1f) == (uint)SigHash.Single && nIn < Outputs.Count)
                {
                    CoinStream ss = CreateHashWriter(sigversion);
                    ss.ReadWrite(Outputs[nIn]);
                    hashOutputs = GetHash(ss);
                }

                CoinStream sss = CreateHashWriter(sigversion);
                // Version
                sss.ReadWrite(Version);
                // Input prevouts/nSequence (none/all, depending on flags)
                sss.ReadWrite(hashPrevouts);
                sss.ReadWrite(hashSequence);
                // The input being signed (replacing the scriptSig with scriptCode + amount)
                // The prevout may already be contained in hashPrevout, and the nSequence
                // may already be contain in hashSequence.
                sss.ReadWrite(Inputs[nIn].PrevOut);
                sss.ReadWrite(scriptCode);
                sss.ReadWrite(spentOutput.Value.Satoshi);
                sss.ReadWrite((uint)Inputs[nIn].Sequence);
                // Outputs (none/one/all, depending on flags)
                sss.ReadWrite(hashOutputs);
                // Locktime
                sss.ReadWriteStruct(LockTime);
                // Sighash type
                sss.ReadWrite(nForkHashType);

                return GetHash(sss);
            }

            if (nIn >= Inputs.Count)
            {
                return uint256.One;
            }

            SigHash hashType = nHashType & (SigHash)31;

            // Check for invalid use of SIGHASH_SINGLE
            if (hashType == SigHash.Single)
            {
                if (nIn >= Outputs.Count)
                {
                    return uint256.One;
                }
            }

            Script scriptCopy = scriptCode.Clone();
            scriptCopy.FindAndDelete(OpcodeType.OP_CODESEPARATOR);

            if (!CustomTransaction.GetCustomTransaction(CurrencyID, out Transaction txCopy))
            {
                txCopy = new Transaction();
            }

            txCopy.FromBytes(this.ToBytes());
            //Set all TxIn script to empty string
            foreach (TxIn txin in txCopy.Inputs)
            {
                txin.ScriptSig = new Script();
            }
            //Copy subscript into the txin script you are checking
            txCopy.Inputs[nIn].ScriptSig = scriptCopy;

            if (hashType == SigHash.None)
            {
                //The output of txCopy is set to a vector of zero size.
                txCopy.Outputs.Clear();

                //All other inputs aside from the current input in txCopy have their nSequence index set to zero
                foreach (TxIn input in txCopy.Inputs.Where((x, i) => i != nIn))
                {
                    input.Sequence = 0;
                }
            }
            else if (hashType == SigHash.Single)
            {
                //The output of txCopy is resized to the size of the current input index+1.
                txCopy.Outputs.RemoveRange(nIn + 1, txCopy.Outputs.Count - (nIn + 1));
                //All other txCopy outputs aside from the output that is the same as the current input index are set to a blank script and a value of (long) -1.
                for (int i = 0; i < txCopy.Outputs.Count; i++)
                {
                    if (i == nIn)
                    {
                        continue;
                    }

                    txCopy.Outputs[i] = new TxOut();
                }
                //All other txCopy inputs aside from the current input are set to have an nSequence index of zero.
                foreach (TxIn input in txCopy.Inputs.Where((x, i) => i != nIn))
                {
                    input.Sequence = 0;
                }
            }

            if ((nHashType & SigHash.AnyoneCanPay) != 0)
            {
                //The txCopy input vector is resized to a length of one.
                TxIn script = txCopy.Inputs[nIn];
                txCopy.Inputs.Clear();
                txCopy.Inputs.Add(script);
                //The subScript (lead in by its length as a var-integer encoded!) is set as the first and only member of this vector.
                txCopy.Inputs[0].ScriptSig = scriptCopy;
            }

            //Serialize TxCopy, append 4 byte hashtypecode
            CoinStream stream = CreateHashWriter(sigversion);
            txCopy.ReadWrite(stream);
            //stream.ReadWrite(nForkHashType);
            return GetHash(stream);
        }

        private bool UsesForkId(SigHash nHashType)
        {
            return ((uint)nHashType & 0x40u) != 0;
        }

        private static uint256 GetHash(CoinStream stream)
        {
            uint256 preimage = ((HashStream)stream.Inner).GetHash();
            stream.Inner.Dispose();
            return preimage;
        }

        internal uint256 GetHashOutputs()
        {
            uint256 hashOutputs;
            CoinStream ss = CreateHashWriter(HashVersion.Witness);
            foreach (TxOut txout in Outputs)
            {
                ss.ReadWrite(txout);
            }
            hashOutputs = GetHash(ss);
            return hashOutputs;
        }

        internal uint256 GetHashSequence()
        {
            uint256 hashSequence;
            CoinStream ss = CreateHashWriter(HashVersion.Witness);
            foreach (TxIn input in Inputs)
            {
                ss.ReadWrite((uint)input.Sequence);
            }
            hashSequence = GetHash(ss);
            return hashSequence;
        }

        internal uint256 GetHashPrevouts()
        {
            uint256 hashPrevouts;
            CoinStream ss = CreateHashWriter(HashVersion.Witness);
            foreach (TxIn input in Inputs)
            {
                ss.ReadWrite(input.PrevOut);
            }
            hashPrevouts = GetHash(ss);
            return hashPrevouts;
        }
    }

    public interface IHasForkId
    {
        uint ForkId
        {
            get;
        }
    }
}