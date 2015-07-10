using System.Linq;
using Bricklayer.Core.Client.Interface.Screens;
using Bricklayer.Core.Common;
using Bricklayer.Core.Common.Net.Messages;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Windows
{
    /// <summary>
    /// Dialog for creating levels in the lobby list
    /// </summary>
    public sealed class CreateLevelDialog : Dialog
    {
        //Controls
        private readonly Button createBtn;
        private readonly Label lblDescription;
        private readonly Label lblName;
        private readonly LobbyScreen screen;
        private readonly TextBox txtDescription;
        private readonly TextBox txtName;

        public CreateLevelDialog(Manager manager, LobbyScreen screen)
            : base(manager)
        {
            this.screen = screen;
            //Setup the window
            Text = "Create Level";
            TopPanel.Visible = false;
            Resizable = false;
            Width = 250;
            Height = 190;
            Center();

            //Add controls
            lblName = new Label(manager) {Left = 8, Top = 8, Text = "Name:", Width = ClientWidth - 16};
            lblName.Init();
            Add(lblName);

            txtName = new TextBox(manager) {Left = 8, Top = lblName.Bottom + 4, Width = ClientWidth - 16};
            txtName.Init();
            txtName.TextChanged += delegate(object o, EventArgs e)
            {
                if (txtName.Text.Length > CreateLevelMessage.MaxNameLength)
                    txtName.Text = txtName.Text.Truncate(CreateLevelMessage.MaxNameLength);
                e.Handled = false; // Makes sure aspects like text selection rendering gets covered by MonoForce
            };
            Add(txtName);

            lblDescription = new Label(manager)
            {
                Left = 8,
                Top = txtName.Bottom + 4,
                Text = "Description:",
                Width = ClientWidth - 16
            };
            lblDescription.Init();
            Add(lblDescription);

            txtDescription = new TextBox(manager)
            {
                Left = 8,
                Top = lblDescription.Bottom + 4,
                Width = ClientWidth - 16,
                Height = 34,
                Mode = TextBoxMode.Multiline,
                ScrollBars = ScrollBars.None
            };
            txtDescription.Init();
            txtDescription.TextChanged += delegate(object o, EventArgs e)
            {
                //Filter the text by checking for length and lines
                if (txtDescription.Text.Length > CreateLevelMessage.MaxDescriptionLength)
                    txtDescription.Text = txtDescription.Text.Truncate(CreateLevelMessage.MaxDescriptionLength);
                var newLines = txtDescription.Text.Count(c => c == '\n');
                if (newLines >= CreateLevelMessage.MaxDescriptionLines)
                {
                    txtDescription.Text = txtDescription.Text.Substring(0, txtDescription.Text.Length - 1);
                    txtDescription.CursorPosition = 0;
                }
                e.Handled = false; // Makes sure aspects like text selection rendering gets covered by MonoForce
            };
            Add(txtDescription);

            createBtn = new Button(manager) {Top = 8, Text = "Create"};
            createBtn.Init();
            createBtn.Left = (Width / 2) - (createBtn.Width / 2);
            createBtn.Click += CreateBtn_Click;
            BottomPanel.Add(createBtn);
        }

        /// <summary>
        /// When the create button is clicked
        /// </summary>
        private void CreateBtn_Click(object sender, EventArgs e)
        {
            screen.Client.Network.Send(new CreateLevelMessage(txtName.Text, txtDescription.Text));
            Close();
        }
    }
}