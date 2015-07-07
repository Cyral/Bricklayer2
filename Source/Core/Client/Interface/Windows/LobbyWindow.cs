using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bricklayer.Client.Interface;
using Bricklayer.Core.Client.Interface.Controls;
using Bricklayer.Core.Client.Interface.Screens;
using Bricklayer.Core.Common;
using Bricklayer.Core.Common.Net;
using Bricklayer.Core.Common.Net.Messages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoForce.Controls;
using EventArgs = MonoForce.Controls.EventArgs;

namespace Bricklayer.Core.Client.Interface.Windows
{
    public sealed class LobbyWindow : Dialog
    {
        private static readonly string searchStr = "Search...";
        private readonly ComboBox cmbSort;
        private readonly GroupPanel grpServer;
        private readonly Label lblName, lblDescription, lblInfo;
        private readonly LobbyScreen lobbyScreen;
        private readonly ControlList<LobbyDataControl> lstLevels;
        private readonly List<string> sortFilters = new List<string> {"Online", "Rating", "Plays", "Random", "Mine"};
        //Controls
        private readonly TextBox txtSearch;

        private ImageBox imgBanner;
        private byte[] bannerInfo;

        public LobbyWindow(Manager manager, LobbyScreen screen)
            : base(manager)
        {
            lobbyScreen = screen;
            //Setup the window
            CaptionVisible = false;
            TopPanel.Visible = false;
            Movable = false;
            Resizable = false;
            Width = (int)(Manager.ScreenWidth * .9);
            Height = (int)(Manager.ScreenHeight * .9);
            Shadow = true;
            Center();

            //Group panels
            grpServer = new GroupPanel(Manager)
            {
                Left = ClientWidth - Globals.Values.MaxBannerWidth - 1,
                Width = Globals.Values.MaxBannerWidth,
                Height = ClientHeight - BottomPanel.Height + 2,
                Text = "Server"
            };
            grpServer.Init();
            Add(grpServer);
           
            var grpLobby = new GroupPanel(Manager)
            {
                Width = ClientWidth - grpServer.Width, //Fill remaining space
                Height = ClientHeight - BottomPanel.Height + 2,
                Text = "Levels"
            };
            grpLobby.Init();
            Add(grpLobby);

            //Top controls
            txtSearch = new TextBox(Manager) {Left = 8, Top = 8, Width = (int)((grpLobby.Width / 2d) - 16)};
            txtSearch.Init();
            txtSearch.Text = searchStr;
            txtSearch.TextChanged += delegate { RefreshLevels(); };
            //Show "Search..." text, but make it dissapear on focus
            txtSearch.FocusGained += delegate
            {
                if (txtSearch.Text.Trim() == searchStr)
                    txtSearch.Text = string.Empty;
            };
            txtSearch.FocusLost += delegate
            {
                if (txtSearch.Text.Trim() == string.Empty)
                    txtSearch.Text = searchStr;
            };
            grpLobby.Add(txtSearch);

            cmbSort = new ComboBox(Manager) {Left = txtSearch.Right + 8, Top = 8, Width = (int)((grpLobby.Width / 2) - 16 - 20)};
            cmbSort.Init();
            cmbSort.Items.AddRange(sortFilters);
            cmbSort.ItemIndex = 0;
            cmbSort.ItemIndexChanged += delegate { RefreshLevels(); };
            grpLobby.Add(cmbSort);

            var btnReload = new Button(Manager)
            {
                Left = cmbSort.Right + 8,
                Top = 8,
                Width = 20,
                Height = 20,
                Text = string.Empty
            };
            btnReload.Init();
            btnReload.Glyph = new Glyph(screen.Client.Content["gui.icons.refresh"]);
            btnReload.ToolTip.Text = "Refresh";
            btnReload.Click += delegate
            {
                btnReload.Enabled = false;
                screen.Client.Network.Send(new RequestMessage(MessageTypes.Init));
            };
            grpLobby.Add(btnReload);

            //Main level list
            lstLevels = new ControlList<LobbyDataControl>(Manager)
            {
                Left = 8,
                Top = txtSearch.Bottom + 8,
                Width = grpLobby.Width - 16,
                Height = grpLobby.Height - 16 - txtSearch.Bottom - 24
            };
            lstLevels.Init();
            grpLobby.Add(lstLevels);

            // When client gets banner data from server
            screen.Client.Events.Network.Game.LobbyBannerRecieved.AddHandler(args =>
            {
                bannerInfo = args.Banner;
                //Convert byte array to stream to be read
                var stream = new MemoryStream(bannerInfo);
                var image = Texture2D.FromStream(screen.Client.GraphicsDevice, stream);

                if (image.Height <= Globals.Values.MaxBannerHeight && image.Width <= Globals.Values.MaxBannerWidth)
                {
                    imgBanner = new ImageBox(Manager)
                    {
                        Image = image,
                    };
                    imgBanner.Init();
                    imgBanner.SetPosition((grpServer.Width / 2) - (imgBanner.Image.Width / 2), 0);
                    imgBanner.SetSize(image.Width, image.Height);
                    grpServer.Add(imgBanner);

                    lblName.Top = imgBanner.Bottom + 8;
                    lblDescription.Top = 8 + lblName.Bottom;
                    lblInfo.Top = 8 + lblDescription.Bottom;
                }
            });

            //Server info labels
            lblName = new Label(Manager)
            {
                Text = "Loading...",
                Top = 8,
                Font = FontSize.Default20,
                Left = 8,
                Alignment = Alignment.MiddleCenter,
                Height = 30,
                Width = grpServer.ClientWidth - 16
            };
            lblName.Init();
            grpServer.Add(lblName);

            lblDescription = new Label(Manager)
            {
                Text = string.Empty,
                Top = 8 + lblName.Bottom,
                Left = 8,
                Alignment = Alignment.MiddleCenter,
                Width = grpServer.ClientWidth - 16
            };
            lblDescription.Init();
            grpServer.Add(lblDescription);

            lblInfo = new Label(Manager)
            {
                Text = string.Empty,
                Top = 8 + lblDescription.Bottom,
                Left = 8,
                Alignment = Alignment.TopLeft,
                Width = grpServer.ClientWidth - 16,
                Height = grpServer.Height
            };
            lblInfo.Init();
            grpServer.Add(lblInfo);
            //Bottom buttons
            var btnCreate = new Button(Manager) {Top = 8, Text = "Create"};
            btnCreate.Left = (ClientWidth / 2) - (btnCreate.Width / 2);
            btnCreate.Init();
            btnCreate.Click += delegate
            {
                var window = new CreateWorldDialog(manager, this);
                window.Init();
                Manager.Add(window);
                window.Show();
            };
            BottomPanel.Add(btnCreate);

            var btnJoin = new Button(Manager) {Right = btnCreate.Left - 8, Top = 8, Text = "Join"};
            btnJoin.Init();
            btnJoin.Click += delegate { JoinLevel(lstLevels.ItemIndex); };
            BottomPanel.Add(btnJoin);

            var btnDisconnect = new Button(Manager) {Left = btnCreate.Right + 8, Top = 8, Text = "Quit"};
            btnDisconnect.Init();
            btnDisconnect.Click += delegate
            {
                screen.Client.Network.NetClient.Disconnect("Left Lobby");
                MainWindow.ScreenManager.SwitchScreen(new LoginScreen());
            };
            BottomPanel.Add(btnDisconnect);

            screen.Client.Events.Network.Game.Init.AddHandler(OnInit);

            LoadLevels();

            screen.Client.Network.Send(new RequestMessage(MessageTypes.Banner));
        }

        private void OnInit(EventManager.NetEvents.GameServerEvents.InitEventArgs args)
        {
            lobbyScreen.ScreenManager.SwitchScreen(new LobbyScreen(args.Message.Description, args.Message.ServerName,
               args.Message.Intro, args.Message.Online, args.Message.Levels));
        }

        protected override void Dispose(bool disposing)
        {
            lobbyScreen.Client.Events.Network.Game.Init.RemoveHandler(OnInit);
            base.Dispose(disposing);
        }

        /// <summary>
        /// Joins a world
        /// </summary>
        public void JoinLevel(int index)
        {
            if (index >= 0)
            {
                //MainWindow.ScreenManager.SwitchScreen(new Screen(new Action((new Action(() =>
                //{
                // screen.Client.Network.Send(new Bricklayer.Common.Networking.Messages.JoinLevelMessage(
                //     (LevelListCtrl.Items[index] as LobbyDataControl).Data.ID));
                // })))));
            }
        }

        private void FilterLevels()
        {
            //Filter search results
            if (txtSearch.Text != searchStr && !string.IsNullOrWhiteSpace(txtSearch.Text))
                lstLevels.Items =
                    lstLevels.Items.Where(
                        x => ((LobbyDataControl)x).Data.Name.ToLower().Contains(txtSearch.Text.ToLower())).ToList();

            //Filter by category
            switch (cmbSort.ItemIndex)
            {
                case 0:
                    lstLevels.Items = lstLevels.Items.OrderByDescending(x => ((LobbyDataControl)x).Data.Online).ToList();
                    break;
                case 1:
                    lstLevels.Items = lstLevels.Items.OrderByDescending(x => ((LobbyDataControl)x).Data.Rating).ToList();
                    break;
                case 2:
                    lstLevels.Items = lstLevels.Items.OrderByDescending(x => ((LobbyDataControl)x).Data.Plays).ToList();
                    break;
                case 3:
                    lstLevels.Items.Shuffle();
                    break;
            }
        }

        /// <summary>
        /// Loads levels from the recieved lobby message
        /// </summary>
        private void LoadLevels()
        {
            //Set text with what has been recieved
            grpServer.Text = "Server [" + lobbyScreen.Client.Network.NetClient.ServerConnection.RemoteEndPoint + "]";
            lblName.Text = lobbyScreen.Name;
            lblDescription.Text = lobbyScreen.Description;
            lblDescription.Height = (int)MainWindow.DefaultSpriteFont.MeasureRichString(lblDescription.Text, Manager).Y;

            lblInfo.Top = 16 + lblDescription.Bottom;
            lblInfo.Text = ReplaceVariables(lobbyScreen.Intro);
            lblInfo.Height = (int)MainWindow.DefaultSpriteFont.MeasureRichString(lblInfo.Text, Manager).Y;

            RefreshLevels();
        }

        private void RefreshLevels()
        {
            var screen = MainWindow.ScreenManager.Current as LobbyScreen;
            lstLevels.Items.Clear();
            if (screen != null)
                foreach (var s in screen.Levels)
                    lstLevels.Items.Add(new LobbyDataControl(screen, Manager, s, lstLevels));
            FilterLevels();
        }

        /// <summary>
        /// Replaces variables like "$Online" with text in the server info
        /// </summary>
        private string ReplaceVariables(string infoText)
        {
            //Replace variables in the info text
            infoText = infoText.Replace("$Online", lobbyScreen.Online.ToString());
            infoText = infoText.Replace("$Name", lobbyScreen.Name);
            infoText = infoText.Replace("$Levels", lobbyScreen.Levels.Count.ToString());
            return infoText;
        }
    }
}
