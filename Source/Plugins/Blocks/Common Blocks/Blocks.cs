﻿using Bricklayer.Core.Common.World;
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
            MetalPurple,

            MaterialStone,
            MaterialDirt,
            MaterialGrass,
            MaterialBrick,
            MaterialWood,
            MaterialSlab,
            MaterialGlass;

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

            MaterialStone = new BlockType("Stone", Layer.All, BlockCollision.Impassable, BlockPacks.Materials);
            MaterialDirt = new BlockType("Dirt", Layer.All, BlockCollision.Impassable, BlockPacks.Materials);
            MaterialGrass = new BlockType("Grass", Layer.All, BlockCollision.Impassable, BlockPacks.Materials);
            MaterialBrick = new BlockType("Brick", Layer.All, BlockCollision.Impassable, BlockPacks.Materials);
            MaterialWood = new BlockType("Wood", Layer.All, BlockCollision.Impassable, BlockPacks.Materials);
            MaterialSlab = new BlockType("Slab", Layer.All, BlockCollision.Impassable, BlockPacks.Materials);
            MaterialGlass = new BlockType("Glass", Layer.All, BlockCollision.Impassable, BlockPacks.Materials);
        }

        public static void RemoveBlocks()
        {
            // Remove blocks when plugin is unloaded.
            BlockType.Blocks.Remove(ClassicGray);
            BlockType.Blocks.Remove(ClassicWhite);
            BlockType.Blocks.Remove(ClassicBlack);
            BlockType.Blocks.Remove(ClassicRed);
            BlockType.Blocks.Remove(ClassicOrange);
            BlockType.Blocks.Remove(ClassicYellow);
            BlockType.Blocks.Remove(ClassicGreen);
            BlockType.Blocks.Remove(ClassicCyan);
            BlockType.Blocks.Remove(ClassicBlue);
            BlockType.Blocks.Remove(ClassicPurple);

            BlockType.Blocks.Remove(MetalGray);
            BlockType.Blocks.Remove(MetalWhite);
            BlockType.Blocks.Remove(MetalBlack);
            BlockType.Blocks.Remove(MetalRed);
            BlockType.Blocks.Remove(MetalOrange);
            BlockType.Blocks.Remove(MetalYellow);
            BlockType.Blocks.Remove(MetalGreen);
            BlockType.Blocks.Remove(MetalCyan);
            BlockType.Blocks.Remove(MetalBlue);
            BlockType.Blocks.Remove(MetalPurple);

            BlockType.Blocks.Remove(MaterialStone);
            BlockType.Blocks.Remove(MaterialDirt);
            BlockType.Blocks.Remove(MaterialGrass);
            BlockType.Blocks.Remove(MaterialBrick);
            BlockType.Blocks.Remove(MaterialWood);
            BlockType.Blocks.Remove(MaterialSlab);
            BlockType.Blocks.Remove(MaterialGlass);
        }
    }
}