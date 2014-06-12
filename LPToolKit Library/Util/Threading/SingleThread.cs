using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LPToolKit.Util
{
    /// <summary>
    /// Simple wrapper for a thread that accepts a delegate that is 
    /// repeatedly called between the calls to Start() and Stop(),
    /// handling all thread-safing needs.
    /// </summary>
    /// <remarks>
    /// TODO: implement IShutdownPolitely interface
    /// TODO: use this more often so threads have a common setup
    /// </remarks>
    internal class SingleThread
    {
        #region Constructors

        public SingleThread(ThreadStart step = null)
        {
            Step = step ?? OnStep;
        }

        ~SingleThread()
        {
            Stop();
        }

        #endregion

        #region Properties

        /// <summary>
        /// True while the thread is running.
        /// </summary>
        public bool Running
        {
            get { lock (_threadLock) { return _activeThread != null; } }
        }

        /// <summary>
        /// The method called each step while the thread is running.
        /// Changes only take effect if the thread is restarted.
        /// </summary>
        public ThreadStart Step;

        public ThreadPriority Priority = ThreadPriority.Normal;

        /// <summary>
        /// When true, the processor is released after each step
        /// automatically to prevent hogging the cpu.
        /// </summary>
        public bool SleepAfterStep = false;

        #endregion

        #region Methods

        /// <summary>
        /// Starts the the thread only if itis not already running.
        /// </summary>
        public void Start()
        {
            lock (_threadLock)
            {
                if (_activeThread == null)
                {
                    _activeThread = new Thread(ThreadMain);
                    _activeThread.Priority = Priority;
                    _activeThread.Start();
                    ThreadManager.Current.Register(_activeThread);
                }
            }
        }

        /// <summary>
        /// Stops the thread, aborting the thread after a given timeout.
        /// </summary>
        public void Stop(int abortTimeoutMsec = 1000)
        {
            lock (_threadLock)
            {
                if (_activeThread != null)
                {
                    var t = _activeThread;
                    _activeThread = null;
                    if (t.Join(abortTimeoutMsec) == false)
                    {
                        t.Abort();
                    }
                    ThreadManager.Current.Unregister(t);
                }
            }
        }

        /// <summary>
        /// Optionally subclasses can override this method.  It is 
        /// called whenever the Step is null.
        /// </summary>
        public virtual void OnStep()
        {
        }

        #endregion
        
        #region Private

        /// <summary>
        /// Provides code to run after each step.  Allows subclasses
        /// to provide standardized behavior, like waiting until a
        /// signal to run the Step() function again.
        /// </summary>
        protected virtual void AfterStep()
        {
            if (SleepAfterStep)
            {
                Thread.Sleep(0);
            }
        }

        /// <summary>
        /// The actual thread function, which repeatedly calls Step()
        /// until Stop() is called.
        /// </summary>
        private void ThreadMain()
        {
            try 
            {
                // grab the method we'll call, and stop the thread if it's empty
                var fn = Step;
                if (fn == null)
                {
                    lock (_activeThread)
                    {
                        _activeThread = null;
                        return;
                    }
                }

                // we'll keep processing until the thread is changed
                var myThread = _activeThread;
                while (myThread == _activeThread)
                {
                    Step();
                    AfterStep();
                }
            }
            catch (Exception ex)
            {
                Session.UserSession.Current.Console.Add("SingleThread Error: " + ex.ToString());
            }
        }

        /// <summary>
        /// Mutex for accessing the active thread reference from other
        /// threads.
        /// </summary>
        private readonly object _threadLock = new object();

        /// <summary>
        /// Reference to the thread running.
        /// </summary>
        private Thread _activeThread = null;

        #endregion
    }

    /// <summary>
    /// A thread that runs each step only after recieving a signal,
    /// optionally waiting for some time before accepting a repeated
    /// signal.
    /// </summary>
    internal class SignalThead : SingleThread
    {

        #region Constructors

        public SignalThead(ThreadStart step = null) : base(step)
        {
        }

        #endregion


        #region Properties

        /// <summary>
        /// The number of milliseconds to wait between each step, 
        /// regardless of the number of signals received.
        /// </summary>
        public int WaitBetweenStepsMsec = 0;

        #endregion

        #region Methods

        /// <summary>
        /// Sets the signal to run a step.
        /// </summary>
        public void SetSignal()
        {
            lock (_signal)
            {
                _setCalled = true;
                Monitor.PulseAll(_signal);
            }
        }

        /// <summary>
        /// Waits for the signal to be set and optionally an amount of 
        /// time, between each time Step() is called.
        /// </summary>
        protected override void AfterStep()
        {
            base.AfterStep();

            // mark time we ran this step
            _lastStep = DateTime.UtcNow;


            lock (_signal)
            {
                // wait for signal
                while (_setCalled == false)
                {
                    Monitor.Wait(_signal);
                }

                // clear signal
                _setCalled = false;
            }

            // make sure to pause if this is too soon
            if (WaitBetweenStepsMsec > 0)
            {
                var timeout = (int)((DateTime.UtcNow - _lastStep).TotalMilliseconds);
                if (timeout > 0 && timeout < WaitBetweenStepsMsec)
                {
                    Thread.Sleep(WaitBetweenStepsMsec - timeout);
                }
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// Synchronization object for waiting for the signal to be set.
        /// </summary>
        private readonly object _signal = new object();

        /// <summary>
        /// Set to true whenever Set() is called.
        /// </summary>
        private bool _setCalled = false;

        /// <summary>
        /// Last time Step() was called.
        /// </summary>
        private DateTime _lastStep = DateTime.MinValue;

        #endregion
    }

}
