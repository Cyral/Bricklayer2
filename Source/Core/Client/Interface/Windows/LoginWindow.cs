using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private ColorPicker BodyClr;

        public LoginWindow(Manager manager) : base(manager)
        {
            Game.State = GameState.Login;

            //Setup the window
            CaptionVisible = false;
            Caption.Text = "Welcome to Bricklayer!";
            Description.Text =
                "An open source, fully moddable and customizable 2D\nbuilding game built with the community in mind.";
            Movable = false;
            Resizable = false;
            Width = 450;
            Height = 218;
            TopPanel.Height = 72;
            Shadow = true;
            Center();

            //Text boxes
            txtUsername = new TextBox(Manager);
            txtUsername.Init();
            txtUsername.Width = ClientWidth / 2 - 16;
            txtUsername.Left = 8;
            txtUsername.Top = TopPanel.Bottom + 8;
            txtUsername.Placeholder = "Username";
            Add(txtUsername);

            txtPassword = new TextBox(Manager);
            txtPassword.Init();
            txtPassword.Width = txtUsername.Width;
            txtPassword.Left = txtUsername.Left;
            txtPassword.Top = txtUsername.Bottom + 8;
            txtPassword.Placeholder = "Password";
            txtPassword.Mode = TextBoxMode.Password;
            Add(txtPassword);

            chkRemember = new CheckBox(Manager);
            chkRemember.Init();
            chkRemember.Width = txtUsername.Width;
            chkRemember.Left = txtUsername.Right + 16;
            chkRemember.Top = txtPassword.Top + 3;
            chkRemember.Text = "Remember Me";
            Add(chkRemember);

            //Sign in buttons
            btnLoginAccount = new Button(Manager);
            btnLoginAccount.Init();
            btnLoginAccount.Width = txtUsername.Width;
            btnLoginAccount.Left = txtUsername.Left;
            btnLoginAccount.Top = 8;
            btnLoginAccount.Text = "Sign in";
            BottomPanel.Add(btnLoginAccount);

            btnLoginGuest = new Button(Manager);
            btnLoginGuest.Init();
            btnLoginGuest.Width = btnLoginAccount.Width;
            btnLoginGuest.Left = btnLoginAccount.Right + 16;
            btnLoginGuest.Top = btnLoginAccount.Top;
            btnLoginGuest.Text = "Try as Guest";
            BottomPanel.Add(btnLoginGuest);

            //Links
            lnkForgot = new LinkLabel(Manager);
            lnkForgot.Init();
            lnkForgot.Width = txtUsername.Width;
            lnkForgot.Alignment = Alignment.MiddleCenter;
            lnkForgot.Left = txtUsername.Left;
            lnkForgot.Top = btnLoginAccount.Bottom + 8;
            lnkForgot.Text = "Forgot Password?";
            lnkForgot.URL = Constants.Strings.ForgotPasswordURL;
            BottomPanel.Add(lnkForgot);

            lnkCreateAccount = new LinkLabel(Manager);
            lnkCreateAccount.Init();
            lnkCreateAccount.Width = txtUsername.Width;
            lnkCreateAccount.Alignment = Alignment.MiddleCenter;
            lnkCreateAccount.Left = btnLoginGuest.Left;
            lnkCreateAccount.Top = lnkForgot.Top;
            lnkCreateAccount.Text = "Create an account.";
            lnkCreateAccount.URL = Constants.Strings.CreateAccountURL;
            BottomPanel.Add(lnkCreateAccount);

            //Color selector
            BodyClr = new ColorPicker(Manager) { Left = txtUsername.Right + 16, Top = txtUsername.Top, Width = txtUsername.Width - 18 - 8, Saturation = 210, Value = 250 };
            BodyClr.Init();
            BodyClr.ValueChanged += (sender, args) =>
            {
                BodyImg.Color = BodyClr.SelectedColor;
                //TODO: Update value in the game class
            };
            Add(BodyClr);

            BodyImg = new ImageBox(Manager) { Left = BodyClr.Right + 8, Top = BodyClr.Top, Width = 18, Height = 18, Image = Game.Content["entity.body"] };
            BodyImg.Init();
            Add(BodyImg);

            SmileyImg = new ImageBox(Manager) { Left = BodyImg.Left, Top = BodyImg.Top, Width = 18, Height = 18, Image = Game.Content["entity.smileys"], SourceRect = new Rectangle(0, 0, 18, 18) };
            SmileyImg.Init();
            SmileyImg.ToolTip.Text = "I love this color!";
            Add(SmileyImg);

            BodyClr.Hue = 1;

            BottomPanel.Height = lnkForgot.Bottom + 28;
            BottomPanel.Top = Height - BottomPanel.Height;
        }
    }
}