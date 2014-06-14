using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.OSC;

namespace LPToolKit.Core.Tasks.ImplantEvents
{
    /// <summary>
    /// Event triggered when an implant needs to be notified about
    /// an OSC Message.
    /// </summary>
    public class OscImplantEvent : ImplantEvent
    {

        public OscImplantEvent()
        {
            //EventType = ImplantEventType.OscMessage;
        }

        /// <summary>
        /// The message being sent to an implant.
        /// </summary>
        public OscDataMessage Osc;

        /// <summary>
        /// Includes OSC data in the conversion.
        /// </summary>
        public override Implants.JSAPI.EventJSInstance Convert()
        {
            var ret = base.Convert();
            if (Osc != null)
            {
                ret.values = string.Join(",", Osc.Values);
                ret.address = Osc.Address;
            }
            return ret;
        }

        public override T Clone<T>()
        {
            var ret = base.Clone<T>() as OscImplantEvent;
            ret.Osc = Osc;
            return ret as T;
        }

        public override ImplantEvent Clone()
        {
            return Clone<OscImplantEvent>() as ImplantEvent;
        }
    }
}
