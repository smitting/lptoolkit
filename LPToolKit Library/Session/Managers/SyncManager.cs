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
using LPToolKit.Core.Tasks;

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

        /// <summary>
        /// Constructor starts clock task.
        /// </summary>
        public SyncManager(UserSession parent)
            : base(parent)
        {
            new SyncTask(this).ScheduleTask();           
        }

        #endregion

        #region Properties
        
        /// <summary>
        /// Current beat-synced timer for the session.
        /// </summary>
        public readonly SyncTime BeatSync = new SyncTime();

        #endregion

        #region Methods

        #endregion

        #region Private

        /// <summary>
        /// A very high priority repeating task that sends and event
        /// every 1/96 tick.
        /// </summary>
        private class SyncTask : RepeatingKernelTask
        {
            #region Constructor

            public SyncTask(SyncManager parent) : base()
            {
                Parent = parent;
                MinimumRepeatTimeMsec = 1;
                ExpectedLatencyMsec = 1;
            }

            #endregion

            #region Properties

            /// <summary>
            /// Instance containing the system clock
            /// </summary>
            public readonly SyncManager Parent;

             #endregion

            #region IRepeatingKernelTask Implementation

            /// <summary>
            /// Returns true whenever the clock tick is ready
            /// </summary>
            public override bool ReadyToRun
            {
                get
                {
                    return Parent.BeatSync.SecondsPer96 <= (DateTime.UtcNow - _lastStep).TotalSeconds;
                }
            }

            /// <summary>
            /// Synchronizes the timer at 1/96th measures and sends an
            /// event.
            /// </summary>
            public override void RunTask()
            {
                _lastStep = DateTime.UtcNow;

                Parent.BeatSync.Mark();
                var tick = Parent.BeatSync.Tick;
                if (_lastValue != tick)
                {
                    new Clock96ImplantEvent()
                    {
                        Value = tick,
                        X = (int)Parent.BeatSync.Measure96AsDouble
                    }.ScheduleTask();
                    _lastValue = tick;
                }
            }

            #endregion

            #region Private

            /// <summary>
            /// How long since the last tick.
            /// </summary>
            private DateTime _lastStep = DateTime.MinValue;

            /// <summary>
            /// Value of last event, to make sure we don't send the
            /// same tick twice.
            /// </summary>
            private int _lastValue = -1;

            #endregion
        }

        #endregion
    }
}
