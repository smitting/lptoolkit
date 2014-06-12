using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jurassic;
using Jurassic.Library;
using LPToolKit;
using LPToolKit.LaunchPad;
using LPToolKit.MIDI.Pads;
using LPToolKit.MIDI.Hardware;
using LPToolKit.Core.Tasks;

namespace LPToolKit.Implants.JSAPI
{
    /// <summary>
    /// This provides javascript with access to hardware that uses lit buttons.
    /// </summary>
    /// <remarks>
    /// This replaces the previous LaunchPadJSInstance class.
    /// </remarks>
    public class PadJSInstance : ImplantEventBaseJSInstance
    {
        #region Constructors

        public PadJSInstance(ObjectInstance prototype)
            : base(prototype)
        {
        }

        #endregion

        #region Javascript Properties

        /// <summary>
        /// The width of the enabled pad region.
        /// </summary>
        [JSProperty]
        public int width
        {
            get { return Parent.ActiveArea.TotalWidth; }
        }

        /// <summary>
        /// The height of the enabled pad region.
        /// </summary>
        [JSProperty]
        public int height
        {
            get { return Parent.ActiveArea.TotalHeight; }
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

        /// <summary>
        /// Sets a color of a pad at the selected location
        /// </summary>
        [JSFunction(Name = "set")]
        public void Set(int x, int y, string name)
        {
            new PadSetColorImplantAction()
            {
                Source = Parent,
                X = x,
                Y = y,
                Color = name
            }.ScheduleTask();
        }

        /// <summary>
        /// Changes the scroll position of the virtual grid.
        /// </summary>
        [JSFunction(Name="scrollTo")]
        public void ScrollTo(int x, int y)
        {
            new PadScrollToImplantAction()
            {
                Source = Parent,
                X = x,
                Y = y
            }.ScheduleTask();
        }

        // TODO: add get()

        #endregion


        #region Properties


        #endregion

        #region Methods


        #endregion
    }
}
