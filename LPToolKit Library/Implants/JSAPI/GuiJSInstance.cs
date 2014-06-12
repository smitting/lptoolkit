using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jurassic;
using Jurassic.Library;
using LPToolKit.Session;
using LPToolKit.GUI;

namespace LPToolKit.Implants.JSAPI
{
    /// <summary>
    /// This provides all access for an implant to control the GUI 
    /// displayed on a tablet or laptop screen.  Instead of granting
    /// direct access to a web app, all controls will be specified
    /// using a layout system similar to Java or Unity, which will
    /// allow implants to support any future user interface option
    /// like a native mobile app instead of a web app without
    /// any changes the the implant javascript.
    /// </summary>
    public class GuiJSInstance : ImplantEventBaseJSInstance
    {
        public GuiJSInstance(ObjectInstance prototype)
            : base(prototype)
        {
            //this.PopulateFunctions();
            //this.PopulateFields();
        }


        #region Javascript Methods

        /// <summary>
        /// Registers an event callback.
        /// </summary>
        [JSFunction(Name = "on")]
        public void On(string eventName, FunctionInstance fn)
        {
            Callbacks.Add(new EventCallback() { EventName = eventName, Callback = fn });
        }


        // TODO: not sure if these should be here or on the event, as
        // they only work correctly during the paint event
        [JSFunction(Name = "beginHorizontal")]
        public void BeginHorizontal()
        {
            Context.BeginLayout(LayoutType.HLayout);            
        }

        [JSFunction(Name = "endHorizontal")]
        public void EndHorizontal()
        {
            Context.EndLayout();
        }

        [JSFunction(Name = "beginVertical")]
        public void BeginVertical()
        {
            Context.BeginLayout(LayoutType.VLayout);
        }

        [JSFunction(Name = "endVertical")]
        public void EndVertical()
        {
            Context.EndLayout();
        }

        [JSFunction(Name = "knob")]
        public double Knob(double value, double minValue, double maxValue)
        {
            // TODO: options as JSON data
           
            return Context.Knob(value, minValue, maxValue);
        }

        [JSFunction(Name = "spacer")]
        public void Spacer(int size)
        {
            Context.Spacer(size, size);
        }

        [JSFunction(Name = "verticalFader")]
        public double VerticalFader(double value, double minValue, double maxValue)
        {
            return Context.VerticalFader(value, minValue, maxValue);

        }

        [JSFunction(Name = "repaint")]
        public void Repaint()
        {
            Context.Invalidate();
        }
        


        #endregion

        #region Properties

        public GuiContext Context
        {
            get { return Parent.Session.Gui.Context; }
        }

        #endregion

        #region Methods


        #endregion
    }
}
