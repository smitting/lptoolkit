using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.Core.Cluster
{
    /// <summary>
    /// Information about the overall status of the cluster network.
    /// </summary>
    public class ClusterNetwork
    {
        #region Properties

        /// <summary>
        /// The cluster singleton.
        /// </summary>
        public readonly static ClusterNetwork Current = new ClusterNetwork();

        /// <summary>
        /// All of the nodes connected to the cluster this computer
        /// is participating in.
        /// </summary>
        public List<ClusterNode> Nodes = new List<ClusterNode>();

        #endregion
    }
}
