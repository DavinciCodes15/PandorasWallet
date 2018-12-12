using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.ClientLib
{
    public interface ICurrencyAccountList : IEnumerable<CurrencyAccount>
    {
        void AddCurrencyAccount(uint aCurrencyId, string aAddress);

#if DEBUG

        bool RemoveMonitoredAddress(ulong aCurrencyId);

#endif

        int IndexOfAddress(uint aCurrencyId, string aAddress);

        CurrencyAccount this[int i] { get; }

        CurrencyAccount[] GetById(uint aId);
    }
}