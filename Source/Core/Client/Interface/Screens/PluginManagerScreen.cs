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

        public override void Setup(ScreenManager screenManager)
        {
            base.Setup(screenManager);

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

            WndPlugins = new PluginManagerWindow(Manager, this);
            WndPlugins.Init();
            AddControl(WndPlugins);
            WndPlugins.Show();
        }
    }
}