using Pandora.Client.Exchange.Exchanges;
using Pandora.Client.Universal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.Factories
{
    public class PandoraExchangeFactory : IPandoraExchangeFactory
    {
        protected static Dictionary<Type, PandoraExchangeFactory> FInstances = new Dictionary<Type, PandoraExchangeFactory>();
        protected Type FType;

        protected PandoraExchangeFactory(Type aType)
        {
            FType = aType;
        }

        public static PandoraExchangeFactory GetInstance(Type aType)
        {
            if (!FInstances.TryGetValue(aType, out PandoraExchangeFactory lFactory))
            {
                lFactory = new PandoraExchangeFactory(aType);
                FInstances.Add(aType, lFactory);
            }
            return lFactory;
        }

        public virtual IPandoraExchange GetNewPandoraExchange(params string[] aParams)
        {
            if (!typeof(IPandoraExchange).IsAssignableFrom(FType))
                throw new Exception($"Lib error. Class {FType.Name} does not implement IPandoraExchange interface");
            var lActivatorFlags = BindingFlags.Public | BindingFlags.Instance;
            var lParameters = new object[] { aParams[0], aParams[1] };
            var lResult = (IPandoraExchange)Activator.CreateInstance(FType, lActivatorFlags, null, lParameters, null);
            return lResult;
        }
    }
}