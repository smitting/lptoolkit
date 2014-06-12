using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using LPToolKit.GUI.Layouts;

namespace LPToolKit.GUI.Controls
{
    /// <summary>
    /// Base class for elements displayed within the session GUI.
    /// </summary>
    public abstract class GuiControl
    {
        #region Constructors

        #endregion

        #region Properties

        /// <summary>
        /// The last context to draw this control.
        /// </summary>
        public GuiContext Parent;

        /// <summary>
        /// Object assigned for dealing with user input for this control.
        /// </summary>
        public IGuiInputHandler InputHandler;

        /// <summary>
        /// The boundaries of this control the last time it was 
        /// rendered.  This is needed to know where to route
        /// mouse events.
        /// </summary>
        public Rectangle LastRenderPosition = new Rectangle();

        /// <summary>
        /// Unique ID for this element.
        /// </summary>
        public string ID;

        /// <summary>
        /// The size of this element.
        /// </summary>
        public int Width
        {
            get { return Settings.Width; }
            set { Settings.Width = value;  }
        }

        public int Height
        {
            get { return Settings.Height; }
            set { Settings.Height = value; }
        }

        public int TotalWidth
        {
            get { return Width + Settings.LeftPadding + Settings.RightPadding; }
        }

        public int TotalHeight
        {
            get { return Height + Settings.TopPadding + Settings.RightPadding; }
        }


        public GuiLayoutOption Settings = new GuiLayoutOption();

        #endregion

        #region Methods

        /// <summary>
        /// Draws this control, or if it's a container, all of the
        /// nested controls.
        /// </summary>
        public abstract void Render(GuiContext c);

        #endregion
    }

    public enum GuiControlType
    {
        Knob,
        VerticalFader,
        HorizontalFader
    }

    

}
