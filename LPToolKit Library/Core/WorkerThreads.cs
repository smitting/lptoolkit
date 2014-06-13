//#define FORCE_SINGLETHREADED

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LPToolKit.Util;
using LPToolKit.Core.Tasks;

namespace LPToolKit.Core
{
    /// <summary>
    /// Creates one thread per processor for workers and assigns 
    /// one thread to each of those processors.
    /// </summary>
    internal class WorkerThreads
    {
        #region Constructors

        /// <summary>
        /// Creates the workers and work slots to match the number
        /// of available processors.
        /// </summary>
        public WorkerThreads()
        {
#if FORCE_SINGLETHREADED
            var count = 1;
#warning KERNEL FORCED TO SINGLE THREADED MODE!
#else
            var count = Environment.ProcessorCount * 2; // TESTING WITH 4 THREADS PER CORE... TODO: fix the deadlocks instead
#endif

            // create empty work load
            _work = new IKernelTask[count];
            for (var i = 0; i < count; i++)
            {
                _work[i] = null;
            }

            //start up the threads
            _threads = new Thread[count];
            Start();
        }

        #endregion

        #region Properties

        /// <summary>
        /// When changed to false the workers will stop working.
        /// </summary>
        public bool Running { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Keeps trying to do a task, blocking until a worker is
        /// available.
        /// </summary>
        public void DoTaskASAP(IKernelTask task)
        {
            while (DoTask(task) == false)
            {
                BlockUntilFreeWorker(500);
            }
        }

        /// <summary>
        /// Gives the task to a worker that isn't doing anything,
        /// returnning null if all workers are busy.
        /// </summary>
        public bool DoTask(IKernelTask task)
        {
            lock (_workLock)
            {
                for (var i = 0; i < _threads.Length; i++)
                {
                    if (_work[i] == null)
                    {
                        lock (_newWorkSignal)
                        {
                            _work[i] = task;
                            Monitor.PulseAll(_newWorkSignal);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Blocks the calling thread until there's a free worker.
        /// </summary>
        public bool BlockUntilFreeWorker(int expireTimeout = -1)
        {
            // keep testing until expiration
            DateTime timeout = expireTimeout <= 0 ? DateTime.MaxValue : DateTime.UtcNow.AddMilliseconds(expireTimeout);
            while (DateTime.UtcNow < timeout)
            {
                lock (_workerDoneSignal)
                {
                    // check if a worker is free
                    lock (_workLock)
                    {
                        for (var i = 0; i < _threads.Length; i++)
                        {
                            if (_work[i] == null)
                            {
                                return true;
                            }
                        }
                    }

                    // wait until a new worker becomes free
                    if (expireTimeout > 0)
                    {
                        Monitor.Wait(_workerDoneSignal, timeout - DateTime.UtcNow);
                    }
                    else
                    {
                        Monitor.Wait(_workerDoneSignal);
                    }
                }
            }

            // timeout expired
            return false;
        }

        /// <summary>
        /// Starts all of the worker threads.
        /// </summary>
        public void Start()
        {
            Running = true;
            for (var i = 0; i < _threads.Length; i++)
            {
                _threads[i] = new Thread(WorkerThreadMain);
                //_threads[i].Priority = ThreadPriority.AboveNormal;
                _threads[i].Start();
            }
        }

        /// <summary>
        /// Stops all of the worker threads, giving each 1 second to
        /// finish up before they are aborted.
        /// </summary>
        public void Stop()
        {
            Running = false;
            lock (_newWorkSignal)
            {
                Monitor.PulseAll(_newWorkSignal);
            }
            foreach (var thread in _threads)
            {
                if (thread.Join(1000) == false)
                {
                    thread.Abort();
                }
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// The worker threads.
        /// </summary>
        private readonly Thread[] _threads;

        /// <summary>
        /// The current tasks each worker is on.
        /// </summary>
        private readonly IKernelTask[] _work;

        /// <summary>
        /// Signal threads wait on when they have no work.
        /// </summary>
        private readonly object _newWorkSignal = new object();

        /// <summary>
        /// Signal used when a worker has nothing to do.
        /// </summary>
        private readonly object _workerDoneSignal = new object();

        /// <summary>
        /// Mutex for messing with the work queue.
        /// </summary>
        private readonly object _workLock = new object();

        /// <summary>
        /// Thread loop for each worker.
        /// </summary>
        private void WorkerThreadMain()
        {
            // find my index
            int workerIndex = -1;
            for (var i = 0; i < _threads.Length; i++)
            {
                if (Thread.CurrentThread == _threads[i])
                {
                    workerIndex = i;
                    break;
                }
            }
            if (workerIndex == -1)
            {
                return;
            }

            // force processor selector if available
            CPUCore.SetThreadProcessor(workerIndex);

            // set the name for vs.net debugging
            Thread.CurrentThread.Name = "Worker #" + workerIndex;

            // TODO: allow shutdown politely to be registered with ThreadManager and have it call stop
            ThreadManager.Current.Register(Thread.CurrentThread);
            try
            {
                try
                {
                    // work until it's quittin' time
                    while (Running)
                    {
                        // run this task
                        var nextTask = _work[workerIndex];
                        if (nextTask != null)
                        {
                            try
                            {
                                nextTask.RunTask();
                            }
                            catch (Exception ex)
                            {
                                Session.UserSession.Current.Console.Add("UNCAUGHT EXCEPTION IN TASK: " + ex.Message, "Kernel");
                            }
                        }

                        // block until there's more work
                        lock (_workerDoneSignal)
                        {
                            _work[workerIndex] = null;
                            Monitor.PulseAll(_workerDoneSignal);
                        }
                        lock (_newWorkSignal)
                        {
                            while (_work[workerIndex] == null)
                            {
                                Thread.Sleep(0);
                                Monitor.Wait(_newWorkSignal);
                            }
                        }
                    }
                }
                catch (ThreadAbortException)
                {

                }
                catch (Exception ex)
                {
                    Session.UserSession.Current.Console.Add("KERNEL PANIC: " + ex.Message, "Kernel");
                }

            }
            finally
            {
                ThreadManager.Current.Unregister(Thread.CurrentThread);
            }
        }

        #endregion
    }
}
