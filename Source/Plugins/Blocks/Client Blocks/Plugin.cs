using Bricklayer.Core.Client;
using Bricklayer.Plugins.DefaultBlocks.Common;

namespace Bricklayer.Plugins.DefaultBlocks.Client
{
    /// <summary>
    /// Client part of the default blocks plugin.
    /// </summary>
    public class Plugin : ClientPlugin
    {
        public Plugin(Core.Client.Client host) : base(host) {}

        public override void Load()
        {
            Categories.AddCategories();
            Blocks.AddBlocks();
        }

        protected override void Unload() {}
    }
}