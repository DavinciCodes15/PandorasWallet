using System;

namespace Pandora.Client.ThreadingMgrProposal.Logic.Business.EventsArgs
{
    /// <summary>
    /// Example of inheritance of EventArgs to use in IWorkerProcess
    /// </summary>
    public class FinishEventArgs : EventArgs
    {
        public string FMsg { get; set; }

        public FinishEventArgs(string aMsg)
        {
            this.FMsg = aMsg;
        }
    }
}