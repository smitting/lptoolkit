using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.MIDI.Pads.Mappers
{
    /// <summary>
    /// Contract for objects that convert between MIDI pitches and
    /// colors displayed on buttons for a specific hardware type.
    /// Color names such as "red" or "off" should be mapped elsewhere,
    /// and this class should deal with colors in RGB formats of
    /// #RRGGBB like "#00FF40".  The hardware should handle the 
    /// quantizing, so green should be "#00FF00" NOT "#000300"
    /// even though it only supports 3 shades of green.
    /// </summary>
    public interface IButtonColorMapper
    {
        /// <summary>
        /// Converts a color in format "#00FF00" to its MIDI pitch.
        /// i.e. for the launchpad this would correlate to green=3
        /// or the midi pitch 16*3+12 = 60.
        /// </summary>
        int ColorToValue(string color);

        /// <summary>
        /// Converts a MIDI pitch into a color string in the format
        /// "#00FF00".  For example, a full green would be returned
        /// as "#00FF00" even though the LaunchPad's highest green
        /// value is 3.
        /// </summary>
        string ValueToColor(int value);
    }
}
