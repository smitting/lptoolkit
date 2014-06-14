using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.Core.Tasks.ImplantEvents
{

    public class ModeChangeImplantEvent : ImplantEvent
    {
        public ModeChangeImplantEvent()
        {
         //   EventType = ImplantEventType.ModeChange;
        }

        public override ImplantEvent Clone()
        {
            return Clone<ModeChangeImplantEvent>();
        }
    }

}
