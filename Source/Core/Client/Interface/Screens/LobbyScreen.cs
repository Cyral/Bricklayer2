using System.Collections.Generic;
using Bricklayer.Core.Client.Interface.Windows;
using Bricklayer.Core.Common.Data;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Screens
{
    public class LobbyScreen : Screen
    {
        /// <summary>
        /// The server description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The server intro/MOTD text.
        /// </summary>
        public string Intro { get; }

        internal List<LevelData> Levels { get; }

        /// <summary>
        /// The server name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The number of players online.
        /// </summary>
        public int Online { get; }

        public override GameState State => GameState.Lobby;

        /// <summary>
        /// Background image.
        /// </summary>
        public ImageBox ImgBackground { get; set; }

        /// <summary>
        /// Lobby window with list of levels.
        /// </summary>
        public LobbyWindow WndLobby { get; set; }

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

            ImgBackground = new ImageBox(Manager)
            {
                Image = Client.Content["gui.background"],
                SizeMode = SizeMode.Stretched
            };
            ImgBackground.SetSize(Window.Width, Window.Height);
            ImgBackground.SetPosition(0, 0);
            ImgBackground.Init();
            Window.Add(ImgBackground);

            // Add the login window
            WndLobby = new LobbyWindow(Manager, this);
            WndLobby.Init();
            Window.Add(WndLobby);
            WndLobby.Show();
        }

        public override void Remove()
        {
            Window.Remove(WndLobby);
            Window.Remove(ImgBackground);
        }
    }
}