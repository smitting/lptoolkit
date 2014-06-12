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
using LPToolKit.MIDI.Hardware;
using LPToolKit.OSC;

namespace LPToolKit.WebServer.Ajax
{
    internal class MappingSettingsAjax
    {
        /// <summary>
        /// Handler for ajax requests to /settings/midimap
        /// </summary>
        /// <param name="ctx"></param>
        public static void Process(LPWebContext ctx)
        {
            try
            {
                string cmd = ctx.Request["cmd"] ?? "load";
                switch (cmd.ToLower())
                {
                    case "outputdevices": // list of MIDI devices we could output to
                        {
                            var list = new List<string>();
                            list.Add("All Midi");
                            foreach (var d in UserSession.Current.Devices[typeof(MidiOutputHardwareInterface)])//[MidiDeviceMapping.MidiOutput])
                            {
                                list.Add(d.Device.Name);
                            }
                            ctx.Response.Write(JsonConvert.SerializeObject(list));
                        }
                        break;
                    case "implants": // list of implants we could use as a source
                        {
                            var list = new List<string>();
                            list.Add("Any Implant");
                            foreach (var i in UserSession.Current.Implants.Running)
                            {
                                list.Add(i.GetSourceName());
                            }
                            ctx.Response.Write(JsonConvert.SerializeObject(list));
                        }
                        break;
                    case "save":
                        {
                            var item = UserSession.Current.MidiMap.GetById(ctx.Request["id"]);

                            // must be a new item if not found
                            if (item == null)
                            {
                                item = new OscToMidiMap();
                                item.ID = ctx.Request["id"];
                                item.OscAddress = ctx.Request["oscAddress"];
                                item.OscValueFrom = double.Parse(ctx.Request["oscValueFrom"]);
                                item.OscValueTo = double.Parse(ctx.Request["oscValueTo"]);
                                item.OscSource = ctx.Request["oscSource"];
                                item.MidiType = ctx.Request["midiType"].GetMidiMessageType(); //OscToMidiMap.FromString(ctx.Request["midiType"]);
                                item.MidiNote = ctx.Request["midiNote"];
                                item.MidiValueFrom = int.Parse(ctx.Request["midiValueFrom"]);
                                item.MidiValueTo = int.Parse(ctx.Request["midiValueTo"]);
                                item.MidiDestination = ctx.Request["midiDestination"];
                                if (item.OscSource.ToLower() == "any implant")
                                {
                                    item.OscSource = null;
                                }
                                if (item.MidiDestination.ToLower() == "all midi")
                                {
                                    item.MidiDestination = null;
                                }
                                UserSession.Current.MidiMap.Mappings.Add(item);
                            }
                            else
                            {
                                item.OscAddress = ctx.Request["oscAddress"];
                                item.OscValueFrom = double.Parse(ctx.Request["oscValueFrom"]);
                                item.OscValueTo = double.Parse(ctx.Request["oscValueTo"]);
                                item.OscSource = ctx.Request["oscSource"];
                                item.MidiType = ctx.Request["midiType"].GetMidiMessageType();
                                item.MidiNote = ctx.Request["midiNote"];
                                item.MidiValueFrom = int.Parse(ctx.Request["midiValueFrom"]);
                                item.MidiValueTo = int.Parse(ctx.Request["midiValueTo"]);
                                item.MidiDestination = ctx.Request["midiDestination"];
                                if (item.OscSource.ToLower() == "any implant")
                                {
                                    item.OscSource = null;
                                }
                                if (item.MidiDestination.ToLower() == "all midi")
                                {
                                    item.MidiDestination = null;
                                }
                            }
                            ctx.Response.Write("Saving ID #" + ctx.Request["id"]);
                            UserSession.Current.Save();
                        }
                        break;
                    case "remove":
                        {
                            UserSession.Current.MidiMap.DeleteById(ctx.Request["id"]);
                            UserSession.Current.Save();
                            ctx.Response.Write("Deleting ID #" + ctx.Request["id"]);
                        }
                        break;
                    default:
                        {
                            // just return the list for all current device mappings
                            var list = new List<MappingItemJson>();
                            foreach (var mapping in UserSession.Current.MidiMap.Mappings)
                            {
                                var item = new MappingItemJson();
                                item.id = mapping.ID;
                                item.oscSource = mapping.OscSource ?? "Any Implant";
                                item.oscAddress = mapping.OscAddress;
                                item.oscValueFrom = mapping.OscValueFrom;
                                item.oscValueTo = mapping.OscValueTo;
                                item.midiDestination = mapping.MidiDestination ?? "All Midi";
                                item.midiType = mapping.MidiType.GetString(); 
                                item.midiNote = mapping.MidiNote;
                                item.midiValueFrom = mapping.MidiValueFrom;
                                item.midiValueTo = mapping.MidiValueTo;
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

        private class MappingItemJson
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

        #endregion
    }
}
