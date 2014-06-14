using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.Core
{
    /// <summary>
    /// Location for key app-wide settings that are not distinct to
    /// each user session.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// The root folder where sessions are stored by default.
        /// </summary>
        public static string SessionFolder = "./";

        /// <summary>
        /// The root folder for implants.
        /// </summary>
        public static string ImplantFolder = "./implants/";

        /// <summary>
        /// The root folder for the web site.
        /// </summary>
        public static string WebFolder = "./wwwroot/";
    }
}
