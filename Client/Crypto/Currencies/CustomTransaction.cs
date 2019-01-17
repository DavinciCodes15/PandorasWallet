using System.Collections.Generic;

namespace Pandora.Client.Crypto.Currencies
{
    public static class CustomTransaction
    {
        private delegate Transaction TransactionGenerator(string aHexData = null, Network aNetwork = null);

        private static Dictionary<long, TransactionGenerator> FTxFunc = new Dictionary<long, TransactionGenerator>();

        static CustomTransaction()
        {
            FTxFunc.Add(10010, new TransactionGenerator((aHex, aNetwork) => (aHex == null || aNetwork == null) ? new AltReddCoinTransaction() : new AltReddCoinTransaction(aHex, aNetwork)));
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
    }
}