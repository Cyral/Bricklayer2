using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Core.Client.Interface.Screens;
using Bricklayer.Core.Common;
using Microsoft.Xna.Framework;
using MonoForce.Controls;
using EventArgs = System.EventArgs;

namespace Bricklayer.Core.Client.Interface.Windows
{
    /// <summary>
    /// The second window shown in Bricklayer, listing the servers.
    /// </summary>
    internal sealed class ServerWindow : Dialog
    {
        private LoginScreen screen;

        public ServerWindow(Manager manager, LoginScreen screen) : base(manager)
        {
            this.screen = screen;
            screen.Client.State = GameState.Game;

            //Setup the window
            CaptionVisible = false;
            Caption.Text = "Welcome to Bricklayer!";
            Description.Text =
                "An open source, fully moddable and customizable 2D\nbuilding game.";
            Movable = false;
            Resizable = false;
            Width = 450;
            Height = 218;
            TopPanel.Height = 72;
            Shadow = true;
            Center();

            BottomPanel.Height = 100;
            BottomPanel.Top = Height - BottomPanel.Height;
        }

        private void ShowError(Manager manager, string message)
        {
            var msgBox = new MessageBox(Manager, MessageBoxType.Warning, message, "Error");
            msgBox.Init();
            manager.Add(msgBox);
            msgBox.ShowModal();
        }
    }
}
