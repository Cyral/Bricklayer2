using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Core.Client.Components;
using Microsoft.Xna.Framework.Graphics;

namespace Bricklayer.Core.Client
{
    /// <summary>
    /// Handles loading and organizing of texture files.
    /// A dot (.) may be used instead of a slash for the file path.
    /// </summary>
    public class ContentManager
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
        private Dictionary<string, Texture2D> Textures { get; } = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

        public int Count => Textures.Count;

        /// <summary>
        /// Load textures for a given path
        /// </summary>
        internal void LoadTextures(string path, Client client)
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
                        var name = Path.Combine(directoryName.Remove(0, path.Length + 1),
                            Path.GetFileNameWithoutExtension(file));

                        var texture = client.TextureLoader.FromFile(file);

                        // Add it to the dictionary
                        Textures[name] = texture;
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
    }
}
