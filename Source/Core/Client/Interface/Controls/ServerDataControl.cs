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
    internal class ServerDataControl : Control
    {
        public ServerSaveData Data;

        private Label Name, Motd, Stats, Host;
        private StatusBar Gradient;

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
            Gradient = new StatusBar(manager);
            Gradient.Init();
            Gradient.Alpha = .8f;
            Add(Gradient);

            //Add controls
            Name = new Label(Manager) { Width = this.Width, Text = server.Name, Left = 4, Top = 4, Font = FontSize.Default14, Alignment = Alignment.TopLeft };
            Name.Init();
            Add(Name);

            Stats = new Label(Manager) { Width = this.Width, Text = string.Empty, Alignment = Alignment.TopLeft, Top = 4, Font = FontSize.Default14, };
            Stats.Init();
            Add(Stats);

            Motd = new Label(Manager) { Width = this.Width, Left = 4, Top = Name.Bottom + 8, Font = FontSize.Default8, Alignment = Alignment.TopLeft };
            Motd.Init();
            Motd.Text = "Querying server for data...";
            Motd.Height = (int)Manager.Skin.Fonts["Default8"].Height * 2;
            Add(Motd);

            Host = new Label(Manager) { Width = this.Width, Text = server.GetHostString(), Alignment = Alignment.TopLeft, Left = 4, Top = Motd.Bottom, TextColor = Color.LightGray };
            Host.Init();
            Add(Host);

            Screen.Client.Events.Network.Game.ServerInfo.AddHandler(args =>
            {
                Stats.Text = args.Players + "/" + args.MaxPlayers;
                Motd.Text = args.Description;

                Stats.TextColor = onlineColor;
            });

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
