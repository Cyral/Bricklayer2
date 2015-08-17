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
            PackCategories.AddCategories();
            BlockPacks.AddPacks();
            Blocks.AddBlocks();
        }

        public override void Unload()
        {
            PackCategories.RemoveCategories();
            BlockPacks.RemovePacks();
            Blocks.RemoveBlocks();
        }
    }
}