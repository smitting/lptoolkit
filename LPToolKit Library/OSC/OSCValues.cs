using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.OSC
{
    /// <summary>
    /// Used by LaunchPadMode instances to map its values to unique
    /// OSC addresses.  Provides a way to insert a number into its
    /// address name.
    /// </summary>
    public class OSCMapping
    {
        public OSCMapping()
        {

        }

        public OSCMapping(string format)
        {
            OSCAddress = format;
        }

        /// <summary>
        /// Core address format.  The following replacement values can
        /// be used for modes that have multiple values:
        ///   {x} - x coordinate or first index
        ///   {y} - y coordinate of second index
        /// </summary>
        public string OSCAddress;

        /// <summary>
        /// The starting index to use for this mode.  Allows multiple
        /// instances to use the same general OSC format, such as
        /// having one set of faders on the top and a 2nd on the 
        /// bottom of the launchpad.
        /// </summary>
        public int StartX = 0;

        /// <summary>
        /// The starting y index to use for this mode.  This is used
        /// less often, and can only be used when X is also in use.
        /// </summary>
        public int StartY = 0;

        /// <summary>
        /// Returns the OSC address to use from the format. 
        /// </summary>
        public string FillAddress(int x, int y = 0)
        {
            var ret = OSCAddress.Replace("{x}", x.ToString());
            ret = ret.Replace("{y}", y.ToString());
            return ret;
        }

        public static string Format(string format, int x, int y = 0)
        {
            var ret = format.Replace("{x}", x.ToString());
            ret = ret.Replace("{y}", y.ToString());
            return ret;
        }
    }

    /// <summary>
    /// Stores all of the OSC values currently controlled by this 
    /// program, providing a central place for all controllers to
    /// report to, and the ability to get all changes since a
    /// certain timestamp.
    /// </summary>
    [Obsolete("OSC Values should no longer be saved.  Reading OSC should only happen via events")]
    public class OSCValues
    {
        #region Constructors

        /// <summary>
        /// Default constructor does nothing right now.
        /// </summary>
        public OSCValues()
        {
            ChangeSet = 0;
            FloatValue = new OSCDataAccess(this);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Singleton instance to be shared in simple programs where
        /// separate sets of OSC values don't need to be managed.
        /// </summary>
        public static readonly OSCValues Current = new OSCValues();

        /// <summary>
        /// Incremented everytime a value is changed, so all values 
        /// since a certain change can be retrieved.
        /// </summary>
        public int ChangeSet { get; private set; }

        /// <summary>
        /// Provides array-like access to the OSC data.
        /// </summary>
        public readonly OSCDataAccess FloatValue;

        #endregion

        #region Methods

        /// <summary>
        /// Sets an OSC value at a given address.
        /// </summary>
        public void SetValue(string address, float value)
        {
            lock (this)
            {
                OSCData data;
                if (_data.ContainsKey(address))
                {
                    data = _data[address];
                    if (data.FloatValue != value)
                    {
                        data.FloatValue = value;
                        data.ChangeSet = ++ChangeSet;
                    }
                }
                else
                {
                    data = new OSCData();
                    data.Address = address;
                    data.FloatValue = value;
                    data.ChangeSet = ++ChangeSet;
                    _data.Add(address, data);
                }
            }
        }

        /// <summary>
        /// Return the float value at a given address.  Any value that
        /// does not exist is returned as zero.
        /// </summary>
        public float GetValue(string address)
        {
            if (_data.ContainsKey(address))
            {
                return _data[address].FloatValue;
            }
            return 0;
        }

        /// <summary>
        /// Returns true iff a value has been set since a certain change set.
        /// </summary>
        public bool GetValueSince(string address, int changeSet, out float value)
        {
            if (_data.ContainsKey(address))
            {
                var data = _data[address];
                value = data.FloatValue;
                return data.ChangeSet > changeSet;
            }
            value = 0;
            return false;
        }

        /// <summary>
        /// Gets all OSC value changes since a certain timestamp.
        /// </summary>
        public Dictionary<string, float> GetChangesSince(int changeSet)
        {
            var ret = new Dictionary<string, float>();
            foreach (var key in _data.Keys)
            {
                var data = _data[key];
                if (data.ChangeSet > changeSet)
                {
                    ret.Add(key, data.FloatValue);
                }
            }
            return ret;
        }

        #endregion

        #region Nested Classes

        /// <summary>
        /// Provides a mechanism for accessing OSC data via a 
        /// simulated array.
        /// </summary>
        public class OSCDataAccess
        {
            public OSCDataAccess(OSCValues parent)
            {
                Parent = parent;
            }

            public readonly OSCValues Parent;

            public float this[string address]
            {
                get { return Parent.GetValue(address); }
                set { Parent.SetValue(address, value); }
            }
        }

        /// <summary>
        /// Information stored about each OSC value
        /// </summary>
        private class OSCData
        {
            /// <summary>
            /// The change set when this value was last changed.
            /// </summary>
            public int ChangeSet;

            /// <summary>
            /// The OSC address.
            /// </summary>
            public string Address;

            /// <summary>
            /// The value of this OSC if using float data (the default)
            /// </summary>
            public float FloatValue;
        }

        #endregion

        #region Private

        /// <summary>
        /// Actual data storage for each unique OSC address.  The 
        /// address is the key.
        /// </summary>
        private readonly Dictionary<string, OSCData> _data = new Dictionary<string, OSCData>();

        #endregion
    }
}
