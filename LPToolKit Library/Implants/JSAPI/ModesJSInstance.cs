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
    /// Provides a javascript with a way to control and detect changes
    /// in the current session's mode.  The mode controls what implants
    /// currently have access to the physical hardware, such as switching
    /// between a virtual keyboard and a drum sequencer.
    /// </summary>
    public class ModesJSInstance : ImplantEventBaseJSInstance
    {
        public ModesJSInstance(ObjectInstance prototype)
            : base(prototype)
        {
            //this.PopulateFunctions();
            //this.PopulateFields();
        }


        #region Javascript Properties

        /// <summary>
        /// Gets or sets the current mode for the user session, which
        /// is a number from 0 to the number modes.
        /// </summary>
        [JSProperty]
        public int current 
        {
            get { return Parent.Session.Modes.CurrentMode; }
            set { Parent.Session.Modes.CurrentMode = value; } 
        }

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
    }
}
