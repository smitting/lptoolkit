using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.MIDI;
using LPM = LPToolKit.MIDI;
using MonoMac.CoreMidi;
using System.Runtime.InteropServices;

namespace LPToolKit.Platform
{
    /// <summary>
    /// Mac OS X specific implementation of MIDI support.
    /// </summary>
    public class MacOSXMidiDriver : MidiDriver
    {
        #region Constructors

        public MacOSXMidiDriver()
            : base()
        {
            _outputPort = _client.CreateOutputPort("LPToolKit Output Port");
            _inputPort = _client.CreateInputPort("LPToolKit Input Port");
        }

        ~MacOSXMidiDriver()
        {
            Release();
        }

        /// <summary>
        /// Initializes CoreMidi only once per application
        /// </summary>
        static MacOSXMidiDriver()
        {
            MonoMac.CoreMidi.Midi.Restart();
            _client = new MidiClient("LPToolKit MIDI Client");
        }


        #endregion

        #region MidiDriver Implementation

        /// <summary>
        /// Sends a MIDI message to the device.  Ignored if the 
        /// device is not set or does not support output.
        /// </summary>
        internal override void SendMessage(MidiMessage msg)
        {
            if (_outputPort == null) return;
            if (_outputEndpoint == null) return;

            // send the packet
            //_outputPort.Send(_outputEndpoint, new MidiPacket[] { new MidiPacket(0, new byte[] { msg.Type.GetBinary(msg.Channel), (byte)msg.Number, (byte)msg.Value }) });
            _outputPort.Send(_outputEndpoint, new MidiPacket[] { new MidiPacket(0, msg.CreatePacket()) });
        }

        /// <summary>
        /// Returns all connected devices.
        /// </summary>
        protected override List<LPM.MidiDevice> GetDeviceList()
        {
            var ret = new List<LPM.MidiDevice>();

            // grab all the input devices
            for (var i = 0; i < MonoMac.CoreMidi.Midi.SourceCount; i++)
            {
                MidiEndpoint src = MidiEndpoint.GetSource(i);
                var device = new LPM.MidiDevice();
                device.ID = GetDeviceID(src);
                device.Name = device.ID;
                device.Direction = MidiDirection.Input;
                ret.Add(device);
            }

            // grab all output devices, and combine devices that are
            // already in the input list
            for (var i = 0; i < MonoMac.CoreMidi.Midi.DestinationCount; i++)
            {
                MidiEndpoint dest = MidiEndpoint.GetDestination(i);
                string outputID = GetDeviceID(dest);

                // look for the same device in the inputs
                LPM.MidiDevice inDevice = null;
                foreach (var d in ret)
                {
                    if (d.ID == outputID)
                    {
                        inDevice = d;
                        break;
                    }
                }

                // mark as IO device if found in input
                if (inDevice != null)
                {
                    inDevice.Direction = MidiDirection.IO;
                }
                // otherwise create a new output-only device
                else
                {
                    var device = new LPM.MidiDevice();
                    device.ID = outputID;
                    device.Name = outputID;
                    device.Direction = MidiDirection.Output;
                    ret.Add(device);
                }
            }


            return ret;
        }

        /// <summary>
        /// Called whenever the midi device is changed so the platform
        /// specific code and acquire the appropriate resources from
        /// the operating system.
        /// </summary>
        protected override void UpdateSelectedDevice()
        {
            Release();
            Acquire(SelectedDevice);
        }

        #endregion

        #region Private


        /// <summary>
        /// CoreMidi handle for our program.
        /// </summary>
        private static MidiClient _client;

        /// <summary>
        /// CoreMidi handle for one MIDI input from our software.
        /// </summary>
        private MidiPort _inputPort;

        /// <summary>
        /// CoreMidi handle for one MIDI output from our software.
        /// </summary>
        private MidiPort _outputPort;

        /// <summary>
        /// A destination device for sending MIDI data.
        /// </summary>
        private MidiEndpoint _outputEndpoint;

        /// <summary>
        /// A source device for receiving MIDI data.
        /// </summary>
        private MidiEndpoint _inputEndpoint;

        /// <summary>
        /// Gets the unique ID to use from a device, currently the name
        /// </summary>
        private string GetDeviceID(MidiEndpoint d)
        {
            string name = d.DisplayName;
            if (name.Contains(d.Name) == false)
            {
                name = d.Name + "(" + name + ")";
            }
            return name;
        }


        /// <summary>
        /// Gets all MIDI resources needed to use the selected device.
        /// </summary>
        private void Acquire(LPM.MidiDevice device)
        {
            if (device != null)
            {
                // setup the input device
                if (device.CanRead)
                {
                    _inputEndpoint = null;
                    for (var i = 0; i < MonoMac.CoreMidi.Midi.SourceCount; i++)
                    {
                        MidiEndpoint dest = MidiEndpoint.GetSource(i);
                        if (GetDeviceID(dest) == device.ID)
                        {
                            _inputEndpoint = dest;
                            break;
                        }
                    }

                    if (_inputEndpoint == null)
                    {
                        throw new Exception("Could not find MIDI input device " + device.ID);
                    }


                    // setup the device
                    _inputPort.ConnectSource(_inputEndpoint);

                    // send all input events to Receive()
                    _inputPort.MessageReceived += (object sender, MidiPacketsEventArgs e) =>
                    {
                        for (var i = 0; i < e.Packets.Length; i++)
                        {
                            var packet = e.Packets[i];
                            var managedArray = new byte[packet.Length];
                            Marshal.Copy(packet.Bytes, managedArray, 0, packet.Length);
                            if (managedArray.Length >= 3)
                            {
                                MidiMessageType type;
                                int channel;
                                MidiHelpers.ParseTypeAndChannel(managedArray[0], out type, out channel);

                                Receive(new MidiMessage()
                                {
                                    //Type = managedArray[0].GetMidiMessageType(),
                                    //Channel = 0,
                                    Type = type,
                                    Channel = channel,
                                    Pitch = (int)managedArray[1],
                                    Velocity = (int)managedArray[2]
                                });
                            }
                        }
                    };
                }
                else
                {
                    _inputEndpoint = null;
                }

                if (device.CanWrite)
                {
                    _outputEndpoint = null;
                    for (var i = 0; i < MonoMac.CoreMidi.Midi.DestinationCount; i++)
                    {
                        MidiEndpoint dest = MidiEndpoint.GetDestination(i);
                        if (GetDeviceID(dest) == device.ID)
                        {
                            _outputEndpoint = dest;
                            break;
                        }
                    }

                    if (_outputEndpoint == null)
                    {
                        throw new Exception("Could not find MIDI output device " + device.ID);
                    }                          
                }
                else
                {
                    _outputEndpoint = null;
                }
            }
        }

        /// <summary>
        /// Releases all currently used MIDI resources.
        /// </summary>
        private void Release()
        {
            if (_inputEndpoint != null)
            {
                _inputPort.Disconnect(_inputEndpoint);
                _inputEndpoint = null;
            }
            if (_outputEndpoint != null)
            {
                _outputEndpoint = null;
            }
        }

        #endregion
        
    }
}
