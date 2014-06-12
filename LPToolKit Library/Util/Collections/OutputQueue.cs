using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LPToolKit.Util
{
    public interface IShutdownNicely
    {
        /// <summary>
        /// True when a shutdown us the underlying thread has been 
        /// requested.
        /// </summary>
        bool ShutdownRequested { get; }

        /// <summary>
        /// True when the calling thread has called EndShutdown()
        /// </summary>
        bool ShutdownComplete { get; }
        
        /// <summary>
        /// Requests a shutdown and waits for the consumer thread to
        /// call EndShutdown().
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Requests a shutdown and wakes up all waiting consumers.
        /// </summary>
        void StartShutdown();

        /// <summary>
        /// Called by the consumer thread when it exits after a shutdown
        /// request.
        /// </summary>
        void EndShutdown();
        
        /// <summary>
        /// Waits until the EndShutdown is called.
        /// </summary>
        void WaitForShutdown();
    }

    /// <summary>
    /// Interface for the different types of producer/consumer queues
    /// used throughout the system.
    /// </summary>
    public interface IWorkQueue<T>
    {
        /// <summary>
        /// Adds a work item without blocking.
        /// </summary>
        void Enqueue(T item);

        /// <summary>
        /// Grabs the oldest work item.  The implementation decides if
        /// we should block until there is data.
        /// </summary>
        bool Dequeue(out T item);
    }

    /// <summary>
    /// This queue never blocks the producers sending data, but blocks
    /// the consumer thread whenever the queue is empty until work is
    /// available.  Also provides a polite exit mechanism, which 
    /// prevents new work from becoming available and stops blocking
    /// the consumer so it can read the flag to destory itself as
    /// soon as possible.
    /// </summary>
    public class OutputQueue<T> : IWorkQueue<T>, IShutdownNicely
    {
        #region Constructors

        public OutputQueue()
        {
            _hasData = false;
            first = new Node();
            last = first;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns true if the queue is empty without blocking.
        /// </summary>
        public bool IsEmpty
        {
            get { return !_hasData; }
        }

        /// <summary>
        /// True when the thread should shut down once all messages
        /// are processed.
        /// </summary>
        public bool ShutdownRequested { get; private set; }

        /// <summary>
        /// True when the calling thread has called EndShutdown()
        /// </summary>
        public bool ShutdownComplete { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Requests a shutdown and waits for the consumer thread to
        /// call EndShutdown().
        /// </summary>
        public void Shutdown()
        {
            StartShutdown();
            WaitForShutdown();
        }

        /// <summary>
        /// Requests a shutdown and wakes up all waiting consumers.
        /// </summary>
        public void StartShutdown()
        {
            lock (_lock)
            {
                ShutdownRequested = true;
                Monitor.PulseAll(_lock);
            }
        }

        /// <summary>
        /// Called by the consumer thread when it exits after a shutdown
        /// request.
        /// </summary>
        public void EndShutdown()
        {
            lock (_shutdownLock)
            {
                ShutdownComplete = true;
                Monitor.PulseAll(_shutdownLock);
            }
        }

        /// <summary>
        /// Waits until the EndShutdown is called.
        /// </summary>
        public void WaitForShutdown()
        {
            lock (_shutdownLock)
            {
                while (!ShutdownComplete)
                {
                    Monitor.Wait(_shutdownLock);
                }
            }
        }

        /// <summary>
        /// Adds a new item to the queue with minimal blocking.
        /// </summary>
        public void Enqueue(T item)
        {
            // ignore request after shutdown
            if (ShutdownRequested) return;
            
            lock (_lock)
            {
                // append to linked list
                Node newNode = new Node();
                newNode.item = item;
                Node old = Interlocked.Exchange(ref first, newNode);
                old.next = newNode;

                // notify consumer that data is available
                _hasData = true;
                Monitor.PulseAll(_lock);
            }
        }

        /// <summary>
        /// Returns an item from the queue, blocking until a value is
        /// available or a shutdown has been requested.  Returns true
        /// iff an item was feteched.
        /// </summary>
        public bool Dequeue(out T item)
        {
            return Dequeue(out item, true);
        }

        /// <summary>
        /// Returns an item from the queue, with optional blocking.
        /// </summary>
        public bool Dequeue(out T item, bool blockUntilData = true)
        {
            lock (_lock)
            {
                // if in shutdown and no data is available, just return
                if (ShutdownRequested && !_hasData)
                {
                    item = default(T);
                    return false;
                }

                // check if data is available

                if (!_hasData)
                {
                    // exit early if blocking is turned off
                    if (!blockUntilData)
                    {
                        item = default(T);
                        return false;
                    }

                    // otherwise block until data is available
                    Monitor.Wait(_lock);
                }

                // find the next non-null element.  if none found, return false
                bool found = true;
                Node current;
                do
                {
                    current = last;
                    if (current.next == null)
                    {
                        found = false;
                        _hasData = false;
                        break;
                    }
                }
                while (current != Interlocked.CompareExchange(ref last, current.next, current));

                // return element
                if (found)
                {
                    _hasData = true;
                    item = current.next.item;
                    current.next.item = default(T);
                    return true;
                }
            }

            // if there was no data available, wait until there is and repeat
            return Dequeue(out item);
        }

        #endregion

        #region Private

        private bool _hasData;
        private readonly object _lock = new object();
        private readonly object _shutdownLock = new object();

        private Node first;
        private Node last;

        private class Node
        {
            public T item;
            public Node next;
        }

        #endregion
    }
}
