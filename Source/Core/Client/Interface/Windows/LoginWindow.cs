using Bricklayer.Core.Client.Interface.Screens;
using Microsoft.Xna.Framework;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Windows
{
    /// <summary>
    /// The first window shown in Bricklayer, which allows users to login.
    /// </summary>
    public sealed class LoginWindow : Dialog
    {
        /// <summary>
        /// Username textbox.
        /// </summary>
        public TextBox TxtUsername { get; }

        //It doesn't matter if this is public, plugins could still use the Controls list to access it.
        /// <summary>
        /// Password textbox.
        /// </summary>
        public TextBox TxtPassword { get; }

        /// <summary>
        /// Remember me checkbox.
        /// </summary>
        public CheckBox ChkRemember { get; }

        /// <summary>
        /// Button to sign in with account details.
        /// </summary>
        public Button BtnLoginAccount { get; }

        /// <summary>
        /// Button to sign in as a guest.
        /// </summary>
        public Button BtnLoginGuest { get; }

        /// <summary>
        /// Link to send password reset email.
        /// </summary>
        public LinkLabel LnkForgot { get; }

        /// <summary>
        /// Link to create new account.
        /// </summary>
        public LinkLabel LnkCreateAccount { get; }

        /// <summary>
        /// Image of player body, tinted to the color.
        /// </summary>
        public ImageBox ImgBody { get; }

        /// <summary>
        /// Image of smiley.
        /// </summary>
        public ImageBox ImgSmiley { get; }

        /// <summary>
        /// Color picker.
        /// </summary>
        public ColorPicker ColorPicker { get; }

        private readonly LoginScreen screen;

        public LoginWindow(Manager manager, LoginScreen screen) : base(manager)
        {
            this.screen = screen;

            // Events

            // Listen for valid response from auth server
            screen.Client.Events.Network.Auth.InitReceived.AddHandler(OnInit);

            // Listen for failed login response from auth server
            screen.Client.Events.Network.Auth.FailedLogin.AddHandler(OnFailedLogin);

            // Setup the window
            CaptionVisible = false;
            Caption.Text = "Welcome to Bricklayer!";
            Description.Text =
                "An open source, fully moddable and customizable 2D\nbuilding game.";
            Movable = false;
            Resizable = false;
            Width = 450;
            Height = 218;
            TopPanel.Height = 72;
            Shadow = true;
            Center();

            // Text boxes
            TxtUsername = new TextBox(Manager);
            TxtUsername.Init();
            TxtUsername.Width = ClientWidth/2 - 16;
            TxtUsername.Left = 8;
            TxtUsername.Top = TopPanel.Bottom + 8;
            TxtUsername.Placeholder = "Username";
            TxtUsername.Text = screen.Client.IO.Config.Client.Username;
            Add(TxtUsername);

            TxtPassword = new TextBox(Manager);
            TxtPassword.Init();
            TxtPassword.Width = TxtUsername.Width;
            TxtPassword.Left = TxtUsername.Left;
            TxtPassword.Top = TxtUsername.Bottom + 8;
            TxtPassword.Placeholder = "Password";
            TxtPassword.Text = screen.Client.IO.GetPassword();
            TxtPassword.Mode = TextBoxMode.Password;
            Add(TxtPassword);

            ChkRemember = new CheckBox(Manager);
            ChkRemember.Init();
            ChkRemember.Width = TxtUsername.Width;
            ChkRemember.Left = TxtUsername.Right + 16;
            ChkRemember.Top = TxtPassword.Top + 3;
            ChkRemember.Text = "Remember Me";
            ChkRemember.Checked = screen.Client.IO.Config.Client.RememberMe;
            Add(ChkRemember);

            // Sign in buttons
            BtnLoginAccount = new Button(Manager);
            BtnLoginAccount.Init();
            BtnLoginAccount.Width = TxtUsername.Width;
            BtnLoginAccount.Left = TxtUsername.Left;
            BtnLoginAccount.Top = 8;
            BtnLoginAccount.Click += LoginAccountClick;
            BtnLoginAccount.Text = "Sign in";
            BottomPanel.Add(BtnLoginAccount);

            BtnLoginGuest = new Button(Manager);
            BtnLoginGuest.Enabled = false;
            BtnLoginGuest.Init();
            BtnLoginGuest.Width = BtnLoginAccount.Width;
            BtnLoginGuest.Left = BtnLoginAccount.Right + 16;
            BtnLoginGuest.Top = BtnLoginAccount.Top;
            BtnLoginGuest.Text = "Try as Guest";
            BottomPanel.Add(BtnLoginGuest);

            // Links
            LnkForgot = new LinkLabel(Manager);
            LnkForgot.Init();
            LnkForgot.Width = TxtUsername.Width;
            LnkForgot.Alignment = Alignment.MiddleCenter;
            LnkForgot.Left = TxtUsername.Left;
            LnkForgot.Top = BtnLoginAccount.Bottom + 8;
            LnkForgot.Text = "Forgot Password?";
            LnkForgot.URL = Constants.Strings.ForgotPasswordURL;
            LnkForgot.ToolTip = new ToolTip(Manager) {Text = "A new browser window will be opened."};
            BottomPanel.Add(LnkForgot);

            LnkCreateAccount = new LinkLabel(Manager);
            LnkCreateAccount.Init();
            LnkCreateAccount.Width = TxtUsername.Width;
            LnkCreateAccount.Alignment = Alignment.MiddleCenter;
            LnkCreateAccount.Left = BtnLoginGuest.Left;
            LnkCreateAccount.Top = LnkForgot.Top;
            LnkCreateAccount.Text = "Create an account";
            LnkCreateAccount.URL = Constants.Strings.CreateAccountURL;
            BottomPanel.Add(LnkCreateAccount);

            // Color selector
            ColorPicker = new ColorPicker(Manager)
            {
                Left = TxtUsername.Right + 16,
                Top = TxtUsername.Top,
                Width = TxtUsername.Width - 18 - 8,
                Saturation = 210,
                Value = 250
            };
            ColorPicker.Init();
            ColorPicker.ValueChanged += (sender, args) =>
            {
                ImgBody.Color = ColorPicker.SelectedColor;
                // TODO: Update value in the game class
            };
            Add(ColorPicker);

            ImgBody = new ImageBox(Manager)
            {
                Left = ColorPicker.Right + 8,
                Top = ColorPicker.Top,
                Width = 18,
                Height = 18,
                Image = screen.Client.Content["entity.body"]
            };
            ImgBody.Init();
            Add(ImgBody);

            ImgSmiley = new ImageBox(Manager)
            {
                Left = ImgBody.Left,
                Top = ImgBody.Top,
                Width = 18,
                Height = 18,
                Image = screen.Client.Content["entity.smileys"],
                SourceRect = new Rectangle(0, 0, 18, 18)
            };
            ImgSmiley.Init();
            ImgSmiley.ToolTip.Text = "I love this color!";
            Add(ImgSmiley);

            ColorPicker.Hue = screen.Client.IO.Config.Client.Color;

            BottomPanel.Height = LnkForgot.Bottom + 28;
            BottomPanel.Top = Height - BottomPanel.Height;
        }

        private void OnFailedLogin(EventManager.NetEvents.AuthServerEvents.FailedLoginEventArgs args)
        {
            ShowError(screen.ScreenManager.Manager, $"Failed to login:\n{args.ErrorMessage}.");
        }

        private async void OnInit(EventManager.NetEvents.AuthServerEvents.InitEventArgs args)
        {
            if (ChkRemember.Checked)
                screen.Client.IO.SetPassword(TxtPassword.Text);
            screen.Client.IO.Config.Client.Username = ChkRemember.Checked ? TxtUsername.Text : string.Empty;
            screen.Client.IO.Config.Client.RememberMe = ChkRemember.Checked;
            screen.Client.IO.Config.Client.Color = ChkRemember.Checked ? ColorPicker.Hue : 40;
            await screen.Client.IO.SaveConfig(screen.Client.IO.Config);
        }

        protected override void Dispose(bool disposing)
        {
            screen.Client.Events.Network.Auth.InitReceived.RemoveHandler(OnInit);
            screen.Client.Events.Network.Auth.FailedLogin.RemoveHandler(OnFailedLogin);
            base.Dispose(disposing);
        }

        private void ShowError(Manager manager, string message)
        {
            var msgBox = new MessageBox(Manager, MessageBoxType.Warning, message, "Error");
            msgBox.Init();
            manager.Add(msgBox);
            msgBox.ShowModal();
            BtnLoginAccount.Text = "Sign In";
            BtnLoginAccount.Enabled = true;
        }

        private void LoginAccountClick(object sender, EventArgs eventArgs)
        {
            // Simple validation
            if (!string.IsNullOrEmpty(TxtUsername.Text) && !string.IsNullOrEmpty(TxtPassword.Text) &&
                TxtUsername.Text.Length < 100 && TxtPassword.Text.Length < 100)
            {
                // Connect to Auth Server. Tempoary testing method for the auth server. Will be removed
                screen.Client.Network.LoginToAuth(TxtUsername.Text, TxtPassword.Text);
                BtnLoginAccount.Enabled = BtnLoginGuest.Enabled = false;
                BtnLoginAccount.Text = "Signing In...";
            }
        }
    }
}