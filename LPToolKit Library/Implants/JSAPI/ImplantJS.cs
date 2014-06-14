using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jurassic;
using Jurassic.Library;
using LPToolKit.Core;
using LPToolKit.Core.Tasks.ImplantEvents;

namespace LPToolKit.Implants.JSAPI
{
    /// <summary>
    /// Jurassic instance class for the "this" instance sent to the
    /// implant javascript.  Each instance contains a reference to
    /// each of the other native code javascript objects used by
    /// implants.
    /// 
    /// Note: currently these are assigned via javascript in the
    /// system folder.
    /// </summary>
    public class ImplantJSInstance : ImplantEventBaseJSInstance
    {
        public ImplantJSInstance(ObjectInstance prototype)
            : base(prototype)
        {
            //this.PopulateFunctions();
            //this.PopulateFields();
        }

        ~ImplantJSInstance()
        {
            Destroy();
        }

        #region Javascript Properties

        /// <summary>
        /// Access to all lighted button interfaces connected.
        /// </summary>
        [JSProperty]
        public PadJSInstance pads { get; set; }

        /// <summary>
        /// Access to all knobs and faders for connected devices.
        /// </summary>
        [JSProperty]
        public KnobJSInstance knobs { get; set; }

        /// <summary>
        /// Access to MIDI I/O from standard music keyboards.
        /// </summary>
        [JSProperty]
        public MidiKeysJSInstance keys { get; set; }

        /// <summary>
        /// Access to read and write OSC values.
        /// </summary>
        [JSProperty]
        public OscJSInstance osc { get; set; }

        /// <summary>
        /// Access for controlling the tablet GUI and receiving events.
        /// </summary>
        [JSProperty]
        public GuiJSInstance gui { get; set; }

        /// <summary>
        /// Access to timing a beat synchronization.
        /// </summary>
        [JSProperty]
        public TimeJSInstance time { get; set; }

        /// <summary>
        /// Access to the settings for the current implant instance.
        /// </summary>
        [JSProperty]
        public SettingsJsInstance settings { get; set; }

        /// <summary>
        /// Shares data between all instances of this implant type.
        /// </summary>
        [JSProperty]
        public StaticJSInstance shared { get; set;}

        /// <summary>
        /// Access to data that is stored as the current "session" and
        /// can be reloaded next time this set of settings is loaded again.
        /// </summary>
        [JSProperty]
        public SessionJSInstance session { get; set; }

        /// <summary>
        /// Access to the current mode that the hardware is in, allowing
        /// for the same hardware to be used for different concepts in
        /// the same session.
        /// </summary>
        [JSProperty]
        public ModesJSInstance mode { get; set; }

        /// <summary>
        /// Gets the mode this implant is active on.
        /// </summary>
        [JSProperty]
        public int assignedMode
        {
            get { return Parent.AssignedMode; }
        }

        /// <summary>
        /// Returns the ID of this cluster node.
        /// </summary>
        [JSProperty]
        public string clusterNodeId
        {
            get { return Core.Cluster.ClusterNode.Local.Name; }
        }

        #endregion

        #region Javascript Methods

        /// <summary>
        /// Registers an event callback.
        /// </summary>
        [JSFunction(Name = "on")]
        public void On(string eventName, FunctionInstance fn)
        {
            Callbacks.Add(new EventCallback() { EventName = eventName, Callback = fn });
        }

        /// <summary>
        /// Writes to the console using the same formatting as Console.WriteLine().
        /// TODO: All console functions need to be passed bia the IImplantApp interface
        /// so that GUI applications can provide a custom inteface for accessing
        /// console information, possibly segregating messages by implant for debugging.
        /// </summary>
        [JSFunction(Name = "print")]
        public void Print(string format, params object[] args)
        {
            string s;
            if (args == null)
            {
                s = format;
            }
            else
            {
                try 
                {
                    s = string.Format(format, args);
                }
                catch
                {
                    s = format;
                }
            }
            Session.UserSession.Current.Console.Add(s + "\n", Parent.ImplantType.Name);
            //Console.WriteLine(s);
        }

        /// <summary>
        /// Prints a message and waits for the user to hit enter.
        /// </summary>
        [JSFunction(Name = "wait")]
        public void Wait()
        {
            Session.UserSession.Current.Console.Add("Press enter to continue javascript\n", Parent.ImplantType.Name);
            // TODO: some how get this from the host app            
            Console.ReadLine();
        }

        /// <summary>
        /// Simulation of standard javascript method.
        /// </summary>
        [JSFunction(Name = "setInterval")]
        public int SetInterval(FunctionInstance fn, int delayMs)
        {
            return Intervals.Current.Add(this, fn, delayMs);
        }

        /// <summary>
        /// Stops a repeating function using the number returned by 
        /// setInterval().
        /// </summary>
        [JSFunction(Name = "clearInterval")]
        public void ClearInterval(int id)
        {
            Intervals.Current.Remove(id);
        }

        #endregion

        #region Properties


        #endregion

        #region Methods

        /// <summary>
        /// Releases all resources used by this object.
        /// Currently that's just setInterval() stuff.
        /// </summary>
        public void Destroy()
        {
            Intervals.Current.RemoveAll(this);
        }

        /// <summary>
        /// Passes on an event to the appropriate javascript methods.
        /// Out of range events will be simply ignored.
        /// </summary>
        public void Trigger(ImplantEvent e)
        {
            var jsEvent = e.Convert();

            // if this event type has a target, trigger the event and record the destination
            ImplantEventBaseJSInstance target = GetEventTarget(e);
            if (target != null)
            {
                target.Trigger(jsEvent);
                e.LogDestination(Parent.GetSourceName());         
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// Returns the instance of the object that should receive
        /// a particular event, or null if none should.
        /// </summary>
        private ImplantEventBaseJSInstance GetEventTarget(ImplantEvent e)
        {
            if (e is Clock96ImplantEvent)
            {
                return time;
            }
            else if (e is NoteImplantEvent)
            {
                return keys;
            }
            else if (e is PadImplantEvent)
            {
                return pads;
            }
            else if (e is KnobImplantEvent)
            {
                return knobs;
            }
            else if (e is OscImplantEvent)
            {
                return osc;
            }
            else if (e is GuiImplantEvent)
            {
                return gui;
            }
            else if (e is ModeChangeImplantEvent)
            {
                return mode;
            }
            else if (e is DeviceChangeImplantEvent)
            {
                return this;
            }
            /*
            switch (e.EventType)
            {
                case ImplantEventType.GuiPaint:
                    return gui;
                case ImplantEventType.NoteOn:
                case ImplantEventType.NoteOff:
                    return keys;
                case ImplantEventType.KnobChange:
                    return knobs;
                case ImplantEventType.PadPress:
                case ImplantEventType.PadRelease:
                case ImplantEventType.PadDoubleClick:
                    return pads;
                case ImplantEventType.DeviceChange:
                    return this;
                case ImplantEventType.ModeChange:
                    return mode;
                case ImplantEventType.Clock96:
                    return time;
                case ImplantEventType.OscMessage:
                    return osc;
            }
             */
            return null;
        }

        #endregion
    }
}
