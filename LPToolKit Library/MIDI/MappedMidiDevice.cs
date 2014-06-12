using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.MIDI.Hardware;

namespace LPToolKit.MIDI
{
    /*
    public enum MidiDeviceMapping
    {
        None,
        PadDevice,
        KnobDevice,
        MidiKeyboard,
        MidiOutput
    }

    static class MidiDeviceMappingExtensionMethods
    {

        /// <summary>
        /// Converts the enum to a string for json.
        /// </summary>
        public static string GetString(this MidiDeviceMapping map)
        {
            switch (map)
            {
                case MidiDeviceMapping.PadDevice: return "Pad Device";
                case MidiDeviceMapping.KnobDevice: return "Knob Device";
                case MidiDeviceMapping.MidiKeyboard: return "MIDI Keyboard";
                case MidiDeviceMapping.MidiOutput: return "MIDI Output";
            }
            return "---";
        }

        /// <summary>
        /// Converts a string from json to an enum.
        /// </summary>
        public static MidiDeviceMapping GetMidiDeviceMapping(this string map)
        {
            switch (map)
            {
                case "Pad Device": return MidiDeviceMapping.PadDevice;
                case "Knob Device": return MidiDeviceMapping.KnobDevice;
                case "MIDI Keyboard": return MidiDeviceMapping.MidiKeyboard;
                case "MIDI Output": return MidiDeviceMapping.MidiOutput;
            }
            return MidiDeviceMapping.None;
        }
    }
    */
    /// <summary>
    /// Combines input and output devices into a single concept with
    /// a common hardware ID, which is how the information is presented
    /// to the user, even if the operating system keeps them separated.
    /// 
    /// TODO: separate the mapping from the device.  I want to use
    /// MidiDevice objects to reference a particular device throughout
    /// the system.
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
        /// How this device is being used in hardware.
        /// </summary>
        //public MidiDeviceMapping MappedAs;

        /// <summary>
        /// Hardware specific mapping that converts raw MIDI to 
        /// ImplantEvent objects.
        /// </summary>
        public MidiHardwareInterface Hardware;

        /// <summary>
        /// True iff this device is currently enabled
        /// </summary>
        public bool Enabled;


        #endregion


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
    }
}
