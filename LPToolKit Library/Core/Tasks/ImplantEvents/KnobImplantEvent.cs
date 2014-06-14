using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.Core.Tasks.ImplantEvents
{
    /// <summary>
    /// Base class for the different knob implant events
    /// </summary>
    public abstract class KnobImplantEvent : ImplantEvent
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

    public class KnobChangeImplantEvent : KnobImplantEvent
    {
        public KnobChangeImplantEvent()
        {
            //EventType = ImplantEventType.KnobChange;
        }


        public override ImplantEvent Clone()
        {
            return Clone<KnobChangeImplantEvent>();
        }
    }
}
