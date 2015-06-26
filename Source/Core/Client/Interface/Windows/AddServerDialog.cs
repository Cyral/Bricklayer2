using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Client.Interface;
using Bricklayer.Core.Client.Interface.Screens;
using Bricklayer.Core.Common;
using Bricklayer.Core.Server.Data;
using Microsoft.Xna.Framework;
using MonoForce.Controls;
using EventArgs = System.EventArgs;

namespace Bricklayer.Core.Client.Interface.Windows
{
    /// <summary>
    /// Dialog for adding/editing servers on the server list
    /// </summary>
     internal sealed class AddServerDialog : Dialog
    {
        private bool Edit; //Edit OR Add a server?
        private int Index; //Index of the server in the list

        //Controls
        private Button SaveBtn;
        private TextBox NameTxt, AddressTxt;
        private Label NameLbl, AddressLbl;
        private ServerWindow ServerList;
        private string IP;
        private int port;
        private char[] separator = new char[1] { ':' };

        public AddServerDialog(Manager manager, ServerWindow parent, int index, bool edit, string name, string address)
            : base(manager)
        {
            //Are we editing a server or adding one (They use same dialog)
            Edit = edit;
            Index = index;
            ServerList = parent;
            //Setup the window
            Text = Edit ? "Edit Server" : "Add Server";
            TopPanel.Visible = false;
            Resizable = false;
            Width = 250;
            Height = 180;
            Center();

            //Add controls
            NameLbl = new Label(manager) { Left = 8, Top = 8, Text = "Name:", Width = this.ClientWidth - 16 };
            NameLbl.Init();
            Add(NameLbl);

            NameTxt = new TextBox(manager) { Left = 8, Top = NameLbl.Bottom + 4, Width = this.ClientWidth - 16 };
            NameTxt.Init();
            NameTxt.Text = name;
            NameTxt.TextChanged += NameTxt_TextChanged;
            Add(NameTxt);

            AddressLbl = new Label(manager)
            {
                Left = 8,
                Top = NameTxt.Bottom + 8,
                Text = string.Format("Address: (Default port is {0})", Globals.Values.DefaultServerPort),
                Width = this.ClientWidth - 16
            };
            AddressLbl.Init();
            Add(AddressLbl);

            AddressTxt = new TextBox(manager) { Left = 8, Top = AddressLbl.Bottom + 4, Width = this.ClientWidth - 16 };
            AddressTxt.Init();
            AddressTxt.Text = address;
            AddressTxt.TextChanged += AddressTxt_TextChanged;
            Add(AddressTxt);

            SaveBtn = new Button(manager) { Top = 8, Text = Edit ? "Save" : "Add", };
            SaveBtn.Init();
            SaveBtn.Left = (Width / 2) - (SaveBtn.Width / 2);
            SaveBtn.Click += SaveBtn_Click;
            SaveBtn.Enabled = false;
            BottomPanel.Add(SaveBtn);

            if (Edit)
                Validate(); //Validate existing text
        }

        void NameTxt_TextChanged(object sender, MonoForce.Controls.EventArgs e)
        {
            Validate();
        }

        private void AddressTxt_TextChanged(object sender, MonoForce.Controls.EventArgs e)
        {
            Validate();
        }

        private void Validate()
        {
            //Validate name
            NameTxt.TextColor = MainWindow.DefaultTextColor;
            AddressTxt.TextColor = MainWindow.DefaultTextColor;
            SaveBtn.Enabled = true;

            if (string.IsNullOrWhiteSpace(NameTxt.Text))
            {
                NameTxt.TextColor = Color.Red;
                SaveBtn.Enabled = false;
            }
            string[] address = AddressTxt.Text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            port = 0;
            if (address.Length > 0)
            {
                IP = address[0];
                //Validate IP
                IPAddress host;
                if (!IPAddress.TryParse(IP, out host) && !IP.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                {
                    AddressTxt.TextColor = Color.Red;
                    SaveBtn.Enabled = false;
                }
                //Validate port (if any)
                if (address.Length > 1 && int.TryParse(address[1], out port))
                {
                    if (!(port > 0 && port < 65535))
                    {
                        AddressTxt.TextColor = Color.Red;
                        SaveBtn.Enabled = false;
                    }

                }
            }
            else
            {
                AddressTxt.TextColor = Color.Red;
                SaveBtn.Enabled = false;
            }
        }

        /// <summary>
        /// When the save button is clicked
        /// </summary>
        private void SaveBtn_Click(object sender, MonoForce.Controls.EventArgs e)
        {
            if (Edit)
                ServerList.EditServer(Index, new ServerSaveData(NameTxt.Text, IP, port));
            else
                ServerList.AddServer(new ServerSaveData(NameTxt.Text, IP, port));
            Close();
        }
    }
}
