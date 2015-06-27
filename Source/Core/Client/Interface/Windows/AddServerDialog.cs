using System;
using Bricklayer.Core.Common;
using Bricklayer.Core.Server.Data;
using MonoForce.Controls;
using EventArgs = MonoForce.Controls.EventArgs;

namespace Bricklayer.Core.Client.Interface.Windows
{
    /// <summary>
    /// Dialog for adding/editing servers on the server list
    /// </summary>
    internal sealed class AddServerDialog : Dialog
    {
        private readonly Button btnSave;

        /// <summary>
        /// True if editing a server (as opposed to adding a new one)
        /// </summary>
        private readonly bool edit;

        /// <summary>
        /// Index of the server in the list
        /// </summary>
        private readonly int index;

        private readonly Label lblHost;
        private readonly Label lblName;
        private readonly char[] separator = {':'};
        private readonly TextBox txtHost;
        private readonly TextBox txtName;
        private readonly ServerWindow wndServer;
        private string host;
        private int port;

        public AddServerDialog(Manager manager, ServerWindow parent, int index, bool edit, string name, string host,
            int port)
            : base(manager)
        {
            //Are we editing a server or adding one (They use same dialog)
            this.edit = edit;
            this.index = index;
            wndServer = parent;

            //Setup the window
            Text = this.edit ? "Edit Server" : "Add Server";
            TopPanel.Visible = false;
            Resizable = false;
            Width = 250;
            Height = 180;
            Center();

            //Add controls
            lblName = new Label(manager) {Left = 8, Top = 8, Text = "Name:", Width = ClientWidth - 16};
            lblName.Init();
            Add(lblName);

            txtName = new TextBox(manager) {Left = 8, Top = lblName.Bottom + 4, Width = ClientWidth - 16};
            txtName.Init();
            txtName.Text = name;
            txtName.TextChanged += TxtNameTextChanged;
            Add(txtName);

            lblHost = new Label(manager)
            {
                Left = 8,
                Top = txtName.Bottom + 8,
                Text = $"Address: (Default port is {Globals.Values.DefaultServerPort})",
                Width = ClientWidth - 16
            };
            lblHost.Init();
            Add(lblHost);

            txtHost = new TextBox(manager) {Left = 8, Top = lblHost.Bottom + 4, Width = ClientWidth - 16};
            txtHost.Init();
            txtHost.Text = port != 0 && port != Globals.Values.DefaultServerPort ? $"{host}:{port}" : host;
            txtHost.TextChanged += TxtHostTextChanged;
            Add(txtHost);

            btnSave = new Button(manager) {Top = 8, Text = this.edit ? "Save" : "Add"};
            btnSave.Init();
            btnSave.Left = (Width / 2) - (btnSave.Width / 2);
            btnSave.Click += BtnSaveClick;
            BottomPanel.Add(btnSave);

            if (this.edit)
                Validate(); //Validate existing text
        }

        /// <summary>
        /// When the save button is clicked, add the server or edit the existing one
        /// </summary>
        private void BtnSaveClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text) || string.IsNullOrEmpty(txtHost.Text))
                return;
            if (edit)
                wndServer.EditServer(index, new ServerSaveData(txtName.Text, host, port));
            else
                wndServer.AddServer(new ServerSaveData(txtName.Text, host, port));
            Close();
        }

        private void TxtHostTextChanged(object sender, EventArgs e) => Validate();

        private void TxtNameTextChanged(object sender, EventArgs e) => Validate();

        private void Validate()
        {
            var address = txtHost.Text.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            port = 0;
            if (address.Length > 0)
                host = address[0];
            if (address.Length > 1)
                int.TryParse(address[1], out port);
        }
    }
}