using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.GUI.Controls
{

    /// <summary>
    /// A control that sets a value between a minimum and maximum
    /// like a knob or fader.
    /// </summary>
    public abstract class ValueControl : GuiControl
    {
        #region Properties

        /// <summary>
        /// The allowed range for this control.
        /// </summary>
        public double MinValue, MaxValue;

        /// <summary>
        /// The current value for this control.
        /// </summary>
        public double Value;

        /// <summary>
        /// Set to true whenever the GUI caused a value change
        /// </summary>
        public bool GuiChangedValue = false;


        /// <summary>
        /// The size of the range between the values.
        /// </summary>
        public double Range
        {
            get { return MaxValue - MinValue; }
        }

        /// <summary>
        /// The percentage of the range between 0 and 1.
        /// </summary>
        public double Percent
        {
            get { return Math.Max(Math.Min((Value - MinValue) / Range, 1), 0); }
        }

        #endregion
    }
}
