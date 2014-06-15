using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.Core.Tasks;

namespace LPToolKit.MIDI.Hardware
{
    /// <summary>
    /// Device mapping for sending a MIDI clock signal.
    /// </summary>
    public class MidiClockOutputHardwareInterface : MidiHardwareInterface
    {
        #region Constructors

        public MidiClockOutputHardwareInterface(MappedMidiDevice mapped) : base(mapped)
        {

        }

        #endregion

#warning This means some devices will need to support multiple hardware interfaces at the same time

        #region MidiHardwareInterface Implementation

        /// <summary>
        /// Name for this interface.
        /// </summary>
        public override string Name
        {
            get { return "MIDI Clock Out"; }
        }

        /// <summary>
        /// Is never automapped.
        /// </summary>
        public override bool Supports(MidiDevice device)
        {
            return false;
        }

        /// <summary>
        /// Converts no messages.
        /// </summary>
        public override Core.ImplantEvent Convert(MidiMessage midi)
        {
            return null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Immediately sends a clock signal to the mapped device.
        /// </summary>
        public void Tick()
        {
            new MidiClockAction()
            {
                Driver = Mapping.Driver,
                Message = new MidiMessage() { Type = MidiMessageType.ClockTick }
            }.RunTask();
            //.ScheduleTask();
        }

        #endregion
    }
}
