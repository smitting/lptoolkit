using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace LPToolKit.GUI.Renderers
{
    /// <summary>
    /// Types of background for the sequencer
    /// </summary>
    public enum SequencerBackgrounds
    {
        PianoRoll,
        Drum,
        Pattern,
        Overview
    }

    /// <summary>
    /// Base class for objects that draw the backgrounds for the
    /// GuiRenderer in different ways.  While a SequenceClip always
    /// represents MIDI data, the sequence may not always be representing
    /// a set of piano-style music, such as drum sequences and 
    /// clip patterns, which need different graphical representations.
    /// </summary>
    internal abstract class SequenceRendererBackground
    {
        /// <summary>
        /// Actual background rendering by subclass.
        /// </summary>
        public abstract void Draw(SequenceRenderer renderer, Graphics g);

        /// <summary>
        /// Converts an instance into a type enum.
        /// </summary>
        public static SequencerBackgrounds ToEnum(SequenceRendererBackground b)
        {
            if (b is PianoRollBackground)
            {
                return SequencerBackgrounds.PianoRoll;
            }
            if (b is DrumBackground)
            {
                return SequencerBackgrounds.Drum;
            }
            if (b is PatternBackground)
            {
                return SequencerBackgrounds.Pattern;
            }
            if (b is OverviewBackground)
            {
                return SequencerBackgrounds.Overview;
            }
            return SequencerBackgrounds.PianoRoll;
        }

        /// <summary>
        /// Creates a new instance from a type enum.
        /// </summary>
        public static SequenceRendererBackground FromEnum(SequencerBackgrounds b)
        {
            switch (b)
            {
                case SequencerBackgrounds.PianoRoll:
                    return new PianoRollBackground();
                case SequencerBackgrounds.Drum:
                    return new DrumBackground();
                case SequencerBackgrounds.Pattern:
                    return new PatternBackground();
                case SequencerBackgrounds.Overview:
                    return new OverviewBackground();
            }
            throw new NotImplementedException("Unknown background type: " + b.ToString());
        }
    }


    /// <summary>
    /// Background for a typical piano roll, which uses different colors
    /// for white and black keys, and draws a line below C and F.
    /// </summary>
    internal class PianoRollBackground : SequenceRendererBackground
    {
        public override void Draw(SequenceRenderer renderer, Graphics g)
        {
            for (int note = renderer.TopNote; note >= renderer.FirstNote && note >= 0; note--)
            {
                Brush b = SequenceRenderer.NoteIsSharp(note) ? renderer.blackKey : renderer.whiteKey;
                var y = renderer.GetNoteY(note);
                g.FillRectangle(b, 0, y, renderer.Image.Width, renderer.NoteHeight);
            }

            for (int note = renderer.TopNote; note >= renderer.FirstNote && note >= 0; note--)
            {
                var y = renderer.GetNoteY(note);
                if (note % 12 == 0)
                {
                    g.DrawLine(renderer.grid1, 0, y + renderer.NoteHeight, renderer.Image.Width, y + renderer.NoteHeight);
                }
                else if (note % 12 == 5)
                {
                    g.DrawLine(renderer.grid2, 0, y + renderer.NoteHeight, renderer.Image.Width, y + renderer.NoteHeight);
                }
            }


            // draw beats and measures grid
            int top = (int)Math.Max(0, renderer.GetNoteY(127));
            int bottom = (int)Math.Min(renderer.Image.Height, renderer.GetNoteY(-1));

            int startTick = (renderer.AnimationFirstTick / 24) * 24;

            for (int tick = startTick; ; tick += 24)
            {
                var tickx = renderer.GetTickX(tick);
                if ((tick / 24) % 4 == 0)
                {
                    g.DrawLine(renderer.grid1, tickx, top, tickx, bottom);
                }
                else
                {
                    g.DrawLine(renderer.grid3, tickx, top, tickx, bottom);
                }

                if (tickx > renderer.Image.Width) break;
            }
        }
    }

    /// <summary>
    /// Background for a drum sequence, which alternates the color of
    /// each line, and draws a line between every 4th row.
    /// </summary>
    internal class DrumBackground : SequenceRendererBackground
    {
        public override void Draw(SequenceRenderer renderer, Graphics g)
        {
            for (int note = renderer.TopNote; note >= renderer.FirstNote && note >= 0; note--)
            {
                Brush b = note % 2 == 1 ? renderer.blackKey : renderer.whiteKey;
                var y = renderer.GetNoteY(note);
                g.FillRectangle(b, 0, y, renderer.Image.Width, renderer.NoteHeight);
            }

            for (int note = renderer.TopNote; note >= renderer.FirstNote && note >= 0; note--)
            {
                if (note % 4 == 0)
                {
                    var y = renderer.GetNoteY(note);
                    g.DrawLine(renderer.grid1, 0, y + renderer.NoteHeight, renderer.Image.Width, y + renderer.NoteHeight);
                }
            }


            // draw beats and measures grid
            int top = (int)Math.Max(0, renderer.GetNoteY(127));
            int bottom = (int)Math.Min(renderer.Image.Height, renderer.GetNoteY(-1));

            int startTick = (renderer.AnimationFirstTick / 24) * 24;

            for (int tick = startTick; ; tick += 24)
            {
                var tickx = renderer.GetTickX(tick);
                if ((tick / 24) % 4 == 0)
                {
                    g.DrawLine(renderer.grid1, tickx, top, tickx, bottom);
                }
                else
                {
                    g.DrawLine(renderer.grid3, tickx, top, tickx, bottom);
                }

                if (tickx > renderer.Image.Width) break;
            }
        }
    }

    /// <summary>
    /// TODO: decide how to represent patterns.
    /// </summary>
    internal class PatternBackground : SequenceRendererBackground
    {
        public override void Draw(SequenceRenderer renderer, Graphics g)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Background for zoomed out versions of the overall clip.
    /// Alternates between white and black key colors on each
    /// measure of 4 bars.
    /// </summary>
    internal class OverviewBackground : SequenceRendererBackground
    {
        public override void Draw(SequenceRenderer renderer, Graphics g)
        {
            int startTick = (renderer.AnimationFirstTick / 24) * 24;
            for (int tick = startTick; ; tick += 24)
            {
                var x0 = renderer.GetTickX(tick);
                var x1 = renderer.GetTickX(tick + 24);
                var b = (tick / 96) % 2 == 0 ? renderer.whiteKey : renderer.blackKey;
                g.FillRectangle(b, x0, 0, x1 - x0, renderer.Image.Height);
                if (x1 > renderer.Image.Width) break;
            }

        }
    }
}
