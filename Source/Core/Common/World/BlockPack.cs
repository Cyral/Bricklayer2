using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Core.Common.World
{
    /// <summary>
    /// A category/pack for blocks.
    /// </summary>
    public class BlockPack
    {
        /// <summary>
        /// List of all block packs.
        /// </summary>
        public static List<BlockPack> Packs { get; }

        /// <summary>
        /// Name of the category
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of blocks in this pack. (Automatically added)
        /// </summary>
        public IEnumerable<BlockType> Blocks => BlockType.Blocks.Where(b => b.Pack == this);

        /// <summary>
        /// ID of this category.
        /// </summary>
        public byte ID { get; private set; }

        public PackCategory Category { get; set; }

        static BlockPack()
        {
            Packs = new List<BlockPack>();
        }

        public BlockPack(string name)
        {
            Name = name;
            ID = (byte)Packs.Count;
            if (PackCategory.Categories.Count > 0) // Choose default category.
                Category = PackCategory.Categories[0];
        }

        public BlockPack(string name, PackCategory category)
        {
            Name = name;
            ID = (byte)Packs.Count;
            Category = category;
            Packs.Add(this);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
