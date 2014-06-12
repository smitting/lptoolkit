using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.GUI.Layouts
{
    /// <summary>
    /// Draws controls horizontally.
    /// </summary>
    public class HLayout : GuiLayout
    {
        #region Methods

        /// <summary>
        /// Lays out all of the contained controls horizontally.
        /// </summary>
        public override void Render(GuiContext c)
        {
            Width = 0;
            Height = 0;
            c.PushXY();
            foreach (var control in Controls.ToArray())
            {
                // adjust position for padding
                c.MoveTo(c.X + control.Settings.LeftPadding, c.Y);
                c.PushXY();
                c.MoveTo(c.X, c.Y + control.Settings.TopPadding);

                // save layout's size
                if (control.TotalHeight > Height)
                {
                    Height = control.TotalHeight;
                }
                Width += control.TotalWidth;
                
                // render and save the extents for mouse events
                control.Render(c);
                control.LastRenderPosition.X = c.X;
                control.LastRenderPosition.Y = c.Y;
                control.LastRenderPosition.Width = control.Width;
                control.LastRenderPosition.Height = control.Height;
                
                // move to the next control's position
                c.PopXY();
                c.MoveTo(c.X + control.Width + control.Settings.RightPadding, c.Y);
            }
            c.PopXY();
        }

        #endregion
    }
}
