using Pandora.Client.ThreadingMgrProposal.Logic.Business.EventsArgs;
using Pandora.Client.ThreadingMgrProposal.Logic.Business.Mgr;
using Pandora.Client.ThreadingMgrProposal.Logic.Client;
using System;
using System.Windows.Forms;

namespace Pandora.Client.ThreadingMgrProposal
{
    public partial class Form1 : Form
    {
        private IWorksMgr FThreadsMgr = new WorksMgr();

        #region constructor

        public Form1()
        {
            InitializeComponent();
            this.btnEnd2.Enabled = false;
        }

        #endregion constructor

        #region events ui

        private void Button1_Click(object sender, EventArgs e)
        {
            //custom logic in ui process
            btnWorking.Enabled = false;

            //create instance of IWorkerProcess
            var idWorker = "w01";
            var lProc01 = new WorkerProcess01(40) { SynchronizingObject = this };
            //register handlers callbacks
            lProc01.OnWorking += WorkerProccess01_OnWorking;
            lProc01.OnFinish += WorkerProccess01_OnFinish;
            lProc01.OnTerminated += (object aSender, EventArgs aE) => FThreadsMgr.RemoveProccess(idWorker);

            //add instance to mgr
            FThreadsMgr.AddProccess(idWorker, lProc01);

            //run process instance
            FThreadsMgr.RunProccess(idWorker);
        }

        private void BtnProcess2_Click(object sender, EventArgs e)
        {
            //custom logic in ui process
            btnProcess2.Enabled = false;
            btnEnd2.Enabled = true;
            txtWorking2.Text = "";
            txtFinish2.Text = "";

            //create instance of IWorkerProcess
            var idWorker = "w02";
            var lProc02 = new WorkerProcess02() { SynchronizingObject = this };
            //register handlers callbacks
            lProc02.OnWorking += WorkerProccess02_OnWorking;
            lProc02.OnFinish += WorkerProccess02_OnFinish;
            lProc02.OnTerminated += (object aSender, EventArgs aE) => FThreadsMgr.RemoveProccess(idWorker);

            //add instance to mgr
            FThreadsMgr.AddProccess(idWorker, lProc02);

            //run process instance
            FThreadsMgr.RunProccess(idWorker);
        }

        private void BtnEnd2_Click(object sender, EventArgs e)
        {
            //get instance from mgr using key object
            var proc2 = FThreadsMgr.Get<WorkerProcess02>("w02");
            //stop procesing
            proc2.Stop();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            FThreadsMgr.Dispose();
        }

        #endregion events ui

        #region events workers (methodsjet objects)

        private void WorkerProccess01_OnWorking(object sender, WorkingEventArgs e)
        {
            //custom logic in ui process
            var lMsg = $"{txtWorking.Text} {Environment.NewLine} {e.FIndex} - {DateTime.Now.ToString("HH:mm:ss.fff")} - {e.FMsg}";
            txtWorking.Text = lMsg;
            txtWorking.SelectionStart = txtWorking.TextLength;
            txtWorking.ScrollToCaret();
        }

        private void WorkerProccess01_OnFinish(object sender, FinishEventArgs e)
        {
            //custom logic in ui process
            var lMsg = $"{DateTime.Now.ToString("HH:mm:ss.fff")} - {e.FMsg}";
            txtFinish.Text = lMsg;
            btnWorking.Enabled = true;
            MessageBox.Show("Process finish", "End thread", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void WorkerProccess02_OnWorking(object sender, AnotherWorkingEventArgs e)
        {
            //custom logic in ui process
            var lMsg = $"{txtWorking2.Text} {Environment.NewLine} {DateTime.Now.ToString("HH:mm:ss.fff")} - {e.FMsg}";
            txtWorking2.Text = lMsg;
            txtWorking2.SelectionStart = txtWorking2.TextLength;
            txtWorking2.ScrollToCaret();
        }

        private void WorkerProccess02_OnFinish(object sender, FinishEventArgs e)
        {
            //custom logic in ui process
            var lMsg = $"{DateTime.Now.ToString("HH:mm:ss.fff")} - {e.FMsg}";
            txtFinish2.Text = lMsg;
            btnProcess2.Enabled = true;
            btnEnd2.Enabled = false;
            MessageBox.Show("Process finish", "End thread", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion events workers (methodsjet objects)
    }
}