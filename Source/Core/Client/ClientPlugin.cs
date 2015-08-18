using Bricklayer.Core.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bricklayer.Core.Client
{
    /// <summary>
    /// The base of all client plugins.
    /// </summary>
    public abstract class ClientPlugin : Plugin
    {
        /// <summary>
        /// The client host.
        /// </summary>
        public Client Client { get; protected set; }

        /// <summary>
        /// Optional icon to display in the plugin manager.
        /// </summary>
        public Texture2D Icon { get; set; }

        /// <summary>
        /// Creates an instance of the plugin with the specified client host.
        /// </summary>
        public ClientPlugin(Client host)
        {
            Client = host;
            IsEnabled = true;
        }

        /// <summary>
        /// Performed when the plugin should draw anything it needs to draw. This is called one to four times per frame, depending on the game state.
        /// Each call is called at a different time, so it is possible to draw behind or in front of game objects.
        /// </summary>
        public virtual void Draw(DrawPass pass, SpriteBatch spriteBatch, GameTime delta)
        {
            
        }

        /// <summary>
        /// Performed when the plugin should update its logic.
        /// This is called before the game logic is update.
        /// </summary>
        public virtual void Update(GameTime delta)
        {

        }
    }

    /// <summary>
    /// Indicates the state a draw call is from. (The Draw method will be called multiple times by the client, so that the
    /// plugin can draw behind or over certain elements)
    /// </summary>
    public enum DrawPass
    {
        /// <summary>
        /// Draw pass before any game objects are drawn. Only called when in game (GameState.Game).
        /// </summary>
        Before,
        /// <summary>
        /// Draw pass after the background tiles have been drawn. Only called when in game (GameState.Game).
        /// </summary>
        Background,
        /// <summary>
        /// Draw pass after the foreground tiles have been drawn. Only called when in game (GameState.Game).
        /// </summary>
        Foreground,
        /// <summary>
        /// Draw pass after all game objects have been drawn. This pass is always called.
        /// </summary>
        After
    }
}