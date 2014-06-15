using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.MIDI;
using LPToolKit.OSC;
using LPToolKit.Logs;

namespace LPToolKit.Session.Managers
{
    /// <summary>
    /// Stores OSC to MIDI mapping information for this session.
    /// </summary>
    public class MidiMappingManager : SessionManagerBase
    {        
        #region Constructors

        public MidiMappingManager(UserSession parent) : base(parent)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// All available mappings.
        /// </summary>
        public readonly List<OscToMidiMap> Mappings = new List<OscToMidiMap>();

        /// <summary>
        /// When set to true, all MIDI messages are written to the log, both
        /// those outgoing from here, and those incoming from devices.
        /// TODO: not sure this is the write place for this log.
        /// </summary>
        public bool LoggingEnabled = false;

        /// <summary>
        /// Logged OSC messages
        /// </summary>
        //public List<MidiLog> Log = new List<MidiLog>();
        public OrdinalList<MidiLog> Log = new OrdinalList<MidiLog>();

        #endregion

        #region Methods

        /// <summary>
        /// Returns a mapping by id.
        /// </summary>
        public OscToMidiMap GetById(string id)
        {
            foreach (var mapping in Mappings)
            {
                if (mapping.ID == id)
                {
                    return mapping;
                }
            }
            return null;
        }

        /// <summary>
        /// Removes a mapping by id.
        /// </summary>
        public void DeleteById(string id)
        {
            foreach (var mapping in Mappings)
            {
                if (mapping.ID == id)
                {
                    Mappings.Remove(mapping);
                    return;
                }
            }
        }

        /// <summary>
        /// Finds the first mapper instance that matches the address.
        /// </summary>
        public OscToMidiMap FindMapping(string oscAddress, string oscSource = null)
        {
            foreach (var m in Mappings)
            {
                // check the source is allowed
                if (oscSource != null)
                {
                    if (m.OscSource != null && m.OscSource.ToLower() != oscSource.ToLower())
                    {
                        continue;
                    }
                }

                // check the address is a match
                if (m.IsMatch(oscAddress))
                {
                    return m;
                }
            }
            return null;
        }

        /// <summary>
        /// Maps to a MIDI message if it can, or returns null.
        /// </summary>
        /// <param name="osc">The OSC message to map</param>
        /// <param name="mapping">The mapping used to generate this packet.</param>
        public MidiMessage Map(OscDataMessage osc, out OscToMidiMap mapping)
        {
            // find a mapping that can convert this osc
            mapping = FindMapping(osc.Address, osc.Source);
            if (mapping == null)
            {
                return null;
            }

            // build midi
            var ret = new MidiMessage();
            ret.Type = mapping.MidiType;
            ret.Pitch = mapping.GetMidiNote(osc.Address);
            ret.Value = mapping.GetMidiValue(osc.Value);
            return ret;
        }

        #endregion
    }
}
