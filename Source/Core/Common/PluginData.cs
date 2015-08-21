using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bricklayer.Core.Common
{
    /// <summary>
    /// Metadata information about a plugin.
    /// </summary>
    public class PluginData
    {
        /// <summary>
        /// The (preferably short) display name of the plugin.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The unique identification name of this plugin.
        /// This name must not be used by any other plugin.
        /// </summary>
        /// <example>
        /// Examples:
        /// com.pyratron.plugins.testplugin
        /// myplugin-123
        /// MyName.MyPlugin
        /// The plugin identifier will be validated when uploading to the official database.
        /// </example>
        public string Identifier { get; internal set; }

        /// <summary>
        /// The description of the plugin.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// The name(s) of the author(s) of the plugin.
        /// </summary>
        public List<string> Authors { get; internal set; }

        /// <summary>
        /// The plugin identifiers that this plugin requires to be installed to run.
        /// </summary>
        public List<string> Dependencies { get; internal set; }

        /// <summary>
        /// The version of this plugin.
        /// </summary>
        /// <remarks>
        /// This value is used for mod updating and must be increased with each release.
        /// </remarks>
        public Version Version { get; internal set; }

        /// <summary>
        /// Full name of the main type loaded.
        /// </summary>
        [JsonIgnore]
        internal string MainTypeName { get; set; }

        /// <summary>
        /// Path to this plugin's root folder.
        /// </summary>
        [JsonIgnore]
        public string Path { get; internal set; }

        private bool Equals(PluginData other)
        {
            return Version == other.Version && string.Equals(Identifier, other.Identifier);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((PluginData) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Version.GetHashCode()*397) ^ Name.GetHashCode();
            }
        }

        /// <summary>
        /// Returns a readable string containing the plugin's name, unique identifier, and version.
        /// </summary>
        public string GetInfoString()
        {
            return $"{Name} ({Identifier}) v{Version}";
        }
    }
}