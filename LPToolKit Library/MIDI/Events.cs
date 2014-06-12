using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.MIDI
{
    /// <summary>
    /// Event handlers that receive incoming message from a MidiDriver.
    /// </summary>
    public delegate void MidiMessageEventHandler(object sender, MidiMessageEventArgs e);

    /// <summary>
    /// Type of message for routing to implants by device type.
    /// </summary>
    public enum ImplantMessageType
    {
        /// <summary>
        /// Type has not been assigned yet.
        /// </summary>
        None,

        /// <summary>
        /// Message is from a launchpad type device.
        /// </summary>
        PadDevice,

        /// <summary>
        /// Message is from a knob or fader.
        /// </summary>
        KnobDevice,

        /// <summary>
        /// Message is from a keyboard.
        /// </summary>
        KeyDevice
    }

    /// <summary>
    /// Data send to midi message handlers.
    /// </summary>
    public class MidiMessageEventArgs : EventArgs
    {
        /// <summary>
        /// The driver sending the message.
        /// </summary>
        public MidiDriver Driver;

        /// <summary>
        /// The device the message came from.
        /// </summary>
        public MidiDevice Device;

        /// <summary>
        /// The message itself.
        /// </summary>
        public MidiMessage Message;
    }
}
