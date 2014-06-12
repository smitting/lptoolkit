using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bespoke.Common.Osc;
using System.Net;
using System.Net.Sockets;
using LPToolKit.Util;

namespace LPToolKit.OSC
{
    /// <summary>
    /// Manages communications with Ableton Live 9 when OSC support
    /// is installed.  This is available via the LiveOSC plugins
    /// hosted at http://livecontrol.q3f.org/ableton-liveapi/liveosc/
    /// 
    /// TODO: implement this in an implant
    /// </summary>
    [Obsolete]
    public class AbletonOSC
    {
        /// <summary>
        /// Constructor accepts ableton session to target.
        /// </summary>
        public AbletonOSC(List<string> targets)
        {
            Targets = new List<IPEndPoint>();
            foreach (var target in targets)
            {
                Targets.Add(NetUtil.CreateIPEndPoint(target, AbletonPort));
            }
        }

        /// <summary>
        /// Accepts ableton targets as a variable parameter list.
        /// </summary>
        /// <param name="targets"></param>
        public AbletonOSC(params string[] targets)
            : this(new List<string>(targets))
        {
        }

        #region Settings

        /// <summary>
        /// The UDP port that Ableton Live listens on.
        /// </summary>
        public static int AbletonPort = 9000;

        /// <summary>
        /// The UDP port that Ableton Live send messages to.
        /// </summary>
        public static int AbletonListenPort = 9001;

        #endregion

        #region Properties

        /// <summary>
        /// All Ableton sessions to send UDP messages to.
        /// </summary>
        public readonly List<IPEndPoint> Targets;

        /// <summary>
        /// Sets the synced tempo.
        /// </summary>
        public int Tempo
        {
            get { return _tempo; }
            set
            {
                if (_tempo != value)
                {
                    _tempo = value;
                    Send("/live/tempo", _tempo);
                }
            }
        }
        private int _tempo = 120;

        #endregion

        #region Methods

        /// <summary>
        /// Syncs and starts playback of all sessions.
        /// </summary>
        public void Play()
        {
            Send("/live/tempo", _tempo);
            Send("/live/play");
        }

        /// <summary>
        /// Stops playback on all sessions.
        /// </summary>
        public void Stop()
        {
            Send("/live/stop");
        }

        /// <summary>
        /// Sends an OSC message to all connected Ableton Live sessions.
        /// </summary>
        public void Send(string message, params float[] args)
        {
            var msg = new OscMessage(SourceEndPoint, message);
            foreach (var f in args)
            {
                msg.Append(f);
            }
            foreach (var target in Targets)
            {
                msg.Send(target);
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// Where to send UDP messages from.
        /// </summary>
        protected static IPEndPoint SourceEndPoint = new IPEndPoint(IPAddress.Loopback, 9999);

        #endregion
    }
}
