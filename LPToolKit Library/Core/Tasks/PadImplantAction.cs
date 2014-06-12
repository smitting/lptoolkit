using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.MIDI;
using LPToolKit.MIDI.Hardware;

namespace LPToolKit.Core.Tasks
{

    /// <summary>
    /// Base class for all implant action involving a change on an
    /// implant device.
    /// </summary>
    public abstract class PadImplantAction : ImplantAction
    {
        #region Properties

        /// <summary>
        /// The coordinate to change.
        /// </summary>
        public int X, Y;

        #endregion
    }

    /// <summary>
    /// Action created when an implant requests a change to a color of
    /// a button on a pad device.  The physical X/Y address is determined
    /// at schedule time.
    /// </summary>
    public class PadSetColorImplantAction : PadImplantAction
    {
        #region Properties

        /// <summary>
        /// The name of the color to change.
        /// </summary>
        public string Color;

        /// <summary>
        /// The device this virtual address was mapped to.
        /// </summary>
        public MappedMidiDevice Device;

        #endregion

        #region IKernalTask Implementation

        /// <summary>
        /// Converts the data into a MIDI message for delivery to
        /// one or more devices.
        /// TODO: these midi messages should likely be reinserted
        /// into the kernel.
        /// </summary>
        public override void RunTask()
        {
            // send to only the mapped device
            var device = Device.Hardware as MidiXYHardwareInterface;
            if (device != null)
            {
                device.Send(X, Y, Color, Source.GetSourceName());
            }
        }

        /// <summary>
        /// TODO: may need to save some info like the current ScrollX and ScrollY 
        /// before scheduling
        /// </summary>
        public override IKernelTask ScheduleTask()
        {
            // get the physical address for this virtual address            
            int px, py;
            var hasPhysicalAddress = Source.ActiveArea.GetPhysicalAddress(X, Y, out Device, out px, out py);

            var isXYDevice = Device != null && Device.Hardware is MidiXYHardwareInterface;

            // only schedule if a physical address was available on an XY device
            if (hasPhysicalAddress && isXYDevice)
            {
                X = px;
                Y = py;
                return base.ScheduleTask();
            }
            return null;
        }

        #endregion
    }

    /// <summary>
    /// Action for scrolling the virtual grid for an implant.
    /// TODO: each virtual grid should be PER-IMPLANT! not per hardware!!!!
    /// </summary>
    public class PadScrollToImplantAction : PadImplantAction
    {
        public override void RunTask()
        {
            foreach (var device in Source.Devices.GetForImplant(Source))
            {
                if (device.Hardware is MidiXYHardwareInterface)
                {
                    (device.Hardware as MidiXYHardwareInterface).Grid.ScrollTo(X, Y);
                }
            }
        }
    }
}
