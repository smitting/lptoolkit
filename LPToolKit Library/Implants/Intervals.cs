using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LPToolKit.Util;
using Jurassic.Library;

namespace LPToolKit.Implants
{
    /// <summary>
    /// Manages triggering all callbacks registered to setInterval() 
    /// appropriately in a single thread for the entire app.
    /// </summary>
    internal class Intervals
    {
        #region Constructors

        /// <summary>
        /// Use singleton.  This constructor starts the interval thread.
        /// </summary>
        private Intervals()
        {
            Start();
        }

        /// <summary>
        /// Stops the interval thread.
        /// </summary>
        ~Intervals()
        {
            Stop();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static readonly Intervals Current = new Intervals();

        #endregion

        #region Methods

        /// <summary>
        /// Starts repeatedly calling a javascript function.
        /// </summary>
        public int Add(object sender, FunctionInstance fn, int delayMs)
        {
            var ie = new IntervalEvent();
            ie.Sender = sender;
            ie.Callback = fn;
            ie.DelayMsec = delayMs;
            IntervalEvents.Add(ie);
            return ie.ID;
        }

        /// <summary>
        /// Stops calling a function registered with Add()
        /// </summary>
        public void Remove(int id)
        {
            foreach (var ie in IntervalEvents)
            {
                if (ie.ID == id)
                {
                    IntervalEvents.Remove(ie);
                    break;
                }
            }
        }

        /// <summary>
        /// Removes all functions registered by a sender.
        /// </summary>
        public void RemoveAll(object sender)
        {
            var removeList = new List<IntervalEvent>();
            foreach (var ie in IntervalEvents)
            {
                if (ie.Sender == sender)
                {
                    removeList.Add(ie);                    
                }
            }
            foreach (var ie in removeList)
            {
                IntervalEvents.Remove(ie);
            }
        }

        #endregion

        #region Private


        /// <summary>
        /// Stores functions being called repeatedly by the 
        /// setInterval() javascript function.
        /// </summary>
        private class IntervalEvent
        {
            /// <summary>
            /// Constructor assigns a new callback id.
            /// </summary>
            public IntervalEvent()
            {
                ID = _nextId++;
            }

            /// <summary>
            /// The object that registered this event.
            /// </summary>
            public object Sender;

            /// <summary>
            /// Function being called.
            /// </summary>
            public FunctionInstance Callback;

            /// <summary>
            /// How often to call the funciton.
            /// </summary>
            public int DelayMsec;

            /// <summary>
            /// Last time the function was called.
            /// </summary>
            public DateTime LastCall = DateTime.MinValue;

            /// <summary>
            /// Marks when the function is already running so the
            /// same call is never running twice, just because it
            /// took longer to finish than it did to get to the
            /// next event time.
            /// </summary>
            public bool Running = false;

            /// <summary>
            /// The unique id for clearInterval()
            /// </summary>
            public readonly int ID;

            /// <summary>
            /// The ID of the next registered interval.
            /// </summary>
            private static int _nextId = 1;
        }

        /// <summary>
        /// All functions currently repeating.
        /// </summary>
        private List<IntervalEvent> IntervalEvents = new List<IntervalEvent>();

        /// <summary>
        /// Starts a thread running that calls the functions 
        /// registered by setInterval() at regular times.
        /// </summary>
        private void Start()
        {
            if (_intervals != null) return;
            _intervals = ThreadManager.Current.Register(() =>
            {
                while (_intervals != null)
                {
                    lock (_listLock)
                    {
                        foreach (var ie in IntervalEvents)
                        {
                            if (ie.LastCall < DateTime.Now.AddMilliseconds(-ie.DelayMsec))
                            {
                                ie.LastCall = DateTime.Now;
                                Call(ie);
                            }
                        }
                    }
                    Thread.Sleep(1);
                }
            });
            _intervals.Start();
        }

        /// <summary>
        /// Makes a non-blocking call to the function passed to 
        /// setInterval().  Will never call the same function again
        /// until the first non-blocking call finishes. 
        /// </summary>
        /// <remarks>
        /// Slow functions will never prevent other functions from 
        /// triggering, and as a consequence, true javascript threading
        /// can be implemented by simply using setInterval with a call
        /// that never returns.
        /// </remarks>
        private void Call(IntervalEvent ie)
        {
            // ignore if already running
            if (ie == null || ie.Running) return;

            // use a non-blocking call to run the function
            try
            {
                ie.Running = true;
                Task.Factory.StartNew(() =>
                {
                    ie.Callback.Call(null);
                });
            }
            finally
            {
                ie.Running = false;
            }
        }

        /// <summary>
        /// Kills the interval thread.
        /// </summary>
        private void Stop()
        {
            if (_intervals != null)
            {
                var t = _intervals;
                _intervals = null;
                if (t.Join(10) == false)
                {
                    t.Abort();
                }
                ThreadManager.Current.Unregister(t);
            }
        }

        /// <summary>
        /// The thread running the intervales
        /// </summary>
        private Thread _intervals;

        /// <summary>
        /// Mutex so we don't get errors for looping through an array
        /// that has changed.
        /// </summary>
        private readonly object _listLock = new object();


        #endregion
    }
}
