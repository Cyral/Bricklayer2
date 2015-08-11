using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using Microsoft.Xna.Framework;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Windows
{
    /// <summary>
    /// Window that is displayed when a plugin starts downloading.
    /// </summary>
    internal sealed class PluginDownloadWindow : Dialog
    {
        private static readonly List<int> downloads;
        private readonly MainWindow window;
        private readonly int id;
        private readonly string fileName;
        private readonly WebClient client;
        private readonly Label lblName;
        private readonly Label lblPercent;
        private readonly ProgressBar pbDownloaded;

        static PluginDownloadWindow()
        {
            downloads = new List<int>();
        }

        /// <summary>
        /// Returns true if the plugin/revision ID is already downloading.
        /// </summary>
        internal static bool IsDownloading(int id)
        {
            return downloads.Contains(id); // Already downloading this plugin
        }

        public PluginDownloadWindow(Manager manager, MainWindow window, string modName, int id, string fileName,
            bool updating)
            : base(manager)
        {
            this.window = window;
            this.id = id;
            this.fileName = fileName;
            CaptionVisible = false;
            TopPanel.Visible = false;
            BottomPanel.Visible = false;
            Movable = false;
            Resizable = false;
            Passive = true;
            Width = 350;
            Shadow = true;
            Center();
            Top = 24;

            // Add controls
            lblName = new Label(manager)
            {
                Left = 8,
                Top = 8,
                Text = updating ? "Updating " : "Downloading \"" + modName + "\"",
                Width = ClientWidth - 16
            };
            lblName.Init();
            lblName.Alignment = Alignment.TopLeft;
            Add(lblName);

            // Add controls
            lblPercent = new Label(manager) {Left = 8, Top = 8, Text = "0%", Width = ClientWidth - 16};
            lblPercent.Init();
            lblPercent.Alignment = Alignment.TopRight;
            Add(lblPercent);

            pbDownloaded = new ProgressBar(manager) {Left = 8, Top = lblName.Bottom + 4, Width = ClientWidth - 16, Color = Color.LawnGreen};
            pbDownloaded.Init();
            Add(pbDownloaded);

            Height = MinimumHeight = pbDownloaded.Bottom + 22;

            // For multiple dialogs, stack them.
            Top += (Height + 8) * downloads.Count;

            // Download the file
            downloads.Add(id);
            try
            {
                client = new WebClient {Proxy = null};
                client.DownloadProgressChanged += DownloadProgress;
                client.DownloadFileCompleted += DownloadComplete;
                client.DownloadFileAsync(new Uri(Constants.Strings.DownloadPluginURL + id),
                    Path.Combine(window.Client.IO.Directories["Plugins"], fileName));
            }
            catch (Exception e)
            {
                lblName.Text = "Error Downloading (Hover)";
                lblName.ToolTip.Text = e.Message;
            }
        }

        protected override void Dispose(bool disposing)
        {
            client.DownloadProgressChanged -= DownloadProgress;
            client.DownloadFileCompleted -= DownloadComplete;
            base.Dispose(disposing);
        }

        private void DownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            // Load new plugin (LoadPlugins will only load new plugins, not existing ones again)
            window.Client.Plugins.LoadPlugins();
            downloads.Remove(id);
            Close();
        }

        private void DownloadProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            var percentage = e.BytesReceived / (double)e.TotalBytesToReceive * 100;

            lblPercent.Text = $"{Math.Round(percentage)}% of {(e.TotalBytesToReceive / 1024) / 1024}MB";
            pbDownloaded.Value = (int)percentage;
        }
    }
}