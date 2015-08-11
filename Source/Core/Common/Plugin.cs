namespace Bricklayer.Core.Common
{
    /// <summary>
    /// The basic definition of a plugin.
    /// </summary>
    public abstract class Plugin : PluginData
    {
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
