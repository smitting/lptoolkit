using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.Platform;
using LPToolKit.Core.Tasks;
using LPToolKit.MIDI.Hardware;

namespace LPToolKit.MIDI
{
    /// <summary>
    /// Provides the most direct access to MIDI hardware available to
    /// the rest of the library.  This is the base class for 
    /// platform specific MIDI implementations.  Each MIDI driver
    /// can be assigned one active device for both input and output.
    /// </summary>
    public abstract class MidiDriver
    {
        #region Constructors

        /// <summary>
        /// Use CreatePlatformInstance() instead.
        /// </summary>
        protected MidiDriver()
        {
        }

        /// <summary>
        /// Creates a new instance of MidiDriver appropriate for
        /// the selected operating system.
        /// </summary>
        public static MidiDriver CreatePlatformInstance()
        {
            switch (OS.Platform)
            {
                case Platforms.Windows:
                    return new WindowsMidiDriver();
                case Platforms.MacOSX:
                    return new MacOSXMidiDriver();
            }
            throw new Exception("Unsupported platform " + OS.Platform);
        }

        #endregion

        #region Settings

        /// <summary>
        /// How many seconds to keep the device list in cache.
        /// </summary>
        public static int RefreshDeviceListSeconds = 30;

        #endregion

        #region Properties

        /// <summary>
        /// Returns all connected devices, which is cached according 
        /// to settings on this object.
        /// </summary>
        public List<MidiDevice> Devices
        {
            get 
            {
                if (_deviceList == null || _deviceListAge.TotalSeconds > RefreshDeviceListSeconds)
                {
                    _deviceList = GetDeviceList();
                }
                return _deviceList;
            }
        }

        /// <summary>
        /// The current device controlled by this driver.
        /// </summary>
        public MidiDevice SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                if (_selectedDevice != value)
                {
                    _selectedDevice = value;
                    UpdateSelectedDevice();
                }
            }
        }

        /// <summary>
        /// Hardware specific mapping that converts raw MIDI to 
        /// ImplantEvent objects.
        /// </summary>
        public MidiHardwareInterface Hardware;

        #endregion

        #region Methods

        /// <summary>
        /// Messages are now immediately sent on the current thread
        /// because only the Kernel should be calling this method.
        /// </summary>
        public void Send(MidiMessage msg)
        {
            // log that the selected midi divice its a destination
            msg.LogAsOutgoing();
            if (SelectedDevice != null)
            {
                msg.LogDestination(SelectedDevice.Name);
            }

            SendMessage(msg);
        }

        /// <summary>
        /// Adds a task to the kernel that a midi message has been 
        /// received by a specific platform driver, and has a hardware
        /// class available to convert the message to an implant event.
        /// </summary>
        /// <remarks>
        /// This is not private so that simulators can seems like a
        /// real MIDI device to the core system.  Should be used
        /// sparingly for this purpose.
        /// </remarks>
        internal void Receive(MidiMessage msg)
        {
            // log that this is an incoming message from this device
            msg.LogAsIncoming();
            msg.LogSource(SelectedDevice == null ? "none" : SelectedDevice.Name);

            if (Hardware != null)
            {
                new MidiEvent()
                {
                    Message = msg,
                    Hardware = Hardware,
                    ExpectedLatencyMsec = msg.Type == MidiMessageType.ControlChange ? 1000 : 100
                }.ScheduleTask();
            }
        }

        /// <summary>
        /// Returns the first device by name, optionally doing an
        /// exact match (default partial match).
        /// </summary>
        public MidiDevice FindDevice(string name, bool exactMatch = false)
        {
            name = name.ToLower();
            foreach (var device in Devices)
            {
                var dname = (device.Name ?? "").ToLower();
                if (exactMatch)
                {
                    if (dname == name)
                    {
                        return device;
                    }
                }
                else
                {
                    if (dname.Contains(name))
                    {
                        return device;
                    }
                }
            }
            return null;
        }

        #endregion

        #region Abstract Platform-Specific Methods 

        /// <summary>
        /// Sends a MIDI message to the device.  Ignored if the 
        /// device is not set or does not support output.
        /// </summary>
        internal abstract void SendMessage(MidiMessage msg);

        /// <summary>
        /// Returns all connected devices.
        /// </summary>
        protected abstract List<MidiDevice> GetDeviceList();

        /// <summary>
        /// Called whenever the midi device is changed so the platform
        /// specific code and acquire the appropriate resources from
        /// the operating system.
        /// </summary>
        protected abstract void UpdateSelectedDevice();

        #endregion

        #region Private

        /// <summary>
        /// The current device
        /// </summary>
        private MidiDevice _selectedDevice = null;

        /// <summary>
        /// Cached list of devices, updated every few minutes.
        /// </summary>
        private List<MidiDevice> _deviceList = null;

        /// <summary>
        /// How long since the device list was cached.
        /// </summary>
        private TimeSpan _deviceListAge
        {
            get { return DateTime.Now - _deviceListLoadedAt; }
        }

        /// <summary>
        /// When the device list was cached.
        /// </summary>
        private DateTime _deviceListLoadedAt = DateTime.MinValue;

        #endregion
    }
}
