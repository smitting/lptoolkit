using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jurassic;
using Jurassic.Library;
using LPToolKit.Core;
using LPToolKit.Core.Tasks.ImplantEvents;

namespace LPToolKit.Implants.JSAPI
{
    /// <summary>
    /// The argument sent to all event handler callbacks.
    /// </summary>
    public class EventJSInstance : ObjectInstance
    {
        public EventJSInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFunctions();
            this.PopulateFields();
        }

        /// <summary>
        /// The type of event this is, such as press or release
        /// </summary>
        [JSProperty]
        public string eventName { get; set; }

        /// <summary>
        /// The x location of the pad or knob, when applicable.
        /// </summary>
        [JSProperty]
        public int x { get; set; }

        /// <summary>
        /// The y location of the pad or knob, when applicable.
        /// </summary>
        [JSProperty]
        public int y { get; set; }

        /// <summary>
        /// The new value at this location, when not a pad.
        /// </summary>
        [JSProperty]
        public int value { get; set; }

        /// <summary>
        /// The OSC address for OSC messages.
        /// </summary>
        [JSProperty]
        public string address { get; set; }

        /// <summary>
        /// Data along with OSC messages.
        /// </summary>
        [JSProperty]
        public string values { get; set; }


        public override string ToString()
        {
            return string.Format("[{0} x={1},y={2},value={3}]", x, y, value);
        }


        /// <summary>
        /// Gets the event name for javascript from an implant event instance.
        /// </summary>
        internal static string GetJSEventName(ImplantEvent e)
        {
            if (e is Clock96ImplantEvent)
            {
                return "1/96";
            }
            if (e is KnobChangeImplantEvent)
            {
                return "change";
            }
            if (e is PadPressImplantEvent)
            {
                return "press";
            }
            if (e is PadReleaseImplantEvent)
            {
                return "release";
            }
            if (e is PadDoubleClickImplantEvent)
            {
                return "doublepress";
            }
            if (e is DeviceChangeImplantEvent)
            {
                return "devicechange";
            }
            if (e is ModeChangeImplantEvent)
            {
                return "modechanged";
            }
            if (e is GuiPaintImplantEvent)
            {
                return "paint";
            }
            if (e is NoteOnImplantEvent)
            {
                return "noteon";
            }
            if (e is NoteOffImplantEvent)
            {
                return "noteoff";
            }
            if (e is OscImplantEvent)
            {
                return "change";
            }
            return "unknown";
        }

/*
        /// <summary>
        /// Gets the event name for javascript from an implant event type.
        /// </summary>
        internal static string GetJSEventName(ImplantEventType type)
        {
            switch (type)
            {
                case ImplantEventType.Clock96:      
                    return "1/96";
                case ImplantEventType.KnobChange:   
                    return "change";
                case ImplantEventType.PadPress: 
                    return "press";
                case ImplantEventType.PadRelease:
                    return "release";
                case ImplantEventType.PadDoubleClick:
                    return "doublepress";
                case ImplantEventType.DeviceChange:
                    return "devicechange";
                case ImplantEventType.ModeChange:
                    return "modechanged";
                case ImplantEventType.GuiPaint:
                    return "paint";
                case ImplantEventType.NoteOn:
                    return "noteon";
                case ImplantEventType.NoteOff:
                    return "noteoff";
                case ImplantEventType.OscMessage:
                    return "change";
            }
            return "unknown";
        }
 */
    }
}
