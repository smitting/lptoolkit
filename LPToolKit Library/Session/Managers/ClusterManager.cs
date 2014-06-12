using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LPToolKit.Core.Cluster;
using LPToolKit.Core.Cluster.Messages;
using LPToolKit.Util;

namespace LPToolKit.Session.Managers
{
    /// <summary>
    /// Manager for handling the cluster of several copies of 
    /// MidiSymbiont running together on a network.  This class
    /// facilitates connecting up to the cluster and passing shared
    /// messages to the kernel as appropriate.
    /// </summary>
    public class ClusterManager : SessionManagerBase
    {
        #region Constructors

        public ClusterManager(UserSession parent)
            : base(parent)
        {
            ClusterClient.Instance.Received += ClusterMessage_Received;

            // thread announces its existence every 10 seconds.
            /*
            _announcementThread = new SingleThread()
            {
                Step = () =>
                {
                    Announce();
                    Thread.Sleep(10000);
                }
            };
            _announcementThread.Start();
             */
        }

        ~ClusterManager()
        {
            if (_announcementThread != null)
            {
                _announcementThread.Stop();
                _announcementThread = null;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// The current cluster.
        /// </summary>
        public ClusterNetwork Current { get { return ClusterNetwork.Current; } } //= new ClusterNetwork();

        #endregion

        #region Methods

        /// <summary>
        /// Sends an announcement.
        /// TODO: should be handled by the kernel on a super low
        /// priority so it just does it whenever it has nothing
        /// better to do.
        /// </summary>
        public void Announce()
        {
            ClusterClient.Instance.Send(new HelloClusterMessage() { Node = ClusterNode.Local });
        }

        #endregion

        #region Private

        private SingleThread _announcementThread;

        /// <summary>
        /// Handler for all messages received across the cluster.
        /// TODO: should just be sent to the kernel
        /// </summary>
        void ClusterMessage_Received(ClusterMessage msg)
        {
            // keep track of all nodes detected, and announce ourselves
            // whenever a new node is discovered
            if (msg is HelloClusterMessage)
            {
                ClusterNode node = null;
                node = (msg as HelloClusterMessage).Node;
                if (node == null) return;

                lock (Current)
                {
                    if (Current.Nodes.Contains(node) == false)
                    {
                        // this is for testing
                        Parent.Console.Add("Detected new node: " + node.ToString());

                        // announce to this specific ip in case it can't hear the broadcasts
                        if (node != ClusterNode.Local)
                        {
                            ClusterClient.Instance.Send(node.IP, new HelloClusterMessage() { Node = ClusterNode.Local });
                        }

                        // add to the list
                        Current.Nodes.Add(node);
                    }
                }

                node.LastHeardFrom = DateTime.Now;
            }
            else if (msg is OSCClusterMessage)
            {
                // TODO: send this value to the OSC manager
                var oscMessage = msg as OSCClusterMessage;
                if (oscMessage == null) return;

                Parent.OSC.FromCluster(oscMessage.OSCAddress, oscMessage.OSCValues);

                // TODO: cause implant events to notice this new value
            }

        }

        #endregion
    }
}
