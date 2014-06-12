using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.LaunchPad.UI;
using LPToolKit.MIDI.Pads;

namespace LPToolKit.LaunchPad
{
    #region Midi Events

    [Obsolete]
    public delegate void MidiNoteOnEventHandler(object sender, MidiNoteOnEventArgs e);
    [Obsolete]
    public delegate void MidiNoteOffEventHandler(object sender, MidiNoteOffEventArgs e);
    [Obsolete]
    public delegate void MidiControlChangeEventHandler(object sender, MidiControlChangeEventArgs e);


    [Obsolete]
    public class MidiNoteOnEventArgs
    {
        public int Channel;
        public int Pitch;
        public int Velocity;
    }

    [Obsolete]
    public class MidiNoteOffEventArgs
    {
        public int Channel;
        public int Pitch;
        public int Velocity;
    }

    [Obsolete]
    public class MidiControlChangeEventArgs
    {
        public int Channel;
        public int Control;
        public int Value;
    }

    #endregion

    #region Basic Launchpad Input Events

    /// <summary>
    /// Event generated for button press and release events.
    /// </summary>
    [Obsolete]
    public delegate void LaunchPadButtonEventHandler(object sender, LaunchPadButtonEventArgs e);

    /// <summary>
    /// Arguments for when launchpad buttons are pressed or released.
    /// </summary>
    [Obsolete]
    public class LaunchPadButtonEventArgs : EventArgs
    {
        /// <summary>
        /// The X coordinate of the button.
        /// 0-7 for the main grid, 8 for the right-side menu.
        /// </summary>
        public int X;

        /// <summary>
        /// The Y coordinate of the button.
        /// 0-7 for the main grid, -1 for the top control menu.
        /// </summary>
        public int Y;

        /// <summary>
        /// True when the button is down, false when the button is up.
        /// </summary>
        public bool Pressed;

        /// <summary>
        /// True iff this was on the top menu.
        /// </summary>
        public bool IsTop;

        /// <summary>
        /// True iff this was on the right menu.
        /// </summary>
        public bool IsRight;

        /// <summary>
        /// True iff this is a double click event.
        /// </summary>
        public bool DoubleClick;

        /// <summary>
        /// The last time this button was pressed, for detecting double
        /// clicks.  This is DateTime.MinValue the first time this 
        /// button is pressed for a LaunchPad instance.
        /// </summary>
        public DateTime LastTimePressed;

        /// <summary>
        /// The time when this button was pressed down.
        /// </summary>
        public DateTime TimePressed;

        /// <summary>
        /// The time when this button was released if this is a button up event,
        /// other wise its value will be DateTime.MinValue.
        /// </summary>
        public DateTime TimeReleased;
    }

    #endregion


    #region Buffer Events (TODO: clean up)

    /// <summary>
    /// Event triggered when any value on a LaunchPad buffer changes.
    /// </summary>
    [Obsolete]
    public delegate void LaunchPadStateChanged();

    /// <summary>
    /// Event triggered when the LaunchPad mode has been changed, via
    /// the top row of buttons (program specific).
    /// </summary>
    [Obsolete]
    public delegate void LaunchPadModeChanged();

    /// <summary>
    /// Event triggered that one pad value has changed, with the values
    /// passed along.
    /// </summary>
    [Obsolete]
    public delegate void LaunchPadValueChangedEventHandler(object sender, LaunchPadValueChangedEventArgs e);

    /// <summary>
    /// Arguments sent whenever a buffer value has been changed.
    /// </summary>
    [Obsolete]
    public class LaunchPadValueChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The grid x-coordinate affected, from 0-7.
        /// </summary>
        public int X;

        /// <summary>
        /// The grid y-coordinate affected, from 0-7.
        /// </summary>
        public int Y;

        /// <summary>
        /// The new value inserted into the buffer.
        /// </summary>
        public LaunchPadColor Value;

        /// <summary>
        /// The previous value overwritten in the buffer.
        /// </summary>
        public LaunchPadColor PreviousValue;
    }

    #endregion

}
