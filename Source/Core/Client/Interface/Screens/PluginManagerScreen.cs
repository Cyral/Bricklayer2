using Bricklayer.Core.Client.Interface.Windows;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Screens
{
    public class PluginManagerScreen : Screen
    {
        public override GameState State => GameState.PluginManager;

        /// <summary>
        /// Plugin manager window.
        /// </summary>
        public PluginManagerWindow WndPlugins { get; set; }

        /// <summary>
        /// Background image.
        /// </summary>
        public ImageBox ImgBackground { get; set; }

        public override void Add(ScreenManager screenManager)
        {
            base.Add(screenManager);

            ImgBackground = new ImageBox(Manager)
            {
                Image = Client.Content["gui.background"],
                SizeMode = SizeMode.Stretched
            };
            ImgBackground.SetSize(Window.Width, Window.Height);
            ImgBackground.SetPosition(0, 0);
            ImgBackground.Init();
            Window.Add(ImgBackground);

            WndPlugins = new PluginManagerWindow(Manager, this);
            WndPlugins.Init();
            Window.Add(WndPlugins);
            WndPlugins.Show();
        }

        public override void Remove()
        {
            Window.Remove(WndPlugins);
            Window.Remove(ImgBackground);
            WndPlugins?.Dispose();
        }
    }
}