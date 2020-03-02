using Pandora.Client.Exchange.Exchanges;
using Pandora.Client.Exchange.Factories;
using Pandora.Client.Universal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Pandora.Client.Exchange.Factories
{
    public class PandoraExchangeFactoryProducer
    {
        private static PandoraExchangeFactoryProducer FInstance;
        private ReadOnlyDictionary<AvailableExchangesList, Type> FExchangeTypeInventory;
        private ConcurrentDictionary<string, IPandoraExchangeFactory> FExchangeFactoryCache;

        public IDictionary<int, string> Inventory => FExchangeTypeInventory.Keys.ToDictionary(lkey => (int) lkey, lKey => lKey.ToString());        

        private PandoraExchangeFactoryProducer()
        {
            FExchangeFactoryCache = new ConcurrentDictionary<string, IPandoraExchangeFactory>();
            var lExchangeInventory = new Dictionary<AvailableExchangesList, Type>();

            var lCurrentAssembly = Assembly.GetExecutingAssembly();
            var lExchangeTypes = lCurrentAssembly.GetTypes().Where(lType => typeof(IPandoraExchanger).IsAssignableFrom(lType) && !lType.IsInterface);
            if (!lExchangeTypes.Any())
                throw new Exception("Exchange libraries not found");
            foreach (var lExchangeType in lExchangeTypes)
            {
                var lIdentifierField = lExchangeType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(fi => fi.IsLiteral && !fi.IsInitOnly).First();
                var lIdentifier = (AvailableExchangesList) lIdentifierField.GetValue(null);
                if (lIdentifier <= 0)
                    throw new Exception($"Missing exchange identifier in class {lExchangeType.Name}");
                lExchangeInventory.Add(lIdentifier, lExchangeType);
            }
            FExchangeTypeInventory = new ReadOnlyDictionary<AvailableExchangesList, Type>(lExchangeInventory);
        }

        public static PandoraExchangeFactoryProducer GetInstance()
        {
            if (FInstance == null)
                FInstance = new PandoraExchangeFactoryProducer();
            return FInstance;
        }

        private string GetUserIdentifier(string aUsername, string aEmail, int aProfileID)
        {
            return string.Concat(aProfileID, "_", aUsername, "_", aEmail);
        }

        public IPandoraExchangeFactory GetExchangeFactory(string aUsername, string aEmail, int aProfileID)
        {
            var lUserIdentifier = GetUserIdentifier(aUsername, aEmail, aProfileID);
            if (!FExchangeFactoryCache.TryGetValue(lUserIdentifier, out IPandoraExchangeFactory lExchangeFactory))
            {
                lExchangeFactory = new PandoraExchangeFactory(FExchangeTypeInventory);
                FExchangeFactoryCache.TryAdd(lUserIdentifier, lExchangeFactory);
            }
            return lExchangeFactory;
        }
    }
}