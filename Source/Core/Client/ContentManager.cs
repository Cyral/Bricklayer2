using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Bricklayer.Core.Client
{
    /// <summary>
    /// Handles loading and organizing of the game's texture files.
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
        private Dictionary<string, Texture2D> Textures { get; } = new Dictionary<string, Texture2D>();

        /// <summary>
        /// Load textures for a given pack
        /// </summary>
        internal void LoadTextures(Client client)
        {
            var directory = Path.Combine(IO.Directories["Content"], "Textures");
            var files = DirSearch(directory, ".png");

            //For each file name, load it from disk.
            foreach (var file in files)
            {
                //Remove the full path to return the name of the file
                var directoryName = Path.GetDirectoryName(file);
                if (directoryName != null)
                {
                    var name = Path.Combine(directoryName.Remove(0, directory.Length + 1), Path.GetFileNameWithoutExtension(file));

                    var texture = client.TextureLoader.FromFile(file);

                    //Add it to the dictionary
                    if (Textures.ContainsKey(name))
                        Textures[name] = texture;
                    else
                        Textures.Add(name, texture);
                }
            }
        }

        /// <summary>
        /// Recursively search a directory for sub folders and files.
        /// </summary>
        /// <returns>A list of files within the folder or subfolders.</returns>
        private static IEnumerable<string> DirSearch(string directory, params string[] extensions)
        {
            var dir = new DirectoryInfo(directory);

            var files = (from f in dir.GetFiles("*.*") where f.Extension.EqualsAny(extensions) select f.FullName).ToList();
            foreach (var d in dir.GetDirectories("*.*"))
                {
                    files.AddRange(DirSearch(d.FullName, extensions));
                }

            return files;
        }
    }
}
