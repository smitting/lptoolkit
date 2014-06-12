using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.MIDI.Pads
{
    /// <summary>
    /// Maps colors to launchpad MIDI note velocities to display 
    /// colors.  This value of this object never changes after 
    /// the constructor.
    /// </summary>
    [Obsolete]
    public class LaunchPadColor
    {
        #region Constructor

        /// <summary>
        /// Constructor accepts the red and green level to use, and
        /// optionally flags.
        /// </summary>
        public LaunchPadColor(int red, int green, int flags = 12)
        {
            R = red < 0 ? 0 : red > 3 ? 3 : red;
            G = green < 0 ? 0 : green > 3 ? 3 : green;
            Flags = flags;
            Encoded = Encode(red, green, flags);
        }

        #endregion

        #region Preset Colors

        /// <summary>
        /// Returns a color by name.
        /// </summary>
        public static LaunchPadColor FromName(string name)
        {
            if (name == null) return Off;

            // attempt to find a named color
            switch (name.ToLower().Trim())
            {
                case "off": return Off;
                case "red": return Red;
                case "redorange": return RedOrange;
                case "orange": return Orange;
                case "amber": return Amber;
                case "yellow": return Yellow;
                case "yellowgreen": return YellowGreen;
                case "green": return Green;
            }

            // attempt from RG or RGB format
            if (name.StartsWith("#"))
            {
                name = name.Substring(1);
            }

            if (name.Length == 2 || name.Length == 3)
            {
                if (Char.IsDigit(name[0]) && Char.IsDigit(name[1]))
                {
                    int r = int.Parse(name.Substring(0, 1));
                    int g = int.Parse(name.Substring(1, 1));
                    return new LaunchPadColor(r, g);                    
                }
            }

            return Off;
        }


        public static LaunchPadColor Off = new LaunchPadColor(0, 0);
        public static LaunchPadColor Red = new LaunchPadColor(3, 0);
        public static LaunchPadColor RedOrange = new LaunchPadColor(3, 1);
        public static LaunchPadColor Orange = new LaunchPadColor(3, 2);
        public static LaunchPadColor Amber = new LaunchPadColor(3, 3);
        public static LaunchPadColor Yellow = new LaunchPadColor(2, 3);
        public static LaunchPadColor YellowGreen = new LaunchPadColor(1, 3);
        public static LaunchPadColor Green = new LaunchPadColor(0, 3);

        #endregion

        #region Properties

        /// <summary>
        /// Level of red to show 0..3
        /// </summary>
        public readonly int R;

        /// <summary>
        /// Level of green to show 0..3
        /// </summary>
        public readonly int G;

        /// <summary>
        /// Flags for the launchpad.  12 = normal.
        /// </summary>
        public readonly int Flags;

        /// <summary>
        /// The value encoded for sending to the device.
        /// </summary>
        public readonly int Encoded;

        /// <summary>
        /// Returns true if all colors are off.
        /// </summary>
        public bool IsOff
        {
            get { return R == 0 && G == 0; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the code to be passed to the launchpad to emit
        /// a certain color.
        /// </summary>
        public static int Encode(int red, int green, int flags = 12)
        {
            if (green < 0 || green > 3)
            {
                throw new ArgumentOutOfRangeException("green");
            }
            if (red < 0 || red > 3)
            {
                throw new ArgumentOutOfRangeException("red");
            }
            // TODO: check flags too
            return green * 16 + red + flags;
        }

        /// <summary>
        /// Gets the components from an encoded color.
        /// </summary>
        public static void Decode(int midi, out int red, out int green, out int flags)
        {
            //g=3,r=0,f=12 === 3 * 16 + 0 + 12 = 60
            //g=3,r=3,f=12 === 3 * 16 + 3 + 12 = 63
            //g=0,r=3,f=12 === 0 * 16 + 3 + 12 = 15
            //g=1,r=0,f=12 === 1 * 16 + 0 + 12 = 28
            //g=1,r=3,f=12 === 1 * 16 + 3 + 12 = 31
            //g=2,r=0,f=0  === 2 * 16 + 0 + 0  = 32
            //g=2,r=3,f=12 === 2 * 16 + 3 + 12 = 47

            green = (int)Math.Floor(midi / 16.0);
            var leftover = midi % 16;

            //r=0,f=00 === 0 + 00 = 0
            //r=0,f=12 === 0 + 12 = 12
            //r=1,f=00 === 1 + 00 = 1
            //r=1,f=12 === 1 + 12 = 13
            //r=2,f=00 === 2 + 0  = 2
            //r=2,f=12 === 2 + 12 = 14

            red = leftover % 4;
            flags = leftover - red;
        }

        #endregion
    }
}
