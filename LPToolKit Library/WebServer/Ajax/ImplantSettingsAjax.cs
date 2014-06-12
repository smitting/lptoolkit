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
using System.Diagnostics;

namespace LPToolKit.WebServer.Ajax
{
    internal class ImplantSettingsAjax
    {


        /// <summary>
        /// Handler for ajax requests to /settings/implant
        /// </summary>
        /// <param name="ctx"></param>
        public static void Process(LPWebContext ctx)
        {
            try
            {

                string cmd = ctx.Request["cmd"] ?? "load";
                switch (cmd.ToLower())
                {
                    case "implantlist":
                        {
                            var files = ImplantPath.GetImplantFiles();
                            ctx.Response.Write(JsonConvert.SerializeObject(files));
                        }
                        break;
                    case "devicelist":
                        {
                            // devices currently available for implant mapping
                            var deviceNames = new List<string>();
                            foreach (var mapping in UserSession.Current.Devices.Mapped)
                            {
                                // TODO: device which are appropriate to be mapped
                                if (mapping.Hardware is MIDI.Hardware.MidiXYHardwareInterface)
                                {
                                    deviceNames.Add(mapping.Device.Name);
                                }
                            }
                            ctx.Response.Write(JsonConvert.SerializeObject(deviceNames));
                        }
                        break;
                    case "save":
                        {
                            // gotta parse out a new range object either way
                            var rangeJson = ImplantItemJson.Range.FromRequest(ctx.Request);
                            var range = rangeJson.ToRangeMap();

                            // must be a new item if not found
                            var item = UserSession.Current.Implants.GetById(ctx.Request["id"]);
                            if (item == null)
                            {
                                var vpath = ctx.Request["vpath"];
                                var id = ctx.Request["id"];
                                string oscFormat = ctx.Request["oscFormat"];
                                
                                int mode = 0;
                                int.TryParse(ctx.Request["mode"], out mode);

                                var implant = UserSession.Current.Implants.Load(vpath, range, oscFormat);
                                implant.AssignedMode = mode;
                                implant.ImplantID = id;
                            }
                            else
                            {
                                var id = ctx.Request["id"];

                                /*
                                var vpath = ctx.Request["vpath"];
                                if (vpath != item.ImplantType.VPath) // TODO: point at vpath
                                {
                                    throw new Exception("Cannot current change the type of implant this way.");
                                }
                                */

                                if (ctx.Request["oscFormat"] != null)
                                {
                                    item.OscFormat = ctx.Request["oscFormat"];
                                }
                                item.ActiveArea = range;

                                if (ctx.Request["mode"] != null)
                                {
                                    item.AssignedMode = int.Parse(ctx.Request["mode"]);
                                }
                            }
                            ctx.Response.Write("Saving ID #" + ctx.Request["id"]);
                            UserSession.Current.Save();
                        }
                        break;
                    case "run":
                        UserSession.Current.Implants.Start(ctx.Request["id"]);
                        UserSession.Current.Save();
                        ctx.Response.Write("Running ID #" + ctx.Request["id"]);
                        break;
                    case "stop":
                        UserSession.Current.Implants.Stop(ctx.Request["id"]);
                        UserSession.Current.Save();
                        ctx.Response.Write("Stopping ID #" + ctx.Request["id"]);
                        break;
                    case "unload":
                        UserSession.Current.Implants.Unload(ctx.Request["id"]);
                        UserSession.Current.Save();
                        ctx.Response.Write("Unloading ID #" + ctx.Request["id"]);
                        break;
                    case "reload":
                        {
                            UserSession.Current.Implants.Reload(ctx.Request["id"]);
                            UserSession.Current.Save();
                            ctx.Response.Write("Reload ID #" + ctx.Request["id"]);
                        }
                        break;
                    case "texteditor":
                        {
                            // just for fun
                            var item = UserSession.Current.Implants.GetById(ctx.Request["id"]);
                            if (item == null) throw new Exception("Could not find ImplantID " + ctx.Request["id"]);
#warning need a new setting for the text editor to use
                            System.Diagnostics.Process.Start("notepad.exe", item.ImplantType.Path.FullPath);
                            ctx.Response.Write("Launching text editor for ID #" + ctx.Request["id"]);
                        }
                        break;
                    default:
                        {
                            // just return the list for all loaded implants
                            var list = new List<ImplantItemJson>();
                            foreach (var implant in UserSession.Current.Implants.Items)
                            {
                                list.Add(ImplantItemJson.Convert(implant));
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


        private class ImplantItemJson
        {
            public string id;
            public string vpath;
            public Range range;
            public string status;
            public string oscFormat;
            public int mode;
            public string error;

            public class Range
            {
                public int x, y;
                public int width, height;
                public int virtualX, virtualY;
                public string deviceName;
                public List<Range> children = new List<Range>();

                /// <summary>
                /// Creates a real RangeMap object from this data 
                /// using the current devices in the session.
                /// </summary>
                public MIDI.RangeMap ToRangeMap()
                {
                    var device = UserSession.Current.Devices.GetMappedDeviceByName(deviceName)
                        ?? MIDI.MappedMidiDevice.CreateNullDevice();

                    var range = new MIDI.RangeMap(device);
                    range.X = x;
                    range.Y = y;
                    range.Width = width;
                    range.Height = height;
                    range.VirtualX = virtualX;
                    range.VirtualY = virtualY;
                    foreach (var child in children)
                    {
                        range.Children.Add(child.ToRangeMap());
                    }
                    return range;
                }

                /// <summary>
                /// Grabs all of the ranges from a request and returns
                /// it as children to one range, which the first 
                /// being the parent range.
                /// </summary>
                public static Range FromRequest(LPWebRequest request)
                {
                    var ret = FromRequestString(request["range"]);
                    for (int i = 0; i < 1000; i++)
                    {
                        if (request["range_" + i] != null)
                        {
                            ret.children.Add(FromRequestString(request["range_" + i]));
                        }
                    }
                    return ret;
                }

                /// <summary>
                /// Converts a flattened range from a request in the
                /// format:
                ///     Device|x|y|vx|vy|w|h
                /// </summary>
                public static Range FromRequestString(string s)
                {
                    var parts = s.Split('|');

                    var ret = new Range();
                    ret.deviceName = parts[0];
                    if (parts.Length > 1)
                    {
                        int.TryParse(parts[1], out ret.x);
                    }
                    if (parts.Length > 2)
                    {
                        int.TryParse(parts[2], out ret.y);
                    }
                    if (parts.Length > 3)
                    {
                        int.TryParse(parts[3], out ret.virtualX);
                    }
                    if (parts.Length > 4)
                    {
                        int.TryParse(parts[4], out ret.virtualY);
                    }
                    if (parts.Length > 5)
                    {
                        int.TryParse(parts[5], out ret.width);
                    }
                    if (parts.Length > 6)
                    {
                        int.TryParse(parts[6], out ret.height);
                    }
                    return ret;
                }


                public static Range Convert(MIDI.RangeMap range)
                {
                    var ret = new Range();
                    ret.x = range.X;
                    ret.y = range.Y;
                    ret.width = range.Width;
                    ret.height = range.Height;
                    ret.deviceName = range.Device.Device.Name;
                    ret.virtualX = range.VirtualX;
                    ret.virtualY = range.VirtualY;
                    foreach (var child in range.Children)
                    {
                        ret.children.Add(Convert(child));
                    }
                    return ret;
                }
            }

            public static ImplantItemJson Convert(JavascriptImplant implant)
            {
                var ret = new ImplantItemJson();
                ret.id = implant.ImplantID;
                ret.vpath = implant.ImplantType.VPath;
                ret.oscFormat = implant.OscFormat;
                ret.status = implant.GetStatus();
                ret.range = Range.Convert(implant.ActiveArea);
                ret.mode = implant.AssignedMode;
                ret.error = implant.ServerInitException != null ? implant.ServerInitException.Message : null;
                return ret;
            }
        }

    }
}
