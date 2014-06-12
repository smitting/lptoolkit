using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.Session;

namespace LPToolKit.Util
{
    /// <summary>
    /// Works like Console.WriteLine, but automatically detects if 
    /// the session console manager is available, and if it is not,
    /// detects if we're in a console application and can just write
    /// out any messages directly.
    /// </summary>
    internal class LPConsole
    {
        public static void WriteLine(string source, string format, params object[] args)
        {
            var s = string.Format(format, args);
            if (UserSession.Current != null)
            {
                UserSession.Current.Console.Add(s, source);
            }
            else
            {
                try
                {
                    Console.WriteLine("[{0}] {1}", source, s);
                }
                catch
                {
                    // ignore exceptions because we're not in console mode
                }
            }            
        }

    }
}
