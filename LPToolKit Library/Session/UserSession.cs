using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LPToolKit.Implants;
using LPToolKit.LaunchPad;
using Newtonsoft.Json;
using System.IO;
using LPToolKit.Sync;
using LPToolKit.Util;
using LPToolKit.MIDI;
using LPToolKit.Session.Managers;
using LPToolKit.GUI;
using LPToolKit.OSC;
using LPToolKit.Core.Tasks;

namespace LPToolKit.Session
{
    /// <summary>
    /// Manages a set of loaded implants and other settings for a user
    /// that can be saved and loaded back again.
    /// </summary>
    public class UserSession
    {
        #region Constructor

        /// <summary>
        /// Creates an instance for managing each aspect of a user session.
        /// </summary>
        public UserSession()
        {
            Implants = new ImplantManager(this);
            Devices = new DeviceManager(this);
            MidiMap = new MidiMappingManager(this);
            OSC = new OscManager(this);
            Modes = new ModeManager(this);
            Sync = new SyncManager(this);
            Console = new ConsoleManager(this);
            Gui = new GuiManager(this);
            Cluster = new ClusterManager(this);
            _saveTask = new SaveTask(this);
        }


        #endregion

        #region Properties

        /// <summary>
        /// The active session.
        /// </summary>
        public static UserSession Current = new UserSession();

        /// <summary>
        /// The current loaded implants.
        /// </summary>
        public readonly ImplantManager Implants;

        /// <summary>
        /// The current device settings.
        /// </summary>
        public readonly DeviceManager Devices;

        /// <summary>
        /// The current OSC to MIDI mappings.
        /// </summary>
        public readonly MidiMappingManager MidiMap;

        /// <summary>
        /// Manages distribution of OSC messages.
        /// </summary>
        public readonly OscManager OSC;

        /// <summary>
        /// Manages beat synchronization.
        /// </summary>
        public readonly SyncManager Sync;

        /// <summary>
        /// Stores all standard output style text from implants and the system.
        /// </summary>
        public readonly ConsoleManager Console;

        /// <summary>
        /// Manages the selected mode.
        /// </summary>
        public readonly ModeManager Modes;

        /// <summary>
        /// Manages painting events on the gui
        /// </summary>
        public readonly GuiManager Gui;

        /// <summary>
        /// Manages the networked cluster.
        /// </summary>
        public readonly ClusterManager Cluster;

        /// <summary>
        /// The filename this session will be saved to.
        /// </summary>
        public FilePath Filename = new FilePath()
            {
                BaseFolder = Core.Settings.SessionFolder,
                Filename = "~/session1.userSession",
                Source = "UserSession"
            };

        #endregion

        #region Methods

        /// <summary>
        /// Returns a user session saved from disk.
        /// </summary>
        public static UserSession Load(string filename)
        {
            var ret = new UserSession();
            ret.Reload(filename);
            return ret;
        }


        /// <summary>
        /// Loads a user session from disk, either using the current
        /// filename or a new one provided.
        /// </summary>
        public void Reload(string filename = null)
        {
            UpdateFilename(filename);
            if (Filename.Exists)
            {
                SessionFile.Load(this, Filename);
            }
        }

        /// <summary>
        /// Saves a user session to disk, either using the current
        /// filename or a new one provided.
        /// </summary>
        public void Save(string filename = null)
        {
            UpdateFilename(filename);
            _saveTask.Signal();           
        }

        /// <summary>
        /// Does everything needed to prepare for an app shutdown.
        /// </summary>
        public static void Shutdown()
        {
            // clear out all pad device displays
            foreach (var device in Current.Devices.Mapped)
            {
                if (device.Hardware != null)
                {
                    device.Hardware.Clear();
                }
            }

            // stop all threads
            LPToolKit.Util.ThreadManager.Current.KillAll();
        }

        #endregion

        #region Private

        /// <summary>
        /// Repeating task that saves the session in the background.
        /// </summary>
        private SaveTask _saveTask;

        /// <summary>
        /// Updates the current filename.
        /// </summary>
        private void UpdateFilename(string filename = null)
        {
            Filename.BaseFolder = Core.Settings.SessionFolder;
            if (string.IsNullOrEmpty(filename) == false)
            {
                Filename.Filename = filename;
            }
        }

        /// <summary>
        /// This is a task that is constantly rescheduled by the kernel
        /// to save the session file as needed every 2 seconds, without
        /// requiring a new thread.
        /// </summary>
        private class SaveTask : RepeatingKernelTask
        {
            public SaveTask(UserSession session) : base()
            {
                MinimumRepeatTimeMsec = 2000;
                ExpectedLatencyMsec = 5000;
                Session = session;
            }

            /// <summary>
            /// The session to be saved.
            /// </summary>
            public readonly UserSession Session;

            /// <summary>
            /// We're only ready to run if we're been signaled and
            /// the time has elapsed.
            /// </summary>
            public override bool ReadyToRun
            {
                get { return _signalled && base.ReadyToRun; }
            }

            /// <summary>
            /// Tells the task the write the file on its next pass.
            /// </summary>
            public void Signal()
            {
                _signalled = true;
            }

            /// <summary>
            /// Saves the file whenever it has been signaled to do so.
            /// </summary>
            public override void RunTask()
            {
                _signalled = false;
                if (Session != null)
                {
                    SessionFile.Save(Session, Session.Filename);
                }
            }

            private bool _signalled = false;
        }

        #endregion
    }
}
