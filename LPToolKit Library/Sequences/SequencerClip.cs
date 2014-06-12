using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace LPToolKit.Sequences
{
    /// <summary>
    /// A series of sequenced notes for a single track stored in a way
    /// to be useful from a LaunchPad but still reasonably compatible
    /// with MIDI files.
    /// </summary>
    public class SequencerClip
    {
        /// <summary>
        /// One note within the sequence.
        /// </summary>
        public class Note
        {
            /// <summary>
            /// The starting point in 1/96th ticks.
            /// </summary>
            public int Tick;

            /// <summary>
            /// The duration in ticks.  A 16th note takes 6 ticks.
            /// </summary>
            public int Length;

            /// <summary>
            /// The MIDI note value.
            /// </summary>
            public int Pitch;

            /// <summary>
            /// The MIDI velocity for this note.
            /// </summary>
            public int Velocity;

            /// <summary>
            /// Provides a deep copy of this note for storing the
            /// previous value of the instance.
            /// </summary>
            public Note Clone()
            {
                return new Note()
                {
                    Tick = this.Tick,
                    Length = this.Length,
                    Pitch = this.Pitch,
                    Velocity = this.Velocity
                };
            }
        }

        /// <summary>
        /// Where the launchpad is currently mapped to within this clip.
        /// </summary>
        public Rectangle LaunchPad = new Rectangle(0, 0, 24 * 8, 8);

        /// <summary>
        /// The notes in this sequence.
        /// </summary>
        public List<Note> Notes = new List<Note>();

        /// <summary>
        /// The loop length of this clip, in ticks
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// The lowest note in the sequence.
        /// </summary>
        public int LowNote { get; private set; }

        /// <summary>
        /// The highest note in the sequence.
        /// </summary>
        public int HighNote { get; private set; }

        /// <summary>
        /// The earliest note in this clip.
        /// </summary>
        public int FirstNote { get; private set; }

        /// <summary>
        /// The start of the last note in this clip.
        /// </summary>
        public int LastNote { get; private set; }

        /// <summary>
        /// Adds all of the notes from the other clip and reanalyzes.
        /// </summary>
        public void CombineWith(SequencerClip other)
        {
            Notes.AddRange(other.Notes);
            Sort();
            Analyze();
        }

        /// <summary>
        /// Updates the properties describing the range of notes.
        /// </summary>
        public void Analyze()
        {
            if (Notes.Count == 0) return;
            HighNote = LowNote = Notes[0].Pitch;
            FirstNote = LastNote = Notes[0].Tick;

            foreach (var note in Notes)
            {
                if (note.Tick < FirstNote)
                {
                    FirstNote = note.Tick;
                }
                if (note.Tick > LastNote)
                {
                    LastNote = note.Tick;
                }
                if (note.Pitch < LowNote)
                {
                    LowNote = note.Pitch;
                }
                if (note.Pitch > HighNote)
                {
                    HighNote = note.Pitch;
                }
            }
        }

        /// <summary>
        /// Sorts the notes by their order in time.
        /// </summary>
        public void Sort()
        {
            Notes.Sort((a, b) => { return a.Tick.CompareTo(b.Tick); });
        }
    }

}
