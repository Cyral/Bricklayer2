using System;

namespace Bricklayer.Core.Common
{
    /// <summary>
    /// The base of a plugin.
    /// </summary>
    public abstract class Plugin
    {
        /// <summary>
        /// The author of the plugin.
        /// </summary>
        public abstract string Author { get; }

        /// <summary>
        /// The description of the plugin.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// The name of the plugin.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The current version of the plugin.
        /// </summary>
        public abstract Version Version { get; }

        public abstract void Load();
    }
}