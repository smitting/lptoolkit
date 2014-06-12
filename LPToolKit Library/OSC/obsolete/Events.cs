using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bespoke.Common.Osc;
using System.Net;
using System.Net.Sockets;

namespace LPToolKit.OSC
{
    public delegate void LaunchPadMessageEventHandler(object sender, LaunchPadMessageEventArgs e);

    public delegate void LaunchPadUpdateEventHandler(object sender, LaunchPadUpdateEventArgs e);

    public delegate void LaunchPadStepEventHandler(object sender, LaunchPadStepEventArgs e);

    public delegate void LaunchPadMessageError(Exception ex);

    /// <summary>
    /// Base class for all message event arguments.
    /// </summary>
    public class LaunchPadMessageEventArgs : EventArgs
    {
        public LaunchPadMessageEventArgs(OscMessage msg)
        {
            Message = msg.Address;
            FloatValues = new float[msg.Data.Count];
            for (var i = 0; i < msg.Data.Count; i++)
            {
                if (msg.Data[i] is float)
                {
                    FloatValues[i] = msg.At<float>(i);
                }
                else
                {
                    FloatValues[i] = 0f;
                }
            }
        }

        /// <summary>
        /// The OSC address of the packet received.
        /// </summary>
        public string Message;

        /// <summary>
        /// Raw float argument data.  Any values that are not of a
        /// float type are set to zero.
        /// </summary>
        public float[] FloatValues;
    }

    /// <summary>
    /// Data received from update message, where Reaktor sends grid
    /// data to this program.
    /// </summary>
    public class LaunchPadUpdateEventArgs : LaunchPadMessageEventArgs
    {
        public LaunchPadUpdateEventArgs(OscMessage msg)
            : base(msg)
        {
            if (FloatValues.Length < 3)
            {
                throw new ArgumentException("Invalid update message received.  Expected 3 arguments, got " + FloatValues.Length);
            }
            X = (int)FloatValues[0];
            Y = (int)FloatValues[1];
            Value = (int)FloatValues[2];
        }
        public int X;
        public int Y;
        public int Value;
    }

    /// <summary>
    /// Data received from step message, where Reaktor says which 
    /// beat should currently be highlighted.
    /// </summary>
    public class LaunchPadStepEventArgs : LaunchPadMessageEventArgs
    {
        public LaunchPadStepEventArgs(OscMessage msg)
            : base(msg)
        {
            if (FloatValues.Length < 1)
            {
                throw new ArgumentException("Invalid step message received.  Expected 1 argument, got " + FloatValues.Length);
            }
            Step = (int)FloatValues[0];
        }
        public int Step;
    }

}
