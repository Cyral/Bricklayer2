using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Core.Common.World
{
    /// <summary>
    /// An overall category for block packs.
    /// </summary>
    public class PackCategory
    {
        /// <summary>
        /// List of all pack categories.
        /// </summary>
        public static List<PackCategory> Categories{ get; }

        /// <summary>
        /// Name of the category.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of pakcs in this category. (Automatically added)
        /// </summary>
        public IEnumerable<BlockPack> Packs => BlockPack.Packs.Where(b => b.Category == this);

        /// <summary>
        /// ID of this category.
        /// </summary>
        public byte ID { get; private set; }

        static PackCategory()
        {
            Categories = new List<PackCategory>();
        }

        public PackCategory(string name)
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
