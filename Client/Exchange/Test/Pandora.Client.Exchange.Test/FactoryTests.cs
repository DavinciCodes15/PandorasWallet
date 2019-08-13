using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pandora.Client.Exchange.Exchanges;
using Pandora.Client.Exchange.Factories;
using System;

namespace Pandora.Client.Exchange.Test
{
    [TestClass]
    public class FactoryTests
    {
        [TestMethod]
        public void FactoryInstanceGetTest()
        {
            var lFactoryProducer = PandoraExchangeFactoryProducer.GetInstance();
            Assert.IsNotNull(lFactoryProducer);
            var lFactoryProducer2 = PandoraExchangeFactoryProducer.GetInstance();
            Assert.IsTrue(Object.ReferenceEquals(lFactoryProducer, lFactoryProducer2));

            Assert.IsTrue(lFactoryProducer.Inventory.Count > 0);

            var lFactoryExchange = lFactoryProducer.GetExchangeFactory("Bittrex");
            Assert.IsNotNull(lFactoryExchange);
            var lFactoryExchange2 = lFactoryProducer.GetExchangeFactory(10);
            Assert.IsTrue(Object.ReferenceEquals(lFactoryExchange, lFactoryExchange2));

            IPandoraExchange lBittrexInstance = lFactoryExchange.GetNewPandoraExchange("testkey", "testpass");
            Assert.IsNotNull(lBittrexInstance);
            Assert.AreEqual(lBittrexInstance.Name, "Bittrex");
            Assert.AreEqual(lBittrexInstance.ID, (uint)10);
        }
    }
}