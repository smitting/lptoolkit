using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.Logs;
using Newtonsoft.Json;

namespace LPToolKit.MIDI
{
    /// <summary>
    /// The format of all message sent to and received from a MIDI
    /// driver.  
    /// </summary>
    /// <remarks>
    /// Using a single object type allows for simplified routing to 
    /// and from implants.  A generic library for MIDI would likely 
    /// want to split these messages up at a higher level.
    /// </remarks>
    public class MidiMessage
    {
        #region Properties

        /// <summary>
        /// The type of message.
        /// </summary>
        public MidiMessageType Type;

        /// <summary>
        /// The MIDI channel.
        /// TODO: use this more often
        /// </summary>
        public int Channel = 0;

        /// <summary>
        /// The pitch on NoteOn/off and the control number on CC.
        /// </summary>
        public int Number;

        /// <summary>
        /// The velocity on NoteOn/NoteOff and the new value on CC.
        /// </summary>
        public int Value;

        /// <summary>
        /// Alias for number for natural usage by NoteOn/NoteOff.
        /// </summary>
        public int Pitch
        {
            get { return Number; }
            set { Number = value; }
        }

        /// <summary>
        /// Aliase for value for natural usage by NoteOn/NoteOff.
        /// </summary>
        public int Velocity
        {
            get { return Value; }
            set { Value = value; }
        }

        /// <summary>
        /// Alias for number for natural usage by ControlChange.
        /// </summary>
        public int Control
        {
            get { return Number; }
            set { Number = value; }
        }

        /// <summary>
        /// The log entry for this message.
        /// </summary>        
        [JsonIgnore]
        public MidiLog Log { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Marks this message as an incoming MIDI message in the logs.
        /// </summary>
        public void LogAsIncoming()
        {
            CreateLogAsNeeded();
            if (Log != null)
            {
                Log.Incoming = true;
            }
        }

        /// <summary>
        /// Marks this messaage as an outgoing MIDI message in the logs.
        /// </summary>
        public void LogAsOutgoing()
        {
            CreateLogAsNeeded();
            if (Log != null)
            {
                Log.Incoming = false;
            }
        }

        /// <summary>
        /// Changes the source in the log
        /// </summary>
        public void LogSource(string source)
        {
            CreateLogAsNeeded();
            if (Log != null)
            {
                Log.Source = source;
            }
        }

        /// <summary>
        /// Adds a destination in the log
        /// </summary>
        public void LogDestination(string destination)
        {
            CreateLogAsNeeded();
            if (Log != null)
            {
                if (Log.Destination == null)
                {
                    Log.Destination = destination;
                }
                else
                {
                    Log.Destination += "," + destination;
                }
            }
        }

        /// <summary>
        /// Converts this MIDI message to an arry of bytes to be 
        /// delivered to a low-level MIDI driver.
        /// </summary>
        public byte[] CreatePacket()
        {
            return new byte[] { MidiHelpers.BindTypeAndChannel(Type, Channel), (byte)Number, (byte)Value };
        }

        /// <summary>
        /// Converts a packet into the DWORD expected by the WinMM api.
        /// </summary>
        public UInt32 CreateShortMessage()
        {
            byte main = MidiHelpers.BindTypeAndChannel(Type, Channel);//, (byte)Number, (byte)Value
            return (UInt32)((int)main | (Number << 8) | (Value << 16));
        }

        /// <summary>
        /// Converts a DWORD from the WinMM api to a MidiMessage or
        /// null if not supported.
        /// </summary>
        public static MidiMessage FromShortMessage(uint dwParam1)
        {
            var ret = new MidiMessage();
            ret.Channel = (int)(dwParam1 & 0x0F);
            ret.Number = (int)((dwParam1 & 0xFF00) >> 8);
            ret.Value = (int)((dwParam1 & 0xFF0000) >> 16);
            switch ((int)(dwParam1 & 0x00F0))
            {
                case 0x90:
                    ret.Type = MidiMessageType.NoteOn;
                    break;
                case 0x80:
                    ret.Type = MidiMessageType.NoteOff;
                    break;
                case 0xB0:
                    ret.Type = MidiMessageType.ControlChange;
                    break;
                case 0xF0:
                    switch((int)(dwParam1 & 0x00FF))
                    {
                        case 0xFA:
                            ret.Type = MidiMessageType.ClockStart;
                            ret.Channel = 0;
                            break;
                        case 0xFC:
                            ret.Type = MidiMessageType.ClockStop;
                            ret.Channel = 0;
                            break;
                        case 0xF8:
                            ret.Type = MidiMessageType.ClockTick;
                            ret.Channel = 0;
                            break;
                        case 0xFB:
                            ret.Type = MidiMessageType.ClockContinue;
                            ret.Channel = 0;
                            break;
                        default:
                            return null;
                    }
                    break;
                default:
                    return null;
            }
            return ret;
        }

        #endregion

        #region Private

        private void CreateLogAsNeeded()
        {
            if (Log == null)
            {
                var midiMap = LPToolKit.Session.UserSession.Current.MidiMap;
                if (midiMap.LoggingEnabled)
                {
                    Log = new MidiLog() { Message = this };
                    midiMap.Log.Add(Log);
                }
            }
        }

        #endregion
    }

}
