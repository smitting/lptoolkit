using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.MIDI;
using LPToolKit.Core;

namespace LPToolKit.MIDI.Pads.Mappers
{
    /// <summary>
    /// The XY mapping for the Novation LaunchPad buttons.
    /// </summary>
    public class LaunchPadXYMapper : IButtonXYMapper
    {
        public const int FirstModeControl = 104;

        public bool ConvertXY(MidiMessageType type, int noteNumber, out int x, out int y, out ImplantEventType eventType)
        {
            switch (type)
            {
                case MidiMessageType.ControlChange:
                    y = 0;
                    x = noteNumber - FirstModeControl;
                    eventType = ImplantEventType.PadPress;
                    return true;
                case MidiMessageType.NoteOn:
                case MidiMessageType.NoteOff:
                    y = noteNumber / 16 + 1;
                    x = noteNumber % 16;
                    eventType = ImplantEventType.PadPress;
                    return true;
            }
            x = y = 0;
            eventType = ImplantEventType.PadPress;
            return false;
        }

        public bool ConvertXY(int x, int y, out MidiMessageType type, out int noteNumber)
        {
            if (y < 1)
            {
                type = MidiMessageType.ControlChange;
                noteNumber = FirstModeControl + x;
            }
            else
            {
                type = MidiMessageType.NoteOn;
                noteNumber = (y - 1) * 16 + x;
            }
            return true;
        }
    }
}
