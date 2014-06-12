using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.Core.Tasks.ImplantEvents
{

    public class Clock96ImplantEvent : ImplantEvent
    {
        public Clock96ImplantEvent()
        {
            EventType = ImplantEventType.Clock96;
        }

        public override int ExpectedLatencyMsec
        {
            get
            {
                return 25;
            }
        }

    }
}
