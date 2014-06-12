using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.GUI.Renderers
{
    /// <summary>
    /// Arguments passed along with render events.
    /// </summary>
    public class GuiRenderEventArgs
    {
        public GuiRenderer Renderer;
    }

    /// <summary>
    /// Event triggered whenever a GuiRenderer has changed its
    /// output bitmap.
    /// </summary>
    public delegate void GuiRenderEventHandler(object sender, GuiRenderEventArgs e);
}
