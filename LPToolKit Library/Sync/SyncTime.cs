using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.Sync
{
    /// <summary>
    /// Coordinates playback time with support for OSC and MIDI 
    /// synchronization.  Time events are typically sent in
    /// 1/96 of a measure increments to be compatible with MIDI-centric
    /// software such as Reaktor.
    /// </summary>
    public class SyncTime
    {
        #region Constructor
        
        /// <summary>
        /// Creates a new time object, setting the start of the 
        /// song as the current time.
        /// </summary>
        public SyncTime()
        {
            Tempo = 120;
            Start();
        }

        #endregion

        #region Properties

        /// <summary>
        /// When the timer was started.  All location calculations
        /// are based against this value.
        /// </summary>
        public DateTime StartTime;

        /// <summary>
        /// The current tempo in BPM, which all synchronization is 
        /// based against.
        /// </summary>        
        public double Tempo
        {
            get { return _tempo; }
            set
            {
                if (_tempo != value)
                {
                    _tempo = value;
                    UpdateDurations();
                }
            }
        }

        /// <summary>
        /// Number of beats in each measure
        /// </summary>
        public int BeatsPerMeasure = 4;

        /// <summary>
        /// When enabled, the Ellapsed is only updated whenever
        /// the Mark() method is called, so all implants get the
        /// exact same time measurement on each pass in the pipeline.
        /// </summary>
        public bool UseMark = true;

        #endregion

        #region Calculated Properties

        /// <summary>
        /// The number of measures ellapsed since playback started.
        /// </summary>
        public int Measure
        {
            get { return (int)(TotalBeats / BeatsPerMeasure); }
        }

        /// <summary>
        /// The current beat in the measure.  Always less than the
        /// number of beats per measure.
        /// </summary>
        public int Beat
        {
            get { return (int)BeatAsDouble; }
        }

        /// <summary>
        /// Returns a number 0-999 of how far off we are from being
        /// exactly on a beat, mainly for DAW style time displays
        /// </summary>
        public int ThousandsthBeat
        {
            get { return (int)((BeatAsDouble - (double)Beat) * 1000.0); }
        }

        /// <summary>
        /// The current 1/96th tick, always between 0 and 95.
        /// </summary>
        public int Tick
        {
            get { return ((int)Measure96AsDouble) % 96; }
        }

        /// <summary>
        /// Total number of beats since playback started.
        /// </summary>
        public double TotalBeats
        {
            get { return Elapsed.TotalSeconds / SecondsPerBeat; }
        }

        /// <summary>
        /// Beat as a double, including partial beats.
        /// </summary>
        public double BeatAsDouble
        {
            get { return TotalBeats % BeatsPerMeasure; }
        }

        /// <summary>
        /// 96th of a measure, which is a common Reaktor measure.
        /// </summary>
        public double Measure96AsDouble
        {
            get { return Elapsed.TotalSeconds / SecondsPer96; }
        }

        /// <summary>
        /// How long since playback was started in seconds.
        /// </summary>
        public TimeSpan Elapsed
        {
            get { return UseMark ? _elapsed : ElapsedActual; }
        }

        /// <summary>
        /// Gets the exact time elapsed, ignoring Mark() settings.
        /// </summary>
        public TimeSpan ElapsedActual
        {
            get { return DateTime.Now - StartTime; }
        }

        /// <summary>
        /// The seconds portion of elapsed, between 0 and 59.999.
        /// </summary>
        public double ElapsedSeconds
        {
            get { return Elapsed.TotalSeconds % 60.0; }
        }

        /// <summary>
        /// The minites portion of elapsed, not wrapped.
        /// </summary>
        public int ElapsedMinutes
        {
            get { return Elapsed.Minutes; }
        }

        /// <summary>
        /// How long each 1/96th tick is in seconds.
        /// </summary>
        public double SecondsPer96 { get; private set; }

        /// <summary>
        /// How long each beat is in seconds.
        /// </summary>
        public double SecondsPerBeat { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the start of the platback to the current time.
        /// </summary>
        public void Start()
        {
            StartTime = DateTime.Now;
        }

        /// <summary>
        /// Updates the the elapsed time, so all implants get the 
        /// exact save time values at each step.
        /// </summary>
        public void Mark()
        {
            _elapsed = ElapsedActual;
        }

        /// <summary>
        /// Returns a string with simple debugging information for the 
        /// current time object.
        /// </summary>
        public override string ToString()
        {
            return string.Format("[{0:000}:{1:00}:{2:000}] = {3:000}:{4:00.000}", Measure, Beat, ThousandsthBeat, ElapsedMinutes, ElapsedSeconds);
        }

        #endregion

        #region Private


        /// <summary>
        /// Updates the number of seconds it takes to elapse different
        /// measurements used in beat calculations that are based on
        /// tempo.
        /// </summary>
        private void UpdateDurations()
        {
            // prevent devide by zero by treating tempo 0 as 0.0001
            var t = Tempo;
            if (t == 0) t = 0.0001;

            // calculate times based on tempo
            SecondsPerBeat = 60.0 / Tempo;
            SecondsPer96 = SecondsPerBeat / 24.0;
        }

        /// <summary>
        /// Storage for current tempo.
        /// </summary>
        private double _tempo = -1f;

        /// <summary>
        /// Storage for marked time intervals.
        /// </summary>
        private TimeSpan _elapsed = TimeSpan.Zero;

        #endregion
    }
}
