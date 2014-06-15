using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.Implants;
using LPToolKit.LaunchPad;
using LPToolKit.MIDI.Pads;
using LPToolKit.Core;
using LPToolKit.Core.Tasks;

namespace LPToolKit.MIDI.Hardware
{
    /// <summary>
    /// Base class for all objects that takes raw MIDI data from a
    /// MidiDriver object and converts it to meaningful data for some
    /// type of objects, like a PadDevice, for some specific hardware
    /// manufacturer, such as mapping a LaunchControl to its
    /// PadDevice and KnobDevice constituents.
    /// </summary>
    public abstract class MidiHardwareInterface
    {
        #region Constructors

        /// <summary>
        /// Sets up the IO for the subclass.
        /// </summary>
        public MidiHardwareInterface(MappedMidiDevice mapping)
        {
            this.Mapping = mapping;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The device and driver this interface is translating for.
        /// TODO: be nice if this wasn't needed
        /// </summary>
        public readonly MappedMidiDevice Mapping;

        #endregion

        #region Subclass Implementation Methods 
        
        /// <summary>
        /// Provides a name for a hardware interface.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Provides a way for a hardware implementation to guess
        /// whether a specific MidiDevice is a product this class
        /// would work for.
        /// </summary>
        public abstract bool Supports(MidiDevice device);

        /// <summary>
        /// Converts a raw MIDI message into an event ready to be
        /// sent to an implant, or returns null if the data would
        /// be useless.
        /// </summary>
        public abstract ImplantEvent Convert(MidiMessage midi);

        /// <summary>
        /// Allows whatever the hardware is doing to reset its state.
        /// </summary>
        public virtual void Clear()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sends implant event to EventReceived.
        /// </summary>
        public void Trigger(ImplantEvent ie)
        {
            if (ie != null)
            {
                ie.Hardware = this.Mapping;
                ie.ScheduleTask();
            }
        }

        /// <summary>
        /// Called by MidiInput to translate a midi message to an 
        /// implant event to trigger.  Exposed here to allow simulated
        /// devices to insert data into the event chain.
        /// </summary>
        public void Trigger(MidiMessage m)
        {
            ImplantEvent e = Convert(m);
            if (e != null)
            {
                e.CausedBy = m;
                Trigger(e);
            }
        }

        #endregion

        #region Protected

        /// <summary>
        /// Helps the interface tell the MidiMessage it creates where
        /// the message came from by storing the last source that called
        /// us.  Not fool proof, but effective.
        /// </summary>
        protected string LastSourceName = null;
        
        #endregion
    }
}
