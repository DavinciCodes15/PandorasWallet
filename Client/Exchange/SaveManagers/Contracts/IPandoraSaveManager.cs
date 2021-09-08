using Pandora.Client.ClientLib.Contracts;
using Pandora.Client.Exchange.Models;
using Pandora.Client.Exchange.SaveManagers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.SaveManagers
{
    public interface IPandoraSaveManager : IDisposable, IExchangeKeyValueHandled
    {
        int Version { get; }
        bool Initialized { get; }

        void Initialize(params string[] aInitializeParamethers);

        bool ReadOrderLogs(int aOrderID, out List<OrderMessage> aMessages);

        IEnumerable<DBUserTradeOrder> ReadOrders(ICurrencyIdentity aCurrency = null, int? aExchangeID = null);

        bool WriteOrderLog(int aOrderID, string aMessage, OrderMessage.OrderMessageLevel aMessageLevel);

        int? WriteOrder(UserTradeOrder aMarketTransaction);

        bool UpdateOrder(UserTradeOrder aMarketTransaction, OrderStatus aStatus);

        PandoraExchangeProfile[] LoadProfiles();

        bool SaveProfile(PandoraExchangeProfile aProfile);

        void DeleteProfile(int aProfileID);
    }
}