//#define DISABLE_WEB_TASK

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using LPToolKit.Core;
using LPToolKit.Core.Tasks;
using LPToolKit.Util;
using System.Threading;
using System.Threading.Tasks;

namespace LPToolKit.WebServer
{
    /// <summary>
    /// Provides non-blocking methods for the Midi kernel to check 
    /// if there are new connections waiting at regular intervals
    /// and to complete those requests without affecting latency
    /// of other systems.
    /// </summary>
    public class KernelWebHost : IHaveUrl
    {
        #region Constructors

        /// <summary>
        /// Constructors accepts the IP and port to listen for requests
        /// on and the instance that will process requests.
        /// </summary>
        public KernelWebHost(IWebRequestHandler handler, string listenIP, int listenPort)
        {
            ListenIP = IPAddress.Parse(listenIP);
            ListenPort = listenPort;
            Handler = handler;


            _server = new TcpListener(ListenIP, ListenPort);
            _server.Start();

            // start infinite continuation of checking the web server
#if DISABLE_WEB_TASK
#warning Web Server Task is DISABLED!!!!
#else
            _pendingTask = new PendingTask() { Parent = this };
#endif

            /*
#warning I hate this thread.
            new SingleThread()
            {
                Name = "Restart web host scheduler",
                Step = () =>
                {
                    if ((DateTime.Now - _pendingTask.LastRan).TotalSeconds > 2.5)
                    {
                        Session.UserSession.Current.Console.Add("Had to restart scheduler");
                        _pendingTask.ScheduleTask();
                    }
                    Thread.Sleep(2500);
                },
                SleepAfterStep = true
            }
            .Start();
            */
            
        }

        private PendingTask _pendingTask;

        ~KernelWebHost()
        {
            _server.Stop();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Instance that actually handles web request.
        /// </summary>
        public readonly IWebRequestHandler Handler;

        /// <summary>
        /// The address the listen for web requests on.
        /// </summary>
        public readonly IPAddress ListenIP;

        /// <summary>
        /// The port to listen to web requests on.
        /// </summary>
        public readonly int ListenPort;

        /// <summary>
        /// Returns true iff the web server has a request to process.
        /// </summary>
        public bool HasPending
        {
            get { return _server == null ? false : _server.Pending(); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// If there is a pending connection, the request is parsed an
        /// scheduled to be executed in the kernel.
        /// </summary>
        public void SchedulePending()
        {
            while (_server.Pending())
            {
                new RequestTask()
                {
                    Handler = Handler,
                    Context = LPWebContext.FromSocket(_server.AcceptSocket())
                }.ScheduleTask();
            }
        }

        /// <summary>
        /// Returns the URL to display this web site.
        /// </summary>
        public string GetUrl()
        {
            return "http://" + ListenIP + ":" + ListenPort + "/index.html";
        }

        #endregion

        #region Nested Classes

        /// <summary>
        /// A kernel task that checks if there are any web requests to
        /// be processed.  Is queued to run every 100 msec.
        /// </summary>
        public class PendingTask : RepeatingKernelTask
        {
            #region Constructor

            public PendingTask() : base()
            {
                ExpectedLatencyMsec = 500;
                MinimumRepeatTimeMsec = 100;
            }

            #endregion
            #region Properties

            public KernelWebHost Parent;

            public DateTime LastRan = DateTime.MinValue;

            #endregion

            #region IKernalTask Implementation

            public override bool ReadyToRun
            {
                get
                {
                    return base.ReadyToRun && Parent.HasPending;
                }
            }

            /// <summary>
            /// Send the response.
            /// </summary>
            public override void RunTask()
            {
                try
                {
                    Parent.SchedulePending();
                    LastRan = DateTime.Now;
                }
                catch (Exception ex)
                {
                    Session.UserSession.Current.Console.Add("Error: " + ex.Message, "KernelWebHost");
                }                
            }

            #endregion
        }

        /// <summary>
        /// A kernel task that allows web requests to be scheduled after
        /// real-time actions are completed.
        /// </summary>
        public class RequestTask : IKernelTask
        {
            #region Properties

            /// <summary>
            /// The request to be completed
            /// </summary>
            public LPWebContext Context;

            /// <summary>
            /// The handler that will run the request.
            /// </summary>
            public IWebRequestHandler Handler;

            #endregion

            #region IKernalTask Implementation

            /// <summary>
            /// Send the response.
            /// </summary>
            public virtual void RunTask()
            {
                try
                {
                    Handler.HandleRequest(Context);
                }
                catch (Exception ex)
                {
                    Session.UserSession.Current.Console.Add("WEB ERROR: " + ex.Message, "KernelWebHost.RequestTask");
                }
                finally
                {
                    Context.Finish();
                }                
            }

            /// <summary>
            /// Call this late after half a second.
            /// </summary>
            public virtual int ExpectedLatencyMsec
            {
                get { return 500; }
            }

            /// <summary>
            /// Adds this task to the scheduler.
            /// </summary>
            public virtual IKernelTask ScheduleTask()
            {
                Kernel.Current.Add(this);
                return this;
            }

            #endregion
        }

        #endregion

        #region Private

        /// <summary>
        /// The socked object listening for new requests.
        /// </summary>
        private TcpListener _server = null;

        #endregion
    }
}
