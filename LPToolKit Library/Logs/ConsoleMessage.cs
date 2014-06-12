using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.Logs
{
    /// <summary>
    /// How console messages are stored once they hit the console manager.
    /// </summary>
    public class ConsoleMessage : LogBaseWithSourceAndDestination
    {
        #region Constructors

        /// <summary>
        /// Constructor builds the timestamp and ordinal.
        /// </summary>
        public ConsoleMessage() : base()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// The message itself.
        /// </summary>
        public string Message;

        #endregion

        #region Private

        #endregion
    }
}
