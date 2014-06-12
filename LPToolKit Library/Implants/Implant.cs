using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LPToolKit.LaunchPad;
using LPToolKit.Session;
using LPToolKit.Session.Managers;
using LPToolKit.MIDI;

namespace LPToolKit.Implants
{
    /// <summary>
    /// Base class for implants implemented in any language.
    /// </summary>
    public abstract class Implant
    {
        public Implant(RangeMap activeArea)
        {
            if (activeArea == null)
            {
                throw new ArgumentNullException("RangeMap");
            }

            ImplantID = string.Format("implant_{0:##}", nextId++);
            ActiveArea = activeArea;
            
        }

        /// <summary>
        /// The unique ID for this implant;
        /// </summary>
        public string ImplantID;

        private static int nextId = 1;
        
        /// <summary>
        /// Returns the device manager for this implant.
        /// </summary>
        public DeviceManager Devices
        {
            get { return UserSession.Current.Devices; }
        }

        /// <summary>
        /// The sections of different devices that this implant should
        /// accept events from.
        /// </summary>
        public RangeMap ActiveArea;

        /// <summary>
        /// Creates a string that can be used to identify a particular implant.
        /// </summary>
        public virtual string GetSourceName()
        {
            throw new NotImplementedException();
        }
    }
}
