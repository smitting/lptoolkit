using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.WebServer
{
    /// <summary>
    /// Contract for objects that can handle a web request.  Allows
    /// an object to focus on how it will handle a request without 
    /// knowing if the web service is running as a thread or by
    /// using kernel events.
    /// </summary>
    public interface IWebRequestHandler
    {
        /// <summary>
        /// Handles the request.
        /// </summary>
        void HandleRequest(LPWebContext context);
    }

}
