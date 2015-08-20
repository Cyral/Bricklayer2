using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bricklayer.Core.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Bricklayer.Core.Client.Components
{
    /// <summary>
    /// Handles loading and organizing of texture files.
    /// A dot (.) may be used instead of a slash for the file path.
    /// </summary>
    public class ContentComponent : ClientComponent
    {
        /// <summary>
        /// All textures loaded into the game.
        /// </summary>
        public Image this[string name]
        {
            get
            {
                name = name.Replace('.', '\\');
                if (Textures.ContainsKey(name))
                    return Textures[name];
                throw new KeyNotFoundException($"The texture {name} was not found.");
            }
            private set { Textures[name] = value; }
        }

        /// <summary>
        /// List of all textures loaded into the game.
        /// </summary>
        public Dictionary<string, Image> Textures { get; } =
            new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// List of all effects loaded into the game.
        /// </summary>
        public Dictionary<string, Effect> Effects { get; } =
            new Dictionary<string, Effect>(StringComparer.OrdinalIgnoreCase);

        public ContentComponent(Client client) : base(client)
        {
        }

        public override async Task Init()
        {
            await LoadTextures(Path.Combine(Client.IO.Directories["Content"], "Textures"));
            await LoadEffects(Path.Combine(Client.IO.Directories["Content"], "Effects"));
        }

        /// <summary>
        /// Load textures for a given path.
        /// </summary>
        private async Task LoadTextures(string path)
        {
            if (Directory.Exists(path))
            {
                var files = DirSearch(path, ".png", ".jpeg", ".jpg");

                // For each file name, load it from disk.
                foreach (var file in files)
                {
                    // Remove the full path to return the name of the file
                    var directoryName = Path.GetDirectoryName(file);
                    if (directoryName != null)
                    {
                        var name = directoryName.Length == path.Length
                            ? Path.GetFileNameWithoutExtension(file)
                            : Path.Combine(directoryName.Remove(0, path.Length + 1),
                                Path.GetFileNameWithoutExtension(file));

                        var texture = Client.IO.LoadTexture(file);

                        // Add it to the dictionary
                        Textures[name] = texture;
                    }
                }
            }
            else
                Console.WriteLine($"Directory {path} does not exist.");
        }

        /// <summary>
        /// Load MonoGame effects for a given path.
        /// </summary>
        /// <remarks>
        /// Compile effects with 2MGFX.
        /// Example:
        /// "C:\Program Files (x86)\MSBuild\MonoGame\v3.0\Tools\2MGFX.exe" effet.fx effect.mgfxo /Profile:DirectX_11
        /// </remarks>
        private async Task LoadEffects(string path)
        {
            if (Directory.Exists(path))
            {
                var files = DirSearch(path, ".mgfxo");

                // For each file name, load it from disk.
                foreach (var file in files)
                {
                    // Remove the full path to return the name of the file
                    var directoryName = Path.GetDirectoryName(file);
                    if (directoryName != null)
                    {
                        var name = directoryName.Length == path.Length
                            ? Path.GetFileNameWithoutExtension(file)
                            : Path.Combine(directoryName.Remove(0, path.Length + 1),
                                Path.GetFileNameWithoutExtension(file));

                        var effect = await Client.IO.LoadEffect(file);

                        // Add it to the dictionary.
                        Effects[name] = effect;
                    }
                }
            }
            else
                Console.WriteLine($"Directory {path} does not exist.");
        }

        /// <summary>
        /// Recursively search a directory for sub folders and files.
        /// </summary>
        /// <returns>A list of files within the folder or subfolders.</returns>
        private static IEnumerable<string> DirSearch(string directory, params string[] extensions)
        {
            var dir = new DirectoryInfo(directory);

            var files =
                dir.GetFiles().Where(f => f.Extension.EqualsAny(extensions)).Select(f => f.FullName).ToList();
            foreach (var d in dir.GetDirectories())
                files.AddRange(DirSearch(d.FullName, extensions));

            return files;
        }

        /// <summary>
        /// Load effects and textures from a plugin.
        /// </summary>
        internal async void LoadPluginContent(ClientPlugin plugin)
        {
            await LoadTextures(Path.Combine(plugin.Path, "Content", "Textures"));
            await LoadEffects(Path.Combine(plugin.Path, "Content", "Effects"));
        }
    }

    /// <summary>
    /// A collection of images within another texture. Texture atlases speed up rendering by not needing to switch textures.
    /// </summary>
    public class TextureAtlas : Texture2D
    {
        public List<Image> Images { get; internal set; }

        public static TextureAtlas CreateAtlas(GraphicsDevice graphics, List<Image> images)
        {
            if (images.Count > 1)
            {
                // Pack the textures next to each other.
                int h = 0, finalWidth = 0, finalHeight = 0, finalRowHeight = 0, rowWidth = 0;
                var items = images.OrderByDescending(x => x.Texture.Width).ToArray();
                for (int i = 0, row = 0; i < items.Length; i++, row++)
                {
                    var image = items[i];
                    // TODO: Better algorithm.
                    if (row > Math.Sqrt(items.Length))
                    {
                        h += finalRowHeight;
                        finalWidth = Math.Max(finalWidth, rowWidth);
                        row = rowWidth = finalRowHeight = 0;
                    }
                    image.SourceRect = new Rectangle(rowWidth, h, image.Texture.Width, image.Texture.Height);
                    rowWidth += image.Texture.Width;
                    if (image.Texture.Height > finalRowHeight)
                        finalRowHeight += image.Texture.Height;
                    finalHeight = h;
                }
                finalHeight += finalRowHeight;
                return new TextureAtlas(graphics, images, finalWidth, finalHeight);
            }
            throw new InvalidOperationException("More than 1 image must be supplied.");
        }

        private TextureAtlas(GraphicsDevice graphicsDevice, List<Image> images, int width, int height) : base(graphicsDevice, width, height)
        {
            Images = images;
            var spriteBatch = new SpriteBatch(graphicsDevice);

            // Render all the textures to RenderTarget.
            var renderTarget = new RenderTarget2D(graphicsDevice, width, height);
     
                var viewportBackup = graphicsDevice.Viewport;
                graphicsDevice.SetRenderTarget(renderTarget);
                graphicsDevice.Clear(Color.Transparent);

                foreach (var image in Images)
                {
                    spriteBatch.Begin();
                    spriteBatch.Draw(image, image.SourceRect, Color.White);
                    spriteBatch.End();
                }

                // Release the GPU back to drawing to the screen
                graphicsDevice.SetRenderTarget(null);
                graphicsDevice.Viewport = viewportBackup;

                // Store data from render target because the RenderTarget2D is volatile
                var data = new Color[renderTarget.Width * renderTarget.Height];
                renderTarget.GetData(data);
                var texture = (Texture2D)renderTarget;
                foreach (var image in Images)
                    image.Texture = texture;

                // Unset texture from graphic device and set modified data back to it
                //graphicsDevice.Textures[0] = null;
        
        }
    }
}