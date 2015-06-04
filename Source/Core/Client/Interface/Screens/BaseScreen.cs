using System;
using Bricklayer.Client.Interface;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Screens
{
    /// <summary>
    /// A basic implentation of a UI screen.
    /// </summary>
    public abstract class BaseScreen : IScreen
    {
        /// <summary>
        /// The controlling <c>ScreenManager</c>.
        /// </summary>
        protected ScreenManager ScreenManager { get; private set; }

        /// <summary>
        /// The window this screen is attached to.
        /// </summary>
        protected MainWindow Window { get { return ScreenManager.Window; } }

        /// <summary>
        /// The MonoForce UI manage used for creating controls.
        /// </summary>
        protected Manager Manager { get { return ScreenManager.Manager; } }

        /// <summary>
        /// Action to be performed when the screen is initialized. (When the controls are added)
        /// </summary>
        public Action Initialized { get; set; }

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
