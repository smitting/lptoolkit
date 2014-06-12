using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using LPToolKit.Platform;

namespace LPToolKit.Util
{
    /// <summary>
    /// Tools for controlling hardware level CPU processing.
    /// </summary>
    internal class CPUCore
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentThread();

        [DllImport("kernel32.dll")]
        static extern IntPtr SetThreadAffinityMask(IntPtr hThread, IntPtr dwThreadAffinityMask);

        /// <summary>
        /// Forces the calling thread to a specific core if the 
        /// operating system supports this (OS X does not for example)
        /// </summary>
        public static void SetThreadProcessor(int processor)
        {
            switch (Platform.OS.Platform)
            {
                case Platforms.Windows:
                    SetThreadAffinityMask(GetCurrentThread(), new IntPtr(1 << processor));
                    break;
            }
        }
    }
}
