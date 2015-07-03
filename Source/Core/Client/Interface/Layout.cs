using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bricklayer.Core.Client.Interface.Screens;
using Microsoft.Xna.Framework;
using MonoForce.Controls;
using Bricklayer.Core.Client;
using Microsoft.Xna.Framework.Graphics;

namespace Bricklayer.Client.Interface
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
        public static ScreenManager ScreenManager { get; set; }

        public static Color DefaultTextColor = new Color(32, 32, 32);

        public static SpriteFont DefaultSpriteFont;

        public MainWindow(Manager manager, Core.Client.Client client)
            : base(manager)
        {
            DefaultSpriteFont = Manager.Skin.Fonts["Default8"].Resource;
            Client = client;
            ElapsedTime = TimeSpan.Zero;

            //Make the window full size, without any border, disallow resize and move, etc., to use the entire screen size.
            ClearBackground = true;
            Resizable = false;
            Movable = false;
            CanFocus = false;
            StayOnBack = true;
            Left = Top = 0;
            AutoScroll = false;

            Width = manager.Graphics.PreferredBackBufferWidth;
            Height = manager.Graphics.PreferredBackBufferHeight;

            //Set up the ScreenManager which will handle all of the controls from here
            ScreenManager = new ScreenManager(this);
            ScreenManager.SwitchScreen(new LoginScreen());

            Client.Events.Network.Auth.Init.AddHandler(args =>
            {
                ScreenManager.SwitchScreen(new ServerScreen());
            });
        }
    }
}
