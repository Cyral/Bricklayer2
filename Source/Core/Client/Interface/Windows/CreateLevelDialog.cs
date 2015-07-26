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
        // Controls
        private readonly Button createBtn;
        private readonly Label lblDescription;
        private readonly Label lblName;
        private readonly LobbyScreen screen;
        private readonly TextBox txtDescription;
        private readonly TextBox txtName;

        public CreateLevelDialog(Manager manager, LobbyScreen screen) : base(manager)
        {
            this.screen = screen;
            // Setup the window
            Text = "Create Level";
            TopPanel.Visible = false;
            Resizable = false;
            Width = 250;
            Height = 196;
            Center();

            // Add controls
            lblName = new Label(manager) { Left = 8, Top = 8, Text = "Name:", Width = ClientWidth - 16 };
            lblName.Init();
            Add(lblName);

            txtName = new TextBox(manager) { Left = 8, Top = lblName.Bottom + 4, Width = ClientWidth - 16 };
            txtName.Init();
            txtName.TextChanged += (sender, args) =>
            {
                // If you're trying to fix this function, please check out line 77 first.
                if (txtName.Text.Length > CreateLevelMessage.MaxNameLength)
                    txtName.Text = txtName.Text.Truncate(CreateLevelMessage.MaxNameLength);

                createBtn.Enabled = !string.IsNullOrWhiteSpace(txtName.Text);

                // Makes sure aspects like text selection rendering gets covered by MonoForce
                args.Handled = false;
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
                Height = 40,
                Mode = TextBoxMode.Multiline,
                ScrollBars = ScrollBars.None
            };
            txtDescription.Init();
            txtDescription.TextChanged += (sender, args) =>
            {
                // Dear maintainer:
                //
                // If you've come here to avoid the truncation and instead
                // stop the player from typing those characters on the first place
                // then you're reading the right comment, because I already tried it!
                //
                // Here is why it doesn't work:
                // * NeoForce just straight ignore args.Handled on this function;
                // * KeyDown works however if you hold down a key it just ignores your code;
                // * KeyUp is useless, as is KeyPress;
                //
                // Once you are done trying to 'fix' this routine, and have realized
                // what a terrible mistake that was, please increment the following
                // counter as a warning to the next guy:
                //
                // total_hours_wasted_here = 4

                if (txtDescription.Text.Length > CreateLevelMessage.MaxDescriptionLength)
                    txtDescription.Text = txtDescription.Text.Truncate(CreateLevelMessage.MaxDescriptionLength);

                var newLines = txtDescription.Text.Count(c => c == '\n') + 1; // There is always one line!
                if (newLines > CreateLevelMessage.MaxDescriptionLines)
                {
                    txtDescription.Text = string.Join("\n",
                        txtDescription.Text.Split('\n'), 0, CreateLevelMessage.MaxDescriptionLines);
                    txtDescription.CursorPosition = txtDescription.Text.Length - 1;
                }

                // Makes sure aspects like text selection rendering gets covered by MonoForce
                // Works everytime, 50% of the time.
                args.Handled = false;
            };
            Add(txtDescription);

            createBtn = new Button(manager) { Top = 8, Text = "Create", Enabled = false };
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