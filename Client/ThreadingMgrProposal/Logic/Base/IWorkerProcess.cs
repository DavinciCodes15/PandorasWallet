using System;

namespace Pandora.Client.ThreadingMgrProposal.Logic.Base
{
    /// <summary>
    /// base interface ow worker process
    /// </summary>
    public interface IBaseWorkerProcess : IDisposable
    {
    }

    /// <summary>
    /// interface used to implement simple behavior of MethodJet class
    /// </summary>
    /// <typeparam name="T">EventArgs type that will be used in handler of OnWorking event</typeparam>
    /// <typeparam name="P">EventArgs type that will be used in handler of OnFinish event</typeparam>
    public interface IWorkerProcess<T, P> : IBaseWorkerProcess where T : EventArgs where P : EventArgs
    {
        event EventHandler<T> OnWorking;

        event EventHandler<P> OnFinish;
    }
}