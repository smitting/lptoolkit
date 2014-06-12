using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.Midi;

namespace LPToolKit.Sequences
{

    /// <summary>
    /// Converts MIDI files from NAudio to and from SequencerClips
    /// </summary>
    public class MidiSequenceConverter
    {
        public static SequencerClip CreateClip(MidiFile file)
        {
            long lastNoteEnd = 0;

            var sublist = new List<NoteOnEvent>();
            var trackCount = file.Tracks;
            for (var track = 0; track < trackCount; track++)
            {
                //Console.WriteLine("Analyzing track #{0}", track);
                var events = file.Events.GetTrackEvents(track);
                foreach (var e in events)
                {
                    if (e is NoteOnEvent)
                    {
                        var ne = e as NoteOnEvent;
                        if (ne.Velocity > 0)
                        {
                            var noteEnd = ne.AbsoluteTime + ne.NoteLength;
                            if (noteEnd > lastNoteEnd)
                            {
                                lastNoteEnd = noteEnd;
                            }
                            sublist.Add(ne);
                        }
                    }
                }
            }
            return CreateClip(sublist, ConvertMidiTimeToTicks(lastNoteEnd));
        }

        /// <summary>
        /// Ignores all but the note on events.
        /// </summary>
        public static SequencerClip CreateClip(List<MidiEvent> events, int length = 96 * 4)
        {
            var sublist = new List<NoteOnEvent>();
            foreach (var e in events)
            {
                if (e is NoteOnEvent)
                {
                    sublist.Add(e as NoteOnEvent);
                }
            }
            return CreateClip(sublist, length);
        }

        public static int ConvertMidiTimeToTicks(long absoluteTime)
        {
            // 120 midi ticks per quarter note
            // 96 ticks per measure = 24 ticks per note
            // ticks = time / 120 * 24
            return (int)(absoluteTime / 5);
        }

        public static SequencerClip CreateClip(List<NoteOnEvent> events, int length = 96 * 4)
        {
            var ret = new SequencerClip();
            ret.Length = length;

            foreach (var e in events)
            {
                if (e.Velocity > 0)
                {
                    var note = new SequencerClip.Note();
                    note.Tick = ConvertMidiTimeToTicks(e.AbsoluteTime);
                    note.Pitch = e.NoteNumber;
                    note.Velocity = e.Velocity;
                    note.Length = ConvertMidiTimeToTicks(e.NoteLength);
                    ret.Notes.Add(note);
                }
            }

            ret.Analyze();
            ret.Sort();
            return ret;
        }

    }
}
