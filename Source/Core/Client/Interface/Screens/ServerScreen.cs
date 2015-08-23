﻿using Bricklayer.Core.Client.Interface.Windows;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Screens
{
    public class ServerScreen : Screen
    {
        public override GameState State => GameState.Server;
        public ImageBox ImgBackground { get; set; }

        /// <summary>
        /// Window with server list and buttons.
        /// </summary>
        public ServerWindow WndServer { get; set; }

        public override void Setup(ScreenManager screenManager)
        {
            base.Setup(screenManager);
            Client.State = GameState.Lobby;

            ImgBackground = new ImageBox(Manager)
            {
                Image = Client.Content["gui.background"],
                SizeMode = SizeMode.Stretched,
                CanFocus = false,
                StayOnBack = true
            };
            ImgBackground.SetSize(Window.Width, Window.Height);
            ImgBackground.SetPosition(0, 0);
            ImgBackground.Init();
            AddControl(ImgBackground);

            // Add the server window
            WndServer = new ServerWindow(Manager, this);
            WndServer.Init();
            AddControl(WndServer);
            WndServer.Show();
        }
    }
}