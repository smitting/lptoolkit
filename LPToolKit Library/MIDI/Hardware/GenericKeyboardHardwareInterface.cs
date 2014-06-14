using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.Implants;
using LPToolKit.Core;
using LPToolKit.Core.Tasks.ImplantEvents;

namespace LPToolKit.MIDI.Hardware
{
    /// <summary>
    /// Sends MIDI note through to the keys objects of the implant.
    /// </summary>
    public class GenericKeyboardHardwareInterface : MidiHardwareInterface
    {
        #region Constructors

        public GenericKeyboardHardwareInterface(MappedMidiDevice device)
            : base(device)
        {
        }

        #endregion

        #region MidiHardwareInterface Implementation

        /// <summary>
        /// Hardware type name.
        /// </summary>
        public override string Name
        {
            get { return "Generic MIDI Keyboard"; }
        }

        /// <summary>
        /// The generic keyboard is never automapped.
        /// </summary>
        public override bool Supports(MidiDevice device)
        {
            return false;
        }

        /// <summary>
        /// Creates a note implant event from a midi message.
        /// </summary>
        public override ImplantEvent Convert(MidiMessage midi)
        {
            ImplantEvent ret = null;
            switch (midi.Type)
            {
                case MidiMessageType.NoteOn:
                    ret = new NoteOnImplantEvent();
                    break;
                case MidiMessageType.NoteOff:
                    ret = new NoteOffImplantEvent();
                    break;
            }
            if (ret != null)
            {
                ret.X = midi.Pitch;
                ret.Value = midi.Velocity;
            }
            return ret;
        }

        #endregion
    }
}
