using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Client.Interface;
using Bricklayer.Core.Client.Interface.Controls;
using Bricklayer.Core.Client.Interface.Screens;
using Bricklayer.Core.Common.Net;
using Bricklayer.Core.Server.Data;
using Microsoft.Xna.Framework;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Windows
{
    public class LobbyWindow : Dialog
    {
        private const string searchStr = "Search...";
        private readonly List<string> sortFilters = new List<string>() { "Online", "Rating", "Plays", "Random", "Mine" };
        //Controls
        private TextBox txtSearch;
        private ComboBox cmbSort;
        private Button btnJoin, btnCreate, btnDisconnect, btnReload;
        private GroupPanel grpLobby, grpServer;
        private Label lblName, lblDescription, lblInfo;

        public ControlList<LobbyDataControl> RoomListCtrl;

        public LobbyScreen lobbyScreen;

        public LobbyWindow(Manager manager, LobbyScreen screen)
            : base(manager)
        {

            lobbyScreen = screen;
            //Setup the window
            CaptionVisible = false;
            TopPanel.Visible = false;
            Movable = false;
            Resizable = false;
            Width = 700;
            Height = 500;
            Shadow = true;
            Center();

            //Group panels
            grpLobby = new GroupPanel(Manager) { Width = ClientWidth / 2, Height = ClientHeight - BottomPanel.Height + 2, Text = "Rooms" };
            grpLobby.Init();
            Add(grpLobby);

            grpServer = new GroupPanel(Manager) { Left = (ClientWidth / 2) - 1, Width = (ClientWidth / 2) + 1, Height = ClientHeight - BottomPanel.Height + 2, Text = "Server" };
            grpServer.Init();
            Add(grpServer);

            //Top controls
            txtSearch = new TextBox(Manager) { Left = 8, Top = 8, Width = (ClientWidth / 4) - 16, };
            txtSearch.Init();
            txtSearch.Text = searchStr;
            txtSearch.TextChanged += new MonoForce.Controls.EventHandler(delegate (object o, MonoForce.Controls.EventArgs e)
            {
                RefreshRooms();
            });
            //Show "Search..." text, but make it dissapear on focus
            txtSearch.FocusGained += new MonoForce.Controls.EventHandler(delegate (object o, MonoForce.Controls.EventArgs e)
            {
                if (txtSearch.Text.Trim() == searchStr)
                    txtSearch.Text = string.Empty;
            });
            txtSearch.FocusLost += new MonoForce.Controls.EventHandler(delegate (object o, MonoForce.Controls.EventArgs e)
            {
                if (txtSearch.Text.Trim() == string.Empty)
                    txtSearch.Text = searchStr;
            });
            grpLobby.Add(txtSearch);

            cmbSort = new ComboBox(Manager) { Left = txtSearch.Right + 8, Top = 8, Width = (ClientWidth / 4) - 16 - 20, };
            cmbSort.Init();
            cmbSort.Items.AddRange(sortFilters);
            cmbSort.ItemIndex = 0;
            cmbSort.ItemIndexChanged += new MonoForce.Controls.EventHandler(delegate (object o, MonoForce.Controls.EventArgs e)
            {
                RefreshRooms();
            });
            grpLobby.Add(cmbSort);

            btnReload = new Button(Manager) { Left = cmbSort.Right + 8, Top = 8, Width = 20, Height = 20, Text = string.Empty, };
            btnReload.Init();
            //btnReload.Glyph = new Glyph(ContentPack.Textures["gui\\icons\\refresh"]);
            btnReload.ToolTip.Text = "Refresh";
            btnReload.Click += new MonoForce.Controls.EventHandler(delegate (object o, MonoForce.Controls.EventArgs e)
            {
               // Game.NetManager.Send(new RequestMessage(MessageTypes.Lobby));
            });
            grpLobby.Add(btnReload);

            //Main room list
            RoomListCtrl = new ControlList<LobbyDataControl>(Manager) { Left = 8, Top = txtSearch.Bottom + 8, Width = grpLobby.Width - 16, Height = grpLobby.Height - 16 - txtSearch.Bottom - 24 };
            RoomListCtrl.Init();
            grpLobby.Add(RoomListCtrl);

            //Server info labels
            lblName = new Label(Manager) { Text = "Loading...", Top = 8, Font = FontSize.Default20, Left = 8, Alignment = Alignment.MiddleCenter, Height = 30, Width = grpServer.ClientWidth - 16 };
            lblName.Init();
            grpServer.Add(lblName);

            lblDescription = new Label(Manager) { Text = string.Empty, Top = 8 + lblName.Bottom, Left = 8, Alignment = Alignment.MiddleCenter, Width = grpServer.ClientWidth - 16 };
            lblDescription.Init();
            grpServer.Add(lblDescription);

            lblInfo = new Label(Manager) { Text = string.Empty, Top = 8 + lblDescription.Bottom, Left = 8, Alignment = Alignment.TopLeft, Width = grpServer.ClientWidth - 16, Height = grpServer.Height };
            lblInfo.Init();
            grpServer.Add(lblInfo);
            //Bottom buttons
            btnCreate = new Button(Manager) { Top = 8, Text = "Create" };
            btnCreate.Left = (ClientWidth / 2) - (btnCreate.Width / 2);
            btnCreate.Init();
            btnCreate.Click += new MonoForce.Controls.EventHandler(delegate (object o, MonoForce.Controls.EventArgs e)
            {
                CreateWorldDialog window = new CreateWorldDialog(manager, this);
                window.Init();
                Manager.Add(window);
                window.Show();
            });
            BottomPanel.Add(btnCreate);

            btnJoin = new Button(Manager) { Right = btnCreate.Left - 8, Top = 8, Text = "Join" };
            btnJoin.Init();
            btnJoin.Click += new MonoForce.Controls.EventHandler(delegate (object o, MonoForce.Controls.EventArgs e)
            {
                JoinRoom(RoomListCtrl.ItemIndex);
            });
            BottomPanel.Add(btnJoin);

            btnDisconnect = new Button(Manager) { Left = btnCreate.Right + 8, Top = 8, Text = "Quit" };
            btnDisconnect.Init();
            btnDisconnect.Click += new MonoForce.Controls.EventHandler(delegate (object o, MonoForce.Controls.EventArgs e)
            {
                screen.Client.Network.NetClient.Disconnect("Left Lobby");
                screen.Client.State = GameState.Lobby;
                MainWindow.ScreenManager.SwitchScreen(new LoginScreen());
            });
            BottomPanel.Add(btnDisconnect);

            //When finished, request server send lobby data
            //Game.NetManager.Send(new RequestMessage(MessageTypes.Lobby));

            // Test code
             //
            lobbyScreen.Intro = "Eyy welcome";
            lobbyScreen.Description = "Woazers! You're in our server!";
            lobbyScreen.Name = "Example Server";
            lobbyScreen.Rooms.Add(new LobbySaveData("Test", 1337,
                "Check out this amazing level. It will blow your butt off. Neat!", 10, 9001, 5));
            LoadRooms();
             //
            // Test code

        }
        /// <summary>
        /// Joins a world
        /// </summary>
        public void JoinRoom(int index)
        {
            if (index >= 0)
            {
                //MainWindow.ScreenManager.SwitchScreen(new Screen(new Action((new Action(() =>
                //{
                   // screen.Client.Network.Send(new Bricklayer.Common.Networking.Messages.JoinRoomMessage(
                   //     (RoomListCtrl.Items[index] as LobbyDataControl).Data.ID));
               // })))));
            }
        }
        /// <summary>
        /// Loads rooms from the recieved lobby message
        /// </summary>
        public void LoadRooms()
        {
            //Set text with what has been recieved
            grpServer.Text = "Server [" + lobbyScreen.Client.Network.NetClient.ServerConnection.RemoteEndPoint.ToString() + "]";
            lblName.Text = lobbyScreen.Name;
            lblDescription.Text = lobbyScreen.Description;
            lblDescription.Height = (int)MainWindow.DefaultSpriteFont.MeasureRichString(lblDescription.Text, Manager).Y;

            lblInfo.Top = 16 + lblDescription.Bottom;
            lblInfo.Text = ReplaceVariables(lobbyScreen.Intro);
            lblInfo.Height = (int)MainWindow.DefaultSpriteFont.MeasureRichString(lblInfo.Text, Manager).Y;

            RefreshRooms();
        }

        private void RefreshRooms()
        {
            LobbyScreen screen = MainWindow.ScreenManager.Current as LobbyScreen;
            RoomListCtrl.Items.Clear();
            foreach (LobbySaveData s in screen.Rooms)
                RoomListCtrl.Items.Add(new LobbyDataControl(Manager, s));
            FilterRooms();
        }

        private void FilterRooms()
        {
            //Filter search results
            if (txtSearch.Text != searchStr && !string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                for (int i = 0; i < RoomListCtrl.Items.Count; i++)
                {
                    if (!(RoomListCtrl.Items[i] as LobbyDataControl).Data.Name.ToLower().Contains(txtSearch.Text.ToLower()))
                    {
                        RoomListCtrl.Items.RemoveAt(i);
                        i--;
                    }
                }
            }
            //Filter by category
            if (cmbSort.ItemIndex == 0) //Online
                RoomListCtrl.Items = RoomListCtrl.Items.OrderByDescending(x => (x as LobbyDataControl).Data.Online).ToList();
            //RoomListCtrl.Items.Sort((x, y) => (x as LobbyDataControl).Data.Players.CompareTo((y as LobbyDataControl).Data.Players));
            else if (cmbSort.ItemIndex == 1)
                RoomListCtrl.Items = RoomListCtrl.Items.OrderByDescending(x => (x as LobbyDataControl).Data.Rating).ToList();
            else if (cmbSort.ItemIndex == 2)
                RoomListCtrl.Items = RoomListCtrl.Items.OrderByDescending(x => (x as LobbyDataControl).Data.Plays).ToList();
            else if (cmbSort.ItemIndex == 3)
                RoomListCtrl.Items.Shuffle();
        }
        /// <summary>
        /// Replaces variables like "$Online" with text in the server info
        /// </summary>
        private string ReplaceVariables(string infoText)
        {
            //Replace variables in the info text
            infoText = infoText.Replace("$Online", lobbyScreen.Online.ToString());
            infoText = infoText.Replace("$Name", lobbyScreen.Name);
            infoText = infoText.Replace("$Rooms", lobbyScreen.Rooms.Count.ToString());
            return infoText;
        }
    }
}
