using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Bricklayer.Core.Client.Interface.Controls;
using Bricklayer.Core.Client.Interface.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        private Dictionary<string, bool> pluginStatuses;

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
            Description.Text = "Many plugins come installed by default and are required for most servers.\nDisabling or enabling a plugin does not garauntee it to function properly.\nIt is recommended to restart after disabling or enabling a plugin.";
            TopPanel.Height += 22;

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
                pluginStatuses[identifier] =
                    !pluginStatuses[identifier];
                var plugin = screen.Client.Plugins.Plugins.FirstOrDefault(x => x.Identifier.Equals(identifier));

                try
                {
                    if (!pluginStatuses[identifier])
                        screen.Client.Plugins.DisablePlugin(plugin, pluginStatuses);
                    else
                        plugin = screen.Client.Plugins.EnablePlugin(plugin, pluginStatuses);
                }
                catch (Exception e)
                {
                    pluginStatuses[identifier] =
                        !pluginStatuses[identifier];
                    //Show error if plugin could not be disabled or enabled. (Missing dependencies?)
                    var errorBox = new MessageBox(Manager, MessageBoxType.Warning,
                    e.ToString(), "Error");
                    errorBox.Init();
                    manager.Add(errorBox);
                    errorBox.ShowModal();
                }

                LstPlugins.Items[LstPlugins.ItemIndex] = new PluginDataControl(Manager, LstPlugins,
                    plugin);
                BtnToggle.Text = (pluginStatuses[identifier] ? "Disable " : "Enable ") + " Plugin";
                BtnToggle.TextColor = !pluginStatuses[identifier]
                    ? Color.Lime
                    : Color.Red;

                var msgBox = new MessageBox(Manager, MessageBoxType.Warning,
                    "It is recommended to restart Bricklayer for all changes to take effect.","Note");
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
                    // If user clicked yes, remove plugin from list and delete it.
                    var data = ((PluginDataControl) LstPlugins.Items[LstPlugins.ItemIndex]).Data;
                    pluginStatuses.Remove(data.Identifier);
                    LstPlugins.Items.RemoveAt(LstPlugins.ItemIndex);

                    var plugin = screen.Client.Plugins.Plugins.FirstOrDefault(x => x.Identifier.Equals(data.Identifier));
                    if (plugin != null)
                        screen.Client.Plugins.DeletePlugin(plugin, pluginStatuses);

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
                BtnToggle.Text = (pluginStatuses[data.Identifier] ? "Disable " : "Enable ") + " Plugin";
                BtnToggle.TextColor = !pluginStatuses[data.Identifier] ? Color.Lime : Color.Red;
            };

            // Add plugins to list.
            RefreshList();
        }

        /// <summary>
        /// Refesh plugin list.
        /// </summary>
        private void RefreshList()
        {
            if (!pluginScreen.Client.Plugins.Initialized) return;

            pluginStatuses = pluginScreen.Client.IO.ReadPluginStatus();
            LstPlugins.Items.Clear();

            foreach (var plugin in pluginScreen.Client.Plugins.Plugins.OrderBy(x => !x.IsEnabled).ThenBy(x => x.Name))
                LstPlugins.Items.Add(new PluginDataControl(Manager, LstPlugins, plugin));

            if (LstPlugins.Items.Count > 0)
            {
                LstPlugins.ItemIndex = 0;

                var item = ((PluginDataControl) LstPlugins.Items[LstPlugins.ItemIndex]).Data;
                BtnToggle.Text = (pluginStatuses[item.Identifier] ? "Disable " : "Enable ") + " Plugin";
                BtnToggle.TextColor = !pluginStatuses[item.Identifier]
                    ? Color.Lime
                    : Color.Red;
            }
            BtnToggle.Enabled = BtnDelete.Enabled = LstPlugins.Items.Count > 0;
        }
    }
}