using Bricklayer.Core.Common;
using MonoForce.Controls;

namespace Bricklayer.Core.Client
{
    /// <summary>
    /// The base of all client plugins.
    /// </summary>
    public abstract class ClientPlugin : Plugin
    {
        /// <summary>
        /// The client host.
        /// </summary>
        public Client Client { get; set; }

        /// <summary>
        /// Creates an instance of the plugin with the specified client host.
        /// </summary>
        public ClientPlugin(Client host)
        {
            Client = host;
        }
    }
}
