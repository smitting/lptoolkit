using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.Core.Cluster.Messages
{
    /// <summary>
    /// Messages to send changes to OSC values across the cluster.
    /// TODO: consider if these values should automatically be 
    /// retransmitted to all connected OSC devices when delivered
    /// to the cluster.
    /// </summary>
    public class OSCClusterMessage : ClusterMessage
    {
        #region Properties

        /// <summary>
        /// The address for this OSC packet.
        /// </summary>
        public string OSCAddress;

        /// <summary>
        /// An arbitrary number of double values to send with this message.
        /// </summary>
        public double[] OSCValues;

        #endregion

        #region ClusterMessage Implementation

        public override string MessageID
        {
            get { return "OSC"; }
        }

        public override string MessageData
        {
            get
            {
                return OSCAddress == null ? null : string.Format("{0}|{1}", OSCAddress, string.Join(",", StringValues));
            }
            set
            {
                var parts = value.Split('|');
                if (parts.Length != 2)
                {
                    OSCAddress = null;
                    OSCValues = null;
                }
                else
                {
                    OSCAddress = parts[0];
                    StringValues = parts[1].Split(',');
                }
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// The values cast as strings.
        /// </summary>
        private string[] StringValues
        {
            get
            {
                if (OSCValues == null) return new string[0];
                var ret = new string[OSCValues.Length];
                for (var i = 0; i < OSCValues.Length; i++)
                {
                    ret[i] = OSCValues[i].ToString();
                }
                return ret;
            }
            set
            {
                if (value == null)
                {
                    OSCValues = new double[0];
                    return;
                }
                OSCValues = new double[value.Length];
                for (var i = 0; i < value.Length; i++)
                {
                    if (double.TryParse(value[i], out OSCValues[i]) == false)
                    {
                        OSCValues[i] = 0;
                    }
                }
            }
        }

        #endregion
    }
}
