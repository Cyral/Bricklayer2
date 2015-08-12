using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Bricklayer.Core.Common;
using Bricklayer.Core.Common.World;
using Bricklayer.Core.Server;
using Bricklayer.Plugins.DefaultBlocks.Common;
using Microsoft.Xna.Framework;

namespace Bricklayer.Plugins.Snake
{
    /// <summary>
    /// Snake minigame.
    /// </summary>
    public class SnakePlugin : ServerPlugin
    {
        private const int speed = 20; // Timer update speed (ms).
        private const int TTL = 70; // Time (ticks) to live.
        private static readonly BlockType startType = Blocks.ClassicGreen;
        private readonly Dictionary<LevelPoint, SnakeTile> points = new Dictionary<LevelPoint, SnakeTile>();
        private readonly Dictionary<int, BlockType> types = new Dictionary<int, BlockType>();
        private Timer timer;

        public SnakePlugin(Server host) : base(host)
        {
            types.Add(35, Blocks.ClassicRed);
        }

        public override void Load()
        {
            timer = new Timer(speed);
            timer.Elapsed += (sender, args) =>
            {
                lock (points)
                {
                    for (var i = points.Count - 1; i >= 0; i--)
                    {
                        var point = points.ElementAt(i);
                        point.Value.TTL--;

                        if (point.Value.TTL == 0)
                        {
                            point.Key.SetTile(point.Value.OriginalType);
                            points.Remove(point.Key);
                        }
                        else
                        {
                            foreach (var type in types)
                            {
                                if (point.Value.TTL == type.Key)
                                    point.Key.SetTile(type.Value);
                            }
                        }
                    }
                }
            };
            timer.Start();
            Server.Events.Game.Level.BlockPlaced.AddHandler(args =>
            {
                if (args.Layer != Layer.Foreground) return;
                if (args.Type == startType)
                {
                    lock (points)
                    {
                        if (!points.Any(x => x.Key.Point.X == args.X && x.Key.Point.Y == args.Y))
                            points.Add(new LevelPoint(args.Level, args.X, args.Y),
                                new SnakeTile(args.OldType));
                    }
                }
            }, EventPriority.Initial);
        }

        protected override void Unload()
        {
        }

        private class LevelPoint
        {
            public Point Point { get; }
            public Level Level { get; }

            public LevelPoint(Level level, int x, int y)
            {
                Level = level;
                Point = new Point(x, y);
            }

            public void SetTile(BlockType type)
            {
                Level.Tiles[Point.X, Point.Y] = type;
            }
        }

        private class SnakeTile
        {
            public int TTL { get; set; }
            public BlockType OriginalType { get; }

            public SnakeTile(BlockType originalType)
            {
                TTL = SnakePlugin.TTL;
                OriginalType = originalType;
            }
        }
    }
}