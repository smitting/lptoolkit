using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace LPToolKit.WebServer
{
    /// <summary>
    /// Event triggered whenever a new request is received by WebHost.
    /// </summary>
    public delegate void LPWebContextHandler(object sender, LPWebContext context);

    /// <summary>
    /// Uses sockets to open up a port to provide a web server without
    /// requiring administrator rights, as WebListener does, because it
    /// doesn't using HTTP.SYS.  While this makes the server less 
    /// efficient, the kernal level performance boost is not needed for
    /// this application.
    /// </summary>
    public class WebHost
    {
        #region Constructors

        /// <summary>
        /// Sets the IP and port that the web server will run on, optionally
        /// starting the server.
        /// </summary>
        public WebHost(string listenIP, int listenPort, bool autoStart = false)
        {
            ListenIP = IPAddress.Parse(listenIP);
            ListenPort = listenPort;
            if (autoStart)
            {
                Start();
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// The address the listen for web requests on.
        /// </summary>
        public readonly IPAddress ListenIP;

        /// <summary>
        /// The port to listen to web requests on.
        /// </summary>
        public readonly int ListenPort;

        /// <summary>
        /// True iff the server is accepting new requests.
        /// </summary>
        public bool IsListening { get; private set; }

        /// <summary>
        /// The event to fire when requests are received.
        /// </summary>
        public event LPWebContextHandler OnRequest;

        #endregion

        #region Methods

        /// <summary>
        /// Starts the web server thread accepting new connections.
        /// </summary>
        public void Start()
        {
            lock (_lock)
            {
                if (_webThread != null) return;
                _webThread = new Thread(WebServerThread);
                _server = new TcpListener(ListenIP, ListenPort);
                _server.Start();

                _webThread.Start();
            }
        }

        /// <summary>
        /// Stops the web server thread from accepting new connections.
        /// Gives existing connections 1.5 seconds to finish.
        /// </summary>
        public void Stop()
        {
            lock (_lock)
            {
                if (_webThread == null) return;


                if (_server != null)
                {
                    _server.Stop();
                    _server = null;
                }

                if (_webThread != null)
                {
                    _webThread.Join(1500);
                    _webThread.Abort();
                    _webThread = null;
                }
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// The thread listening for new requests.
        /// </summary>
        private Thread _webThread = null;

        /// <summary>
        /// Mutex for starting the server thread.
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// The socked object listening for new requests.
        /// </summary>
        private TcpListener _server = null;

        /// <summary>
        /// Thread that listens to new requests and starts off tasks
        /// to send the requests to the OnRequest event.
        /// </summary>
        private void WebServerThread()
        {
            // from http://www.codeproject.com/Articles/1505/Create-your-own-Web-Server-using-C

            try
            {
                IsListening = true;
                while (_server != null)
                {
                    var socket = _server.AcceptSocket();
                    Task.Factory.StartNew(() => 
                    {
                        var ctx = LPWebContext.FromSocket(socket);
                        try
                        {
                            if (OnRequest != null)
                            {
                                OnRequest(this, ctx);
                            }
                        }
                        finally
                        {
                            ctx.Finish();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                IsListening = false;
            }
        }

        #endregion
    }
}
