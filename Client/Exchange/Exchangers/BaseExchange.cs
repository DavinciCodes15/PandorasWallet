using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.Exchangers
{
    public abstract class AbstractExchange
    {
        public const AvailableExchangesList Identifier = 0;
        public abstract string Name { get; }
        public abstract int ID { get; }

        private string FUniqueID = null;

        public string UID
        {
            get
            {
                if (FUniqueID == null)
                    FUniqueID = Guid.NewGuid().ToString();
                return FUniqueID;
            }
        }
    }
}