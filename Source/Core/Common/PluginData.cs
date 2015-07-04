using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Bricklayer.Core.Common
{
    /// <summary>
    /// Metadata information about a plugin.
    /// </summary>
    public class PluginData
    {
        /// <summary>
        /// The (preferably short) name of the plugin.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// The description of the plugin.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// The name of the author of this plugin.
        /// </summary>
        public string Author { get; internal set; }

        /// <summary>
        /// The version of this plugin.
        /// </summary>
        /// <remarks>
        /// This value is used for mod updating and must be increased with each release.
        /// </remarks>
        public Version Version { get; internal set; }

        /// <summary>
        /// Path to this plugin's root folder.
        /// </summary>
        [JsonIgnore]
        public string Path { get; internal set; }
    }
}
