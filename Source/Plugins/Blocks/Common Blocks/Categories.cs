using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Core.Common.World;

namespace Bricklayer.Plugins.DefaultBlocks.Common
{
    /// <summary>
    /// Provides access to the default/official block categories that come with Bricklayer.
    /// </summary>
    public class Categories
    {
        public static BlockCategory Default;

        public static void AddCategories()
        {
            // Set default categories.
            Default = new BlockCategory("Default");
        }
    }
}
