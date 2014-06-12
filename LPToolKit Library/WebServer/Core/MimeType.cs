using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LPToolKit.WebServer
{
    /// <summary>
    /// Web-server file information based on extension.
    /// </summary>
    public class MimeType
    {
        #region Properties

        public string Ext;
        public string ContentType;
        public bool IsBinary;

        #endregion

        #region Static Utility

        /// <summary>
        /// Predefined mime-types.
        /// </summary>
        public static MimeType[] MimeTypes = new MimeType[] {
            new MimeType() { Ext = ".txt", ContentType="text/plain", IsBinary=false},
            new MimeType() { Ext = ".html", ContentType="text/html", IsBinary=false},
            new MimeType() { Ext = ".js", ContentType="text/javascript", IsBinary=false},
            new MimeType() { Ext = ".css", ContentType="text/css", IsBinary=false},
            new MimeType() { Ext = ".png", ContentType="image/png", IsBinary=true},
            new MimeType() { Ext = ".svg", ContentType="image/svg+xml", IsBinary=true},
            new MimeType() { Ext = ".ttf", ContentType="application/x-font-truetype", IsBinary=true},
            new MimeType() { Ext = ".otf", ContentType="application/x-font-opentype", IsBinary=true},
            new MimeType() { Ext = ".woff", ContentType="application/font-woff", IsBinary=true},
            new MimeType() { Ext = ".eot", ContentType="application/vnd.ms-fontobject", IsBinary=true},
        };

        public static MimeType GetMimeType(string path)
        {
            var ext = Path.GetExtension(path.Trim()).ToLower();
            foreach (var type in MimeTypes)
            {
                if (type.Ext == ext)
                {
                    return type;
                }
            }
            return MimeTypes[0]; // first one is the default
        }

        #endregion
    }
}
