using Bricklayer.Core.Client.Interface.Screens;
using Bricklayer.Core.Common.Net.Messages;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Windows
{
    /// <summary>
    /// Dialog for creating levels in the lobby list.
    /// </summary>
    public sealed class CreateLevelDialog : Dialog
    {
        // Controls
        public Button CreateBtn { get; }
        public Label LblDescription { get; }
        public Label LblName { get; }

        /// <summary>
        /// Text box for level description.
        /// </summary>
        public TextBox TxtDescription { get; }

        /// <summary>
        /// Textbox for level name.
        /// </summary>
        public TextBox TxtName { get; }

        private readonly LobbyScreen screen;

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
            LblName = new Label(manager) {Left = 8, Top = 8, Text = "Name:", Width = ClientWidth - 16};
            LblName.Init();
            Add(LblName);

            TxtName = new TextBox(manager) {Left = 8, Top = LblName.Bottom + 4, Width = ClientWidth - 16};
            TxtName.Init();
            TxtName.MaxLength = CreateLevelMessage.MaxNameLength;
            TxtName.Text = ""; // Fix bug with cursor caret not showing.
            TxtName.TextChanged += (sender, args) => { CreateBtn.Enabled = !string.IsNullOrWhiteSpace(TxtName.Text); };
            Add(TxtName);

            LblDescription = new Label(manager)
            {
                Left = 8,
                Top = TxtName.Bottom + 4,
                Text = "Description:",
                Width = ClientWidth - 16
            };
            LblDescription.Init();
            Add(LblDescription);

            TxtDescription = new TextBox(manager)
            {
                Left = 8,
                Top = LblDescription.Bottom + 4,
                Width = ClientWidth - 16,
                Height = 40,
                Mode = TextBoxMode.Multiline,
                ScrollBars = ScrollBars.None
            };
            TxtDescription.Init();
            TxtDescription.MaxLines = CreateLevelMessage.MaxDescriptionLines;
            TxtDescription.MaxLength = CreateLevelMessage.MaxDescriptionLength;
            Add(TxtDescription);

            CreateBtn = new Button(manager) {Top = 8, Text = "Create", Enabled = false};
            CreateBtn.Init();
            CreateBtn.Left = (Width/2) - (CreateBtn.Width/2);
            CreateBtn.Click += CreateBtn_Click;
            BottomPanel.Add(CreateBtn);
        }

        /// <summary>
        /// When the create button is clicked
        /// </summary>
        private void CreateBtn_Click(object sender, EventArgs e)
        {
            screen.Client.Network.Send(new CreateLevelMessage(TxtName.Text, TxtDescription.Text));
            Close();
        }
    }
}