using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.MIDI.Pads;

namespace LPToolKit.MIDI.Pads.Mappers
{

    /// <summary>
    /// Provides MIDI to color mapping for all novation devices, uncluding
    /// the LaunchPad, LaunchControl, and LaunchKeys.
    /// </summary>
    public class NovationColorMapper : IButtonColorMapper
    {
        /// <summary>
        /// Always use the 12 flag.
        /// </summary>
        public const int FLAGS = 12;

        public int ColorToValue(string c)
        {
            var color = new ButtonColor(c, 4);
            return color.QGreen * 16 + color.QRed + FLAGS;
        }

        public string ValueToColor(int value)
        {
            int green = (int)Math.Floor(value / 16.0);
            var leftover = value % 16;
            int red = leftover % 4;
            int flags = leftover - red;

            return new ButtonColor(red, green, 0, 4).Text;
        }
    }

}
