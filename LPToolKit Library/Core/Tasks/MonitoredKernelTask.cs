using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.Core.Tasks
{
    /// <summary>
    /// Different states for an IMonitoredKernelTask.
    /// </summary>
    public enum KernelTaskState
    {
        /// <summary>
        /// Nothing has happened with this task yet.
        /// </summary>
        None,
        
        /// <summary>
        /// The task is scheduled with the kernel but not executed.
        /// </summary>
        Scheduled,

        /// <summary>
        /// This task was cancelled before processing.
        /// </summary>
        Cancelled,

        /// <summary>
        /// This task was processed by the kernel.
        /// </summary>
        Processed
    }

    /// <summary>
    /// A kernel task that provides events about when it is executed
    /// with options to cancel.
    /// </summary>
    public interface IMonitoredKernelTask : IKernelTask
    {
        /// <summary>
        /// Event triggered by the kernel after the task is run.
        /// </summary>
        event IKernelTaskHandler TaskProcessed;

        /// <summary>
        /// The state of this task.  Allows the task to be cancelled
        /// if it has not yet been processed.
        /// </summary>
        KernelTaskState TaskState { get; set; }
    }

    /// <summary>
    /// Basic implementation of the monitored interface, which
    /// handles state management and events.
    /// </summary>
    public abstract class MonitoredKernelTask : IMonitoredKernelTask
    {
        #region IKernelTask Implementation

        /// <summary>
        /// This is defined by the subclass.
        /// </summary>
        public abstract void RunTask();

        /// <summary>
        /// Schedules this task with the kernel.
        /// </summary>
        public IKernelTask ScheduleTask()
        {
            Kernel.Current.Add(this);
            return this;
        }

        /// <summary>
        /// The default latency is 500msec.
        /// </summary>
        public virtual int ExpectedLatencyMsec { get { return 500; } set { } }

        #endregion

        #region IMonitoredKernelTask Implementation

        /// <summary>
        /// Registers an event to be called when this task is processed.
        /// Calls the event immediately if the task is already processed.
        /// </summary>
        public event IKernelTaskHandler TaskProcessed
        {
            add
            {
                if (_taskState == KernelTaskState.Processed)
                {
                    if (value != null)
                    {
                        value(this);
                    }
                }
                _taskProcessed += value;
            }
            remove
            {
                _taskProcessed -= value;
            }
        }

        /// <summary>
        /// Assigns the state provided, unless it's a cancel state 
        /// and the task has already been processed, then the request
        /// is ignored.
        /// </summary>
        public KernelTaskState TaskState
        {
            get
            {
                return _taskState;
            }
            set
            {
                // ignore cancellations that come too late
                if (value == KernelTaskState.Cancelled)
                {
                    if (_taskState == KernelTaskState.Processed) return;
                }

                // trigger event as needed
                if (value == KernelTaskState.Processed)
                {
                    if (_taskProcessed != null)
                    {
                        _taskProcessed(this);
                    }
                }

                // set state
                _taskState = value;
            }
        }

        #endregion

        #region Private

        private event IKernelTaskHandler _taskProcessed;
        private KernelTaskState _taskState = KernelTaskState.None;

        #endregion
    }
}
