using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.Implants;
using LPToolKit.LaunchPad;
using LPToolKit.MIDI;
using LPToolKit.Core;
using LPToolKit.Core.Tasks.ImplantEvents;

namespace LPToolKit.Session.Managers
{
    /// <summary>
    /// Storage for the data that an implant wants persisted in the session.
    /// </summary>
    /// <remarks>
    /// This was moved outside of the javascript objects because it 
    /// caused data-loss when an implant was reloaded.
    /// </remarks>
    public class ImplantSessionData
    {
        /// <summary>
        /// Value storage by key for numbers.
        /// </summary>
        public readonly Dictionary<string, double> DoubleValues = new Dictionary<string, double>();

        public readonly Dictionary<string, string> StringValues = new Dictionary<string, string>();
    }


    /// <summary>
    /// This manages the list of loaded implants, and propogates 
    /// events appropriate by coordinating with the ModeManasger.
    /// </summary>
    public class ImplantManager : SessionManagerBase
    {
        #region Constructor

        public ImplantManager(UserSession session) : base(session)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// The currently loaded implants.
        /// </summary>
        public readonly List<JavascriptImplant> Items = new List<JavascriptImplant>();

        /// <summary>
        /// Persistent session data for each implant, with implantID as the key.
        /// </summary>
        public readonly Dictionary<string, ImplantSessionData> Data = new Dictionary<string, ImplantSessionData>();

        /// <summary>
        /// All loaded implants that are currently running.
        /// </summary>
        public List<JavascriptImplant> Running
        {
            get { return Items.Where(ji => ji.Active).ToList(); }
        }

        /// <summary>
        /// Alias for ModeManager.CurrentImplants, which provides the
        /// list of implants within the current mode.
        /// </summary>
        public List<JavascriptImplant> Visible
        {
            get { return Parent.Modes.CurrentImplants; }
        }

        /// <summary>
        /// Event called just before it is passed on to implants.
        /// </summary>
        public ImplantEventHander ImplantEventTriggered;

        #endregion

        #region Methods

        /// <summary>
        /// Returns the ImplantSessionData object for a given implant
        /// by ID, so this call will work the next time the app loads.
        /// </summary>
        public ImplantSessionData GetDataForImplant(string implantId)
        {
            lock (_implantDataLock)
            {
                ImplantSessionData ret;
                if (Data.TryGetValue(implantId, out ret) == false)
                {
                    ret = new ImplantSessionData();
                    Data.Add(implantId, ret);
                }
                return ret;
            }
        }

        private readonly object _implantDataLock = new object();
        

        /// <summary>
        /// Called only by the kernel
        /// </summary>
        internal void TriggerFromKernel(ImplantEvent e)
        {
            MappedMidiDevice device = e.Hardware;

            // allow event interception
            if (ImplantEventTriggered != null)
            {
                ImplantEventTriggered(this, e);
            }

            // OSC events are special.  they are delievered to all
            // implants except their source
            // TODO: likely need an OSC registration system so only
            // implants that have registered for a specific OSC address
            // will get the message
            if (e is OscImplantEvent)
            {
                var osc = e as OscImplantEvent;
                if (osc.Osc != null)
                {
                    foreach (var implant in Items.ToArray())
                    {
                        if (implant.Active && implant.Running)
                        {
                            if (implant.GetSourceName() != osc.Osc.Source)
                            {
                                implant.Trigger(e);
                            }
                        }
                    }
                }
                return;
            }
            


            // determine which implants are to receive the event
            if (device == null || SendToAllEvents(e))
            {
                // send these events to all implants untranslated
                // TODO: how do we deal with visible here?
                foreach (var implant in Items.ToArray())
                {
                    if (implant.Active && implant.Running)
                    {
                        implant.Trigger(e);
                    }
                }
            }
            else
            {
                // send translated event to each implant.
                var targets = GetImplantsByPhysicalAddress(device, e, true);
                foreach (var implant in targets.Keys)
                {
                    implant.Trigger(targets[implant]);
                }
            }
        }

        /// <summary>
        /// Returns true if a given event is one that should go to
        /// all implants.
        /// </summary>
        private bool SendToAllEvents(ImplantEvent e)
        {
            switch (e.EventType)
            {
                case ImplantEventType.PadDoubleClick:
                case ImplantEventType.PadPress:
                case ImplantEventType.PadRelease:
                case ImplantEventType.KnobChange:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns the list of all Implants that are mapped to a given
        /// device coordinate so we never waste time sending events
        /// to implants that won't use them.
        /// </summary>
        private Dictionary<JavascriptImplant, ImplantEvent> GetImplantsByPhysicalAddress(MappedMidiDevice device, ImplantEvent e, bool visibleOnly = false)
        {
            // TODO: need to decide how to deal with modes here
            var target = visibleOnly ? Visible.ToArray() : Items.ToArray();

            var ret = new Dictionary<JavascriptImplant, ImplantEvent>();
            foreach (var implant in target)
            {
                if (implant.Active == false) continue;
                if (implant.Running == false) continue;

                int vx, vy;
                if (implant.ActiveArea.GetVirtualAddress(device, e.X, e.Y, out vx, out vy))
                {
                    // TODO: scroll these?!!!

                    // clone this event with a virtual address for this implant
                    var implantEvent = e.Clone();
                    implantEvent.X = vx;
                    implantEvent.Y = vy;
                    ret.Add(implant, implantEvent);
                }
            }
            return ret;
        }

        /// <summary>
        /// Loads, but does not start, an implant by vpath, returning the loaded object
        /// </summary>
        public JavascriptImplant Load(string vpath, RangeMap range, string oscFormat = "/osc/{x}/{y}")
        {
            lock (Items)
            {
                var implant = new JavascriptImplant(vpath, oscFormat, range);
                Items.Add(implant);
                return implant;
            }
        }

        /// <summary>
        /// Returns an implant by its id, or null if not loaded.
        /// </summary>
        public JavascriptImplant GetById(string id)
        {
            return Items.Where(ji => ji.ImplantID == id).FirstOrDefault();
        }

        /// <summary>
        /// Removes an implant from memory by implant id.
        /// </summary>
        public void Unload(string id)
        {
            lock (Items)
            {
                var item = GetById(id);
                if (item == null) throw new Exception("Could not find ImplantID " + id);
                Items.Remove(item);
                item.Destroy();
            }
        }

        /// <summary>
        /// Reloads an implant from disk.
        /// </summary>
        public void Reload(string id)
        {
            lock (Items)
            {
                var item = GetById(id);
                if (item == null) throw new Exception("Could not find ImplantID " + id);


                var vpath = item.ImplantType.VPath;
                var range = item.ActiveArea;
                var oscFormat = item.OscFormat;

                // remove the old one
                Unload(id);

                // load the new one
                Load(vpath, range, oscFormat);
            }
        }

        /// <summary>
        /// Executes an implant.
        /// </summary>
        public void Start(string id)
        {
            // TODO: this need to know when to should unpause and when it's never been started
            var item = GetById(id);
            if (item == null) throw new Exception("Could not find ImplantID " + id);
            item.ServerInit();
        }

        /// <summary>
        /// Stops an implant.
        /// </summary>
        /// <param name="id"></param>
        public void Stop(string id)
        {
            // TODO: this needs to be more robust 
            var item = GetById(id);
            if (item == null) throw new Exception("Could not find ImplantID " + id);
            item.Stop();
        }

        /// <summary>
        /// Removes all implants and destroys them.
        /// </summary>
        public void ClearAll()
        {
            lock (Items)
            {
                Items.ForEach((ji) => { ji.Destroy(); });
                Items.Clear();
            }
        }

        #endregion

        #region Private

        #endregion
    }
}
