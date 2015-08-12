using Bricklayer.Core.Server;
using Bricklayer.Plugins.DefaultBlocks.Common;

namespace Bricklayer.Plugins.DefaultBlocks.Server
{
    /// <summary>
    /// Server part of the default blocks plugin.
    /// </summary>
    public class Plugin : ServerPlugin
    {
        public Plugin(Core.Server.Server host) : base(host) {}

        public override void Load()
        {
            PackCategories.AddCategories();
            BlockPacks.AddPacks();
            Blocks.AddBlocks();
        }

        protected override void Unload() {}
    }
}