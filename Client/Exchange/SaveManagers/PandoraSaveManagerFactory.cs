using Pandora.Client.Exchange.SaveManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.SaveManagers
{
    public enum SavePlace
    {
        SQLiteDisk = 2
    }

    public class PandoraSaveManagerFactory
    {
        private SavePlace FSelectedSavePlace;

        private static PandoraSaveManagerFactory FInstance;

        private PandoraSaveManagerFactory()
        {
        }

        public static PandoraSaveManagerFactory GetSaveMangerFactory()
        {
            if (FInstance == null)
                FInstance = new PandoraSaveManagerFactory();
            return FInstance;
        }

        public IPandoraSaveManager GetNewPandoraSaveManager(SavePlace aSavePlace)
        {
            switch (aSavePlace)
            {
                case SavePlace.SQLiteDisk:
                    return PandoraExchangeSQLiteSaveManager.GetNewSaveManager();

                default:
                    throw new Exception("No factory manager set for that type of save place");
            }
        }
    }
}