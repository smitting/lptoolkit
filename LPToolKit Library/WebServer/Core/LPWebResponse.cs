using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.WebServer
{
    /// <summary>
    /// Equivalent of HttpWebResponse for LPToolKit
    /// </summary>
    public class LPWebResponse
    {
        #region Constructors

        /// <summary>
        /// Constructor accepting context that the response will be 
        /// sent back to.
        /// </summary>
        public LPWebResponse(LPWebContext context)
        {
            Context = context;
            HeaderSent = false;
            ContentLength64 = -1;
            StatusCode = 200;
            ContentType = "text/html";
        }

        #endregion

        #region Properties

        /// <summary>
        /// The context this response is for.
        /// </summary>
        public readonly LPWebContext Context;

        /// <summary>
        /// The name reported as the server.
        /// </summary>
        public const string Server = "LPToolKit v?.?? alpha";

        /// <summary>
        /// The mime-type to send with the file.
        /// </summary>
        public string ContentType
        {
            get { return _contentType; }
            set { AssertHeadersNotSent(); _contentType = value; }
        }

        /// <summary>
        /// The HTTP status code for this request.
        /// </summary>
        public int StatusCode
        {
            get { return _statusCode; }
            set { AssertHeadersNotSent(); _statusCode = value; }
        }

        /// <summary>
        /// Optional length of the data in the response.
        /// </summary>
        public long ContentLength64
        {
            get { return _contentLength64; }
            set { AssertHeadersNotSent(); _contentLength64 = value; }
        }

        /// <summary>
        /// Gets set to true when the headers are sent.
        /// </summary>
        public bool HeaderSent { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Detects if a file is binary or text and sends appropriately.
        /// </summary>
        public void SendFile(Util.FilePath path)
        {
            if (path.Exists == false)
            {
                Send404();
            }
            else
            {
                var mimeType = MimeType.GetMimeType(path.FullPath);
                if (mimeType.IsBinary)
                {
                    var data = Util.FileIO.LoadBinaryFile(path);
                    StatusCode = 200;
                    ContentType = mimeType.ContentType;
                    ContentLength64 = data.Length;
                    Write(data);
                    Close();
                }
                else
                {
                    var data = Util.FileIO.LoadTextFile(path);
                    StatusCode = 200;
                    ContentType = mimeType.ContentType;
                    ContentLength64 = data.Length;
                    Write(data);
                    Close();
                }
            }
        }

        /// <summary>
        /// Sends a 404 file not found error.
        /// </summary>
        public void Send404()
        {
            string data = "file not found: " + Context.Request.RawUrl;
            StatusCode = 404;
            ContentType = "text/plain";
            ContentLength64 = data.Length;
            Write(data);
            Close();
        }

        /// <summary>
        /// Sends a 500 server error.
        /// </summary>
        public void Send500(string message = "Server Error")
        {
            StatusCode = 500;
            ContentType = "text/plain";
            ContentLength64 = message.Length;
            Write(message);
            Close();
        }

        /// <summary>
        /// Sends a string as part of the response.
        /// </summary>
        public void Write(string s)
        {
            Write(Encoding.ASCII.GetBytes(s));
            //_buffer.Append(s);
        }

        /// <summary>
        /// Sends binary data as part of the response.
        /// </summary>
        public void Write(byte[] b)
        {
            Flush();
            InternalWrite(b);
        }

        /// <summary>
        /// Ends the connecftion.
        /// </summary>
        public void Close()
        {
            Flush();
            Context.Finish();
        }

        public void Flush()
        {
            /*
            if (_buffer.Length > 0)
            {
                InternalWrite(Encoding.ASCII.GetBytes(_buffer.ToString()));
                _buffer.Clear();
            }*/
        }
        

        /// <summary>
        /// Sends the headers to the response before data is sent to
        /// the browser.  Automatically called the first time data
        /// is sent on a response and is only sent once.
        /// </summary>
        public void SendHeaders()
        {
            if (HeaderSent == false)
            {
                var sb = new StringBuilder();
                sb.AppendLine("HTTP/1.1 " + StatusCode + " OK");
                sb.AppendLine("Server: " + Server);
                sb.AppendLine("Content-Type: " + ContentType);
                if (ContentLength64 >= 0)
                {
                    sb.AppendLine("Content-Length: " + ContentLength64);
                }
                sb.AppendLine();

                _Send(Encoding.ASCII.GetBytes(sb.ToString()));
                HeaderSent = true;
            }
        }

        #endregion

        #region Private

        private void AssertHeadersNotSent()
        {
            if (HeaderSent)
            {
                throw new InvalidOperationException("Headers cannot be changed after the headers have been sent.");
            }
        }
        
        private int _statusCode;
        private string _contentType;
        private long _contentLength64;

        //private StringBuilder _buffer = new StringBuilder();

        private void InternalWrite(byte[] b)
        {
            if (!HeaderSent) SendHeaders();
            _Send(b);
        }

        private void _Send(byte[] b)
        {
            try
            {
                if (Context.ConnectedSocket.Connected)
                {
                    Context.ConnectedSocket.Send(b);
                }
            }
            catch
            {
                // TODO: should we care if this fails?
            }
        }

        #endregion
    }
}
