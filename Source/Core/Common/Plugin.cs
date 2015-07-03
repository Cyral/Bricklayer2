using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Core.Common
{
    /// <summary>
    /// The basic definition of a plugin.
    /// </summary>
    public abstract class Plugin
    {
        /// <summary>
        /// The (preferably short) name of the plugin.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The description of the plugin.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// The name of the author of this plugin.
        /// </summary>
        public abstract string Author { get; }

        /// <summary>
        /// The version of this plugin.
        /// </summary>
        /// <remarks>
        /// This value is used for mod updating and must be increased with each release.
        /// </remarks>
        public abstract Version Version { get; }

        /// <summary>
        /// Performed when the plugin is loaded.
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// Performed when the plugin is unloaded.
        /// </summary>
        protected abstract void Unload();
    }
}
