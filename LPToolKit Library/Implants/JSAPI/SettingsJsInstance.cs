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
    /// Provides javascript with access to the current settings for
    /// an implant type and instance.  This is different from session
    /// settings in that these stay the same for all user sessions,
    /// and should be used for configuration options, not data.
    /// </summary>
    public class SettingsJsInstance : ImplantBaseJSInstance
    {
        public SettingsJsInstance(ObjectInstance prototype)
            : base(prototype)
        {
            //this.PopulateFunctions();
            //this.PopulateFields();
        }
    }
}
