using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.MIDI.Pads
{
    /// <summary>
    /// Maps x/y coordinates on the launchpad to MIDI values.
    /// </summary>
    [Obsolete]
    public class LaunchPadXY
    {
        #region Properties

        /// <summary>
        /// The MIDI control message for the first mode button.
        /// </summary>
        public const int FirstModeControl = 104;

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new MIDI message from an x/y location and
        /// whether to indicate press or release.  Mainly for
        /// simulating a launchpad.
        /// </summary>
        public static MidiMessage GetMidiMessage(int x, int y, bool down = true)
        {
            var msg = new MidiMessage();
            if (y < 0)
            {
                msg.Type = MidiMessageType.ControlChange;
                msg.Control = FirstModeControl + x;
                msg.Value = down ? 127 : 0;
            }
            else
            {
                msg.Type = MidiMessageType.NoteOn;
                msg.Pitch = PitchFromGrid(x, y);
                msg.Velocity = down ? 127 : 0;
            }
            return msg;
        }

        /// <summary>
        /// Returns which MIDI pitch to use to refer to a specific
        /// button on the grid.
        /// </summary>
        public static int PitchFromGrid(int x, int y)
        {
            return y * 16 + x;
        }

        /// <summary>
        /// Converts both pitchs and control changes to x/y 
        /// coordinates.  Control changes have y = -1.
        /// </summary>
        public static void GridFromPitch(int pitch, bool controlChange, out int x, out int y)
        {
            if (controlChange)
            {
                y = -1;
                x = LaunchPadXY.GetControlX(pitch);
            }
            else
            {
                GridFromPitch(pitch, out x, out y);
            }
        }

        /// <summary>
        /// Returns the x and y coordinate of the pitch returned
        /// by a button press.
        /// </summary>
        public static void GridFromPitch(int pitch, out int x, out int y)
        {
            y = pitch / 16;
            x = pitch - y * 16;
        }

        /// <summary>
        /// Converts a control change control number to X position of
        /// the top menu button.
        /// </summary>
        public static int GetControlX(int control)
        {
            return control - FirstModeControl;
        }

        #endregion
    }
}
