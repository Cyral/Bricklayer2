using System.Diagnostics;
using Bricklayer.Core.Client.Interface.Windows;
using Microsoft.Xna.Framework;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Screens
{
    /// <summary>
    /// The first screen, with a login window, logo, etc.
    /// </summary>
    public class LoginScreen : Screen
    {
        /// <summary>
        /// Button to open the plugin manager.
        /// </summary>
        public Button BtnManagePlugins { get; private set; }

        /// <summary>
        /// Logo image above the login window.
        /// </summary>
        public ImageBox ImgLogo { get; private set; }

        /// <summary>
        /// GitHub icon linking to the project page.
        /// </summary>
        public ImageBox ImgGithub { get; private set; }

        /// <summary>
        /// Pyratron icon linking to the project page.
        /// </summary>
        public ImageBox ImgPyratron { get; private set; }

        /// <summary>
        /// Background image of the screen.
        /// </summary>
        public ImageBox ImgBackground { get; private set; }

        /// <summary>
        /// Label with the current version.
        /// </summary>
        public Label LblVersion { get; private set; }

        /// <summary>
        /// Login window.
        /// </summary>
        public LoginWindow WndLogin { get; private set; }

        public override GameState State => GameState.Login;

        /// <summary>
        /// Setup the login screen content and controls. (Login window, logo, etc.)
        /// </summary>
        public override void Setup(ScreenManager screenManager)
        {
            base.Setup(screenManager);

            // Set the background.
            ImgBackground = new ImageBox(Manager)
            {
                Image = Client.Content["gui.background"],
                SizeMode = SizeMode.Stretched,
                CanFocus = false,
                StayOnBack = true
            };
            ImgBackground.SetSize(Window.Width, Window.Height);
            ImgBackground.SetPosition(0, 0);
            ImgBackground.Init();
            AddControl(ImgBackground);

            // Add the logo image.
            ImgLogo = new ImageBox(Manager) {Image = Client.Content["gui.logosmall"], SizeMode = SizeMode.Normal};
            ImgLogo.Init();
            ImgLogo.SetSize(ImgLogo.Image.Width, ImgLogo.Image.Height);
            ImgLogo.SetPosition((Window.Width/2) - (ImgLogo.Image.Width/2), 0);
            AddControl(ImgLogo);

            // Add github contribute link.
            ImgGithub = new ImageBox(Manager)
            {
                Image = Client.Content["gui.icons.github"],
                SizeMode = SizeMode.Auto,
                ToolTip = {Text = "Contribute to Bricklayer on GitHub."}
            };
            ImgGithub.SetSize(ImgGithub.Width, ImgGithub.Height);
            ImgGithub.SetPosition(Window.Width - ImgGithub.Width - 8, Window.Height - ImgGithub.Height - 8);
            ImgGithub.Init();
            ImgGithub.Color = Color.White*.9f;
            ImgGithub.MouseOut += (sender, args) => ImgGithub.Color = Color.White*.9f;
            ImgGithub.MouseOver += (sender, args) => ImgGithub.Color = Color.White;
            ImgGithub.Click +=
                (sender, args) => { if (Manager.Game.IsActive) Process.Start(Constants.Strings.GithubURL); };
            AddControl(ImgGithub);

            // Add Pyratron link.
            ImgPyratron = new ImageBox(Manager)
            {
                Image = Client.Content["gui.icons.pyratron"],
                SizeMode = SizeMode.Auto,
                ToolTip = {Text = "Visit Pyratron Studios."}
            };
            ImgPyratron.SetSize(ImgPyratron.Width, ImgPyratron.Height);
            ImgPyratron.SetPosition(Window.Width - ImgGithub.Width - 16 - ImgPyratron.Width,
                Window.Height - ImgPyratron.Height - 8);
            ImgPyratron.Init();
            ImgPyratron.Color = Color.White*.9f;
            ImgPyratron.MouseOut += (sender, args) => ImgPyratron.Color = Color.White*.9f;
            ImgPyratron.MouseOver += (sender, args) => ImgPyratron.Color = Color.White;
            ImgPyratron.Click +=
                (sender, args) => { if (Manager.Game.IsActive) Process.Start(Constants.Strings.PyratronURL); };
            AddControl(ImgPyratron);

            // Add version tag.
            LblVersion = new Label(Manager) {Font = FontSize.Default14};
            LblVersion.Init();
            LblVersion.SetPosition(8, Window.Height - LblVersion.Height - 8);
            LblVersion.Text = Constants.VersionString;
            LblVersion.Width =
                (int)
                    Manager.Skin.Fonts[LblVersion.Font.ToString()].Resource.MeasureRichString(LblVersion.Text, Manager)
                        .X;
            AddControl(LblVersion);

            BtnManagePlugins = new Button(Manager)
            {
                Text = "Manage Plugins",
                Width = 100,
                Top = LblVersion.Top - 4,
                Left = LblVersion.Right + 8
            };
            BtnManagePlugins.Init();
            AddControl(BtnManagePlugins);
            BtnManagePlugins.Click += (sender, args) => { ScreenManager.SwitchScreen(new PluginManagerScreen()); };

            WndLogin = new LoginWindow(Manager, this);
            WndLogin.Init();

            // If the login window is overlapping the logo, push it down a bit. (For smaller screens)
            if (WndLogin.Top < ImgLogo.Top + ImgLogo.Height + 8)
                WndLogin.Top = ImgLogo.Top + ImgLogo.Height + 24;
            AddControl(WndLogin);
        }
    }
}