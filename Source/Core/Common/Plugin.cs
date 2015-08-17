namespace Bricklayer.Core.Common
{
    /// <summary>
    /// The basic structure of a plugin.
    /// </summary>
    public abstract class Plugin : PluginData
    {
        /// <summary>
        /// Performed when the plugin is loaded.
        /// </summary>
        /// <remarks>
        /// Initialization logic should be put here and NOT in the constructor, as this method will be called if the plugin is
        /// reloaded.
        /// </remarks>
        public abstract void Load();

        /// <summary>
        /// Performed when the plugin is unloaded.
        /// </summary>
        /// <remarks>
        /// It is up to the plugin author to properly unload/reset everything. If the plugin is not reset completely, undesired
        /// side effects may occur.
        /// For this reason, plugin reloading is not completely supported. The client or server must be completely restarted.
        /// </remarks>
        public abstract void Unload();
    }
}