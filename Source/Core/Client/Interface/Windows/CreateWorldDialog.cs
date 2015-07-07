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
    /// Dialog for creating levels in the lobby list
    /// </summary>
    public class CreateWorldDialog : Dialog
    {
        //Controls
        private Button createBtn;
        private TextBox txtName, txtDescription;
        private Label lblName, lblDescription;
        private LobbyWindow levelList;

        public CreateWorldDialog(Manager manager, LobbyWindow parent)
            : base(manager)
        {
            levelList = parent;
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
            txtName.TextChanged += delegate (object o, MonoForce.Controls.EventArgs e)
            {
                if (txtName.Text.Length > CreateLevelMessage.MaxNameLength)
                    txtName.Text = txtName.Text.Truncate(CreateLevelMessage.MaxNameLength);
                e.Handled = false; // Makes sure aspects like text selection rendering gets covered by NeoForce
            };
            Add(txtName);

            lblDescription = new Label(manager) { Left = 8, Top = txtName.Bottom + 4, Text = "Description:", Width = this.ClientWidth - 16 };
            lblDescription.Init();
            Add(lblDescription);

            txtDescription = new TextBox(manager) { Left = 8, Top = lblDescription.Bottom + 4, Width = this.ClientWidth - 16, Height = 34, Mode = TextBoxMode.Multiline, ScrollBars = ScrollBars.None };
            txtDescription.Init();
            txtDescription.TextChanged += delegate (object o, MonoForce.Controls.EventArgs e)
            {
                //Filter the text by checking for length and lines
                if (txtDescription.Text.Length > CreateLevelMessage.MaxDescriptionLength)
                    txtDescription.Text = txtDescription.Text.Truncate(CreateLevelMessage.MaxDescriptionLength);
                int newLines = txtDescription.Text.Count(c => c == '\n');
                if (newLines >= CreateLevelMessage.MaxDescriptionLines)
                {
                    txtDescription.Text = txtDescription.Text.Substring(0, txtDescription.Text.Length - 1);
                    txtDescription.CursorPosition = 0;
                }
                e.Handled = false; // Makes sure aspects like text selection rendering gets covered by NeoForce
            };
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
            //                Game.NetManager.SendMessage(new Bricklayer.Client.Networking.Messages.JoinLevelMessage(worldName));
            //MainWindow.ScreenManager.SwitchScreen(new Screen(new Action((new Action(() => {
            //    levelList.lobbyScreen.Client.Network.Send(new CreateLevelMessage(txtName.Text, txtDescription.Text));
            //})))));
            Close();
        }
    }
}
