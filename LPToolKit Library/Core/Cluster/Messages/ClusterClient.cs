using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace LPToolKit.Core.Cluster.Messages
{
    /// <summary>
    /// Delegate for event triggered when a message is received.
    /// </summary>
    public delegate void ClusterMessageHandler(ClusterMessage message);

    public abstract class ClusterClient
    {

        #region Properties


        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static readonly ClusterClient Instance = new NullClusterClient();// = new UdpClusterClient();

        /// <summary>
        /// Event triggered when a message is received.
        /// </summary>
        public event ClusterMessageHandler Received;

        #endregion

        #region Methods
        
        /// <summary>
        /// Broadcasts a message from this node that it would like to
        /// participate in a cluster.
        /// </summary>
        public abstract void Send(ClusterMessage message);

        /// <summary>
        /// Sends to all known cluster nodes specifically without using
        /// a broadcast.
        /// </summary>
        public abstract void SendAll(ClusterMessage message);

        /// <summary>
        /// Sends the host message directly to an IP, in case it can't
        /// seem to see the broadcasts.
        /// </summary>
        public abstract void Send(string ip, ClusterMessage message);
        

        #endregion

        #region Protected

        protected void OnReceived(ClusterMessage message)
        {
            if (Received != null)
            {
                Received(message);
            }
        }

        #endregion
    }

    /// <summary>
    /// A cluster client that does nothing until we can make an 
    /// implementation that doesn't screw up the rest of the program.
    /// </summary>
    public class NullClusterClient : ClusterClient
    {
        public override void Send(ClusterMessage message)
        {

        }

        public override void SendAll(ClusterMessage message)
        {

        }

        public override void Send(string ip, ClusterMessage message)
        {

        }
    }


    /// <summary>
    /// Manages transmission of UDP messages between cluster nodes.
    /// </summary>
    public class UdpClusterClient : ClusterClient
    {
        #region Constructors

        /// <summary>
        /// Use singleton instead of constructor.  Sets up the UDP client
        /// on the default port.
        /// </summary>
        internal UdpClusterClient()
        {
            OpenUdp();
        }

        /// <summary>
        /// Closes any open UDP resources.
        /// </summary>
        ~UdpClusterClient()
        {
            CloseUdp();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The port used for UDP advertisement messages.
        /// </summary>
        public int BroadcastPort
        {
            get { lock(_udpLock) return _port; }
            set 
            {
                lock (_udpLock)
                {
                    _port = value;
                    CloseUdp();
                    OpenUdp();
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Broadcasts a message from this node that it would like to
        /// participate in a cluster.
        /// </summary>
        public override void Send(ClusterMessage message)
        {
            lock (_udpLock)
            {
                var packet = message.ToPacket();
                _client.Send(packet, packet.Length, _broadcast);
            }
        }

        /// <summary>
        /// Sends to all known cluster nodes specifically without using
        /// a broadcast.
        /// </summary>
        public override void SendAll(ClusterMessage message)
        {
            lock (_udpLock)
            {
                var packet = message.ToPacket();
                foreach (var node in ClusterNetwork.Current.Nodes)
                {
                    _client.Send(packet, packet.Length, Util.NetUtil.CreateIPEndPoint(node.IP, _port));
                }
            }
        }

        /// <summary>
        /// Sends the host message directly to an IP, in case it can't
        /// seem to see the broadcasts.
        /// </summary>
        public override void Send(string ip, ClusterMessage message)
        {
            lock (_udpLock)
            {
                var packet = message.ToPacket();
                try
                {
                    _client.Send(packet, packet.Length, Util.NetUtil.CreateIPEndPoint(ip, _port));
                }
                catch (Exception ex)
                {
                    if (ex != null)
                    {

                    }
                }
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// Creates all objects needs to send and receive UDP on the 
        /// specified port.
        /// </summary>
        private void OpenUdp()
        {
            // setup resources
            lock (_udpLock)
            {
                _client = new UdpClient(_port);
                _broadcast = new IPEndPoint(IPAddress.Broadcast, _port);
                _any = new IPEndPoint(IPAddress.Any, _port);
            }

            // resource the listening process
            StartListening();
        }

        /// <summary>
        /// Closes any existing UDP objects if they exist.
        /// </summary>
        private void CloseUdp()
        {
            lock (_udpLock)
            {
                if (_client != null)
                {
                    _client.Close();
                    _client = null;
                }
            }
        }

        /// <summary>
        /// Non-blocking call to receive the next broadcast UDP packet.
        /// </summary>
        private void StartListening()
        {
            lock (_udpLock)
            {
                _client.BeginReceive(Receive, _client);
            }
        }

        /// <summary>
        /// Receives results from the non-blocking call within 
        /// StartListening() to raise the Recieved event.  Will keep
        /// calling StartListening() until the UdpClient is different
        /// than the one that called it.
        /// </summary>
        private void Receive(IAsyncResult ar)
        {
            IPEndPoint ip;
            UdpClient client = null;            

            try
            {
                // grab resources now as they might change while blocked
                lock (_udpLock)
                {
                    client = ar.AsyncState as UdpClient;
                    ip = _any;
                }

                // finish listening for broadcast
                byte[] packet = null;
                if (client != null)
                {
                    packet = client.EndReceive(ar, ref ip);
                }

                // send events
                if (packet != null)
                {
                    var message = ClusterMessage.Parse(packet);
                    if (message != null)
                    {
                        OnReceived(message);
                    }
                }
            }
            finally
            {
                // make sure we keep listening if the port hasn't changed
                bool stillOnSamePort;
                lock (_udpLock)
                {
                    stillOnSamePort = client == _client;
                }
                if (stillOnSamePort)
                {
                    StartListening();
                }
            }
        }

        /// <summary>
        /// The client for sending and receiving UDP messages.
        /// </summary>
        private UdpClient _client = null;

        /// <summary>
        /// The IP for sending broadcasts on the current port.
        /// </summary>
        private IPEndPoint _broadcast = null;

        /// <summary>
        /// The IP for receiving broadcasts on the current port.
        /// </summary>
        private IPEndPoint _any = null;

        /// <summary>
        /// The current port.
        /// </summary>
        private int _port = 15000;

        /// <summary>
        /// Mutex for fealing with UDP resources.
        /// </summary>
        private readonly object _udpLock = new object();

        #endregion
    }
}
