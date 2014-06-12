using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.Sync;
using LPToolKit.Util;
using LPToolKit.MIDI;
using LPToolKit.Session.Managers;
using LPToolKit.GUI;
using LPToolKit.OSC;

namespace LPToolKit.Session
{
    /// <summary>
    /// Manages writing a session to and from a json file.
    /// </summary>
    public class SessionFile
    {
        #region Methods

        public static void Load(UserSession session, FilePath filename)
        {
            Deserialize(session, FileIO.LoadJsonFile<SessionFileFormat>(filename));
        }

        public static void Save(UserSession session, FilePath filename)
        {
            FileIO.SaveJsonFile(filename, Serialize(session));
        }

        #endregion


        #region Private

        /// <summary>
        /// Converts the current session settings into a JSON ready
        /// format that can be saved.
        /// </summary>
        private static SessionFileFormat Serialize(UserSession session)
        {
            var ret = new SessionFileFormat();
            foreach (var implant in session.Implants.Items)
            {
                var item = new SessionFileFormat.ImplantFormat();
                item.vpath = implant.ImplantType.VPath;
                item.oscFormat = implant.OscFormat;
                item.activeArea = SessionFileFormat.ImplantFormat.RangeMap.Convert(implant.ActiveArea);
                item.status = implant.GetStatus();
                item.doubleValues = implant.ImplantInstance.session.DoubleValues;
                item.stringValues = implant.ImplantInstance.session.StringValues;
                item.assignedMode = implant.AssignedMode;
                ret.implants.Add(item);
            }

            foreach (var device in session.Devices.Mapped)
            {
                var item = new SessionFileFormat.DeviceFormat();
                item.deviceName = device.Device.Name;
                //item.mappedAs = device.MappedAs.GetString();
                item.mappedAs = device.Hardware == null ? "---" : device.Hardware.Name;
                item.enabled = device.Enabled;
                ret.devices.Add(item);
            }

            foreach (var mapping in session.MidiMap.Mappings)
            {
                var item = new SessionFileFormat.MidiMapFormat();
                item.id = mapping.ID;
                item.oscSource = mapping.OscSource;
                item.oscAddress = mapping.OscAddress;
                item.oscValueFrom = mapping.OscValueFrom;
                item.oscValueTo = mapping.OscValueTo;
                item.midiDestination = mapping.MidiDestination;
                item.midiType = mapping.MidiType.GetString();
                item.midiNote = mapping.MidiNote;
                item.midiValueFrom = mapping.MidiValueFrom;
                item.midiValueTo = mapping.MidiValueTo;
                ret.midiMap.Add(item);
            }

            return ret;
        }

        /// <summary>
        /// Overwrites the current session settings from a parsed 
        /// JSON object.
        /// </summary>
        private static void Deserialize(UserSession session, SessionFileFormat file)
        {
            // devices must loaded first for implant loaded to work
            session.Devices.ClearAll();
            foreach (var item in file.devices)
            {
                try
                {
                    session.Devices.MapByName(item.deviceName, item.mappedAs);
                    var mapping = session.Devices.GetMappedDeviceByName(item.deviceName);
                    mapping.Enabled = item.enabled;
                }
                catch
                {

                }
            }
            session.Devices.AutoMap();

            // load the implants
            session.Implants.ClearAll();
            foreach (var item in file.implants)
            {
                RangeMap range;
                if (item.activeArea == null)
                {
                    range = new RangeMap(MIDI.MappedMidiDevice.CreateNullDevice());
                }
                else
                {
                    range = item.activeArea.ToRangeMap();
                }
                if (item.status != "not loaded")
                {
                    var implant = session.Implants.Load(item.vpath, range, item.oscFormat);

                    // important to load session values first so the init doesn't screw it up
                    if (item.doubleValues != null)
                    {
                        implant.ImplantInstance.session.DoubleValues.Clear();
                        foreach (var key in item.doubleValues.Keys)
                        {
                            implant.ImplantInstance.session.DoubleValues.Add(key, item.doubleValues[key]);
                        }
                    }
                    if (item.stringValues != null)
                    {
                        implant.ImplantInstance.session.StringValues.Clear();
                        foreach (var key in item.stringValues.Keys)
                        {
                            implant.ImplantInstance.session.StringValues.Add(key, item.stringValues[key]);
                        }
                    }


                    implant.AssignedMode = item.assignedMode;
                    if (item.status == "running")
                    {
                        implant.ServerInit();
                    }
                }
            }
            session.MidiMap.Mappings.Clear();
            foreach (var item in file.midiMap)
            {
                var m = new OscToMidiMap();
                m.ID = item.id;
                m.OscSource = item.oscSource;
                m.OscAddress = item.oscAddress;
                m.OscValueFrom = item.oscValueFrom;
                m.OscValueTo = item.oscValueTo;
                m.MidiDestination = item.midiDestination;
                m.MidiType = item.midiType.GetMidiMessageType(); //OscToMidiMap.FromString(item.midiType);
                m.MidiNote = item.midiNote;
                m.MidiValueFrom = item.midiValueFrom;
                m.MidiValueTo = item.midiValueTo;
                session.MidiMap.Mappings.Add(m);
            }
        }


        /// <summary>
        /// JSON format of the user session file when persisted to disk.
        /// </summary>
        private class SessionFileFormat
        {
            public List<ImplantFormat> implants = new List<ImplantFormat>();

            public class ImplantFormat
            {
                public string vpath;
                public RangeMap activeArea;
                public string oscFormat;
                public string status;
                public int assignedMode;
                public Dictionary<string, double> doubleValues;
                public Dictionary<string, string> stringValues;

                public class RangeMap
                {
                    public int x, y;
                    public int width, height;
                    public int virtualX, virtualY;
                    public string deviceName;
                    public List<RangeMap> children = new List<RangeMap>();

                    public static RangeMap Convert(MIDI.RangeMap item)
                    {
                        var activeArea = new SessionFileFormat.ImplantFormat.RangeMap();
                        activeArea.x = item.X;
                        activeArea.y = item.Y;
                        activeArea.width = item.Width;
                        activeArea.height = item.Height;
                        activeArea.virtualX = item.VirtualX;
                        activeArea.virtualY = item.VirtualY;
                        activeArea.deviceName = item.Device.Device.Name;
                        foreach (var child in item.Children)
                        {
                            activeArea.children.Add(Convert(child));
                        }
                        return activeArea;
                    }

                    public MIDI.RangeMap ToRangeMap()
                    {
                        var device = UserSession.Current.Devices.GetMappedDeviceByName(deviceName);
                        if (device == null)
                        {
                            device = MIDI.MappedMidiDevice.CreateNullDevice();
                        }

                        var ret = new MIDI.RangeMap(device);
                        ret.X = this.x;
                        ret.Y = this.y;
                        ret.Width = this.width;
                        ret.Height = this.height;
                        ret.VirtualX = this.virtualX;
                        ret.VirtualY = this.virtualY;
                        foreach (var child in this.children)
                        {
                            ret.Children.Add(child.ToRangeMap());
                        }
                        return ret;
                    }
                }
            }

            public List<DeviceFormat> devices = new List<DeviceFormat>();

            public class DeviceFormat
            {
                public string deviceName;
                public string mappedAs;
                public bool enabled;
            }

            public List<MidiMapFormat> midiMap = new List<MidiMapFormat>();

            public class MidiMapFormat
            {

                public string id;
                public string oscAddress;
                public double oscValueFrom, oscValueTo;
                public string oscSource;
                public string midiType;
                public string midiNote;
                public int midiValueFrom, midiValueTo;
                public string midiDestination;
            }
        }

        #endregion
    }
}
