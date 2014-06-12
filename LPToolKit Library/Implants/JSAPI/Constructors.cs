using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jurassic;
using Jurassic.Library;

namespace LPToolKit.Implants.JSAPI
{
    // *** Note ***
    // This file contains all of the constructor factory classes for
    // native code classes available from javascript within implants.
    //

    /// <summary>
    /// Jurassic constructor class for the "this" instance sent to
    /// the implant javascript.
    /// </summary>
    public class ImplantJSConstructor : ClrFunction
    {
        public ImplantJSConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "ImplantObject", new ImplantJSInstance(engine.Object.InstancePrototype))
        {

        }

        [JSConstructorFunction]
        public ImplantJSInstance Construct()
        {
            return new ImplantJSInstance(this.InstancePrototype);
        }

        public ImplantJSInstance Construct(JavascriptImplant parent)
        {
            var ret = Construct();
            ret.Parent = parent;
            return ret;
        }

    }

    /// <summary>
    /// </summary>
    public class EventJSConstructor : ClrFunction
    {
        public EventJSConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "LaunchPadEvent", new ImplantJSInstance(engine.Object.InstancePrototype))
        {

        }

        [JSConstructorFunction]
        public EventJSInstance Construct()
        {
            return new EventJSInstance(this.InstancePrototype);
        }

    }

    /// <summary>
    /// Jurassic constructor class for OscJSInstance.
    /// </summary>
    public class OscJSConstructor : ClrFunction
    {
        public OscJSConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "OscManager", new ImplantJSInstance(engine.Object.InstancePrototype))
        {

        }

        [JSConstructorFunction]
        public OscJSInstance Construct()
        {
            return new OscJSInstance(this.InstancePrototype);
        }

        public OscJSInstance Construct(JavascriptImplant parent)
        {
            var ret = Construct();
            ret.Parent = parent;
            return ret;
        }
    }

    public class PadJSConstructor : ClrFunction
    {
        public PadJSConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "Pads", new PadJSInstance(engine.Object.InstancePrototype))
        {

        }


        [JSConstructorFunction]
        public PadJSInstance Construct()
        {
            return new PadJSInstance(this.InstancePrototype);
        }

        public PadJSInstance Construct(JavascriptImplant parent)
        {
            var ret = Construct();
            ret.Parent = parent;
            return ret;
        }
    }

    public class KnobJSConstructor : ClrFunction
    {
        public KnobJSConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "Knobs", new KnobJSInstance(engine.Object.InstancePrototype))
        {

        }

        [JSConstructorFunction]
        public KnobJSInstance Construct()
        {
            return new KnobJSInstance(this.InstancePrototype);
        }

        public KnobJSInstance Construct(JavascriptImplant parent)
        {
            var ret = Construct();
            ret.Parent = parent;
            return ret;
        }
    }

    public class MidiKeysJSConstructor : ClrFunction
    {
        public MidiKeysJSConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "Keys", new MidiKeysJSInstance(engine.Object.InstancePrototype))
        {

        }

        [JSConstructorFunction]
        public MidiKeysJSInstance Construct()
        {
            return new MidiKeysJSInstance(this.InstancePrototype);
        }

        public MidiKeysJSInstance Construct(JavascriptImplant parent)
        {
            var ret = Construct();
            ret.Parent = parent;
            return ret;
        }
    }

    public class GuiJSConstructor : ClrFunction
    {
        public GuiJSConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "Gui", new GuiJSInstance(engine.Object.InstancePrototype))
        {

        }

        [JSConstructorFunction]
        public GuiJSInstance Construct()
        {
            return new GuiJSInstance(this.InstancePrototype);
        }

        public GuiJSInstance Construct(JavascriptImplant parent)
        {
            var ret = Construct();
            ret.Parent = parent;
            return ret;
        }
    }

    public class TimeJSConstructor : ClrFunction
    {
        public TimeJSConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "Time", new TimeJSInstance(engine.Object.InstancePrototype))
        {

        }

        [JSConstructorFunction]
        public TimeJSInstance Construct()
        {
            return new TimeJSInstance(this.InstancePrototype);
        }


        public TimeJSInstance Construct(JavascriptImplant parent)
        {
            var ret = Construct();
            ret.Parent = parent;
            return ret;
        }
    }

    public class SettingsJSConstructor : ClrFunction
    {
        public SettingsJSConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "Settings", new SettingsJsInstance(engine.Object.InstancePrototype))
        {

        }

        [JSConstructorFunction]
        public SettingsJsInstance Construct()
        {
            return new SettingsJsInstance(this.InstancePrototype);
        }

        public SettingsJsInstance Construct(JavascriptImplant parent)
        {
            var ret = Construct();
            ret.Parent = parent;
            return ret;
        }
    }

    public class StaticJSConstructor : ClrFunction
    {
        public StaticJSConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "Static", new StaticJSInstance(engine.Object.InstancePrototype))
        {

        }

        [JSConstructorFunction]
        public StaticJSInstance Construct()
        {
            return new StaticJSInstance(this.InstancePrototype);
        }

        public StaticJSInstance Construct(JavascriptImplant parent)
        {
            var ret = Construct();
            ret.Parent = parent;
            return ret;
        }
    }

    public class SessionJSConstructor : ClrFunction
    {
        public SessionJSConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "Session", new SessionJSInstance(engine.Object.InstancePrototype))
        {

        }

        [JSConstructorFunction]
        public SessionJSInstance Construct()
        {
            return new SessionJSInstance(this.InstancePrototype);
        }

        public SessionJSInstance Construct(JavascriptImplant parent)
        {
            var ret = Construct();
            ret.Parent = parent;
            return ret;
        }
    }

    public class ModesJSConstructor : ClrFunction
    {
        public ModesJSConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "Modes", new ModesJSInstance(engine.Object.InstancePrototype))
        {

        }

        [JSConstructorFunction]
        public ModesJSInstance Construct()
        {
            return new ModesJSInstance(this.InstancePrototype);
        }

        public ModesJSInstance Construct(JavascriptImplant parent)
        {
            var ret = Construct();
            ret.Parent = parent;
            return ret;
        }
    }
}
