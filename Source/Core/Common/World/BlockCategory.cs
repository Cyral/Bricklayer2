using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Core.Common.World
{
    /// <summary>
    /// A category for blocks.
    /// </summary>
    public class BlockCategory
    {
        /// <summary>
        /// List of all block categories.
        /// </summary>
        public static List<BlockCategory> Categories { get; }

        /// <summary>
        /// Name of the category
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of blocks in this category. (Automatically added)
        /// </summary>
        public IEnumerable<BlockType> Blocks => Blocks.Where(b => b.Category == this);

        /// <summary>
        /// ID of this category.
        /// </summary>
        public byte ID { get; private set; }

        static BlockCategory()
        {
            Categories = new List<BlockCategory>();
        }

        public BlockCategory(string name)
        {
            Name = name;
            ID = (byte)Categories.Count;
            Categories.Add(this);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
