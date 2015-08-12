using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Bricklayer.Core.Common.Data;
using Bricklayer.Core.Common.Entity;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace Bricklayer.Core.Common.World
{
    /// <summary>
    /// Represents a level/map, with an array of tiles and a list of players.
    /// </summary>
    public class Level : LevelData
    {
        /// <summary>
        /// The number of players currently online this level.
        /// </summary>
        public override int Online => Players.Count;

        /// <summary>
        /// The list of players currently in the map, synced with the server
        /// </summary>
        public List<Player> Players { get; internal set; }

        /// <summary>
        /// The spawn point new players will originate from
        /// </summary>
        public Vector2 Spawn { get; internal set; }

        /// <summary>
        /// The tile array for the map, containing all tiles and tile data [X, Y, Layer/Z]
        /// Layer 0 = Background
        /// Layer 1 = Foreground
        /// </summary>
        /// <remarks>
        /// An indexer is used to provide custom logic to the array.
        /// Messages and events will be sent/raised automatically on any tiles modified after world generation.
        /// </remarks>
        public TileMap Tiles { get; internal set; }

        /// <summary>
        /// The width, in blocks, of the map
        /// </summary>
        public int Width => Tiles.Width;

        /// <summary>
        /// The height, in blocks, of the map
        /// </summary>
        public int Height => Tiles.Height;

        protected Random random;

        public Level(PlayerData creator, string name, Guid uuid, string description, int plays, double rating)
            : this(new LevelData(creator, name, uuid, description, 0, plays, rating))
        {
        }

        public Level(LevelData level)
            : base(level.Creator, level.Name, level.UUID, level.Description, 0, level.Plays, level.Rating)
        {
            Players = new List<Player>();
            Tiles = new TileMap(1000, 500);
            Spawn = new Vector2(Tile.Width, Tile.Height);
            random = new Random();
        }

        internal Level(NetIncomingMessage im) : base(im)
        {
            Players = new List<Player>();
            Spawn = new Vector2(Tile.Width, Tile.Height);
            random = new Random();

            // Read player data
            var playersLength = im.ReadInt32();

            for (var i = 0; i < playersLength; i++)
                Players.Add(new Player(im));

            // Read the tile data
            var memLength = im.ReadInt32();
            using (var memory = new MemoryStream(im.ReadBytes(memLength)))
            using (var gzip = new GZipStream(memory, CompressionMode.Decompress, true))
            using (var reader = new BinaryReader(gzip))
                DecodeTiles(reader);
        }

        /// <summary>
        /// Reads tile data from a binary stream.
        /// </summary>
        internal virtual void DecodeTiles(BinaryReader reader)
        {
            Tiles = new TileMap(reader.ReadInt32(), reader.ReadInt32()) {Generated = true};

            // Read the background layer, then foreground layer.
            for (var layer = 0; layer < 2; layer++)
            {
                for (var y = 0; y < Height; y++)
                {
                    for (var x = 0; x < Width; x++)
                    {
                        Tiles[x, y, layer] = new Tile(BlockType.FromID(reader.ReadUInt16()));
                    }
                }
            }
        }

        internal override void Encode(NetOutgoingMessage om)
        {
            // Write the data such as name, description, etc.
            base.Encode(om);


            // Read player data
            om.Write(Players.Count);
            foreach (var player in Players)
                player.Encode(om);

            // Write the tile data
            using (var memory = new MemoryStream())
            {
                // Use Gzip to compress the data
                // Note: In Bricklayer v1, RLE was used, Gzip is simpler to use and results in better compression hower.
                using (var gzip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    using (var writer = new BinaryWriter(gzip))
                        EncodeTiles(writer);
                    var memArr = memory.ToArray();
                    om.Write(memArr.Length);
                    om.Write(memArr);
                }
            }
        }

        internal void EncodeTiles(BinaryWriter writer)
        {
            writer.Write(Width);
            writer.Write(Height);
            // Write the background layer, then foreground layer, so values that are the same are written next to each other
            for (var layer = 0; layer < 2; layer++)
            {
                for (var y = 0; y < Height; y++)
                {
                    for (var x = 0; x < Width; x++)
                    {
                        Tiles[x, y, layer].Write(writer);
                    }
                }
            }
        }

        /// <summary>
        /// Determines if a grid position is in the bounds of the map array.
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
    }
}