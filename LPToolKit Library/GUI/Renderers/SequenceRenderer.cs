using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using LPToolKit.MIDI;
using LPToolKit.Sequences;

namespace LPToolKit.GUI.Renderers
{
    /// <summary>
    /// Draws a SequenceClip to a bitmap as a piano roll.
    /// </summary>
    public class SequenceRenderer : GuiRenderer
    {
        #region Constructors

        /// <summary>
        /// Constructor sets up default as a piano roll showing 12
        /// measures over 4 octaves starting at C2.
        /// </summary>
        public SequenceRenderer(Bitmap bmp) : base(bmp)
        {
            BackgroundType = SequencerBackgrounds.PianoRoll;

            // default size
            HeightInNotes = 12 * 4; // 4 octaves
            WidthInTicks = 12 * 96; // 12 measures
            FirstNote = 12 * 3;     // C2
        }

        #endregion

        #region Color Settings

        public Brush whiteKey = new SolidBrush(Color.FromArgb(103, 103, 103));
        public Brush blackKey = new SolidBrush(Color.FromArgb(84, 84, 84));
        public Pen grid1 = new Pen(Color.FromArgb(44, 44, 44));
        public Pen grid2 = new Pen(Color.FromArgb(66, 66, 66));
        public Pen grid3 = new Pen(Color.FromArgb(77, 77, 77));
        public Brush noteBrush = new SolidBrush(Color.Cyan);
        public Pen notePen = Pens.DarkCyan;


        public Brush highlightNoteBrush = new SolidBrush(Color.Red);
        public Pen highlightNotePen = Pens.DarkRed;

        public Pen selectionPen = new Pen(Color.Yellow);
        public Brush selectionBrush = new SolidBrush(Color.FromArgb(25, Color.Yellow));


        public Pen launchPadPen = new Pen(Color.Orange, 3);
        public Brush launchPadBrush = new SolidBrush(Color.FromArgb(50, Color.Orange));


        public Pen zoomPen = new Pen(Color.Blue, 3);
        public Brush zoomBrush = new SolidBrush(Color.FromArgb(50, Color.Blue));

        public Pen playCursorPen = new Pen(Color.Green, 1);
        public Brush playCursorBrush = new SolidBrush(Color.FromArgb(25, Color.Green));

        #endregion

        #region Properties

        /// <summary>
        /// Uses an enum to choose with background class to render with.
        /// </summary>
        public SequencerBackgrounds BackgroundType
        {
            get { return SequenceRendererBackground.ToEnum(_background); }
            set { lock (_backgroundLock) _background = SequenceRendererBackground.FromEnum(value); }
        }

        /// <summary>
        /// The clip to render.
        /// </summary>
        public SequencerClip Clip;

        /// <summary>
        /// Box to draw when in select mode.
        /// </summary>
        public Rectangle SelectBox;

        /// <summary>
        /// When true a box is shown where the mouse is currently 
        /// dragging to select notes.
        /// </summary>
        public bool ShowSelectBox = false;

        /// <summary>
        /// When true the area available on the launchpad is drawn.
        /// </summary>
        public bool ShowLaunchPad = true;

        /// <summary>
        /// The current tick where the play cursor is
        /// </summary>
        public int PlayCursor = 0;

        /// <summary>
        /// When true the play cursor is displayed
        /// </summary>
        public bool ShowPlayCursor = false;

        /// <summary>
        /// Draw box showing the visible area of a different view 
        /// within this view.
        /// </summary>
        public bool ShowZoom = false;

        /// <summary>
        /// The location of the box for the zoom area.
        /// </summary>
        public Rectangle ZoomBox;

        /// <summary>
        /// The first note to draw
        /// </summary>
        public int FirstNote
        {
            get { return TopNote - (int)HeightInNotes; }
            set { TopNote = value + (int)HeightInNotes; }
        }
            
            //= 12 * 3;     // C2

        public int TopNote { get; private set; }

        /// <summary>
        /// How much of the piano to show.
        /// </summary>
        public float HeightInNotes
        {
            get { return Image.Height / NoteHeight; }
            set { NoteHeight = (float)Image.Height / value; }
        }

        /// <summary>
        /// The offset into the clip to start drawing.
        /// </summary>
        public int FirstTick
        {
            get { return TargetTick; }
            set
            {
                TargetTick = value;
                Render();
            }
        }

        public int AnimationFirstTick
        {
            get
            {
                return TargetTick;
            }
        }


        public int TargetTick { get; private set; }
        
        /// <summary>
        /// How many notes to draw.
        /// </summary>
        public float WidthInTicks
        {
            get { return Image.Width / PixelsPerTick; }
            set { PixelsPerTick = Image.Width / value; }
        }

        /// <summary>
        /// Computed size of each tick.
        /// </summary>
        public float PixelsPerTick { get; private set; }

        /// <summary>
        /// Computed size of each note.
        /// </summary>
        public float NoteHeight { get; private set; }


        /// <summary>
        /// Alows the number of beats to be set.
        /// </summary>
        public int WidthInBeats 
        { 
            get
            {
                return (int)(WidthInTicks / 24);
            }
            set
            {
                WidthInTicks = value * 24;                
            }
        }

        /// <summary>
        /// Note to highlight in red
        /// </summary>
        public SequencerClip.Note SelectedNote = null;

        /// <summary>
        /// All selected notes to highlight in red.
        /// </summary>
        public List<SequencerClip.Note> SelectedNotes = null;

        #endregion

        #region Methods

        /// <summary>
        /// Changes the size of the output bitmap, and the size 
        /// settings so the result is the same pixel size as
        /// before.
        /// </summary>
        public void ResizeTo(int w, int h)
        {
            lock (_imageLock)
            {
                Image = new Bitmap(w, h);
            }
        }

        /// <summary>
        /// Returns the note at a given position or null if not found.
        /// </summary>
        public SequencerClip.Note GetNoteAt(int x, int y)
        {
            var tick = GetTickAt(x);
            var pitch = GetNoteAt(y);
            foreach (var note in Clip.Notes)
            {
                if (note.Tick > tick) return null;
                if (note.Pitch == pitch)
                {
                    if (note.Tick <= tick && note.Tick + note.Length > tick)
                    {
                        return note;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns how far through a note the given coord is as a
        /// percentage.  Useful to detect if we've grabed the start
        /// or the end of a note.
        /// </summary>
        public float GetPercentageThroughNoteAt(SequencerClip.Note note, int x)
        {
            if (note == null) return 0;
            var startX = GetTickX(note.Tick);
            var endX = GetTickX(note.Tick + note.Length);
            if (startX == endX) return 0;
            if (x < startX) return 0;
            if (x > endX) return 1;
            return (x - startX) / (endX - startX);
        }

        /// <summary>
        /// Returns all notes within box.
        /// </summary>
        public List<SequencerClip.Note> GetNotesWithin(int x0, int y0, int x1, int y1)
        {
            var ret = new List<SequencerClip.Note>();

            var tick0 = GetTickAt(x0);
            var pitch0 = GetNoteAt(y0);
            var tick1 = GetTickAt(x1);
            var pitch1 = GetNoteAt(y1);


            if (tick1 < tick0)
            {
                var t = tick1;
                tick1 = tick0;
                tick0 = t;
            }
            if (pitch1 < pitch0)
            {
                var t = pitch1;
                pitch1 = pitch0;
                pitch0 = t;
            }


            foreach (var note in Clip.Notes)
            {
                if (note.Tick > tick1) break;
                if (note.Pitch >= pitch0 && note.Pitch <= pitch1)
                {
                    var noteStart = note.Tick;
                    var noteEnd = note.Tick + note.Length;
                    if ((noteStart >= tick0 && noteStart <= tick1) || (noteEnd >= tick0 && noteEnd <= tick1))
                    {
                        ret.Add(note);
                    }
                }
            }

            return ret.Count == 0 ? null : ret;
        }


        /// <summary>
        /// Renders the sequence.
        /// </summary>
        protected override void OnRender()
        {
            if (Clip == null) return;

            var g = Graphics.FromImage(Image);
                    

            // draw note backgrounds and grid
            lock (_backgroundLock)
            {
                if (_background != null)
                {
                    _background.Draw(this, g);
                }
            }


            // draw the notes.
            foreach (var note in Clip.Notes)
            {
                var x0 = GetTickX(note.Tick);
                if (x0 >= Image.Width) break;

                var x1 = GetTickX(note.Tick + note.Length);
                var width = x1 - x0;


                var y0 = GetNoteY(note.Pitch);

                if (note == SelectedNote || (SelectedNotes != null && SelectedNotes.Contains(note)))
                {
                    g.FillRectangle(highlightNoteBrush, x0, y0, width, NoteHeight);
                    g.DrawRectangle(highlightNotePen, x0, y0, width, NoteHeight);
                }
                else
                {
                    g.FillRectangle(noteBrush, x0, y0, width, NoteHeight);
                    g.DrawRectangle(notePen, x0, y0, width, NoteHeight);
                }
            }

            // draw the launchpad
            if (ShowLaunchPad)
            {
                DrawRectangle(g, Clip.LaunchPad, launchPadPen, launchPadBrush, true);
            }

            // draw the zoombox
            if (ShowZoom)
            {
                DrawRectangle(g, ZoomBox, zoomPen, zoomBrush, true);
            }

            // draw the selection box
            if (ShowSelectBox)
            {
                DrawRectangle(g, SelectBox, selectionPen, selectionBrush, false);
            }

            // draw the play cursor
            if (ShowPlayCursor)
            {
                var r = new Rectangle();
                r.X = (int)GetTickX(PlayCursor);
                r.Y = 0;
                r.Width = 10;
                r.Height = Image.Height;
                DrawRectangle(g, r, playCursorPen, playCursorBrush, false);

            }
                     
        }

        #endregion

        #region Private


        /// <summary>
        /// Gets the MIDI tick displayed at a given x coordinate.
        /// </summary>
        internal int GetTickAt(int x)
        {
            return (int)(AnimationFirstTick + (x / PixelsPerTick));
        }

        /// <summary>
        /// Gets the MIDI pitch displayed at a given y coordinate.
        /// </summary>
        internal int GetNoteAt(int y)
        {
            return (int)((float)TopNote - ((float)y / NoteHeight)) + 1;
        }

        /// <summary>
        /// Returns the Y for the top of a pitch on the piano roll.
        /// </summary>
        internal float GetNoteY(int pitch)
        {
            return ((float)TopNote - (float)pitch) * NoteHeight;
        }

        /// <summary>
        /// Returns the X location for a given tick in time.
        /// </summary>
        internal float GetTickX(long tick)
        {
            return ((float)tick - AnimationFirstTick) * PixelsPerTick;
        }

        /// <summary>
        /// Returns true if a pitch is a black key.
        /// </summary>
        internal static bool NoteIsSharp(int note)
        {
            switch (note % 12)
            {
                case 1:
                case 3:
                case 6:
                case 8:
                case 10:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Draws a filled rectangle with a border, optionally using
        /// ticks and pitches instead of pixels.
        /// </summary>
        private void DrawRectangle(Graphics g, Rectangle r, Pen p, Brush b, bool convertToPixels = false)
        {
            r = convertToPixels ? GetPixelRectangle(r) : MakePositive(r);
            g.FillRectangle(b, r);
            g.DrawRectangle(p, r);
        }

        /// <summary>
        /// If either the width or height is negative, makes them positive 
        /// without changing the rectangle.
        /// </summary>
        private Rectangle MakePositive(Rectangle r)
        {
            float x0 = r.X;
            float y0 = r.Y;
            float x1 = r.X + r.Width;
            float y1 = r.Y + r.Height;
            if (x1 < x0)
            {
                var t = x1;
                x1 = x0;
                x0 = t;
            }
            if (y1 < y0)
            {
                var t = y1;
                y1 = y0;
                y0 = t;
            }
            return new Rectangle((int)x0, (int)y0, (int)(x1 - x0), (int)(y1 - y0));
        }

        /// <summary>
        /// Converts a rectangle that has coordinates as pitch and tick to pixels.
        /// </summary>
        private Rectangle GetPixelRectangle(Rectangle r)
        {
            float x0 = GetTickX(r.X);
            float y0 = GetNoteY(r.Y);
            float x1 = GetTickX(r.X + r.Width);
            float y1 = GetNoteY(r.Y + r.Height);
            return MakePositive(new Rectangle((int)x0, (int)y0, (int)(x1 - x0), (int)(y1 - y0)));
        }

        /// <summary>
        /// The object drawing the background.
        /// </summary>
        private SequenceRendererBackground _background;

        /// <summary>
        /// Mutex for updating the background object.
        /// </summary>
        private readonly object _backgroundLock = new object();
        
        #endregion
    }

}
