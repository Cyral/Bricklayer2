using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Client.Interface;
using Bricklayer.Core.Client.Components;
using Bricklayer.Core.Common;
using MonoForce.Controls;
using EventArgs = System.EventArgs;

namespace Bricklayer.Core.Client.Interface.Windows
{
    internal sealed class ModInstallWindow : Dialog
    {

        private readonly ProgressBar pbDownloaded;
        private readonly Label lblName;
        private readonly Label lblPercent;

        public ModInstallWindow(Manager manager, MainWindow window, string modName, string id, string fileName, bool updating)
            : base(manager)
        {

            CaptionVisible = false;
            TopPanel.Visible = false;
            Movable = false;
            Resizable = false;
            Width = 350;
            Height = 80;
            Shadow = true;
            Center();
            Top = ClientHeight-5;

            //Add controls
            lblName = new Label(manager) { Left = 0, Top = 50, Text = updating ? "Updating " : "Downloading \"" + modName + "\"", Width = ClientWidth - 16 };
            lblName.Init();
            lblName.Alignment = Alignment.BottomCenter;
            Add(lblName);

            //Add controls
            lblPercent = new Label(manager) { Left = 0, Top = 30, Text = "0%", Width = ClientWidth - 16 };
            lblPercent.Init();
            lblPercent.Alignment = Alignment.BottomCenter;
            Add(lblPercent);

            pbDownloaded = new ProgressBar(manager) { Left = 0, Top = 10, Width = ClientWidth - 16 };
            pbDownloaded.Init();
            pbDownloaded.SetPosition(10, 0);
            Add(pbDownloaded);

            var client = new WebClient();
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgress);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadComplete);
            client.DownloadFileAsync(new Uri("https://community.pyratron.com/app.php/db/download/" + id), window.Client.IO.MainDirectory + "\\Plugins\\" + fileName);
        }

        void DownloadProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;

            lblPercent.Text = Math.Truncate(percentage).ToString() + "%";
            pbDownloaded.Value = int.Parse(Math.Truncate(percentage).ToString());
        }

        private void DownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            this.Close();
        }
    }
}
