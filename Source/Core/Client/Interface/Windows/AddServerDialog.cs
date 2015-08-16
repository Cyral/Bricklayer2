using System;
using Bricklayer.Core.Common;
using Bricklayer.Core.Common.Data;
using MonoForce.Controls;
using EventArgs = MonoForce.Controls.EventArgs;

namespace Bricklayer.Core.Client.Interface.Windows
{
    /// <summary>
    /// Dialog for adding/editing servers on the server list
    /// </summary>
    internal sealed class AddServerDialog : Dialog
    {
        /// <summary>
        /// Button to save or add the server.
        /// </summary>
        public Button BtnSave { get; }

        /// <summary>
        /// True if editing a server (as opposed to adding a new one).
        /// </summary>
        public bool IsEditing { get; }

        /// <summary>
        /// Index of the server in the list.
        /// </summary>
        public int Index { get; }

        public Label LblHost { get; }
        public Label LblName { get; }

        /// <summary>
        /// Textbox for hostname and port.
        /// </summary>
        public TextBox TxtHost { get; }

        /// <summary>
        /// Text box for user defined server name.
        /// </summary>
        public TextBox TxtName { get; }

        private readonly char[] separator = {':'};
        private readonly ServerWindow wndServer;
        private string host;
        private int port;

        public AddServerDialog(Manager manager, ServerWindow parent, int index, bool isEditing, string name, string host,
            int port)
            : base(manager)
        {
            // Are we editing a server or adding one? (The same dialog is used)
            IsEditing = isEditing;
            Index = index;
            wndServer = parent;

            // Setup the window.
            Text = IsEditing ? "Edit Server" : "Add Server";
            TopPanel.Visible = false;
            Resizable = false;
            Width = 250;
            Height = 180;
            Center();

            // Add controls.
            LblName = new Label(manager) {Left = 8, Top = 8, Text = "Name:", Width = ClientWidth - 16};
            LblName.Init();
            Add(LblName);

            TxtName = new TextBox(manager) {Left = 8, Top = LblName.Bottom + 4, Width = ClientWidth - 16};
            TxtName.Init();
            TxtName.Text = name;
            TxtName.TextChanged += TxtNameTextChanged;
            Add(TxtName);

            LblHost = new Label(manager)
            {
                Left = 8,
                Top = TxtName.Bottom + 8,
                Text = $"Address: (Default port is {Globals.Values.DefaultServerPort})",
                Width = ClientWidth - 16
            };
            LblHost.Init();
            Add(LblHost);

            TxtHost = new TextBox(manager) {Left = 8, Top = LblHost.Bottom + 4, Width = ClientWidth - 16};
            TxtHost.Init();
            TxtHost.Text = port != 0 && port != Globals.Values.DefaultServerPort ? $"{host}:{port}" : host;
            TxtHost.TextChanged += TxtHostTextChanged;
            Add(TxtHost);

            BtnSave = new Button(manager) {Top = 8, Text = IsEditing ? "Save" : "Add"};
            BtnSave.Init();
            BtnSave.Left = (Width/2) - (BtnSave.Width/2);
            BtnSave.Click += BtnSaveClick;
            BottomPanel.Add(BtnSave);

            if (IsEditing)
                Validate(); // Validate existing text.
        }

        /// <summary>
        /// When the save button is clicked, add the server or edit the existing one.
        /// </summary>
        private void BtnSaveClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TxtName.Text) || string.IsNullOrEmpty(TxtHost.Text))
                return;
            if (IsEditing)
                wndServer.EditServer(Index, new ServerData(TxtName.Text, host, port));
            else
                wndServer.AddServer(new ServerData(TxtName.Text, host, port));
            Close();
        }

        private void TxtHostTextChanged(object sender, EventArgs e) => Validate();
        private void TxtNameTextChanged(object sender, EventArgs e) => Validate();

        private void Validate()
        {
            var address = TxtHost.Text.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            port = 0;
            if (address.Length > 0)
                host = address[0];
            if (address.Length > 1)
                int.TryParse(address[1], out port);
        }
    }
}