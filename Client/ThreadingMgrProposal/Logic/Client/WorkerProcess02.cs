using Pandora.Client.ThreadingMgrProposal.Logic.Base;
using Pandora.Client.ThreadingMgrProposal.Logic.Business.EventsArgs;
using Pandora.Client.Universal.Threading;
using System;
using System.Threading;

namespace Pandora.Client.ThreadingMgrProposal.Logic.Client
{
    /// <summary>
    /// example implementation of IWorkerProcess interface using AnotherWorkingEventArgs and FinishEventArgs like arguments of events callbacks
    /// </summary>
    public class WorkerProcess02 : MethodJetThread, IWorkerProcess<AnotherWorkingEventArgs, FinishEventArgs>
    {
        public event EventHandler<AnotherWorkingEventArgs> OnWorking;

        public event EventHandler<FinishEventArgs> OnFinish;

        public WorkerProcess02()
        {
        }

        protected override void InternalInitialize()
        {
            base.InternalInitialize();
            this.BeginInvoke(new Action(Process), null);
        }

        /// <summary>
        /// Custom logic to implement during processing work
        /// </summary>
        public void Process()
        {
            Thread.Sleep(200);
            var args = new AnotherWorkingEventArgs("on Working in proc 2...");
            if (OnWorking != null)
                SynchronizingObject.BeginInvoke(OnWorking, new object[] { null, args });
            this.BeginInvoke(new Action(Process), null);
        }

        ///Custom logic to implement when end work
        public void Stop()
        {
            var args = new FinishEventArgs("Finished!");
            if (OnFinish != null)
                SynchronizingObject.BeginInvoke(OnFinish, new object[] { null, args });
            Terminate();
        }

        public override void Dispose()
        {
            base.Dispose();
            this.ActiveThread.Join();
        }
    }
}