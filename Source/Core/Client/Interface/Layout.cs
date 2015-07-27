using Bricklayer.Core.Client.Interface.Screens;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface
{
    /// <summary>
    /// Handles the layout aspects of the window
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        /// <summary>
        /// The game instance this window belongs to.
        /// </summary>
        public Core.Client.Client Client { get; }

        /// <summary>
        /// The main <c>ScreenManager</c> that handles control adding/removing.
        /// </summary>
        public static ScreenManager ScreenManager { get; private set; }


        public MainWindow(Manager manager, Core.Client.Client client)
            : base(manager)
        {
            Client = client;

            // Make the window full size, without any border, disallow resize and move, etc., to use the entire screen size.
            ClearBackground = true;
            Resizable = false;
            Movable = false;
            CanFocus = false;
            StayOnBack = true;
            Left = Top = 0;
            AutoScroll = false;

            Width = manager.Graphics.PreferredBackBufferWidth;
            Height = manager.Graphics.PreferredBackBufferHeight;

            // Set up the ScreenManager which will handle all of the controls from here
            ScreenManager = new ScreenManager(this);
            ScreenManager.SwitchScreen(new LoginScreen());

            Client.Events.Network.Auth.InitReceived.AddHandler(args =>
            {
                ScreenManager.SwitchScreen(new ServerScreen());
            });
        }
    }
}
