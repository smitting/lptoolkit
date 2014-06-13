using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using LPToolKit.Implants;
using LPToolKit.MIDI;
using LPToolKit.Util;
using LPToolKit.Core.Tasks;

namespace LPToolKit.Core
{ 
    /// <summary>
    /// This is a new experiment that may not go well.
    /// 
    /// This is a kernel in the operating system sense of the word.  
    /// Instead of having each thread with the unlimited ability to
    /// pass events to the implants, this kernel controls the processing
    /// of all events using the following steps:
    ///     - recieve raw events from subsystems and hardware drivers
    ///     - convert these events to ImplantEvents
    ///     - pass the ImplantEvents to implants
    ///     - all API calls generate ImplantAction objects passed to this object
    ///     - ImplantActions are converted into raw events 
    ///     - Raw events are passed back to subsystems and hardware drivers
    ///     
    /// The goals of this kernal:
    ///     - provide consistency in latency of event processing
    ///     - ability to prioritize events
    ///     - have events that affect audio managed in real time
    ///     - manages best utilization of multiple processors
    ///     
    /// Details:
    ///     - even web calls should be managed to prevent overloading the audio systems (maybe should be a separate process instead?)
    ///     
    /// It's possible this effort will be worthless, but our output has
    /// been far too inconsistent to date for live performances when 
    /// providing MIDI note data instead of just turning knobs.
    /// </summary>
    /// <remarks>
    /// Note: if we want to use multi-threading later (and we probably
    /// do) we should have use a single-threaded Producer/Consumer
    /// model to decide which tasks should be done, where the consumer
    /// passes work to a separate worker thread pool on an individual
    /// basis.
    /// </remarks>
    public class Kernel
    {
        #region Constructors

        /// <summary>
        /// Starts the threads.  Use the singleton.
        /// </summary>
        private Kernel()
        {
            _workers = new WorkerThreads();
            _scheduler = new SingleThread(SchedulerStep);
            _scheduler.Name = "Kernel Scheduler";
            //_scheduler.Priority = ThreadPriority.AboveNormal;            
            _scheduler.Start();
        }

        /// <summary>
        /// Shutsdown the threads if they're still running.
        /// </summary>
        ~Kernel()
        {
            if (_scheduler != null)
            {
                _scheduler.Stop();
                _scheduler = null;
            }
            if (_workers != null)
            {
                _workers.Stop();
                _workers = null;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Singleton instance of the kernel, as there can be just one.
        /// </summary>
        public static readonly Kernel Current = new Kernel();

        #endregion

        #region Methods

        /// <summary>
        /// Adds a schedulable task to the work queue, optionally with
        /// the maximum time we can wait for the task to be processed.
        /// If not set, the default latency for the type of task is used.
        /// </summary>
        public void Add(IKernelTask task, int maxMsec = -1)
        {
            if (maxMsec < 0)
            {
                maxMsec = task.ExpectedLatencyMsec;
            }
            _workQueue.Enqueue(task, maxMsec);
        }

        #endregion

        #region Private

        /// <summary>
        /// This is one step of the kernel.
        /// </summary>
        private void SchedulerStep()
        {
            bool ranTask = false;

            try
            {
                // find the most urgent task and run it on a worker
                // block if no tasks or workers are available
                IKernelTask next;
                if (_workQueue.Dequeue(out next))
                {
                    IMonitoredKernelTask monitored = next as IMonitoredKernelTask;

                    // ignore cancelled tasks
                    if (monitored != null)
                    {
                        if (monitored.TaskState == KernelTaskState.Cancelled)
                        {
                            return;
                        }
                    }

                    // TODO: this can result in a task being scheduled several times before it actually runs if its priority is too high
                    // we may want the scheduler to notice this and schedule it based on this.

                    // just reschedule and exit when we get a task too early
                    if (next is IRepeatingKernelTask)
                    {
                        var repeater = next as IRepeatingKernelTask;
                        if (repeater.ReadyToRun == false)
                        {
                            repeater.ScheduleTask();
                            return;
                        }
                    }

                    // do the work when a worker is available
                    _workers.DoTaskASAP(next);
                    ranTask = true;

                    // mark that we did this task and schedule it for next time
                    if (next is IRepeatingKernelTask)
                    {
                        var repeater = next as IRepeatingKernelTask;
                        repeater.MarkRan();
                        repeater.ScheduleTask();
                    }

                    // notify listeners
                    if (next is IMonitoredKernelTask)
                    {
                        (next as IMonitoredKernelTask).TaskState = KernelTaskState.Processed;
                    }
                }
            }
            finally
            {
                // if we didn't run a task, we need to check if any 
                // work is ready to be run
                if (ranTask == false)
                {
                    BlockForWork();
                }
            }
        }

        /// <summary>
        /// Blocks the calling thread until there is a work item 
        /// available.  Can no longer only depend on the work queue
        /// being empty since we now have repeating tasks that require
        /// a certain amount of time to pass before it can be 
        /// processed again.
        /// </summary>
        private void BlockForWork()
        {
            while (WorkIsAvailable() == false)
            {
                _workQueue.ProcessInsertions();
                Thread.Sleep(1);
                _workQueue.BlockForWork();
            }
        }

        /// <summary>
        /// Returns true iff the work queue is not empty and contains
        /// at least one task that is ready to run.
        /// </summary>
        private bool WorkIsAvailable()
        {
            return _workQueue.AnyMatch((task) =>
            {
                // ask the repeating tasks if they're ready
                if (task is IRepeatingKernelTask)
                {
                    return (task as IRepeatingKernelTask).ReadyToRun;
                }

                // other kinds of tasks are always read
                return true;
            });
        }

        /// <summary>
        /// The scheduler thread.
        /// </summary>
        private SingleThread _scheduler;

        /// <summary>
        /// The worker threads.
        /// </summary>
        private WorkerThreads _workers;

        /// <summary>
        /// A queue that orders work by its urgency, which is based
        /// on when the work was scheduled and how long it was
        /// considered acceptable to wait before the work was done.
        /// </summary>
        private RealTimeQueue<IKernelTask> _workQueue = new RealTimeQueue<IKernelTask>();

        #endregion
    }
}
