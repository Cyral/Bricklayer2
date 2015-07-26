using Bricklayer.Core.Common.World;

namespace Bricklayer.Plugins.Blocks.Common
{
    public class Blocks
    {
        public static BlockType Empty, Default;

        public static void AddBlocks()
        {
             Empty = new BlockType("Empty", Layer.All) { IsRenderable = false };
             Default = new BlockType("Default", Layer.All, BlockCollision.Impassable);
        }
    }
}
