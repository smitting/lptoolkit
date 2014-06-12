using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LPToolKit.Util
{
    /// <summary>
    /// TODO: This is just a bandaid during development and should be removed.
    /// Maintains a list of active threads so they can be forcibly killed
    /// when the program exists.
    /// </summary>
    public class ThreadManager
    {
        private ThreadManager()
        {

        }

        ~ThreadManager()
        {
            KillAll();
        }

        /// <summary>
        /// Singleton instance.
        /// </summary>
        public readonly static ThreadManager Current = new ThreadManager();

        /// <summary>
        /// Registers and immediately runs a thread function.
        /// </summary>
        public Thread Run(ThreadStart ts)
        {
            var t = Register(new Thread(ts));
            t.Start();
            return t;
        }

        /// <summary>
        /// Creates a thread for the function and registers it.
        /// </summary>
        public Thread Register(ThreadStart ts)
        {
            return Register(new Thread(ts));
        }

        /// <summary>
        /// Adds an existing thread to the list.
        /// </summary>
        public Thread Register(Thread t)
        {
            lock (_lock)
            {
                if (_activeThreads.Contains(t) == false)
                {
                    _activeThreads.Add(t);
                }
            }
            return t;
        }

        /// <summary>
        /// Removes a thread from the list.
        /// </summary>
        public void Unregister(Thread t)
        {
            lock (_lock)
            {
                if (_activeThreads.Contains(t))
                {
                    _activeThreads.Remove(t);
                }
            }
        }

        /// <summary>
        /// Causes all registered threads to abort immediately.
        /// </summary>
        public void KillAll()
        {
            lock (_lock)
            {
                if (_activeThreads != null)             
                {
                    foreach (var t in _activeThreads.ToArray())
                    {
                        try
                        {
                            t.Abort();
                        }
                        catch
                        {
                        }
                    }
                }
                _activeThreads.Clear();
            }
        }

        private List<Thread> _activeThreads = new List<Thread>();
        private readonly object _lock = new object();
    }
}
