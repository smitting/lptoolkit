using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace LPToolKit.GUI.Controls
{

    /// <summary>
    /// A sliding fader type control.
    /// </summary>
    internal class FaderControl : ValueControl, IGuiInputHandler
    {

        public void KeyDown(Keys keyCode)
        {

        }

        private bool mouseDown = false;
        private int startY = 0;
        private int startX = 0;
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
                // get a percentage from last drawn area
                if (Type == FaderType.Horizontal)
                {
                    var p = (double)(x - this.LastRenderPosition.X) / (double)this.LastRenderPosition.Width;
                    var valueRange = MaxValue - MinValue;
                    Value = MaxValue - valueRange * p;

                    GuiChangedValue = true;
                    if (Parent != null)
                    {
                        Parent.Invalidate();
                    }
                }
                else if (Type == FaderType.Vertical)
                {
                    var p = (double)(y - this.LastRenderPosition.Y) / (double)this.LastRenderPosition.Height;
                    var valueRange = MaxValue - MinValue;
                    Value = MaxValue - valueRange * p;

                    GuiChangedValue = true;
                    if (Parent != null)
                    {
                        Parent.Invalidate();
                    }
                }

                if (Value > MaxValue)
                {
                    Value = MaxValue;
                }
                else if (Value < MinValue)
                {
                    Value = MinValue;
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

        public enum FaderType
        {
            Vertical,
            Horizontal
        }

        public FaderControl(FaderType type)
        {
            Type = type;
        }

        public FaderType Type;

        public override void Render(GuiContext c)
        {
            var box = new Rectangle(c.X, c.Y, Width, Height);

            var g = c.GetGraphics();

            // important locations (TODO: fix for horizontal)
            var fullRect = new RectangleF(c.X, c.Y, Width, Height);
            var faderAreaRect = new RectangleF(c.X, c.Y, Width / 2, Height);
            var slotRect = new RectangleF(faderAreaRect.X + faderAreaRect.Width / 2 - 3, faderAreaRect.Y, 6, faderAreaRect.Height);
            var valueAreaRect = new RectangleF(c.X + Width / 2, c.Y, Width / 2, Height); 

            // draw slot
            g.FillRectangle(new SolidBrush(Parent.Gray1), slotRect);
            g.DrawLine(new Pen(Parent.Gray5, 2), slotRect.X, slotRect.Y, slotRect.X + slotRect.Width, slotRect.Y);
            g.DrawLine(new Pen(Parent.Gray5, 2), slotRect.X, slotRect.Y, slotRect.X, slotRect.Y + slotRect.Height);

            // draw fader knob
            float knobHeight = faderAreaRect.Height / 10;

            RectangleF faderKnob = new RectangleF();            
            if (Type == FaderType.Vertical)
            {
                faderKnob = new RectangleF(faderAreaRect.X, faderAreaRect.Y + faderAreaRect.Height - (int)(Percent * (faderAreaRect.Height * 0.9)) - faderAreaRect.Height / 10, faderAreaRect.Width, faderAreaRect.Height / 10);
            }
            else if (Type == FaderType.Horizontal)
            {
                faderKnob = new RectangleF(faderAreaRect.X + (int)(Percent * (faderAreaRect.Width * 0.9)), faderAreaRect.Y, faderAreaRect.Width / 10, faderAreaRect.Height);
            }
            g.FillRectangle(new SolidBrush(Parent.BaseColor), faderKnob);


            // draw value
            var valuePen = new Pen(Parent.BaseColor, 5);
            if (Type == FaderType.Vertical)
            {
                float x0 = valueAreaRect.X + valueAreaRect.Width / 2;
                float y1 = valueAreaRect.Y + valueAreaRect.Height;
                float y0 = (float)(y1 - valueAreaRect.Height * Percent);
                g.DrawLine(valuePen, x0, y0, x0, y1);
            }


            // draw borders
            //var pen = new Pen(Color.White, 3);
            //  g.DrawRectangle(pen, box);
        }
    }
}
