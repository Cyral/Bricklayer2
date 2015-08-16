using Bricklayer.Core.Common.World;

namespace Bricklayer.Plugins.DefaultBlocks.Common
{
    /// <summary>
    /// Provides access to the default/official block packs that come with Bricklayer.
    /// </summary>
    public class BlockPacks
    {
        public static BlockPack Classic, Metal, Materials;

        public static void AddPacks()
        {
            // Set default packs.
            Classic = new BlockPack("Classic", PackCategories.Blocks);
            Metal = new BlockPack("Metal", PackCategories.Blocks);
            Materials = new BlockPack("Materials", PackCategories.Blocks);
        }
    }
}