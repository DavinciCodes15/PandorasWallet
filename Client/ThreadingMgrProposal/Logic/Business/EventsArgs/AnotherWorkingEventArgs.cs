using System;

namespace Pandora.Client.ThreadingMgrProposal.Logic.Business.EventsArgs
{
    /// <summary>
    /// Example of inheritance of EventArgs to use in IWorkerProcess
    /// </summary>
    public class AnotherWorkingEventArgs : EventArgs
    {
        public string FMsg { get; set; }

        public AnotherWorkingEventArgs(string aMsg)
        {
            this.FMsg = aMsg;
        }
    }
}