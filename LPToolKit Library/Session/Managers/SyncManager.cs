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
using LPToolKit.MIDI;
using LPToolKit.MIDI.Hardware;

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

            
            // TESTING A THREAD TO SEND SYNC
            var t = new Thread(() =>
            {
                try
                {
                    DateTime _lastStep = DateTime.UtcNow;
                    DateTime _lastSendCheck = DateTime.MinValue;

                    int lastValue = -1;
                    List<MappedMidiDevice> _clockSends = null;
                    for (; ; )
                    {
                        BeatSync.Mark();
                        if (lastValue == BeatSync.Tick) continue;
                        _lastStep = DateTime.UtcNow;
                        lastValue = BeatSync.Tick;


                        // check for MIDI sync output devices
                        if (_clockSends == null || (_lastStep - _lastSendCheck).TotalSeconds > 5)
                        {
                            _clockSends = Parent.Devices[typeof(MidiClockOutputHardwareInterface)];
                            // TODO: would be nice to have an event to tell us when to pull this list again immediately
                            _lastSendCheck = DateTime.UtcNow;
                        }

                        // send MIDI clock tick to all devices
                        foreach (var mapping in _clockSends)
                        {
                            if (mapping.Hardware is MidiClockOutputHardwareInterface)
                            {
                                (mapping.Hardware as MidiClockOutputHardwareInterface).Tick();
                            }
                        }
                        Thread.Sleep(BeatSync.MillisecondsPer96 / 2);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR IN TEST: " + ex.Message);
                }
            });
            t.Start();
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


                // update the click value
                Parent.BeatSync.Mark();
                var tick = Parent.BeatSync.Tick;
                if (_lastValue != tick)
                {
                    // send event if we have a new tick
                    new Clock96ImplantEvent()
                    {
                        Value = tick,
                        X = (int)Parent.BeatSync.Measure96AsDouble
                    }.ScheduleTask();
                    _lastValue = tick;
                    /*
                    // check for MIDI sync output devices
                    if ((_lastStep - _lastSendCheck).TotalSeconds > 15)
                    {
                        _clockSends = Parent.Parent.Devices[typeof(MidiClockOutputHardwareInterface)];
                        // TODO: would be nice to have an event to tell us when to pull this list again immediately
                        _lastSendCheck = DateTime.UtcNow;
                    }

                    // send MIDI clock tick to all devices
                    foreach (var mapping in _clockSends)
                    {
                        if (mapping.Hardware is MidiClockOutputHardwareInterface)
                        {
                            (mapping.Hardware as MidiClockOutputHardwareInterface).Tick();
                        }
                    }*/
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

            /// <summary>
            /// List of places to send MIDI clock signals.
            /// </summary>
            private List<MappedMidiDevice> _clockSends = new List<MappedMidiDevice>();

            /// <summary>
            /// Last time we checked the clock signal list.
            /// </summary>
            private DateTime _lastSendCheck = DateTime.MinValue;

            #endregion
        }

        #endregion
    }
}
