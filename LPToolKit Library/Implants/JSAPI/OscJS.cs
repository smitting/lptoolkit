using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jurassic;
using Jurassic.Library;
using LPToolKit.OSC;
using LPToolKit.Session;
using LPToolKit.Core.Tasks.ImplantEvents;

namespace LPToolKit.Implants.JSAPI
{

    /// <summary>
    /// Javascript class that provides all support for OSC messaging 
    /// for server implants.  By default, one or more OSC values are
    /// provided for each implant.  As these values are changed, they
    /// are automatically send to all connected OSC devices.  These
    /// values are accessed from the javascript by x and y coordinate,
    /// but these coordinates are ignored if they are not specified
    /// in their OSC Format settings, like /fader/{x}
    /// 
    /// Raw OSC messages will also be supported.
    /// </summary>
    public class OscJSInstance : ImplantEventBaseJSInstance
    {
        
        public OscJSInstance(ObjectInstance prototype)
            : base(prototype)
        {            
        }

        #region Javascript Properties

        /// <summary>
        /// The format to use for OSC messages, just as /fader/{x}
        /// </summary>
        [Obsolete("Implants will no longer have a set format.  they will register their messages so the user can remap as desired")]
        [JSProperty]
        public string Format { get; set; }

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
        /// Sets a value via the x and y coordinate.
        /// </summary>
        [JSFunction(Name = "set")]
        public void SetValue(int x, int y, double value)
        {
            var key = OSCMapping.Format(Format, x, y);
            Parent.Session.OSC.FromImplant(Parent, key, value);
        }
        
        /// <summary>
        /// Lets the implant specific that it will be sending or receiving and 
        /// particular OSC address.  Later features will allow this address to
        /// automatically to be changed to something else by the user.
        /// </summary>
        [JSFunction(Name = "register")]
        public void Register(string address)
        {
            Parent.Session.Console.Add("Implant registered " + address, Parent.GetSourceName());
        }

        /// <summary>
        /// Sends an OSC message from the implant by its address and
        /// an array of doubles.
        /// </summary>
        [JSFunction(Name = "send")]
        public void SendMessage(string address, params object[] values)
        {
            var osc = new OscDataMessage();
            osc.Address = address;
            if (values.Length > 0 && values[0] is ArrayInstance)
            {
                var array = values[0] as ArrayInstance;                
                osc.Values = new double[array.Length];
                for (var i = 0; i < array.Length; i++)
                {
                    double.TryParse(array[i].ToString(), out osc.Values[i]);
                }
            }
            osc.Source = Parent.GetSourceName();
            Parent.Session.OSC.Add(osc);
        }

        #endregion
    }
}
