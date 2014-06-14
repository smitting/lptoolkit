using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jurassic;
using Jurassic.Library;
using LPToolKit.Implants.JSAPI;
using System.IO;
using LPToolKit.LaunchPad;
using System.Configuration;
using LPToolKit.Util;

namespace LPToolKit.Implants
{
    /// <summary>
    /// Each unique filename loaded get an implant type associated 
    /// with it that stores the source code post-processing, a
    /// unique ID, and a handle to the constructor function supplied
    /// by the plugin file.
    /// </summary>
    public class JavascriptImplantType
    {
        #region Constructors

        /// <summary>
        /// Constructor is private because types are shared by unique
        /// filenames.  Use Load() or Get() instead.
        /// </summary>
        private JavascriptImplantType(string id)
        {
            ID = id;
        }

        /// <summary>
        /// Sets up the javascript engine, global variables, and injection code.
        /// </summary>
        static JavascriptImplantType()
        {
            // create the engine with global native-code classes
            JavascriptEngine = new ScriptEngine();
            JavascriptEngine.SetGlobalValue("LPToolKit", new JSAPI.LPToolKit(JavascriptEngine));

            EventFactory = new EventJSConstructor(JavascriptEngine);

            // create all the factory classes for native-code object instances
            ImplantFactory = new ImplantJSConstructor(JavascriptEngine);
            OscFactory = new OscJSConstructor(JavascriptEngine);
            PadFactory = new PadJSConstructor(JavascriptEngine);
            KnobFactory = new KnobJSConstructor(JavascriptEngine);
            KeysFactory = new MidiKeysJSConstructor(JavascriptEngine);
            GuiFactory = new GuiJSConstructor(JavascriptEngine);
            TimeFactory = new TimeJSConstructor(JavascriptEngine);
            SettingsFactory = new SettingsJSConstructor(JavascriptEngine);
            StaticFactory = new StaticJSConstructor(JavascriptEngine);
            SessionFactory = new SessionJSConstructor(JavascriptEngine);
            ModesFactory = new ModesJSConstructor(JavascriptEngine);

            // load the injection
            InjectionCode = FileIO.LoadTextFile(new FilePath()
                {
                    BaseFolder = Core.Settings.ImplantFolder,
                    Filename = "~/system/ImplantBase.js",
                    Source = "InjectionCode"
                });
        }

        #endregion

        #region Properties

        /// <summary>
        /// Javascript environment shared by all implants.
        /// </summary>
        public static readonly ScriptEngine JavascriptEngine;

        /// <summary>
        /// Shared constructor used by all implants.
        /// </summary>
        public static readonly EventJSConstructor EventFactory;

        /// <summary>
        /// Javascript injected to the start of every implant.
        /// </summary>
        public static readonly string InjectionCode;

        /// <summary>
        /// The type id for this implant type.
        /// </summary>
        public readonly string ID;

        /// <summary>
        /// The name of this implant type.  For now is the vpath.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The path of the source code file relative to the implant root.
        /// </summary>
        public string VPath 
        {
            get { return Path.Filename; }
            private set { Path.Filename = value; }
        }

        /// <summary>
        /// The full path to the implant file.
        /// </summary>
        public FilePath Path 
        {
            get
            {
                return _path ?? (_path = new FilePath() { BaseFolder = Core.Settings.ImplantFolder, Filename = null, Source = "JavascriptImplantType" });
            }
        }

        private FilePath _path = null;

        /// <summary>
        /// The javascript function to be called whenever a new
        /// instance is created.
        /// </summary>
        public FunctionInstance Constructor;

        /// <summary>
        /// The javascript source loaded for this implant type
        /// </summary>
        public string SourceCode { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a unique instance of the plugin type, with new
        /// instances of all native-code instances needed to access
        /// the entire hardware API.
        /// </summary>
        public ImplantJSInstance CreateInstance(JavascriptImplant parent)
        {
            var implant = ImplantFactory.Construct(parent);
            implant.osc = OscFactory.Construct(parent);
            implant.pads = PadFactory.Construct(parent);
            implant.knobs = KnobFactory.Construct(parent);
            implant.keys = KeysFactory.Construct(parent);
            implant.gui = GuiFactory.Construct(parent);
            implant.time = TimeFactory.Construct(parent);
            implant.settings = SettingsFactory.Construct(parent);
            implant.shared = StaticFactory.Construct(parent);
            implant.session = SessionFactory.Construct(parent);
            implant.mode = ModesFactory.Construct(parent);

            return implant;
        }

        #endregion

        #region Static Methods
        /// <summary>
        /// Returns the implant type object by id.  Only one type can
        /// be registered for each ID.
        /// </summary>
        public static JavascriptImplantType Get(string id)
        {
            JavascriptImplantType ret;
            lock (_lock)
            {
                if (!_implantTypes.TryGetValue(id, out ret))
                {
                    ret = new JavascriptImplantType(id);
                    _implantTypes.Add(id, ret);
                }
            }
            return ret;
        }

        /// <summary>
        /// Returns the type for a given vpath.  The first time this
        /// vpath is loaded, the javascript is loaded and run to
        /// register the constructor for this type.
        /// </summary>
        public static JavascriptImplantType Load(string vpath)
        {
            var ret = Get(GetIDForPath(vpath));
            if (ret.Constructor == null)
            {
                ret.VPath = vpath;
                ret.Name = vpath;

                // load source code into wrapper 
                var injection = InjectionCode;
                injection = injection.Replace("%%typeid%%", ret.ID);
                injection = injection.Replace("%%script%%", Util.FileIO.LoadTextFile(ret.Path)); // LoadJavascriptFile(vpath));
                ret.SourceCode = injection;

                // run javascript to get the constructor method
                JavascriptEngine.Execute(injection);
            }
            return ret;
        }

        #endregion

        #region Private

        /// <summary>
        /// Returns a unique ID for each path provided.
        /// </summary>
        private static string GetIDForPath(string path)
        {
            string id;
            lock (_lock)
            {
                if (!_pathIDMap.TryGetValue(path, out id))
                {
                    id = "ImplantType" + (_nextID++);
                    _pathIDMap.Add(path, id);
                }
            }
            return id;
        }

        /// <summary>
        /// Storage of all instances by type id.
        /// </summary>
        private static Dictionary<string, JavascriptImplantType> _implantTypes = new Dictionary<string, JavascriptImplantType>();

        /// <summary>
        /// Maps paths to IDs.
        /// </summary>
        private static Dictionary<string, string> _pathIDMap = new Dictionary<string, string>();

        /// <summary>
        /// Used to thread safe the creation of IDs
        /// </summary>
        private static object _lock = new object();

        /// <summary>
        /// The next ID that will be generated.
        /// </summary>
        private static int _nextID = 1;

        /// <summary>
        /// Instances that creates plugin instances.
        /// </summary>
        private static ImplantJSConstructor ImplantFactory;
        private static OscJSConstructor OscFactory;
        private static PadJSConstructor PadFactory;
        private static KnobJSConstructor KnobFactory;
        private static MidiKeysJSConstructor KeysFactory;
        private static GuiJSConstructor GuiFactory;
        private static TimeJSConstructor TimeFactory;
        private static SettingsJSConstructor SettingsFactory;
        private static StaticJSConstructor StaticFactory;
        private static SessionJSConstructor SessionFactory;
        private static ModesJSConstructor ModesFactory;



        #endregion


    }
}
