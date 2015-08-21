using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bricklayer.Core.Client
{
    /// <summary>
    /// Loads textures without use of the content pipeline.
    /// Needed to load textures with alpha/transparency properly.
    /// Based on: http://jakepoz.com/jake_poznanski__background_load_xna.html and
    /// http://gamedev.stackexchange.com/questions/35460/load-texture-from-image-content-in-runtime
    /// </summary>
    internal class TextureLoader : IDisposable
    {
        private static readonly BlendState blendColorBlendState;
        private static readonly BlendState blendAlphaBlendState;
        private readonly GraphicsDevice graphicsDevice;
        private readonly SpriteBatch spriteBatch;

        static TextureLoader()
        {
            blendColorBlendState = new BlendState
            {
                ColorDestinationBlend = Blend.Zero,
                ColorWriteChannels = ColorWriteChannels.Red | ColorWriteChannels.Green | ColorWriteChannels.Blue,
                AlphaDestinationBlend = Blend.Zero,
                AlphaSourceBlend = Blend.SourceAlpha,
                ColorSourceBlend = Blend.SourceAlpha
            };

            blendAlphaBlendState = new BlendState
            {
                ColorWriteChannels = ColorWriteChannels.Alpha,
                AlphaDestinationBlend = Blend.Zero,
                ColorDestinationBlend = Blend.Zero,
                AlphaSourceBlend = Blend.One,
                ColorSourceBlend = Blend.One
            };
        }

        public TextureLoader(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            spriteBatch = new SpriteBatch(graphicsDevice);
        }

        public void Dispose()
        {
            spriteBatch.Dispose();
        }

        public Texture2D FromFile(string path, bool preMultiplyAlpha = true)
        {
            using (Stream fileStream = File.OpenRead(path))
                return FromStream(fileStream, preMultiplyAlpha);
        }

        public Texture2D FromStream(Stream stream, bool preMultiplyAlpha = true)
        {
            var texture = Texture2D.FromStream(graphicsDevice, stream);

            if (preMultiplyAlpha)
            {
                // Setup a render target to hold our final texture which will have premulitplied alpha values
                using (var renderTarget = new RenderTarget2D(graphicsDevice, texture.Width, texture.Height))
                {
                    var viewportBackup = graphicsDevice.Viewport;
                    graphicsDevice.SetRenderTarget(renderTarget);
                    graphicsDevice.Clear(Color.Black);

                    // Multiply each color by the source alpha, and write in just the color values into the final texture
                    spriteBatch.Begin(SpriteSortMode.Immediate, blendColorBlendState);
                    spriteBatch.Draw(texture, texture.Bounds, Color.White);
                    spriteBatch.End();

                    // Now copy over the alpha values from the source texture to the final one, without multiplying them
                    spriteBatch.Begin(SpriteSortMode.Immediate, blendAlphaBlendState);
                    spriteBatch.Draw(texture, texture.Bounds, Color.White);
                    spriteBatch.End();

                    // Release the GPU back to drawing to the screen
                    graphicsDevice.SetRenderTarget(null);
                    graphicsDevice.Viewport = viewportBackup;

                    // Store data from render target because the RenderTarget2D is volatile
                    var data = new Color[texture.Width*texture.Height];
                    renderTarget.GetData(data);

                    // Unset texture from graphic device and set modified data back to it
                    graphicsDevice.Textures[0] = null;
                    texture.SetData(data);
                }
            }

            return texture;
        }
    }
}