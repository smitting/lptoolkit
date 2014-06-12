using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jurassic;
using Jurassic.Library;
using LPToolKit.Session.Managers;

namespace LPToolKit.Implants.JSAPI
{
    /// <summary>
    /// Provides javascript with a set of session data to be stored
    /// for this implant instance.  This data is persisted and 
    /// restored when the user reloads a saved session, similar
    /// to sessions in other DAW software.
    /// </summary>
    public class SessionJSInstance : ImplantBaseJSInstance
    {
        public SessionJSInstance(ObjectInstance prototype)
            : base(prototype)
        {
            //this.PopulateFunctions();
            //this.PopulateFields();
        }

        #region Properties



        /// <summary>
        /// Grabs the reference in the ImplantManager for session storage.
        /// Creates the reference as needed.
        /// </summary>
        public ImplantSessionData DataStorage
        {
            get
            {
                return _dataStorage ?? (_dataStorage = Parent.Session.Implants.GetDataForImplant(Parent.ImplantID));
            }
        }
        private ImplantSessionData _dataStorage = null;

        /// <summary>
        /// The reference for the number key/value pair persistent storage.
        /// </summary>
        public Dictionary<string, double> DoubleValues
        {
            get { return DataStorage.DoubleValues; }
        }

        /// <summary>
        /// The reference for the string key/value pair persistent storage.
        /// </summary>
        public Dictionary<string, string> StringValues
        {
            get { return DataStorage.StringValues; }
        }
            
        //public readonly Dictionary<string, double> DoubleValues = new Dictionary<string, double>();

        #endregion

        #region Javascript Methods

        /// <summary>
        /// Returns true iff the given key is in the session data.
        /// </summary>
        [JSFunction(Name = "has")]
        public bool Has(string key)
        {
            return DoubleValues.ContainsKey(key);
        }

        /// <summary>
        /// </summary>
        [JSFunction(Name = "get")]
        public double GetDouble(string key)
        {
            double ret = 0;
            DoubleValues.TryGetValue(key, out ret);
            return ret;
        }


        /// <summary>
        /// </summary>
        [JSFunction(Name = "set")]
        public void SetDouble(string key, double value)
        {
            DoubleValues[key] = value;
            Parent.Session.Save();
        }

        /// <summary>
        /// Returns true iff the given key is in the session data.
        /// </summary>
        [JSFunction(Name = "hasString")]
        public bool HasString(string key)
        {
            return StringValues.ContainsKey(key);
        }

        /// <summary>
        /// </summary>
        [JSFunction(Name = "getString")]
        public string GetString(string key)
        {
            string ret = null;
            StringValues.TryGetValue(key, out ret);
            return ret;
        }


        [JSFunction(Name = "setString")]
        public void SetString(string key, string value)
        {

            StringValues[key] = value;
            Parent.Session.Save();
        }


        #endregion

        #region Private


        #endregion

    }
}
