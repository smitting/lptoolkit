using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.Util
{

    /// <summary>
    /// Stores a list of times that wraps at some length, proving a
    /// way for classes to monitor how long certain events are taking
    /// in order to measure lag.
    /// </summary>
    internal class LagTracker
    {
        #region Constructor

        public LagTracker(int size)
        {
            Index = 0;
            Times = new int[size];
            Tags = new string[size];
            for (var i = 0; i < size; i++)
            {
                Times[i] = -1;
                Tags[i] = null;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Next index where the time will be written.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// The times in msec recorded, or -1 for empty.
        /// </summary>
        public readonly int[] Times;

        /// <summary>
        /// Allows the source of the lag to be differentiated.
        /// </summary>
        public readonly string[] Tags;

        /// <summary>
        /// Number of samples stored.
        /// </summary>
        public int Size
        {
            get { return Times.Length; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Logs a new time in this instance.
        /// </summary>
        public void Log(int msec, string tag = null)
        {
            lock (this)
            {
                Times[Index] = msec;
                Tags[Index] = tag;
                Index++;
                if (Index >= Size)
                {
                    Index = 0;
                }
            }
        }

        /// <summary>
        /// Logs the delta milliseconds in this instance.
        /// </summary>
        public void Log(DateTime start, DateTime end, string tag = null)
        {
            Log((int)(end - start).TotalMilliseconds, tag);
        }

        /// <summary>
        /// Returns the most recent events in order with average.
        /// </summary>
        public Stats GetRecent(int count)
        {
            var stats = new Stats();
            for (int i = Index - 1; i >= 0; i--)
            {
                if (stats.Times.Count >= count) break;
                var msec = Times[i];
                if (msec > -1)
                {
                    stats.Times.Add(msec);
                    stats.Tags.Add(Tags[i]);
                    stats.TotalTime += msec;
                }
            }
            for (int i = Size - 1; i >= Index; i--)
            {
                if (stats.Times.Count >= count) break;
                var msec = Times[i];
                if (msec > -1)
                {
                    stats.Times.Add(msec); 
                    stats.Tags.Add(Tags[i]);
                    stats.TotalTime += msec;
                }
            }
            stats.Average = stats.TotalTime / (double)stats.Times.Count;
            return stats;
        }

        #endregion

        #region Types

        /// <summary>
        /// Output of the GetRecent() method, suitable for returning
        /// as ajax.
        /// </summary>
        public class Stats
        {
            public List<int> Times = new List<int>();
            public List<string> Tags = new List<string>();
            public double TotalTime = 0;
            public double Average = 0;
        }

        #endregion

        #region Private


        #endregion
    }

}
