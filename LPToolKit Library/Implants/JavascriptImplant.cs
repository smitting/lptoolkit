using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LPToolKit.LaunchPad;
using Jurassic;
using Jurassic.Library;
using LPToolKit.Implants.JSAPI;
using LPToolKit.Session;
using LPToolKit.Sync;
using LPToolKit.MIDI;
using LPToolKit.Core;

namespace LPToolKit.Implants
{
    /// <summary>
    /// An implant implemented in javascript.
    /// </summary>
    public class JavascriptImplant : Implant
    {
        #region Constructors

        /// <summary>
        /// Loads the javascript file to be run, but does not start it.
        /// If the vpath starts with "~", then the filename is relative
        /// to the plugins folder.
        /// </summary>
        public JavascriptImplant(string vpath, string oscFormat, RangeMap activeArea) : base(activeArea)
        {
            // override the implant ID with something more useful
#warning This ImplantID will not be unique if the same implant is loaded twice!!!
            ImplantID = vpath;


            Active = true;

            ImplantType = JavascriptImplantType.Load(vpath);
            ImplantInstance = ImplantType.CreateInstance(this);
            ImplantInstance.osc.Format = oscFormat;            
        }

        ~JavascriptImplant()
        {
        }

        #endregion

        #region Properties


        /// <summary>
        /// The compiled library for this implant.
        /// </summary>
        public readonly JavascriptImplantType ImplantType;

        /// <summary>
        /// The specific javascript instance for this implant.
        /// </summary>
        public readonly ImplantJSInstance ImplantInstance;

        /// <summary>
        /// Returns the format of OSC messages sent by this instance.
        /// </summary>
        public string OscFormat
        {
            get { return ImplantInstance != null ? ImplantInstance.osc.Format : null; }
            set { if (ImplantInstance != null) ImplantInstance.osc.Format = value; }
        }

        /// <summary>
        /// The mode number this implant is assigned to.  This implant
        /// is only connected to MIDI when this number matches the
        /// current mode in the user session.
        /// </summary>
        /// <remarks>
        /// -1 means it is available on all modes
        /// </remarks>
        public int AssignedMode = 0;

        /// <summary>
        /// Events only work when this is active.
        /// </summary>
        public bool Active { get; private set; }

        /// <summary>
        /// Gets set to true after ServerInit().
        /// </summary>
        public bool Running { get; private set; }

        /// <summary>
        /// If starting the implant fails, this exception will be
        /// set with the cause.
        /// </summary>
        public Exception ServerInitException { get; private set; }

        /// <summary>
        /// Access to beat information for javascript.
        /// </summary>
        public SyncTime BeatSync 
        { 
            get { return Session.Sync.BeatSync; } 
        }

        /// <summary>
        /// The session this implant is running on.
        /// </summary>
        public UserSession Session
        {
            get { return UserSession.Current; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the current status string describing the state
        /// of this implant.
        /// </summary>
        public string GetStatus()
        {
            if (ServerInitException != null)
            {
                return "error";
            }
            else if (Running)
            {
                return "running";
            }
            else if (Active)
            {
                return "loaded";
            }
            else
            {
                return "not loaded";
            }
        }

        /// <summary>
        /// Triggers a javascript event on this implant.  Events not
        /// in range will be simply ignored.
        /// </summary>
        public void Trigger(ImplantEvent e)  
        {
            if (ImplantInstance != null)
            {
                ImplantInstance.Trigger(e);
            }
        }

        /// <summary>
        /// The API sends any javascript errors received here.
        /// </summary>
        public void HandleJavascriptError(Exception ex)
        {
            // TODO:
            if (ex is JavaScriptException)
            {
                var jse = ex as JavaScriptException;
                string msg = string.Format("Javascript error in ImplantType {0}\n\tLine: {1}\n\tFunction: {2}\n\tMessage: {3}\n", ImplantType.Name, jse.LineNumber, jse.FunctionName, jse.Message);
                Session.Console.Add(msg, ImplantType.Name);                
            }
            else
            {
                string msg = string.Format("Exception of type {0} in {1}: {2}", ex.GetType().FullName, ImplantType.Name, ex.ToString());
                Session.Console.Add(msg, ImplantType.Name);
            }
            //Console.ReadLine();
        }

        /// <summary>
        /// Runs the server-side initialization javascript.
        /// </summary>
        public void ServerInit()
        {          
            // TODO: this call should be ignored if the application has not yet started


            if (ImplantType == null)
            {
                throw new Exception("ImplantType not loaded.");
            }
            if (ImplantType.Constructor == null)
            {
                throw new Exception("Implant constructor not loaded.");
            }

            try
            {
                ServerInitException = null;

                // run the constructor
                ImplantType.Constructor.Call(ImplantInstance, ImplantInstance);
                Running = true;
            }
            catch (Exception ex)
            {
                ServerInitException = ex;
                HandleJavascriptError(ex);
            }
        }

        /// <summary>
        /// Pauses the implant.
        /// </summary>
        public void Stop()
        {
            Running = false;
        }

        /// <summary>
        /// Unload all the events and javascript
        /// </summary>
        public void Destroy()
        {
            // make it so the next instance reloads from file
            ImplantType.Constructor = null;

            // stop all current events.
            Running = false;
            Active = false;
        }

        /// <summary>
        /// Generates a unique name for each instance to identify this
        /// implant as the OSC source for midi mapping.
        /// </summary>
        public override string GetSourceName()
        {
            // TODO: combine name with implant id to make a good display for user that identifies a unique implant.
            //list.Add(i.ImplantID);
            return ImplantType.Name;
        }

        #endregion

    }
}
