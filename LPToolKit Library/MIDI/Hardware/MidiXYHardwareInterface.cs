using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.MIDI.Pads;
using LPToolKit.Implants;
using LPToolKit.MIDI.Pads.Mappers;
using LPToolKit.Core;
using LPToolKit.Core.Tasks;

namespace LPToolKit.MIDI.Hardware
{
    /// <summary>
    /// Method the VirtualGrid uses to say a MIDI message needs 
    /// routed by x/y coordinant.
    /// </summary>
    public delegate IKernelTask MidiXYRouteHandler(int x, int y, int value);
 

    /// <summary>
    /// Base class for hardware that has an XY mapping.
    /// </summary>
    public abstract class MidiXYHardwareInterface : MidiHardwareInterface
    {
        #region Constructors

        /// <summary>
        /// Constructor creates the virual grid for the XY hardware.
        /// </summary>
        public MidiXYHardwareInterface(MappedMidiDevice mapping)
            : base(mapping)
        {
            CreateMappers();
            Grid = new VirtualGrid((int x, int y, int value) =>
            {
                if (XYMapper == null) return null;
                if (mapping == null) return null;
                // TODO: gonna need to tell the virtual grid to refresh whenever the mapping changes!

                MidiMessage msg = new MidiMessage();
                if (XYMapper.ConvertXY(x, y, out msg.Type, out msg.Number))
                {
                    msg.Velocity = value;
                    msg.LogSource(LastSourceName);
                    return new MidiIndicatorAction() { Driver = mapping.Driver, Message = msg }.ScheduleTask();
                }
                return null;
            }, 64, 64, 9, 9); // TODO: maybe have more reasonable defaults?
        }

        #endregion

        #region Properties

        /// <summary>
        /// Object providing the virtual grid space to make the 
        /// hardware seem larger to the implant.
        /// </summary>
        public readonly VirtualGrid Grid;
        // TODO: MOVE VIRTUAL GRID TO IMPLANT SPECIFIC!!!!

        /// <summary>
        /// Maps XY coordinates to MIDI pitches.
        /// </summary>
        protected IButtonXYMapper XYMapper;

        /// <summary>
        /// Maps colors to MIDI velocities.
        /// </summary>
        protected IButtonColorMapper ColorMapper;

        /// <summary>
        /// How quickly to count as double click.
        /// </summary>
        public static int DoubleClickTime = 250;

        #endregion

        #region Methods

        /// <summary>
        /// Allows to subclass to create all mapper instances like
        /// the ColorMapper.
        /// </summary>
        protected abstract void CreateMappers();

        /// <summary>
        /// Handles request from implants to set a value to an x/y
        /// coordinant.
        /// </summary>
        public void Send(int x, int y, string name, string sourceName)
        {
            if (ColorMapper == null)
            {
                throw new Exception("Color names are not supported");
            }
            Send(x, y, ColorMapper.ColorToValue(name), sourceName);
        }

        public virtual void Send(int x, int y, int value, string sourceName)
        {
            LastSourceName = sourceName;
            Grid.Set(x, y, value);            
        }

        /// <summary>
        /// Converts a raw MIDI message into an event ready to be
        /// sent to an implant.
        /// </summary>
        public override ImplantEvent Convert(MidiMessage midi)
        {
            var ret = new ImplantEvent();

            // convert to XY
            XYMapper.ConvertXY(midi.Type, midi.Pitch, out ret.X, out ret.Y, out ret.EventType);

            // differentiate pad event types
            if (ret.EventType == ImplantEventType.PadPress)
            {
                var doublePress = IsDoublePress(midi);
                ret.EventType = midi.Value > 0
                    ? (doublePress ? ImplantEventType.PadDoubleClick : ImplantEventType.PadPress)
                    : ImplantEventType.PadRelease;
            }
            ret.Value = midi.Value;
            return ret;
        }


        public override void Clear()
        {
            // TESTING: erasing the entire grid region when we leave
            Grid.SetAll(0);

            /*
            if (LPIO != null)
            {
                LPIO.Input.ClearEvents();
                LPIO.Output.Reset();
            }
            */
        }

        #endregion



        #region Private

        /// <summary>
        /// Watches all midi messages, returning true when the current
        /// one should be treated as a double press.
        /// </summary>
        private bool IsDoublePress(MidiMessage midi)
        {
            bool doublePress = false;
            if (midi.Value > 0)
            {
                if (midi.Pitch == lastPressNote)
                {
                    if ((DateTime.Now - lastPressTime).TotalMilliseconds < DoubleClickTime)
                    {
                        doublePress = true;
                    }
                }
                lastPressNote = midi.Pitch;
                lastPressTime = DateTime.Now;
            }
            return doublePress;
        }

        /// <summary>
        /// Used for tracking double clicks.
        /// </summary>
        private DateTime lastPressTime;
        private int lastPressNote = -1;

        #endregion
    }

}
