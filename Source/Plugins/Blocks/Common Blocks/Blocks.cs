using Bricklayer.Core.Common.World;

namespace Bricklayer.Plugins.Blocks.Common
{
    public class Blocks
    {
        public static void AddBlocks()
        {
             BlockType.Blocks.Add(new BlockType("Air", Layer.All));
             BlockType.Blocks.Add(new BlockType("Default", Layer.All, BlockCollision.Impassable));
        }
    }
}
