using Bricklayer.Core.Common;
using Microsoft.Xna.Framework.Graphics;

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
        /// Optional icon to display in the plugin manager.
        /// </summary>
        public Texture2D Icon { get; set; }

        /// <summary>
        /// Gets or sets if the plugin is enabled and should be ran.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Creates an instance of the plugin with the specified client host.
        /// </summary>
        public ClientPlugin(Client host)
        {
            Client = host;
            IsEnabled = true;
        }
    }
}
