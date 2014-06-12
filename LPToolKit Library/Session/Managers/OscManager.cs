using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.Implants;
using LPToolKit.OSC;
using LPToolKit.Logs;
using LPToolKit.Core.Cluster;
using LPToolKit.Core.Cluster.Messages;
using LPToolKit.Core.Tasks.ImplantEvents;

namespace LPToolKit.Session.Managers
{
    /// <summary>
    /// Manages all OSC activity for the current user session, such
    /// as what OSC addresses to send to which remote IPs and ports
    /// optionally depending on the implant sending them.  Should
    /// also be able to receive OSC messages and route them either
    /// through implants or directly map them to another destination
    /// like the MIDI mapper.
    /// </summary>
    public class OscManager : SessionManagerBase
    {
         #region Constructors

        public OscManager(UserSession parent) : base(parent)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Current destinations for OSC.
        /// </summary>
        public readonly List<OSCConnection> Connections = new List<OSCConnection>();

        /// <summary>
        /// When set to true, all OSC messages are written to the log.
        /// </summary>
        public bool LoggingEnabled = true;

        /// <summary>
        /// Logged OSC messages
        /// </summary>
        public OrdinalList<OscLog> Log = new OrdinalList<OscLog>();

        #endregion

        #region Methods

        /// <summary>
        /// Sends the OSC message to all connected cluster nodes.
        /// </summary>
        public void ToCluster(OscDataMessage osc)
        {
            var msg = new OSCClusterMessage()
            {
                OSCAddress = osc.Address,
                OSCValues = osc.Values
            };
            ClusterClient.Instance.SendAll(msg);
        }

        /// <summary>
        /// Called when an OSC message is received from the cluster.
        /// </summary>
        public void FromCluster(string address, double[] values)
        {
            var osc = new OscDataMessage();
            osc.Address = address;
            osc.Values = values;
            osc.Source = "Cluster";
            Add(osc);
        }

        /// <summary>
        /// Called whenever an OSC message is sent by an implant.
        /// </summary>
        public void FromImplant(JavascriptImplant implant, string address, double value)
        {
            var osc = new OscDataMessage();
            osc.Address = address;
            osc.Value = value;
            osc.Source = implant.GetSourceName();
            Add(osc);
        }


        /// <summary>
        /// Forwards along an OSC message from any source.
        /// </summary>
        public void Add(OscDataMessage osc)
        {
#warning all this routing should be handled by the kernel, not just NotifyImplants


            // add to log when enabled
            if (LoggingEnabled)
            {                
                Log.Add(new OscLog() { Source = osc.Source, Message = osc });
            }

            // forward the OSC message to all other subsystems
            OscToMidi(osc);
            TransmitOSC(osc);
            if (osc.Source != "Cluster")
            {
                ToCluster(osc);
            }
            NotifyImplants(osc);
        }

        /// <summary>
        /// Sends an event to all implants except the source that this
        /// OSC value was changed.
        /// </summary>
        public void NotifyImplants(OscDataMessage osc)
        {
            new OscImplantEvent() { Osc = osc }.ScheduleTask();
        }

        /// <summary>
        /// Sends this OSC message over all appropriate OSC connections.
        /// </summary>
        public void TransmitOSC(OscDataMessage osc)
        {
            // TODO: need more control on which connections send to from which implants.
            foreach (var remote in Connections.ToArray())
            {
                remote.Send(osc.Address, osc.ValueAsFloat);
            }
        }

        /// <summary>
        /// Sends MIDI messages using the current mapping settings 
        /// from this OSC message.
        /// </summary>
        public void OscToMidi(OscDataMessage osc)
        {
            // create midi packet using current mapping settings
            OscToMidiMap mapping;
            var midi = Parent.MidiMap.Map(osc, out mapping);
            if (midi != null)
            {
                // log where this message came from
                midi.LogAsOutgoing();
                midi.LogSource(osc.Source); // TODO: log the OSC message as the source?

                // send midi to the devices specified by this mapping
                foreach (var device in Parent.Devices[typeof(MIDI.Hardware.MidiOutputHardwareInterface)])
                {
                    // filter out by destination
                    if (mapping != null && mapping.MidiDestination != null)
                    {
                        if (mapping.MidiDestination.ToLower() != device.Device.Name.ToLower())
                        {
                            continue;
                        }
                    }

                    // send packet
                    device.Driver.Send(midi);
                }
            }
        }

        #endregion
    }
}
