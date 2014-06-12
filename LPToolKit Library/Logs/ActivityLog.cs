using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.Logs
{
    /// <summary>
    /// This is a log showing the activity from the source of an 
    /// input event, through the event handler in an implant triggering
    /// and output event which is sent to a target.  The result is
    /// a graph of all activity in the system, with latency measurements.
    /// 
    /// TODO: this should probably be a session manager called ActivityManager
    /// </summary>
    public class ActivityMonitor
    {
        /// <summary>
        /// The record of all activity since that last time the list
        /// was cleared.
        /// </summary>
        public List<ActivityLog> Items = new List<ActivityLog>();
    }

    /// <summary>
    /// One flattened record in the activity monitor.  Some of the nodes
    /// are empty to demonstrate that a previous node caused multiple
    /// output events to happen from a single input event.
    /// </summary>
    public class ActivityLog : LogBase
    {
        /// <summary>
        /// When this log was created, for calculating the DeltaMsec properties.
        /// </summary>
        public DateTime CreatedOn = DateTime.Now;

        /// <summary>
        /// The original event.
        /// </summary>
        public ActivityNode Source;

        /// <summary>
        /// The ImplantEvent after processing for the implant.
        /// </summary>
        public ActivityNode Input;
        
        /// <summary>
        /// The implant that processed the event.
        /// </summary>
        public ActivityNode Implant;

        /// <summary>
        /// The output event from the implant.
        /// </summary>
        public ActivityNode Output;

        /// <summary>
        /// The final output delivered from the system.
        /// </summary>
        public ActivityNode Target;
    }

    /// <summary>
    /// One node within the activity monitor.
    /// TODO: this needs to be efficient and optionally be able to
    /// be recorded in some sort of a database.
    /// </summary>
    public struct ActivityNode
    {
        /// <summary>
        /// The heading for this entry.
        /// </summary>
        public string Head;

        /// <summary>
        /// The main text for this entry.
        /// </summary>
        public string Text;

        /// <summary>
        /// The milliseconds since the source event.
        /// </summary>
        public int DeltaMsec;
    }
}
