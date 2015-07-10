using System.Diagnostics;
using Bricklayer.Core.Client.Interface.Windows;
using Bricklayer.Core.Common;
using Microsoft.Xna.Framework;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Screens
{
    internal class LoginScreen : Screen
    {
        //Controls
        private ImageBox imgLogo, imgGithub, imgPyratron, imgBackground;
        private Label lblVersion;
        private LoginWindow wndLogin;

        protected internal override GameState State => GameState.Login;

        /// <summary>
        /// Setup the login and serverlist screen content and controls.
        /// </summary>
        /// <param name="screenManager"></param>
        public override void Add(ScreenManager screenManager)
        {
            base.Add(screenManager);

            imgBackground = new ImageBox(Manager)
            {
                Image = Client.Content["gui.background"],
                SizeMode = SizeMode.Stretched
            };
            imgBackground.SetSize(Window.Width, Window.Height);
            imgBackground.SetPosition(0, 0);
            imgBackground.Init();
            Window.Add(imgBackground);

            //Add the logo image
            imgLogo = new ImageBox(Manager) {Image = Client.Content["gui.logosmall"], SizeMode = SizeMode.Normal};
            imgLogo.Init();
            imgLogo.SetSize(imgLogo.Image.Width, imgLogo.Image.Height);
            imgLogo.SetPosition((Window.Width / 2) - (imgLogo.Image.Width / 2), 0);
            Window.Add(imgLogo);

            //Add github contribute link
            imgGithub = new ImageBox(Manager)
            {
                Image = Client.Content["gui.icons.github"],
                SizeMode = SizeMode.Auto,
                ToolTip = {Text = "Contribute to Bricklayer on GitHub."}
            };
            imgGithub.SetSize(imgGithub.Width, imgGithub.Height);
            imgGithub.SetPosition(Window.Width - imgGithub.Width - 8, Window.Height - imgGithub.Height - 8);
            imgGithub.Init();
            imgGithub.Color = Color.White * .9f;
            imgGithub.MouseOut += (sender, args) => imgGithub.Color = Color.White * .9f;
            imgGithub.MouseOver += (sender, args) => imgGithub.Color = Color.White;
            imgGithub.Click +=
                (sender, args) => { if (Manager.Game.IsActive) Process.Start(Constants.Strings.GithubURL); };
            Window.Add(imgGithub);

            //Add Pyratron link
            imgPyratron = new ImageBox(Manager)
            {
                Image = Client.Content["gui.icons.pyratron"],
                SizeMode = SizeMode.Auto,
                ToolTip = { Text = "Visit Pyratron Studios." }
            };
            imgPyratron.SetSize(imgPyratron.Width, imgPyratron.Height);
            imgPyratron.SetPosition(Window.Width - imgGithub.Width - 16 - imgPyratron.Width, Window.Height - imgPyratron.Height - 8);
            imgPyratron.Init();
            imgPyratron.Color = Color.White * .9f;
            imgPyratron.MouseOut += (sender, args) => imgPyratron.Color = Color.White * .9f;
            imgPyratron.MouseOver += (sender, args) => imgPyratron.Color = Color.White;
            imgPyratron.Click +=
                (sender, args) => { if (Manager.Game.IsActive) Process.Start(Constants.Strings.PyratronURL); };
            Window.Add(imgPyratron);

            //Add version tag
            lblVersion = new Label(Manager) {Font = FontSize.Default14};
            lblVersion.Init();
            lblVersion.SetSize(200, 20);
            lblVersion.SetPosition(8, Window.Height - lblVersion.Height - 8);
            lblVersion.Text = Constants.VersionString;
            Window.Add(lblVersion);

            wndLogin = new LoginWindow(Manager, this);
            wndLogin.Init();
            //If the login window is overlapping the logo, push it down a bit. (For smaller screens)
            if (wndLogin.Top < imgLogo.Top + imgLogo.Height + 8)
                wndLogin.Top = imgLogo.Top + imgLogo.Height + 24;
            Window.Add(wndLogin);
        }

        /// <summary>
        /// Remove the login screen content and controls.
        /// </summary>
        public override void Remove()
        {
            Window.Remove(imgBackground);
            Window.Remove(imgLogo);
            Window.Remove(imgPyratron);
            Window.Remove(imgGithub);
            Window.Remove(lblVersion);
            Window.Remove(wndLogin);
            wndLogin?.Dispose();
        }
    }
}