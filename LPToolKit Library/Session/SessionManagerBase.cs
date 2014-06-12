using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.Session
{
    /// <summary>
    /// Base class for all main property objects in the UserSession.
    /// </summary>
    public abstract class SessionManagerBase
    {
        #region Constructor

        public SessionManagerBase(UserSession parent)
        {
            Parent = parent;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The session these settings are applied to.
        /// </summary>
        public readonly UserSession Parent;

        #endregion

        #region Methods

        /// <summary>
        /// Allows for a graceful shutdown.
        /// </summary>
        public virtual void Shutdown()
        {
        }

        #endregion
    }
}
