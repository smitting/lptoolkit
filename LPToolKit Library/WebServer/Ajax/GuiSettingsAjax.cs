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
using System.Drawing;
using System.IO;

namespace LPToolKit.WebServer.Ajax
{
    internal class GuiSettingsAjax
    {
        /// <summary>
        /// Handler for ajax requests to /settings/gui
        /// </summary>
        /// <param name="ctx"></param>
        public static void Process(LPWebContext ctx)
        {
            try
            {
                string cmd = ctx.Request["cmd"] ?? "load";
                int x = 0, y = 0;
                int.TryParse(ctx.Request["x"], out x);
                int.TryParse(ctx.Request["y"], out y);
                // TODO: add key commands

                switch (cmd.ToLower())
                {
                    case "up":
                        UserSession.Current.Gui.Context.MouseUp(x, y);
                        break;
                    case "down":
                        UserSession.Current.Gui.Context.MouseDown(x, y, System.Windows.Forms.Keys.None);
                        break;
                    case "move":
                        UserSession.Current.Gui.Context.MouseMove(x, y);
                        break;
                    default:
                        ctx.Response.Write(UserSession.Current.Gui.Context.GetLastRenderAsBase64());
                        break;
                }                
            }
            catch (Exception ex)
            {
                ctx.Response.Send500(ex.ToString());
            }
        }

    }
}
