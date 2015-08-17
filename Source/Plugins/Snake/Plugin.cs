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
        private Dictionary<LevelPoint, SnakeTile> points;
        private Dictionary<int, BlockType> types;
        private Timer timer;

        public SnakePlugin(Server host) : base(host)
        {

        }

        public override void Load()
        {
            points = new Dictionary<LevelPoint, SnakeTile>();
            // Add types to appear once X ms. are remaining.
            types = new Dictionary<int, BlockType> {{35, Blocks.ClassicRed}};

            timer = new Timer(speed);
            timer.Elapsed += (sender, args) =>
            {
                lock (points)
                {
                    // Each tick, decrease the point's value.
                    for (var i = points.Count - 1; i >= 0; i--)
                    {
                        var point = points.ElementAt(i);
                        point.Value.Time--;

                        // At zero, restore the original block.
                        if (point.Value.Time == 0)
                        {
                            point.Key.SetTile(point.Value.OriginalType);
                            points.Remove(point.Key);
                        }
                        else
                        {
                            foreach (var type in types)
                            {
                                // If it matches a type, replace the block.
                                if (point.Value.Time == type.Key)
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
                        // Record each block place of the configured block.
                        if (!points.Any(x => x.Key.Point.X == args.X && x.Key.Point.Y == args.Y))
                            points.Add(new LevelPoint(args.Level, args.X, args.Y),
                                new SnakeTile(args.OldType));
                    }
                }
            }, EventPriority.Initial);
        }

        public override void Unload()
        {
            timer.Stop();
            timer.Dispose();
            points.Clear();
            types.Clear();
        }

        /// <summary>
        /// A point in a level the block was placed on.
        /// </summary>
        private class LevelPoint
        {
            public Point Point { get; }
            private readonly Level level;

            public LevelPoint(Level level, int x, int y)
            {
                this.level = level;
                Point = new Point(x, y);
            }

            public void SetTile(BlockType type)
            {
               level.Tiles[Point.X, Point.Y] = type;
            }
        }

        /// <summary>
        /// The time remaining for a tile, and its original type.
        /// </summary>
        private class SnakeTile
        {
            public int Time { get; set; }
            public BlockType OriginalType { get; }

            public SnakeTile(BlockType originalType)
            {
                Time = TTL;
                OriginalType = originalType;
            }
        }
    }
}