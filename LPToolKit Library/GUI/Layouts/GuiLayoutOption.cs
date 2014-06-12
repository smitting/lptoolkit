using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.GUI.Layouts
{
    /// <summary>
    /// Options for controlling details on drawing GUI conttrols.
    /// </summary>
    public class GuiLayoutOption
    {
        #region Properties

        /// <summary>
        /// The size of the control without padding.
        /// </summary>
        public int Width = 0;

        /// <summary>
        /// The size of the control without padding.
        /// </summary>
        public int Height = 0;

        /// <summary>
        /// The gap to the left of the control.
        /// </summary>
        public int LeftPadding = 0;

        /// <summary>
        /// The gap to the right of the control.
        /// </summary>
        public int RightPadding = 0;

        /// <summary>
        /// The gap above the control.
        /// </summary>
        public int TopPadding = 0;

        /// <summary>
        /// The gap below the control.
        /// </summary>
        public int BottomPadding = 0;

        #endregion

        #region Methods

        /// <summary>
        /// Adds all of the objects to the current instance and 
        /// returns a reference to this instance.
        /// </summary>
        public GuiLayoutOption Combine(GuiLayoutOption[] options)
        {
            options.ToList().ForEach(i => Add(i));
            return this;
        }

        /// <summary>
        /// Adds one new option to the list.
        /// </summary>
        public void Add(GuiLayoutOption option)
        {
            if (option.Width != 0)
            {
                Width = option.Width;
            }
            if (option.Height != 0)
            {
                Height = option.Height;
            }
            if (option.LeftPadding != 0)
            {
                LeftPadding = option.LeftPadding;
            }
            if (option.RightPadding != 0)
            {
                RightPadding = option.RightPadding;
            }
            if (option.TopPadding != 0)
            {
                TopPadding = option.TopPadding;
            }
            if (option.BottomPadding != 0)
            {
                BottomPadding = option.BottomPadding;
            }
        }

        #endregion
    }


}
