using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Client.Interface;
using Bricklayer.Core.Client.Interface.Controls;
using Bricklayer.Core.Client.Interface.Screens;
using Bricklayer.Core.Common;
using Bricklayer.Core.Server.Data;
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
        public ControlList<ServerDataControl> ServerListCtrl;
        public List<ServerSaveData> Servers;

        private Button JoinBtn, AddBtn, RemoveBtn, EditBtn, RefreshBtn;
        private TextBox NameTxt;
        private Label NameLbl, ColorLbl;
        private ImageBox BodyImg, SmileyImg;
        private ColorPicker BodyClr;
        private LoginScreen Screen;

        public ServerWindow(Manager manager, LoginScreen screen) : base(manager)
        {
            Screen = screen;
            //Setup the window
            CaptionVisible = false;
            Caption.Text = "Welcome to Bricklayer!";
            Description.Text = "An open source, fully moddable and customizable 2D\nbuilding game built with the community in mind.";
            Movable = false;
            Resizable = false;
            Width = 450;
            Height = 350;
            Shadow = true;
            Center();

            //Create main server list
            ServerListCtrl = new ControlList<ServerDataControl>(manager) { Left = 8, Top = TopPanel.Bottom + 34, Width = ClientWidth - 16, Height = ClientHeight - TopPanel.Height - BottomPanel.Height - 42 };
            ServerListCtrl.Init();
            Add(ServerListCtrl);
            RefreshServerList();

            //Add BottomPanel controls
            JoinBtn = new Button(manager) { Text = "Connect", Left = 24, Top = 8, Width = 100, };
            JoinBtn.Init();
            JoinBtn.Click += new MonoForce.Controls.EventHandler(delegate (object o, MonoForce.Controls.EventArgs e)
            {

            });
            BottomPanel.Add(JoinBtn);

            AddBtn = new Button(manager) { Text = "Add", Left = JoinBtn.Right + 8, Top = 8, Width = 64, };
            AddBtn.Init();
            AddBtn.Click += new MonoForce.Controls.EventHandler(delegate (object o, MonoForce.Controls.EventArgs e)
            {
                AddServerDialog window = new AddServerDialog(manager, this, ServerListCtrl.ItemIndex, false, string.Empty, string.Empty);
                window.Init();
                Manager.Add(window);
                window.Show();
            });
            BottomPanel.Add(AddBtn);

            EditBtn = new Button(manager) { Text = "Edit", Left = AddBtn.Right + 8, Top = 8, Width = 64, };
            EditBtn.Init();
            EditBtn.Click += new MonoForce.Controls.EventHandler(delegate (object o, MonoForce.Controls.EventArgs e)
            {
                if (ServerListCtrl.Items.Count > 0)
                {
                    AddServerDialog window = new AddServerDialog(manager, this, ServerListCtrl.ItemIndex, true, Servers[ServerListCtrl.ItemIndex].Name, Servers[ServerListCtrl.ItemIndex].GetHostString());
                    window.Init();
                    Manager.Add(window);
                    window.Show();
                }
            });
            BottomPanel.Add(EditBtn);

            RemoveBtn = new Button(manager) { Text = "Remove", Left = EditBtn.Right + 8, Top = 8, Width = 64 };
            RemoveBtn.Init();
            RemoveBtn.Click += new MonoForce.Controls.EventHandler(delegate (object o, MonoForce.Controls.EventArgs e)
            {
                if (ServerListCtrl.Items.Count > 0)
                {
                    MessageBox confirm = new MessageBox(manager, MessageBoxType.YesNo, "Are you sure you would like to remove\nthis server from your server list?", "[color:Red]Confirm Removal[/color]");
                    confirm.Init();
                    confirm.Closed += new WindowClosedEventHandler(delegate (object sender, WindowClosedEventArgs args)
                    {
                        if ((sender as Dialog).ModalResult == ModalResult.Yes) //If user clicked yes
                        {
                            Servers.RemoveAt(ServerListCtrl.ItemIndex);
                            ServerListCtrl.Items.RemoveAt(ServerListCtrl.ItemIndex);
                            IO.WriteServers(Servers);
                            RefreshServerList();
                        }
                    });
                    Manager.Add(confirm);
                    confirm.Show();
                }
            });
            BottomPanel.Add(RemoveBtn);

            RefreshBtn = new Button(manager) { Text = "Refresh", Left = RemoveBtn.Right + 8, Top = 8, Width = 64 };
            RefreshBtn.Init();
            RefreshBtn.Click += new MonoForce.Controls.EventHandler(delegate (object o, MonoForce.Controls.EventArgs e)
            {
                RefreshServerList();
            });
            BottomPanel.Add(RefreshBtn);
            MainWindow.ScreenManager.FadeIn();
        }


        public void AddServer(ServerSaveData server)
        {
            Servers.Add(server);
            IO.WriteServers(Servers);
            RefreshServerList();
        }
        public void EditServer(int index, ServerSaveData server)
        {
            Servers[index] = server;
            IO.WriteServers(Servers);
            RefreshServerList();
        }
        private void RefreshServerList()
        {
            ServerListCtrl.Items.Clear();
            //Read the servers from config
            Servers = IO.ReadServers();
            //Populate the list 
            foreach (var server in Servers)
            {
                var control = new ServerDataControl(Screen, Manager, server);
                ServerListCtrl.Items.Add(control);
                control.PingServer();
            }
            if (ServerListCtrl.Items.Count > 0)
                ServerListCtrl.ItemIndex = 0;
        }

        internal void Disconnected(string message, string title)
        {
            //Re-enable join button and display error if couldnt connect
            if (!JoinBtn.Enabled)
            {
                JoinBtn.Enabled = true;
                JoinBtn.Text = "Connect";
                MessageBox error = new MessageBox(Manager, MessageBoxType.Error, message, title);
                error.Init();
                Manager.Add(error);
                error.Show();
            }
        }
    }
}
