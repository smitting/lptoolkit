using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LPToolKit.GUI;
using LPToolKit.GUI.Layouts;
using LPToolKit.GUI.Renderers ;
using LPToolKit.GUI.Controls;
using LPToolKit.MIDI;
using LPToolKit.Session;
using LPToolKit.Sequences;


namespace ImplantPlayer
{
    /// <summary>
    /// Window for testing GUI Controls
    /// </summary>
    public partial class GuiTestForm : Form
    {
        public GuiTestForm()
        {
            InitializeComponent();


            // TESTING SEQUENCER
            GUI = UserSession.Current.Gui.Context;

            
            var clip = new SequencerClip();

            // create clip view
            Renderer = new SequenceRenderer(new Bitmap(ClientSize.Width, ClientSize.Height - 100));
            Renderer.Clip = clip;
            Renderer.Rendered += (sender, e) => { this.Invalidate(); };
            Renderer.BackgroundType = SequencerBackgrounds.Drum;
            Renderer.HeightInNotes = 8;
            Renderer.WidthInBeats = 16;
            Renderer.Clip.LaunchPad.Y = Renderer.FirstNote;

            Handler = new ClipUIHandler()
            {
                Clip = Renderer.Clip,
                Renderer = Renderer,
                ActiveArea = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height - 100)
            };
            Handler.Mode = ClipEditModes.Drum;
            Renderer.Render();


            // create the full song view
            Renderer2 = new SequenceRenderer(new Bitmap(ClientSize.Width, 100));
            Renderer2.Clip = clip;
            Renderer2.Rendered += (sender, e) => { this.Invalidate(); };
            Renderer2.BackgroundType = SequencerBackgrounds.Overview;
            Renderer2.WidthInTicks = 128 * 24;
            Renderer2.HeightInNotes = 8;
            Renderer2.ZoomBox = new Rectangle(Renderer.Clip.LaunchPad.X, Renderer.Clip.LaunchPad.Y, 24 * 16, 8);

            Handler2 = new ClipUIHandler()
            {
                Clip = Renderer.Clip,
                Renderer = Renderer2,
                ActiveArea = new Rectangle(0, ClientSize.Height - 100, ClientSize.Width, 100),
                ParentRenderer = Renderer
            };

            Handler2.Mode = ClipEditModes.Overview;
            Renderer2.Render();



            // END TEST


            // send window events to gui controls
            this.MouseDown += (sender, e) =>
            {
                GUI.MouseDown(e.X, e.Y, Control.ModifierKeys);
            };
            this.MouseUp += (sender, e) =>
            {
                GUI.MouseUp(e.X, e.Y);
            };
            this.MouseMove += (sender, e) =>
            {
                GUI.MouseMove(e.X, e.Y);
            };
            this.KeyDown += (sender, e) =>
            {
                GUI.KeyDown(e.KeyCode);
            };

            this.Resize += (sender, e) =>
            {
                // TODO: need to pass more detailed information to GuiRenderers when the size changes
                //GUI.Resize(ClientSize.Width, ClientSize.Height);
                Invalidate();
            };

            // called when a GUI control changes value.
            GUI.NeedsRenderered += (sender, e) =>
            {
                this.Invalidate();
            };

            this.Paint += (sender, e) =>
            {
                try
                {
                    UserSession.Current.Gui.Render();
                    e.Graphics.DrawImage(GUI.Render(), 0, 0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            };

            /*
            this.Paint += (sender, e) =>
            {               
                double startValue = 0.0;
                var w = ((ClientSize.Width / 2) - (4 * 50)) / 4;
                var faderHeight = ClientSize.Height / 2;
                var options = new GuiLayoutOption() { Width = w, Height = w, LeftPadding = 25, RightPadding = 25, BottomPadding = 25 };
                var foptions = new GuiLayoutOption() { Width = w, Height = faderHeight, LeftPadding = w / 3, RightPadding = w / 3, TopPadding = 0, BottomPadding = 0 };
                var foptions2 = new GuiLayoutOption() { Width = w, Height = w / 3, LeftPadding = 2, RightPadding = 2, TopPadding = 10, BottomPadding = 2 };
                var seqOptions = new GuiLayoutOption() { Width = ClientSize.Width / 2 - 10, Height = ClientSize.Height - 60, LeftPadding = 5, RightPadding = 5, TopPadding = 5, BottomPadding = 5 };
                var seqOptions2 = new GuiLayoutOption() { Width = ClientSize.Width / 2, Height = 50 };


                GUI.BeginVertical();
                {
                    GUI.BeginHorizontal();
                    {
                        // testing adding the sequencer at the start
                        GUI.AddRenderer(Renderer, Handler, seqOptions);

                        // original knobs test
                        double d = 0;
                        for (var i = 0; i < 4; i++, d += 0.21)
                        {
                            GUI.BeginVertical();
                            {
                                for (var y = 0; y < 2; y++)
                                {
                                    GUI.Knob((startValue + d) / (y + 1), 0, 1, options);
                                }
                                GUI.VerticalFader((startValue + d), 0, 1, foptions);
                                GUI.HorizontalFader((startValue + d), 0, 1, foptions2);
                            }
                            GUI.EndVertical();

                        }
                    }
                    GUI.EndHorizontal();

                    // testing adding the overview at the bottom
                    GUI.AddRenderer(Renderer2, Handler2, seqOptions2);
                }
                GUI.EndVertical();


                try
                {
                    e.Graphics.DrawImage(GUI.Render(), 0, 0);
                }
                catch (Exception ex)
                {
                }
            };*/
        }

        public ClipUIHandler Handler;
        public ClipUIHandler Handler2;
        public SequenceRenderer Renderer;
        public SequenceRenderer Renderer2;
        public readonly GuiContext GUI;
    }
}
