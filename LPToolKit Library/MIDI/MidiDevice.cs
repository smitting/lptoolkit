using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.MIDI
{
    public enum MidiDirection
    {
        Input,
        Output,
        IO
    }

    /// <summary>
    /// Represents one available hardware MIDI device in the system.
    /// </summary>
    public class MidiDevice
    {
        #region Properties

        /// <summary>
        /// The unique ID for this device in the current software.  Note
        /// that this can change everytime the software is restarted, so
        /// name should be used instead.
        /// 
        /// TODO: would be nice if it stayed the same
        /// </summary>
        public string ID;

        /// <summary>
        /// The name of this device provided by the driver.
        /// </summary>
        public string Name;

        /// <summary>
        /// Whether input and/or output is supported.
        /// </summary>
        public MidiDirection Direction;

        /// <summary>
        /// True iff this device provides MIDI data.
        /// </summary>
        public bool CanRead
        {
            get { return Direction == MidiDirection.Input || Direction == MidiDirection.IO; }
        }

        /// <summary>
        /// True iff this device can be sent MIDI data.
        /// </summary>
        public bool CanWrite
        {
            get { return Direction == MidiDirection.Output || Direction == MidiDirection.IO; }
        }

        #endregion
    }


    /// <summary>
    /// A virtual device to be used with the VirtualMidiDriver to 
    /// simulate a MIDI device within the system.
    /// </summary>
    public abstract class VirtualMidiDevice : MidiDevice
    {
        /// <summary>
        /// Called by the VirtualMidiDriver when data is sent to
        /// this virtual device.
        /// </summary>
        public abstract void Receive(MidiMessage msg);
    }

    /// <summary>
    /// Device that does nothing.
    /// </summary>
    public class NullMidiDevice : VirtualMidiDevice
    {
        public NullMidiDevice()
        {
            ID = "NULL";
            Name = "Null Device";
        }
        public override void Receive(MidiMessage msg)
        {
            
        }
    }
}
