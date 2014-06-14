using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Awesomium;
using Awesomium.Core;
using Awesomium.Web;
using Awesomium.Windows;
using ImplantApp;
using LPToolKit.Session;
namespace ImplantPlayer
{
    public partial class Form1 : Form, IImplantApp
    {

        public Form1()
        {
            InitializeComponent();

            // quit app when closing
            this.FormClosing += (sender, e) =>
            {
                try
                {
                    LPApp.Stop();
                    if (webControl1 != null)
                    {
                        this.Controls.Remove(webControl1);
                        webControl1.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "SYSTEM ERROR");
                }
                try
                {
                    Awesomium.Core.WebCore.Shutdown();
                }
                catch
                {

                }
                Application.Exit();
            };


        }

        private void ShowSimulator()
        {
            if (Program.SimWindow == null)
            {
                Program.SimWindow = new LaunchPadSimForm();
            }
            //LPToolKit.MIDI.MappedMidiDevice padDevice = UserSession.Current.Devices[LPToolKit.MIDI.MidiDeviceMapping.PadDevice].FirstOrDefault();
            LPToolKit.MIDI.MappedMidiDevice padDevice = UserSession.Current.Devices[typeof(LPToolKit.MIDI.Hardware.LaunchPadHardwareInterface)].FirstOrDefault();
            if (padDevice.Device is LPToolKit.LaunchPad.LaunchPadSimulator)
            {
                Program.SimWindow.Simulator = padDevice.Device as LPToolKit.LaunchPad.LaunchPadSimulator;
            }
            else
            {
                Program.SimWindow.Simulator = null;
            }


            Program.SimWindow.Show();
        }



        public void HandleAppRequest(LPAppRequest request)
        {
            //MessageBox.Show("TODO: HandleAppRequest(" + request.ToString() + ")", "App Request");
            if (request == LPAppRequest.ShowLaunchPadSimulator)
            {
                this.BeginInvoke((MethodInvoker) delegate(){ ShowSimulator(); });
            }
        }


        /// <summary>
        /// Launches a web page in the internal browser.
        /// </summary>
        public void ShowWebPage(string url, string target = "main")
        {
            try
            {
                if (webControl1 != null)
                {
                    webControl1.Source = new Uri(url);
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Reports errors to the main app so it can handle them 
        /// gracefully.
        /// </summary>
        public void HandleException(Exception ex)
        {
            try
            {
                Console.WriteLine("ImplantApp Exception: " + ex.ToString());
            }
            catch
            {

            }
        }


        LPApplication LPApp;



        private void Form1_Shown(object sender, EventArgs e)
        {
            // start up the app
            LPApp = new LPApplication(this);

            UserSession.Current.Console.MessageAdded += (sender2, e2) =>
                {
                    Program.ConsoleWindow.AddMessage(e2.Message.Timestamp, e2.Message.Message, e2.Message.Source);
                };

            // open the window with test test gui
            testGui = new GuiTestForm();
            testGui.Show();

        }
        GuiTestForm testGui;

        /// <summary>
        /// Closes the current window, which should close the program.
        /// </summary>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// Opens the console window.
        /// </summary>
        private void consoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Program.ConsoleWindow.Show(this);
        }

        /// <summary>
        /// Opens the user interface in the default web browser instead
        /// of the build in web browser.
        /// </summary>
        private void openUIInWebBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {

            //System.Diagnostics.Process.Start(LPApp.WebServer.GetUrl());

        }

        private void launchPadSImulatorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowSimulator();
        }



    }
}
