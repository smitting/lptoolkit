using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LPToolKit.WebServer;
using LPToolKit.OSC;
using LPToolKit.LaunchPad;
using LPToolKit.Implants;
using LPToolKit.LaunchPad.UI;
using LPToolKit.Session;
using LPToolKit.MIDI;
using LPToolKit.GUI;
using LPToolKit.Core;
using LPToolKit.Core.Tasks.ImplantEvents;

namespace ImplantApp
{
    /// <summary>
    /// Controls the main functionality for the cross-platform app
    /// making using of the LPToolKit.  This object runs the web 
    /// server, maintains the connected hardware, loads and executes
    /// the implants, and provides all network connectivity.  Messages
    /// that cannot be handled in a cross-platform library are sent
    /// to the specific platform's application via the IImplantApp
    /// interface.
    /// </summary>
    public class LPApplication
    {
        #region Constructors

        /// <summary>
        /// Initiates the application on the behalf of the host and 
        /// optionally starts running.
        /// </summary>
        public LPApplication(IImplantApp host, bool autoStart = true)
        {
            Running = false;



            // TODO: this should be configurable
            var localIp = LPToolKit.Util.NetUtil.GetLocalIP();

            Settings = new ProgramSettings();
            // temp hack to avoid null exceptions
            Settings.OscRemoteIP = Settings.OscRemoteIP ?? localIp;
            ImplantPath.ImplantRoot = Settings.ImplantFolder;



            Host = host;
            //WebSiteUrlProvider
            //WebServer = new TabletHost(new SettingsWebRequestHandler(Settings.WebFolder), localIp, Settings.WebPort);
            WebServer = null;
            WebSiteUrlProvider = new KernelWebHost(new SettingsWebRequestHandler(Settings.WebFolder), localIp, Settings.WebPort);

            // TODO: this should be configured elswhere
            OSCSettings.SourceIP = Settings.OscLocalIP ?? localIp;
            OSCSettings.SourcePort = Settings.OscPort == 0 ? 8000 : Settings.OscPort;
            if (Settings.OscRemoteIP != null)
            {
                UserSession.Current.OSC.Connections.Add(new OSCConnection(Settings.OscRemoteIP, Settings.OscPort));
            }

            // TODO: always load last saved session, not just the default filename
            UserSession.SessionFolder = Settings.SessionFolder;
            UserSession.Current.Reload();


            // notify host of device changes
            UserSession.Current.Implants.ImplantEventTriggered += (sender, e) =>
                {
                    if (e is DeviceChangeImplantEvent)
                    {
                        var mapping = (e as DeviceChangeImplantEvent).Mapping;
                        if (mapping.Device is LaunchPadSimulator && mapping.Hardware != null)
                        {
                            Host.HandleAppRequest(LPAppRequest.ShowLaunchPadSimulator);
                        }

                    }

                };


            // write some stuff to the console
            try
            {
                UserSession.Current.Console.Add(string.Format("Web server listening on {0}:{1}", localIp, Settings.WebPort), "system");
                UserSession.Current.Console.Add(string.Format("Web files are in {0}", Settings.WebFolder), "system");
                UserSession.Current.Console.Add(string.Format("Implants are in {0}", Settings.ImplantFolder), "system");
            }
            catch
            {

            }


            // auto start if specified
            if (autoStart)
            {
                Start();
            }
        }

        #endregion

        #region Properties
        
        /// <summary>
        /// The platform-specific implementation hosting this application.
        /// </summary>
        public readonly IImplantApp Host;

        /// <summary>
        /// The settings used by this application, stored in app.config
        /// </summary>
        public readonly ProgramSettings Settings;

        /// <summary>
        /// The web server displaying the implant interface.
        /// </summary>
        public readonly TabletHost WebServer;

        public readonly IHaveUrl WebSiteUrlProvider;

        /// <summary>
        /// True iff the app is started and not stopped.
        /// </summary>
        public bool Running { get; private set; }


        #endregion

        #region Methods

        /// <summary>
        /// Starts the UI web server and loaded implants
        /// </summary>
        public void Start()
        {
            if (Running) return;
            Running = true;

            // start up the web server
            if (WebServer != null)
            {
                WebServer.Start();
            }

            Host.ShowWebPage(WebSiteUrlProvider.GetUrl());            
            
            // notify all implants of a mode change 
            UserSession.Current.Modes.UpdateCurrentMode();
        }

        /// <summary>
        /// Shuts down the UI web server and all active threads.
        /// </summary>
        public void Stop()
        {
            if (!Running) return;

            // shut down the web server
            if (WebServer != null)
            {
                WebServer.Stop();
            }

            Running = false;

            // kill threads
            LPToolKit.Session.UserSession.Shutdown();
            
        }

        #endregion

        #region Private



        #endregion
    }
}
