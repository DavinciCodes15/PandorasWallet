using System;

namespace Pandora.Client.ThreadingMgrProposal.Logic.Business.EventsArgs
{
    /// <summary>
    /// Example of inheritance of EventArgs to use in IWorkerProcess
    /// </summary>
    public class WorkingEventArgs : EventArgs
    {
        public string FMsg { get; set; }
        public int FIndex { get; set; }

        public WorkingEventArgs(string aMsg, int aIndex)
        {
            this.FMsg = aMsg;
            this.FIndex = aIndex;
        }
    }
}