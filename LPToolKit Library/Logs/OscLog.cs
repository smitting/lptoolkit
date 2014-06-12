using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.OSC;

namespace LPToolKit.Logs
{
    /// <summary>
    /// One log of an OSC message that was sent or received in the system.
    /// </summary>
    public class OscLog : LogBaseWithSourceAndDestination
    {
        #region Constructors

        public OscLog() : base()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// The OSC data to be logged.
        /// </summary>
        public OscDataMessage Message;

        #endregion
    }
}
