using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace LPToolKit.Util
{
    /// <summary>
    /// This is a queue where the order is based on the timestamp of
    /// when each item must be completed by.  
    /// </summary>
    /// <remarks>
    /// New items are inserted into the queue depending on the 
    /// required completion time.  The time is stored as the number 
    /// of milliseconds elapsed according to a Stopwatch instance.
    /// Inserts are not immediately added to the queue, but are
    /// put into a LockFreeQueue until Dequeue is called when there
    /// are no late tasks.
    /// 
    /// The SkipTest property can be used to supply a means to test
    /// if there are tasks that should be ignored for the time being.
    /// This is used to implement the ReadyToRun property in kernel
    /// tasks, so just because a task seems to be late, it does not
    /// block other tasks when it is "sleeping".
    /// </remarks>
    public class PriorityQueue<T> : IWorkQueue<T> 
    {
        #region Constructors

        /// <summary>
        /// Constructor creates an empty queue and starts the timer.
        /// </summary>
        public PriorityQueue()
        {
            _insertionQueue = new LockFreeQueue<Node>();
            _timer = new Stopwatch();
            _timer.Start();
        }

        #endregion

        #region Delegates

        /// <summary>
        /// Allows an assertion to be tested against items in the
        /// queue without popping them.
        /// </summary>
        public delegate bool Test(T t);

        #endregion

        #region Properties

        /// <summary>
        /// The current internal timer value.
        /// </summary>
        public long ElapsedMsec
        {
            get { return _timer.ElapsedMilliseconds; }
        }

        /// <summary>
        /// True iff the work queue is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { lock (_workQueueLock) return _head == null; }
        }

        /// <summary>
        /// Delegate to check if a work item should be skipped over
        /// by Dequeue in favor of one further down the list.  Will
        /// not return any item that returns true to this delegate
        /// when provided.
        /// </summary>
        public Test SkipTest = null;

        #endregion

        #region Methods

        /// <summary>
        /// Runs a test against items in the queue, returning true
        /// the first time an item returns true, returning false
        /// after all tests fail.  Returns false if the queue is empty.
        /// </summary>
        public bool AnyMatch(Test test)
        {
            lock (_workQueueLock)
            {
                if (IsEmpty) return false;
                for (Node node = _head; node != null; node = node.next)
                {
                    if (test(node.item))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Runs a test against items in the queue, returning false
        /// the first time an item returns false, returning true
        /// after all tests pass.  Returns false if the queue is empty.
        /// </summary>
        public bool AllMatch(Test test)
        {
            lock (_workQueueLock)
            {
                if (IsEmpty) return false;
                for (Node node = _head; node != null; node = node.next)
                {
                    if (test(node.item) == false)
                    {
                        return false;
                    }
                }
                return true;
            }
        }        
        
        /// <summary>
        /// Adds a work item without blocking.  This first goes into
        /// the insertion queue, which is automatically processed the
        /// first time Dequeue is called where the head element is
        /// not already late.
        /// </summary>
        /// <param name="maxMsec">How long until we must guarantee this item to be processed.</param>
        public void Enqueue(T item, int maxMsec)
        {
            // insert with contractual completion time
            var node = new Node()
            {
                item = item,
                contractTime = ElapsedMsec + maxMsec
            };
            _insertionQueue.Enqueue(node);

            // notify any blocked consumers
            lock (_emptyQueueLock)
            {
                Monitor.PulseAll(_emptyQueueLock);
            }
        }        
           
        /// <summary>
        /// Enqueues the items with a default contract time of 1000msec.
        /// Most likely it will be done before then, but if we don't 
        /// demand a completion time then we must not really care.
        /// </summary>
        public void Enqueue(T item)
        {
            Enqueue(item, 1000);
        }

        /// <summary>
        /// Adds all items in the insert queue into the work queue
        /// until there is late work already in the queue.
        /// </summary>
        public void ProcessInsertions()
        {
            while (HasLateWork == false)
            {
                if (ProcessNextInsertion() == false)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Grabs the work item with the earliest required finish 
        /// date, which will be at the head of the linked list.  If
        /// no items are late, the insertion queue is automatically
        /// processed.
        /// </summary>
        public bool Dequeue(out T item)
        {
            lock (_workQueueLock)
            {
                // first add in any insertions
                ProcessInsertions();

                // the link list is ordered by the most urgent work,
                // but we can't use the head because of the option to
                // skip work until it is ready, so we have to return
                // the first item that shouldn't be skipped.
                Node first, prior;
                first = GetFirstNode(out prior);

                // since we can no longer guarantee this is the head,
                // we may be to change a link in the middle of the list,
                // so we need the prior node too
                if (first != null)
                {
                    item = first.item;
                    if (prior != null)
                    {
                        prior.next = first.next;
                    }
                    else
                    {
                        _head = _head.next;
                    }
                    return true;
                }
            }

            // queue must have no items that are ready
            item = default(T);
            return false;
        }

        /// <summary>
        /// Blocks the calling thread until work is available.
        /// </summary>
        public void BlockForWork(int timeoutMsec = -1)
        {
            lock(_emptyQueueLock)
            {
                // only lock if the queue is empty
                if (IsEmpty == false) return;

                // wait for a pulse from an add
                if (timeoutMsec > 0)
                {
                    Monitor.Wait(_emptyQueueLock, timeoutMsec);
                }
                else 
                {
                    Monitor.Wait(_emptyQueueLock);
                }
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// The timer that keeps track of priorities.
        /// </summary>
        private readonly Stopwatch _timer;

        /// <summary>
        /// Items waiting to be inserted into the main queue.
        /// </summary>
        private LockFreeQueue<Node> _insertionQueue;

        /// <summary>
        /// The head of the linked list.
        /// </summary>
        private Node _head;

        /// <summary>
        /// Use for mutex on the linked list.
        /// </summary>
        private readonly object _workQueueLock = new object();

        /// <summary>
        /// Useds for blocking the consumer thread until there is
        /// work to do.
        /// </summary>
        private readonly object _emptyQueueLock = new object();

        /// <summary>
        /// Inserts the oldest element in the insertion queue into
        /// the work queue at the correct location for its contracted
        /// completion time.  Returns true if a node was inserted.
        /// </summary>
        /// <remarks>
        /// This is private to avoid locking requirements.  Only call
        /// when the linked list is locked.
        /// </remarks>
        private bool ProcessNextInsertion()
        {
            // get the next node to insert (skipping nulls)
            Node node = null;
            while (node == null)
            {
                if (_insertionQueue.Dequeue(out node) == false)
                {
                    return false;
                }
            }

            // if the queue is empty, just make this item the new head
            if (_head == null)
            {
                _head = node;
                return true;
            }

            // follow each link on the linked list until the next
            // item is due after our insertion
            Node before = null;
            Node after = _head;
            while (after != null && after.contractTime < node.contractTime)
            {
                before = after;
                after = before.next;
            }

            // insert this node here.  if the before is still null, 
            // then we're inserted a new head.  we don't have to
            // worry about after being null
            if (before == null)
            {
                _head = node;
            }
            else
            {
                before.next = node;
            }
            node.next = after;
            return true;
        }

        /// <summary>
        /// Returns true iff there is work in the queue that is past
        /// it's contractual completion time.  
        /// </summary>
        /// <remarks>
        /// This is private to avoid locking requirements.  Only call
        /// when the linked list is locked.
        /// </remarks>
        private bool HasLateWork
        {
            get 
            {
                Node prior;
                var first = GetFirstNode(out prior);
                return first != null && first.contractTime < _timer.ElapsedMilliseconds; 
            }
        }

        /// <summary>
        /// Returns the first node that shouldn't be skipped, or the
        /// head if SkipTest isn't set. 
        /// </summary>
        /// <remarks>
        /// This is private to avoid locking requirements.  Only call
        /// when the linked list is locked.
        /// </remarks>
        private Node GetFirstNode(out Node prior)
        {
            prior = null;
            if (SkipTest == null)
            {
                return _head;
            }
            Node ret = _head;
            while (ret != null)
            {
                if (SkipTest(ret.item) == false)
                {
                    return ret;
                }
                prior = ret;
                ret = ret.next;
            }
            return ret;
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
            /// Must be completed beforet the timer reaches this value.
            /// </summary>
            public long contractTime;
        }

        #endregion
    }
}
