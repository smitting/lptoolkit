using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace LPToolKit.GUI.Renderers
{
    /// <summary>
    /// Base class for GUI components that are drawn to a bitmap.
    /// </summary>
    public abstract class GuiRenderer
    {
        #region Constructors

        /// <summary>
        /// Constructor accepts bitmap to render to.
        /// </summary>
        public GuiRenderer(Bitmap bmp)
        {
            if (bmp == null) throw new ArgumentNullException("bmp");
            Image = bmp;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The object assigned to deal with input for this object.
        /// </summary>
        public IGuiInputHandler InputHandler;

        /// <summary>
        /// Called whenever the Render() method finishes.
        /// </summary>
        public event GuiRenderEventHandler Rendered;

        /// <summary>
        /// Where to render to
        /// </summary>
        public Bitmap Image { get; protected set; }

        #endregion

        #region Methods

        /// <summary>
        /// Called by other classes to render this object, wrapping the
        /// subclass's handling thread-safing and events for the subclass
        /// automatically.
        /// </summary>
        public void Render()
        {
            lock (_imageLock)
            {
                try
                {
                    // allow only one rendering at a time
                    if (rendering) return;

                    // fail if no bitmap is available to render to
                    if (Image == null) return;

                    // render
                    rendering = true;
                    OnRender();

                    // send notification that rendering is complete
                    if (Rendered != null)
                    {
                        Rendered(this, new GuiRenderEventArgs() { Renderer = this });
                    }
                }
                catch
                {
                    // just ignore rendering errors
                    return;
                }
                finally
                {
                    // mark as finished
                    rendering = false;
                }
            }

        }

        #endregion

        #region Protected Abstract Methods

        /// <summary>
        /// Actual rendering implementation in subclasses.
        /// </summary>
        protected abstract void OnRender();

        #endregion

        #region Protected

        /// <summary>
        /// Mutex object for changing the image reference
        /// </summary>
        protected readonly object _imageLock = new object();

        /// <summary>
        /// Set to true when rendering so it happens one at a time.
        /// </summary>
        protected bool rendering = false;

        #endregion
    }
}
