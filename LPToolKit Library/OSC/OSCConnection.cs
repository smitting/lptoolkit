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
    /// Sends OSC messages to a remote IP.
    /// </summary>
    public class OSCConnection
    {
        #region Constructors

        /// <summary>
        /// Constructor takes destination to send to.
        /// </summary>
        public OSCConnection(string remoteIP, int remotePort)
        {
            Target = NetUtil.CreateIPEndPoint(remoteIP, remotePort);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Where messages are going.
        /// </summary>
        public readonly IPEndPoint Target;

        #endregion

        #region Methods

        /// <summary>
        /// Sends an OSC message to the remote host.
        /// </summary>
        public void Send(string message, params float[] args)
        {
            var msg = new OscMessage(OSCSettings.SourceEndPoint, message);
            foreach (var f in args)
            {
                msg.Append(f);
            }
            msg.Send(Target);
        }

        #endregion

        #region Private

        #endregion
    }
}
