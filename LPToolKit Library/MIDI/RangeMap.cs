using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.MIDI
{
    /// <summary>
    /// Provides a mapping of all of the physical controls for various
    /// MIDI devices that are to be used by an implant.  This maps their
    /// physical xy addresses to their relative virtual addresses.
    /// </summary>
    /// <remarks>
    /// This is the device independent replacement for LPRange.
    /// </remarks>
    public class RangeMap
    {
        #region Constructors

        /// <summary>
        /// Constructor accepting the device this range is for.
        /// </summary>
        public RangeMap(MappedMidiDevice device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("MappedMidiDevice");
            }
            if (device.Device == null)
            {
                throw new ArgumentNullException("MappedMidiDevice.MidiDevice");
            }
            Device = device;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The device this range is on.
        /// </summary>
        public MappedMidiDevice Device;

        /// <summary>
        /// The starting physical address for this block.
        /// </summary>
        public int X, Y;

        /// <summary>
        /// The size of this block.
        /// </summary>
        public int Width, Height;

        /// <summary>
        /// The relative virtual coordinate for this block.
        /// </summary>
        public int VirtualX, VirtualY;

        /// <summary>
        /// Other controls to be treated as mapped within this same
        /// range, allowing for ranges that go across devices or are
        /// not contiguous on one device.
        /// </summary>
        public readonly List<RangeMap> Children = new List<RangeMap>();

        /// <summary>
        /// Returns the total width of the virtual space including 
        /// all children.
        /// </summary>
        public int TotalWidth { get { return CalculateWidth(); } }

        /// <summary>
        /// Returns the total height of the virtual space including 
        /// all children.
        /// </summary>
        public int TotalHeight { get { return CalculateHeight(); } }

        #endregion

        #region Methods

        /// <summary>
        /// Returns true iff the provided physical coordinate for the 
        /// given device, including any of its children.
        /// </summary>
        public bool Contains(MappedMidiDevice device, int x, int y)
        {
            return GetRangeFor(device, x, y) != null;
        }

        /// <summary>
        /// Returns true iff the provided virtual coordinate is valid, 
        /// including any of its children.
        /// </summary>
        public bool ContainsVirtual(int x, int y)
        {
            return GetRangeForVirtual(x, y) != null;
        }

        /// <summary>
        /// Outputs the relative virtual x/y address for a given
        /// physical address, returning true if the mapping worked.
        /// The returned address assumes the current scrolling is
        /// at 0,0, and will need to be adjusted if the view is
        /// scrolled.
        /// </summary>
        public bool GetVirtualAddress(MappedMidiDevice device, int x, int y, out int vx, out int vy)
        {
            var range = GetRangeFor(device, x, y);
            if (range != null)
            {
                // compute the virtual address for discovered mapping
                var dx = x - range.X;
                var dy = y - range.Y;
                vx = range.VirtualX + dx;
                vy = range.VirtualY + dy;
                return true;
            }

            // return a failed mapping
            vx = 0;
            vy = 0;
            return false;
        }

        /// <summary>
        /// Gets the device and physical XY address for a given 
        /// virtual address, returning false if it cannot be mapped.
        /// </summary>
        public bool GetPhysicalAddress(int x, int y, out MappedMidiDevice device, out int px, out int py)
        {
            var range = GetRangeForVirtual(x, y);
            if (range != null)
            {
                // compute the physical address for discovered mapping
                var dx = x - range.VirtualX;
                var dy = y - range.VirtualY;
                device = range.Device;
                px = range.X + dx;
                py = range.Y + dy;
                return true;
            }

            // return a failed mapping
            device = null;
            px = 0;
            py = 0;
            return false;
        }


        /// <summary>
        /// Returns the specific block within this range that is 
        /// mapped to a virtual address
        /// </summary>
        public RangeMap GetRangeForVirtual(int x, int y)
        {
            RangeMap result = null;

            // check if this result is cached
            string cacheKey = GetVirtualCacheKey(x, y);
            if (_contains.TryGetValue(cacheKey, out result))
            {
                return result;
            }

            // compute result if not cached
            try
            {
                // check this block
                if (x >= VirtualX && x < VirtualX + Width)
                {
                    if (y >= VirtualY && y < VirtualY + Height)
                    {
                        return result = this;
                    }
                }

                // check child blocks
                foreach (var child in Children)
                {
                    result = child.GetRangeForVirtual(x, y);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            finally
            {
                // cache result
                _contains[cacheKey] = result;
            }
            return result;
        }

        /// <summary>
        /// Returns the specific block within this range that is 
        /// mapping a device's physical x/y address.  Returns null
        /// if this address is not mapped to this object or its
        /// children.
        /// </summary>
        public RangeMap GetRangeFor(MappedMidiDevice device, int x, int y)
        {
            RangeMap result = null;

            // check input
            if (device == null)
            {
                throw new ArgumentNullException("MidiDevice");
            }

            // check if this result is cached
            string cacheKey = GetCacheKey(device, x, y);
            if (_contains.TryGetValue(cacheKey, out result))
            {
                return result;
            }

            // compute result if not cached
            try
            {
                // check this block
                if (device.Device.ID == Device.Device.ID)
                {
                    if (x >= X && x < X + Width)
                    {
                        if (y >= Y && y < Y + Height)
                        {
                            return result = this;
                        }
                    }
                }

                // check child blocks
                foreach (var child in Children)
                {
                    result = child.GetRangeFor(device, x, y);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            finally
            {
                // cache result
                _contains[cacheKey] = result;
            }
            return result;
        }

        #endregion

        #region Private 

        /// <summary>
        /// Creates a caching key for _contains for mapping physical
        /// address to virtual address.
        /// </summary>
        private string GetCacheKey(MappedMidiDevice device, int x, int y)
        {
            return string.Format("PHYS_{0}_{1}_{2}", device.Device.ID, x, y);
        }

        /// <summary>
        /// Creates a caching key for _contains for mapping virtual
        /// address to physical addresses
        /// </summary>
        private string GetVirtualCacheKey(int x, int y)
        {
            return string.Format("VIRT_{0}_{1}", x, y);
        }

        /// <summary>
        /// Calculates the width of the virtual space, not including
        /// virtual grid scrolling, but what it thinks is visible at once.
        /// TODO: cache this until changed.
        /// </summary>
        private int CalculateWidth()
        {
            int ret = VirtualX + Width;
            foreach (var child in Children)
            {
                var childWidth = child.CalculateWidth();
                if (childWidth > ret)
                {
                    ret = childWidth;
                }
            }
            return ret;
        }

        /// <summary>
        /// Calculates the height of the total visible space.
        /// </summary>
        private int CalculateHeight()
        {
            int ret = VirtualY + Height;
            foreach (var child in Children)
            {
                var childHeight = child.CalculateHeight();
                if (childHeight > ret)
                {
                    ret = childHeight;
                }
            }
            return ret;
        }

        /// <summary>
        /// Memoized storage for which block within this range provides
        /// the mapping for a physical address on a specific device,
        /// or null if it's not mapped.
        /// </summary>
        private readonly Dictionary<string, RangeMap> _contains = new Dictionary<string, RangeMap>();

        #endregion

    }
}
