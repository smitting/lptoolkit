using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.Core.Tasks.ImplantEvents
{
    /// <summary>
    /// Base class for the different note implant events
    /// </summary>
    public abstract class NoteImplantEvent : ImplantEvent
    {
        public override int ExpectedLatencyMsec
        {
            get
            {
                return 25;
            }
        }

        public override Implants.JSAPI.EventJSInstance Convert()
        {
            return base.Convert();
        }
    }

    public class NoteOnImplantEvent : NoteImplantEvent
    {
        public NoteOnImplantEvent()
        {
            //EventType = ImplantEventType.NoteOn;
        }


        public override ImplantEvent Clone()
        {
            return Clone<NoteOnImplantEvent>();
        }
    }

    public class NoteOffImplantEvent : NoteImplantEvent
    {
        public NoteOffImplantEvent()
        {
            //EventType = ImplantEventType.NoteOff;
        }


        public override ImplantEvent Clone()
        {
            return Clone<NoteOffImplantEvent>();
        }
    }
}
