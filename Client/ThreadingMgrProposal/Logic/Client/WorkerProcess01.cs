using Pandora.Client.ThreadingMgrProposal.Logic.Base;
using Pandora.Client.ThreadingMgrProposal.Logic.Business.EventsArgs;
using Pandora.Client.Universal.Threading;
using System;
using System.Threading;

namespace Pandora.Client.ThreadingMgrProposal.Logic.Client
{
    /// <summary>
    /// example implementation of IWorkerProcess interface using WorkingEventArgs and FinishEventArgs like arguments of events callbacks
    /// </summary>
    public class WorkerProcess01 : MethodJetThread, IWorkerProcess<WorkingEventArgs, FinishEventArgs>
    {
        private readonly int FTimes;
        private int FCount;

        public event EventHandler<WorkingEventArgs> OnWorking;

        public event EventHandler<FinishEventArgs> OnFinish;

        public WorkerProcess01(int aTimes)
        {
            FTimes = aTimes;
        }

        protected override void InternalInitialize()
        {
            base.InternalInitialize();
            this.BeginInvoke(new Action<int>(Process), new object[] { FTimes });
        }

        /// <summary>
        /// Custom logic to implement during processing work
        /// </summary>
        /// <param name="aTimes">number of times that the process will be executed</param>
        public void Process(int aTimes)
        {
            if (FCount++ < aTimes)
            {
                Thread.Sleep(400);
                var args = new WorkingEventArgs("Working...", FCount);
                if (OnWorking != null)
                    SynchronizingObject.BeginInvoke(OnWorking, new object[] { this, args });
                this.BeginInvoke(new Action<int>(Process), new object[] { FTimes });
            }
            else
            {
                var args = new FinishEventArgs("Finished!");
                if (OnFinish != null)
                    SynchronizingObject.BeginInvoke(OnFinish, new object[] { this, args });
                Terminate();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            this.ActiveThread.Join();
        }
    }
}