using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.Implants;
using LPToolKit.MIDI;
using LPToolKit.Core;
using LPToolKit.Core.Tasks.ImplantEvents;

namespace LPToolKit.Session.Managers
{
    /// <summary>
    /// Manages the current "mode" for the user session, which defines
    /// which implants are currently actively mapped to real hardware,
    /// allowing different "screens" on a launchpad via a menu implant.
    /// </summary>
    public class ModeManager : SessionManagerBase
    {
        #region Constructors

        public ModeManager(UserSession parent)
            : base(parent)
        {

        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets/sets the selected mode, which controls which implants
        /// are currently visible on connected MIDI devices.
        /// </summary>
        public int CurrentMode
        {
            get { return _currentMode; }
            set
            {
                if (_currentMode != value)
                {
                    _currentMode = value;
                    UpdateCurrentMode();
                }
            }
        }

        /// <summary>
        /// Returns the list of implants that match the current mode 
        /// and thus can be connected to MIDI hardware.
        /// </summary>
        public List<JavascriptImplant> CurrentImplants
        {
            get { return GetImplantsByMode(CurrentMode); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Mode number meaning an implant is always available 
        /// regardless of mode.
        /// </summary>
        public const int ALL_MODES = -1;

        /// <summary>
        /// Returns all of the loaded javascript implants that are
        /// assigned to a given mode in the implant manager.
        /// </summary>
        public List<JavascriptImplant> GetImplantsByMode(int mode)
        {
            return Parent.Implants.Running.Where(ji => ji.AssignedMode == ALL_MODES || ji.AssignedMode == mode).ToList();
        }

        /// <summary>
        /// Makes sure all devices are in sync with the current mode and 
        /// sends a modechanged event to all loaded implants.
        /// </summary>
        /// <remarks>
        /// Called whenever the current mode is changed, which handles
        /// any changes in hardware needed, updates the device's display
        /// to show the new implants, and sends events that the mode
        /// has been changes.
        /// </remarks>
        public void UpdateCurrentMode()
        {
            // TEMP: clear entire launchpad view
            // TODO: change the system so that by its nature this automatically
            // happens.  Each implant should have it's own launchpad buffer
            // behind the scenes, instead of having one buffer attached to
            // each midi device that is a launchpad.
            // TODO: should this be called on all device types and let them deal with it?
            //foreach (var device in Parent.Devices[typeof(MIDI.Hardware.LaunchPadHardwareInterface)])//[MIDI.MidiDeviceMapping.PadDevice])
            foreach (var device in Parent.Devices.Mapped)
            {
                if (device.Enabled && device.Hardware != null)
                {
                    device.Hardware.Clear();
                }
            }



            // notify all implants via event
            //Parent.Implants.Trigger(new ImplantEvent() { EventType = ImplantEventType.ModeChange, Value = _currentMode }, null);
            new ModeChangeImplantEvent()
            {
                Value = _currentMode
            }.ScheduleTask();
        }

        #endregion

        #region Private

        /// <summary>
        /// Actual storage for current mode.
        /// </summary>
        private int _currentMode = 0;


        #endregion

    }
}
