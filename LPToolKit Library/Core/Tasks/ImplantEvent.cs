using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.Core.Tasks;

namespace LPToolKit.Core
{
    /// <summary>
    /// Differernt types allowed for ImplantEvents.
    /// </summary>
    public enum ImplantEventType
    {
        /// <summary>
        /// A hardware device setting changed.
        /// </summary>
        DeviceChange,

        /// <summary>
        /// A button went down
        /// </summary>
        PadPress,

        /// <summary>
        /// A button went up
        /// </summary>
        PadRelease,

        /// <summary>
        /// A button went down twice within 100msec
        /// </summary>
        PadDoubleClick,

        /// <summary>
        /// A knob was turned, a fader was moved.
        /// </summary>
        KnobChange,

        /// <summary>
        /// 1/96th of a measure has elapsed.
        /// </summary>
        Clock96,

        /// <summary>
        /// Called whenever the session's mode is changed.
        /// </summary>
        ModeChange,

        /// <summary>
        /// Called when an implant needs to be painted.
        /// </summary>
        GuiPaint,

        /// <summary>
        /// MIDI NoteOn message from a generic keyboard
        /// </summary>
        NoteOn,

        /// <summary>
        /// MIDI NoteOff message from a generic keyboard
        /// </summary>
        NoteOff,

        /// <summary>
        /// An OSC message has caused a value change
        /// </summary>
        OscMessage
    }

    /// <summary>
    /// A generic event that will eventually be delivered to
    /// an implant as a pad, knob, keys, or beat event.
    /// This is the rawest form the event will be in once
    /// it leaves a physical device.
    /// </summary>
    public class ImplantEvent : IKernelTask 
    {
        /// <summary>
        /// The type of event.
        /// TODO: consider using subclasses
        /// </summary>
        [Obsolete("We need to convert all of these to subclasses")]
        public ImplantEventType EventType;

        /// <summary>
        /// Location of the event for pads and knobs.
        /// This is not translated.
        /// </summary>
        public int X, Y;

        /// <summary>
        /// The value of this event when applicable.
        /// </summary>
        public int Value;

        /// <summary>
        /// Stores the MIDI message that triggered this event.
        /// NULL if the cause was not a MidiMessage.
        /// </summary>
        public MIDI.MidiMessage CausedBy;

        /// <summary>
        /// This is the device that originally generated this event.
        /// TODO: we may not want this long term
        /// </summary>
        public MIDI.MappedMidiDevice Hardware { get; set; }

        public override string ToString()
        {
            return string.Format("{0}({1},{2})={3}", EventType, X, Y, Value);
        }

        /// <summary>
        /// Logs that this event was delivered to an implant.
        /// </summary>
        public void LogDestination(string destination)
        {
            if (CausedBy != null)
            {
                CausedBy.LogDestination(destination);
            }
        }

        /// <summary>
        /// Creates a new deep copy of this event, since if the event
        /// is delievered to multiple implants it will have different
        /// virtual addresses.
        /// </summary>
        public virtual T Clone<T>() where T : ImplantEvent, new()
        {
            return new T()
            {
                EventType = EventType,
                X = X,
                Y = Y,
                Value = Value,
                CausedBy = CausedBy 
            };
        }

        public virtual ImplantEvent Clone()
        {
            return Clone<ImplantEvent>();
        }

        #region IKernalTask Implementation


        public virtual void RunTask()
        {
#warning TODO - likely do something more sophisticated than just passing this along

            Session.UserSession.Current.Implants.TriggerFromKernel(this);
        }

        public virtual int ExpectedLatencyMsec
        {
            get { return 500; }
        }

        public virtual IKernelTask ScheduleTask()
        {
            Kernel.Current.Add(this);
            return this;
        }

        #endregion

    }



    /// <summary>
    /// Event handler receiving new implant events.
    /// </summary>
    public delegate void ImplantEventHander(object sender, ImplantEvent e);
}
