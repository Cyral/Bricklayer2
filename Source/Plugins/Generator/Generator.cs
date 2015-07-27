using Bricklayer.Core.Common;
using Bricklayer.Core.Common.World;
using Bricklayer.Core.Server;
using Bricklayer.Plugins.DefaultBlocks.Common;

namespace Bricklayer.Plugins.Generator
{
    /// <summary>
    /// Generates an empty world.
    /// </summary>
    public class Plugin : ServerPlugin
    {
        public Plugin(Server host) : base(host) {}

        public override void Load()
        {
            // When a level is created, add a border around the world and set all others to empty.
            Server.Events.Game.Levels.LevelCreated.AddHandler(args =>
            {
                for (var x = 0; x < args.Level.Width; x++)
                {
                    for (var y = 0; y < args.Level.Height; y++)
                    {
                        if (x == 0 || y == 0 || x == args.Level.Width - 1 || y == args.Level.Height - 1)
                            args.Level.Tiles[x, y, 1] = new Tile(Blocks.Default);
                        else
                            args.Level.Tiles[x, y, 1] = new Tile(Blocks.Empty); 
                        args.Level.Tiles[x, y, 0] = new Tile(Blocks.Empty);
                    }
                }
            }, EventPriority.Initial);
        }

        protected override void Unload() {}
    }
}