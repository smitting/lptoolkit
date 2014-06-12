using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.Core.Tasks
{
    /// <summary>
    /// Interface for a task that is automatically rescheduled by the
    /// kernel after running.
    /// </summary>
    /// <remarks>
    /// This provides an efficient replacement for all slow running
    /// background threads that need to do something every few seconds,
    /// like saving the session file every couple of seconds if there
    /// were any changes.
    /// </remarks>
    public interface IRepeatingKernelTask : IKernelTask
    {
        /// <summary>
        /// This task will not execute until this returns true.
        /// </summary>
        bool ReadyToRun { get; }

        /// <summary>
        /// Tells the task that is have been run, so subclasses don't
        /// have to manage this manually.
        /// </summary>
        void MarkRan();
    }

    /// <summary>
    /// Base class providing common functionality for repeating tasks.
    /// </summary>
    public abstract class RepeatingKernelTask : IRepeatingKernelTask
    {
        #region Constructors

        /// <summary>
        /// Constructor immediately schedules this task unless specifically
        /// requested to not do so.
        /// </summary>
        public RepeatingKernelTask(bool autoStart = true)
        {
            LastRunTime = DateTime.Now.AddSeconds(-10);
            if (autoStart)
            {
                ScheduleTask();
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the minimum amount of time to wait before executing
        /// this task after the last time it was executed, which is 
        /// the default way to determine if the task is ready.
        /// 
        /// Default time is 1000 msec.
        /// </summary>
        public virtual int MinimumRepeatTimeMsec 
        { 
            get { return _repeating; } 
            set { _repeating = value; } 
        }

        /// <summary>
        /// Returns the number of milliseconds since this task was
        /// last run.
        /// </summary>
        public int MsecSinceLastRun
        {
            get { return (int)(DateTime.UtcNow - LastRunTime).TotalMilliseconds; }
        }

        /// <summary>
        /// Set by the kernel automatically went running an IRepeatingKernelTask.
        /// </summary>
        public DateTime LastRunTime { get; private set; }

        #endregion

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
        public virtual int ExpectedLatencyMsec 
        { 
            get { return _latency; } 
            set { _latency = value; } 
        }

        #endregion

        #region IRepeatingKernelTask Implementation

        /// <summary>
        /// The default implementation is the task is ready to run
        /// after the minimum repeat time has ellapsed.
        /// </summary>
        public virtual bool ReadyToRun
        {
            get { return MsecSinceLastRun > MinimumRepeatTimeMsec; }
        }

        /// <summary>
        /// Saves the last run time.
        /// </summary>
        public void MarkRan()
        {
            LastRunTime = DateTime.UtcNow;
        }

        #endregion

        #region Private

        private int _latency = 500;
        private int _repeating = 1000;


        #endregion

    }
}
