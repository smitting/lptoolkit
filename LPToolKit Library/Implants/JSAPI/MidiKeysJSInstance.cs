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
    /// This provides javascript with access to read and write MIDI 
    /// data coming from a standard musical keyboard.
    /// </summary>
    public class MidiKeysJSInstance : ImplantEventBaseJSInstance
    {
        #region Constructors

        public MidiKeysJSInstance(ObjectInstance prototype)
            : base(prototype)
        {
        }

        #endregion

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


        #endregion
    }
}
