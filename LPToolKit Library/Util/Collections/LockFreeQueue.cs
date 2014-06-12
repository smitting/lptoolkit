using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LPToolKit.Util
{
    /// <summary>
    /// A lock free queue.
    /// Source: http://www.mycsharp.de/wbb2/thread.php?threadid=69007
    /// </summary>
    public class LockFreeQueue<T>
    {
        public LockFreeQueue()
        {
            first = new Node();
            last = first;
        }

        Node first;
        Node last;

        public void Enqueue(T item)
        {
            Node newNode = new Node();
            newNode.item = item;
            Node old = Interlocked.Exchange(ref first, newNode);
            old.next = newNode;
        }

        public bool Dequeue(out T item)
        {
            Node current;
            do
            {
                current = last;
                if (current.next == null)
                {
                    item = default(T);
                    return false;
                }
            }
            while (current != Interlocked.CompareExchange(ref last, current.next, current));
            item = current.next.item;
            current.next.item = default(T);
            return true;
        }

        class Node
        {
            public T item;
            public Node next;
        }
    }
}
