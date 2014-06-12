using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace LPToolKit.GUI.Controls
{
    /// <summary>
    /// Puts in a space as an alternative to padding.
    /// </summary>
    internal class SpacerControl : GuiControl
    {
        /// <summary>
        /// Draws nothing.
        /// </summary>
        public override void Render(GuiContext c)
        {           
        }
    }
}
