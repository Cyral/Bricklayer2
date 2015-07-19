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
        /// The height, in blocks, of the map
        /// </summary>
        public virtual int Height { get; protected set; }

        /// <summary>
        /// The number of players currently online this level.
        /// </summary>
        public override int Online => Players.Count;

        /// <summary>
        /// The list of players currently in the map, synced with the server
        /// </summary>
        public List<Player> Players { get; set; }

        /// <summary>
        /// The spawn point new players will originate from
        /// </summary>
        public virtual Vector2 Spawn { get; set; }

        /// <summary>
        /// The tile array for the map, containing all tiles and tile data [X, Y, Layer/Z]
        /// Layer 0 = Background
        /// Layer 1 = Foreground
        /// </summary>
        public virtual Tile[,,] Tiles { get; protected set; }

        /// <summary>
        /// The width, in blocks, of the map
        /// </summary>
        public virtual int Width { get; protected set; }

        protected Random random;

        public Level(PlayerData creator, string name, Guid uuid, string description, int plays, double rating)
            : this(new LevelData(creator, name, uuid, description, 0, plays, rating))
        {
        }

        public Level(LevelData level)
            : base(level.Creator, level.Name, level.UUID, level.Description, 0, level.Plays, level.Rating)
        {
            Players = new List<Player>();
            Width = 1000;
            Height = 500;
            Tiles = new Tile[Width, Height, 2];
            Spawn = new Vector2(1, 1);
            random = new Random();
        }

        internal Level(NetIncomingMessage im) : base(im)
        {
            Players = new List<Player>();
            Spawn = new Vector2(1, 1);
            random = new Random();

            //Read the tile data
            var memLength = im.ReadInt32();
            using (var memory = new MemoryStream(im.ReadBytes(memLength)))
            {
                using (var gzip = new GZipStream(memory, CompressionMode.Decompress, true))
                {
                    using (var reader = new BinaryReader(gzip))
                    { 
                        Width = reader.ReadInt32();
                        Height = reader.ReadInt32();
                        Tiles = new Tile[Width, Height, 2];
                        //Write the background layer, then foreground layer, so values that are the same are written next to each other
                        for (var layer = 0; layer < 2; layer++)
                        {
                            for (var y = 0; y < Height; y++)
                            {
                                for (var x = 0; x < Width; x++)
                                {
                                    Tiles[x, y, layer] = new Tile(BlockType.FromID(reader.ReadInt16()));
                                }
                            }
                        }
                    }
                }
            }
        }

        internal override void Encode(NetOutgoingMessage om)
        {
            //Write the data such as name, description, etc.
            base.Encode(om);
            //Write the tile data
            using (var memory = new MemoryStream())
            {
                //Use Gzip to compress the data
                //Note: In Bricklayer v1, RLE was used, Gzip is simpler to use and results in better compression hower.
                using (var gzip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    using (var writer = new BinaryWriter(gzip))
                    {
                        writer.Write(Width);
                        writer.Write(Height);
                        //Write the background layer, then foreground layer, so values that are the same are written next to each other
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
                    var memArr = memory.ToArray();
                    om.Write(memArr.Length);
                    om.Write(memArr);
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