using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using LPToolKit.Session;
using LPToolKit.LaunchPad;
using LPToolKit.Implants;
using LPToolKit.WebServer.Ajax;

namespace LPToolKit.WebServer
{
    public interface IHaveUrl
    {
        string GetUrl();
    }

    /// <summary>
    /// iPad/Android tablet integration via a web server.  A static 
    /// HTML page provides the user interface, and the current data
    /// is send via AJAX requests.
    /// 
    /// TODO: this was recently converted from HttpListener to the custom WebHost.
    /// Cleanup is needed.
    /// </summary>
    /// <remarks>
    /// This is pretty pointless with the guts torn out, and is being
    /// replaced a with something that routes the requests through
    /// the kernel so web requests never slow down audio data.
    /// </remarks>
    public class TabletHost : IHaveUrl
    {
        #region Constructors

        /// <summary>
        /// Constructor sets up, but does not start, the web server
        /// for interacting with a tablet device.  
        /// </summary>
        /// <remarks>
        /// If a port is not provided, the DefaultPort is used 
        /// instead.
        /// </remarks>
        public TabletHost(IWebRequestHandler handler, string localIP, int port = -1)
        {
            Handler = handler;

            Listener = new WebHost(localIP, port == -1 ? DefaultPort : port);
            Listener.OnRequest += (sender, e) => { Handler.HandleRequest(e); };
        }


        /// <summary>
        /// Closes the web server if still open.
        /// </summary>
        ~TabletHost()
        {
            Stop();
            //Listener.Close();
            Listener = null;
        }

        #endregion

        #region Settings

        /// <summary>
        /// The port to listen on if not specified.
        /// </summary>
        public static int DefaultPort = 3333;

        #endregion

        #region Properties

        /// <summary>
        /// Object handling web requests.
        /// </summary>
        public readonly IWebRequestHandler Handler;

        public string IP { get { return Listener.ListenIP.ToString(); } }

        /// <summary>
        /// The port listening to web requests.
        /// </summary>
        public int Port { get { return Listener.ListenPort; } }

        /// <summary>
        /// Object receiving HTTP requests.
        /// </summary>
        public WebHost Listener { get; private set; }

        /// <summary>
        /// True iff we're currently listening for web requests.
        /// </summary>
        public bool Running 
        {
            get { return Listener.IsListening; }        
        }


        #endregion

        #region Methods

        /// <summary>
        /// Launches the web page for this server on a local web browser.
        /// </summary>
        [Obsolete]
        public void OpenInBrowser()
        {
            Process.Start(GetUrl());
        }

        /// <summary>
        /// Returns the URL to display this web site.
        /// </summary>
        public string GetUrl()
        {
            return "http://" + IP + ":" + Port + "/index.html";
        }

        /// <summary>
        /// Starts listening for requests.  Call is ignored if already
        /// running.  Throws an exception if we can't start.
        /// </summary>
        public void Start()
        {
            if (Listener == null) return;
            if (Running) return;

            Listener.Start();          
        }

        /// <summary>
        /// Stops listening for requests.  Call is ignored if not running.
        /// </summary>
        public void Stop()
        {
            if (Listener == null) return;
            if (!Running) return;
            Listener.Stop();
        }

        #endregion
    }
}
