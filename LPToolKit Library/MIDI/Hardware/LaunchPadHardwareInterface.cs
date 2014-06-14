using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.MIDI.Pads;
using LPToolKit.Implants;
using LPToolKit.MIDI.Pads.Mappers;

namespace LPToolKit.MIDI.Hardware
{
    /// <summary>
    /// Maps raw MIDI data from a Novation LaunchPad to events that 
    /// are meaningful for implants.
    /// </summary>
    public class LaunchPadHardwareInterface : MidiXYHardwareInterface
    {
        #region Constructors

        public LaunchPadHardwareInterface(MappedMidiDevice mapping)
            : base(mapping)
        {
        }

        #endregion

        #region MidiXYHardwareInterface Implementation

        /// <summary>
        /// Report as a launchpad.
        /// </summary>
        public override string Name
        {
            get { return "Novation LaunchPad"; }
        }

        /// <summary>
        /// Automap if the name contains "launchpad".
        /// </summary>
        public override bool Supports(MidiDevice device)
        {
            return device.Name.ToLower().Contains("launchpad");
        }

        /// <summary>
        /// Creates the objects that map concepts like colors and XY
        /// coordinates into MIDI values.
        /// </summary>
        protected override void CreateMappers()
        {
            ColorMapper = new NovationColorMapper();
            XYMapper = new LaunchPadXYMapper();
        }

        #endregion
    }
}
