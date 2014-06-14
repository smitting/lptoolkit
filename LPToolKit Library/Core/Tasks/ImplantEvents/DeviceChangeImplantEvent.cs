using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.Core.Tasks.ImplantEvents
{

    public class DeviceChangeImplantEvent : ImplantEvent
    {
        public DeviceChangeImplantEvent()
        {
            //EventType = ImplantEventType.DeviceChange;
        }

        public MIDI.MappedMidiDevice Mapping;

        public override T Clone<T>()
        {
            var ret = base.Clone<T>() as DeviceChangeImplantEvent;
            ret.Mapping = Mapping;
            return ret as T; 
        }

        public override ImplantEvent Clone()
        {
            return Clone<DeviceChangeImplantEvent>() as ImplantEvent;
        }
    }
}
