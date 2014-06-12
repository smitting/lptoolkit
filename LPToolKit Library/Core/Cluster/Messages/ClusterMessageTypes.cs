using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPToolKit.Core.Cluster.Messages
{

    /// <summary>
    /// Manages the available types of messages by their ids.
    /// </summary>
    internal class ClusterMessageTypes
    {
        /// <summary>
        /// Finds all subclasses of ClusterMessage and registers them.
        /// </summary>
        static ClusterMessageTypes()
        {
            var types = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                         from type in assembly.GetTypes()
                         where type.IsSubclassOf(typeof(ClusterMessage))
                         select type
                                ).ToArray();
            foreach (var type in types)
            {
                Register(type);
            }
        }

        /// <summary>
        /// Registers a new type by creating an instance to ask it
        /// for the message id it processes.
        /// </summary>
        public static void Register(Type t)
        {
            Register(Activator.CreateInstance(t) as ClusterMessage);
        }

        /// <summary>
        /// Registers a new type from an instance.
        /// </summary>
        public static void Register(ClusterMessage instance)
        {
            if (instance == null) return;
            var messageId = instance.MessageID;
            if (messageId == null) return;
            messageId = messageId.ToLower().Trim();
            lock (_types)
            {
                if (_types.ContainsKey(messageId) == false)
                {
                    _types.Add(messageId, instance.GetType());
                }
            }
        }

        /// <summary>
        /// Returns the type of ClusterMessage that can parse a given
        /// message if, or null if not found.
        /// </summary>
        public static Type GetTypeByMessageId(string messageId)
        {
            if (messageId == null) return null;
            messageId = messageId.ToLower().Trim();
            lock (_types)
            {
                Type t = null;
                _types.TryGetValue(messageId, out t);
                return t;
            }
        }

        /// <summary>
        /// Creates an instance appropriate for the message id, or null
        /// if no type is registered for that id.
        /// </summary>
        public static ClusterMessage CreateInstanceForMessageId(string messageId)
        {
            var type = GetTypeByMessageId(messageId);
            if (type == null) return null;
            return Activator.CreateInstance(type) as ClusterMessage;
        }

        private static Dictionary<string, Type> _types = new Dictionary<string, Type>();
    }

}
