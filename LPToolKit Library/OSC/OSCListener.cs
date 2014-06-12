using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Bespoke.Common.Osc;
using LPToolKit.Util;

namespace LPToolKit.OSC
{
    /// <summary>
    /// Event sent when there was an error in an event handler.
    /// </summary>
    public delegate void OscExceptionHandler(object sender, Exception ex);

    /// <summary>
    /// Event triggers for simple OSC messages.
    /// </summary>
    public delegate void OscMessageEventHandler(object sender, OscMessageEventArgs e);

    /// <summary>
    /// Base class for simple OSC messages that just consist of a 
    /// message and zero or more float values
    /// </summary>
    public class OscMessageEventArgs : EventArgs
    {
        public OscMessageEventArgs(OscMessage msg)
        {
            Full = msg;
            Message = msg.Address;
            FloatValues = new float[msg.Data.Count];
            for (var i = 0; i < msg.Data.Count; i++)
            {
                if (msg.Data[i] is float)
                {
                    FloatValues[i] = msg.At<float>(i);
                }
                else
                {
                    FloatValues[i] = 0f;
                }
            }
        }

        /// <summary>
        /// The full packet.
        /// </summary>
        public readonly OscMessage Full;

        /// <summary>
        /// The OSC address of the packet received.
        /// </summary>
        public string Message;

        /// <summary>
        /// Raw float argument data.  Any values that are not of a
        /// float type are set to zero.
        /// </summary>
        public float[] FloatValues;
    }


    /// <summary>
    /// Object that listens for OSC messages to come via UDP packets
    /// on a local IP at a specific port.
    /// </summary>
    public class OSCListener
    {
        #region Constructor

        /// <summary>
        /// Constructor accepts ip and port to listen for messages on.
        /// </summary>
        public OSCListener(int port)
        {
            ListenEndPoint = NetUtil.CreateIPEndPoint(OSCSettings.SourceIP, port);
            Server = new OscServer(TransportType.Udp, ListenEndPoint.Address, ListenEndPoint.Port);
            Server.FilterRegisteredMethods = false;
            Server.MessageReceived += OnMessageReceived;
            Server.Start();
        }

        /// <summary>
        /// Stops listening for messages.
        /// </summary>
        ~OSCListener()
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

        #region Properties

        /// <summary>
        /// Address for listening for OSC messages.
        /// </summary>
        public readonly IPEndPoint ListenEndPoint;

        /// <summary>
        /// Server listening for OSC messages.
        /// </summary>
        public OscServer Server { get; private set; }

        /// <summary>
        /// Event triggered when any message is received, unfiltered.
        /// </summary>
        public OscMessageEventHandler Received;

        /// <summary>
        /// Triggered whenever there is an exception in a message handler.
        /// </summary>
        public OscExceptionHandler MessageError;

        #endregion

        #region Methods

        /// <summary>
        /// Adds an event handler for a specific address.
        /// </summary>
        public void AddListener(string address, OscMessageEventHandler e)
        {
            lock (_filteredHandlers)
            {
                List<OscMessageEventHandler> handlers;
                if (_filteredHandlers.TryGetValue(address.ToLower(), out handlers) == false)
                {
                    handlers = new List<OscMessageEventHandler>();
                    _filteredHandlers.Add(address.ToLower(), handlers);
                }
                handlers.Add(e);
            }            
        }

        /// <summary>
        /// Removes an event handler for a specific address.
        /// </summary>
        public void RemoveListener(string address,  OscMessageEventHandler e)
        {
            lock (_filteredHandlers)
            {
                List<OscMessageEventHandler> handlers;
                if (_filteredHandlers.TryGetValue(address.ToLower(), out handlers))
                {
                    handlers.Remove(e);
                }
            }    
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
                // create event to send to handlers
                var args = new OscMessageEventArgs(e.Message);

                // send to any message specific handlers
                lock (_filteredHandlers)
                {
                    List<OscMessageEventHandler> handlers;
                    if (_filteredHandlers.TryGetValue(e.Message.Address.ToLower(), out handlers))
                    {
                        foreach (var handler in handlers)
                        {
                            handler(this, args);
                        }
                    }
                }

                // send to the generic handler
                if (Received != null)
                {
                    Received(this, args);
                }
            }
            catch (Exception ex)
            {
                // trigger exception event
                if (MessageError != null)
                {
                    MessageError(this, ex);
                }
            }
        }

        /// <summary>
        /// Mapping for filtered event handlers.
        /// </summary>
        private readonly Dictionary<string, List<OscMessageEventHandler>> _filteredHandlers = new Dictionary<string, List<OscMessageEventHandler>>();

        #endregion

    }
}
