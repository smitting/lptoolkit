using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.MIDI
{
    /// <summary>
    /// Types of MIDI messages supported by the library.
    /// </summary>
    public enum MidiMessageType
    {
        NoteOff,
        NoteOn,
        ControlChange,
        ClockStart,
        ClockStop,
        ClockTick,
        ClockContinue,
        Unsupported
    }

    public class MidiHelpers
    {
        public const int MIDI_TYPE_MASK = 0x00f0;
        public const int MIDI_CHANNEL_MASK = 0x000f;

        public static void ParseTypeAndChannel(byte b, out MidiMessageType type, out int channel)
        {
            switch ((int)b & MIDI_TYPE_MASK) 
            {
                case 0x80:
                    channel = (int)b % MIDI_CHANNEL_MASK;
                    type = MidiMessageType.NoteOff;
                    break;
                case 0x90:
                    channel = (int)b % MIDI_CHANNEL_MASK;
                    type = MidiMessageType.NoteOn;
                    break;
                case 0xb0:
                    channel = (int)b % MIDI_CHANNEL_MASK;
                    type = MidiMessageType.ControlChange;
                    break;
                case 0xf0:
                    switch ((int)b)
                    {
                        case 0xfa:
                            channel = 0;
                            type = MidiMessageType.ClockStart;                            
                            break;
                        case 0xfc:
                            channel = 0;
                            type = MidiMessageType.ClockStop;
                            break;
                        case 0xf8:
                            channel = 0;
                            type = MidiMessageType.ClockTick;
                            break;
                        case 0xfb:
                            channel = 0;
                            type = MidiMessageType.ClockContinue;
                            break;
                        default:
                            channel = 0;
                            type = MidiMessageType.Unsupported;
                            break;
                    }
                    break;
                default:
                    channel = 0;
                    type = MidiMessageType.Unsupported;
                    break;
            }
        }


        public static byte BindTypeAndChannel(MidiMessageType type, int channel)
        {
            switch (type)
            {
                case MidiMessageType.NoteOn:
                    return (byte)(0x90 + channel);
                case MidiMessageType.NoteOff:
                    return (byte)(0x80 + channel);
                case MidiMessageType.ControlChange:
                    return (byte)(0xB0 + (byte)channel);
                case MidiMessageType.ClockStart:
                    return 0xFA;
                case MidiMessageType.ClockStop:
                    return 0xFC;
                case MidiMessageType.ClockTick:
                    return 0xF8;
                case MidiMessageType.ClockContinue:
                    return 0xFB;
                default:
                    return 0x00;
            }
        }
    }

    /// <summary>
    /// Conversions for MIDI enums.
    /// </summary>
    static class ExtensionMethods
    {
        /// <summary>
        /// Convert a MIDI type enum to a string.
        /// </summary>
        public static string GetString(this MidiMessageType type)
        {
            switch (type)
            {
                case MidiMessageType.NoteOff:
                    return "noteoff";
                case MidiMessageType.NoteOn:
                    return "noteon";
                case MidiMessageType.ControlChange:
                    return "cc";
                default:
                    return "unknown";
            }
        }

        /// <summary>
        /// Convert a string to a MIDI type enum.
        /// </summary>
        public static MidiMessageType GetMidiMessageType(this string type)
        {
            switch (type)
            {
                case "noteon":
                    return MidiMessageType.NoteOn;
                case "noteoff":
                    return MidiMessageType.NoteOff;
                case "cc":
                    return MidiMessageType.ControlChange;
            }
            return MidiMessageType.NoteOn;
        }


        /*
        
        /// <summary>
        /// Convert a binary MIDI type the to enum.
        /// </summary>
        public static MidiMessageType GetMidiMessageType(this byte b)
        {
            //switch ((int)b & MIDI_TYPE_MASK)
            switch ((int)b)
            {
                case 0x80:
                case 0x88:
                    return MidiMessageType.NoteOff;
                case 0x90:
                case 0x98:
                    return MidiMessageType.NoteOn;
                case 0xb0:
                case 0xb8:
                    return MidiMessageType.ControlChange;
                case 0xfa:
                    return MidiMessageType.ClockStart;
                case 0xfc:
                    return MidiMessageType.ClockStop;
                case 0xf8:
                    return MidiMessageType.ClockTick;
                case 0xfb:
                    return MidiMessageType.ClockContinue;
                default:
                    return MidiMessageType.NoteOn;
            }
        }

        /// <summary>
        /// Convert a MIDI type enum to binary.
        /// </summary>
        public static byte GetBinary(this MidiMessageType type, int channel = 0)
        {
            switch (type)
            {
                case MidiMessageType.NoteOn:
                    return (byte)(0x90 + channel);
                case MidiMessageType.NoteOff:
                    return (byte)(0x80 + channel);
                case MidiMessageType.ControlChange:
                    return (byte)(0xB0 + (byte)channel);
                case MidiMessageType.ClockStart:
                    return 0xFA;
                case MidiMessageType.ClockStop:
                    return 0xFC;
                case MidiMessageType.ClockTick:
                    return 0xF8;
                case MidiMessageType.ClockContinue:
                    return 0xFB;
                default:
                    return 0x00;
            }
        }
         */
    }

}
