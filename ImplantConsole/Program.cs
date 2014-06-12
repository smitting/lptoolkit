using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImplantApp;
using System.Diagnostics;
using LPToolKit.Session;

namespace ImplantConsole
{
    /// <summary>
    /// Program implementing the LPToolKit app in a console, only
    /// providing a UI via the web server.
    /// </summary>
    class Program : IImplantApp
    {
        /// <summary>
        /// Launches a web page using Process.Start
        /// </summary>
        public void ShowWebPage(string url, string target = "main")
        {
            Console.WriteLine("ShowWebPage({0},{1})", url, target);
            Process.Start(url);
        }

        /// <summary>
        /// Reports errors to the main app so it can handle them 
        /// gracefully.
        /// </summary>
        public void HandleException(Exception ex)
        {
            Console.WriteLine("Exception: " + ex.ToString());
        }

        /// <summary>
        /// These app requests are ignored since they don't make sense 
        /// in a console window.
        /// </summary>
        public void HandleAppRequest(LPAppRequest request)
        {
            Console.WriteLine("Ignored HandleAppRequest({0})", request);
        }

        static void Main(string[] args)
        {
            var program = new Program();         
            UserSession.Current.Console.MessageAdded += (sender2, e2) =>
            {
                Console.WriteLine("[{0}-{1}] {2}", e2.Message.Timestamp, e2.Message.Source, e2.Message.Message);
            };

            var lpapp = new LPApplication(program);
            try
            {
                Console.WriteLine("Running.  Press enter to stop.");
                Console.ReadLine();
            }
            finally
            {
                lpapp.Stop();
            }
        }
    }
}
