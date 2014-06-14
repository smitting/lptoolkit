using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.Core.Tasks.ImplantEvents
{
    /// <summary>
    /// Base class for the different pad implane events
    /// </summary>
    public abstract class PadImplantEvent : ImplantEvent
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

    public class PadPressImplantEvent : PadImplantEvent
    {
        public PadPressImplantEvent()
        {
            //EventType = ImplantEventType.PadPress;
        }

        public override ImplantEvent Clone()
        {
            return Clone<PadPressImplantEvent>();
        }
    }

    public class PadReleaseImplantEvent : PadImplantEvent
    {
        public PadReleaseImplantEvent()
        {
            //EventType = ImplantEventType.PadRelease;
        }

        public override ImplantEvent Clone()
        {
            return Clone<PadReleaseImplantEvent>();
        }
    }

    public class PadDoubleClickImplantEvent : PadImplantEvent
    {
        public PadDoubleClickImplantEvent()
        {
        //    EventType = ImplantEventType.PadDoubleClick;
        }

        public override ImplantEvent Clone()
        {
            return Clone<PadDoubleClickImplantEvent>();
        }
    }


}
