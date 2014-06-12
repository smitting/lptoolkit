using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using LPToolKit.GUI.Renderers;

namespace LPToolKit.GUI.Controls
{
    /// <summary>
    /// This wraps an object that renders to a bitmap, such as the
    /// sequence renderer, within a GuiControl.
    /// </summary>
    public class RendererWrapper : GuiControl
    {
        #region Constructors

        /// <summary>
        /// Constructor requires the object that will be wrapped.
        /// </summary>
        public RendererWrapper(GuiRenderer rendererer)
        {
            Rendererer = rendererer;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The object being wrapped.
        /// </summary>
        public readonly GuiRenderer Rendererer;

        #endregion

        #region Methods

        /// <summary>
        /// Copies the renderered image to the GuiContext.
        /// </summary>
        public override void Render(GuiContext c)
        {
            // should we assume its already rendered?
            //Rendererer.Render();

            var r = new Rectangle();
            r.X = c.X;
            r.Y = c.Y;
            r.Width = Width;
            r.Height = Height;

            var g = c.GetGraphics();
            g.DrawImageUnscaledAndClipped(Rendererer.Image, r);
        }

        #endregion
    }
}
