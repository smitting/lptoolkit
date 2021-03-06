﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using LPToolKit.MIDI;
using LPToolKit.MIDI.Pads;
using LPToolKit.MIDI.Pads.Mappers;
using LPToolKit.Core;

namespace LPToolKit.LaunchPad
{
    public delegate void LaunchPadChanged();

    /// <summary>
    /// Object that simulates a launchpad for users who don't have 
    /// one.  Will show up in the device manager and whenever it is
    /// mapped as the pad device, and then the window for the simulator
    /// pops up on the host device.  Send the same signals to the MIDI
    /// input units as a real launchpad.
    /// </summary>
    public class LaunchPadSimulator : VirtualMidiDevice
    {
        #region Constructor

        public LaunchPadSimulator(int w = 540, int h = 540)
        {
            ID = Name = "LaunchPadSimulator";
            Direction = MidiDirection.IO;


            CurrentImage = new Bitmap(w, h);
            _padSizeX = w / 9;
            _padSizeY = h / 9;


            // testing
            XYMapper = new LaunchPadXYMapper();
            ColorMapper = new NovationColorMapper();
        }

        #endregion

        #region Properties

        public event LaunchPadChanged RepaintNeeded;

        /// <summary>
        /// Event handlers to be sent all MIDI events generated by
        /// this simulated device.
        /// TODO: perhaps actually create a subclass of MidiDevice somehow,
        /// so it's REALLY in the MIDI chain?
        /// </summary>
        public event MidiMessageEventHandler Input;


        

        /// <summary>
        /// Size of the bitmap.
        /// </summary>
        public int Width
        {
            get { return CurrentImage.Width; }
        }

        /// <summary>
        /// Size of the bitmap.
        /// </summary>
        public int Height
        {
            get { return CurrentImage.Height; }
        }

        /// <summary>
        /// The current drawing of the launchpad.
        /// </summary>
        public Bitmap CurrentImage { get; private set; }

        /// <summary>
        /// Driver to report for events.  Optional.
        /// </summary>
        public MidiDriver Driver = null;

        /// <summary>
        /// Maps colors.  TODO: Should probably be in a hardware driver or something.
        /// </summary>
        public IButtonColorMapper ColorMapper;

        /// <summary>
        /// Maps xy.    TODO: Should probably be in a hardware driver or something.
        /// </summary>
        public IButtonXYMapper XYMapper;

        #endregion

        #region Methods

        /// <summary>
        /// Receives a MidiMessage from the MIDI pipe.
        /// </summary>
        public override void Receive(MidiMessage msg)
        {
            switch (msg.Type)
            { 
                case MidiMessageType.NoteOn:
                    SetPadMidiNote(msg.Pitch, msg.Velocity);
                    break;
                case MidiMessageType.ControlChange:
                    SetPadMidiCC(msg.Control, msg.Value);
                    break;
            }
        }

        /// <summary>
        /// Updates the bitmap image with the latest data.
        /// </summary>
        public void Paint()
        {
            var g = Graphics.FromImage(CurrentImage);
            g.FillRectangle(Brushes.DarkGray, 0, 0, CurrentImage.Width, CurrentImage.Height);
            for (var x = _state.GetLowerBound(0); x <= _state.GetUpperBound(0); x++)
            {
                for (var y = _state.GetLowerBound(1); y <= _state.GetUpperBound(1); y++)
                {
                    if (y == 0 && x == 8) continue;

                    int px, py;
                    PadToPixel(x, y, out px, out py);
                    Brush b = MidiToBrush(_state[x, y].MidiColor);
                 
                    // TODO: don't hard code padding size to 4.

                    if (y == 0 || x == 8) // circles
                    {
                        g.FillEllipse(b, px + 2, py + 2, _padSizeX - 4, _padSizeY - 4);
                    }
                    else // squares
                    {
                        g.FillRectangle(b, px + 2, py + 2, _padSizeX - 4, _padSizeY - 4);
                    }
                }
            }
        }

        /// <summary>
        /// Converts launchpad midi velocities to .net solid brushes.
        /// </summary>
        private Brush MidiToBrush(int midi)
        {
            Brush ret;
            if (_midiToBrush.TryGetValue(midi, out ret) == false)
            {
                ret = new SolidBrush(MidiToColor(midi));
                _midiToBrush.Add(midi, ret);
            }
            return ret;
        }

        /// <summary>
        /// Memoization for color conversion.
        /// </summary>
        private Dictionary<int, Brush> _midiToBrush = new Dictionary<int, Brush>();

        /// <summary>
        /// Converts launchpad midi velocities to .net colors.
        /// </summary>
        private Color MidiToColor(int midi)
        {
            Color ret;
            if (_midiToColor.TryGetValue(midi, out ret) == false)
            {
                var c = new ButtonColor(ColorMapper.ValueToColor(midi), 4);

                //LaunchPadColor.Decode(midi, out red, out green, out flags);
                //ret = Color.FromArgb(red * 85, green *85, 0);
                ret = Color.FromArgb(c.Red, c.Green, 0);
                _midiToColor.Add(midi, ret);
            }
            return ret;
        }

        /// <summary>
        /// Memoization for color conversion.
        /// </summary>
        private static Dictionary<int, Color> _midiToColor = new Dictionary<int, Color>();
        

        /// <summary>
        /// Tells this class that a mouse was clicked at a certain point.
        /// </summary>
        public void MouseDown(int x, int y)
        {
            int padx, pady;
            PixelToPad(x, y, out padx, out pady);
            PadDown(padx, pady);
        }

        public void MouseUp(int x, int y)
        {
            int padx, pady;
            PixelToPad(x, y, out padx, out pady);
            PadDown(padx, pady);
        }

        private int _padSizeX, _padSizeY;

        private void PixelToPad(int mx, int my, out int padx, out int pady)
        {
            padx = mx / _padSizeX;
            pady = (my / _padSizeY) - 1;
        }

        private void PadToPixel(int padx, int pady, out int mx, out int my)
        {
            mx = padx * _padSizeX;
            my = pady * _padSizeY;
        }
        /// <summary>
        /// Simulates a button being pressed.
        /// </summary>
        public void PadDown(int x, int y)
        {

            var e = new MidiMessageEventArgs();
            //e.Message = LaunchPadXY.GetMidiMessage(x, y, true);
            e.Message = new MidiMessage();
            XYMapper.ConvertXY(x, y, out e.Message.Type, out e.Message.Value);
            e.Message.Velocity = 127;

            e.Driver = Driver;
            e.Device = this;
            if (Input != null)
            {
                Input(this, e);
            }
        }

        /// <summary>
        /// Simulates a button being released.
        /// </summary>
        public void PadUp(int x, int y)
        {
            var e = new MidiMessageEventArgs();
            //e.Message = LaunchPadXY.GetMidiMessage(x, y, false);
            e.Message = new MidiMessage();
            XYMapper.ConvertXY(x, y, out e.Message.Type, out e.Message.Value);
            e.Message.Velocity = 0;

            e.Driver = Driver;
            e.Device = this;
            if (Input != null)
            {
                Input(this, e);
            }
        }

        /// <summary>
        /// Sets the color at a pad via it's midi pitch.
        /// </summary>
        public void SetPadMidiNote(int pitch, int velocity)
        {
            int x, y;
            ImplantEventType eventType;
            XYMapper.ConvertXY(MidiMessageType.NoteOn, pitch, out x, out y, out eventType);            
            //LaunchPadXY.GridFromPitch(pitch, out x, out y);
            SetPad(x, y, velocity);
        }

        public int GetPadMidiNote(int pitch)
        {
            int x, y;
            ImplantEventType eventType;
            XYMapper.ConvertXY(MidiMessageType.NoteOn, pitch, out x, out y, out eventType);       
            //LaunchPadXY.GridFromPitch(pitch, out x, out y);
            return GetPad(x, y);
        }

        public void SetPadMidiCC(int cc, int value)
        {
            int x, y;
            ImplantEventType eventType;
            XYMapper.ConvertXY(MidiMessageType.ControlChange, cc, out x, out y, out eventType);    
            //LaunchPadXY.GridFromPitch(cc, true, out x, out y);
            SetPad(x, y, value);
        }

        public int GetPadMidiCC(int cc)
        {
            int x, y;
            ImplantEventType eventType;
            XYMapper.ConvertXY(MidiMessageType.ControlChange, cc, out x, out y, out eventType);    
            //LaunchPadXY.GridFromPitch(cc, true, out x, out y);
            return GetPad(x, y);
        }

        /// <summary>
        /// Sets the color at a pad via the encoded MIDI velocity.
        /// </summary>
        public void SetPad(int x, int y, int velocity)
        {
            if (_state[x, y + 1].MidiColor != velocity)
            {
                _state[x, y + 1].MidiColor = velocity;
                if (RepaintNeeded != null)
                {
                    RepaintNeeded();
                }
            }
        }

        /// <summary>
        /// Returns the encoded MIDI color.
        /// </summary>
        public int GetPad(int x, int y)
        {
            return _state[x, y + 1].MidiColor;
        }

        #endregion

        #region Private

        private readonly PadState[,] _state = new PadState[9,9];


        private struct PadState
        {
            /// <summary>
            /// The midi velocity of the color displayed at a point.
            /// </summary>
            public int MidiColor;
        }


        #endregion
    }
}
