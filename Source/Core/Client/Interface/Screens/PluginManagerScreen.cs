using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Core.Client.Interface.Windows;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Screens
{
    public class PluginManagerScreen : Screen
    {
        protected internal override GameState State => GameState.PluginManager;
        private PluginManagerWindow wndPlugins;
        private ImageBox imgBackground;

        public override void Add(ScreenManager screenManager)
        {
            base.Add(screenManager);
            Client.State = GameState.PluginManager;

            imgBackground = new ImageBox(Manager)
            {
                Image = Client.Content["gui.background"],
                SizeMode = SizeMode.Stretched
            };
            imgBackground.SetSize(Window.Width, Window.Height);
            imgBackground.SetPosition(0, 0);
            imgBackground.Init();
            Window.Add(imgBackground);

            wndPlugins = new PluginManagerWindow(Manager, this);
            wndPlugins.Init();
            Window.Add(wndPlugins);
            wndPlugins.Show();
         }
        
        public override void Remove()
        {
            Window.Remove(wndPlugins);
            Window.Remove(imgBackground);
            wndPlugins?.Dispose();
        }
    }
}
