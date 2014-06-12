using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPToolKit.GUI;
using LPToolKit.GUI.Controls;
using LPToolKit.GUI.Layouts;

namespace LPToolKit.GUI.Util
{
    /// <summary>
    /// Object that maintains all current GuiControl object 
    /// references to prevent object churning.
    /// </summary>
    public class ControlRegistration
    {
        #region Constructor

        /// <summary>
        /// Constructor accepts the context this is for.
        /// </summary>
        public ControlRegistration(GuiContext parent)
        {
            Parent = parent;
        }

        #endregion

        #region Types

        /// <summary>
        /// Method called by Get() to create a new instance when
        /// no existing instance was found.
        /// </summary>
        public delegate GuiControl ConstructorMethod();

        #endregion

        #region Properties

        /// <summary>
        /// The owner of these controls.
        /// </summary>
        public readonly GuiContext Parent;

        /// <summary>
        /// Returns a list of all gui controls registered.
        /// </summary>
        public List<GuiControl> All
        {
            get
            {
                var ret = new List<GuiControl>();
                foreach (var key in _allControls.Keys)
                {
                    ret.Add(_allControls[key]);
                }
                return ret;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns all controls that are located at the x/y coordinate.
        /// </summary>
        public List<GuiControl> GetAtXY(int x, int y)
        {
            return All.Where(c => c.LastRenderPosition.Contains(x, y)).ToList();
        }

        /// <summary>
        /// Stores a control by ID, and if it's of special types it
        /// is also stored in type specific dictionaries, such as
        /// the value controls.
        /// </summary>
        public void Register(string id, GuiControl g)
        {
            g.Parent = Parent;
            if (g is ValueControl)
            {
                _valueControls[id] = g as ValueControl;
            }
            else if (g is RendererWrapper)
            {
                _wrapperControls[id] = g as RendererWrapper;
            }
            else if (g is GuiLayout)
            {
                _layouts[id] = g as GuiLayout;
            }
            _allControls[id] = g;
        }

        /// <summary>
        /// Returns a value from the internal dictionary, calling the 
        /// function to create a new instance and registering it if
        /// none already exists.
        /// </summary>
        public T Get<T>(string id, ConstructorMethod createInstance) where T : GuiControl
        {
            T ret;
            if (TryGetValue<T>(id, out ret) == false)
            {
                ret = createInstance() as T;
                if (ret != default(T))
                {
                    ret.ID = id;

                    if (ret is IGuiInputHandler)
                    {
                        ret.InputHandler = ret as IGuiInputHandler;
                    }

                    Register(id, ret);
                }
            }
            return ret;
        }

        /// <summary>
        /// Calls TryGetValue on the appropriate dictionary, which
        /// depends on if the type dictatates using a special list
        /// instead of the general list.
        /// </summary>
        public bool TryGetValue<T>(string id, out T control) where T : GuiControl
        {
            if (typeof(T) == typeof(RendererWrapper))
            {
                RendererWrapper rw;
                if (_wrapperControls.TryGetValue(id, out rw))
                {
                    control = rw as T;
                    return true;
                }
                else
                {
                    control = null;
                    return false;
                }
            }
            else if (typeof(T) == typeof(ValueControl))
            {
                ValueControl vc;
                if (_valueControls.TryGetValue(id, out vc))
                {
                    control = vc as T;
                    return true;
                }
                else
                {
                    control = null;
                    return false;
                }
            }
            else if (typeof(T) == typeof(GuiLayout))
            {
                GuiLayout gl;
                if (_layouts.TryGetValue(id, out gl))
                {
                    control = gl as T;
                    return true;
                }
                else
                {
                    control = null;
                    return false;
                }
            }
            else
            {
                GuiControl gc;
                if (_allControls.TryGetValue(id, out gc))
                {
                    control = gc as T;
                    return true;
                }
                else
                {
                    control = null;
                    return false;
                }
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// All instances of GuiLayout.
        /// </summary>
        private readonly Dictionary<string, GuiLayout> _layouts = new Dictionary<string, GuiLayout>();

        /// <summary>
        /// All instances of RendererWrapper.
        /// </summary>
        private readonly Dictionary<string, RendererWrapper> _wrapperControls = new Dictionary<string, RendererWrapper>();

        /// <summary>
        /// All instances of ValueControl.
        /// </summary>
        private readonly Dictionary<string, ValueControl> _valueControls = new Dictionary<string, ValueControl>();

        /// <summary>
        /// Maintains a list of all known control objects by ID.
        /// </summary>
        private readonly Dictionary<string, GuiControl> _allControls = new Dictionary<string, GuiControl>();

        #endregion
    }

}
