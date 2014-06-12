using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.MIDI;

namespace LPToolKit.Platform
{
    /// <summary>
    /// Windows specific implementation of MIDI support.
    /// </summary>
    public class WindowsMidiDriver : MidiDriver
    {
        #region Constructors

        internal WindowsMidiDriver() : base()
        {
            _callback = new Win32API.MidiInProc(this.InputCallback);
        }

        ~WindowsMidiDriver()
        {
            Release();
        }

        #endregion

        #region MidiDriver Implementation

        /// <summary>
        /// Sends a MIDI message to the device.  Ignored if the 
        /// device is not set or does not support output.
        /// </summary>
        internal override void SendMessage(MidiMessage msg)
        {
            if (_outputOpen && msg != null)
            {
                Win32API.midiOutShortMsg(_outputHandle, msg.CreateShortMessage());
            }
        }

        /// <summary>
        /// Returns all connected devices.
        /// </summary>
        protected override List<MidiDevice> GetDeviceList()
        {          
            var ret = new List<MidiDevice>();

            // get the list of input and output devices from WinMM.dll
            var inputs = GetInputList();
            var outputs = GetOutputList();
            var allDevices = inputs.Union(outputs);

            // create a MidiDevice for each device, deciding the direction
            // by which list the IDs are in.
            foreach (var device in allDevices)
            {
                MidiDirection direction;
                if (inputs.Contains(device))
                {
                    direction = outputs.Contains(device) ? MidiDirection.IO : MidiDirection.Input;
                }
                else // if (outputs.Contains(device))
                {
                    direction = MidiDirection.Output;
                }
                ret.Add(new MidiDevice()
                {
                    Direction = direction,
                    ID = device,
                    Name = device
                });
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
        /// Reference to the callback method.
        /// </summary>
        private Win32API.MidiInProc _callback;

        /// <summary>
        /// WinMM handle to the input device.
        /// </summary>
        private Win32API.HMIDIIN _inputHandle;

        /// <summary>
        /// WinMM handle to the output device.
        /// </summary>
        private Win32API.HMIDIOUT _outputHandle;

        /// <summary>
        /// Set to true when output is open.
        /// </summary>
        private bool _outputOpen = false;

        /// <summary>
        /// Set to true when input is open.
        /// </summary>
        private bool _inputOpen = false;

        /// <summary>
        /// Mutex for changing midi settings.
        /// </summary>
        private readonly object _acquireLock = new object();

        /// <summary>
        /// The list of recent device IDs by name.
        /// </summary>
        private static Dictionary<string, uint> _deviceIdByName = new Dictionary<string, uint>();

        /// <summary>
        /// Returns all the names of input devices from WinMM.dll
        /// </summary>
        private List<string> GetInputList()
        {
            var ret = new List<string>();

            uint inputDeviceCount = Win32API.midiInGetNumDevs();
            for (uint deviceId = 0; deviceId < inputDeviceCount; deviceId++)
            {
                Win32API.MIDIINCAPS caps = new Win32API.MIDIINCAPS();
                Win32API.midiInGetDevCaps((UIntPtr)deviceId, out caps);
                ret.Add(caps.szPname);

                _deviceIdByName["IN_" + caps.szPname] = deviceId;
            }

            return ret;
        }

        /// <summary>
        /// Returns all the names of output devices from WinMM.dll
        /// </summary>
        private List<string> GetOutputList()
        {
            var ret = new List<string>();

            uint outputDeviceCount = Win32API.midiOutGetNumDevs();
            for (uint deviceId = 0; deviceId < outputDeviceCount; deviceId++)
            {
                Win32API.MIDIOUTCAPS caps = new Win32API.MIDIOUTCAPS();
                Win32API.midiOutGetDevCaps((UIntPtr)deviceId, out caps);
                ret.Add(caps.szPname);

                _deviceIdByName["OUT_" + caps.szPname] = deviceId;
            }

            return ret;
        }

        /// <summary>
        /// Gets all MIDI resources needed to use the selected device.
        /// </summary>
        private void Acquire(MidiDevice device)
        {
            lock (_acquireLock)
            {
                if (device != null)
                {
                    // setup the input device
                    if (device.CanRead)
                    {
                        // grab the device id by name
                        uint deviceId = 0;
                        if (_deviceIdByName.TryGetValue("IN_" + device.ID, out deviceId) == false)
                        {
                            throw new Exception("Could not find MIDI input device " + device.ID);
                        }

                        // start streaming the input
                        WinMM.CheckReturnCode(Win32API.midiInOpen(out _inputHandle, (UIntPtr)deviceId, _callback, (UIntPtr)0));
                        WinMM.CheckReturnCode(Win32API.midiInStart(_inputHandle));
                        _inputOpen = true;
                    }
                    else
                    {
                        _inputOpen = false;
                    }

                    // setup the output device
                    if (device.CanWrite)
                    {
                        // grab the device id by name
                        uint deviceId = 0;
                        if (_deviceIdByName.TryGetValue("OUT_" + device.ID, out deviceId) == false)
                        {
                            throw new Exception("Could not find MIDI output device " + device.ID);
                        }

                        // setup the device
                        WinMM.CheckReturnCode(Win32API.midiOutOpen(out _outputHandle, (UIntPtr)deviceId, null, (UIntPtr)0));
                        _outputOpen = true;
                    }
                    else
                    {
                        _outputOpen = false;
                    }
                }
            }
        }

        /// <summary>
        /// Releases all currently used MIDI resources.
        /// </summary>
        private void Release()
        {
            lock (_acquireLock)
            {
                if (_inputOpen)
                {
                    WinMM.CheckReturnCode(Win32API.midiInStop(_inputHandle));
                    WinMM.CheckReturnCode(Win32API.midiInClose(_inputHandle));
                    _inputOpen = false;
                }

                if (_outputOpen)
                {
                    WinMM.CheckReturnCode(Win32API.midiOutReset(_outputHandle));// TODO: not sure if we should
                    WinMM.CheckReturnCode(Win32API.midiOutClose(_outputHandle));
                    _outputOpen = false;
                }
            }
        }

        /// <summary>
        /// The input callback for midiOutOpen.
        /// </summary>
        private void InputCallback(Win32API.HMIDIIN hMidiIn, Win32API.MidiInMessage wMsg,
            UIntPtr dwInstance, UIntPtr dwParam1, UIntPtr dwParam2)
        {
            if (wMsg == Win32API.MidiInMessage.MIM_DATA)
            {
                var msg = MidiMessage.FromShortMessage((uint)dwParam1);
                if (msg != null)
                {
                    Receive(msg);
                }
            }
        }

        #endregion
    }
}
