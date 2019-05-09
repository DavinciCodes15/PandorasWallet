using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pandora.Client.ThreadingMgrProposal.Logic.Business.Mgr;
using Pandora.Client.ThreadingMgrProposal.Logic.Client;

namespace Pandora.Client.ThreadingMgrProposal.Test
{
    [TestClass]
    public class IWorksMgrTest
    {
        [TestMethod]
        public void IWorksMgr_WhenListIsEmpty()
        {
            //Arrange
            IWorksMgr lThreadsMgr = new WorksMgr();

            //Act

            //Assert
            Assert.AreEqual(lThreadsMgr.Count(), 0);
        }

        [TestMethod]
        public void IWorksMgr_WhenItemIsAdded()
        {
            //Arrange
            IWorksMgr lThreadsMgr = new WorksMgr();
            var lIdWorker01 = "worker01";
            var lWorker01 = new WorkerProcess01(100);

            //Act
            lThreadsMgr.AddProccess(lIdWorker01, lWorker01);

            //Assert
            Assert.AreEqual(lThreadsMgr.Count(), 1);
        }

        [TestMethod]
        public void IWorksMgr_WhenItemIsRemoved()
        {
            //Arrange
            IWorksMgr lThreadsMgr = new WorksMgr();

            var lIdWorker01 = "worker01";
            var lWorker01 = new WorkerProcess01(100);

            //Act
            lThreadsMgr.AddProccess(lIdWorker01, lWorker01);
            lThreadsMgr.RemoveProccess(lIdWorker01);

            //Assert
            Assert.AreEqual(lThreadsMgr.Count(), 0);
        }

        [TestMethod]
        public void IWorksMgr_WhenGetExistentItem()
        {
            //Arrange
            IWorksMgr lThreadsMgr = new WorksMgr();

            var lIdWorker01 = "worker01";
            var lWorker01 = new WorkerProcess01(100);

            //Act
            lThreadsMgr.AddProccess(lIdWorker01, lWorker01);
            var lEvalWorker = lThreadsMgr.Get<WorkerProcess01>(lIdWorker01);

            //Assert
            Assert.AreEqual(lEvalWorker.GetHashCode() == lWorker01.GetHashCode(), true);
        }

        [TestMethod]
        public void IWorksMgr_WhenGetInexistentItem()
        {
            //Arrange
            IWorksMgr lThreadsMgr = new WorksMgr();

            //Act
            var lEvalWorker = lThreadsMgr.Get<WorkerProcess01>("another item key");

            //Assert
            Assert.AreEqual(lEvalWorker == null, true);
        }
    }
}