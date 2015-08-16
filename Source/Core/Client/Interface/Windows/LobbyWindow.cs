using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bricklayer.Core.Client.Interface.Controls;
using Bricklayer.Core.Client.Interface.Screens;
using Bricklayer.Core.Common;
using Bricklayer.Core.Common.Net;
using Bricklayer.Core.Common.Net.Messages;
using Microsoft.Xna.Framework.Graphics;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Windows
{
    public sealed class LobbyWindow : Dialog
    {
        private static readonly string searchStr = "Search...";

        /// <summary>
        /// ComboBox to sort levels.
        /// </summary>
        public ComboBox CmbSort { get; }

        /// <summary>
        /// Groupbox for server information.
        /// </summary>
        public GroupPanel GrpServer { get; }

        /// <summary>
        /// Groupbox for level list.
        /// </summary>
        public GroupPanel GrpLobby { get; }

        /// <summary>
        /// Label for server name.
        /// </summary>
        public Label LblName { get; }

        /// <summary>
        /// Label for server description.
        /// </summary>
        public Label LblDescription { get; }

        /// <summary>
        /// Label for server info/MOTD.
        /// </summary>
        public Label LblInfo { get; }

        /// <summary>
        /// List of level controls.
        /// </summary>
        public ControlList<LobbyDataControl> LstLevels { get; }

        /// <summary>
        /// Search box.
        /// </summary>
        public TextBox TxtSearch { get; }

        /// <summary>
        /// Reload button to refresh view.
        /// </summary>
        public Button BtnReload { get; }

        /// <summary>
        /// Button to create new level.
        /// </summary>
        public Button BtnCreate { get; }

        /// <summary>
        /// Button to join selected level.
        /// </summary>
        public Button BtnJoin { get; }

        /// <summary>
        /// Button to disconnect from server.
        /// </summary>
        public Button BtnDisconnect { get; }

        /// <summary>
        /// Server's image/banner.
        /// </summary>
        public ImageBox ImgBanner { get; private set; }

        private readonly LobbyScreen lobbyScreen;
        private readonly List<string> sortFilters = new List<string> {"Online", "Rating", "Plays", "Random", "Mine"};

        public LobbyWindow(Manager manager, LobbyScreen screen)
            : base(manager)
        {
            lobbyScreen = screen;
            // Setup the window
            CaptionVisible = false;
            TopPanel.Visible = false;
            Movable = false;
            Resizable = false;
            Width = (int) (Manager.ScreenWidth*.9);
            Height = (int) (Manager.ScreenHeight*.9);
            Shadow = true;
            Center();

            // Group panels
            GrpServer = new GroupPanel(Manager)
            {
                Left = ClientWidth - Globals.Values.MaxBannerWidth - 1,
                Width = Globals.Values.MaxBannerWidth,
                Height = ClientHeight - BottomPanel.Height + 2,
                Text = "Server"
            };
            GrpServer.Init();
            Add(GrpServer);

            GrpLobby = new GroupPanel(Manager)
            {
                Width = ClientWidth - GrpServer.Width, // Fill remaining space
                Height = ClientHeight - BottomPanel.Height + 2,
                Text = "Levels"
            };
            GrpLobby.Init();
            Add(GrpLobby);

            // Top controls
            TxtSearch = new TextBox(Manager) {Left = 8, Top = 8, Width = (int) ((GrpLobby.Width/2d) - 16)};
            TxtSearch.Init();
            TxtSearch.Text = searchStr;
            TxtSearch.TextChanged += (sender, args) => { RefreshLevels(); };
            // Show "Search..." text, but make it dissapear on focus
            TxtSearch.FocusGained += (sender, args) =>
            {
                if (TxtSearch.Text.Trim() == searchStr)
                    TxtSearch.Text = string.Empty;
            };
            TxtSearch.FocusLost += (sender, args) =>
            {
                if (TxtSearch.Text.Trim() == string.Empty)
                    TxtSearch.Text = searchStr;
            };
            GrpLobby.Add(TxtSearch);

            CmbSort = new ComboBox(Manager) {Left = TxtSearch.Right + 8, Top = 8, Width = (GrpLobby.Width/2) - 16 - 20};
            CmbSort.Init();
            CmbSort.Items.AddRange(sortFilters);
            CmbSort.ItemIndex = 0;
            CmbSort.ItemIndexChanged += (sender, args) => { RefreshLevels(); };
            GrpLobby.Add(CmbSort);

            BtnReload = new Button(Manager)
            {
                Left = CmbSort.Right + 8,
                Top = 8,
                Width = 20,
                Height = 20,
                Text = string.Empty
            };
            BtnReload.Init();
            BtnReload.Glyph = new Glyph(screen.Client.Content["gui.icons.refresh"]);
            BtnReload.ToolTip.Text = "Refresh";
            BtnReload.Click += (sender, args) =>
            {
                BtnReload.Enabled = false;
                lobbyScreen.Client.Network.Send(new RequestMessage(MessageTypes.Init));
            };
            GrpLobby.Add(BtnReload);

            // Main level list
            LstLevels = new ControlList<LobbyDataControl>(Manager)
            {
                Left = 8,
                Top = TxtSearch.Bottom + 8,
                Width = GrpLobby.Width - 16,
                Height = GrpLobby.Height - 16 - TxtSearch.Bottom - 24
            };
            LstLevels.Init();
            LstLevels.DoubleClick += (sender, args) =>
            {
                if (LstLevels.ItemIndex < 0)
                    return;

                var ldc = (LobbyDataControl) LstLevels.Items[LstLevels.ItemIndex];
                // Make sure the user clicks the item and not the empty space in the list
                if (ldc.CheckPositionMouse(((MouseEventArgs) args).Position - LstLevels.AbsoluteRect.Location))
                    JoinLevel(LstLevels.ItemIndex);
            };

            GrpLobby.Add(LstLevels);

            // Server info labels
            LblName = new Label(Manager)
            {
                Text = "Loading...",
                Top = 8,
                Font = FontSize.Default20,
                Left = 8,
                Alignment = Alignment.MiddleCenter,
                Height = 30,
                Width = GrpServer.ClientWidth - 16
            };
            LblName.Init();
            GrpServer.Add(LblName);

            LblDescription = new Label(Manager)
            {
                Text = string.Empty,
                Top = 8 + LblName.Bottom,
                Left = 8,
                Alignment = Alignment.MiddleCenter,
                Width = GrpServer.ClientWidth - 16
            };
            LblDescription.Init();
            GrpServer.Add(LblDescription);

            LblInfo = new Label(Manager)
            {
                Text = string.Empty,
                Top = 8 + LblDescription.Bottom,
                Left = 8,
                Alignment = Alignment.TopLeft,
                Width = GrpServer.ClientWidth - 16,
                Height = GrpServer.Height
            };
            LblInfo.Init();
            GrpServer.Add(LblInfo);
            // Bottom buttons
            BtnCreate = new Button(Manager) {Top = 8, Text = "Create"};
            BtnCreate.Left = (ClientWidth/2) - (BtnCreate.Width/2);
            BtnCreate.Init();
            BtnCreate.Click += (sender, args) =>
            {
                var window = new CreateLevelDialog(manager, screen);
                window.Init();
                Manager.Add(window);
                window.ShowModal();
            };
            BottomPanel.Add(BtnCreate);

            BtnJoin = new Button(Manager) {Right = BtnCreate.Left - 8, Top = 8, Text = "Join"};
            BtnJoin.Init();
            BtnJoin.Click += (sender, args) => { JoinLevel(LstLevels.ItemIndex); };
            BottomPanel.Add(BtnJoin);

            BtnDisconnect = new Button(Manager) {Left = BtnCreate.Right + 8, Top = 8, Text = "Quit"};
            BtnDisconnect.Init();
            BtnDisconnect.Click += (sender, args) =>
            {
                lobbyScreen.Client.Network.NetClient.Disconnect("Left Lobby");
                lobbyScreen.ScreenManager.SwitchScreen(new LoginScreen());
            };
            BottomPanel.Add(BtnDisconnect);

            // When client gets banner data from server
            screen.Client.Events.Network.Game.LobbyBannerReceived.AddHandler(LobbyBannerReceived);

            // When client receives level data after joining/creating a level
            screen.Client.Events.Network.Game.LevelDataReceived.AddHandler(LevelDataReceived);

            // When the lobby data is received (after connecting or on reload)
            screen.Client.Events.Network.Game.InitReceived.AddHandler(InitReceived);

            LoadLevels();

            screen.Client.Network.Send(new RequestMessage(MessageTypes.Banner));
        }

        private void InitReceived(EventManager.NetEvents.GameServerEvents.InitEventArgs args)
        {
            lobbyScreen.ScreenManager.SwitchScreen(new LobbyScreen(args.Message.Description, args.Message.ServerName,
                args.Message.Intro, args.Message.Online, args.Message.Levels));
        }

        private void LevelDataReceived(EventManager.NetEvents.GameServerEvents.LevelDataEventArgs args)
        {
            lobbyScreen.Client.Level = args.Level;
            lobbyScreen.ScreenManager.SwitchScreen(new GameScreen());
        }

        private void LobbyBannerReceived(EventManager.NetEvents.GameServerEvents.BannerEventArgs args)
        {
            // Convert byte array to stream to be read.
            var stream = new MemoryStream(args.Banner);
            var image = Texture2D.FromStream(lobbyScreen.Client.GraphicsDevice, stream);

            if (image.Height > Globals.Values.MaxBannerHeight || image.Width > Globals.Values.MaxBannerWidth)
                return;

            ImgBanner = new ImageBox(Manager) {Image = image};
            ImgBanner.Init();
            ImgBanner.SetPosition((GrpServer.Width/2) - (ImgBanner.Image.Width/2), 0);
            ImgBanner.SetSize(image.Width, image.Height);
            GrpServer.Add(ImgBanner);

            // Move other controls down.
            LblName.Top = ImgBanner.Bottom + 8;
            LblDescription.Top = 8 + LblName.Bottom;
            LblInfo.Top = 8 + LblDescription.Bottom;
        }

        protected override void Dispose(bool disposing)
        {
            lobbyScreen.Client.Events.Network.Game.LobbyBannerReceived.RemoveHandler(LobbyBannerReceived);
            lobbyScreen.Client.Events.Network.Game.LevelDataReceived.RemoveHandler(LevelDataReceived);
            lobbyScreen.Client.Events.Network.Game.InitReceived.RemoveHandler(InitReceived);
            base.Dispose(disposing);
        }

        /// <summary>
        /// Joins a world
        /// </summary>
        public void JoinLevel(int index)
        {
            if (index < 0)
                return;
            lobbyScreen.Client.Network.Send(new JoinLevelMessage(((LobbyDataControl) LstLevels.Items[index]).Data.UUID));
        }

        private void FilterLevels()
        {
            // Filter search results
            if (TxtSearch.Text != searchStr && !string.IsNullOrWhiteSpace(TxtSearch.Text))
                LstLevels.Items =
                    LstLevels.Items.Where(
                        x => ((LobbyDataControl) x).Data.Name.ToLower().Contains(TxtSearch.Text.ToLower())).ToList();

            // Filter by category
            switch (CmbSort.ItemIndex)
            {
                case 0:
                    LstLevels.Items =
                        LstLevels.Items.OrderByDescending(x => ((LobbyDataControl) x).Data.Online).ToList();
                    break;
                case 1:
                    LstLevels.Items =
                        LstLevels.Items.OrderByDescending(x => ((LobbyDataControl) x).Data.Rating).ToList();
                    break;
                case 2:
                    LstLevels.Items = LstLevels.Items.OrderByDescending(x => ((LobbyDataControl) x).Data.Plays).ToList();
                    break;
                case 3:
                    LstLevels.Items.Shuffle();
                    break;
            }
        }

        /// <summary>
        /// Loads levels from the recieved lobby message
        /// </summary>
        private void LoadLevels()
        {
            // Set text with what has been recieved
            GrpServer.Text = "Server [" + lobbyScreen.Client.Network.NetClient.ServerConnection.RemoteEndPoint + "]";
            LblName.Text = lobbyScreen.Name;
            LblDescription.Text = lobbyScreen.Description;
            LblDescription.Height =
                (int)
                    Manager.Skin.Fonts[LblDescription.Font.ToString()].Resource.MeasureRichString(LblDescription.Text,
                        Manager).Y;

            LblInfo.Top = 16 + LblDescription.Bottom;
            LblInfo.Text = ReplaceVariables(lobbyScreen.Intro);
            LblInfo.Height =
                (int) Manager.Skin.Fonts[LblInfo.Font.ToString()].Resource.MeasureRichString(LblInfo.Text, Manager).Y;

            RefreshLevels();
        }

        private void RefreshLevels()
        {
            LstLevels.Items.Clear();
            if (lobbyScreen != null)
                LstLevels.Items.AddRange(
                    lobbyScreen.Levels.Select(x => new LobbyDataControl(lobbyScreen, Manager, x, LstLevels)));
            FilterLevels();
        }

        /// <summary>
        /// Replaces variables like "$Online" with text in the server info
        /// </summary>
        private string ReplaceVariables(string infoText)
        {
            // Replace variables in the info text
            return infoText.Replace("$Online", lobbyScreen.Online.ToString())
                .Replace("$Name", lobbyScreen.Name)
                .Replace("$Levels", lobbyScreen.Levels.Count.ToString());
        }
    }
}