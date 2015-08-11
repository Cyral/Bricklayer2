using Bricklayer.Core.Common.World;

namespace Bricklayer.Plugins.DefaultBlocks.Common
{
    /// <summary>
    /// Provides access to the official/default blocks that come with Bricklayer.
    /// </summary>
    public class Blocks
    {
        public static BlockType Empty,
            DefaultBlack,
            DefaultWhite,
            DefaultGray,
            DefaultRed,
            DefaultOrange,
            DefaultYellow,
            DefaultGreen,
            DefaultCyan,
            DefaultBlue,
            DefaultPurple;

        public static void AddBlocks()
        {
            // Set default blocks.
            Empty = new BlockType("Empty", Layer.All) {IsRenderable = false};
            DefaultGray = new BlockType("Gray", Layer.All, BlockCollision.Impassable, Categories.Default);
            DefaultWhite = new BlockType("White", Layer.All, BlockCollision.Impassable, Categories.Default);
            DefaultBlack = new BlockType("Black", Layer.All, BlockCollision.Impassable, Categories.Default);
            DefaultRed = new BlockType("Red", Layer.All, BlockCollision.Impassable, Categories.Default);
            DefaultOrange = new BlockType("Orange", Layer.All, BlockCollision.Impassable, Categories.Default);
            DefaultYellow = new BlockType("Yellow", Layer.All, BlockCollision.Impassable, Categories.Default);
            DefaultGreen = new BlockType("Green", Layer.All, BlockCollision.Impassable, Categories.Default);
            DefaultCyan = new BlockType("Cyan", Layer.All, BlockCollision.Impassable, Categories.Default);
            DefaultBlue = new BlockType("Blue", Layer.All, BlockCollision.Impassable, Categories.Default);
            DefaultPurple = new BlockType("Purple", Layer.All, BlockCollision.Impassable, Categories.Default);
        }
    }
}