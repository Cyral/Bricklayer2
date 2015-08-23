using System;
using System.Diagnostics;
using System.Linq;
using Bricklayer.Core.Client;
using Bricklayer.Core.Client.Interface.Screens;
using MonoForce.Controls;

namespace Bricklayer.Plugins.MultiAccounts
{
    /// <summary>
    /// A plugin to add multiple accounts to the login screen. (Useful for debugging)
    /// </summary>
    public class MultiAccounts : ClientPlugin
    {
        private static readonly string configFile = "accounts.json";
        private AccountsConfig config;

        public MultiAccounts(Client host) : base(host)
        {
        }

        public override async void Load()
        {
            // Load or create config file.
            config = await Client.IO.LoadConfig<AccountsConfig>(this, configFile);

            // When the login screen is created, add buttons for each account.
            Client.Events.Game.ScreenChanged.AddHandler(args =>
            {
                var screen = args.NewScreen as LoginScreen;
                if (screen != null)
                {
                    // Initial positions.
                    var top = screen.WndLogin.Top;
                    var right = screen.WndLogin.Left - 8;
                    var width = 150;

                    // Add buttons for each account.
                    for (var i = 0; i < config.Accounts.Count; i++)
                    {
                        var acc = config.Accounts.ElementAt(i);
                        var button = new Button(Client.UI)
                        {
                            Width = width,
                            Top = top,
                            Right = right,
                            Text = acc.Key.Replace('_', ' ')
                        };
                        button.Init();
                        top += button.Height + 4;
                        button.Click += (sender, eventArgs) =>
                        {
                            Client.Network.LoginToAuth(acc.Key, acc.Value);
                            ((Button) sender).Enabled = false;
                        };
                        screen.AddControl(button);
                    }

                    // Add edit button.
                    var editButton = new Button(Client.UI)
                    {
                        Width = width,
                        Top = top,
                        Right = right,
                        Text = "Edit Accounts"
                    };
                    editButton.Init();
                    editButton.Click +=
                        (sender, eventArgs) => { Process.Start(System.IO.Path.Combine(Path, configFile)); };
                    screen.AddControl(editButton);
                }
            });
        }

        public override void Unload()
        {
            
        }
    }
}