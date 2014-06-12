using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using LPToolKit.MIDI;

namespace LPToolKit.OSC
{


    /// <summary>
    /// Definition of one mapping from OSC to MIDI.
    /// </summary>
    public class OscToMidiMap
    {
        public OscToMidiMap()
        {
            ID = "midimap_" + (_nextId++);
        }

        private static int _nextId = 0;

        #region Properties

        /// <summary>
        /// Unique ID for this mapping.
        /// </summary>
        public string ID;

        /// <summary>
        /// The allowed OSC source.  Blank for any source.
        /// </summary>
        public string OscSource;

        /// <summary>
        /// The OSC address to map, which can include replacement
        /// values {x}.  TODO: also support {y} or {z}.
        /// </summary>
        public string OscAddress
        {
            get { return _oscAddress; }
            set 
            { 
                _oscAddress = value;
                BuildRegex();
            }
        }

        private string _oscAddress;

        /// <summary>
        /// The lowest OSC value to map.
        /// </summary>
        public double OscValueFrom;

        /// <summary>
        /// The highest OSC value to map.
        /// </summary>
        public double OscValueTo;

        /// <summary>
        /// The type of message to map to.
        /// </summary>
        public MidiMessageType MidiType;

        /// <summary>
        /// Formula for translating an OSC address to a midi note
        /// or control number, such as 25 + {x}, where {x} is a
        /// portion of the OscAddress.  Currenly only supports {x}
        /// and addition.
        /// </summary>
        public string MidiNote;

        /// <summary>
        /// The lowest number to use for value output.
        /// </summary>
        public int MidiValueFrom;

        /// <summary>
        /// The highest number to use for value output.
        /// </summary>
        public int MidiValueTo;

        /// <summary>
        /// The destination MIDI device id, or blank for all
        /// </summary>
        public string MidiDestination;

        #endregion

        #region Methods
        /*
        public static string ToString(MidiMessageType type)
        {
            switch (type)
            {
                case MidiMessageType.NoteOn:
                    return "noteon";
                case MidiMessageType.NoteOff:
                    return "noteoff";
                case MidiMessageType.ControlChange:
                    return "cc";
            }
            return "unknown";
        }
        

        public static MidiMessageType FromString(string type)
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
        */

        /// <summary>
        /// Returns true iff the supplied address matches the pattern.
        /// </summary>
        public bool IsMatch(string oscAddress)
        {
            if (_addressRegex == null)
            {
                return _oscAddress == oscAddress;
            }
            else
            {
                return _addressRegex.IsMatch(oscAddress);
            }
        }

        /// <summary>
        /// Returns the midi note for a given OSC address.
        /// </summary>
        public int GetMidiNote(string oscAddress)
        {
            int ret = 0;

            // must just be a literal value if no regex
            if (_addressRegex == null)
            {
                int.TryParse(MidiNote, out ret);
            }
            else
            {
                int x = 0;
                var match = _addressRegex.Match(oscAddress);
                if (match.Success)
                {
                    if (match.Groups.Count >= 2)
                    {
                        if (match.Groups[1].Captures.Count > 0)
                        {
                            var index = match.Groups[1].Captures[0].Index;
                            string s = "";
                            for (var i = index; i < oscAddress.Length; i++)
                            {
                                if (char.IsDigit(oscAddress[i]))
                                {
                                    s += oscAddress[i];
                                }
                                else
                                {
                                    break;
                                }
                            }
                            int.TryParse(s, out x);
                        }
                    }

                    
                }
                
                foreach (var parts in MidiNote.Split('+'))
                {
                    int i = 0;
                    if (parts.Trim() == "{x}")
                    {
                        ret += x;
                    }
                    else
                    {
                        if (int.TryParse(parts, out i))
                        {
                            ret += i;
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Maps an OSC value to a MIDI value.
        /// </summary>
        public int GetMidiValue(double oscValue)
        {
            var percent = (oscValue - OscValueFrom) / OscValueTo;
            var delta = (double)MidiValueTo - (double)MidiValueFrom;
            return (int)(MidiValueFrom + delta * percent);
        }

        #endregion

        #region Private

        /// <summary>
        /// Regular expression used to parse an OSC address with 
        /// replacements.
        /// </summary>
        private Regex _addressRegex;

        private void BuildRegex()
        {
            if (_oscAddress.Contains("{"))
            {
                _addressRegex = new Regex(_oscAddress.Replace("{x}", "(.*?)"));
            }
            else
            {
                _addressRegex = null;
            }
        }

        #endregion
    }
}
