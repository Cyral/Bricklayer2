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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;
using Bricklayer.Core.Common.Data;
using Bricklayer.Core.Common.Net;
using Bricklayer.Core.Common.Net.Messages;

namespace Bricklayer.Core.Client.Interface.Screens
{
    public class LobbyScreen : Screen
    {
        //Controls
        private LobbyWindow wndLobby;
        private ImageBox  imgBackground;

        public string Description { get; }
        public string Name { get; }
        public string Intro { get; }
        public int Online { get; }

        public List<LevelData> Levels { get; private set; }

        public LobbyScreen(string description, string name, string intro, int online, List<LevelData> levels)
        {
            Description = description;
            Name = name;
            Intro = intro;
            Online = online;
            Levels = levels;


        }

        protected internal override GameState State => GameState.Lobby;

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

            //Add the login window
            wndLobby = new LobbyWindow(Manager, this);
            wndLobby.Init();
            Window.Add(wndLobby);
            wndLobby.Show();
        }

        public override void Remove()
        {
            Window.Remove(wndLobby);
            Window.Remove(imgBackground);
        }
    }
}
