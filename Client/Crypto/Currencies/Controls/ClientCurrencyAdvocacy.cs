using System;
using System.Collections.Generic;
using System.Text;

namespace Pandora.Client.Crypto.Currencies.Controls
{
    public class ClientCurrencyAdvocacy : IClientCurrencyAdvocacy
    {
        protected ClientCurrencyAdvocacy(bool aTestnet) { TestNet = aTestnet; }

        public ClientCurrencyAdvocacy(uint aId, IChianParams aChainparams)
        {
            Id = aId;
            ForkFromId = aChainparams.ForkFromId;
            TestNet = aChainparams.Network == ChainParams.NetworkType.TestNet;
            Network = new Bitcoin.BitcoinNetwork(aChainparams);
        }

        public Network Network { get; protected set; }

        private Dictionary<string, uint> FAddressLookup = new Dictionary<string, uint>();

        public bool TestNet { get; protected set; }

        public virtual string Name
        {
            get
            {
                return Network.NetworkName;
            }
        }

        public virtual uint Id { get; private set; }

        /// <summary>
        /// Provides the types of Address and private key generation
        ///
        /// PublicKey     -- Each address has a unque private key that must be accessed indivudally
        /// ExtPublicKey  -- The addresses generated are HD and one private key give user access to all keys.
        /// </summary>
        /// TODO: The return string should be in a list of base type where we select the index of the text so its
        ///       located in one place.
        ///       
        public virtual string KeyType { get { return "PublicKey"; } }

        public virtual long ForkFromId { get; protected set; }

        public string RootSeed { get; set; }

        protected virtual string GetIndexKey(long aIndex)
        {
            if (RootSeed.Length != 32) throw new Exception("RootKey must be 32 Characters long");
            var lKey = RootSeed;
            var lHex = aIndex.ToString("X");
            long lKeyId = Id;
            if (ForkFromId > 0)
                lKeyId = ForkFromId;
            lKey = String.Format("{0:00000}{1}{2}", lKeyId - 1, RootSeed.Substring(4, 32 - 5 - lHex.Length), lHex);
            return lKey;
        }

        public virtual string GetAddress(uint aIndex)
        {
            var lKey = new CCKey(Encoding.ASCII.GetBytes(GetIndexKey(aIndex)));
            string lAddress = lKey.PubKey.GetAddress(Network).ToString();
            if (!FAddressLookup.ContainsKey(lAddress))
                FAddressLookup.Add(lAddress, aIndex);
            return lAddress;
        }

        public virtual string GetPrivateKey(uint aIndex)
        {
            var lKey = new CCKey(Encoding.ASCII.GetBytes(GetIndexKey(aIndex)));
            return lKey.GetCoinSecret(Network).ToString();
        }

        public virtual string SignTransaction(string aTxData, ICurrencyTransaction aValidationInfo)
        {
            Transaction tx = new Transaction(aTxData, Network);
            // check how much is being spent and insure all is spent.
            ulong lTotalSent = 0;
            foreach (var linput in aValidationInfo.Inputs) lTotalSent += linput.Amount;
            ulong lTotalSpending = aValidationInfo.TxFee;
            foreach (var lOutput in aValidationInfo.Outputs) lTotalSpending += lOutput.Amount;
            if (lTotalSent != lTotalSpending)
                throw new Exception("The total of the inputs does not equal the total outputs.");
            // Check output amounts in the transaction to sign.
            int lValidCount = 0;
            foreach (var lOutput in tx.Outputs)
            {
                var lAddress = lOutput.GetAddress(Network);
                foreach (var lValOut in aValidationInfo.Outputs)
                    if (lAddress == lValOut.Address)
                        if ((ulong)lOutput.Value.Satoshi == lValOut.Amount) lValidCount++;
            }
            if (lValidCount != aValidationInfo.Outputs.Length)
                throw new Exception("Transaction to sign is not correct.");

            List<CCKey> lKeys = new List<CCKey>();
            List<ICoin> lCoins = new List<ICoin>();
            for (int i = 0; i < tx.Inputs.Count; i++)
            {
                uint lIndex;
                if (!FAddressLookup.TryGetValue(aValidationInfo.Inputs[i].Address, out lIndex))
                    throw new Exception(string.Format("Address {0} does not exist.", aValidationInfo.Inputs[i].Address));
                lKeys.Add(new CCKey(Encoding.ASCII.GetBytes(GetIndexKey(lIndex))));
                lCoins.Add(new Coin(tx.Inputs[i].PrevOut, new TxOut()
                {
                    ScriptPubKey = tx.Inputs[i].ScriptSig
                }));
            }
            tx.Sign(lKeys.ToArray(), lCoins.ToArray());
            return tx.ToHex();
        }

        public virtual bool IsValidAddress(string aAddress)
        {
            return CoinScriptAddress.IsValid(aAddress, Network);
        }
    }
}
