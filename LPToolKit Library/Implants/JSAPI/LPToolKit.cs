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
    /// Native code class for javascript implants that provide
    /// low-level functionality, normally for system libraries
    /// only.
    /// </summary>
    public class LPToolKit : ObjectInstance
    {
        public LPToolKit(ScriptEngine engine)
            : base(engine)
        {
            this.PopulateFunctions();
            this.PopulateFields();
        }

        /// <summary>
        /// Version string issued to implants.
        /// </summary>
        [JSField()]
        public static string version = "LPToolKit v0.01 alpha";

        /// <summary>
        /// Version as number supplied to implants in case they need to
        /// do version checks in the future as features are added for
        /// backwards compatibility
        /// </summary>
        [JSField()]
        public static float versionNumber = 0.01f;

        /// <summary>
        /// Called by the wrapper for all implant scripts to provide
        /// a constructor function for an implant type.
        /// </summary>
        [JSFunction(Name = "registerImplant")]
        public static void RegisterImplant(string implantId, FunctionInstance fn)
        {
            var implant = JavascriptImplantType.Get(implantId);
            implant.Constructor = fn;
        }
    }
}
