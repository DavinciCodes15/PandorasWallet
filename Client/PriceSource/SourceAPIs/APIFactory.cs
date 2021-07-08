using Pandora.Client.PriceSource.Contracts;
using Pandora.Client.PriceSource.SourceAPIs.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.PriceSource.SourceAPIs
{
    internal class APIFactory
    {
        private static ConcurrentDictionary<int, IPriceSourceAPI> FAPIs;

        static APIFactory()
        {
            FAPIs = new ConcurrentDictionary<int, IPriceSourceAPI>();
        }

        private static IEnumerable<IPriceSourceAPI> GetAPIEntities()
        {
            return Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == "Pandora.Client.PriceSource.SourceAPIs.Entities" && t.GetInterfaces().Contains(typeof(IPriceSourceAPI))).Select(t => (IPriceSourceAPI) Activator.CreateInstance(t));
        }

        public static IEnumerable<Tuple<string, IPriceSourceAPI>> GetPriceAPIs()
        {
            var lResult = new List<Tuple<string, IPriceSourceAPI>>();
            if (!FAPIs.Any())
            {
                foreach (var lEntity in GetAPIEntities())
                    FAPIs.TryAdd(lEntity.Id, lEntity);
            }

            foreach (var lAPI in FAPIs)
                if (lAPI.Value.TestConnection())
                    lResult.Add(new Tuple<string, IPriceSourceAPI>(lAPI.Value.Name, lAPI.Value));
                else
                {
                    var lNewIntance = (IPriceSourceAPI) Activator.CreateInstance(lAPI.Value.GetType());
                    lAPI.Value.Dispose();
                    FAPIs.AddOrUpdate(lAPI.Key, lNewIntance, (a, b) => lNewIntance);
                }

            if (lResult == null)
                throw new Exception("Unable to connect to any price source API");

            return lResult;
        }
    }
}