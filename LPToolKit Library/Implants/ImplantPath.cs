using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LPToolKit.Implants
{
    /// <summary>
    /// Facilliates the use of "~" in implant filenames so they can
    /// be relative to a root implant folder.
    /// </summary>
    public class ImplantPath
    {
        /// <summary>
        /// The folder that implant files are relative to.
        /// </summary>
        public static string ImplantRoot = ".\\";        

        /// <summary>
        /// Returns a list of all virtual paths contained under the implant root.
        /// </summary>
        public static List<string> GetImplantFiles()
        {
            var ret = new List<string>();

            foreach (var filename in Directory.GetFiles(ImplantRoot, "*.js", SearchOption.AllDirectories))
            {
                var vpath = filename.Replace(ImplantRoot, "~").Replace("\\", "/");
                if (vpath.StartsWith("~system/") == false)
                {
                    if (Path.GetFileName(vpath).StartsWith(".") == false)
                    {
                        ret.Add(vpath);
                    }
                }
            }

            return ret;
        }
    }
}
