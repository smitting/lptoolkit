using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.GUI.Controls;

namespace LPToolKit.GUI.Layouts
{
    /// <summary>
    /// Base class for objects responsible for controlling the position
    /// of controls.
    /// </summary>
    public abstract class GuiLayout : GuiControl
    {
        #region Constructors

        #endregion

        #region Properties

        /// <summary>
        /// The controls contained within this layout.
        /// </summary>
        public List<GuiControl> Controls = new List<GuiControl>();

        #endregion

        #region Methods


        #endregion
    }


}
