using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.MIDI;
using LPToolKit.MIDI.Hardware;

namespace LPToolKit.Core.Tasks
{
    /// <summary>
    /// A MIDI message that will eventually be delivered to a MIDI
    /// output device by the kernel.
    /// </summary>
    public class MidiAction : MonitoredKernelTask
    {
        #region Properties

        /// <summary>
        /// The message received.
        /// </summary>
        public MidiMessage Message;

        /// <summary>
        /// The device this message will be delivered to.
        /// </summary>
        public MidiDriver Driver;

        #endregion

        #region IKernalTask Implementation


        /// <summary>
        /// Schedules this task with the kernel.
        /// </summary>
        public override IKernelTask ScheduleTask()
        {
            Kernel.Current.Add(this);
            return this;
        }

        /// <summary>
        /// Called by the scheduler when it wants this task to run.
        /// </summary>
        public override void RunTask()
        {
            if (Driver != null)
            {
                Driver.Send(Message);
            }
        }

        /// <summary>
        /// The number of milliseconds into the future to set the 
        /// contractual completion time when not explicitly set.
        /// </summary>
        public override int ExpectedLatencyMsec 
        {
            get { return 25; }
        }

        public override string ToString()
        {
            return string.Format("[MidiAction Message={0} Driver={1}]", Message, Driver.Hardware.Name);
        }

        #endregion
    }

    /// <summary>
    /// Midi clock messages are VERY sensitive to waiting to be sent.
    /// </summary>
    public class MidiClockAction : MidiAction
    {
        public override int ExpectedLatencyMsec
        {
            get
            {
                return -1;
            }
        }
    }



    /// <summary>
    /// An output MIDI message that will change some indicator or a 
    /// device (such as a light on a launchpad) rather than say some
    /// note data.  These messages are not considered to be important
    /// enough to be given real-time priority.
    /// </summary>
    public class MidiIndicatorAction : MidiAction
    {
        public override int ExpectedLatencyMsec
        {
            //get { return int.MaxValue; }
            get { return 80000; }
        }

        /// <summary>
        /// Schedules this task with the kernel.
        /// </summary>
        public override IKernelTask ScheduleTask()
        {
            Kernel.Current.Add(this);
            return this;
        }

        /// <summary>
        /// Called by the scheduler when it wants this task to run.
        /// </summary>
        public override void RunTask()
        {
            if (Driver != null)
            {
                Driver.Send(Message);
            }
        }


        public override string ToString()
        {
            return string.Format("[MidiIndicatorAction Message={0} Driver={1}]", Message, Driver.Hardware.Name);
        }
    }
}
