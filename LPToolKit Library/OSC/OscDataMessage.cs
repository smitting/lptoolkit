using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.OSC
{
    /// <summary>
    /// Standardized format for an OSC message transmitted around the
    /// system, similar to MidiMessage. 
    /// </summary>
    /// <remarks>
    /// The entire OSC format is not handled by this object, only the
    /// parts we're interested for this application.  Couldn't be called
    /// OscMessage because of conflicts with the library we are using
    /// (for now)
    /// </remarks>
    public class OscDataMessage
    {
        /// <summary>
        /// The OSC address
        /// </summary>
        public string Address;

        /// <summary>
        /// Description of where this came from.
        /// TODO: this should probably be an object
        /// </summary>
        public string Source;

        // TODO: need to have built in support for variables within the address

        /// <summary>
        /// Provides access to the first array element.
        /// </summary>
        public double Value
        {
            get { return Values.Length == 0 ? 0.0 : Values[0]; }
            set 
            { 
                if (Values.Length == 0)
                {
                    SetSize(1);
                }
                Values[0] = value; 
            }
        }

        /// <summary>
        /// The values for this message, which will be downcast into
        /// floats when transmitted.
        /// </summary>
        public double[] Values = new double[1];

        public float[] ValuesAsFloat
        {
            get
            {
                if (Values == null)
                {
                    return new float[0];
                }

                var ret = new float[Values.Length];
                for (var i = 0; i < Values.Length; i++ )
                {
                    ret[i] = (float)Values[i];
                }
                return ret;
            }
        }

        public float ValueAsFloat
        {
            get { return (float)Value; }
        }

        /// <summary>
        /// Resizes the number of doubles to store in this message.
        /// </summary>
        public void SetSize(int size)
        {
            if (size < 1)
            {
                throw new InvalidOperationException("The size cannot be less than 1");
            }
            var old = Values;
            Values = new double[size];
            if (old != null)
            {
                for (var i = 0; i < size; i++)
                {
                    Values[i] = i < old.Length ? old[i] : 0;
                }
            }
        }
    }
}
