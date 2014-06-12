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
    /// Base class for ObjectInstances with a Parent reference to 
    /// the c# instance of the implant.
    /// </summary>
    public abstract class ImplantBaseJSInstance : ObjectInstance
    {
        public ImplantBaseJSInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFunctions();
            this.PopulateFields();
        }

        public JavascriptImplant Parent
        {
            get { return _parent; }
            set { _parent = value; ParentChanged(); }
        }

        /// <summary>
        /// Called whenever parent property is changed.
        /// </summary>
        protected virtual void ParentChanged()
        {
        }

        private JavascriptImplant _parent;
    }

    /// <summary>
    /// Base class for javascript native-code instances that provide
    /// a javascript on() function for receiving event events.
    /// TODO: also include the Trigger() function here
    /// </summary>
    public abstract class ImplantEventBaseJSInstance : ImplantBaseJSInstance
    {
        #region Constructors

        public ImplantEventBaseJSInstance(ObjectInstance prototype)
            : base(prototype)
        {
            //this.PopulateFunctions();
            //this.PopulateFields();
            
        }

        #endregion

        #region Javascript Methods


        #endregion

        #region Properties

        /// <summary>
        /// Registration of events for this instance.
        /// </summary>
        public class EventCallback
        {
            /// <summary>
            /// The name of the event registered.
            /// </summary>
            public string EventName;

            /// <summary>
            /// The optional arguments for this event.
            /// </summary>
            public string Arguments;

            /// <summary>
            /// The function to call for this.
            /// </summary>
            public FunctionInstance Callback;
        }

        /// <summary>
        /// All events currently registered with this implant instance.
        /// </summary>
        public List<EventCallback> Callbacks = new List<EventCallback>();

        #endregion

        #region Methods

        /// <summary>
        /// Calls all appropriate callbacks on this instance.
        /// </summary>
        internal void Trigger(EventJSInstance e)
        {
            // prevent any event from firing when not active and running
            // TODO: do these checks at a lower level
            if (!Parent.Active) return;
            if (!Parent.Running) return;


            try
            {
                foreach (var cb in Callbacks.ToArray())
                {
                    if (cb.EventName == e.eventName)
                    {
                        cb.Callback.Call(e, e);
                    }
                }
            }
            catch (Exception ex)
            {
                Parent.HandleJavascriptError(ex);
            }
        }

        #endregion
    }

}
