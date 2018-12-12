using System;

namespace Pandora.Client.ClientLib
{
    public class UserStatus
    {
        public UserStatus()
        {
        }

        public UserStatus(bool aActive, string aExtInfo, DateTime aDate)
        {
            StatusDate = aDate;
            Active = aActive;
            ExtendedInfo = aExtInfo;
        }

        /// <summary>
        /// Date the status was created.
        /// </summary>
        public DateTime StatusDate { get; private set; }

        public bool Active { get; private set; }

        public string ExtendedInfo { get; private set; }
    }
}