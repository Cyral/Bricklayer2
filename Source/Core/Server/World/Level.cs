using System;
using Bricklayer.Core.Common.Data;
using Bricklayer.Core.Common.World;

namespace Bricklayer.Core.Server.World
{
    public class Level : Common.World.Level
    {
        public Level(PlayerData creator, string name, Guid uuid, string description, int plays, double rating) :
            base(creator, name, uuid, description, plays, rating)
        {
            Generate();
        }

        public Level(LevelData level) : base(level)
        {
            
        }

        /// <summary>
        /// Populates the tile array with empty tiles and a border around the level.
        /// </summary>
        private void Generate()
        {
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                        Tiles[x, y, 1] = new Tile(BlockType.Blocks[0]);
                    else
                        Tiles[x, y, 1] = new Tile(BlockType.Blocks[1]);
                    Tiles[x, y, 0] = new Tile(BlockType.Blocks[0]);
                }
            }
        }
    }
}
