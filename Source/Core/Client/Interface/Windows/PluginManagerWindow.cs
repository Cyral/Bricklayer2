using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private readonly Button btnToggle, btnDelete;
        private readonly ControlList<PluginDataControl> lstPlugins;
        private readonly PluginManagerScreen pluginScreen;
        private Dictionary<string, bool> pluginsStatus;

        public PluginManagerWindow(Manager manager, PluginManagerScreen screen) : base(manager)
        {
            pluginScreen = screen;
            // Setup the window.
            CaptionVisible = false;
            Movable = false;
            Resizable = false;
            Width = (int) (Manager.ScreenWidth*.7);
            Height = (int) (Manager.ScreenHeight*.9);
            Center();

            Caption.Text = "Manage your Bricklayer client plugins.";
            Description.Text = "Many plugins come installed by default and are required for most servers.";
            TopPanel.Height -= 6;

            pluginsStatus = screen.Client.IO.ReadPlugins();

            // List of installed plugins.
            lstPlugins = new ControlList<PluginDataControl>(Manager)
            {
                Left = 8,
                Top = TopPanel.Bottom + 8,
                Width = ClientWidth - 16,
                Height = BottomPanel.Top - TopPanel.Bottom - 16
            };
            lstPlugins.Init();
            Add(lstPlugins);

            // Toggle button.
            btnToggle = new Button(Manager)
            {
                Top = 8,
                Left = 8,
                Width = 128
            };
            btnToggle.Init();
            BottomPanel.Add(btnToggle);

            // If user enables or disables a plugin.
            btnToggle.Click += (sender, args) =>
            {
                pluginsStatus[((PluginDataControl) lstPlugins.Items[lstPlugins.ItemIndex]).Data.Name] =
                    !pluginsStatus[((PluginDataControl) lstPlugins.Items[lstPlugins.ItemIndex]).Data.Name];
                screen.Client.IO.WritePlugins(pluginsStatus);
                RefreshList();
                var msgBox = new MessageBox(Manager, MessageBoxType.Warning,
                    "Bricklayer must be restarted for changes to take affect.", "Note");
                msgBox.Init();
                manager.Add(msgBox);
                msgBox.ShowModal();
            };

            // Delete button.
            btnDelete = new Button(Manager)
            {
                Top = 8,
                Left = btnToggle.Right + 8,
                Text = "Delete Plugin",
                Width = 128
            };
            btnDelete.Init();
            BottomPanel.Add(btnDelete);

            // If user wants to delete a plugin.
            btnDelete.Click += (sender, args) =>
            {
                var msgBox = new MessageBox(Manager, MessageBoxType.YesNo,
                    "Are you sure you want to delete \"" +
                    ((PluginDataControl) lstPlugins.Items[lstPlugins.ItemIndex]).Data.Name + "\"? The plugin and plugin files will be permanently deleted.", "Confirm");
                msgBox.Init();
                manager.Add(msgBox);
                msgBox.ShowModal();
                msgBox.Closed += (closedSender, closedArgs) =>
                {
                    var dialog = closedSender as Dialog;
                    if (dialog?.ModalResult != ModalResult.Yes)
                        return;
                    // If user clicked yes:
                    var data = ((PluginDataControl) lstPlugins.Items[lstPlugins.ItemIndex]).Data;
                    pluginsStatus.Remove(data.Name);
                    lstPlugins.Items.RemoveAt(lstPlugins.ItemIndex);
                    screen.Client.IO.WritePlugins(pluginsStatus);

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

                    lstPlugins.ItemIndex = 0;
                };
            };

            // Right buttons.
            var btnClose = new Button(Manager) {Text = "Back", Width = 100, Right = ClientWidth - 8, Top = 8};
            btnClose.Init();
            btnClose.Click += (sender, args) => screen.ScreenManager.SwitchScreen(new LoginScreen());
            BottomPanel.Add(btnClose);

            var btnOpenFolder = new Button(Manager)
            {
                Top = 8,
                Text = "Open Folder",
                Width = 100,
                Right = btnClose.Left - 8
            };
            btnOpenFolder.Init();
            btnOpenFolder.ToolTip = new ToolTip(Manager) { Text = "Open plugins folder." };
            btnOpenFolder.Click += (sender, args) =>
            {
                // Open plugins folder.
                Process.Start(screen.Client.IO.Directories["Plugins"]);
            };
            BottomPanel.Add(btnOpenFolder);

            var btnGetPlugins = new Button(Manager)
            {
                Top = 8,
                Text = "Get Plugins",
                Width = 100,
                Right = btnOpenFolder.Left - 8
            };
            btnGetPlugins.Init();
            btnGetPlugins.ToolTip = new ToolTip(Manager) { Text = "Download plugins from website." };
            btnGetPlugins.Click += (sender, args) =>
            {
                // Open plugin DB site.
                Process.Start(Constants.Strings.PluginDB_URL);
            };
            BottomPanel.Add(btnGetPlugins);

            // When user clicks plugin on list, update text.
            lstPlugins.MouseUp += (sender, args) =>
            {
                var data = ((PluginDataControl) lstPlugins.Items[lstPlugins.ItemIndex]).Data;
                btnToggle.Text = (pluginsStatus[data.Name] ? "Disable " : "Enable ") + " Plugin";
                btnToggle.TextColor = !pluginsStatus[data.Name] ? Color.Lime : Color.Red;
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
            pluginsStatus = pluginScreen.Client.IO.ReadPlugins();
            lstPlugins.Items.Clear();

            foreach (var plugin in pluginScreen.Client.Plugins.Plugins)
                lstPlugins.Items.Add(new PluginDataControl(Manager, lstPlugins, plugin, pluginsStatus[plugin.Name]));

            if (lstPlugins.Items.Count > 0)
            {
                lstPlugins.ItemIndex = 0;

                var item = ((PluginDataControl) lstPlugins.Items[lstPlugins.ItemIndex]).Data;
                btnToggle.Text = (pluginsStatus[item.Name] ? "Disable " : "Enable ") + " Plugin";
                btnToggle.TextColor = !pluginsStatus[item.Name]
                    ? Color.Lime
                    : Color.Red;
            }
            btnToggle.Enabled = btnDelete.Enabled = lstPlugins.Items.Count > 0;
        }
    }
}