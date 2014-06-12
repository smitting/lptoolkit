using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.MIDI.Hardware;
using LPToolKit.Core.Tasks;

namespace LPToolKit.MIDI.Pads
{

    /// <summary>
    /// Provides an arbitrarily large virtual space for lit button
    /// type MIDI interfaces like the LaunchPad.  This allows implants
    /// to see fixed hardware sizes like and 8x8 grid as much larger,
    /// filtering out any MIDI data sent to a device that would have
    /// no effect, and efficiently handling scrolling.
    /// 
    /// VirtualGrids can also be layered to easily allow temporary
    /// content to be displayed over existing content but easily
    /// restored, such as showing a play cursor.
    /// 
    /// The data is stored simply as MIDI values from 0-127, rather
    /// than having some meaning to the colors.  Colors should be
    /// encoded to MIDI values before being written to this grid,
    /// making this grid device independent.
    /// </summary>
    /// <remarks>
    /// This replaces the concepts from LaunchPadBuffer in a more 
    /// generic sense.
    /// </remarks>
    public class VirtualGrid
    {
        #region Constructors

        /// <summary>
        /// Constructor accepts the size to use.
        /// </summary>
        public VirtualGrid(MidiXYRouteHandler xyhandler, int width, int height, int realWidth = 9, int realHeight = 8)
        {
            // make sure a handler is set or this data is useless.
            if (xyhandler == null)
            {
                throw new ArgumentNullException("MidiXYRouteHandler");
            }
            XYHandler = xyhandler;
            Resize(width, height);
            ResizeVisible(realWidth, realHeight);
            ScrollTo(0, 0);
        }

        #endregion

        #region Properties

        /// <summary>
        /// The device being fed XY MIDI notes.
        /// </summary>
        public readonly MidiXYRouteHandler XYHandler;

        /// <summary>
        /// The width of the virtual buffer storing MIDI values.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// The height of the virtual buffer storing MIDI values.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// The width of the window of data available directly on the
        /// device at one time.
        /// </summary>
        public int VisibleWidth { get; private set; }

        /// <summary>
        /// The height of the window of data available directly on the
        /// device at one time.
        /// </summary>
        public int VisibleHeight { get; private set; }

        /// <summary>
        /// The X location into the virtual space for the left side
        /// of the visible window.
        /// </summary>
        public int VisibleX { get; private set; }

        /// <summary>
        /// The Y location into the virtual space for the top of the 
        /// visible window.
        /// </summary>
        public int VisibleY { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Changes the size of the virtual space.
        /// </summary>
        public void Resize(int w, int h)
        {
            var newGrid = GridStatus.CreateArray(w, h);
            GridStatus.CopyData(_grid, newGrid);

            // TODO: resizes could be dangerous.  may need threadsafing or don't allow resize

            // keep new settings
            Width = w;
            Height = h;
            _grid = newGrid;
        }

        /// <summary>
        /// Changes the size of the space visible on the real device.
        /// </summary>
        public void ResizeVisible(int w, int h)
        {
            VisibleWidth = w;
            VisibleHeight = h;
            _visible = GridStatus.CreateArray(w, h);
            ForceRepaint();            
        }

        /// <summary>
        /// Changes the visible location into the data.
        /// </summary>
        public void ScrollTo(int x, int y)
        {
            VisibleX = x;
            VisibleY = y;
            Repaint();
        }

        /// <summary>
        /// Does relative scrolling of the visible location.
        /// </summary>
        public void ScrollBy(int x, int y)
        {
            ScrollTo(VisibleX + x, VisibleY + y);
        }

        /// <summary>
        /// Relative scrolling by page, which depends on the visible size.
        /// </summary>
        public void ScrollByPage(int x, int y)
        {
            ScrollBy(x * VisibleWidth, y * VisibleHeight);
        }

        /// <summary>
        /// Checks each grid coordinate to make sure the color is correct.
        /// </summary>
        public void Repaint()
        {
            for (var dx = 0; dx < VisibleWidth; dx++)
            {
                for (var dy = 0; dy < VisibleHeight; dy++)
                {
                    SendXY(VisibleX + dx, VisibleY + dy);
                }
            }
        }

        /// <summary>
        /// Does a force redraw by setting the visible cache to -1 and
        /// sending the grid values for each visible coordinate.
        /// </summary>
        public void ForceRepaint()
        {
            for (var dx = 0; dx < VisibleWidth; dx++)
            {
                for (var dy = 0; dy < VisibleHeight; dy++)
                {
                    _visible[dx, dy].Invalidate();
                }
            }
            Repaint();
        }

        /// <summary>
        /// Changes the value on the virtual grid, drawing if needed.
        /// </summary>
        public void Set(int x, int y, int value)
        {
            if (!InVirtualGrid(x, y)) return;
            _grid[x, y].Color = value;
            SendXY(x, y);
        }


        /// <summary>
        /// Sets all visible grid values to the same color.
        /// </summary>
        public void SetAll(int value)
        {
#warning this is part of figuring how how to gracefully handle mode changes
            for (var dx = 0; dx < VisibleWidth; dx++)
            {
                for (var dy = 0; dy < VisibleHeight; dy++)
                {
                    Set(dx, dy, value);
                }
            }
        }

        /// <summary>
        /// Gets a value on the virtual grid, or -1 if off grid.
        /// </summary>
        public int Get(int x, int y)
        {
            return InVirtualGrid(x,y) ? _grid[x, y].Color : -1;
        }

        /// <summary>
        /// Considers hardware lag to get what the most likely color
        /// is actually on the hardware instead of what was last
        /// set to the grid.  Only useful for visible area.
        /// </summary>
        public int GetActual(int x, int y)
        {
            return InVirtualGrid(x, y) ? _grid[x, y].Actual : -1;
        }

        #endregion

        #region Private

        /// <summary>
        /// Returns true iff the x/y coordinate is valid.
        /// </summary>
        private bool InVirtualGrid(int x, int y)
        {
            if (x < 0 || y < 0) return false;
            if (x >= Width || y >= Height) return false;
            return true;
        }

        /// <summary>
        /// Returns true iff the x/y coordinate is visible.
        /// </summary>
        private bool InVisibleGrid(int x, int y)
        {
            if (x < VisibleX || y < VisibleY) return false;
            if (x >= VisibleX + VisibleWidth || y >= VisibleY + VisibleHeight) return false;
            return true;
        }

        /// <summary>
        /// Sends MIDI data only if this change in virtual x/y space 
        /// neccessitates sending the data.
        /// </summary>
        private void SendXY(int x, int y)
        {
            // make sure it's within the grid's bounds
            if (!InVirtualGrid(x, y)) return;

            //  make sure it's on screen
            if (!InVisibleGrid(x, y)) return;

#warning this is where the hardware checks need to happen to decide if we should cancel an event

            // then make sure this actually causes a change
            var targetColor = _grid[x, y].Color;
            var vx = x - VisibleX;
            var vy = y - VisibleY;
            if (_visible[vx, vy].Color != targetColor)
            {
                _visible[vx, vy].Color = targetColor;
                var scheduled = XYHandler(vx, vy, targetColor) as IMonitoredKernelTask;
                if (scheduled != null)
                {
                    scheduled.TaskProcessed += (task) =>
                    {
                        _grid[x, y].Actual = targetColor;
                        _visible[vx, vy].Actual = targetColor;
                    };
                    
                    /*
                    scheduled.OnProcessed((msg) =>
                    {
                        _grid[x, y].Actual = targetColor;
                        _visible[vx, vy].Actual = targetColor;
                    })*/
                }
            }
        }

        /// <summary>
        /// Actual grid storage
        /// </summary>
        private GridStatus[,] _grid = null;

        /// <summary>
        /// Cache of what's currently visible
        /// </summary>
        private GridStatus[,] _visible = null;


        /// <summary>
        /// Color value at a grid coordinant that differentiates 
        /// between what the color will be once all scheduled midi
        /// messages are processed and what the color currently is
        /// on the hardware.
        /// </summary>
        private class GridStatus
        {
            /// <summary>
            /// The color that will be on the hardware once all 
            /// scheduled messages are finished.
            /// </summary>
            public int Color = -1;

            /// <summary>
            /// The color most likely to be on the hardware right now
            /// which gets updated once the MidiDriver reports is has
            /// actually sent the MidiMessage to the device.
            /// </summary>
            public int Actual = -1;

            /// <summary>
            /// Puts the status into an invalid state which will force
            /// the next color to be written to hardware.
            /// </summary>
            public void Invalidate()
            {
                Color = -1;
                Actual = -1;
            }

            /// <summary>
            /// Creates a 2D array of a given size with every cell 
            /// initialized to an object.
            /// </summary>
            public static GridStatus[,] CreateArray(int w, int h)
            {
                var ret = new GridStatus[w, h];
                var maxX = ret.GetUpperBound(0);
                var maxY = ret.GetUpperBound(1);
                for (int x = 0; x <= maxX; x++)
                {
                    for (int y = 0; y <= maxY; y++)
                    {
                        ret[x, y] = new GridStatus();
                    }
                }
                return ret;
            }

            /// <summary>
            /// Copies all available data from the source to the 
            /// destination.  
            /// </summary>
            /// <remarks>
            /// If the source is bigger, the data is truncated. 
            /// If the destination is bigger, only the overlapping area is copied.
            /// Only Color is copied.  Hardware lag is discounted.
            /// </remarks>
            public static void CopyData(GridStatus[,] source, GridStatus[,] destination)
            {
                if (source == null) return;

                int x1 = destination.GetUpperBound(0);
                int oldX1 = source.GetUpperBound(0);
                if (oldX1 < x1)
                {
                    x1 = oldX1;
                }

                int y1 = destination.GetUpperBound(1);
                int oldY1 = source.GetUpperBound(1);
                if (oldY1 < y1)
                {
                    y1 = oldY1;
                }

                for (int x = 0; x <= x1; x++)
                {
                    for (int y = 0; y <= y1; y++)
                    {
                        destination[x, y].Actual = destination[x, y].Color = source[x, y].Color;
                    }
                }
            }
        }

        #endregion

    }
}
