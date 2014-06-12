using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.WebServer.Ajax;

namespace LPToolKit.WebServer
{
    /// <summary>
    /// Object that handles web requests for the ajax UI for Symbiont.
    /// </summary>
    public class SettingsWebRequestHandler : IWebRequestHandler
    {
        public SettingsWebRequestHandler(string webroot)
        {
            WebRoot = webroot;
        }

        /// <summary>
        /// The root folder containing html files.
        /// </summary>
        public string WebRoot { get; set; }

        /// <summary>
        /// Reads the request URL and calls the appropriate method
        /// to send a response.
        /// </summary>
        public void HandleRequest(LPWebContext context)
        {

            // TODO: add route for requests:
            //
            // /settings/midimap
            //  if data posted, update the data
            //  always write out the latest data as a response
            //
            // /settings/implants
            //  if data posted, updated the data
            //  always write out all current status of implants
            //
            // /settings/hardware
            //   for which midi devices are active, and the names of those available

            if (context.Request.Path.StartsWith("/settings/"))
            {
                if (context.Request.Path.StartsWith("/settings/implants"))
                {
                    ImplantSettingsAjax.Process(context);
                }
                else if (context.Request.Path.StartsWith("/settings/devices"))
                {
                    DeviceSettingsAjax.Process(context);
                }
                else if (context.Request.Path.StartsWith("/settings/midimap"))
                {
                    MappingSettingsAjax.Process(context);
                }
                else if (context.Request.Path.StartsWith("/settings/gui"))
                {
                    GuiSettingsAjax.Process(context);
                }
                else
                {
                    context.Response.Send500("Invalid settings ajax request: " + context.Request.RawUrl);
                }
                return;
            }
            else if (context.Request.Path.StartsWith("/logs/"))
            {
                LoggingAjax.Process(context);
                return;
            }

            // static file
            var path = new Util.FilePath()
            {
                BaseFolder = WebRoot,
                Filename = "~" + context.Request.Filename,
                Source = "WebServer"
            };
            context.Response.SendFile(path);
        }
    }

}
