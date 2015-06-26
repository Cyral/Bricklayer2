using System.Collections.Generic;
using Bricklayer.Client.Interface;
using Bricklayer.Core.Client.Interface.Controls;
using Bricklayer.Core.Server.Data;
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
        private List<ServerSaveData> servers; //The list of loaded servers

        public ServerWindow(Manager manager) : base(manager)
        {
            //Setup the window
            CaptionVisible = false;
            TopPanel.Visible = false;
            Movable = false;
            Resizable = false;
            Width = 450;
            Height = 280;
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

            //Add controls to the bottom panel. (Add server, edit server, etc.)
            btnJoin = new Button(manager) {Text = "Connect", Left = 24, Top = 8, Width = 100};
            btnJoin.Init();
            btnJoin.Click += delegate { };
            BottomPanel.Add(btnJoin);

            btnAdd = new Button(manager) {Text = "Add", Left = btnJoin.Right + 8, Top = 8, Width = 64};
            btnAdd.Init();
            btnAdd.Click += delegate
            {
                //Show add server dialog
                var window = new AddServerDialog(manager, this, lstServers.ItemIndex, false, string.Empty,
                    string.Empty);
                window.Init();
                Manager.Add(window);
                window.Show();
            };
            BottomPanel.Add(btnAdd);

            btnEdit = new Button(manager) {Text = "Edit", Left = btnAdd.Right + 8, Top = 8, Width = 64};
            btnEdit.Init();
            btnEdit.Click += delegate
            {
                if (lstServers.Items.Count > 0)
                {
                    var window = new AddServerDialog(manager, this, lstServers.ItemIndex, true,
                        servers[lstServers.ItemIndex].Name, servers[lstServers.ItemIndex].Host);
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
                            IO.WriteServers(servers); //Write the new server list to disk.
                        }
                    };
                    Manager.Add(confirm);
                    confirm.Show();
                }
            };
            BottomPanel.Add(btnRemove);

            btnRefresh = new Button(manager) {Text = "Refresh", Left = btnRemove.Right + 8, Top = 8, Width = 64};
            btnRefresh.Init();
            btnRefresh.Click += delegate { RefreshServerList(); };
            BottomPanel.Add(btnRefresh);

            MainWindow.ScreenManager.FadeIn();
        }

        public void AddServer(ServerSaveData server)
        {
            servers.Add(server);
            IO.WriteServers(servers);
            RefreshServerList();
        }

        public void EditServer(int index, ServerSaveData server)
        {
            servers[index] = server;
            IO.WriteServers(servers);
            RefreshServerList();
        }

        internal void Disconnected(string message, string title)
        {
            //Enable join button and display error message if couldn't connect to server
            if (!btnJoin.Enabled)
            {
                btnJoin.Enabled = true;
                btnJoin.Text = "Connect";
                var error = new MessageBox(Manager, MessageBoxType.Error, message, title);
                error.Init();
                Manager.Add(error);
                error.Show();
            }
        }

        private void RefreshServerList()
        {
            lstServers.Items.Clear();

            //Read the servers from config
            servers = IO.ReadServers();

            //Populate the list 
            foreach (var server in servers)
                lstServers.Items.Add(new ServerDataControl(Manager, server));
            if (lstServers.Items.Count > 0)
                lstServers.ItemIndex = 0;
        }
    }
}