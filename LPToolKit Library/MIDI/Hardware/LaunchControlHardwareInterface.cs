using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.Implants;
using LPToolKit.MIDI.Pads;
using LPToolKit.MIDI.Pads.Mappers;

namespace LPToolKit.MIDI.Hardware
{
    /// <summary>
    /// Maps raw MIDI signals from a Novation LaunchControl into
    /// meaningful data.
    /// </summary>
    public class LaunchControlHardwareInterface : MidiXYHardwareInterface
    {
        #region Constructors

        public LaunchControlHardwareInterface(MappedMidiDevice mapping)
            : base(mapping)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// The product this class is for.
        /// </summary>
        public override string Name
        {
            get { return "Novation LaunchControl"; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns true iff the device name contains "launch control"
        /// </summary>
        public override bool Supports(MidiDevice device)
        {
            return device.Name.ToLower().Contains("launch control");
        }


        protected override void CreateMappers()
        {
            ColorMapper = new NovationColorMapper();
            XYMapper = new LaunchControlXYMapper();
        }


        #endregion

        #region Private

        #endregion
    }

}
