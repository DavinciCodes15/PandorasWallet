using System;
using System.ComponentModel;
using System.Threading;
using System.Collections.Generic;

namespace Pandora.Client.Universal.Threading
{
    #region Delagates

    // public delegate void NotifyDelegate(object sender);
    public delegate void ErrorHandlerDelegate(object sender, Exception e, ref bool aIsHandled);

    #endregion

    public abstract class MethodJet : ISynchronizeInvoke, IDisposable
    {
        private bool FTerminated = false;
        private EventWaitHandle FQueueSignal;
        private EventHandler FOnTerminated = null;
        private ErrorHandlerDelegate FErrorHandler = null;
        private Queue<DelegateMessage> FMethodQueue;
        private int FActiveThreadID = 0;
        private ISynchronizeInvoke FSynchronizingObject = null;
        private object FEventLock = new object();
        // private methods
        private int GetCurrentThreadId()
        {
            return Thread.CurrentThread.ManagedThreadId;
        }


        #region Protected Members

        /// <summary>
        /// Terminated property determines if the pump will stop running some time soon.
        /// </summary>
        /// <remarks>If an end message is sent this propertiy is set to true.</remarks>
        protected bool Terminated
        {
            get { return FTerminated; }
            set { FTerminated = value; }
        }


        protected virtual void InternalRun()
        {
            FTerminated = false;
            DelegateMessage methodMessage;

            lock (this)
            {
                FActiveThreadID = GetCurrentThreadId();
            }
            try
            {
                InternalInitialize();
                while (GetMethodMessage(out methodMessage))
                    InvokeMethodMessage(methodMessage);
            }
            finally
            {
                try
                {
                    InternalFinalize();
                }
                finally 
                {
                    DoOnTerminate();
                    FActiveThreadID = 0;
                }
            }
        }

        /// <summary>
        /// Gets a Delgate Message from the message queue
        /// </summary>
        /// <remarks>
        /// GetMethodMessage Looks on the message queue if no message is available 
        /// the call is blocked until a message is found
        /// The MethodJet uses the return value to determine whether to end the main 
        /// message loop and stop running. 
        /// The GetMethodMessage function retrieves messages associated with the 
        /// object located on an enternal queue.
        /// During this call, the object delivers pending messages that were sent 
        /// to the object using the Invoke, BeginInvoke, or Terminate.
        /// </remarks>
        /// <returns>Returns true if the "Method Message" is not an end "Method Message".</returns>
        protected virtual bool GetMethodMessage(out DelegateMessage aMethodMessage)
        {
            FQueueSignal.WaitOne(); // wait for a message

            lock (FMethodQueue)
            {
                aMethodMessage = FMethodQueue.Dequeue();
                if (FMethodQueue.Count == 0)
                    FQueueSignal.Reset();
            }
            return !aMethodMessage.TerminateMessage;
        }

        /// <summary>
        /// Executes the method message.
        /// </summary>
        protected virtual bool InvokeMethodMessage(DelegateMessage aMethodMessage)
        {
            try
            {
                aMethodMessage.MethodInvoke();
                return true;
            }
            catch (Exception e)
            {
                if ((e.GetBaseException() is ThreadAbortException) || (!aMethodMessage.CompletedSynchronously && !DoErrorHandler(e)))
                    throw;
                return false;
            }
        }

        protected virtual void InternalInitialize()
        {
        }

        protected virtual void InternalFinalize()
        {
        }

        private void DoOnTerminate()
        {
            lock (FEventLock)
            {
                if (FOnTerminated != null)
                {
                    object[] args = { this, new EventArgs() };
                    DoEvent(FOnTerminated, true, args);
                }
            }
        }

        protected int RunningThreadID
        {
            get
            {
                int result;
                    result = FActiveThreadID;
                return result;
            }
        }

        /// <summary>
        /// Executes an event that occurs inside the running method pump.
        /// </summary>
        /// <remarks>Call the DoEvent to execute an event on the correct thread.  
        /// This method checks if SinchronizingObject property is set and executes 
        /// the event using the ISynchronizeInvoke interface.  By doing so this 
        /// insures that the event is fired only when the thread is in a valid state.</remarks>
        protected virtual void DoEvent(Delegate anEvent, bool asynchronous, params object[] args)
        {
            if (FSynchronizingObject == null)
                anEvent.DynamicInvoke(args);
            else if (asynchronous)
                FSynchronizingObject.BeginInvoke(anEvent, args);
            else
                FSynchronizingObject.Invoke(anEvent, args);
        }


        protected virtual bool DoErrorHandler(Exception e)
        {
            bool ErrorHandled = false;
            object[] args = { this, e.GetBaseException(), ErrorHandled };
            lock (FEventLock)
            {
                if (FErrorHandler != null)
                {
                    DoEvent(FErrorHandler, false, args);
                    ErrorHandled = (bool)args[2];
                }
            }
            return ErrorHandled;
        }

        #endregion

        #region Public Members
        /// <summary>
        /// The ISynchronizeInvoke representing the object used to 
        /// marshal the event-handler calls that are issued when an 
        /// interval has elapsed. The default is a null reference.
        /// </summary>
        /// <remarks>
        /// When SynchronizingObject is a null reference, events executed
        /// with the DoEvent method are called on a thread from the Run 
        /// method was executed on. 
        /// When an event is handled by a visual Forms component, such 
        /// as a button or a Form, accessing the component from another 
        /// thread might result in an exception or just might not work.  
        /// Avoid this effect by setting SynchronizingObject to a Forms 
        /// component, which causes the a method that handles an event 
        /// to be called on the same thread that the component was created on.
        /// </remarks>
        public MethodJet()
        {
            FMethodQueue = new Queue<DelegateMessage>();
            FQueueSignal = new EventWaitHandle(false, EventResetMode.ManualReset);
        }

        public ISynchronizeInvoke SynchronizingObject
        {
            get
            {
                ISynchronizeInvoke result;

                lock (this)
                {
                    result = FSynchronizingObject;
                }
                return result;
            }
            set
            {
                lock (this)
                {
                    FSynchronizingObject = value;
                }
            }
        }

        private void IsRunning()
        {
            if (!Running)
                throw new DevErrorException("MethodJet not running.");
        }

        #region ISynchronizeInvoke Members

        public virtual IAsyncResult BeginInvoke(Delegate method, params object[] args)
        {
            IsRunning();
            DelegateMessage methodMessage = new DelegateMessage(method, args, true);
            lock (FMethodQueue)
            {
                FMethodQueue.Enqueue(methodMessage);
                FQueueSignal.Set(); 
            }
            return methodMessage;
        }

        public virtual object EndInvoke(IAsyncResult result, int aMillisecondsTimeout)
        {
            IsRunning();
            DelegateMessage methodMessage = result as DelegateMessage;
            if (RunningThreadID == GetCurrentThreadId())
                throw new DevErrorException("MethodJet is running in the same thread calling EndInvoke will cause a dead lock!");
            if (aMillisecondsTimeout < 1)
                methodMessage.AsyncWaitHandle.WaitOne();
            else
            methodMessage.AsyncWaitHandle.WaitOne(aMillisecondsTimeout);
            if (methodMessage.ExceptionObject != null)
                throw methodMessage.ExceptionObject;
            return methodMessage.AsyncState;
        }

        public virtual object EndInvoke(IAsyncResult result)
        {
            return EndInvoke(result, 0);
        }

         public virtual object Invoke(Delegate method, params object[] args)
        {
            return EndInvoke(BeginInvoke(method, args));
        }


        public virtual bool InvokeRequired
        {
            get { return RunAsynchronous; }
        }

        #endregion

        public virtual void Terminate()
        {
            lock (this)
            {
                if (!Terminated)
                {
                    if (Running)
                        BeginInvoke(null, null);
                    Terminated = true;
                }
            }
        }


        public abstract void Run();

        /// <summary>
        /// Deturmins if the call to the Run method is asynchronous or not.
        /// </summary>
        /// <remarks>Calling the run method will return once the Method Pump is terminated executing if the RunAsynchronous property is false.  If  true calling Run will execute the MethodPump in a thread and will return once the thread is created.</remarks>
        public virtual bool RunAsynchronous
        {
            get
            {
                return false;
            }
        }

        public bool Running
        {
            get
            {
                return RunningThreadID != 0;
            }
        }

        public event EventHandler OnTerminated
        {
            add
            {
                lock (FEventLock)
                {
                    FOnTerminated = FOnTerminated + value;
                }
            }
            remove
            {
                lock (FEventLock)
                {
                    FOnTerminated = FOnTerminated - value;
                }
            }
        }

        public event ErrorHandlerDelegate OnErrorEvent
        {
            add
            {
                lock (FEventLock)
                {
                    FErrorHandler = FErrorHandler + value;
                }
            }
            remove
            {
                lock (FEventLock)
                {
                    FErrorHandler = FErrorHandler - value;
                }
            }
        }

        #endregion

        #region Internal Classes
        public class WaitEvent : EventWaitHandle
        {
            private DelegateMessage FMethodMessage;
            public WaitEvent(bool initialState, DelegateMessage aMethodMessage)
                : base(initialState, EventResetMode.ManualReset)
            {
                FMethodMessage = aMethodMessage;
            }

            /// <returns>true if the current instance receives a signal. If the current instance is never signaled, <see cref="M:System.Threading.WaitHandle.WaitOne(System.Int32,System.Boolean)"></see> never returns.</returns>
            public override bool WaitOne()
            {
                return WaitOne(-1, false);
            }

            /// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite"></see> (-1) to wait indefinitely.</param>
            /// <param name="exitContext">true to exit the synchronization domain for the context before the wait (if in a synchronized context), and reacquire it afterward; otherwise, false.</param>
            /// <returns>true if the current instance receives a signal; otherwise, false.</returns>
            public override bool WaitOne(int millisecondsTimeout, bool exitContext)
            {
                bool baseResult;

                baseResult = base.WaitOne(millisecondsTimeout, exitContext);
                if (baseResult && (FMethodMessage.ExceptionObject != null))
                    throw FMethodMessage.ExceptionObject;
                return baseResult;
            }

            /// <param name="timeout">A <see cref="T:System.TimeSpan"></see> that represents the number of milliseconds to wait, or a <see cref="T:System.TimeSpan"></see> that represents -1 milliseconds to wait indefinitely.</param>
            /// <param name="exitContext">true to exit the synchronization domain for the context before the wait (if in a synchronized context), and reacquire it afterward; otherwise, false.</param>
            /// <returns>true if the current instance receives a signal; otherwise, false.</returns>
            public override bool WaitOne(TimeSpan timeout, bool exitContext)
            {
                bool baseResult;

                baseResult = base.WaitOne(timeout, exitContext);
                if (baseResult && (FMethodMessage.ExceptionObject != null))
                    throw FMethodMessage.ExceptionObject;
                return baseResult;
            }

        }

        public sealed class DelegateMessage : IAsyncResult
        {
            private Delegate FEventMethod;
            private object[] FArguments;
            private object FResult = null;
            private bool FCompleted = false;
            private bool FAsynchonous;
            private EventWaitHandle FMethodSignal;
            private Exception FException = null;

            public DelegateMessage(Delegate aEventMethod, object[] args, bool isAsynchonous)
            {
                FMethodSignal = new EventWaitHandle(false, EventResetMode.ManualReset);
                FAsynchonous = isAsynchonous;
                FArguments = args;
                FEventMethod = aEventMethod;
            }

            #region IAsyncResult Members

            public object AsyncState
            {
                get { return FResult; }
            }

            public WaitHandle AsyncWaitHandle
            {
                get { return FMethodSignal; }
            }

            public bool CompletedSynchronously
            {
                get { return !FAsynchonous; }
            }

            public bool IsCompleted
            {
                get { return FCompleted; }
            }

            #endregion

            /// <summary>
            /// Calls the method contained by MethodMessage.
            /// </summary>
            /// <remarks>This method is called by TMethodPump after GetMethodMessage is called.</remarks>
            public void MethodInvoke()
            {
                //if (IsCompleted)
                //    throw new MethodExecutedException("Method already executed.");
                try
                {
                    try
                    {
                        FException = null;
                        FResult = FEventMethod.DynamicInvoke(FArguments);
                    }
                    catch (Exception e)
                    {
                        FException = e;
                        throw;
                    }
                }
                finally
                {
                    FMethodSignal.Set();
                    FCompleted = true;
                }
            }

            public bool TerminateMessage { get { return FEventMethod == null; } }

            public Exception ExceptionObject
            {
                get { return FException; }
                set { FException = value; }
            }
            public override string ToString()
            {
                return FEventMethod.Method.Name;
            }
        }
        #endregion


        public virtual void Dispose()
        {
            Terminate();
        }
    }
}
