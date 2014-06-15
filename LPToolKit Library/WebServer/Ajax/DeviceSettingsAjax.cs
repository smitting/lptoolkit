using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit;
using LPToolKit.LaunchPad;
using LPToolKit.Implants;
using LPToolKit.Session;
using Newtonsoft.Json;
using LPToolKit.MIDI;

namespace LPToolKit.WebServer.Ajax
{
    internal class DeviceSettingsAjax
    {
        /// <summary>
        /// Handler for ajax requests to /settings/devices
        /// </summary>
        /// <param name="ctx"></param>
        public static void Process(LPWebContext ctx)
        {
            try
            {
                string cmd = ctx.Request["cmd"] ?? "load";
                switch (cmd.ToLower())
                {
                    case "interfaces":
                        {
                            // list of all products supported
                            ctx.Response.Write(JsonConvert.SerializeObject(MIDI.Hardware.MidiHardwareTypes.Available));
                        }
                        break;
                    case "map":
                        {
                            // change device mapping
                            var id = ctx.Request["id"];
                            var mappedAs = ctx.Request["mappedAs"];
                            UserSession.Current.Devices.Map(id, mappedAs);//.GetMidiDeviceMapping()); 
                            UserSession.Current.Save();
                        }
                        break;
                    case "enable":
                        {
                            // change enabled flag on device
                            var id = ctx.Request["id"];
                            bool enabled = false;
                            if (bool.TryParse(ctx.Request["enabled"], out enabled))
                            {
                                UserSession.Current.Devices.GetMappedDeviceById(id).Enabled = enabled;
                                UserSession.Current.Save();
                            }
                        }
                        break;
                    case "refresh":
                        {
                            UserSession.Current.Devices.Refresh();
                        }
                        break;
                    default:
                        {
                            // just return the list for all current device mappings
                            var list = new List<DeviceItemJson>();
                            foreach (var device in UserSession.Current.Devices.Available)
                            {
                                var mapping = UserSession.Current.Devices.GetMapping(device);
                                var item = new DeviceItemJson();
                                item.id = device.ID;
                                item.name = device.Name;
                                item.hasInput = device.CanRead;
                                item.hasOutput = device.CanWrite;
                                //MidiDeviceMapping mappedAs = mapping == null ? MidiDeviceMapping.None : mapping.MappedAs;                                
//                                item.mappedAs = mappedAs.GetString();
                                item.mappedAs = mapping == null || mapping.Hardware == null ? "---" : mapping.Hardware.Name;
                                item.enabled = mapping == null ? false : mapping.Enabled;
                                list.Add(item);
                            }
                            ctx.Response.Write(JsonConvert.SerializeObject(list));
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                ctx.Response.Send500(ex.ToString());
            }
        }


        #region Nested Classes

        private class DeviceItemJson
        {
            public string id;
            public string name;
            public bool hasInput;
            public bool hasOutput;
            public string mappedAs;
            public bool enabled;
        }

        #endregion
    }
}
