using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Bricklayer.Client.Interface;
using Bricklayer.Core.Client.Components;
using Bricklayer.Core.Client.Interface.Controls;
using Bricklayer.Core.Client.Interface.Screens;
using Bricklayer.Core.Common;
using Bricklayer.Core.Common.Data;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Windows
{
    /// <summary>
    /// The second window shown in Bricklayer, listing the servers and allowing the user to conenct to them.
    /// </summary>
    internal sealed class ServerWindow : Dialog
    {
        private readonly Button btnAdd;
        private readonly Button btnEdit;
        private readonly Button btnJoin;
        private readonly Button btnRefresh;
        private readonly Button btnRemove;
        private readonly ControlList<ServerDataControl> lstServers; //The listbox of server control items
        private List<ServerData> servers; //The list of loaded servers
        private readonly ServerScreen screen;

        public ServerWindow(Manager manager, ServerScreen screen) : base(manager)
        {

            this.screen = screen;
            //Setup the window
            CaptionVisible = false;
            TopPanel.Visible = false;
            Movable = false;
            Resizable = false;
            Width = 550;
            Height = 500;
            Shadow = true;
            Center();

            //Create main server list
            lstServers = new ControlList<ServerDataControl>(manager)
            {
                Left = 8,
                Top = 8,
                Width = ClientWidth - 16,
                Height = ClientHeight - BottomPanel.Height - 16,
            };
            lstServers.Init();
            Add(lstServers);
            RefreshServerList();
            lstServers.DoubleClick += delegate (object o, EventArgs e)
            {
                var sdc = (ServerDataControl)lstServers.Items[lstServers.ItemIndex];
                //Make sure the user clicks the item and not the empty space in the list
                if (sdc.CheckPositionMouse(((MouseEventArgs)e).Position - lstServers.AbsoluteRect.Location))
                    screen.Client.Network.SendSessionRequest(servers[lstServers.ItemIndex].Host, servers[lstServers.ItemIndex].Port);
            };

            //Add controls to the bottom panel. (Add server, edit server, etc.)

            btnEdit = new Button(manager) {Text = "Edit", Top = 8, Width = 64};
            btnEdit.Init();
            btnEdit.Left = (Width / 2) - (btnEdit.Width / 2);
            btnEdit.Click += delegate
            {
                if (lstServers.Items.Count > 0)
                {
                    var window = new AddServerDialog(manager, this, lstServers.ItemIndex, true,
                        servers[lstServers.ItemIndex].Name, servers[lstServers.ItemIndex].Host, servers[lstServers.ItemIndex].Port);
                    window.Init();
                    Manager.Add(window);
                    window.Show();
                }
            };
            BottomPanel.Add(btnEdit);

            btnRemove = new Button(manager) {Text = "Remove", Left = btnEdit.Right + 8, Top = 8, Width = 64};
            btnRemove.Init();
            btnRemove.Click += delegate
            {
                //Show a messagebox that asks for confirmation to delete the selected server.
                if (lstServers.Items.Count > 0)
                {
                    var confirm = new MessageBox(manager, MessageBoxType.YesNo,
                        "Are you sure you would like to remove\nthis server from your server list?",
                        "Confirm Removal");
                    confirm.Init();
                    //When the message box is closed, check if the user clicked yes, if so, removed the server from the list.
                    confirm.Closed += delegate(object sender, WindowClosedEventArgs args)
                    {
                        var dialog = sender as Dialog;
                        if (dialog != null && dialog.ModalResult == ModalResult.Yes) //If user clicked yes
                        {
                            servers.RemoveAt(lstServers.ItemIndex);
                            lstServers.Items.RemoveAt(lstServers.ItemIndex);
                            screen.Client.IO.WriteServers(servers); //Write the new server list to disk.
                        }
                    };
                    Manager.Add(confirm);
                    confirm.Show();
                }
            };
            BottomPanel.Add(btnRemove);

            btnAdd = new Button(manager) { Text = "Add", Top = 8, Width = 64 };
            btnAdd.Init();
            btnAdd.Right = btnEdit.Left - 8;
            btnAdd.Click += delegate
            {
                //Show add server dialog
                var window = new AddServerDialog(manager, this, lstServers.ItemIndex, false, string.Empty,
                    string.Empty, Globals.Values.DefaultServerPort);
                window.Init();
                Manager.Add(window);
                window.Show();
            };
            BottomPanel.Add(btnAdd);

            btnJoin = new Button(manager) { Text = "Connect", Top = 8, Width = 100 };
            btnJoin.Init();
            btnJoin.Right = btnAdd.Left - 8;
            btnJoin.Click += delegate
            {
                btnJoin.Enabled = false;
                btnJoin.Text = "Connecting...";
                screen.Client.Network.SendSessionRequest(servers[lstServers.ItemIndex].Host, servers[lstServers.ItemIndex].Port);
            };
            BottomPanel.Add(btnJoin);

            btnRefresh = new Button(manager) {Text = "Refresh", Left = btnRemove.Right + 8, Top = 8, Width = 64};
            btnRefresh.Init();
            btnRefresh.Click += delegate { RefreshServerList(); };
            BottomPanel.Add(btnRefresh);

            // Listen for when init message is recieved
            screen.Client.Events.Network.Game.Init.AddHandler(OnInit);

            // If user was disconnected from the server
            screen.Client.Events.Network.Game.Disconnect.AddHandler(OnDisconnect);
        }

        private void OnInit(EventManager.NetEvents.GameServerEvents.InitEventArgs args)
        {
            screen.ScreenManager.SwitchScreen(new LobbyScreen(args.Message.Description, args.Message.ServerName,
                args.Message.Intro, args.Message.Online, args.Message.Levels));
        }

        private void OnDisconnect(EventManager.NetEvents.GameServerEvents.DisconnectEventArgs args)
        {
            btnJoin.Enabled = true;
            btnJoin.Text = "Connect";
            var msgBox = new MessageBox(Manager, MessageBoxType.Warning, args.Reason, "Error Connecting to Server");
            msgBox.Init();
            screen.ScreenManager.Manager.Add(msgBox);
            msgBox.ShowModal();
        }

        protected override void Dispose(bool disposing)
        {
            screen.Client.Events.Network.Game.Init.RemoveHandler(OnInit);
            screen.Client.Events.Network.Game.Disconnect.RemoveHandler(OnDisconnect);
            base.Dispose(disposing);
        }

        public void AddServer(ServerData server)
        {
            servers.Add(server);
            screen.Client.IO.WriteServers(servers);
            RefreshServerList();
        }

        public void EditServer(int index, ServerData server)
        {
            servers[index] = server;
            screen.Client.IO.WriteServers(servers);
            RefreshServerList();
        }

        private void RefreshServerList()
        {
            lstServers.Items.Clear();
            //Read the servers from config
            servers = screen.Client.IO.ReadServers();
            //Populate the list 
            foreach (var server in servers)
            {
                var control = new ServerDataControl(screen, Manager, server, lstServers);
                lstServers.Items.Add(control);
                control.Init();
                control.PingServer();
            }
            if (lstServers.Items.Count > 0)
                lstServers.ItemIndex = 0;
        }
    }
}
