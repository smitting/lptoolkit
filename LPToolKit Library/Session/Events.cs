using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.Session.Managers;
using LPToolKit.MIDI;
using LPToolKit.Logs;

namespace LPToolKit.Session
{
    /// <summary>
    /// Event trigger when a new message is ready for the text console.
    /// </summary>
    public delegate void NewConsoleMessageEventHandler(object sender, NewConsoleMessageEventArgs e);

    /// <summary>
    /// Arguments sent with the event when a new message is sent to
    /// the text console.
    /// </summary>
    public class NewConsoleMessageEventArgs : EventArgs
    {
        public ConsoleMessage Message;
    }

}
