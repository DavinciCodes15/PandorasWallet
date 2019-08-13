using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Pandora.Client.Exchange.Test
{
    [TestClass]
    public class Miscellaneous
    {
        [TestMethod]
        public void TestTimeoutErrorStringDecoding()
        {
            var lTxId = SetOrderWatcherForTransactionID("Server Error: Error sending transaction for Bitcoin(testnet) : Transaction Timeout. TxId: 299a3765ccbe1ecea0cf3cae82e34514d32355ea82d9b1b967a0c33469bc32c1. Error Code: 0");
            Assert.AreEqual(lTxId, "299a3765ccbe1ecea0cf3cae82e34514d32355ea82d9b1b967a0c33469bc32c1");
        }

        private string SetOrderWatcherForTransactionID(string aMessage)
        {
            var lSplitMessage = aMessage.Split(new char[] { '.' });
            var lTxId = lSplitMessage.Where(lMessage => lMessage.Contains("TxId:")).FirstOrDefault()?.Split(new char[] { ':' })[1].Trim();
            return lTxId;
        }
    }
}