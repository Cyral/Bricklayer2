using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Bricklayer.Core.Client.Interface.Screens;
using Bricklayer.Core.Common.Data;
using Bricklayer.Core.Common.Net.Messages;
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
        /// <summary>
        /// Label for server description or error message.
        /// </summary>
        public Label LblDescription { get; }

        /// <summary>
        /// Server hostname or IP address label.
        /// </summary>
        public Label LblHost { get; }

        /// <summary>
        /// Label for user-defined name.
        /// </summary>
        public Label LblName { get; }

        /// <summary>
        /// Label for stats such as players online.
        /// </summary>
        public Label LblStats { get; }

        /// <summary>
        /// Server data this control is for.
        /// </summary>
        public ServerData Data { get; }

        /// <summary>
        /// Resolved IP endpoint.
        /// </summary>
        public IPEndPoint EndPoint { get; private set; }

        /// <summary>
        /// Status icon.
        /// </summary>
        public ImageBox ImgStatus { get; }

        /// <summary>
        /// Indicates if the host has been resolved yet.
        /// </summary>
        public bool ResolvedHost { get; private set; }

        private readonly Manager manager;
        private readonly Color offlineColor = Color.Red, onlineColor = Color.Lime;
        private readonly ServerScreen screen;
        private Timer pingTimer;

        public ServerDataControl(ServerScreen screen, Manager manager, ServerData server, Control parent)
            : base(manager)
        {
            this.screen = screen;
            this.manager = manager;

            // Setup
            Passive = false;
            Height = 76;
            Data = server;

            Width = parent.Width + 8;
            // Background "gradient" image
            // TODO: Make an actual control. not a statusbar
            var gradient = new StatusBar(manager);
            gradient.Init();
            gradient.Height = ClientHeight;
            gradient.Alpha = .8f;
            Add(gradient);

            // Add controls
            LblName = new Label(Manager)
            {
                Width = Width,
                Text = Data.Name,
                Left = 4,
                Top = 4,
                Font = FontSize.Default14,
                Alignment = Alignment.TopLeft
            };
            LblName.Init();
            Add(LblName);

            LblStats = new Label(Manager)
            {
                Width = Width,
                Text = string.Empty,
                Alignment = Alignment.TopLeft,
                Top = 4,
                Font = FontSize.Default12
            };
            LblStats.Init();
            Add(LblStats);

            LblDescription = new Label(Manager)
            {
                Width = Width,
                Left = 4,
                Top = LblName.Bottom + 6,
                Font = FontSize.Default8,
                Alignment = Alignment.TopLeft
            };
            LblDescription.Init();
            LblDescription.Text = "Querying server for data...";
            LblDescription.Height = Manager.Skin.Fonts["Default8"].Height*2;
            Add(LblDescription);

            ImgStatus = new ImageBox(Manager)
            {
                Top = LblStats.Top + 6,
                Left = 4,
                Width = 10,
                Height = 10,
                Image = screen.Client.Content["gui.icons.ping"]
            };
            ImgStatus.Init();
            ImgStatus.Color = Color.Transparent;
            Add(ImgStatus);

            LblHost = new Label(Manager)
            {
                Width = Width,
                Text = Data.GetHostString(),
                Alignment = Alignment.TopLeft,
                Left = 4,
                Top = LblDescription.Bottom,
                TextColor = Color.Gray
            };
            LblHost.Init();
            Add(LblHost);

            this.screen.Client.Events.Network.Game.ServerInfoReceived.AddHandler(args =>
            {
                if (EndPoint != null && args.Host.Equals(EndPoint))
                {
                    pingTimer?.Dispose();

                    LblStats.Text = args.Players + "/" + args.MaxPlayers;
                    LblDescription.Text = args.Description;

                    LblStats.TextColor = onlineColor;
                    LblStats.Left = (ClientWidth -
                                     (int) Manager.Skin.Fonts["Default12"].Resource.MeasureString(LblStats.Text).X) - 4 -
                                    32;
                    ImgStatus.Right = LblStats.Left - 2;
                    ImgStatus.Color = onlineColor;
                }
            });
        }

        public override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            // Don't draw anything
            // base.DrawControl(renderer,rect,gameTime);
        }

        public async void PingServer()
        {
            // Resolve IP from host/address and port
            if (!ResolvedHost && !string.IsNullOrEmpty(Data.Host) && Data.Port > 0 && Data.Port < ushort.MaxValue)
            {
                await Task.Factory.StartNew(() =>
                {
                    var host =
                        NetUtility.Resolve(Data.Host);
                    if (host != null)
                        EndPoint = new IPEndPoint(host,
                            Data.Port);
                    ResolvedHost = true;
                });
            }

            if (EndPoint != null)
            {
                // Setup ping timer for 5 seconds
                pingTimer = new Timer(state => { Error("Connection timed out."); }, null, 5000, Timeout.Infinite);
                screen.Client.Network.SendUnconnected(EndPoint, new ServerInfoMessage());
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

            LblDescription.Text = error;
            LblStats.Left = (ClientWidth - 4 - 32);
            ImgStatus.Right = LblStats.Left;
            ImgStatus.Color = offlineColor;
        }
    }
}