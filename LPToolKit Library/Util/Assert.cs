using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.Util
{
    /// <summary>
    /// Used in debug mode to make code assertions.
    /// </summary>
    internal class Assert
    {
        public static void NotNull(string name, object value)
        {
            if (value == null)
            {
                throw new Exception("Assertion.NotNull failed on " + name);
            }
        }
    }
}
