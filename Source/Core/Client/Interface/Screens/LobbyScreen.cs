using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using Bricklayer.Core.Client.Interface.Windows;
using Bricklayer.Core.Server.Data;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Screens
{
    public class LobbyScreen : Screen
    {
        //Controls
        public LobbyWindow Lobby;

        private ImageBox  imgBackground;
        public string Description;
        public string Name;
        public string Intro;
        public int Online;
        public List<LobbySaveData> Rooms = new List<LobbySaveData>();

        public LobbyScreen(string description, string name, string intro, int online, List<LobbySaveData> rooms)
        {
            Description = description;
            Name = name;
            Intro = intro;
            Online = online;
            Rooms = rooms;
        }
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
            Lobby = new LobbyWindow(Manager, this);
            Lobby.Init();
            Window.Add(Lobby);
            Lobby.Show();

        }
        public override void Remove()
        {
            Window.Remove(Lobby);
            Window.Remove(imgBackground);
        }
    }
}
