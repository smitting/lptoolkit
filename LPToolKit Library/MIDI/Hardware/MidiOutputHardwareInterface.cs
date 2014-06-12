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
    /// Interface attached to devices setup for MIDI output via OSC-to-MIDI mapping
    /// </summary>
    public class MidiOutputHardwareInterface : MidiHardwareInterface
    {
        public MidiOutputHardwareInterface(MappedMidiDevice device)
            : base(device)
        {

        }

        public override string Name
        {
            get { return "MIDI Output"; }
        }

        public override bool Supports(MidiDevice device)
        {
            return false;
        }

        public override ImplantEvent Convert(MidiMessage midi)
        {
            /*
            if (midi.Type == MidiMessageType.NoteOn || midi.Type == MidiMessageType.NoteOff)
            {
                return new ImplantEvent()
                {
                    EventType = midi.Type == MidiMessageType.NoteOn ? ImplantEventType.NoteOn : ImplantEventType.NoteOff,
                    X = midi.Pitch,
                    Value = midi.Velocity
                };
            }*/
            return null;
        }
    }
}
