using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.Util;

namespace ImplantApp
{
    /// <summary>
    /// Handles loading and saving the app.config files.
    /// </summary>
    public class ProgramSettings : AppSettingsManager<ProgramSettings>
    {
        [AppSettings("OscPort", Default = "8000")]
        public int OscPort { get; set; }

        [AppSettings("OscRemoteIP")]
        public string OscRemoteIP { get; set; }

        [AppSettings("OscLocalIP")]
        public string OscLocalIP { get; set; }

        /// <summary>
        /// The port to run the webserver on.
        /// </summary>
        [AppSettings("WebPort", Default = "3333")]
        public int WebPort { get; set; }

        /// <summary>
        /// The folder that implants are located in.
        /// </summary>
        [AppSettings("ImplantFolder", Default="./implants/")]
        public string ImplantFolder { get; set; }


        /// <summary>
        /// The folder that implants are located in.
        /// </summary>
        [AppSettings("WebFolder", Default = ".\\wwwroot\\")]
        public string WebFolder { get; set; }


        /// <summary>
        /// </summary>
        [AppSettings("SessionFolder", Default = "./")]
        public string SessionFolder { get; set; }

        /// <summary>
        /// Copies these settings to the internal variables in LPToolKit.
        /// </summary>
        public void Apply()
        {
            LPToolKit.Core.Settings.ImplantFolder = ImplantFolder;
            LPToolKit.Core.Settings.SessionFolder = SessionFolder;
            LPToolKit.Core.Settings.WebFolder = WebFolder;
        }
    }
}
