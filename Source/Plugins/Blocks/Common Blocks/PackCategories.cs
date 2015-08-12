using Bricklayer.Core.Common.World;

namespace Bricklayer.Plugins.DefaultBlocks.Common
{
    /// <summary>
    /// Provides access to the default/official block pack categories that come with Bricklayer.
    /// </summary>
    public class PackCategories
    {
        public static PackCategory Blocks, Interactive, Miscellaneous;

        public static void AddCategories()
        {
            // Set default categories.
            Blocks = new PackCategory("Blocks");
            Interactive = new PackCategory("Interactive");
            Miscellaneous = new PackCategory("Miscellaneous");
        }
    }
}