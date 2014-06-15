using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.Core.Tasks
{
    /// <summary>
    /// Event signature for events about kernel tasks.
    /// </summary>
    public delegate void IKernelTaskHandler(IKernelTask task);

    /// <summary>
    /// Interface for tasks that can be scheduled by the kernel.
    /// </summary>
    public interface IKernelTask
    {
        /// <summary>
        /// Called by the scheduler when it wants this task to run.
        /// </summary>
        void RunTask();

        /// <summary>
        /// The number of milliseconds into the future to set the 
        /// contractual completion time when not explicitly set.
        /// </summary>
        int ExpectedLatencyMsec { get; }

        /// <summary>
        /// Adds this task to the scheduler.  Allows certain actions
        /// to be taken before scheduling in case some data required
        /// for computing this task may change if the events are
        /// handled out-of-order.
        /// </summary>
        /// <remarks>
        /// Returns itself for chaining.
        /// </remarks>
        IKernelTask ScheduleTask();
    }
}
