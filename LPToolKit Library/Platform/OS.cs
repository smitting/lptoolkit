using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.Platform
{
    public enum Platforms
    {
        Windows,
        MacOSX
    }

    /// <summary>
    /// Provides which operating system is currently active.
    /// </summary>
    public class OS
    {
        static OS()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32Windows: // windows 95/98 support?
                case PlatformID.Win32S: // windows 3.11 with 32s support?
                    Platform = Platforms.Windows;
                    break;
                case PlatformID.WinCE:
                    throw new Exception("Windows CE is not supported.");
                case PlatformID.Xbox:
                    throw new Exception("XBOX is not supported.");
                case PlatformID.Unix:
                case PlatformID.MacOSX: // NOTE: can't always tell the difference between Unix and MacOSX
                    Platform = Platforms.MacOSX;
                    break;
                //throw new Exception("Unix/Linux is not supported." + Environment.MachineName);
                default:
                    throw new Exception("Unknown platform: " + Environment.OSVersion.ToString());
            }
        }

        /// <summary>
        /// The current OS the program is running on.
        /// </summary>
        public static Platforms Platform;
        
    }
}
