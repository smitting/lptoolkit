using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using LPToolKit.MIDI;
using LPToolKit.GUI;
using LPToolKit.Sequences;

using System.Windows.Forms;

namespace LPToolKit.GUI.Renderers
{

    /// <summary>
    /// The different behaviors possible for editing notes in a
    /// clip when the mouse is moving.
    /// </summary>
    public enum ClipMouseAction
    {
        None,
        MultiSelect,
        MovingSingle,
        MovingGroup,
        New, // TODO: same as just resize
        Resize
    }

    /// <summary>
    /// Maintains the relative movement of a mouse.
    /// </summary>
    public class MousePoint
    {
        #region Properties

        public int X { get; private set; }
        public int Y { get; private set; }
        public int StartX { get; private set; }
        public int StartY { get; private set; }
        public int DeltaX { get { return X - StartX; } }
        public int DeltaY { get { return Y - StartY; } }

        /// <summary>
        /// Gets set to true if at any point since the last call to
        /// SetStart() the value of IsDrag became true, so you can 
        /// tell the difference between dragging away and then back
        /// to the original position versus never moving the mouse
        /// between the down and up events.
        /// </summary>
        public bool WasDrag { get; private set; }

        /// <summary>
        /// Returns true iff the current movement is enough to be 
        /// considered a drag, which currently requires movement
        /// of at least 3 pixels in either direction.
        /// </summary>
        public bool IsDrag
        {
            get { return (Math.Abs(DeltaX) > 3 || Math.Abs(DeltaY) > 3); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the current mouse position.
        /// </summary>
        public void Set(int x, int y)
        {
            X = x;
            Y = y;
            if (WasDrag == false)
            {
                WasDrag = IsDrag;
            }
        }

        /// <summary>
        /// Updates the starting mouse position.
        /// </summary>
        public void SetStart(int x, int y)
        {
            StartX = x;
            StartY = y;
            WasDrag = false;
        }

        #endregion

    }

    public class MidiMath
    {
        public static int Quantize(int number, int by)
        {
            return (number / by) * by;
        }

        /// <summary>
        /// Rounds to the closest instead of the floor
        /// </summary>
        public static int QuantizeNearest(int number, int by)
        {
            return (int)(Math.Round((float)number / by)) * by;
        }
    }



    /// <summary>
    /// All data associated with using a mouse or touch screen to
    /// manipulate notes on a clip.
    /// </summary>
    public class ClipUIHandler : IGuiInputHandler
    {
        #region Constructors

        public ClipUIHandler()
        {
            Mode = ClipEditModes.Music;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Current area in mouse coordinates that we should respond 
        /// to.  By default, all areas are responded to, which is
        /// indicated with a width of 0.
        /// </summary>
        public Rectangle ActiveArea = new Rectangle(0, 0, 0, 0);

        /// <summary>
        /// The clip this handler is for.
        /// </summary>
        public SequencerClip Clip;

        /// <summary>
        /// The object drawing this clip.
        /// </summary>
        public SequenceRenderer Renderer;

        /// <summary>
        /// Another view related to this view.  For example, this view could
        /// be an entire clip and used to select the visible area within
        /// a zoomed in rendering attached to this property.
        /// </summary>
        public SequenceRenderer ParentRenderer = null;

        /// <summary>
        /// The current action being taken
        /// </summary>
        public ClipMouseAction Action = ClipMouseAction.None;

        /// <summary>
        /// Where the mouse is versus where it was.
        /// </summary>
        public MousePoint MousePosition = new MousePoint();

        /// <summary>
        /// The original values before the start of this action.
        /// </summary>
        public Dictionary<SequencerClip.Note, SequencerClip.Note> Original = new Dictionary<SequencerClip.Note, SequencerClip.Note>();

        /// <summary>
        /// Controls which mouse handler is in use.
        /// </summary>
        public ClipEditModes Mode
        {
            get
            {
                lock (_modeLock)
                {
                    return ClipEditMode.ToEnum(_mode);
                }
            }
            set
            {
                lock (_modeLock)
                {
                    _mode = ClipEditMode.FromEnum(value, this);
                }
            }
        }

        #endregion


        #region Methods

        /// <summary>
        /// Replaces all of the values in the Original property.
        /// </summary>
        public void UpdateOriginal(params SequencerClip.Note[] notes)
        {
            Original.Clear();
            foreach (var note in notes)
            {
                if (note != null)
                {
                    Original.Add(note, note.Clone());
                }
            }
        }

        public void KeyDown(Keys keyCode)
        {
            lock (_modeLock)
            {
                _mode.KeyDown(keyCode);
            }
        }
        

        public void MouseDown(int x, int y, Keys modifierKeys = Keys.None)
        {
            lock (_modeLock)
            {
                //if (ActiveArea.Width <= 0 || ActiveArea.Contains(x, y))
                {
                    _mode.MouseDown(x, y, modifierKeys);
                }
            }
        }

        public void MouseMove(int x, int y)
        {
            lock (_modeLock)
            {
                //if (ActiveArea.Width <= 0 || ActiveArea.Contains(x, y))
                {
                    _mode.MouseMove(x, y);
                }
            }
        }

        public void MouseUp(int x, int y)
        {
            lock (_modeLock)
            {
                //if (ActiveArea.Width <= 0 || ActiveArea.Contains(x, y))
                {
                    _mode.MouseUp(x, y);
                }
            }
        }

        #endregion

        #region Protected

        private ClipEditMode _mode = null;
        protected readonly object _modeLock = new object();

        #endregion

        #region Private

        #endregion
    }

}
