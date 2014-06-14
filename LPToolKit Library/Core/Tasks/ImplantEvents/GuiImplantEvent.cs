using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.Core.Tasks.ImplantEvents
{
    /// <summary>
    /// Base class for the different gui implant events
    /// </summary>
    public abstract class GuiImplantEvent : ImplantEvent
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

    public class GuiPaintImplantEvent : GuiImplantEvent
    {
        public GuiPaintImplantEvent()
        {
            //EventType = ImplantEventType.GuiPaint;
        }


        public override ImplantEvent Clone()
        {
            return Clone<GuiPaintImplantEvent>();
        }
    }
}
