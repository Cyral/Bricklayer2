using Bricklayer.Core.Client.Interface.Windows;
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

            // Add the server window
            WndServer = new ServerWindow(Manager, this);
            WndServer.Init();
            Window.Add(WndServer);
            WndServer.Show();
        }

        public override void Remove()
        {
            Window.Remove(WndServer);
            WndServer.Dispose();
            Window.Remove(ImgBackground);
        }
    }
}