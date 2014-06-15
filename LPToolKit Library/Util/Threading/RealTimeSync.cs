using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace LPToolKit.Util
{
    /// <summary>
    /// A mechanism to sleep a thread to an exact time down to the 
    /// millisecond.  
    /// </summary>
    /// <remarks>
    /// Thread.Sleep() only guarantees that a thread will wait until 
    /// at least a time, but could be any time after that.  This class 
    /// waits half the time using Thread.Sleep() until the time to wait 
    /// is below a certain threshold and then uses SpinWait() to wait 
    /// the final moments to guarantee the thread stops being blocked 
    /// at a precise moment.
    /// </remarks>
    public class RealTimeSync
    {
        #region Constructors

        public RealTimeSync()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// The number of milliseconds prior to the next wake time
        /// to start using SpinWait (and peg the CPU) to guarantee
        /// waking at the correct time.
        /// </summary>
        public int SpinThreshold = 15;

        /// <summary>
        /// The number of iterations for the SpinWait call, effectively
        /// controlling the granularity of WaitUntil()
        /// </summary>
        public int SpinInterations = 100;

        #endregion

        #region Methods

        /// <summary>
        /// Blocks the calling thread until a specific UTC time. 
        /// </summary>
        /// <returns>True if we waited until the time or false if the thread was awaken early by a pulse.</returns>
        public bool WaitUntil(DateTime dt)
        {
            while (dt > DateTime.UtcNow)
            {
                var timeLeft = (int)(DateTime.UtcNow - dt).TotalMilliseconds;
                if (timeLeft < SpinThreshold)
                {
                    // the last moments a blocked by SpinWait() and
                    // we no longer allow interruptions
                    while (dt > DateTime.UtcNow)
                    {
                        Thread.SpinWait(SpinInterations);
                    }
                    return true;
                }
                else
                {
                    // we block against the interruption signal for 
                    // half the time remaining, returning false if 
                    // we do get interrupted.
                    lock (_interrupt)
                    {
                        if (Monitor.Wait(_interrupt, timeLeft / 2))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Forces all blocked calls to return early because our
        /// timing needs to be recalculated.
        /// </summary>
        public void PulseAll()
        {
            lock (_interrupt)
            {
                Monitor.PulseAll(_interrupt);
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// Signal to allow WaitUntil() to be stopped early.
        /// </summary>
        private readonly object _interrupt = new object();

        #endregion
    }
}
