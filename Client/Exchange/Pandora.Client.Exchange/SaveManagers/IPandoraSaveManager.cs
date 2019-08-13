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

        bool ReadTransactions(out MarketOrder[] aMarketOrders, string aBaseTicker = null);

        bool WriteOrderLog(int aOrderID, string aMessage, OrderMessage.OrderMessageLevel aMessageLevel);

        bool WriteTransaction(MarketOrder aMarketTransaction);

        bool UpdateTransaction(MarketOrder aMarketTransaction, OrderStatus aStatus);

        PandoraExchangeProfile[] LoadProfiles();

        bool SaveProfile(PandoraExchangeProfile aProfile);

        void DeleteProfile(int aProfileID);
    }
}