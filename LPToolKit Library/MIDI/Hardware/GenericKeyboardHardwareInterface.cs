using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.Implants;
using LPToolKit.Core;

namespace LPToolKit.MIDI.Hardware
{

    /// <summary>
    /// Sends MIDI note through to the keys objects of the implant.
    /// </summary>
    public class GenericKeyboardHardwareInterface : MidiHardwareInterface
    {
        public GenericKeyboardHardwareInterface(MappedMidiDevice device)
            : base(device)
        {

        }

        public override string Name
        {
            get { return "Generic MIDI Keyboard"; }
        }

        public override bool Supports(MidiDevice device)
        {
            return false;
        }

        public override ImplantEvent Convert(MidiMessage midi)
        {
            if (midi.Type == MidiMessageType.NoteOn || midi.Type == MidiMessageType.NoteOff)
            {
                return new ImplantEvent()
                {
                    EventType = midi.Type == MidiMessageType.NoteOn ? ImplantEventType.NoteOn : ImplantEventType.NoteOff,
                    X = midi.Pitch,
                    Value = midi.Velocity
                };
            }
            return null;
        }
    }
}
