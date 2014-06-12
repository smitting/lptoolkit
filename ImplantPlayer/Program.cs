
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

using Awesomium;
using Awesomium.Core;
using Awesomium.Web;
using Awesomium.Windows;
namespace ImplantPlayer
{
    static class Program
    {
        public static Form1 MainWindow;
        public static ConsoleForm ConsoleWindow;
        public static LaunchPadSimForm SimWindow = null;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MainWindow = new Form1();
            ConsoleWindow = new ConsoleForm();
            Application.Run(MainWindow);
        }
    }
}
   