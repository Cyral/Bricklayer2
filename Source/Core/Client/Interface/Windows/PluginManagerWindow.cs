using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Core.Client.Interface.Controls;
using Bricklayer.Core.Client.Interface.Screens;
using Bricklayer.Core.Common;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Windows
{
    internal sealed class PluginManagerWindow : Dialog
    {
        private GroupPanel grpInfo;
        private Label lblTitle;
        private Label lblDescription;
        private ImageBox imgIcon;
        private Button btnToggle;
        private Button btnDelete;
        private Button btnClose;
        private ControlList<PluginDataControl> lstPlugins;
        private PluginManagerScreen pluginScreen;
        private Dictionary<string, bool> pluginsStatus;

        public PluginManagerWindow(Manager manager, PluginManagerScreen screen) : base(manager)
        {
            pluginScreen = screen;
            // Setup the window
            CaptionVisible = false;
            TopPanel.Visible = false;
            Movable = false;
            Resizable = false;
            Width = (int)(Manager.ScreenWidth * .9);
            Height = (int)(Manager.ScreenHeight * .9);
            Shadow = true;
            Center();

            pluginsStatus = screen.Client.IO.ReadPlugins();

            // Group panels
            grpInfo = new GroupPanel(Manager)
            {
                Left = ClientWidth - Globals.Values.MaxBannerWidth - 1,
                Width = Globals.Values.MaxBannerWidth,
                Height = ClientHeight - BottomPanel.Height + 2,
                Text = "Info"
            };
            grpInfo.Init();
            Add(grpInfo);

            var grpPlugins = new GroupPanel(Manager)
            {
                Width = ClientWidth - grpInfo.Width, // Fill remaining space
                Height = ClientHeight - BottomPanel.Height + 2,
                Text = "Downloaded Plugins"
            };
            grpPlugins.Init();
            Add(grpPlugins);


            // Downloaded Plugins list
            lstPlugins = new ControlList<PluginDataControl>(Manager)
            {
                Left = 8,
                Top = 16,
                Width = grpPlugins.Width - 16,
                Height = grpPlugins.Height - 24
            };
            lstPlugins.Init();
            grpPlugins.Add(lstPlugins);

            // Add Plugins to list
            foreach (var p in screen.Client.Plugins.Plugins)
                lstPlugins.Items.Add(new PluginDataControl(pluginScreen, manager, lstPlugins, p, pluginsStatus[p.Name]));

           lstPlugins.ItemIndex = 0;
           var plugin = screen.Client.Plugins.Plugins[0];

            // Plugin info area
            lblTitle = new Label(Manager)
            {
                Alignment = Alignment.MiddleCenter,
                Top = 8,
                Font = FontSize.Default20,
                Text = plugin.Name,
                Width = grpInfo.Width,
                Height = 30
            };
            lblTitle.Init();
            grpInfo.Add(lblTitle);

            lblDescription = new Label(Manager)
            {
                Alignment = Alignment.MiddleCenter,
                Top = 10 + lblTitle.Bottom,
                Text = plugin.Description,
                Width = grpInfo.Width,
                Height = 30
            };
            lblDescription.Init();
            grpInfo.Add(lblDescription);

            btnToggle = new Button(Manager)
            {
                Top = lblDescription.Bottom,
                Text = (pluginsStatus[plugin.Name] ? "Disable " : "Enable ") + " Plugin",
                Width = 100,
            };
            btnToggle.Init();
            btnToggle.Left = (grpInfo.Width/2) - (btnToggle.Width/2) - 75;
            grpInfo.Add(btnToggle);

            // If user enables or disables a plugin
            btnToggle.Click += (sender, args) =>
            {
                var plugins = pluginsStatus;
                plugins[((PluginDataControl) lstPlugins.Items[lstPlugins.ItemIndex]).data.Name] = !plugins[((PluginDataControl)lstPlugins.Items[lstPlugins.ItemIndex]).data.Name];
                screen.Client.IO.WritePlugins(plugins);
                RefreshList();
                var msgBox = new MessageBox(Manager, MessageBoxType.Warning, "Game must be reset before changes take effect.", "Note");
                msgBox.Init();
                manager.Add(msgBox);
                msgBox.ShowModal();
            };

            btnDelete = new Button(Manager)
            {
                Top = lblDescription.Bottom,
                Text = "Delete",
                Width = 100,
            };
            btnDelete.Init();
            btnDelete.Left = (grpInfo.Width / 2) - (btnDelete.Width / 2) + 75;
            grpInfo.Add(btnDelete);

            // If user wants to delete a plugin
            btnDelete.Click += (sender, args) =>
            {
                var msgBox = new MessageBox(Manager, MessageBoxType.YesNo, "Are you sure you want to delete \"" + ((PluginDataControl)lstPlugins.Items[lstPlugins.ItemIndex]).data.Name + "\"?", "Warning");
                msgBox.Init();
                manager.Add(msgBox);
                msgBox.ShowModal();
                msgBox.Closed += (closedSender, closedArgs) =>
                {
                    var dialog = closedSender as Dialog;
                    if (dialog?.ModalResult != ModalResult.Yes)
                        return;
                    // If user clicked yes
                    var p = ((PluginDataControl) lstPlugins.Items[lstPlugins.ItemIndex]).data;
                    pluginsStatus.Remove(((PluginDataControl)lstPlugins.Items[lstPlugins.ItemIndex]).data.Name);
                    lstPlugins.Items.RemoveAt(lstPlugins.ItemIndex);
                    screen.Client.IO.WritePlugins(pluginsStatus); 

                    if(Directory.Exists(p.Path))
                        Directory.Delete(p.Path, true);

                    pluginScreen.Client.Plugins.Plugins.Remove(p);

                    lstPlugins.ItemIndex = 0;
                };
            };

            btnClose = new Button(Manager) { Width = 100, Top = 8, Text = "Back to Login" };
            btnClose.Left = (ClientWidth / 2) - (btnClose.Width / 2);
            btnClose.Init();
            btnClose.Click += (sender, args) =>
            {
                screen.ScreenManager.SwitchScreen(new LoginScreen());
            };
            BottomPanel.Add(btnClose);

            // When user clicks plugin on list, set info on side on grpInfo
            lstPlugins.Click += (sender, args) =>
            {
                lblTitle.Text = ((PluginDataControl)lstPlugins.Items[lstPlugins.ItemIndex]).data.Name;
                lblDescription.Text = ((PluginDataControl)lstPlugins.Items[lstPlugins.ItemIndex]).data.Description;
                btnToggle.Text = (pluginsStatus[((PluginDataControl)lstPlugins.Items[lstPlugins.ItemIndex]).data.Name] ? "Disable " : "Enable ") + " Plugin";
            };
        }

        // Refresh plugin list
        private void RefreshList()
        {
            pluginsStatus.Clear();
            pluginsStatus = pluginScreen.Client.IO.ReadPlugins();
            lstPlugins.Items.Clear();
            foreach (var p in pluginScreen.Client.Plugins.Plugins)
                lstPlugins.Items.Add(new PluginDataControl(pluginScreen, Manager, lstPlugins, p, pluginsStatus[p.Name]));

            lblTitle.Text = ((PluginDataControl)lstPlugins.Items[lstPlugins.ItemIndex]).data.Name;
            lblDescription.Text = ((PluginDataControl)lstPlugins.Items[lstPlugins.ItemIndex]).data.Description;
            btnToggle.Text = (pluginsStatus[((PluginDataControl)lstPlugins.Items[lstPlugins.ItemIndex]).data.Name] ? "Disable " : "Enable ") + " Plugin";
        }
    }
}
