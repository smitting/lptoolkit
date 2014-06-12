using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace LPToolKit.MIDI.Pads
{

    internal class ButtonColors
    {

        public static ButtonColor Off = new ButtonColor(0, 0, 0, 4);
        public static ButtonColor Red = new ButtonColor(3, 0, 0, 4);
        public static ButtonColor RedOrange = new ButtonColor(3, 1, 0, 4);
        public static ButtonColor Orange = new ButtonColor(3, 2, 0, 4);
        public static ButtonColor Amber = new ButtonColor(3, 3, 0, 4);
        public static ButtonColor Yellow = new ButtonColor(2, 3, 0, 4);
        public static ButtonColor YellowGreen = new ButtonColor(1, 3, 0, 4);
        public static ButtonColor Green = new ButtonColor(0, 3, 0, 4);
    }

    /// <summary>
    /// Helper class for representing a generic color as its components,
    /// providing assistance to quantize values into levels supported
    /// by a specific hardware.
    /// </summary>
    internal class ButtonColor
    {
        #region Constructors

        /// <summary>
        /// Default constructor optionally allows setting QuantizeLevel.
        /// </summary>
        public ButtonColor(int levels = 256)
            : this(0, 0, 0, levels)
        {
        }

        /// <summary>
        /// Constructor accepting RGB text color and optionally QuantizeLevel.
        /// </summary>
        public ButtonColor(string text, int levels = 256)
        {
            QuantizeLevel = levels;
            Set(text);
        }

        /// <summary>
        /// Constructor accepting the quantized RGB colors, say say
        /// a level of 3 with quantize of 4 is 255.
        /// </summary>
        public ButtonColor(int r, int g, int b, int levels = 256)
        {
            QuantizeLevel = levels;
            SetQuantized(r, g, b);
        }

        #endregion

        #region Properties

        public string Text { get; private set; }
        public int Red { get; private set; }
        public int Green { get; private set; }
        public int Blue { get; private set; }
        public int QuantizeLevel { get; private set; }
        public int QRed { get; private set; }
        public int QGreen { get; private set; }
        public int QBlue { get; private set; }

        public double QuantizeFactor
        {
            get { return 256.0 / (double)(QuantizeLevel - 1); }
        }
        #endregion

        #region Methods

        public void Set(string text)
        {
            int r, g, b;

            var named = GetNamedColor(text);
            if (named != null)
            {
                r = named.Red;
                g = named.Green;
                b = named.Blue;
            }
            else
            {
                Parse(text, out r, out g, out b);
            }
            Set(r, g, b);
        }

        public void SetQuantized(int r, int g, int b)
        {
            Set((int)((double)r * QuantizeFactor), (int)((double)g * QuantizeFactor), (int)((double)b * QuantizeFactor));
        }

        public void Set(int r, int g, int b)
        {
            Red = r;
            Green = g;
            Blue = b;
            Quantize();
            Text = string.Format("#{0:X2}{1:X2}{2:X2}", r, g, b);
        }

        private static ButtonColor GetNamedColor(string namedColor)
        {
            if (namedColor == null)
            {
                return ButtonColors.Off;
            }
            switch (namedColor.ToLower().Trim())
            {
                case "off": return ButtonColors.Off;
                case "red": return ButtonColors.Red;
                case "redorange": return ButtonColors.RedOrange;
                case "orange": return ButtonColors.Orange;
                case "amber": return ButtonColors.Amber;
                case "yellow": return ButtonColors.Yellow;
                case "yellowgreen": return ButtonColors.YellowGreen;
                case "green": return ButtonColors.Green;
            }
            return null;
        }

        #endregion

        #region Private

        /// <summary>
        /// Sets the quantized values.
        /// </summary>
        private void Quantize()
        {
            if (QuantizeLevel == 256)
            {
                QRed = Red;
                QGreen = Green;
                QBlue = Blue;
            }
            else
            {
                QRed = Quantize(Red);
                QGreen = Quantize(Green);
                QBlue = Quantize(Blue);
            }

            Red = Clip(Red, 0, 255);
            Green = Clip(Green, 0, 255);
            Blue = Clip(Blue, 0, 255);
            QRed = Clip(QRed, 0, QuantizeLevel - 1);
            QGreen = Clip(QGreen, 0, QuantizeLevel - 1);
            QBlue = Clip(QBlue, 0, QuantizeLevel - 1);
        }

        /// <summary>
        /// Quantizes one double to an integer using the Quantize factor.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private int Quantize(double value)
        {
            return (int)Math.Ceiling((double)value / QuantizeFactor);
        }

        /// <summary>
        /// Clips a value within its allowed range.
        /// </summary>
        private static int Clip(int value, int min, int max)
        {
            if (value > max)
            {
                return max;
            }
            else if (value < min)
            {
                return min;
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// Parses an HTML color into its r/g/b components.
        /// </summary>
        private static void Parse(string color, out int r, out int g, out int b)
        {
            string text = color;
            if (text.StartsWith("#"))
            {
                text = text.Substring(1);
            }
            if (text.Length == 6)
            {
                r = ParseHex(text, 0, 2);
                g = ParseHex(text, 2, 2);
                b = ParseHex(text, 4, 2);
            }
            else if (text.Length == 3)
            {
                r = ParseHex(text, 0, 1) * 16;
                g = ParseHex(text, 1, 1) * 16;
                b = ParseHex(text, 2, 1) * 16;
            }
            throw new Exception("Color is not in a recognized format: '" + color + "'");
        }

        /// <summary>
        /// Parses a single hex number from a string.
        /// </summary>
        private static int ParseHex(string hex, int start = -1, int length = 2)
        {
            if (start > 0)
            {
                hex = hex.Substring(start);
            }
            hex = hex.Substring(0, length);

            int color;
            if (int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out color))
            {
                return color;
            }
            else
            {
                return 0;
            }
        }

        #endregion
    }
}
