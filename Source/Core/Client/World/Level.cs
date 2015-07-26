using System;
using Bricklayer.Core.Common.Data;
using Bricklayer.Core.Common.World;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bricklayer.Core.Client.World
{
    public class Level : Common.World.Level
    {
        /// <summary>
        /// The main camera to follow the player.
        /// </summary>
        public Camera Camera { get; set; }

        /// <summary>
        /// The game client.
        /// </summary>
        public Client Client { get; set; }

        private static readonly float cameraSpeed = .18f;
        private static readonly float cameraParallax = .9f;

        public Level(PlayerData creator, string name, Guid uuid, string description, int plays, double rating) : base(creator, name, uuid, description, plays, rating)
        {

        }

        public Level(LevelData level, Client client) : base(level)
        {
            Client = client;
        }

        /// <summary>
        /// Converts an instance of a common level to a client level.
        /// </summary>
        public Level(Common.World.Level level, Client client) : base(level)
        {
            Tiles = level.Tiles;
            Width = level.Width;
            Height = level.Width;
            Players = level.Players;
            Spawn = level.Spawn;
            Client = client;

            Camera = new Camera(new Vector2(Client.GraphicsDevice.Viewport.Width, Client.GraphicsDevice.Viewport.Height - 24))
            {
                MinBounds = new Vector2(-Client.GraphicsDevice.Viewport.Width, -Client.GraphicsDevice.Viewport.Height),
                MaxBounds = new Vector2((Width * Tile.Width) + Client.GraphicsDevice.Viewport.Width, (Height * Tile.Height) + Client.GraphicsDevice.Viewport.Height)
            };
        }

        public void Update(GameTime delta)
        {
            
        }

        public void Draw(SpriteBatch batch, GameTime delta)
        {
            DrawTiles(batch);
        }

        /// <summary>
        /// Draws the foreground and background blocks of a map
        /// </summary>
        private void DrawTiles(SpriteBatch batch)
        {
            //Draw Foreground Blocks
            for (var x = (int)Camera.Left / Tile.Width; x <= (int)Camera.Right / Tile.Width; x++)
            {
                for (var y = ((int)Camera.Bottom / Tile.Height); y >= (int)Camera.Top / Tile.Height; y--)
                {
                    if (!InDrawBounds(x, y)) continue;
                    var tile = Tiles[x, y, 1];
                    if (tile.Type.IsRenderable)
                        tile.Type.Draw(batch, tile, x, y);
                }
            }
        }
    }
}
