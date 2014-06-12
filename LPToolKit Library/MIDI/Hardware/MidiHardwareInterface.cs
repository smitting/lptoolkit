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
        /// Get the list of available subclasses
        /// </summary>
        static MidiHardwareInterface()
        {
            var interfaces = (from assemby in AppDomain.CurrentDomain.GetAssemblies()
                              from type in assemby.GetTypes()
                              where type.IsSubclassOf(typeof(MidiHardwareInterface))
                              select type
                                  ).ToArray();
            Available = new List<string>();
            _availableInstances = new Dictionary<string, MidiHardwareInterface>();
            foreach (var type in interfaces)
            {
                MidiHardwareInterface instance = CreateInstance(type);
                if (instance != null)
                {
                    Available.Add(instance.Name);
                    _availableInstances.Add(instance.Name, instance);
                }
            }
        }

        /// <summary>
        /// Sets up the IO for the subclass.
        /// </summary>
        public MidiHardwareInterface(MappedMidiDevice mapping)
        {
            this.Mapping = mapping;

            // start input pump
            if (Mapping != null)
            {
                if (Mapping.Device.CanRead)
                {
                    Mapping.Driver.MidiInput += (sender, e) =>
                    {
                        new MidiEvent()
                        {
                            Message = e.Message,
                            Hardware = this,
                            ExpectedLatencyMsec = e.Message.Type == MidiMessageType.ControlChange ? 1000 : 100
                        }.ScheduleTask();
                    };
                }
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// The device and driver this interface is translating for.
        /// </summary>
        public readonly MappedMidiDevice Mapping;

        /// <summary>
        /// Event triggered when a MIDI message is received from this
        /// device that would be meaningful to an implant.
        /// </summary>
        //public event ImplantEventHander EventReceived;

        /// <summary>
        /// List of named hardware implementations currently available 
        /// in the software, which is computed in the static constructor.
        /// </summary>
        public readonly static List<string> Available;

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

        public virtual void Clear()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new MidiHardwareInterface from just a type.
        /// </summary>
        private static MidiHardwareInterface CreateInstance(Type t, MappedMidiDevice mapping = null)
        {
            try
            {
                if (t.IsAbstract) return null;
                var constructor = t.GetConstructor(new Type[] { typeof(MappedMidiDevice) });
                return constructor.Invoke(new object[] { mapping }) as MidiHardwareInterface;
            }
            catch (Exception ex)
            {
                Session.UserSession.Current.Console.Add(ex.ToString(), "MidiHardwareInterface");
                return null;
            }
        }

        /// <summary>
        /// Creates a new instance of a hardware interface from a name
        /// returned from the Available dictionary.
        /// </summary>
        public static MidiHardwareInterface CreateInstance(string name, MappedMidiDevice mapping)
        {
            MidiHardwareInterface mhi = null;
            if (_availableInstances.TryGetValue(name, out mhi))
            {
                return CreateInstance(mhi.GetType(), mapping);
            }
            return null;
        }

        /// <summary>
        /// Checks all available hardware interfaces and returns 
        /// the name of the first interface that thinks it's the
        /// correct mapping.
        /// </summary>
        public static string AutoMap(MidiDevice device)
        {
            foreach (var key in _availableInstances.Keys)
            {
                var instance = _availableInstances[key];
                if (instance.Supports(device))
                {
                    return key;
                }
            }
            return null;
        }

        /// <summary>
        /// Sends implant event to EventReceived.
        /// </summary>
        public void Trigger(ImplantEvent ie)
        {
            if (ie != null)
            {
                ie.Hardware = this.Mapping;
                ie.ScheduleTask();
                /*
                if (EventReceived != null)
                {
                    EventReceived(this, ie);
                }*/
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
        /*
        /// <summary>
        /// Always used by subclasses to send a MIDI message to the
        /// driver.  Allows the base class to deal with logging.
        /// </summary>
        [Obsolete("Use a MidiAction instead so the Kernel can queue the output")]
        protected ScheduledMidiMessage Route(MidiMessage midiMessage)
        {
            if (Mapping == null) return null;
            if (Mapping.Driver == null) return null;
            midiMessage.LogSource(LastSourceName);
            return Mapping.Driver.Send(midiMessage);
        }
        */
        #endregion

        #region Private

        /// <summary>
        /// Stores an instance of each interface so we can get its 
        /// name and attempt automaps.
        /// </summary>
        private readonly static Dictionary<string, MidiHardwareInterface> _availableInstances;


        #endregion
    }

    
}
