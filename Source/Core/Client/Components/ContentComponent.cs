using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

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
        public Texture2D this[string name]
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
        public Dictionary<string, Texture2D> Textures { get; } =
            new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

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
}