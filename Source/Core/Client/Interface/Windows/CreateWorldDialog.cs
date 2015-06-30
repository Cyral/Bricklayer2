using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Client.Interface;
using Bricklayer.Core.Client.Interface.Screens;
using Bricklayer.Core.Common.Net.Messages;
using Microsoft.Xna.Framework;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Windows
{
    /// <summary>
    /// Dialog for creating rooms in the lobby list
    /// </summary>
    public class CreateWorldDialog : Dialog
    {
        //Controls
        private Button createBtn;
        private TextBox txtName, txtDescription;
        private Label lblName, lblDescription;
        private LobbyWindow roomList;

        public CreateWorldDialog(Manager manager, LobbyWindow parent)
            : base(manager)
        {
            roomList = parent;
            //Setup the window
            Text = "Create World";
            TopPanel.Visible = false;
            Resizable = false;
            Width = 250;
            Height = 190;
            Center();

            //Add controls
            lblName = new Label(manager) { Left = 8, Top = 8, Text = "Name:", Width = this.ClientWidth - 16 };
            lblName.Init();
            Add(lblName);

            txtName = new TextBox(manager) { Left = 8, Top = lblName.Bottom + 4, Width = this.ClientWidth - 16 };
            txtName.Init();
            txtName.TextChanged += new MonoForce.Controls.EventHandler(delegate (object o, MonoForce.Controls.EventArgs e)
            {
                if (txtName.Text.Length > CreateRoomMessage.MaxNameLength)
                    txtName.Text = txtName.Text.Truncate(CreateRoomMessage.MaxNameLength);
            });
            Add(txtName);

            lblDescription = new Label(manager) { Left = 8, Top = txtName.Bottom + 4, Text = "Description:", Width = this.ClientWidth - 16 };
            lblDescription.Init();
            Add(lblDescription);

            txtDescription = new TextBox(manager) { Left = 8, Top = lblDescription.Bottom + 4, Width = this.ClientWidth - 16, Height = 34, Mode = TextBoxMode.Multiline, ScrollBars = ScrollBars.None };
            txtDescription.Init();
            txtDescription.TextChanged += new MonoForce.Controls.EventHandler(delegate (object o, MonoForce.Controls.EventArgs e)
            {
                //Filter the text by checking for length and lines
                if (txtDescription.Text.Length > CreateRoomMessage.MaxDescriptionLength)
                    txtDescription.Text = txtDescription.Text.Truncate(CreateRoomMessage.MaxDescriptionLength);
                int newLines = txtDescription.Text.Count(c => c == '\n');
                if (newLines >= CreateRoomMessage.MaxDescriptionLines)
                {
                    txtDescription.Text = txtDescription.Text.Substring(0, txtDescription.Text.Length - 1);
                    txtDescription.CursorPosition = 0;
                }
            });
            Add(txtDescription);

            createBtn = new Button(manager) { Top = 8, Text = "Create" };
            createBtn.Init();
            createBtn.Left = (Width / 2) - (createBtn.Width / 2);
            createBtn.Click += CreateBtn_Click;
            BottomPanel.Add(createBtn);
        }
        /// <summary>
        /// When the create button is clicked
        /// </summary>
        void CreateBtn_Click(object sender, MonoForce.Controls.EventArgs e)
        {
            //                Game.NetManager.SendMessage(new Bricklayer.Client.Networking.Messages.JoinRoomMessage(worldName));
            //MainWindow.ScreenManager.SwitchScreen(new Screen(new Action((new Action(() => {
            //    roomList.lobbyScreen.Client.Network.Send(new CreateRoomMessage(txtName.Text, txtDescription.Text));
            //})))));
            Close();
        }
    }
}
