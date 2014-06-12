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
    /// Control with a knob that turns to set a value.
    /// </summary>
    internal class KnobControl : ValueControl, IGuiInputHandler
    {
        #region IGuiInputHandler Implementation

        public void KeyDown(Keys keyCode)
        {

        }
        
        public void MouseDown(int x, int y, Keys modifierKeys)
        {
            if (Parent != null)
            {
                Parent.GrabMouse(this);
            }
            mouseDown = true;
            startY = y;
            startX = x;
        }
        
        public void MouseMove(int x, int y)
        {
            if (mouseDown)
            {
                var valuePerPixel = (this.MaxValue - this.MinValue) / 100.0;

                var deltaY = -(y - startY);
                var deltaX = x - startX;
                startY = y;
                startX = x;
                Value += (deltaY + deltaX) * valuePerPixel;
                if (Value > MaxValue)
                {
                    Value = MaxValue;
                }
                else if (Value <MinValue)
                {
                    Value = MinValue;
                }
                GuiChangedValue = true;
                if (Parent != null)
                {
                    Parent.Invalidate();
                }
            }
        }

        public void MouseUp(int x, int y)
        {
            if (Parent != null)
            {
                Parent.ReleaseMouse(this);
            }
            mouseDown = false;
        }

        #endregion

        #region Properties

        public Color LineColor = Color.FromArgb(255, 255, 255);
        public Color OuterBorderColor = Color.FromArgb(53, 53, 53);
        public Color InnerBorderColor1 = Color.FromArgb(128, 119, 115);
        public Color InnerBorderColor2 = Color.FromArgb(82, 77, 75);
        public Color KnobColor1 = Color.FromArgb(90, 89, 88);
        public Color KnobColor2 = Color.FromArgb(71, 69, 69);
        public Color BandColor1 = Color.FromArgb(73, 70, 69);
        public Color BandColor2 = Color.FromArgb(38, 36, 35);
        
        public Color ValueColor
        {
            get { return Parent.BaseColor; }
        }

        public Color ValueBandColor
        {
            get { return Color.FromArgb(20, ValueColor); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Draws a white circle 3px thick with a line representing
        /// the current value, with 1/4 of the circle at the bottom
        /// not used, with a curve around it from the start point
        /// to the current point in yellow.
        /// </summary>
        public override void Render(GuiContext c)
        {
            var testbmp = new Bitmap(Width, Height);

            //var g = Graphics.FromImage(testbmp);
            var g = c.GetGraphics();

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // get different radiuses
            var fullRadius = Width / 2.0;
            var lineStartRadius = fullRadius * 0.25;
            var lineEndRadius = fullRadius * 0.55;

            // get theta of value
            var theta0 = Math.PI * (-0.25);
            var theta1 = Math.PI * 1.25;
            var theta = theta0 + ((theta1 - theta0) * (1-Percent));

            // get line sizes
            float valuePenWidth = (float)Width * 0.12F;
            float linePenWidth = (float)Width * 0.04F;

            // shadow positions
            float shadowOffset1 = Width * 0.035F;
            float shadowOffset2 = Width * 0.025F;


            // calculate important points
            var midpoint = new PointF(c.X + Width / 2, c.Y + Height / 2);
            var fullRect = new RectangleF(c.X, c.Y, Width, Height);
            var borderRect = new RectangleF(c.X + valuePenWidth / 2, c.Y + valuePenWidth / 2, Width - valuePenWidth, Height - valuePenWidth);
            var bandCenterRect = new RectangleF(c.X + valuePenWidth, c.Y + valuePenWidth, Width - valuePenWidth * 2, Height - valuePenWidth * 2);
            var knobRect = new RectangleF(c.X + valuePenWidth * 1.5F, c.Y + valuePenWidth * 1.5F, Width - valuePenWidth * 3F, Height - valuePenWidth * 3F);
            var knobBorderRect = new RectangleF(knobRect.X + 1, knobRect.Y + 1, knobRect.Width -2, knobRect.Height -2);

            var shawdowRect = new RectangleF(fullRect.X + 2, fullRect.Y + 5, fullRect.Width, fullRect.Height);


            // draw the outer band
            g.FillEllipse(new LinearGradientBrush(borderRect, BandColor1, BandColor2, LinearGradientMode.Vertical), borderRect);

            // draw the outline for the outer band
            g.DrawEllipse(new Pen(OuterBorderColor, 2), borderRect);
            //g.DrawEllipse(new Pen(InnerBorderColor1, 3), knobBorderRect);
            //g.DrawEllipse(new Pen(InnerBorderColor2, 5), knobBorderRect2);

            // draw the knob
            g.FillEllipse(new LinearGradientBrush(knobRect, KnobColor1, KnobColor2, LinearGradientMode.Vertical), knobRect);
            g.DrawArc(new Pen(InnerBorderColor1, 1), knobBorderRect, 210, 120);
            g.DrawArc(new Pen(InnerBorderColor2, 2), knobBorderRect, 30, 120);


            
            // draw the value arc
            float arcLength = (float)Percent * 270;
            g.DrawArc(new Pen(ValueColor, valuePenWidth), fullRect, 135, arcLength);
            g.DrawArc(new Pen(ValueBandColor, valuePenWidth), bandCenterRect, 135, arcLength);

            // draw the line pointing at the value
            var lineStartRel = new PointF((float)(Math.Cos(theta) * lineStartRadius), (float)(Math.Sin(theta) * lineStartRadius));
            var lineEndRel = new PointF((float)(Math.Cos(theta) * lineEndRadius), (float)(Math.Sin(theta) * lineEndRadius));
            g.DrawLine(new Pen(LineColor, linePenWidth), midpoint.X + lineStartRel.X, midpoint.Y - lineStartRel.Y, midpoint.X + lineEndRel.X, midpoint.Y - lineEndRel.Y);



            // label test
            var font = new Font("arial", 8);

            g.DrawString("Knob", font, new SolidBrush(Color.White), c.X + Width / 2, c.Y + Height, new StringFormat() { Alignment = StringAlignment.Center });
        }

        public static double RadiansToDegrees(double radians)
        {
            return radians * Rad2DegFactor;
        }
        
        #endregion

        #region Private

        private const double Rad2DegFactor = 180.0 / Math.PI;

        private bool mouseDown = false;
        private int startY = 0;
        private int startX = 0;

        #endregion
    }
}
