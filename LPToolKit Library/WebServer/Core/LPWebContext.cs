using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;

namespace LPToolKit.WebServer
{
    /// <summary>
    /// Equivalent of HttpContext for LPToolKit.
    /// </summary>
    public class LPWebContext
    {
        #region Constructors

        /// <summary>
        /// The FromSocket() method should be used instead.
        /// </summary>
        private LPWebContext()
        {
        }

        /// <summary>
        /// Closes out the request if still active.
        /// </summary>
        ~LPWebContext()
        {
            Finish();
        }

        #endregion

        /// <summary>
        /// Contains all of the information sent from the browser.
        /// </summary>
        public LPWebRequest Request;

        /// <summary>
        /// Use to send back a response to the browser.
        /// </summary>
        public LPWebResponse Response;

        /// <summary>
        /// The socket that is communicating with the browser.
        /// </summary>
        public Socket ConnectedSocket;

        #region Methods

        /// <summary>
        /// Closes the connection if still open.
        /// </summary>
        public void Finish()
        {
            if (ConnectedSocket != null && ConnectedSocket.Connected)
            {
                ConnectedSocket.Close();
                ConnectedSocket = null;
            }
        }

        /// <summary>
        /// Creates a new context object for a web request
        /// but reading the data sent to a recently opened
        /// socket.
        /// </summary>
        public static LPWebContext FromSocket(Socket socket)
        {
            var ret = new LPWebContext();
            ret.ConnectedSocket = socket;
            ret.Request = LPWebRequest.Read(socket);
            ret.Response = new LPWebResponse(ret);
            return ret;
        }

        #endregion
    }
}
