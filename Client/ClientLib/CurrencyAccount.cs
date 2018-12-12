using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.ClientLib
{

    public class CurrencyAccount 
    {
        public CurrencyAccount() { }
        public CurrencyAccount(ulong aId, ulong aCurrencyId, string aAddress)
        {
            Id = aId;
            CurrencyId = aCurrencyId;
            Address = aAddress;
        }

        public ulong Id { get; set; }

        public ulong CurrencyId { get;  set; }

        public string Address { get;  set; }
    }
}
   