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
    /// Settings for where OSC messages should come from
    /// </summary>
    public class OSCSettings
    {
        /// <summary>
        /// IP OSC messages will come from.
        /// </summary>
        public static string SourceIP
        {
            get
            {
                if (_ip == null)
                {
                    throw new Exception("Set OSCSettings.SourceIP first!");
                }
                return _ip;
            }
            set
            {
                _ip = value;
                if (_ip != null && _port != 0)
                {
                    _sourceEndPoint = NetUtil.CreateIPEndPoint(_ip, _port);
                }
            }
        }

        /// <summary>
        /// Port OSC messages will come from.
        /// </summary>
        public static int SourcePort
        {
            get
            {
                if (_port == 0)
                {
                    throw new Exception("Set OSCSettings.SourcePort first!");
                }
                return _port;
            }
            set
            {
                _port = value;
                OscPacket.UdpClient = new UdpClient(_port);
                if (_ip != null && _port != 0)
                {
                    _sourceEndPoint = NetUtil.CreateIPEndPoint(_ip, _port);
                }
            }
        }

        public static IPEndPoint SourceEndPoint
        {
            get
            {
                return _sourceEndPoint;
            }
        }

        private static string _ip = null;
        private static int _port = 0;
        private static IPEndPoint _sourceEndPoint;
    }

}
