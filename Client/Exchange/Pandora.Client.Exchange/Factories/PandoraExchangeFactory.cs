using Pandora.Client.Exchange.Exchanges;
using Pandora.Client.Universal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.Factories
{
    internal class PandoraExchangeFactory : IPandoraExchangeFactory
    {
        private ConcurrentDictionary<AvailableExchangesList, IPandoraExchanger> FExchangeInstances;
        private IReadOnlyDictionary<AvailableExchangesList, Type> FInventoryTypes;
        internal PandoraExchangeFactory(IReadOnlyDictionary<AvailableExchangesList,Type> aInventoryOfTypes)
        {
            FExchangeInstances = new ConcurrentDictionary<AvailableExchangesList, IPandoraExchanger>();
            FInventoryTypes = aInventoryOfTypes;
        }

        public IPandoraExchanger GetPandoraExchange(AvailableExchangesList aExchangeElement)
        {
            if (!FInventoryTypes.ContainsKey(aExchangeElement))
                throw new Exception($"Exchange element with name {aExchangeElement.ToString()} is not present at type inventory.");
            if (!FExchangeInstances.TryGetValue(aExchangeElement, out IPandoraExchanger lExchangeInstance))
            {
                lExchangeInstance = CreateNewExchangeInstance(FInventoryTypes[aExchangeElement]);
                FExchangeInstances.TryAdd(aExchangeElement, lExchangeInstance);
            }            
            return lExchangeInstance;
        }

        public IEnumerable<IPandoraExchanger> GetPandoraExchanges()
        {
            return FExchangeInstances.Values.ToArray();
        }


        private IPandoraExchanger CreateNewExchangeInstance(Type aExchangeType)
        {
            Type lPandoraExchangeType = typeof(IPandoraExchanger);
            if (!lPandoraExchangeType.IsAssignableFrom(aExchangeType))
                throw new Exception($"Lib error. Class {aExchangeType.Name} does not implement {lPandoraExchangeType.Name} interface");
            return (IPandoraExchanger) Activator.CreateInstance(aExchangeType,true);
        }


    }
}