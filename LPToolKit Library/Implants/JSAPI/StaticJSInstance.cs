using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jurassic;
using Jurassic.Library;

namespace LPToolKit.Implants.JSAPI
{
    /// <summary>
    /// This provides javascript with a way for all instances of one
    /// implant type to share data between themselves, optionally
    /// across a network.  This data is not saved and is similar
    /// to other instance variables in RAM.
    /// </summary>
    public class StaticJSInstance : ImplantBaseJSInstance
    {
        public StaticJSInstance(ObjectInstance prototype)
            : base(prototype)
        {
            //this.PopulateFunctions();
            //this.PopulateFields();
        }
    }
}
