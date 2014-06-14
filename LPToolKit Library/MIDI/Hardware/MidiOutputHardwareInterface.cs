using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.Implants;
using LPToolKit.Core;

namespace LPToolKit.MIDI.Hardware
{
    /// <summary>
    /// Interface attached to devices setup for MIDI output via OSC-to-MIDI mapping
    /// </summary>
    public class MidiOutputHardwareInterface : MidiHardwareInterface
    {
        #region Constructors

        public MidiOutputHardwareInterface(MappedMidiDevice device)
            : base(device)
        {
        }

        #endregion

        #region MidiHardwareInterface Implementation

        /// <summary>
        /// Name to report hardware as.
        /// </summary>
        public override string Name
        {
            get { return "MIDI Output"; }
        }

        /// <summary>
        /// Output is never automapped.
        /// </summary>
        public override bool Supports(MidiDevice device)
        {
            return false;
        }

        /// <summary>
        /// No data input is converted.
        /// </summary>
        public override ImplantEvent Convert(MidiMessage midi)
        {
            return null;
        }

        #endregion
    }
}
