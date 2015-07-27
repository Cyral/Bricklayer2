using Bricklayer.Core.Common;
using Bricklayer.Core.Common.World;
using Bricklayer.Core.Server;

namespace Bricklayer.Plugins.Generator
{
    /// <summary>
    /// S part of the default blocks plugin.
    /// </summary>
    public class Plugin : ServerPlugin
    {
        public Plugin(Server host) : base(host) {}

        public override void Load()
        {
            Server.Events.Game.Levels.LevelCreated.AddHandler(args =>
            {
                for (var x = 0; x < args.Level.Width; x++)
                {
                    for (var y = 0; y < args.Level.Height; y++)
                    {
                        if (x == 0 || y == 0 || x == args.Level.Width - 1 || y == args.Level.Height - 1)
                            args.Level.Tiles[x, y, 1] = new Tile(Blocks.Common.Blocks.Default);
                        else
                            args.Level.Tiles[x, y, 1] = new Tile(Blocks.Common.Blocks.Empty);
                        args.Level.Tiles[x, y, 0] = new Tile(Blocks.Common.Blocks.Empty);
                    }
                }
            }, EventPriority.Initial);
        }

        protected override void Unload() {}
    }
}