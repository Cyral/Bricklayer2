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
        /// Content available to this plugin, loaded from the "Content/Textures" folder in the plugin directory.
        /// </summary>
        public ContentManager Content { get; set; }

        /// <summary>
        /// Creates an instance of the plugin with the specified client host.
        /// </summary>
        public ClientPlugin(Client host)
        {
            Client = host;
            Content = new ContentManager();
        }

        internal void LoadContent()
        {
            Content.LoadTextures(System.IO.Path.Combine(Path, System.IO.Path.Combine("Content", "Textures")), Client);
            System.Console.WriteLine(Content.Count + " textures loaded.");
        }
    }
}
