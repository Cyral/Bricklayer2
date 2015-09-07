using System;
using Bricklayer.Core.Common.World;
using Bricklayer.Core.Server;
using Bricklayer.Plugins.DefaultBlocks.Common;
using Pyratron.Frameworks.Commands.Parser;

namespace Bricklayer.Plugins.DefaultBlocks.Server
{
    /// <summary>
    /// Server part of the default blocks plugin.
    /// </summary>
    public class Plugin : ServerPlugin
    {
        public Plugin(Core.Server.Server host) : base(host)
        {
        }

        public override void Load()
        {
            // Add blocks, block packs, and pack categories.
            PackCategories.AddCategories();
            BlockPacks.AddPacks();
            Blocks.AddBlocks();

            // Add command to list all blocks.
            Server.Commands.AddCommand(
                Command.Create("Blocks")
                    .AddAlias("blocks")
                    .SetDescription("List all loaded blocks")
                    .SetAction(
                        (arguments, o) =>
                        {
                            var cats = PackCategory.Categories.Count == 1 ? "category" : "categories";
                            var blocks = BlockType.Blocks.Count == 1 ? string.Empty : "s";
                            var packs = BlockPack.Packs.Count == 1 ? string.Empty : "s";
                            Console.WriteLine(
                                $"{BlockType.Blocks.Count} block{blocks} loaded from {BlockPack.Packs.Count} pack{packs} in {PackCategory.Categories.Count} {cats}.");
                            foreach (var block in BlockType.Blocks)
                                Console.WriteLine($"{block.ID} {block.Name} ({block.Pack})");
                        }));
        }

        public override void Unload()
        {
            PackCategories.RemoveCategories();
            BlockPacks.RemovePacks();
            Blocks.RemoveBlocks();
        }
    }
}