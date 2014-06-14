//#define CLOCK_DISABLED

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.Logs;
using LPToolKit.Sync;
using LPToolKit.Util;
using System.Threading;
using LPToolKit.Implants;
using LPToolKit.Core;
using LPToolKit.Core.Tasks.ImplantEvents;

namespace LPToolKit.Session.Managers
{  
    /// <summary>
    /// Controls the playback clock, syncs to remote sources and 
    /// providings syncing to third-party software and hardware
    /// via MIDI and OSC.
    /// 
    /// TODO:
    /// We need to be able to add lists of targets that we sync
    /// against and sources that we sync to.
    /// </summary>
    public class SyncManager : SessionManagerBase
    {
        #region Constructors

        public SyncManager(UserSession parent)
            : base(parent)
        {           
            _syncThread = new SyncThread();
            _syncThread.SleepAfterStep = true;
            _syncThread.Name = "Beat 1/96";

#warning move this to a task instead of a thread

#if CLOCK_DISABLED
#warning Midi Clock Disabled for debugging!!!
#else
            _syncThread.Start();
#endif
        }
        
        /// <summary>
        /// Kills the sync thread.
        /// </summary>
        ~SyncManager()
        {
            if (_syncThread != null)
            {
                _syncThread.Stop(100);
                _syncThread = null;
            }
        }
        #endregion

        #region Properties

        /// <summary>
        /// Current beat-synced timer for the session.
        /// </summary>
        public SyncTime BeatSync
        {
            get { return _syncThread.BeatSync; }
        }

        #endregion

        #region Methods

        #endregion

        #region Private

        /// <summary>
        /// The thread currently syncing the session time.
        /// </summary>
        private SyncThread _syncThread;

        /// <summary>
        /// Thread that provides a time object and causes it to send
        /// tick events based on the current tempo.
        /// </summary>
        private class SyncThread : SingleThread
        {
            public SyncThread() : base (null)
            {
                SleepAfterStep = true;
            }

            private int _lastValue = -1;

            private DateTime _lastStep = DateTime.MinValue;

            /// <summary>
            /// The system clock.
            /// </summary>
            public readonly SyncTime BeatSync = new SyncTime();

            public override void OnStep()
            {
                _lastStep = DateTime.UtcNow;
                BeatSync.Mark();
                var newValue = BeatSync.Tick;
                if (newValue != _lastValue)
                {
                    new Clock96ImplantEvent()
                    {
                        Value = newValue,
                        X = (int)BeatSync.Measure96AsDouble
                    }.ScheduleTask();
                    _lastValue = newValue;
                }
            }

            /// <summary>
            /// Wait for next step.
            /// </summary>
            protected override void AfterStep()
            {
                double desiredWait = BeatSync.SecondsPer96;
                double actualWait = (DateTime.UtcNow - _lastStep).TotalSeconds;
                while (actualWait < desiredWait)
                {
                    var left = (desiredWait - actualWait) * 1000.0;
                    Thread.Sleep((int)left);
                    actualWait = (DateTime.UtcNow - _lastStep).TotalSeconds;
                }
            }
        }

        #endregion
    }
}
