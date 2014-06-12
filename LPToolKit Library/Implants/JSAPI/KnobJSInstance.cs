using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jurassic;
using Jurassic.Library;

namespace LPToolKit.Implants.JSAPI
{
    /// <summary>
    /// This provides javascript with access to read (and some day write)
    /// from hardware with faders and knobs such as the LaunchControl.
    /// </summary>
    public class KnobJSInstance : ImplantEventBaseJSInstance
    {
        public KnobJSInstance(ObjectInstance prototype)
            : base(prototype)
        {
        }

        #region Javascript Properties

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

        #endregion

        #region Methods

        #endregion

        #region Private

        /// <summary>
        /// Called when the Parent is assigned to the base class.
        /// </summary>
        protected override void ParentChanged()
        {
            // TODO: need a way to read in launchcontrol knob changes?
            // or should that now we handled by ImplantEvent?
        }

        #endregion
    }
}
