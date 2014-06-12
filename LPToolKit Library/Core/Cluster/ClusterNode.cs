using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.Core.Cluster
{

    /// <summary>
    /// Information about one computer connected to the cluster.
    /// </summary>
    public class ClusterNode
    {
        /// <summary>
        /// The name this node reports itself as (usually the person's name)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The IP address of this node.
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// Last time a message was received.
        /// </summary>
        public DateTime LastHeardFrom = DateTime.Now;

        /// <summary>
        /// The node that represent this computer.
        /// </summary>
        public static ClusterNode Local
        {
            get
            {
                if (_local == null)
                {
                    _local = new ClusterNode();
                    _local.Name = Util.NetUtil.GetLocalName();
                    _local.IP = Util.NetUtil.GetLocalIP();
                }
                return _local;
            }
        }

        /// <summary>
        /// Returns or creates a node by name and ip to prevent
        /// object churning.
        /// </summary>
        public static ClusterNode Get(string name, string ip)
        {
            // return any existing node matching this data
            lock (_allNodes)
            {
                foreach (var node in _allNodes)
                {
                    if (node.Name == name && node.IP == ip)
                    {
                        return node;
                    }
                }
            }

            // create a new node if none found
            lock (_allNodes)
            {
                var node = new ClusterNode()
                {
                    IP = ip,
                    Name = name
                };
                _allNodes.Add(node);
                return node;
            }
        }

        public override string ToString()
        {
            return string.Format("[Node(Name={0}, IP={1})]", Name, IP);
        }

        private static ClusterNode _local = null;

        /// <summary>
        /// List of all instances to prevent object churning.
        /// </summary>
        private static readonly List<ClusterNode> _allNodes = new List<ClusterNode>();
    }
}
