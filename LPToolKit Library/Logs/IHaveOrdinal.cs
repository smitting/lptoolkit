using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace LPToolKit.Logs
{
    /// <summary>
    /// Contract for objects managed by OrdinalLists
    /// </summary>
    public interface IHaveOrdinal
    {
        /// <summary>
        /// The order this record was created in.
        /// </summary>
        int Ordinal { get; set; }
    }

    /// <summary>
    /// Base class for logs that are created in a certain order, so
    /// changes since a certain ordinal can be loaded, and threads
    /// can be blocked until new data beyond an ordinal is added.
    /// </summary>
    public class OrdinalList<T> where T : IHaveOrdinal, new()
    {
        #region Constructors

        public OrdinalList()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// The next index to be set.
        /// </summary>
        public int NextOrdinal
        {
            get { return _nextOrdinal; }
        }

        /// <summary>
        /// Returns all of the items in this list, but changing this list
        /// does not affect its contents.
        /// </summary>
        public List<T> AllItems
        {
            get { return Items; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the all records since an ordinal number, optionally
        /// blocking the calling thread until data is available
        /// </summary>
        public List<T> GetSinceOrdinal(int ordinal, bool waitForData = false)
        {
            if (waitForData)
            {
                // TODO: use signals to avoid resource usage here and quicker response.
                while (NextOrdinal <= ordinal)
                {
                    Thread.Sleep(100);
                }
            }
            return Items.Where(t => t.Ordinal >= ordinal).ToList();
        }

        /// <summary>
        /// Adds a new record to the list, automatically assigning 
        /// the next ordinal.
        /// </summary>
        public void Add(T t)
        {
            t.Ordinal = (_nextOrdinal++);
            Items.Add(t);
            // TODO: wake up waiting threads here
        }

        #endregion 

        #region Private

        /// <summary>
        /// Actual list storage.
        /// </summary>
        protected List<T> Items = new List<T>();

        private int _nextOrdinal = 1;

        #endregion

    }
}
