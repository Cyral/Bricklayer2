using Bricklayer.Core.Client.Interface.Screens;
using Microsoft.Xna.Framework;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Windows
{
    /// <summary>
    /// The first window shown in Bricklayer, which allows users to login.
    /// </summary>
    internal sealed class LoginWindow : Dialog
    {
        private TextBox txtUsername, txtPassword;
        private CheckBox chkRemember;
        private Button btnLoginAccount, btnLoginGuest;
        private LinkLabel lnkForgot, lnkCreateAccount;
        private ImageBox BodyImg, SmileyImg;
        private ColorPicker bodyClr;
        private LoginScreen screen;

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
            txtUsername = new TextBox(Manager);
            txtUsername.Init();
            txtUsername.Width = ClientWidth / 2 - 16;
            txtUsername.Left = 8;
            txtUsername.Top = TopPanel.Bottom + 8;
            txtUsername.Placeholder = "Username";
            txtUsername.Text = screen.Client.IO.Config.Client.Username;
            Add(txtUsername);

            txtPassword = new TextBox(Manager);
            txtPassword.Init();
            txtPassword.Width = txtUsername.Width;
            txtPassword.Left = txtUsername.Left;
            txtPassword.Top = txtUsername.Bottom + 8;
            txtPassword.Placeholder = "Password";
            txtPassword.Text = screen.Client.IO.GetPassword();
            txtPassword.Mode = TextBoxMode.Password;
            Add(txtPassword);

            chkRemember = new CheckBox(Manager);
            chkRemember.Init();
            chkRemember.Width = txtUsername.Width;
            chkRemember.Left = txtUsername.Right + 16;
            chkRemember.Top = txtPassword.Top + 3;
            chkRemember.Text = "Remember Me";
            chkRemember.Checked = screen.Client.IO.Config.Client.RememberMe;
            Add(chkRemember);

            // Sign in buttons
            btnLoginAccount = new Button(Manager);
            btnLoginAccount.Init();
            btnLoginAccount.Width = txtUsername.Width;
            btnLoginAccount.Left = txtUsername.Left;
            btnLoginAccount.Top = 8;
            btnLoginAccount.Click += LoginAccountClick;
            btnLoginAccount.Text = "Sign in";
            BottomPanel.Add(btnLoginAccount);

            btnLoginGuest = new Button(Manager);
            btnLoginGuest.Enabled = false;
            btnLoginGuest.Init();
            btnLoginGuest.Width = btnLoginAccount.Width;
            btnLoginGuest.Left = btnLoginAccount.Right + 16;
            btnLoginGuest.Top = btnLoginAccount.Top;
            btnLoginGuest.Text = "Try as Guest";
            BottomPanel.Add(btnLoginGuest);

            // Links
            lnkForgot = new LinkLabel(Manager);
            lnkForgot.Init();
            lnkForgot.Width = txtUsername.Width;
            lnkForgot.Alignment = Alignment.MiddleCenter;
            lnkForgot.Left = txtUsername.Left;
            lnkForgot.Top = btnLoginAccount.Bottom + 8;
            lnkForgot.Text = "Forgot Password?";
            lnkForgot.URL = Constants.Strings.ForgotPasswordURL;
            lnkForgot.ToolTip = new ToolTip(Manager) { Text = "A new browser window will be opened." };
            BottomPanel.Add(lnkForgot);

            lnkCreateAccount = new LinkLabel(Manager);
            lnkCreateAccount.Init();
            lnkCreateAccount.Width = txtUsername.Width;
            lnkCreateAccount.Alignment = Alignment.MiddleCenter;
            lnkCreateAccount.Left = btnLoginGuest.Left;
            lnkCreateAccount.Top = lnkForgot.Top;
            lnkCreateAccount.Text = "Create an account";
            lnkCreateAccount.URL = Constants.Strings.CreateAccountURL;
            BottomPanel.Add(lnkCreateAccount);

            // Color selector
            bodyClr = new ColorPicker(Manager) { Left = txtUsername.Right + 16, Top = txtUsername.Top, Width = txtUsername.Width - 18 - 8, Saturation = 210, Value = 250 };
            bodyClr.Init();
            bodyClr.ValueChanged += (sender, args) =>
            {
                BodyImg.Color = bodyClr.SelectedColor;
                // TODO: Update value in the game class
            };
            Add(bodyClr);

            BodyImg = new ImageBox(Manager) { Left = bodyClr.Right + 8, Top = bodyClr.Top, Width = 18, Height = 18, Image = screen.Client.Content["entity.body"] };
            BodyImg.Init();
            Add(BodyImg);

            SmileyImg = new ImageBox(Manager) { Left = BodyImg.Left, Top = BodyImg.Top, Width = 18, Height = 18, Image = screen.Client.Content["entity.smileys"], SourceRect = new Rectangle(0, 0, 18, 18) };
            SmileyImg.Init();
            SmileyImg.ToolTip.Text = "I love this color!";
            Add(SmileyImg);

            bodyClr.Hue = screen.Client.IO.Config.Client.Color;

            BottomPanel.Height = lnkForgot.Bottom + 28;
            BottomPanel.Top = Height - BottomPanel.Height;

        }

        private void OnFailedLogin(EventManager.NetEvents.AuthServerEvents.FailedLoginEventArgs args)
        {
            ShowError(screen.ScreenManager.Manager, $"Failed to login:\n{args.ErrorMessage}.");
        }

        private async void OnInit(EventManager.NetEvents.AuthServerEvents.InitEventArgs args)
        {
            if (chkRemember.Checked)
                screen.Client.IO.SetPassword(txtPassword.Text);
            screen.Client.IO.Config.Client.Username = chkRemember.Checked ? txtUsername.Text : string.Empty;
            screen.Client.IO.Config.Client.RememberMe = chkRemember.Checked;
            screen.Client.IO.Config.Client.Color = chkRemember.Checked ? bodyClr.Hue : 40;
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
            btnLoginAccount.Text = "Sign In";
            btnLoginAccount.Enabled = true;
        }

        private void LoginAccountClick(object sender, EventArgs eventArgs)
        {
            // Simple validation
            if (!string.IsNullOrEmpty(txtUsername.Text) && !string.IsNullOrEmpty(txtPassword.Text) && txtUsername.Text.Length < 100 && txtPassword.Text.Length < 100)
            {
                // Connect to Auth Server. Tempoary testing method for the auth server. Will be removed
                screen.Client.Network.ConnectToAuth(txtUsername.Text, txtPassword.Text);
                btnLoginAccount.Enabled = btnLoginGuest.Enabled = false;
                btnLoginAccount.Text = "Signing In...";
            }
        }
    }
}