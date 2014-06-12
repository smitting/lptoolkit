using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using LPToolKit.GUI.Controls;
using LPToolKit.GUI.Layouts;
using LPToolKit.GUI.Util;
using LPToolKit.GUI.Renderers;

using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.IO;

namespace LPToolKit.GUI
{
    /// <summary>
    /// Interface for GuiControls that can accept user input, either
    /// via the mouse, keyboard, or multitouch.
    /// </summary>
    public interface IGuiInputHandler
    {
        void KeyDown(Keys keyCode);
        void MouseDown(int x, int y, Keys modifierKeys);
        void MouseMove(int x, int y);
        void MouseUp(int x, int y);
    }


    /// <summary>
    /// Simple X/Y object.
    /// </summary>
    public class XY
    {
        public int X, Y;
    }

    /// <summary>
    /// The types of layouts available.
    /// </summary>
    public enum LayoutType
    {
        VLayout,
        HLayout
    }

    /// <summary>
    /// The object that GUIs are rendered to, which gets translated to
    /// any images and scripts needed to be rendered to an appropriate
    /// device.
    /// </summary>
    public class GuiContext
    {
        #region Constructors

        /// <summary>
        /// Constructor accepts the size to make the bitmap this 
        /// context will be written to.
        /// </summary>
        public GuiContext(int w, int h)
        {
            Resize(w, h);
            _controls = new ControlRegistration(this);
        }

        #endregion

        #region User Input Methods (TODO: move to another class?)

        /// <summary>
        /// Allows a control to ask to continue to receive mouse 
        /// events outside its draw area.
        /// </summary>
        public void GrabMouse(GuiControl c)
        {
            if (_grabbedMouse.Contains(c) == false)
            {
                _grabbedMouse.Add(c);
            }
        }

        /// <summary>
        /// Stops sending mouse events outside the area requested
        /// by GramMouse.
        /// </summary>
        public void ReleaseMouse(GuiControl c)
        {
            _grabbedMouse.Remove(c);
        }

        /// <summary>
        /// Tells the UI that it needs to redraw itself.
        /// </summary>
        public void Invalidate()
        { 
            if (NeedsRenderered != null)
            {
                NeedsRenderered(this, new GuiRenderEventArgs() { Renderer = null });
            }
        }

        /// <summary>
        /// Routes the mouse message to the appropriate controls
        /// based on position and current state.
        /// </summary>
        public void MouseDown(int x, int y, Keys modifiedKeys)
        {
            foreach (var c in GetControlAt(x, y))
            {
                c.InputHandler.MouseDown(x, y, modifiedKeys);
            }
        }

        /// <summary>
        /// Routes the mouse message to the appropriate controls
        /// based on position and current state.
        /// </summary>
        public void MouseUp(int x, int y)
        {
            foreach (var c in GetControlAt(x, y))
            {
                c.InputHandler.MouseUp(x, y);
            }
        }

        /// <summary>
        /// Routes the mouse message to the appropriate controls
        /// based on position and current state.
        /// </summary>
        public void MouseMove(int x, int y)
        {
            foreach (var c in GetControlAt(x, y))
            {
                c.InputHandler.MouseMove(x, y);
            }
        }

        /// <summary>
        /// Routes the key message to the appropriate controls
        /// based on position and current state.
        /// </summary>
        public void KeyDown(Keys keyCode)
        {
            // TODO: need to determine which controls should get this key event
            foreach (var v in _controls.All)
            {
                if (v.InputHandler != null)
                {
                    v.InputHandler.KeyDown(keyCode);
                }
            }            
        }

        #endregion

        #region Colors

        /// <summary>
        /// The color that controls should base their highlights on.
        /// </summary>
        public Color BaseColor = Color.FromArgb(255, 116, 66);

        public Color Gray5 = Color.FromArgb(128, 119, 115);
        public Color Gray4 = Color.FromArgb(90, 89, 88);
        public Color Gray3 = Color.FromArgb(73, 70, 69);
        public Color Gray2 = Color.FromArgb(53, 53, 53);
        public Color Gray1 = Color.FromArgb(38, 36, 35);

        #endregion

        #region Properties

        /// <summary>
        /// Event triggered whenever the context knows it needs to be
        /// drawn again.
        /// </summary>
        public GuiRenderEventHandler NeedsRenderered;

        /// <summary>
        /// Where the next control will be drawn.
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// Where the next control will be drawn.
        /// </summary>
        public int Y { get; private set; }

        /// <summary>
        /// The width of the drawing area of this context.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// The height of the drawing area of this context.
        /// </summary>
        public int Height { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Changes the size of this object.
        /// TODO: notify controls of this change.
        /// </summary>
        public void Resize(int w, int h)
        {
            var bmp = new Bitmap(w, h);

            lock (_imagelock)
            {
                Width = w;
                Height = h;
                _renderbitmap = bmp;
            }
            // TODO: need to notify someone to redraw this
        }
        
        /// <summary>
        /// Draws all of the 
        /// </summary>
        /// <returns></returns>
        public Bitmap Render()
        {
            // reset the control ids
            nextLayoutId = 1;
            nextValueId = 1;

            X = 0;
            Y = 0;

            lock (_imagelock)
            {
                var g = GetGraphics();

                // draw background
                var backBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, Height), Color.FromArgb(95, 95, 95), Color.FromArgb(68, 68, 68));
                g.FillRectangle(backBrush, 0, 0, Width, Height);
                if (_topLayout != null)
                {
                    _topLayout.Render(this);
                }

                // make copy of bitmap to avoid locking
                // TODO: do something more efficient
                return new Bitmap(_renderbitmap);
            }
        }

        /// <summary>
        /// Returns the last full rendering of the GUI as a PNG file
        /// encoded as base64.
        /// </summary>
        public string GetLastRenderAsBase64()
        {
            lock (_imagelock)
            {
                string base64;
                using (var ms = new MemoryStream())
                {
                    _renderbitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    base64 = Convert.ToBase64String(ms.ToArray());
                }
                return base64;
            }
        }

        #endregion

        #region Control Drawing Methods

        /// <summary>
        /// Saves the current x/y coordinate
        /// </summary>
        public void PushXY()
        {
            _xyStack.Push(new XY() { X = X, Y = Y });
        }

        /// <summary>
        /// Restores the last saved x/y coordinate.
        /// </summary>
        public void PopXY()
        {
            var xy = _xyStack.Pop();
            X = xy.X;
            Y = xy.Y;
        }

        /// <summary>
        /// Moves where the next control will be drawn.
        /// </summary>
        public void MoveTo(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Returns a drawing object for the range requested.
        /// </summary>
        public Graphics GetGraphics()
        {
            lock (_imagelock)
            {
                return Graphics.FromImage(_renderbitmap);
            }
        }

        #endregion

        #region Adding Controls Methods

        /// <summary>
        /// Starts a layout of the specified type.
        /// </summary>
        public void BeginLayout(LayoutType layoutType)
        {
            var newLayout = GetLayout(layoutType);
            if (_currentLayout != null)
            {
                _layoutStack.Push(_currentLayout);
                _currentLayout.Controls.Add(newLayout);
            }
            else
            {
                _topLayout = newLayout;
            }
            _currentLayout = newLayout;
        }

        /// <summary>
        /// Ends the layout regardless of type.
        /// </summary>
        public void EndLayout()
        {
            _currentLayout = (_layoutStack.Count > 0) ? _layoutStack.Pop() : null;
        }

        /// <summary>
        /// Adds a space of a given width and height to the current layout.
        /// </summary>
        public void Spacer(int width, int height, params GuiLayoutOption[] options)
        {
            GetControl<SpacerControl>(new GuiLayoutOption() { Width = width, Height = height }.Combine(options));
        }

        /// <summary>
        /// Adds a knob to the current layout.  This is build Unity3D
        /// style so every control needs to be build in the same order
        /// to get the ID.  Any controls applied to the GUI will result
        /// in the return value being changed.
        /// </summary>
        public double Knob(double value, double minValue, double maxValue, params GuiLayoutOption[] options)
        {
            var control = GetValueControl(
                GuiControlType.Knob,
                value,
                minValue,
                maxValue,
                new GuiLayoutOption() { Width = 32, Height = 32 }.Combine(options)
                );
            return control.Value;
        }

        /// <summary>
        /// Adds a vertical fadaer to the current layout.
        /// </summary>
        public double VerticalFader(double value, double minValue, double maxValue, params GuiLayoutOption[] options)
        {
            var control = GetValueControl(
                GuiControlType.VerticalFader,
                value,
                minValue,
                maxValue,
                new GuiLayoutOption() { Width = 32, Height = 32 }.Combine(options)
                );
            return control.Value;
        }

        /// <summary>
        /// Adds a horizontal fadaer to the current layout.
        /// </summary>
        public double HorizontalFader(double value, double minValue, double maxValue, params GuiLayoutOption[] options)
        {
            var control = GetValueControl(
                GuiControlType.HorizontalFader,
                value,
                minValue,
                maxValue,
                new GuiLayoutOption() { Width = 32, Height = 32 }.Combine(options)
                );
            return control.Value;
        }

        /// <summary>
        /// Draws a subclass of GuiRenderer at the current location.
        /// </summary>
        public void AddRenderer(GuiRenderer renderer, IGuiInputHandler handler, params GuiLayoutOption[] options)
        {
            var control = GetControl<RendererWrapper>(
                new GuiLayoutOption() { Width = 96, Height = 64 }.Combine(options),
                () => { return new RendererWrapper(renderer); }
                );
            control.InputHandler = handler;
        }
      
        #endregion

        #region Private

        /// <summary>
        /// Where image data is written to.
        /// </summary>
        private Bitmap _renderbitmap;

        /// <summary>
        /// Mutex for access to _renderbitmap.
        /// </summary>
        private readonly object _imagelock = new object();

        /// <summary>
        /// Stack of saved positions.
        /// </summary>
        private Stack<XY> _xyStack = new Stack<XY>();

        /// <summary>
        /// Returns the gui control that was drawn at a given x,y 
        /// coordinate during the last render.  This is so we can
        /// route mouse and touch events accordingly.  Only returns
        /// if the discovered control has an input handler.
        /// </summary>
        private List<GuiControl> GetControlAt(int x, int y)
        {
            // just use those that grabbed that mouse if available
            if (_grabbedMouse.Count > 0)
            {
                return _grabbedMouse.ToList();
            }

            var list = _controls.GetAtXY(x, y);
            List<GuiControl> ret = new List<GuiControl>();
            foreach (var c in list)
            {
                if (c is GuiLayout) continue;
                if (c.InputHandler != null)
                {
                    ret.Add(c);
                }
            }
            return ret;
        }

        /// <summary>
        /// Either gets or creates the layout of the given type for
        /// the current layout order we're on.
        /// </summary>
        private GuiLayout GetLayout(LayoutType type)
        {
            var id = "Layout" + (nextLayoutId++);
            var layout = _controls.Get<GuiLayout>(id, () => { return type == LayoutType.VLayout ? new VLayout() as GuiLayout : new HLayout() as GuiLayout; });
            layout.Controls.Clear();
            return layout;
        }

        /// <summary>
        /// Gets a control using a default constructor when creating
        /// controls that don't exist yet.
        /// </summary>
        private T GetControl<T>(GuiLayoutOption settings) where T : GuiControl, new()
        {
            return GetControl<T>(settings, () => { return new T(); });
        }

        /// <summary>
        /// Either gets or creates the control of the given type for
        /// the current control number we're on. 
        /// </summary>
        private T GetControl<T>(GuiLayoutOption settings, ControlRegistration.ConstructorMethod create) where T : GuiControl
        {
            var id = "Control" + (nextValueId++);
            var control = _controls.Get<T>(id, create);
            control.Settings = settings;
            _currentLayout.Controls.Add(control);
            return control;
        }

        /// <summary>
        /// Either gets or creates the value control object with the 
        /// given settings.
        /// </summary>
        private ValueControl GetValueControl(GuiControlType type, double value, double minValue, double maxValue, GuiLayoutOption settings)
        {
            // controls are only created the first time to avoid object churning
            var id = "ValueControl" + (nextValueId++);
            var control = _controls.Get<ValueControl>(id, () =>
            {
                switch (type)
                {
                    case GuiControlType.Knob:
                        return new KnobControl();
                    case GuiControlType.HorizontalFader:
                        return new FaderControl(FaderControl.FaderType.Horizontal);
                    case GuiControlType.VerticalFader:
                        return new FaderControl(FaderControl.FaderType.Vertical);
                }
                return null;
            });

            // values that are always set to the implant's input
            control.Settings = settings;
            control.MinValue = minValue;
            control.MaxValue = maxValue;

            // allow the implant one chance to grab a changed gui value
            if (control.GuiChangedValue)
            {
                control.GuiChangedValue = false;
            }
            else
            {
                control.Value = value;
            }

            // add control to current layout
            _currentLayout.Controls.Add(control);
            return control;
        }

        private GuiLayout _topLayout = null;
        private GuiLayout _currentLayout = null;
        private Stack<GuiLayout> _layoutStack = new Stack<GuiLayout>();


        private int nextLayoutId = 1;
        private int nextValueId = 1;

        /// <summary>
        /// Controls that have grabbed the mouse.
        /// </summary>
        private List<GuiControl> _grabbedMouse = new List<GuiControl>();

        /// <summary>
        /// Registration of all known GuiControl objects for this instance.
        /// </summary>
        private readonly ControlRegistration _controls;
          
        #endregion
    }
}
