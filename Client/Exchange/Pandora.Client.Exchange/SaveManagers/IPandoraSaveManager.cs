using Pandora.Client.Exchange.Objects;
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

        bool ReadOrders(out UserTradeOrder[] aMarketOrders, string aBaseTicker = null, int aExchangeID = -1);

        bool WriteOrderLog(int aOrderID, string aMessage, OrderMessage.OrderMessageLevel aMessageLevel);

        long? WriteOrder(UserTradeOrder aMarketTransaction);

        bool UpdateOrder(UserTradeOrder aMarketTransaction, OrderStatus aStatus);

        PandoraExchangeProfile[] LoadProfiles();

        bool SaveProfile(PandoraExchangeProfile aProfile);

        void DeleteProfile(int aProfileID);
    }
}