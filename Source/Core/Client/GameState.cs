namespace Bricklayer.Core.Client
{
    /// <summary>
    /// Represents a state of the game. (Usually matches a screen.)
    /// </summary>
    public enum GameState
    {
        Login,
        PluginManager,
        Server,
        Lobby,
        Game,

        /// <summary>
        /// Used for custom screens created by plugins.
        /// </summary>
        Custom
    }
}
