using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.MIDI
{
#warning TODO these should be removed after rewriting the launchpad simulator

    /// <summary>
    /// Event handlers that receive incoming message from a MidiDriver.
    /// </summary>
    public delegate void MidiMessageEventHandler(object sender, MidiMessageEventArgs e);

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
