using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Core.Common.Entity;
using Microsoft.Xna.Framework;

namespace Bricklayer.Core.Common.World
{
    /// <summary>
    /// Represents a level/map, with an array of tiles and a list of players.
    /// </summary>
    public abstract class Map
    {
        /// <summary>
        /// The tile array for the map, containing all tiles and tile data [X, Y, Layer/Z]
        /// Layer 0 = Background
        /// Layer 1 = Foreground
        /// </summary>
        public virtual Tile[,,] Tiles { get; set; }

        /// <summary>
        /// The width, in blocks, of the map
        /// </summary>
        public virtual int Width { get { return width; } set { width = value; } }

        /// <summary>
        /// The height, in blocks, of the map
        /// </summary>
        public virtual int Height { get { return height; } set { height = value; } }

        /// <summary>
        /// The name of the map, defined from the server
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// The ID of this map, for server use
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// The description of the map, defined from the server
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// The number of players online
        /// </summary>
        public virtual int Online => Players.Count;

        /// <summary>
        /// The map's play count
        /// </summary>
        public virtual int Plays { get; set; }

        /// <summary>
        /// The map's rating, defined from the server
        /// </summary>
        public virtual double Rating { get; set; }

        /// <summary>
        /// The list of players currently in the map, synced with the server
        /// </summary>
        public List<Player> Players { get; set; }

        /// <summary>
        /// Defines if this map instance is part of a server
        /// </summary>
        public virtual bool IsServer { get; set; }

        /// <summary>
        /// The spawn point new players will originate from
        /// </summary>
        public virtual Vector2 Spawn { get; set; }

        protected Random random = new Random();
        protected int width, height;

        /// <summary>
        /// Determines if a grid position is in the bounds of the map
        /// </summary>
        public bool InBounds(int x, int y, int z = 1)
        {
            return !(y < 1 || y >= Height - 1 || x < 1 || x >= Width - 1 || z > 1 || z < 0);
        }

        /// <summary>
        /// Determines if a grid position is in the bounds of the drawing area (The map, but not the border)
        /// </summary>
        public bool InDrawBounds(int x, int y)
        {
            return !(y < 0 || y >= Height || x < 0 || x >= Width);
        }

        /// <summary>
        /// Returns a player from an ID
        /// </summary>
        public Player PlayerFromID(int id)
        {
            foreach (Player player in Players)
                if (player.ID == id)
                    return player;
            throw new KeyNotFoundException("Could not find player from ID: " + id);
        }
    }
}
