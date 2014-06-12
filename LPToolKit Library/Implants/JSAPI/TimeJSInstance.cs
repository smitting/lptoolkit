using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jurassic;
using Jurassic.Library;

namespace LPToolKit.Implants.JSAPI
{
    /// <summary>
    /// This provides javascript with access to current time and 
    /// beat synchronization information.  
    /// </summary>
    /// <remarks>
    /// This is synchronized by all software in the current cluster 
    /// and can be used to synchronize audio software against it.
    /// The actual 1/96 event is generated in the UserSession.
    /// </remarks>
    public class TimeJSInstance : ImplantEventBaseJSInstance
    {
        public TimeJSInstance(ObjectInstance prototype)
            : base(prototype)
        {
        }

        #region Javascript Properties

        // TODO: might be nice to have access to time here outside of events?


        [JSProperty]
        public int tempo
        {
            get { return (int)Parent.BeatSync.Tempo; }
            set { Parent.BeatSync.Tempo = value; }
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

        #region Properties

        #endregion

        #region Methods


        #endregion
    }
}
