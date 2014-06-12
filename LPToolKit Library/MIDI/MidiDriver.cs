using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.Platform;

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
        /// Event generated when a MIDI message is received from the 
        /// device.
        /// </summary>
        public event MidiMessageEventHandler MidiInput;

        /*
        /// <summary>
        /// The output thread for this driver.
        /// </summary>
        internal MidiOutputThread OutputThread = new MidiOutputThread();
        */
        #endregion

        #region Methods

        /*
        /// <summary>
        /// Sends a MIDI message to the device.  Ignored if the 
        /// device is not set or does not support output.
        /// </summary>
        /// <remarks>
        /// This calls SendMessage() in the platform driver
        /// as a non-blocking call to avoid OS hangups from
        /// affecting the rest of the system.
        /// 
        /// The return value can be used to cancel scheduled
        /// messages.
        /// </remarks>
        */

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

            /*
            // send message to the output thread's work queue
            var scheduled = new ScheduledMidiMessage() { Driver = this, Message = msg };
            OutputThread.Schedule(scheduled);
            return scheduled;
            */
        }

#warning TODO: want to send and receive bytes from the platform drivers instead

        /// <summary>
        /// Called by platform classes to generate MIDI input events
        /// to all connected handlers.  Is public to allow outside
        /// classes to simulate events (use sparingly).
        /// </summary>
        public void Receive(MidiMessage msg)
        {
            // log that this is an incoming message from this device
            msg.LogAsIncoming();
            msg.LogSource(SelectedDevice == null ? "none" : SelectedDevice.Name);


#warning TODO - consider having all active devices just send an event to the kernel to map to wherever

            // send to any listeners
            if (MidiInput != null)
            {
                var e = new MidiMessageEventArgs();
                e.Driver = this;
                e.Device = SelectedDevice;
                e.Message = msg;
                MidiInput(this, e);
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

        /// <summary>
        /// Tells the output thread to ignore all new queue messages 
        /// and exist once all current messages are processed.
        /// </summary>
        public void StopAndClearQueue()
        {
           // OutputThread.PoliteStop();
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
