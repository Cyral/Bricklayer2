using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Bricklayer.Core.Client.Interface.Controls;
using Bricklayer.Core.Client.Interface.Screens;
using Microsoft.Xna.Framework;
using MonoForce.Controls;
using Console = System.Console;

namespace Bricklayer.Core.Client.Interface.Windows
{
    /// <summary>
    /// Plugin manager window. (Enable/disable installed plugins)
    /// </summary>
    public sealed class PluginManagerWindow : Dialog
    {
        /// <summary>
        /// Button to toggle enabled/disabled status.
        /// </summary>
        public Button BtnToggle { get; }

        /// <summary>
        /// Button to delete a plugin.
        /// </summary>
        public Button BtnDelete { get; }

        /// <summary>
        /// List of plugin controls displaying plugin info.
        /// </summary>
        public ControlList<PluginDataControl> LstPlugins { get; }

        /// <summary>
        /// Button to exit back to login screen.
        /// </summary>
        public Button BtnClose { get; }

        /// <summary>
        /// Button to open plugins folder.
        /// </summary>
        public Button BtnOpenFolder { get; }

        /// <summary>
        /// Button to go to plugin DB on website.
        /// </summary>
        public Button BtnGetPlugins { get; }

        private readonly PluginManagerScreen pluginScreen;
        private Dictionary<string, bool> pluginsStatus;

        public PluginManagerWindow(Manager manager, PluginManagerScreen screen) : base(manager)
        {
            pluginScreen = screen;

            // Setup the window.
            CaptionVisible = false;
            Movable = false;
            Resizable = false;
            Width = (int) (Manager.ScreenWidth*.75);
            Height = (int) (Manager.ScreenHeight*.9);
            Center();

            Caption.Text = "Manage your Bricklayer client plugins.";
            Description.Text = "Many plugins come installed by default and are required for most servers.";
            TopPanel.Height -= 6;

            pluginsStatus = screen.Client.IO.ReadPluginStatus();

            // List of installed plugins.
            LstPlugins = new ControlList<PluginDataControl>(Manager)
            {
                Left = 8,
                Top = TopPanel.Bottom + 8,
                Width = ClientWidth - 16,
                Height = BottomPanel.Top - TopPanel.Bottom - 16
            };
            LstPlugins.Init();
            Add(LstPlugins);

            // Toggle button.
            BtnToggle = new Button(Manager)
            {
                Top = 8,
                Left = 8,
                Width = 128
            };
            BtnToggle.Init();
            BottomPanel.Add(BtnToggle);

            // If user enables or disables a plugin.
            BtnToggle.Click += (sender, args) =>
            {
                var identifier =  ((PluginDataControl) LstPlugins.Items[LstPlugins.ItemIndex]).Data.Identifier;
                pluginsStatus[identifier] =
                    !pluginsStatus[identifier];
                var plugin = screen.Client.Plugins.Plugins.FirstOrDefault(x => x.Identifier.Equals(identifier));
                if (plugin != null)
                    plugin.IsEnabled = pluginsStatus[identifier];
                screen.Client.IO.WritePluginStatus(pluginsStatus);
                RefreshList();
                var msgBox = new MessageBox(Manager, MessageBoxType.Warning,
                    "Bricklayer must be restarted for changes to take affect.", "Note");
                msgBox.Init();
                manager.Add(msgBox);
                msgBox.ShowModal();
            };

            // Delete button.
            BtnDelete = new Button(Manager)
            {
                Top = 8,
                Left = BtnToggle.Right + 8,
                Text = "Delete Plugin",
                Width = 128
            };
            BtnDelete.Init();
            BottomPanel.Add(BtnDelete);

            // If user wants to delete a plugin.
            BtnDelete.Click += (sender, args) =>
            {
                var msgBox = new MessageBox(Manager, MessageBoxType.YesNo,
                    "Are you sure you want to delete \"" +
                    ((PluginDataControl) LstPlugins.Items[LstPlugins.ItemIndex]).Data.Name +
                    "\"? The plugin and plugin files will be permanently deleted.", "Confirm");
                msgBox.Init();
                manager.Add(msgBox);
                msgBox.ShowModal();
                msgBox.Closed += (closedSender, closedArgs) =>
                {
                    var dialog = closedSender as Dialog;
                    if (dialog?.ModalResult != ModalResult.Yes)
                        return;
                    // If user clicked yes:
                    var data = ((PluginDataControl) LstPlugins.Items[LstPlugins.ItemIndex]).Data;
                    pluginsStatus.Remove(data.Identifier);
                    LstPlugins.Items.RemoveAt(LstPlugins.ItemIndex);
                    var plugin = screen.Client.Plugins.Plugins.FirstOrDefault(x => x.Identifier.Equals(data.Identifier));
                    if (plugin != null)
                        plugin.IsEnabled = pluginsStatus[data.Identifier];
                    screen.Client.IO.WritePluginStatus(pluginsStatus);

                    try
                    {
                        if (Directory.Exists(data.Path))
                            Directory.Delete(data.Path, true);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }

                    pluginScreen.Client.Plugins.Plugins.Remove(data);

                    LstPlugins.ItemIndex = 0;
                };
            };

            // Right buttons.
            BtnClose = new Button(Manager) {Text = "Back", Width = 100, Right = ClientWidth - 8, Top = 8};
            BtnClose.Init();
            BtnClose.Click += (sender, args) => screen.ScreenManager.SwitchScreen(new LoginScreen());
            BottomPanel.Add(BtnClose);

            BtnOpenFolder = new Button(Manager)
            {
                Top = 8,
                Text = "Open Folder",
                Width = 100,
                Right = BtnClose.Left - 8
            };
            BtnOpenFolder.Init();
            BtnOpenFolder.ToolTip = new ToolTip(Manager) {Text = "Open plugins folder."};
            BtnOpenFolder.Click += (sender, args) =>
            {
                // Open plugins folder.
                Process.Start(screen.Client.IO.Directories["Plugins"]);
            };
            BottomPanel.Add(BtnOpenFolder);

            BtnGetPlugins = new Button(Manager)
            {
                Top = 8,
                Text = "Get Plugins",
                Width = 100,
                Right = BtnOpenFolder.Left - 8
            };
            BtnGetPlugins.Init();
            BtnGetPlugins.ToolTip = new ToolTip(Manager) {Text = "Download plugins from website."};
            BtnGetPlugins.Click += (sender, args) =>
            {
                // Open plugin DB site.
                Process.Start(Constants.Strings.PluginDB_URL);
            };
            BottomPanel.Add(BtnGetPlugins);

            // When user clicks plugin on list, update text.
            LstPlugins.MouseUp += (sender, args) =>
            {
                var data = ((PluginDataControl) LstPlugins.Items[LstPlugins.ItemIndex]).Data;
                BtnToggle.Text = (pluginsStatus[data.Identifier] ? "Disable " : "Enable ") + " Plugin";
                BtnToggle.TextColor = !pluginsStatus[data.Identifier] ? Color.Lime : Color.Red;
            };

            // Add plugins to list.
            RefreshList();
        }

        /// <summary>
        /// Refesh plugin list.
        /// </summary>
        private void RefreshList()
        {
            pluginsStatus.Clear();
            pluginsStatus = pluginScreen.Client.IO.ReadPluginStatus();
            LstPlugins.Items.Clear();

            foreach (var plugin in pluginScreen.Client.Plugins.Plugins)
                LstPlugins.Items.Add(new PluginDataControl(Manager, LstPlugins, plugin, pluginsStatus[plugin.Identifier]));

            if (LstPlugins.Items.Count > 0)
            {
                LstPlugins.ItemIndex = 0;

                var item = ((PluginDataControl) LstPlugins.Items[LstPlugins.ItemIndex]).Data;
                BtnToggle.Text = (pluginsStatus[item.Identifier] ? "Disable " : "Enable ") + " Plugin";
                BtnToggle.TextColor = !pluginsStatus[item.Identifier]
                    ? Color.Lime
                    : Color.Red;
            }
            BtnToggle.Enabled = BtnDelete.Enabled = LstPlugins.Items.Count > 0;
        }
    }
}