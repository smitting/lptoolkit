using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.GUI.Layouts
{
    /// <summary>
    /// Draws controls vertically.
    /// </summary>
    public class VLayout : GuiLayout
    {
        #region Methods

        /// <summary>
        /// Layouts out all of the contained controls vertically.
        /// </summary>
        public override void Render(GuiContext c)
        {
            Width = 0;
            Height = 0;
            c.PushXY();
            foreach (var control in Controls.ToArray())
            {
                // adjust position for padding
                c.MoveTo(c.X, c.Y + control.Settings.TopPadding);
                c.PushXY();
                c.MoveTo(c.X + control.Settings.LeftPadding, c.Y);

                // save layout size changes
                if (control.TotalWidth > Width)
                {
                    Width = control.TotalWidth;
                }
                Height += control.TotalHeight;
                
                // render and save extends for mouse events
                control.Render(c);
                control.LastRenderPosition.X = c.X;
                control.LastRenderPosition.Y = c.Y;
                control.LastRenderPosition.Width = control.Width;
                control.LastRenderPosition.Height = control.Height;

                // move to the next control's location
                c.PopXY();
                c.MoveTo(c.X, c.Y + control.Height + control.Settings.BottomPadding);
            }
            c.PopXY();
        }

        #endregion
    }
}
