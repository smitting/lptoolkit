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
        #region Constructors

        /// <summary>
        /// Sets any defaults that are platform specific.
        /// </summary>
        static Settings()
        {
            if (Platform.OS.Platform == Platform.Platforms.Windows)
            {
                Terminal = "cmd.exe";
                TextEditor = "/C notepad.exe";
            }
            else if (Platform.OS.Platform == Platform.Platforms.MacOSX)
            {
                Terminal = "/usr/bin/open";
                TextEditor = "-a TextEdit";
                //TextEditor = "/Applications/TextEdit.app/Contents/MacOS/TextEdit";
            }
        }

        #endregion

        #region Properties

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

        /// <summary>
        /// The text editor to use for editing source files.
        /// </summary>
        public static string TextEditor;

        /// <summary>
        /// Specifies the file name to use in Process.Start to run 
        /// something on the command line.
        /// </summary>
        public static string Terminal;

        #endregion
    }
}
