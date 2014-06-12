using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LPToolKit.MIDI;
using LPToolKit.Sequences;

namespace LPToolKit.GUI.Renderers
{
    /// <summary>
    /// The different ways to edit notes in a sequence renderer.
    /// </summary>
    public enum ClipEditModes
    {
        /// <summary>
        /// Piano roll for editing music.
        /// </summary>
        Music,

        /// <summary>
        /// Drum rack for drum sequencing.
        /// </summary>
        Drum,

        /// <summary>
        /// Zoomed out view for selecting where to edit in the zoomed
        /// in view.
        /// </summary>
        Overview
    }

    /// <summary>
    /// Base class for classes that implement different ways of
    /// handling user input to edit a clip in the SequenceRenderer.
    /// </summary>
    internal abstract class ClipEditMode
    {
        #region Constructors

        /// <summary>
        /// Constructor accepts the object that will be handling all
        /// user input.
        /// </summary>
        public ClipEditMode(ClipUIHandler handler)
        {
            Handler = handler;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The handler this mouse mode is providing input controls for.
        /// </summary>
        public readonly ClipUIHandler Handler;

        #endregion

        #region Abstract Methods

        public abstract void MouseDown(int x, int y, Keys modifierKeys);
        public abstract void MouseMove(int x, int y);
        public abstract void MouseUp(int x, int y);
        public abstract void KeyDown(Keys keycode);

        #endregion

        #region Methods

        /// <summary>
        /// Gets the enum for a given object instance.
        /// </summary>
        public static ClipEditModes ToEnum(ClipEditMode instance)
        {
            if (instance is MusicClipEditMode)
            {
                return ClipEditModes.Music;
            }
            else if (instance is OverviewClipEditMode)
            {
                return ClipEditModes.Overview;
            }
            return ClipEditModes.Drum;
        }
        
        /// <summary>
        /// Returns a new object instance for a given enum.
        /// </summary>
        public static ClipEditMode FromEnum(ClipEditModes b, ClipUIHandler handler)
        {
            switch (b)
            {
                case ClipEditModes.Music:
                    return new MusicClipEditMode(handler);
                case ClipEditModes.Drum:
                    return new DrumClipEditMode(handler);
                case ClipEditModes.Overview:
                    return new OverviewClipEditMode(handler);
            }
            throw new NotImplementedException("Unknown ClipEditMode: " + b.ToString());
        }

        #endregion
    }

    /// <summary>
    /// Edit mode suitable for musical notes.
    /// </summary>
    internal class MusicClipEditMode : ClipEditMode
    {
        public MusicClipEditMode(ClipUIHandler handler)
            : base(handler)
        {
        }

        public override void MouseDown(int x, int y, Keys modifierKeys)
        {
            // default action is none
            Handler.Action = ClipMouseAction.None;

            // save original mouse position
            Handler.MousePosition.SetStart(x, y);
            Handler.MousePosition.Set(x, y);

            // create a new note with control click.
            if (modifierKeys == Keys.Control)
            {
                var note = new SequencerClip.Note();
                note.Length = 24;
                note.Tick = MidiMath.Quantize(Handler.Renderer.GetTickAt(x), note.Length);
                note.Pitch = Handler.Renderer.GetNoteAt(y);
                note.Velocity = 100;
                Handler.Renderer.Clip.Notes.Add(note);
                Handler.Renderer.Clip.Sort();
                Handler.Renderer.Render();

                Handler.Renderer.SelectedNote = note;
                Handler.Renderer.SelectedNotes = null;

                Handler.Action = ClipMouseAction.Resize;
            }
            // select a single note if we click on a note
            else
            {
                // find the single note we clicked on
                Handler.Renderer.SelectedNote = Handler.Renderer.GetNoteAt(x, y);
                if (Handler.Renderer.SelectedNote == null)
                {
                    Handler.Renderer.SelectedNotes = null;
                }

                // if that note was part of an already selected group
                // then we need to move the entire group
                if (Handler.Renderer.SelectedNotes != null && Handler.Renderer.SelectedNotes.Contains(Handler.Renderer.SelectedNote))
                {
                    Handler.Action = ClipMouseAction.MovingGroup;
                }
                else if (Handler.Renderer.SelectedNote != null)
                {
                    // unselect group if not part of previous selected group
                    Handler.Renderer.SelectedNotes = null;

                    // decide action based on if we're at the start
                    // or the end of a note
                    var p = Handler.Renderer.GetPercentageThroughNoteAt(Handler.Renderer.SelectedNote, x);
                    if (p < 0.5f)
                    {
                        Handler.Action = ClipMouseAction.MovingSingle;
                    }
                    else
                    {
                        Handler.Action = ClipMouseAction.Resize;
                    }
                }
                else
                {
                    // multi select if we didn't click on anythin
                    Handler.Action = ClipMouseAction.MultiSelect;
                    Handler.Renderer.SelectBox.X = x;
                    Handler.Renderer.SelectBox.Y = y;
                    Handler.Renderer.SelectBox.Width = 1;
                    Handler.Renderer.SelectBox.Height = 1;
                    Handler.Renderer.ShowSelectBox = true;
                }
            }


            // save original values of selected notes
            if (Handler.Renderer.SelectedNotes != null)
            {
                Handler.UpdateOriginal(Handler.Renderer.SelectedNotes.ToArray());
            }
            else
            {
                Handler.UpdateOriginal(Handler.Renderer.SelectedNote);
            }

            // draw
            Handler.Renderer.Render();
        }

        public override void MouseUp(int x, int y)
        {

            Handler.MousePosition.Set(x, y);


            // sort dataset if we moved notes
            switch (Handler.Action)
            {
                case ClipMouseAction.MovingSingle:
                    if (Handler.MousePosition.WasDrag)
                    {
                        Handler.Renderer.Clip.Sort();
                    }
                    break;
                case ClipMouseAction.MovingGroup:
                    // if we just clicked on a member of the group and 
                    // didn't move, then just select that one member
                    if (Handler.MousePosition.WasDrag)
                    {
                        Handler.Renderer.Clip.Sort();
                    }
                    else
                    {
                        Handler.Renderer.SelectedNotes = null;
                        Handler.Renderer.Render();
                    }
                    break;
                case ClipMouseAction.MultiSelect:
                    Handler.Renderer.ShowSelectBox = false;
                    if (Handler.MousePosition.IsDrag)
                    {
                        Handler.Renderer.SelectedNote = null;
                        Handler.Renderer.SelectedNotes = Handler.Renderer.GetNotesWithin(Handler.MousePosition.StartX, Handler.MousePosition.StartY, Handler.MousePosition.X, Handler.MousePosition.Y);
                        if (Handler.Renderer.SelectedNotes != null && Handler.Renderer.SelectedNotes.Count == 1)
                        {
                            Handler.Renderer.SelectedNote = Handler.Renderer.SelectedNotes[0];
                            Handler.Renderer.SelectedNotes = null;
                        }
                    }
                    Handler.Renderer.Render();
                    break;
            }
            Handler.Action = ClipMouseAction.None;
        }

        public override void MouseMove(int x, int y)
        {
            Handler.MousePosition.Set(x, y);

            switch (Handler.Action)
            {
                case ClipMouseAction.MovingSingle:
                    Handler.Renderer.SelectedNote.Tick = MidiMath.Quantize(Handler.Renderer.GetTickAt(Handler.MousePosition.X), 24);
                    Handler.Renderer.SelectedNote.Pitch = Handler.Renderer.GetNoteAt(Handler.MousePosition.Y);
                    Handler.Renderer.Render();
                    break;
                case ClipMouseAction.MovingGroup:
                    // TODO: going to need to keep track of where the notes where when they started
                    foreach (var clip in Handler.Renderer.SelectedNotes)
                    {
                        SequencerClip.Note oclip;
                        if (!Handler.Original.TryGetValue(clip, out oclip)) continue;
                        clip.Tick = MidiMath.Quantize((int)(oclip.Tick + Handler.MousePosition.DeltaX / Handler.Renderer.PixelsPerTick), 24);
                        clip.Pitch = oclip.Pitch - (int)(Handler.MousePosition.DeltaY / Handler.Renderer.NoteHeight);
                    }
                    Handler.Renderer.Render();
                    break;
                case ClipMouseAction.Resize:
                    var endTick = Handler.Renderer.GetTickAt(Handler.MousePosition.X);
                    if (endTick > Handler.Renderer.SelectedNote.Tick)
                    {
                        Handler.Renderer.SelectedNote.Length = MidiMath.Quantize(endTick - Handler.Renderer.SelectedNote.Tick, 24);
                        Handler.Renderer.Render();
                    }
                    break;
                case ClipMouseAction.MultiSelect:
                    Handler.Renderer.SelectBox.Width = Handler.MousePosition.DeltaX;
                    Handler.Renderer.SelectBox.Height = Handler.MousePosition.DeltaY;
                    Handler.Renderer.Render();
                    break;
            }
        }
        public override void KeyDown(Keys keycode)
        {
            switch (keycode)
            {
                case Keys.Delete:
                case Keys.Back:
                    if (Handler.Renderer.SelectedNote != null)
                    {
                        Handler.Renderer.Clip.Notes.Remove(Handler.Renderer.SelectedNote);
                        Handler.Renderer.Render();
                    }
                    else if (Handler.Renderer.SelectedNotes != null)
                    {
                        foreach (var note in Handler.Renderer.SelectedNotes)
                        {
                            Handler.Renderer.Clip.Notes.Remove(note);
                        }
                        Handler.Renderer.Render();
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// User input suitable for editing a drum beat.
    /// </summary>
    internal class DrumClipEditMode : ClipEditMode
    {
        public DrumClipEditMode(ClipUIHandler handler)
            : base(handler)
        {
        }

        private bool isRemoving = false;

        public override void MouseDown(int x, int y, Keys modifierKeys)
        {
            // action is always multiselect, to indicate we're going to
            // keep adding or removing as we go to new places
            Handler.Action = ClipMouseAction.MultiSelect;

            // delete a note if an existing on is clicked on
            var foundNote = Handler.Renderer.GetNoteAt(x, y);
            if (foundNote != null)
            {
                isRemoving = true;
                Handler.Renderer.Clip.Notes.Remove(foundNote);
            }
            else
            {
                isRemoving = false;
                // add a note if clicking outside of a note
                var note = new SequencerClip.Note();
                note.Length = 24;
                note.Tick = MidiMath.Quantize(Handler.Renderer.GetTickAt(x), note.Length);
                note.Pitch = Handler.Renderer.GetNoteAt(y);
                note.Velocity = 100;
                Handler.Renderer.Clip.Notes.Add(note);
                Handler.Renderer.Clip.Sort();
            }

            Handler.Renderer.Render();
        }

        public override void MouseUp(int x, int y)
        {
            Handler.Action = ClipMouseAction.None;

        }

        public override void MouseMove(int x, int y)
        {
            // keep adding or removing if we're dragging
            if (Handler.Action == ClipMouseAction.MultiSelect)
            {                
                var foundNote = Handler.Renderer.GetNoteAt(x, y);
                if (foundNote != null && isRemoving)
                {
                    Handler.Renderer.Clip.Notes.Remove(foundNote);
                    Handler.Renderer.Render();
                }
                else if (foundNote == null && isRemoving == false)
                {
                    var note = new SequencerClip.Note();
                    note.Length = 24;
                    note.Tick = MidiMath.Quantize(Handler.Renderer.GetTickAt(x), note.Length);
                    note.Pitch = Handler.Renderer.GetNoteAt(y);
                    note.Velocity = 100;
                    Handler.Renderer.Clip.Notes.Add(note);
                    Handler.Renderer.Clip.Sort();
                    Handler.Renderer.Render();
                }

            }
        }

        public override void KeyDown(Keys keycode)
        {           
        }
    }


    /// <summary>
    /// User input suitable for selecting what is visible in a zoomed
    /// in view by clicking on a zoomed out view.
    /// </summary>
    internal class OverviewClipEditMode : ClipEditMode
    {
        public OverviewClipEditMode(ClipUIHandler handler) : base(handler)
        {
            Handler.Renderer.ShowZoom = true;
            if (Handler.ParentRenderer == null)
            {
                throw new Exception("ParentRenderer is required for OverviewClipEditMode");
            }
            Handler.ParentRenderer.Rendered += (sender, e) =>
                {
                    // always show current location of parent
                    Handler.Renderer.ZoomBox.X = Handler.ParentRenderer.AnimationFirstTick;

                    // always update view when parent changes
                    Handler.Renderer.Render(); 
                };
        }

        public override void MouseDown(int x, int y, Keys modifierKeys)
        {
            var p = (float)x / (float)Handler.Renderer.Image.Width;
            Handler.ParentRenderer.Clip.LaunchPad.X = Handler.ParentRenderer.FirstTick = MidiMath.Quantize((int)(p * Handler.Renderer.WidthInTicks), 24 * 4);
            Handler.ParentRenderer.Render();
            Handler.Renderer.Render();

            Handler.Action = ClipMouseAction.MultiSelect;
        }

        public override void MouseMove(int x, int y)
        {
            if (Handler.Action == ClipMouseAction.MultiSelect)
            {
                var p = (float)x / (float)Handler.Renderer.Image.Width;
                Handler.ParentRenderer.Clip.LaunchPad.X = Handler.ParentRenderer.FirstTick = MidiMath.Quantize((int)(p * Handler.Renderer.WidthInTicks), 24 * 4);
                Handler.ParentRenderer.Render();
                Handler.Renderer.Render();
            }
        }

        public override void MouseUp(int x, int y)
        {
            Handler.Action = ClipMouseAction.None;

        }

        public override void KeyDown(Keys keycode)
        {
        }
    }

}
