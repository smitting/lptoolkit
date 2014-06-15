//#define DEBUG_LAG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LPToolKit.Util.Collections
{
    /// <summary>
    /// This is a queue for running task at exactly specified times.
    /// Every item is added with an exact time that the item should
    /// be returned from Dequeue.  Dequeue() blocks until there is
    /// and item that should be returned at that exact moment.  If
    /// Dequeue() is blocked and an item is added that is earlier
    /// than the task it is waiting on, it starts waiting for the new
    /// task.  If that task is in the past, Dequeue() returns immediately.
    /// </summary>
    /// <remarks>
    /// This does not implement IWorkQueue<T> because we do not 
    /// want to allow tasks to be enqueued without an exact execution
    /// time, since that is the whole point of real time software.
    /// </remarks>
    public class RealTimeQueue<T> 
    {
         #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public RealTimeQueue()
        {
            _sync = new RealTimeSync();
        }

        #endregion

        #region Properties
        

        /// <summary>
        /// The number of milliseconds prior to the next wake time
        /// to start using SpinWait (and peg the CPU) to guarantee
        /// waking at the correct time.
        /// </summary>
        public int SpinThreshold
        {
            get { return _sync.SpinThreshold; }
        }


        /// <summary>
        /// The number of iterations for the SpinWait call, effectively
        /// controlling the granularity of WaitUntil()
        /// </summary>
        public int SpinInterations
        {
            get { return _sync.SpinInterations; }
        }


        /// <summary>
        /// Stops all processing of the queue when set to false.  All 
        /// blocked threads are released and all calls to Dequeue() 
        /// immediately returns false until set back to true.
        /// </summary>
        public bool Enabled
        {
            get { return _sync.Enabled; }
            set 
            { 
                _sync.Enabled = value; 
                SignalWork();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a work item to be returned at an exact time.
        /// </summary>
        public void Enqueue(T item, DateTime executeAt)
        {
            Enqueue(new Node()
            {
                item = item,
                executeAt = executeAt,
                next = null
            });           
        }

        /// <summary>
        /// Blocks until an item is schedule to be returned at the
        /// current time.  Returns false if the queue is empty only.
        /// </summary>
        public bool Dequeue(out T item)
        {
            Node node = null;

            do
            {
                // save our old node if we're switching
                if (node != null)
                {
                    Enqueue(node);
                }

                // grab the node we'll be returning, failing if we can't get one
                if (Dequeue(out node) == false)
                {
                    item = default(T);
                    return false;
                }
            }
            // repeat if we are interupted to switch nodes
            while (_sync.WaitUntil(node.executeAt) == false);

#if DEBUG_LAG
            // print out the difference in time between the desired 
            // run time and the actual run time (note enabling this
            // printing will pretty much ruin the actual accurace)
            LPConsole.WriteLine("RealTimeQueue", "Execution Lag={0}msec ({1} ticks)", 
                (DateTime.UtcNow - node.executeAt).TotalMilliseconds,
                DateTime.UtcNow.Ticks - node.executeAt.Ticks
                );
#endif

            // we have a node and it's time to execute it
            item = node.item;
            return true;
        }

        /// <summary>
        /// Blocks until there is work in the queue, or stop was called.
        /// </summary>
        public void BlockForWork()
        {
            while (Enabled && _head == null)
            {
                lock (_newWorkSignal)
                {
                    Monitor.Wait(_newWorkSignal);
                }
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// Blocks the thread until an exact time.
        /// </summary>
        private RealTimeSync _sync;

        /// <summary>
        /// The head of the linked list.
        /// </summary>
        private Node _head;

        /// <summary>
        /// The number of nodes in the linked list.  More for debugging
        /// to make sure the list isn't just growing.
        /// </summary>
        private int _listSize = 0;

        /// <summary>
        /// Use for mutex on the linked list.
        /// </summary>
        private readonly object _workQueueLock = new object();

        /// <summary>
        /// Used by BlockForWork() to wait for new work to be added
        /// to the queue.
        /// </summary>
        private readonly object _newWorkSignal = new object();

        /// <summary>
        /// Unblocks BlockForWork().
        /// </summary>
        private void SignalWork()
        {
            lock (_newWorkSignal)
            {
                Monitor.PulseAll(_newWorkSignal);
            }
        }

        /// <summary>
        /// Removes and returns the head of the queue, or null for
        /// an empty list or disabled queue
        /// </summary>
        private bool Dequeue(out Node node)
        {
            // refuse to pop head when disabled
            if (Enabled == false)
            {
                node = null;
                return false;
            }

            // pop the head.
            lock (_workQueueLock)
            {
                if (_head == null)
                {
                    node = null;
                    return false;
                }
                else
                {
                    node = _head;
                    _head = node.next;
                    _listSize--;
                    return true;
                }
            }
        }

        /// <summary>
        /// Inserts a fully formed node.
        /// </summary>
        private void Enqueue(Node node)
        {
            lock (_workQueueLock)
            {
                // insert into linked list by execution time
                Node prior = null;
                Node cursor = _head;
                while (cursor != null && cursor.executeAt < node.executeAt)
                {
                    prior = cursor;
                    cursor = cursor.next;
                }

                // we're inserting at the head if there is no prior
                if (prior == null)
                {
                    node.next = _head;
                    _head = node;

                    // unblock Dequeue() if we inserted at the head
                    _sync.PulseAll();
                }
                // we're inserting later in the list
                else
                {
                    prior.next = node;
                    node.next = cursor;
                }

                // inform all threads new work is available
                _listSize++;
                SignalWork();
            }
        }

        /// <summary>
        /// A linked list that stores when the item has been guaranteed
        /// to be completed.
        /// </summary>
        private class Node
        {
            /// <summary>
            /// The value of this node.
            /// </summary>
            public T item;

            /// <summary>
            /// The next node, implementing a linked list.
            /// </summary>
            public Node next;

            /// <summary>
            /// Exact time to return this node.
            /// </summary>
            public DateTime executeAt;
        }

        #endregion
    }
}
