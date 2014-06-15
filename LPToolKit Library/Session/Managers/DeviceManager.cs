using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.LaunchPad;
using LPToolKit.MIDI;
using LPToolKit.Implants;
using LPToolKit.MIDI.Hardware;
using LPToolKit.Core;
using LPToolKit.Core.Tasks.ImplantEvents;

namespace LPToolKit.Session.Managers
{
    /// <summary>
    /// Manages the MIDI devices for this user's session.
    /// </summary>
    public class DeviceManager : SessionManagerBase
    {
        #region Constructors

        public DeviceManager(UserSession parent) : base(parent)
        {
            _lookup = MidiDriver.CreatePlatformInstance();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Currenty list of all available devices.
        /// </summary>
        public List<MidiDevice> Available
        {
            get 
            { 
                var ret = _lookup.Devices;
                ret.Add(_simulator);
                return ret;
            }
        }

        /// <summary>
        /// All devices currently in use by the software.
        /// </summary>
        public readonly List<MappedMidiDevice> Mapped = new List<MappedMidiDevice>();

        /// <summary>
        /// Index operator returns all devices whose hardware interface
        /// is of a certain type.
        /// </summary>
        public List<MappedMidiDevice> this[Type hardwareType]
        {
            get { return Mapped.Where(d => d.Enabled && d.Hardware != null && d.Hardware.GetType() == hardwareType).ToList(); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Marks all devices as unmapped.
        /// </summary>
        public void ClearAll()
        {
            Mapped.Clear();
        }

        /// <summary>
        /// Returns the mapping object for a given device.
        /// </summary>
        public MappedMidiDevice GetMapping(MidiDevice device)
        {
            return GetMappedDeviceById(device.ID);
        }

        /// <summary>
        /// Returns an object by id from the mapped list.
        /// </summary>
        public MappedMidiDevice GetMappedDeviceById(string deviceId)
        {
            lock (Mapped)
            {
                return Mapped.Where(d => d.Device.ID == deviceId).FirstOrDefault();
            }
        }

        /// <summary>
        /// Returns an object by name from the mapped list.
        /// </summary>
        public MappedMidiDevice GetMappedDeviceByName(string deviceName)
        {
            lock (Mapped)
            {
                return Mapped.Where(d => d.Device.Name == deviceName).FirstOrDefault();
            }
        }

        /// <summary>
        /// Returns an object by id from the available list.
        /// </summary>
        public MidiDevice GetDeviceById(string deviceId)
        {
            return Available.Where(d => d.ID == deviceId).FirstOrDefault();
        }

        /// <summary>
        /// Returns an object by name from the available list.
        /// </summary>
        public MidiDevice GetDeviceByName(string deviceName)
        {
            return Available.Where(d => d.Name == deviceName).FirstOrDefault();
        }

        /// <summary>
        /// Maps a device by id to a new settings.
        /// </summary>
        public void Map(string deviceId, string hardwareInterface) // MidiDeviceMapping map)
        {
            var device = GetDeviceById(deviceId);
            if (device == null)
            {
                throw new Exception("Device not found " + deviceId);
            }
            Map(device, hardwareInterface);
        }

        /// <summary>
        /// Maps a device by its name.
        /// </summary>
        public void MapByName(string deviceName, string hardwareInterface) // MidiDeviceMapping map)
        {
            var device = GetDeviceByName(deviceName);
            if (device == null)
            {
                throw new Exception("Device not found " + deviceName);
            }
            Map(device, hardwareInterface);
        }


        /// <summary>
        /// Runs automap on any device not already mapped.
        /// </summary>
        public void AutoMap()
        {
            foreach (var device in Available)
            {
                var mapping = GetMappedDeviceById(device.ID);
                if (mapping == null)
                {
                    var automap = MidiHardwareTypes.AutoMap(device);
                    if (automap != null)
                    {
                        Map(device, automap);
                    }
                }
            }
        }


        /// <summary>
        /// Maps a device to a new usage.
        /// </summary>
        public void Map(MidiDevice device, string hardwareInterface) // MidiDeviceMapping map)
        {
            lock (Mapped)
            {
                bool changed = false;
                MappedMidiDevice mapping = null;

                // replace the "empty" marker with null
                if (hardwareInterface == "---")
                {
                    hardwareInterface = null;
                }

                try
                {
                    // either find an existing mapping to change or create a new one
                    mapping = GetMapping(device);
                    if (mapping == null)
                    {
                        mapping = new MappedMidiDevice()
                        {
                            Device = device
                        };
                        Mapped.Add(mapping);
                    }

                    // update mapping 
                    if (hardwareInterface != null)
                    {
                        if (mapping.Hardware == null || mapping.Hardware.Name != hardwareInterface)
                        {
                            changed = true;
                        }
                    }
                    else
                    {
                        changed = mapping.Hardware != null;
                    }

                    // SIMULATOR TEMP HACK
                    // TODO: fully integrate this into the midi driver pipeline
                    if (device is LaunchPadSimulator)
                    {
                        // assign virtual driver
                        if (mapping.Driver == null)
                        {
                            mapping.Driver = new LPToolKit.Platform.VirtualMidiDriver();
                            mapping.Driver.SelectedDevice = device;
                        }
                        return;
                    }
                    // END SIMULATOR TEMP HACK

                    // ensure appropriate driver is setup
                    bool needsDriver = hardwareInterface != null; //map != MidiDeviceMapping.None;
                    if (needsDriver)
                    {
                        // create and setup the driver
                        if (mapping.Driver == null)
                        {
                            mapping.Driver = MidiDriver.CreatePlatformInstance();
                        }
                        mapping.Driver.SelectedDevice = mapping.Device;
                    }
                    else
                    {
                        // destroy any used driver if we're not using the device
                        if (mapping.Driver != null)
                        {
                            mapping.Driver.SelectedDevice = null;
                        }
                        mapping.Hardware.Clear();
                        mapping.Hardware = null;
                        mapping.Driver = null;
                        Mapped.Remove(mapping);
                    }

                    // setting hardware specific settings
                    if (hardwareInterface != null)
                    {
                        mapping.Hardware = MidiHardwareTypes.CreateInstance(hardwareInterface, mapping);
                    }
                    else
                    {
                        mapping.Hardware = null;
                    }

                    // destroy any other drivers mapped to this device
                    foreach (var other in Mapped.ToArray())
                    {
                        if (other == mapping) continue;
                        if (other.Device == mapping.Device)
                        {
                            // shut down duplicate device
                            if (mapping.Driver != null)
                            {
                                mapping.Driver.SelectedDevice = null;
                            }
                            mapping.Hardware.Clear();
                            mapping.Hardware = null;
                            mapping.Driver = null;
                            Mapped.Remove(other);
                        }
                    }
                }
                finally
                {
                    // warn about device change 
                    new DeviceChangeImplantEvent()
                    {
                        Mapping = mapping
                    }.ScheduleTask();
                }
            }
        }

        /// <summary>
        /// Scans for any new hardware devices.
        /// </summary>
        public void Refresh()
        {
            // TODO: force driver to rescan hardware immediately
            //FillMidiDevices();
        }

        /// <summary>
        /// Returns all of the devices for a given implant.
        /// </summary>
        public List<MappedMidiDevice> GetForImplant(Implant source)
        {
            // TODO only send to pad devices assigned to this implant
            return Mapped.ToList();
        }

        #endregion

        #region Private

        /// <summary>
        /// Driver instance dedicated to querying the hardware.
        /// </summary>
        private MidiDriver _lookup;

        /// <summary>
        /// Simulated launchpad object is always available
        /// </summary>
        private LaunchPadSimulator _simulator = new LaunchPadSimulator();

        #endregion
    }
}
