using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;

namespace LPToolKit.WebServer
{
    /// <summary>
    /// Provides the request data sent by the browser.
    /// </summary>
    /// <remarks>
    /// Needed to reinvent the HttpRequest wheel in order to avoid
    /// administrative priviledges requirement.
    /// </remarks>
    public class LPWebRequest
    {
        #region Constructors

        #endregion

        #region Properties

        /// <summary>
        /// The method of the request, like GET or POST.
        /// </summary>
        public string Method;

        /// <summary>
        /// The raw URL information of the path including the path and query.
        /// </summary>
        public string RawUrl;

        /// <summary>
        /// The requested filename.
        /// </summary>
        public string Path;

        /// <summary>
        /// The raw text after the ? or null.
        /// </summary>
        public string Query;

        /// <summary>
        /// Gets the filename this request refers to, automatically 
        /// adding index.html for directory requests.
        /// </summary>
        public string Filename
        {
            get
            {
                // grab the raw url
                var filename = RawUrl;

                // remove the query string
                var qi = filename.IndexOf("?");
                if (qi > -1)
                {
                    filename = filename.Substring(0, qi);
                }

                // if this points at a folder, just assume we really want index.html
                if (filename.EndsWith("/"))
                {
                    filename += "index.html";
                }

                return filename;
            }
        }

        /// <summary>
        /// The server headers parsed by key.
        /// </summary>
        public Dictionary<string, string> Headers = new Dictionary<string, string>();

        /// <summary>
        /// The query string parsed by key.
        /// </summary>
        public Dictionary<string, string> QueryString = new Dictionary<string, string>();

        /// <summary>
        /// Simulates HttpWebRequest by reading from the query string then the post.
        /// TODO: support post values to.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key]
        {
            get { return QueryString.ContainsKey(key) ? QueryString[key] : null; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reads the request from the recently opened socket.
        /// Will read all data until the input is complete, including
        /// post data and uploaded files.
        /// </summary>
        public static LPWebRequest Read(Socket socket)
        {
            if (socket.Connected)
            {
                var sb = new StringBuilder();

                // TODO: add file post size limit

                var buffer = new byte[1024];
                while (socket.Connected)
                {
                    // timeout after 10msec
                    if (socket.Poll(10000, SelectMode.SelectRead))
                    {
                        int bytesRead = socket.Receive(buffer, buffer.Length, 0);
                        if (bytesRead > 0)
                        {
                            sb.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                        }
                        if (bytesRead < buffer.Length || bytesRead == 0) break;
                    }
                    else
                    {
                        throw new Exception("Socket timout");
                    }
                }

                return Parse(sb.ToString());
            }
            return null;
        }

        /// <summary>
        /// Parses a web request from a byte buffer.
        /// </summary>
        public static LPWebRequest Parse(byte[] buffer, int index, int size)
        {
            return Parse(Encoding.ASCII.GetString(buffer, index, size));
        }

        /// <summary>
        /// Creates a web request record from an incoming request
        /// converted to a string.
        /// </summary>
        public static LPWebRequest Parse(string request)
        {
            var ret = new LPWebRequest();

            request = request.Replace("\r\n", "\n");
            var lines = request.Split('\n');

            // parse the first line
            if (lines.Length == 0 || string.IsNullOrEmpty(lines[0]))
            {
                throw new Exception("Invalid first line of request.");
            }

            var httpVersionIndex = lines[0].LastIndexOf(" HTTP");
            if (httpVersionIndex > -1)
            {
                lines[0] = lines[0].Substring(0, httpVersionIndex);
            }

            var blankIndex = lines[0].IndexOf(" ");
            if (blankIndex == -1)
            {
                throw new Exception("Invalid first line of request.");
            }

            ret.Method = lines[0].Substring(0, blankIndex);
            ret.RawUrl = lines[0].Substring(blankIndex + 1);

            // parse the raw url
            var qIndex = ret.RawUrl.IndexOf("?");
            if (qIndex > -1)
            {
                ret.Path = ret.RawUrl.Substring(0, qIndex);
                ret.Query = ret.RawUrl.Substring(qIndex + 1);
                foreach (var pair in ret.Query.Split('&'))
                {
                    var i = pair.IndexOf('=');
                    if (i > -1)
                    {
                        string key = pair.Substring(0, i);
                        var value = System.Net.WebUtility.UrlDecode(pair.Substring(i + 1));
                        ret.QueryString.Add(key, value);
                    }
                    else
                    {
                        ret.QueryString.Add(pair, "");
                    }
                }
            }
            else
            {
                ret.Query = null;
                ret.Path = ret.RawUrl;
            }

            // parse the headers
            for (var i = 1; i < lines.Length; i++)
            {
                if (lines[i] == "") break;
                var colon = lines[i].IndexOf(": ");
                if (colon == -1) continue;
                var key = lines[i].Substring(0, colon);
                var value = lines[i].Substring(colon + 2);
                ret.Headers.Add(key, value);
            }

            return ret;
        }

        #endregion

    }

        
}
