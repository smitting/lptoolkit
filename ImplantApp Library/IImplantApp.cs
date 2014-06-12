using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImplantApp
{
    public enum LPAppRequest
    {
        ShowLaunchPadSimulator
    }

    /// <summary>
    /// Contract for the main class for an application using the
    /// LPToolKit Library.  This is needed because we're going to
    /// need to build the OS X and Windows binaries separately for
    /// full UI support, but we want as much common functionality
    /// within this library as possible.
    /// </summary>
    public interface IImplantApp
    {
        /// <summary>
        /// Launches a web page in the internal browser.
        /// </summary>
        void ShowWebPage(string url, string target = "main");

        /// <summary>
        /// Reports errors to the main app so it can handle them 
        /// gracefully.
        /// </summary>
        void HandleException(Exception ex);


        /// <summary>
        /// Allows LPApplication to tell the host to do something,
        /// like open a console window or a simulator.
        /// </summary>
        void HandleAppRequest(LPAppRequest request);

    }
}
