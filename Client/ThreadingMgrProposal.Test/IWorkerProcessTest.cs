using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pandora.Client.ThreadingMgrProposal.Logic.Business.EventsArgs;
using Pandora.Client.ThreadingMgrProposal.Logic.Business.Mgr;
using Pandora.Client.ThreadingMgrProposal.Logic.Client;
using System.Threading;

namespace Pandora.Client.ThreadingMgrProposal.Test
{
    [TestClass]
    public class IWorkerProcessTest
    {
        [TestMethod]
        public void IWorkerProcess_CollectValueFromIsWorkingEvent()
        {
            //Arrange
            IWorksMgr lThreadsMgr = new WorksMgr();
            var lWorker01 = new WorkerProcess01(10);
            var lIdWorker = "worker01";
            AutoResetEvent waitHandle = new AutoResetEvent(false);
            var lCount = 0;
            lWorker01.SynchronizingObject = lWorker01;

            //Act
            lWorker01.OnWorking += (object sender, WorkingEventArgs e) => lCount = e.FIndex;
            lWorker01.OnFinish += (object sender, FinishEventArgs e) => waitHandle.Set();
            lThreadsMgr.AddProccess(lIdWorker, lWorker01);
            lThreadsMgr.RunProccess(lIdWorker);
            waitHandle.WaitOne();

            //Assert
            Assert.AreEqual(lCount, 10);
        }

        [TestMethod]
        public void IWorkerProcess_OnFinishEvent()
        {
            //Arrange
            IWorksMgr lThreadsMgr = new WorksMgr();
            var lWorker01 = new WorkerProcess01(10);
            var lIdWorker = "worker01";
            AutoResetEvent waitHandle = new AutoResetEvent(false);
            var lIsFinished = false;
            lWorker01.SynchronizingObject = lWorker01;

            //Act
            lWorker01.OnFinish += (object sender, FinishEventArgs e) => { lIsFinished = true; waitHandle.Set(); };
            lThreadsMgr.AddProccess(lIdWorker, lWorker01);
            lThreadsMgr.RunProccess(lIdWorker);
            waitHandle.WaitOne();

            //Assert
            Assert.AreEqual(lIsFinished, true);
        }
    }
}