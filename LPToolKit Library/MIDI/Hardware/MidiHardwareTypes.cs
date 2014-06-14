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
    /// Manages all available MidiHardwareInterface types in all 
    /// loaded assemblies.
    /// </summary>
    public class MidiHardwareTypes
    {
        #region Constructors

        /// <summary>
        /// Get the list of available subclasses
        /// </summary>
        static MidiHardwareTypes()
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

        #endregion

        #region Properties

        /// <summary>
        /// List of named hardware implementations currently available 
        /// in the software, which is computed in the static constructor.
        /// </summary>
        public readonly static List<string> Available;

        #endregion

        #region Methods

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

        #endregion

        #region Private

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
        /// Stores an instance of each interface so we can get its 
        /// name and attempt automaps.
        /// </summary>
        private readonly static Dictionary<string, MidiHardwareInterface> _availableInstances;

        #endregion
    }
}
