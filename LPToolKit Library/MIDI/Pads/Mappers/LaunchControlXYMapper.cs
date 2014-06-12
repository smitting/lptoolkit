using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.Core;

namespace LPToolKit.MIDI.Pads.Mappers
{

    /// <summary>
    /// XY mapping for the pads and knobs of the Novation LaunchControl.
    /// </summary>
    public class LaunchControlXYMapper : IButtonXYMapper
    {
        public const int PAD1 = 9;
        public const int PAD5 = 25;
        public const int KNOB_TOPROW = 21;
        public const int KNOB_BOTTOMROW = 41;

        public bool ConvertXY(MidiMessageType type, int noteNumber, out int x, out int y, out ImplantEventType eventType)
        {
            if (type == MidiMessageType.ControlChange)
            {
                if (noteNumber < KNOB_BOTTOMROW)
                {
                    x = noteNumber - KNOB_TOPROW;
                    y = 0;
                }
                else
                {
                    x = noteNumber - KNOB_BOTTOMROW;
                    y = 1;
                }
                eventType = ImplantEventType.KnobChange;
                return true;
            }
            else
            {
                x = noteNumber < PAD5 ? noteNumber - PAD1 : noteNumber - PAD5 + 4;
                y = 2;
                eventType = ImplantEventType.PadPress;
                return true;

            }
        }

        public bool ConvertXY(int x, int y, out MidiMessageType type, out int noteNumber)
        {

            if (y == 2) // PAD
            {
                type = MidiMessageType.NoteOn; // what about note off?
                noteNumber = x < 4 ? PAD1 + x : PAD5 + (x - 4);
                return true;
            }
            else if (y == 0) // KNOB TOP
            {
                type = MidiMessageType.ControlChange;
                noteNumber = KNOB_TOPROW + x;
            }
            else if (y == 1) // KNOB BOTTOM
            {
                type = MidiMessageType.ControlChange;
                noteNumber = KNOB_BOTTOMROW + x;
            }

            type = MidiMessageType.NoteOn;
            noteNumber = 0;
            return false;
        }
    }

}
