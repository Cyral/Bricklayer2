using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using Bricklayer.Core.Client.Interface.Windows;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Screens
{
    public class ServerScreen : Screen
    {
        //Controls
        private ServerWindow wndServer;
        private ImageBox imgBackground;

        protected internal override GameState State => GameState.Server;

        public override void Add(ScreenManager screenManager)
        {
            base.Add(screenManager);
            Client.State = GameState.Lobby;

            imgBackground = new ImageBox(Manager)
            {
                Image = Client.Content["gui.background"],
                SizeMode = SizeMode.Stretched
            };
            imgBackground.SetSize(Window.Width, Window.Height);
            imgBackground.SetPosition(0, 0);
            imgBackground.Init();
            Window.Add(imgBackground);

            //Add the server window
            wndServer = new ServerWindow(Manager, this);
            wndServer.Init();
            Window.Add(wndServer);
            wndServer.Show();
        }

        public override void Remove()
        {
            Window.Remove(wndServer);
            wndServer.Dispose();
            Window.Remove(imgBackground);
        }
    }
}
