using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandora.Client.Crypto.Currencies.Controls
{
    public class ClientCurrencyAdvocacy : IClientCurrencyAdvocacy
    {
        private readonly Func<string> FGetRootSeed;

        public ClientCurrencyAdvocacy(long aId, IChianParams aChainparams, Func<string> aGetRootSeed)
        {
            Id = aId;
            ForkFromId = aChainparams.ForkFromId;
            TestNet = aChainparams.Network == ChainParams.NetworkType.TestNet;
            Network = new Bitcoin.BitcoinNetwork(aChainparams);
            if (aGetRootSeed == null) throw new ArgumentNullException("aGetRootSeed function must exist.");
            FGetRootSeed = aGetRootSeed;
        }

        public Network Network { get; protected set; }

        private Dictionary<string, long> FAddressLookup = new Dictionary<string, long>();

        public bool TestNet { get; protected set; }

        public virtual string Name => Network.NetworkName;

        public virtual long Id { get; private set; }

        /// <summary>
        /// Provides the types of Address and private key generation
        ///
        /// PublicKey     -- Each address has a unque private key that must be accessed indivudally
        /// ExtPublicKey  -- The addresses generated are HD and one private key give user access to all keys.
        /// </summary>
        /// TODO: The return string should be in a list of base type where we select the index of the text so its
        ///       located in one place.
        ///
        public virtual string KeyType => "PublicKey";

        public virtual long ForkFromId { get; protected set; }

        public string GetRootSeed()
        {
            return FGetRootSeed();
        }

        protected virtual string GetIndexKey(long aIndex)
        {
            string lRootSeed = GetRootSeed();
            if (lRootSeed.Length != 64)
                throw new Exception("RootKey must be 64 Characters long");

            string lKey = lRootSeed;
            if (aIndex == 0)
            {
                // this is the old format for key generation that had some issues
                // we will use this for the first address but will no longer use the first
                // address in the key list only starting at 1 and above
                aIndex++;
                lKey = lRootSeed.Substring(0, 32).ToUpper();
                string lHex = aIndex.ToString("X");
                long lKeyId = Id;
                lKey = string.Format("{0:00000}{1}{2}", lKeyId - 1, lKey.Substring(4, 32 - 5 - lHex.Length), lHex);
            }
            else
            {
                // New way fixes a lot of issues using all 32 bites of the new key lenghth
                // we will also just tack on to back of the key the markers for currnencies and key index
                string lData = string.Format("{0}FF{1}", aIndex.ToString("X"), Id.ToString("X"));
                lKey = lRootSeed.Substring(0, 64 - lData.Length) + lData;
            }
            return lKey;
        }

        public CCKey GetCCKey(long aIndex)
        {
            CCKey lKey;
            if (aIndex == 0)
                lKey = new CCKey(Encoding.ASCII.GetBytes(GetIndexKey(aIndex)));
            else
                lKey = new CCKey(HexStringToByteArray(GetIndexKey(aIndex)));
            return lKey;
        }

        public virtual string GetAddress(long aIndex)
        {
            var lKey = GetCCKey(aIndex);
            string lAddress;
            if (Network.ChainParams.Capabilities.HasFlag(CapablityFlags.SupportSegWit) && aIndex > 0)
                lAddress = lKey.PubKey.GetSegwitAddress(Network).GetScriptAddress().ToString();
            else
                lAddress = lKey.PubKey.GetAddress(Network).ToString();
            if (!FAddressLookup.ContainsKey(lAddress))
                FAddressLookup.Add(lAddress, aIndex);
            return lAddress;
        }

        public virtual string GetPrivateKey(long aIndex)
        {
            CCKey lKey = GetCCKey(aIndex);
            return lKey.GetCoinSecret(Network).ToString();
        }

        public static byte[] HexStringToByteArray(string aHex)
        {
            if (aHex.Length % 2 != 0) throw new ArgumentException("Hex string must be divisable by 2.");
            byte[] lArray = new byte[aHex.Length / 2];
            for (int i = 0; i < aHex.Length; i += 2)
                lArray[i / 2] = Byte.Parse(aHex.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
            return lArray;
        }

        public virtual string SignTransaction(string aTxData, ICurrencyTransaction aValidationInfo)
        {
            if (!CustomTransaction.GetCustomTransaction(Id, out Transaction tx, aTxData, Network))
                tx = new Transaction(aTxData, Network);
            // check how much is being spent and insure all is spent.
            long lTotalSent = 0;
            foreach (ITransactionUnit linput in aValidationInfo.Inputs)
                lTotalSent += linput.Amount;

            long lTotalSpending = aValidationInfo.TxFee;
            foreach (ITransactionUnit lOutput in aValidationInfo.Outputs)
                lTotalSpending += lOutput.Amount;

            if (lTotalSent != lTotalSpending)
                throw new Exception("The total of the inputs does not equal the total outputs.");
            // Check output amounts in the transaction to sign.
            int lValidCount = 0;
            foreach (TxOut lOutput in tx.Outputs)
            {
                string lAddress = lOutput.GetAddress(Network);
                foreach (ITransactionUnit lValOut in aValidationInfo.Outputs)
                {
                    if (lAddress == lValOut.Address)
                    {
                        if (lOutput.Value.Satoshi == lValOut.Amount)
                        {
                            lValidCount++;
                        }
                    }
                }
            }
            if (lValidCount != aValidationInfo.Outputs.Length)
                throw new Exception("Transaction to sign is not correct.");

            List<CCKey> lKeys = new List<CCKey>();
            List<ICoin> lCoins = new List<ICoin>();
            for (int i = 0; i < tx.Inputs.Count; i++)
            {
                if (!FAddressLookup.TryGetValue(aValidationInfo.Inputs[i].Address, out long lIndex))
                    throw new Exception(string.Format("Address {0} does not exist.", aValidationInfo.Inputs[i].Address));

                lKeys.Add(GetCCKey(lIndex));
                ITransactionUnit lPrevOut = aValidationInfo.Inputs
                                                          .Where(lInput => lInput.TxID == tx.Inputs[i].PrevOut.Hash.ToString() && lInput.Index == tx.Inputs[i].PrevOut.N)
                                                          .FirstOrDefault();
                if (lPrevOut == null) throw new Exception("Failed to found previous tx out information");
                lCoins.Add(new Coin(tx.Inputs[i].PrevOut, new TxOut()
                {
                    ScriptPubKey = tx.Inputs[i].ScriptSig,
                    Value = new Money(lPrevOut.Amount)
                }));
                tx.Inputs[i].ScriptSig = Script.Empty;
            }

            var lHexTest = tx.ToHex();
            tx.Sign(lKeys.ToArray(), lCoins.ToArray());
            return tx.ToHex();
        }

        public virtual bool IsValidAddress(string aAddress)
        {
            return CoinScriptAddress.IsValid(aAddress, Network);
        }

        public virtual string BinaryAddress(string aAddress)
        {
            return CoinScriptAddress.AddressToBinString(aAddress, Network);
        }
    }
}