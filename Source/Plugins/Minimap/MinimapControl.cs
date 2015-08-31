using System;
using System.Collections.Generic;
using Bricklayer.Core.Client;
using Bricklayer.Core.Common;
using Bricklayer.Core.Common.World;
using Bricklayer.Plugins.DefaultBlocks.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoForce.Controls;
using MonoGame.Utilities;
using Level = Bricklayer.Core.Client.World.Level;

namespace Bricklayer.Plugins.Minimap
{
    /// <summary>
    /// A control that displays the minimap texture.
    /// </summary>
    public class MinimapControl : Control
    {
        private readonly Level level;
        private readonly Queue<Tuple<int, int, Color>> pixels = new Queue<Tuple<int, int, Color>>();
        private Texture2D texture;
        private readonly float regionAlpha;

        public MinimapControl(Manager manager, Level level, float regionAlpha) : base(manager)
        {
            this.level = level;
            this.regionAlpha = regionAlpha;
            Resizable = true;
            Passive = false;
            Movable = true;
            MaximumWidth = level.Width;
            MaximumHeight = level.Height;
        }

        public override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            base.DrawControl(renderer, rect, gameTime);
            if (texture != null)
            {
                // Set all the pixels in the queue.
                while (pixels.Count > 0)
                {
                    var pixel = pixels.Dequeue();
                    SetPixel(pixel.Item1, pixel.Item2, pixel.Item3);
                }
                // Draw minimap texture.
                renderer.Draw(texture, 0, 0, Color.White * (Alpha / 255f));

                // Draw camera region.
                // TODO: Take into account rotation, zoom, and origin of camera if it is ever used.
                var bounds = level.Camera.TileBounds;
                bounds = new Rectangle(rect.X + bounds.X, rect.Y + bounds.Y, bounds.Width, bounds.Height);
                renderer.SpriteBatch.DrawRectangle(bounds, Color.White * regionAlpha);
            }
        }

        /// <summary>
        /// Set an individual pixel.
        /// </summary>
        public void ChangePixel(int x, int y, Color color)
        {
            if (!level.InDrawBounds(x, y)) return;
            if (texture == null)
                texture = new Texture2D(Manager.GraphicsDevice, level.Width, level.Height);

            // All pixels will be processed when drawn.
            pixels.Enqueue(new Tuple<int, int, Color>(x, y, color));
            Invalidate();
        }

        /// <summary>
        /// Set the entire texture.
        /// </summary>
        public void SetData(Color[] colors)
        {
            if (texture == null)
                texture = new Texture2D(Manager.GraphicsDevice, level.Width, level.Height);
            texture.SetData(colors);
            Invalidate();
        }

        /// <summary>
        /// Returns the color of a specified tile.
        /// </summary>
        /// <remarks>
        /// Foreground blocks always appear over backgrounds, and foreground blocks drawn on the background will be darkened.
        /// </remarks>
        public Color GetColor(Tile foreground, Tile background)
        {
            if (foreground != Blocks.Empty)
            {
                return foreground.Type.Color;
            }
            if (background != Blocks.Empty)
            {
                return background.Type.Layer.HasFlag(Layer.Foreground)
                    ? new Color((int)(background.Type.Color.R * (BlockType.BackgroundTint.R / 255f)),
                        (int)(background.Type.Color.G * (BlockType.BackgroundTint.G / 255f)),
                        (int)(background.Type.Color.B * (BlockType.BackgroundTint.B / 255f)),
                        (int)(background.Type.Color.A * (BlockType.BackgroundTint.A / 255f)))
                    : background.Type.Color;
            }
            return Color.Black;
        }

        private void SetPixel(int x, int y, Color c)
        {
            var r = new Rectangle(x, y, 1, 1);
            var color = new Color[1];
            color[0] = c;

            texture.SetData(0, r, color, 0, 1);
        }
    }
}