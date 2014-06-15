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

            
            /*
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
             */
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
            /// A real-time only task for sending MIDI clock signals.
            /// </summary>
            private class RealTimeTickTask : IKernelTask
            {
                /// <summary>
                /// The MIDI device to send a clock signal to.
                /// </summary>
                public MidiClockOutputHardwareInterface Clock;

                /// <summary>
                /// Sends a Midi clock signal
                /// </summary>
                public void RunTask()
                {
                    Clock.Tick();
                }

                /// <summary>
                /// Throws exception.  This is only for real time use.
                /// </summary>
                public IKernelTask ScheduleTask()
                {
                    throw new Exception("This task should only be real-time scheduled");
                }

                /// <summary>
                /// Will not be used.
                /// </summary>
                public int ExpectedLatencyMsec { get { return 0; } set { } }
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
                    
                    
                    // check for MIDI sync output devices
                    if ((_lastStep - _lastSendCheck).TotalSeconds > 5)
                    {                        
                        // TODO: would be nice to have an event to tell us when to pull this list again immediately
                        _lastSendCheck = DateTime.UtcNow;
                        _clocks.Clear();                        
                        foreach (var send in Parent.Parent.Devices[typeof(MidiClockOutputHardwareInterface)])
                        {
                            if (send.Hardware is MidiClockOutputHardwareInterface)
                            {
                                _clocks.Add(send.Hardware as MidiClockOutputHardwareInterface);
                                //_clockTasks.Add(new RealTimeTickTask() { Clock = send.Hardware as MidiClockOutputHardwareInterface });
                            }
                        }
                        //LPConsole.WriteLine("SyncManager", "Found {0} clocks", _clocks.Count);
                    }
                    //LPConsole.WriteLine("SyncManager", "Current: {0} Scheduled At: {1} Diff in Msec={2}", DateTime.UtcNow.Ticks, time.Ticks, (time - DateTime.UtcNow).TotalMilliseconds);
                }

                // queue up the next 96 ticks in the real time kernel at every measure.
                var measure = Parent.BeatSync.Measure;
                if (measure != _lastMeasure)
                {

                    if (_lastMeasure != measure - 1)
                    {
                        LPConsole.WriteLine("SyncManager", "SKIPPED A MEASURE!!!! last={0} this={1}", _lastMeasure, measure);
                    }


                    _lastMeasure = measure;

                    DateTime dt = Parent.BeatSync.NextMeasureTime;
                    //LPConsole.WriteLine("SyncManager", "Scheduling next measure #{0} starting at {1} {2}msec in the future", measure, dt.ToShortTimeString(), (dt - DateTime.UtcNow).TotalMilliseconds);
                    for (var i = 0; i < 96;i++)
                    {
                        foreach (var clock in _clocks)
                        {
                            Kernel.Current.Add(
                                new RealTimeTickTask() { Clock = clock },
                                dt                            
                                );
                        }
                        dt = dt.AddSeconds(Parent.BeatSync.SecondsPer96);
                    }
                }

            }

            #endregion

            #region Private

            private int _lastMeasure = -1;

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
            /// A list of tasks to schedule every clock tick.
            /// </summary>
            private List<RealTimeTickTask> _clockTasks = new List<RealTimeTickTask>();

            /// <summary>
            /// MIDI output clocks to send to.
            /// </summary>
            private List<MidiClockOutputHardwareInterface> _clocks = new List<MidiClockOutputHardwareInterface>();

            /// <summary>
            /// Last time we checked the clock signal list.
            /// </summary>
            private DateTime _lastSendCheck = DateTime.MinValue;

            #endregion
        }

        #endregion
    }
}
