using System;

namespace Bricklayer.Core.Common.World
{
    /// <summary>
    /// Represents an array of tiles.
    /// </summary>
    /// <remarks>
    /// An indexer is used to provide custom logic to the array.
    /// Messages and events will be sent/raised automatically on any tiles modified after world generation.
    /// </remarks>
    public class TileMap
    {
        /// <summary>
        /// Gets or sets the tile at the specified coordinates.
        /// </summary>
        public Tile this[int x, int y, int z]
        {
            get { return Tiles[x, y, z]; }
            set
            {
                var oldVal = Tiles[x, y, z];
                Tiles[x, y, z] = value;
                if (Generated)
                    BlockPlaced?.Invoke(x, y, z, value, oldVal);
            }
        }

        /// <summary>
        /// Gets or sets the tile at the specified coordinates and layer.
        /// </summary>
        public Tile this[int x, int y, Layer layer = Layer.Foreground]
        {
            get { return this[x, y, (int) layer]; }
            set { this[x, y, (int) layer] = value; }
        }

        /// <summary>
        /// The width, in blocks, of the map
        /// </summary>
        public int Width { get; protected set; }

        /// <summary>
        /// The height, in blocks, of the map
        /// </summary>
        public int Height { get; protected set; }

        /// <summary>
        /// Indicates if the world has been generated/filled. Events and network messages will not be sent/fired while the map is
        /// being generated.
        /// Tile changes by the player or plugin after the world has been generated will be sent to all users in the room and will
        /// cause the block change events to be fired.
        /// </summary>
        public bool Generated { get; set; }

        /// <summary>
        /// The tile array for the map, containing all tiles and tile data [X, Y, Layer/Z]
        /// Layer 0 = Background
        /// Layer 1 = Foreground
        /// </summary>
        /// <remarks>
        /// Modifying this will not send a message or raise an event. It should be used for internal purposes only.
        /// </remarks>
        internal Tile[,,] Tiles { get; set; }

        /// <summary>
        /// Action to be called upon block placement. (Only after generation)
        /// To be used for the client and server to call send network messages and fire event handlers.
        /// </summary>
        internal Action<int, int, int, Tile, Tile> BlockPlaced { get; set; }

        public TileMap(int width, int height)
        {
            Tiles = new Tile[width, height, 2];
            Width = width;
            Height = height;
        }
    }
}