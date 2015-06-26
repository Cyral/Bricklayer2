using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bricklayer.Core.Client.Interface.Screens;
using Bricklayer.Core.Common.Net.Messages;
using Bricklayer.Core.Server.Data;
using Microsoft.Xna.Framework;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Controls
{
    /// <summary>
    /// A control for displaying a servers name, players online, motd, etc in the serverlist
    /// </summary>
    internal sealed ServerDataControl : Control
    {
        public ServerSaveData Data;

        private Label lblName, lblDescription, lblStats, lblHost;
        private ImageBox imgStatus;
        private StatusBar gradient;

        private readonly Color offlineColor = Color.Red, onlineColor = new Color(0, 205, 5);

        private LoginScreen Screen;
        public ServerDataControl(LoginScreen screen, Manager manager, ServerSaveData server)
            : base(manager)
        {
            Screen = screen;
            //Setup
            Passive = false;
            Height = 76;
            ClientWidth = 450 - 16;
            Data = server;

            //Background "gradient" image
            //TODO: Make an actual control. not a statusbar
            gradient = new StatusBar(manager);
            gradient.Init();
            gradient.Alpha = .8f;
            Add(gradient);

            //Add controls
            lblName = new Label(Manager) { Width = Width, Text = server.Name, Left = 4, Top = 4, Font = FontSize.Default14, Alignment = Alignment.TopLeft };
            lblName.Init();
            Add(lblName);

            lblStats = new Label(Manager) { Width = Width, Text = string.Empty, Alignment = Alignment.TopLeft, Top = 4, Font = FontSize.Default14, };
            lblStats.Init();
            Add(lblStats);

            lblDescription = new Label(Manager) { Width = Width, Left = 4, Top = lblName.Bottom + 4, Font = FontSize.Default8, Alignment = Alignment.TopLeft };
            lblDescription.Init();
            lblDescription.Text = "Querying server for data...";
            lblDescription.Height = Manager.Skin.Fonts["Default8"].Height * 2;
            Add(lblDescription);

            //imgStatus = new ImageBox(Manager) {Top = lblDescription.Bottom + 2, Left = 4, Width = Height = 8, };
            //imgStatus.Init();
            //imgStatus.Color = Color.Green;
            //Add(imgStatus);

        }

        public void PingServer()
        {
            
            Screen.Client.Network.SendUnconnected(Data.IP, Data.Port, new RequestServerInfo());

              //  //Error text
              //  Stats.Text = "X";
              //  Stats.TextColor = offlineColor;
              //
              //  Motd.Text = error;
            Stats.Left = (ClientWidth - (int)Manager.Skin.Fonts["Default14"].Resource.MeasureString(Stats.Text).X) - 4 - 32;
        }

        public override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            //Don't draw anything
            //base.DrawControl(renderer,rect,gameTime);
        }
    }
}
