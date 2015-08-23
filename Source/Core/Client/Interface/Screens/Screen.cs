using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Screens
{
    /// <summary>
    /// A basic implentation of a UI screen.
    /// </summary>
    public abstract class Screen
    {
        /// <summary>
        /// The controlling <c>ScreenManager</c>.
        /// </summary>
        internal ScreenManager ScreenManager { get; private set; }

        /// <summary>
        /// The window this screen is attached to.
        /// </summary>
        protected MainWindow Window => ScreenManager.Window;

        /// <summary>
        /// The MonoForce UI manage used for creating controls.
        /// </summary>
        protected Manager Manager => ScreenManager.Manager;

        /// <summary>
        /// The game/client instance this screen belongs to.
        /// </summary>
        protected internal Client Client => ScreenManager.Window.Client;

        /// <summary>
        /// The game state associated with this screen.
        /// </summary>
        public abstract GameState State { get; }

        /// <summary>
        /// Controls added via the AddControl method, which will be automatically removed when the screen is cleared.
        /// </summary>
        protected internal List<Control> AddedControls { get; } = new List<Control>();

        /// <summary>
        /// Creates and adds the controls for this screen to the window.
        /// </summary>
        public virtual void Setup(ScreenManager screenManager)
        {
            ScreenManager = screenManager;
        }

        /// <summary>
        /// Removes the controls for this screen from the window and disposes of objects. If they were added through the
        /// AddControls method or inherit from Dialog, they will be removed automatically.
        /// </summary>
        public virtual void Clear()
        {
            foreach (var control in AddedControls)
            {
                Manager.Remove(control);
                control.Dispose();
            }
            AddedControls.Clear();
        }

        public virtual void Update(GameTime gameTime)
        {

        }

        public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {

        }

        /// <summary>
        /// Adds a control to the screen. When the screen is cleared, the control will be removed. It is recommended to use this
        /// method instead of directly adding controls to the Manager.
        /// </summary>
        public virtual void AddControl(Control control)
        {
            Manager.Add(control);
            AddedControls.Add(control);
        }

        /// <summary>
        /// Removes a control from the screen.
        /// </summary>
        public virtual void RemoveControl(Control control)
        {
            Manager.Remove(control);
            AddedControls.Remove(control);
        }
    }
}