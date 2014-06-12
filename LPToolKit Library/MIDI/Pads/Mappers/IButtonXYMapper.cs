using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.Core;

namespace LPToolKit.MIDI.Pads.Mappers
{
    /// <summary>
    /// Contract for objects that convert between MIDI number and
    /// and message types into XY coordinates.
    /// For now we'll keep doing -1 for the top menu, but we need
    /// to perhaps have different types of x/y?
    /// </summary>
    public interface IButtonXYMapper
    {
        /// <summary>
        /// Converts part of a MIDI message into its x/y coordinates.
        /// </summary>
        bool ConvertXY(MidiMessageType type, int noteNumber, out int x, out int y, out ImplantEventType eventType);

        /// <summary>
        /// Converts an XY coordinate into parts of a MIDI message.
        /// </summary>
        bool ConvertXY(int x, int y, out MidiMessageType type, out int noteNumber);
    }
}
