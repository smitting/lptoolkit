using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.Core.Cluster.Messages
{
    /// <summary>
    /// Base class for all messages communicated over the cluster.
    /// </summary>
    public abstract class ClusterMessage
    {
        #region Properties

        /// <summary>
        /// Identifier for the type of message being sent.
        /// </summary>
        public abstract string MessageID { get; }

        /// <summary>
        /// A string that contains the type-specific message information.
        /// </summary>
        public abstract string MessageData { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the array of bytes to send as a UDP packet.
        /// </summary>
        public byte[] ToPacket()
        {
            string data = MessageData;
            if (data == null) return null;
            return Encoding.UTF8.GetBytes(string.Format("{0}|{1}", MessageID, data));
        }

        /// <summary>
        /// Splits a packet into its type id and data, returning true
        /// iff the packet had a valid format.
        /// </summary>
        public static bool Parse(byte[] packet, out string messageId, out string message)
        {
            var s = Encoding.UTF8.GetString(packet);
            var i = s.IndexOf('|');
            if (i == -1)
            {
                messageId = null;
                message = null;
                return false;
            }

            messageId = s.Substring(0, i);
            message = s.Substring(i + 1);
            return true;
        }

        /// <summary>
        /// Splits the packet and then parses the meaning of the
        /// message using the type specified by the message id.
        /// </summary>
        public static ClusterMessage Parse(byte[] packet)
        {
            // parse the container
            string messageId;
            string message;
            if (!Parse(packet, out messageId, out message))
            {
                return null;
            }

            // create an instance of the type of message
            var instance = ClusterMessageTypes.CreateInstanceForMessageId(messageId);
            if (instance == null)
            {
                return null;
            }

            // use that instance to parse the data
            instance.MessageData = message;
            return instance;
        }

        #endregion
    }
}
