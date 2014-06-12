using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.MIDI;

namespace LPToolKit.Platform
{
    /// <summary>
    /// Implementation of a virtual MIDI drivers so device simulations
    /// such as the LaunchPadSimulator can interface with the software
    /// as if it was a real MIDI device.
    /// </summary>
    public class VirtualMidiDriver : MidiDriver
    {
        #region Constructors

        internal VirtualMidiDriver() : base()
        {
        }

        ~VirtualMidiDriver()
        {
        }

        #endregion

        #region MidiDriver Implementation

        /// <summary>
        /// Sends a MIDI message to the device, passing it on the the
        /// simulated device.
        /// </summary>
        internal override void SendMessage(MidiMessage msg)
        {
            if (SelectedDevice is VirtualMidiDevice)
            {
                (SelectedDevice as VirtualMidiDevice).Receive(msg);
            }           
        }

        /// <summary>
        /// Not allowed on virtual drivers.
        /// </summary>
        protected override List<MidiDevice> GetDeviceList()
        {
            throw new NotImplementedException("You may not get a device list from the virtual driver.");
        }

        /// <summary>
        /// </summary>
        protected override void UpdateSelectedDevice()
        {
        }

        #endregion

        #region Private

        #endregion
    }
}
