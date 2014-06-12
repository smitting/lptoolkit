using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.Logs
{
    /// <summary>
    /// Base class for all log messages.
    /// </summary>
    public abstract class LogBase : IHaveOrdinal 
    {
        #region Constructors

        /// <summary>
        /// Constructor builds the timestamp and ordinal.
        /// </summary>
        public LogBase()
        {
            Timestamp = DateTime.Now;
            Ordinal = (_nextOrdinal++);
        }

        #endregion

        #region Properties

        /// <summary>
        /// When this message was recorded.
        /// </summary>
        public readonly DateTime Timestamp;

        /// <summary>
        /// The order this message was created in.
        /// </summary>
        public int Ordinal { get; set; }

        #endregion

        #region Private

        private static int _nextOrdinal = 1;

        #endregion
    }

    public abstract class LogBaseWithSourceAndDestination : LogBase
    {
        /// <summary>
        /// The source of this message.
        /// </summary>
        public string Source;

        /// <summary>
        /// Where this message got routed to.
        /// </summary>
        public string Destination;

    }
}
