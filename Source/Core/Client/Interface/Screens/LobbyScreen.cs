using System.Collections.Generic;
using Bricklayer.Core.Client.Interface.Windows;
using Bricklayer.Core.Common.Data;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Screens
{
    public class LobbyScreen : Screen
    {
        public string Description { get; }
        public string Intro { get; }
        public List<LevelData> Levels { get; private set; }
        public string Name { get; }
        public int Online { get; }

        protected internal override GameState State => GameState.Lobby;
        private ImageBox imgBackground;
        private LobbyWindow wndLobby;

        public LobbyScreen(string description, string name, string intro, int online, List<LevelData> levels)
        {
            Description = description;
            Name = name;
            Intro = intro;
            Online = online;
            Levels = levels;
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