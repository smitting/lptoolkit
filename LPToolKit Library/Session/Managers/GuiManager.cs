using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.GUI;
using LPToolKit.Core;
using LPToolKit.Core.Tasks.ImplantEvents;

namespace LPToolKit.Session.Managers
{  
    /// <summary>
    /// Manages sending GuiPaint events to each implant in the write
    /// order so they can make calls to the GUI to draw themselves.
    /// </summary>
    public class GuiManager : SessionManagerBase
    {
        #region Constructors

        public GuiManager(UserSession parent)
            : base(parent)
        {
            Context = new GuiContext(1024, 768); // first gen iPad resolution

        }

        #endregion

        #region Properties

        public readonly GuiContext Context;

        #endregion

        #region Methods

        /// <summary>
        /// Sends a paint event to all implants to update the screen.
        /// </summary>
        public void Render()
        {
            var e = new GuiPaintImplantEvent();
            foreach (var implant in Parent.Implants.Running)
            {
                implant.Trigger(e);
            }

            // TODO: deal with the drawing and such here too
        }

        #endregion

    }
}
