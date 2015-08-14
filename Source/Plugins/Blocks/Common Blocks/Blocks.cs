using Bricklayer.Core.Common.World;
using Microsoft.Xna.Framework;

namespace Bricklayer.Plugins.DefaultBlocks.Common
{
    /// <summary>
    /// Provides access to the official/default blocks that come with Bricklayer.
    /// </summary>
    public class Blocks
    {
        public static BlockType
            Empty,

            ClassicBlack,
            ClassicWhite,
            ClassicGray,
            ClassicRed,
            ClassicOrange,
            ClassicYellow,
            ClassicGreen,
            ClassicCyan,
            ClassicBlue,
            ClassicPurple,

            MetalBlack,
            MetalWhite,
            MetalGray,
            MetalRed,
            MetalOrange,
            MetalYellow,
            MetalGreen,
            MetalCyan,
            MetalBlue,
            MetalPurple;

        public static void AddBlocks()
        {
            // Set default blocks.
            Empty = BlockType.FromID(0);
            Empty.Pack = BlockPacks.Classic;

            ClassicGray = new BlockType("Gray", Layer.All, BlockCollision.Impassable, BlockPacks.Classic);
            ClassicWhite = new BlockType("White", Layer.All, BlockCollision.Impassable, BlockPacks.Classic);
            ClassicBlack = new BlockType("Black", Layer.All, BlockCollision.Impassable, BlockPacks.Classic);
            ClassicRed = new BlockType("Red", Layer.All, BlockCollision.Impassable, BlockPacks.Classic);
            ClassicOrange = new BlockType("Orange", Layer.All, BlockCollision.Impassable, BlockPacks.Classic);
            ClassicYellow = new BlockType("Yellow", Layer.All, BlockCollision.Impassable, BlockPacks.Classic);
            ClassicGreen = new BlockType("Green", Layer.All, BlockCollision.Impassable, BlockPacks.Classic);
            ClassicCyan = new BlockType("Cyan", Layer.All, BlockCollision.Impassable, BlockPacks.Classic);
            ClassicBlue = new BlockType("Blue", Layer.All, BlockCollision.Impassable, BlockPacks.Classic);
            ClassicPurple = new BlockType("Purple", Layer.All, BlockCollision.Impassable, BlockPacks.Classic);

            MetalGray = new BlockType("Gray", Layer.All, BlockCollision.Impassable, BlockPacks.Metal);
            MetalWhite = new BlockType("White", Layer.All, BlockCollision.Impassable, BlockPacks.Metal);
            MetalBlack = new BlockType("Black", Layer.All, BlockCollision.Impassable, BlockPacks.Metal);
            MetalRed = new BlockType("Red", Layer.All, BlockCollision.Impassable, BlockPacks.Metal);
            MetalOrange = new BlockType("Orange", Layer.All, BlockCollision.Impassable, BlockPacks.Metal);
            MetalYellow = new BlockType("Yellow", Layer.All, BlockCollision.Impassable, BlockPacks.Metal);
            MetalGreen = new BlockType("Green", Layer.All, BlockCollision.Impassable, BlockPacks.Metal);
            MetalCyan = new BlockType("Cyan", Layer.All, BlockCollision.Impassable, BlockPacks.Metal);
            MetalBlue = new BlockType("Blue", Layer.All, BlockCollision.Impassable, BlockPacks.Metal);
            MetalPurple = new BlockType("Purple", Layer.All, BlockCollision.Impassable, BlockPacks.Metal);
        }
    }
}