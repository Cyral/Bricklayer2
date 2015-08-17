using System.Reflection;
using Bricklayer.Core.Common;

namespace Bricklayer.Core.Server
{
    /// <summary>
    /// The base of all server plugins.
    /// </summary>
    public abstract class ServerPlugin : Plugin
    {
        /// <summary>
        /// The server host.
        /// </summary>
        public Server Server { get; set; }

        /// <summary>
        /// Creates an instance of the plugin with the specified server host.
        /// </summary>
        public ServerPlugin(Server host)
        {
            Server = host;
        }
    }
}
