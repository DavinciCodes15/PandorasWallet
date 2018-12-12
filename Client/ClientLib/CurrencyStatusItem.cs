using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.ClientLib
{
    /// <summary>
    /// Active = The currency is tradable and can be used for sending and recieving coins.
    /// Disabled =  The currency is down and may not return to active status.
    /// Maintenance = The currency is needs to be fixed on the server side before tx can be sent.
    /// </summary>
    public enum CurrencyStatus { Active, Disabled, Maintenance, Updated }

    public class CurrencyStatusItem
    {
        public CurrencyStatusItem()
        {
        }

        public CurrencyStatusItem(long aStatusId, ulong aCurrencyId, DateTime aStatusTime, CurrencyStatus aStatus, string aExtendedInfo, ulong aBlockHeight)
        {
            StatusId = aStatusId;
            CurrencyId = aCurrencyId;
            StatusTime = aStatusTime;
            Status = aStatus;
            ExtendedInfo = aExtendedInfo;
            BlockHeight = aBlockHeight;
        }

        public long StatusId { get; private set; }

        public ulong CurrencyId { get; private set; }

        public DateTime StatusTime { get; private set; }

        public CurrencyStatus Status { get; private set; }

        public string ExtendedInfo { get; private set; }

        public ulong BlockHeight { get; private set; }
    }
}