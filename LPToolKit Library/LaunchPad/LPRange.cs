using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.MIDI.Pads;

namespace LPToolKit.LaunchPad
{
    /// <summary>
    /// Specifies an area of buttons on the launchpad.  Can 
    /// specify any button, including the top row, which are
    /// marked as y=-1.
    /// </summary>
    [Obsolete]
    public class LPRange
    {
        public LPRange(int x0, int y0, int x1, int y1)
        {
            X0 = x0;
            Y0 = y0;
            X1 = x1;
            Y1 = y1;
        }

        public /*readonly */int X0;
        public  /*readonly */ int Y0;
        public  /*readonly */ int X1;
        public  /*readonly */ int Y1;

        public int Width { get { return (X1 - X0) + 1; } }
        public int Height { get { return (Y1 - Y0) + 1; } }

        /// <summary>
        /// Returns how large an array would need to be to store
        /// one value for each button in this range.
        /// </summary>
        public int GetArraySize()
        {
            return Width * Height;
        }
        
        /// <summary>
        /// Returns a unique array offset for each location for
        /// the size of range of this instance.
        /// </summary>
        public int GetArrayOffset(int x, int y)
        {
            return (y-Y0) * Width + (x-X0);
        }

        /// <summary>
        /// Checks that the pitch translates to an X/Y coordinate
        /// that is in bounds.
        /// </summary>
        public bool ContainsPitch(int pitch)
        {
            int x;
            int y;
            LaunchPadXY.GridFromPitch(pitch, out x, out y);
            return Contains(x, y);
        }

        /// <summary>
        /// Checks that a control change translates to an X/Y coordinate
        /// that is in bounds.
        /// </summary>
        public bool ContainsControl(int control)
        {
            int x = LaunchPadXY.GetControlX(control);
            int y = -1;
            return Contains(x, y);
        }

        /// <summary>
        /// Returns true iff the coordinates are within this range.
        /// </summary>
        public bool Contains(int x, int y)
        {
            if (x < X0) return false;
            if (x > X1) return false;
            if (y < Y0) return false;
            if (y > Y1) return false;
            if (y == -1 && x == 8) return false; // no button here, this would be where the ableton logo is
            return true;
        }

        /// <summary>
        /// Just the 8x8 grid of square buttons.
        /// </summary>
        public static LPRange Grid = new LPRange(0, 0, 7, 7);

        /// <summary>
        /// The entire interface.
        /// </summary>
        public static LPRange All = new LPRange(0, -1, 8, 7);

        /// <summary>
        /// The top menu
        /// </summary>
        public static LPRange TopMenu = new LPRange(0, -1, 7, -1);

        /// <summary>
        /// The right menu.
        /// </summary>
        public static LPRange RightMenu = new LPRange(8, 0, 8, 7);



    }


}
