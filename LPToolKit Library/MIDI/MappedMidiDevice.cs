using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.MIDI.Hardware;

namespace LPToolKit.MIDI
{
    /// <summary>
    /// Combines input and output devices into a single concept with
    /// a common hardware ID, which is how the information is presented
    /// to the user, even if the operating system keeps them separated.
    /// </summary>
    public class MappedMidiDevice
    {
        #region Properties

        /// <summary>
        /// Object connected directly to the OS resources providing
        /// MIDI inputs and outputs.
        /// </summary>
        public MidiDriver Driver;

        /// <summary>
        /// The device that was mapped.
        /// </summary>
        public MidiDevice Device;

        /// <summary>
        /// Hardware specific mapping that converts raw MIDI to 
        /// ImplantEvent objects.
        /// </summary>
        public MidiHardwareInterface Hardware
        {
            get { return Driver == null ? null : Driver.Hardware; }
            set
            {
                Util.Assert.NotNull("MappedMidiDevice.Driver", Driver);
                Driver.Hardware = value;
            }
        }

        /// <summary>
        /// True iff this device is currently enabled
        /// </summary>
        public bool Enabled;

        #endregion

        #region Methods

        /// <summary>
        /// Returns an instance of this device that doesn't do anything.
        /// </summary>
        public static MappedMidiDevice CreateNullDevice()
        {
            return new MappedMidiDevice()
            {
                Device = new NullMidiDevice(),
                Driver = new Platform.VirtualMidiDriver()
            };
        }

        #endregion
    }
}
