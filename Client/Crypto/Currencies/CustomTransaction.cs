using System.Collections.Generic;

namespace Pandora.Client.Crypto.Currencies
{
    public static class CustomTransaction
    {
        private delegate Transaction TransactionGenerator(string aHexData = null, Network aNetwork = null);

        private static Dictionary<long, TransactionGenerator> FTxFunc = new Dictionary<long, TransactionGenerator>();

        /// <summary>
        /// Class used to insert custom inhereted transactions classes to be used with Nbitcoin Library.
        /// This not the better way to implement this. Actually it can be improved a lot but is quick to implement
        /// </summary>
        static CustomTransaction()
        {
            FTxFunc.Add(10010, new TransactionGenerator((aHex, aNetwork) => (aHex == null || aNetwork == null) ? new AltReddCoinTransaction() : new AltReddCoinTransaction(aHex, aNetwork)));
            FTxFunc.Add(10009, new TransactionGenerator((aHex, aNetwork) => (aHex == null || aNetwork == null) ? new AltPotCoinTransaction() : new AltPotCoinTransaction(aHex, aNetwork)));
            FTxFunc.Add(10024, new TransactionGenerator((aHex, aNetwork) => (aHex == null || aNetwork == null) ? new AltExtraTimeParamTypeTransaction() : new AltExtraTimeParamTypeTransaction(aHex, aNetwork))); //BlackCoin
            FTxFunc.Add(10022, new TransactionGenerator((aHex, aNetwork) => (aHex == null || aNetwork == null) ? new AltExtraTimeParamTypeTransaction() : new AltExtraTimeParamTypeTransaction(aHex, aNetwork))); //BeanCash
            FTxFunc.Add(10030, new TransactionGenerator((aHex, aNetwork) => (aHex == null || aNetwork == null) ? new AltExtraTimeParamTypeTransaction() : new AltExtraTimeParamTypeTransaction(aHex, aNetwork))); //VeriCoin
            FTxFunc.Add(10050, new TransactionGenerator((aHex, aNetwork) => (aHex == null || aNetwork == null) ? new AltExtraTimeParamTypeTransaction() : new AltExtraTimeParamTypeTransaction(aHex, aNetwork))); //OKCash
            FTxFunc.Add(10070, new TransactionGenerator((aHex, aNetwork) => (aHex == null || aNetwork == null) ? new AltExtraTimeParamTypeTransaction() : new AltExtraTimeParamTypeTransaction(aHex, aNetwork))); //Verge
            FTxFunc.Add(10048, new TransactionGenerator((aHex, aNetwork) => (aHex == null || aNetwork == null) ? new AltExtraTimeParamTypeTransaction() : new AltExtraTimeParamTypeTransaction(aHex, aNetwork))); //Peercoin
            FTxFunc.Add(10054, new TransactionGenerator((aHex, aNetwork) => (aHex == null || aNetwork == null) ? new AltExtraTimeParamTypeTransaction() : new AltExtraTimeParamTypeTransaction(aHex, aNetwork))); //Solarcoin
            FTxFunc.Add(10080, new TransactionGenerator((aHex, aNetwork) => (aHex == null || aNetwork == null) ? new AltExtraTimeParamTypeTransaction() : new AltExtraTimeParamTypeTransaction(aHex, aNetwork))); //Emercoin
            FTxFunc.Add(10100, new TransactionGenerator((aHex, aNetwork) => (aHex == null || aNetwork == null) ? new AltExtraTimeParamTypeTransaction() : new AltExtraTimeParamTypeTransaction(aHex, aNetwork))); //NavCoin
            FTxFunc.Add(10108, new TransactionGenerator((aHex, aNetwork) => (aHex == null || aNetwork == null) ? new AltExtraTimeParamTypeTransaction() : new AltExtraTimeParamTypeTransaction(aHex, aNetwork))); //Sequence
            FTxFunc.Add(10098, new TransactionGenerator((aHex, aNetwork) => (aHex == null || aNetwork == null) ? new AltExtraTimeParamTypeTransaction() : new AltExtraTimeParamTypeTransaction(aHex, aNetwork))); //IOCoin
            FTxFunc.Add(10032, new TransactionGenerator((aHex, aNetwork) => (aHex == null || aNetwork == null) ? new AltExtraTimeParamTypeTransaction() : new AltExtraTimeParamTypeTransaction(aHex, aNetwork))); //CloakCoin
            FTxFunc.Add(10028, new TransactionGenerator((aHex, aNetwork) => (aHex == null || aNetwork == null) ? new AltExtraTimeParamTypeTransaction() : new AltExtraTimeParamTypeTransaction(aHex, aNetwork))); //Evergreencoin
            FTxFunc.Add(10056, new TransactionGenerator((aHex, aNetwork) => (aHex == null || aNetwork == null) ? new AltExtraTimeParamTypeTransaction() : new AltExtraTimeParamTypeTransaction(aHex, aNetwork))); //PinkCoin
    }

        private class AltPotCoinTransaction : Transaction
        {
            public AltPotCoinTransaction()
            {
                Version = 4;
            }

            public AltPotCoinTransaction(string aHex, Network aNetwork) : base(aHex, aNetwork)
            {
                Version = 4;
            }

            private uint nTime = (uint)System.DateTime.UtcNow.ToUnixTimestamp();

            public override void ReadWrite(CoinStream stream)
            {
                base.ReadWrite(stream);
                if (Version > 3)
                {
                    stream.ReadWrite(ref nTime);
                }
            }
        }

        private class AltReddCoinTransaction : Transaction
        {
            public AltReddCoinTransaction()
            {
                Version = 2;
            }

            public AltReddCoinTransaction(string aHex, Network aNetwork) : base(aHex, aNetwork)
            {
                Version = 2;
            }

            private uint nTime = (uint)System.DateTime.UtcNow.ToUnixTimestamp();

            public override void ReadWrite(CoinStream stream)
            {
                base.ReadWrite(stream);
                if (Version > 1)
                {
                    stream.ReadWrite(ref nTime);
                }
            }
        }

        public static bool GetCustomTransaction(long aId, out Transaction aTx)
        {
            aTx = null;

            if (!FTxFunc.ContainsKey(aId))
            {
                return false;
            }

            aTx = FTxFunc[aId].Invoke();

            return true;
        }

        public static bool GetCustomTransaction(long aId, out Transaction aTx, string aHexData, Network aNetwork)
        {
            aTx = null;

            if (!FTxFunc.ContainsKey(aId))
            {
                return false;
            }

            aTx = FTxFunc[aId].Invoke(aHexData, aNetwork);

            return true;
        }

        private class AltExtraTimeParamTypeTransaction : Transaction
        {
            public AltExtraTimeParamTypeTransaction()
            {
            }

            public AltExtraTimeParamTypeTransaction(string aHex, Network aNetwork) : base(aHex, aNetwork)
            {
            }

            private uint nTime = (uint)System.DateTime.UtcNow.ToUnixTimestamp();

            public override void ReadWrite(CoinStream stream)
            {
                stream.ReadWrite(ref nVersion);
                stream.ReadWrite(ref nTime);
                stream.ReadWrite<TxInList, TxIn>(ref vin);
                stream.ReadWrite<TxOutList, TxOut>(ref vout);
                vout.Transaction = this;
                stream.ReadWrite(ref nLockTime);
            }

            public override Transaction Clone(bool cloneCache)
            {
                return new AltExtraTimeParamTypeTransaction(ToHex(), Network);
            }
        }
    }
}