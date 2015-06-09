using System;
using Bricklayer.Client.Interface;
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
        protected ScreenManager ScreenManager { get; private set; }

        /// <summary>
        /// The window this screen is attached to.
        /// </summary>
        protected MainWindow Window => ScreenManager.Window;

        /// <summary>
        /// The MonoForce UI manage used for creating controls.
        /// </summary>
        protected Manager Manager => ScreenManager.Manager;

        /// <summary>
        /// Action to be performed when the screen is initialized. (When the controls are added)
        /// </summary>
        public Action Initialized { get; set; }

        /// <summary>
        /// The game/client instance this screen belongs to.
        /// </summary>
        protected internal Client Client => ScreenManager.Window.Client;

        /// <summary>
        /// Adds the controls for this screen to the window.
        /// </summary>
        public virtual void Add(ScreenManager screenManager)
        {
            Initialized?.Invoke();
            ScreenManager = screenManager;
        }

        /// <summary>
        /// Removes the controls for this screen from the window.
        /// </summary>
        public virtual void Remove()
        {

        }
    }
}
