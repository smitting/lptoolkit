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
    internal class LoggingAjax
    {
        private static string GetFilename(string path)
        {
            var i = path.LastIndexOf("/");
            return i > -1 ? path.Substring(i + 1) : path;
        }

        /// <summary>
        /// Handler for ajax requests to /logs/
        /// </summary>
        /// <param name="ctx"></param>
        public static void Process(LPWebContext ctx)
        {
            try
            {
                string logType = GetFilename(ctx.Request.Path);

                // get requests for data since an ordinal
                int ordinal = 0;
                int.TryParse(ctx.Request["i"], out ordinal);

                

                switch (logType)
                {
                    case "console":
                        ctx.Response.Write(JsonConvert.SerializeObject(UserSession.Current.Console.Messages.GetSinceOrdinal(ordinal)));
                        break;
                    case "midi":
                        ctx.Response.Write(JsonConvert.SerializeObject(UserSession.Current.MidiMap.Log.GetSinceOrdinal(ordinal)));
                        break;
                    case "osc":
                        ctx.Response.Write(JsonConvert.SerializeObject(UserSession.Current.OSC.Log.GetSinceOrdinal(ordinal)));
                        break;
                    case "lag":
                        /*int count = 50;
                        int.TryParse(ctx.Request["count"], out count);
                        ctx.Response.Write(JsonConvert.SerializeObject(MIDI.MidiOutputThread.Lag.GetRecent(count)));
                        break;*/
                        throw new Exception("lag screen is now obsolete.  will be replaces with full kernel debugger");
                    default:
                        throw new Exception("Unknown log type: " + logType);
                }
            }
            catch (Exception ex)
            {
                ctx.Response.Send500(ex.ToString());
            }
        }


        private class LagTimeStats
        {
            public List<int> Times = new List<int>();
            public double TotalTime = 0;
            public double Average = 0;
        }
    }
}
