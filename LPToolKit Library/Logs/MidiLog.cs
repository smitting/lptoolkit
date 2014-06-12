using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.MIDI;

namespace LPToolKit.Logs
{
    /// <summary>
    /// Single MIDI message in the log of incoming and outgoing MIDI
    /// messages for the system.
    /// </summary>
    public class MidiLog : LogBaseWithSourceAndDestination
    {
        #region Constructors

        public MidiLog()
            : base()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// True iff the message came from a device, false if sent
        /// out the OSC map.
        /// </summary>
        public bool Incoming = false;

        /// <summary>
        /// The message sent.
        /// </summary>        
        public MidiMessage Message;

        #endregion
    }
}
