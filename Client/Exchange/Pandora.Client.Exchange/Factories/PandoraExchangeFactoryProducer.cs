using Pandora.Client.Exchange.Exchanges;
using Pandora.Client.Exchange.Factories;
using Pandora.Client.Universal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pandora.Client.Exchange.Factories
{
    public class PandoraExchangeFactoryProducer
    {
        private static PandoraExchangeFactoryProducer FInstance;
        private Dictionary<uint, Type> FExchangeIdInventory;
        private Dictionary<string, uint> FExchangeNameInventory;

        public Dictionary<uint, string> Inventory
        {
            get
            {
                Dictionary<uint, string> lResult = new Dictionary<uint, string>();
                foreach (var lElement in FExchangeNameInventory)
                    lResult.Add(lElement.Value, lElement.Key);
                return lResult;
            }
        }

        private PandoraExchangeFactoryProducer()
        {
            FExchangeIdInventory = new Dictionary<uint, Type>();
            FExchangeNameInventory = new Dictionary<string, uint>();

            var lCurrentAssembly = Assembly.GetExecutingAssembly();
            var lExchangeTypes = lCurrentAssembly.GetChildrenFromBaseClass<BasePandoraExchange>();
            if (!lExchangeTypes.Any())
                throw new Exception("Exchange libraries not found");

            foreach (var lExchangeType in lExchangeTypes)
            {
                var Name = (string)lExchangeType.GetProperty("ExchangeName", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                var Id = (uint)lExchangeType.GetProperty("ExchangeID", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                if (string.IsNullOrEmpty(Name) || Id == 0)
                    throw new Exception($"Missing exchange identifier in class {lExchangeType.Name}");
                FExchangeNameInventory.Add(Name, Id);
                FExchangeIdInventory.Add(Id, lExchangeType);
            }
        }

        public static PandoraExchangeFactoryProducer GetInstance()
        {
            if (FInstance == null)
                FInstance = new PandoraExchangeFactoryProducer();
            return FInstance;
        }

        public IPandoraExchangeFactory GetExchangeFactory(string aExchangeName)
        {
            if (!FExchangeNameInventory.TryGetValue(aExchangeName, out uint lId))
                throw new Exception("Exchange specified not found, please review paramether given");

            return GetExchangeFactory(lId, FExchangeIdInventory[lId]);
        }

        public IPandoraExchangeFactory GetExchangeFactory(uint aExchangeId)
        {
            if (!FExchangeIdInventory.TryGetValue(aExchangeId, out Type lType))
                throw new Exception("Exchange specified not found, please review paramether given");

            return GetExchangeFactory(aExchangeId, lType);
        }

        private IPandoraExchangeFactory GetExchangeFactory(uint aExchangeId, Type aType)
        {
            switch (aExchangeId)
            {
                default:
                    return PandoraExchangeFactory.GetInstance(aType);
            }
        }
    }
}