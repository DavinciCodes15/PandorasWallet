using Pandora.Client.ThreadingMgrProposal.Logic.Base;
using Pandora.Client.Universal.Threading;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pandora.Client.ThreadingMgrProposal.Logic.Business.Mgr
{
    /// <summary>
    /// example implementation of IWorksMgr interface
    /// </summary>
    public class WorksMgr : IWorksMgr
    {
        private readonly Dictionary<string, IBaseWorkerProcess> FListMethodsJet = new Dictionary<string, IBaseWorkerProcess>();

        public void AddProccess<T, P>(string aKey, IWorkerProcess<T, P> aMethod) where T : EventArgs where P : EventArgs
        {
            this.FListMethodsJet.Add(aKey, aMethod);
        }

        public void RemoveProccess(string aKey)
        {
            this.FListMethodsJet.Remove(aKey);
        }

        public void RunProccess(string aKey)
        {
            ((MethodJetThread)this.FListMethodsJet[aKey]).Run();
        }

        public T Get<T>(string aKey) where T : MethodJetThread
        {
            IBaseWorkerProcess temp;
            this.FListMethodsJet.TryGetValue(aKey, out temp);
            return (T)temp ?? null;
        }

        public int Count()
        {
            return this.FListMethodsJet.Count;
        }

        public void Dispose()
        {
            Parallel.ForEach(this.FListMethodsJet.Values, lInstances => { ((MethodJetThread)lInstances)?.Dispose(); });
        }
    }
}