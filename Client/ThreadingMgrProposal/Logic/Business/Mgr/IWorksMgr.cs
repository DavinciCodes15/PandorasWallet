using Pandora.Client.ThreadingMgrProposal.Logic.Base;
using Pandora.Client.Universal.Threading;
using System;

namespace Pandora.Client.ThreadingMgrProposal.Logic.Business.Mgr
{
    /// <summary>
    /// inteface to manage the async process in a safe way
    /// </summary>
    public interface IWorksMgr : IDisposable
    {
        /// <summary>
        /// method to add worker process object to internal list mannage by the mannager
        /// </summary>
        /// <typeparam name="T">EventArgs type that will be used in handler of OnWorking event</typeparam>
        /// <typeparam name="P">EventArgs type that will be used in handler of OnFinish event</typeparam>
        /// <param name="aKey">key or id of worker process object that will be mannage by this implementation</param>
        /// <param name="aMethod">instance of worker process</param>
        void AddProccess<T, P>(string aKey, IWorkerProcess<T, P> aMethod) where T : EventArgs where P : EventArgs;

        /// <summary>
        /// method to remove a worker process instance of inner list mannaged objects
        /// </summary>
        /// <param name="aKey">key or id of worker process instance in inner list mannaged objects</param>
        void RemoveProccess(string aKey);

        /// <summary>
        /// method that execute the run logic (from MethodJet inheritance) of key instance in inner list mannaged objects
        /// </summary>
        /// <param name="aKey">key or id of worker process instance in inner list mannaged objects</param>
        void RunProccess(string aKey);

        /// <summary>
        /// method that return the specified instance of object in inner list mannaged objects
        /// </summary>
        /// <typeparam name="T">type of object mannaged</typeparam>
        /// <param name="aKey"></param>
        /// <returns>key or id of worker process instance in inner list mannaged object</returns>
        T Get<T>(string aKey) where T : MethodJetThread;

        int Count();
    }
}