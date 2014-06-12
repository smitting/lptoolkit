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
    /// A wrapper of a MidiMessage and the hardware interface that 
    /// it was received by.  This is scheduled in the kernel to be
    /// converted into an ImplantEvent as needed.
    /// </summary>
    /// <remarks>
    /// Events are for input from a device that will be fed into an
    /// implant.  Actions are output from an implant that will be
    /// delivered to a device.
    /// </remarks>
    public class MidiEvent : MonitoredKernelTask
    {
        #region Properties

        /// <summary>
        /// The message received.
        /// </summary>
        public MidiMessage Message;

        /// <summary>
        /// The hardware interface it was received by that will need
        /// to convert it into an action.
        /// </summary>
        public MidiHardwareInterface Hardware;

        #endregion

        #region IKernalTask Implementation

        /// <summary>
        /// Called by the scheduler when it wants this task to run.
        /// </summary>
        public override void RunTask()
        {
            if (Hardware != null)
            {
                Hardware.Trigger(Message);
            }
        }

        /// <summary>
        /// The number of milliseconds into the future to set the 
        /// contractual completion time when not explicitly set.
        /// </summary>
        public override int ExpectedLatencyMsec 
        {
            get { return _latency; }
            set { _latency = value; }
        }

        private int _latency = 100;

        #endregion
    }

}
