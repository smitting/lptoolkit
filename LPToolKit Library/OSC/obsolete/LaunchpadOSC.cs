using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bespoke.Common.Osc;
using System.Net;
using System.Net.Sockets;
using LPToolKit.Util;

namespace LPToolKit.OSC
{   
    /// <summary>
    /// Manages communication of the Launchpad OSC API invented by
    /// this program.  Primary used for synchronization with
    /// Reaktor.
    /// 
    /// 
    // TODO: implement an implant that handles all of this
    /// </summary>
    [Obsolete]
    public class LaunchpadOSC
    {
        #region Constructors

        /// <summary>
        /// Constructor sets up end points for sending and receiving
        /// UDP communications with a remote Reaktor session and 
        /// starts listening for OSC messages.
        /// </summary>
        public LaunchpadOSC(string listenIP, string remoteIP)
        {
            ListenEndPoint = NetUtil.CreateIPEndPoint(listenIP, ListenPort);
            TargetEndPoint = NetUtil.CreateIPEndPoint(remoteIP, Port);

            OscPacket.UdpClient = new UdpClient(SourcePort);
            Server = new OscServer(TransportType.Udp, ListenEndPoint.Address, ListenEndPoint.Port);
            Server.FilterRegisteredMethods = false;
            Server.MessageReceived += OnMessageReceived;
            Server.Start();
        }
        

        /// <summary>
        /// Stops listening for messages.
        /// </summary>
        ~LaunchpadOSC()
        {
            if (Server != null)
            {
                if (Server.IsRunning)
                {
                    Server.Stop();
                }
                Server = null;
            }
        }

        #endregion

        #region Settings

        /// <summary>
        /// UDP port messages will come from
        /// </summary>
        public static int SourcePort = 8000;

        /// <summary>
        /// UDP port to send messages to.
        /// </summary>
        public static int Port = 8000;

        /// <summary>
        /// UDP port to received messages on.
        /// </summary>
        public static int ListenPort = 8000;

        #endregion

        #region Properties
        
        /// <summary>
        /// Address for listening for OSC messages.
        /// </summary>
        public readonly IPEndPoint ListenEndPoint;

        /// <summary>
        /// Address to send OSC messages to.
        /// </summary>
        public readonly IPEndPoint TargetEndPoint;

        /// <summary>
        /// Server listening for OSC messages.
        /// </summary>
        public OscServer Server { get; private set; }

        /// <summary>
        /// Triggered when update data is received.
        /// /launchpad/update x,y,value
        /// </summary>
        public event LaunchPadUpdateEventHandler Update;

        /// <summary>
        /// Triggered for reset requests.
        /// /launchpad/reset
        /// </summary>
        public event LaunchPadMessageEventHandler Reset;

        /// <summary>
        /// Triggered for step highlight requests.
        /// /launchpad/step num
        /// </summary>
        public event LaunchPadStepEventHandler Step;

        /// <summary>
        /// Triggered when some other message is received.
        /// </summary>
        public event LaunchPadMessageEventHandler UnknownMessageReceived;

        /// <summary>
        /// Triggered when an invalid packet is received.
        /// </summary>
        public event LaunchPadMessageError InvalidMessage;

        #endregion

        #region Methods

        /// <summary>
        /// Sends an OSC message to the remote host.
        /// </summary>
        public void Send(string message, params float[] args)
        {
            var msg = new OscMessage(SourceEndPoint, message);
            foreach (var f in args)
            {
                msg.Append(f);
            }
            msg.Send(TargetEndPoint);
        }

        /// <summary>
        /// Sends OSC message /launchpad/update
        /// </summary>
        public void SendUpdate(int x, int y, int value)
        {
            Send("/launchpad/update", (float)x, (float)y, (float)value);
        }

        /// <summary>
        /// Sends OSC message /launchpad/ready
        /// </summary>
        public void SendReady()
        {
            Send("/launchpad/ready", 1);
        }

        /// <summary>
        /// Sends OSC message /launchpad/mode
        /// </summary>
        /// <param name="mode"></param>
        public void SendMode(int mode)
        {
            Send("/launchpad/mode", (float)mode);
        }

        #endregion

        #region Private

        /// <summary>
        /// Routes received messages to their appropriate handlers.
        /// </summary>
        private void OnMessageReceived(object sender, OscMessageReceivedEventArgs e)
        {
            try
            {
                switch (e.Message.Address.ToLower())
                {
                    case "/launchpad/update":
                        if (Update != null)
                        {
                            Update(this, new LaunchPadUpdateEventArgs(e.Message));
                        }
                        break;
                    case "/launchpad/reset":
                        if (Reset != null)
                        {
                            Reset(this, new LaunchPadMessageEventArgs(e.Message));
                        }
                        break;
                    case "/launchpad/step":
                        if (Step != null)
                        {
                            Step(this, new LaunchPadStepEventArgs(e.Message));
                        }
                        break;
                    default:
                        if (UnknownMessageReceived != null)
                        {
                            UnknownMessageReceived(this, new LaunchPadMessageEventArgs(e.Message));
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                if (InvalidMessage != null)
                {
                    InvalidMessage(ex);
                }
            }
        }

        /// <summary>
        /// Where outgoing UDP messages will come from.
        /// </summary>
        protected static IPEndPoint SourceEndPoint = new IPEndPoint(IPAddress.Loopback, SourcePort);


        #endregion
    }
}
