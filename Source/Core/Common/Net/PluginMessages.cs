using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Core.Common.Net
{
    /// <summary>
    /// Class contains a list of plugin messages. Server and Client plugins will use this
    /// to recieve an id that both the server and the client knows for the plugin message.
    /// </summary>
    public class PluginMessages
    {
        // List of plugin messages
        public List<string> MessageTypes;

        public PluginMessages(List<string> messageTypes)
        {
            this.MessageTypes = messageTypes;
        }

        public PluginMessages()
        {
            MessageTypes = new List<string>();
        }

        /// <summary>
        /// Get Id by Message Type name
        /// </summary>
        public int GetId(string type)
        {
            return MessageTypes.IndexOf(type);
        }

        /// <summary>
        /// Get type by Message id
        /// </summary>
        public string GetType(int id)
        {
            return MessageTypes[id];
        }

        /// <summary>
        /// Add plugin message
        /// </summary>
        public void AddMessage(string type)
        {
            MessageTypes.Add(type);
        }
    }
}
