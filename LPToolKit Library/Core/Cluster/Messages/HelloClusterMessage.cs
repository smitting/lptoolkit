using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.Core.Cluster.Messages
{
    /// <summary>
    /// Sent to tell other cluster nodes about the existance of a node.
    /// </summary>
    public class HelloClusterMessage : ClusterMessage
    {
        #region Properties

        /// <summary>
        /// The node being identified by this message.
        /// </summary>
        public ClusterNode Node;

        #endregion

        #region ClusterMessage Implementation

        public override string MessageID
        {
            get { return "HELLO"; }
        }

        public override string MessageData
        {
            get
            {
                return Node == null ? null : string.Format("{0}|{1}", Node.IP, Node.Name);
            }
            set
            {
                var parts = value.Split('|');
                if (parts.Length != 2)
                {
                    Node = null;
                }
                else
                {
                    Node = ClusterNode.Get(parts[1], parts[0]);
                }
            }
        }

        #endregion
    }
}
