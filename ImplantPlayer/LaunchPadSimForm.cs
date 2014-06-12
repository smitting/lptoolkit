using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LPToolKit.LaunchPad;
using System.Threading;

namespace ImplantPlayer
{
    /// <summary>
    /// TODO: THE SIMULATOR DOES NOT WORK YET.
    /// The reason the simulator does not work is there is no fake MidiDriver 
    /// for simulated events yet, so it is impossible to send back colors to
    /// the launchpad simulator.  However, the implants do receive fake
    /// midi events.
    /// </summary>
    public partial class LaunchPadSimForm : Form
    {
        public LaunchPadSimForm()
        {
            InitializeComponent();


        }

        public LaunchPadSimulator Simulator
        {
            get { return _simulator; }
            set
            {
                _simulator = value;
                if (_simulator != null)
                {
                    this.Width = _simulator.Width;
                    this.Height = _simulator.Height;
                    StartThread();
                }
            }
        }

        private LaunchPadSimulator _simulator = null;
        private Thread _drawThread = null;

        private void StartThread()
        {
            if (Simulator != null)
            {
                Simulator.RepaintNeeded += () =>
                {
                    Simulator.Paint();
                    pictureBox1.Image = Simulator.CurrentImage;

                };
            }
            /*
            if (_drawThread != null) return;
            _drawThread = LPToolKit.Util.ThreadManager.Current.Run(() =>
            {
                try
                {
                    while (Simulator != null)
                    {
                        if (Visible)
                        {
                            if (Simulator != null)
                            {
                                Simulator.Paint();
                                pictureBox1.Image = Simulator.CurrentImage;
                                //pictureBox1.Invalidate();
                            }
                        }
                        Thread.Sleep(100);
                    }
                }
                finally
                {
                    _drawThread = null;
                }
            });*/
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (Simulator == null) return;
            Simulator.MouseDown(e.X, e.Y);
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (Simulator == null) return;
            Simulator.MouseUp(e.X, e.Y);
        }
    }
}
