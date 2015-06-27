using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Bricklayer.Core.Client.Interface.Screens;
using Bricklayer.Core.Common.Net.Messages;
using Bricklayer.Core.Server.Data;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Controls
{
    /// <summary>
    /// A control for displaying a servers name, players online, motd, etc in the serverlist
    /// </summary>
    internal sealed class ServerDataControl : Control
    {
        private readonly StatusBar gradient;
        private readonly Label lblDescription;
        private readonly Label lblHost;
        private readonly Label lblName;
        private readonly Label lblStats;
        private readonly Color offlineColor = Color.Red, onlineColor = new Color(0, 205, 5);
        private readonly LoginScreen screen;
        private readonly ServerSaveData data;
        private IPEndPoint endPoint;
        private ImageBox imgStatus;
        private Timer pingTimer;
        private bool resolvedHost;

        public ServerDataControl(LoginScreen screen, Manager manager, ServerSaveData server) : base(manager)
        {
            this.screen = screen;


            //Setup
            Passive = false;
            Height = 76;
            ClientWidth = 450 - 16;
            data = server;

            //Background "gradient" image
            //TODO: Make an actual control. not a statusbar
            gradient = new StatusBar(manager);
            gradient.Init();
            gradient.Height = ClientHeight;
            gradient.Alpha = .8f;
            Add(gradient);

            //Add controls
            lblName = new Label(Manager)
            {
                Width = Width,
                Text = server.Name,
                Left = 4,
                Top = 4,
                Font = FontSize.Default14,
                Alignment = Alignment.TopLeft
            };
            lblName.Init();
            Add(lblName);

            lblStats = new Label(Manager)
            {
                Width = Width,
                Text = string.Empty,
                Alignment = Alignment.TopLeft,
                Top = 4,
                Font = FontSize.Default12
            };
            lblStats.Init();
            Add(lblStats);

            lblDescription = new Label(Manager)
            {
                Width = Width,
                Left = 4,
                Top = lblName.Bottom + 6,
                Font = FontSize.Default8,
                Alignment = Alignment.TopLeft
            };
            lblDescription.Init();
            lblDescription.Text = "Querying server for data...";
            lblDescription.Height = Manager.Skin.Fonts["Default8"].Height * 2;
            Add(lblDescription);

            imgStatus = new ImageBox(Manager)
            {
                Top = lblStats.Top + 6,
                Left = 4,
                Width = 10,
                Height = 10,
                Image = screen.Client.Content["gui.icons.ping"],
            };
            imgStatus.Init();
            imgStatus.Color = Color.Transparent;
            Add(imgStatus);

            lblHost = new Label(Manager)
            {
                Width = Width,
                Text = server.GetHostString(),
                Alignment = Alignment.TopLeft,
                Left = 4,
                Top = lblDescription.Bottom,
                TextColor = Color.Gray
            };
            lblHost.Init();
            Add(lblHost);

            this.screen.Client.Events.Network.Game.ServerInfo.AddHandler(args =>
            {
                if (endPoint != null && args.Host.Equals(endPoint))
                {
                    pingTimer.Dispose();

                    lblStats.Text = args.Players + "/" + args.MaxPlayers;
                    lblDescription.Text = args.Description;

                    lblStats.TextColor = onlineColor;
                    lblStats.Left = (ClientWidth -
                                     (int)Manager.Skin.Fonts["Default12"].Resource.MeasureString(lblStats.Text).X) - 4 -
                                    32;
                    imgStatus.Right = lblStats.Left - 2;
                    imgStatus.Color = onlineColor;
                }
            });
        }

        public override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            //Don't draw anything
            //base.DrawControl(renderer,rect,gameTime);
        }

        public async void PingServer()
        {
            //Resolve IP from host/address and port
            if (!resolvedHost && !string.IsNullOrEmpty(data.Host) && data.Port > 0 && data.Port < ushort.MaxValue)
            {
                await Task.Factory.StartNew(() =>
                {
                    var host =
                        NetUtility.Resolve(data.Host);
                    if (host != null)
                        endPoint = new IPEndPoint(host,
                            data.Port);
                    resolvedHost = true;
                });
            }

            if (endPoint != null)
            {
                //Setup ping timer for 5 seconds
                pingTimer = new Timer(state => { Error("Connection timed out."); }, null, 5000, Timeout.Infinite);
                screen.Client.Network.SendUnconnected(endPoint, new ServerInfoMessage());
            }
            else
                Error("Invalid host.");
        }

        protected override void Dispose(bool disposing)
        {
            pingTimer.Dispose();
        }

        /// <summary>
        /// Display an error for the server, such as connection timeout.
        /// </summary>
        private void Error(string error)
        {
            pingTimer?.Dispose();

            lblDescription.Text = error;
            lblStats.Left = (ClientWidth - 4 - 32);
            imgStatus.Right = lblStats.Left;
            imgStatus.Color = offlineColor;
        }
    }
}