using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Core.Client.Interface.Screens;
using Bricklayer.Core.Client.Interface.Windows;
using Bricklayer.Core.Common;
using Bricklayer.Core.Common.Data;
using Microsoft.Xna.Framework;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Controls
{
    /// <summary>
    /// A control for displaying a level's name, rating, online, etc, in the lobby.
    /// </summary>
    public sealed class PluginDataControl : Control
    {
        private readonly ImageBox imgIcon;
        private readonly StatusBar gradient;
        private readonly Label lblName, lblDescription, lblStatus;
        private readonly PluginManagerScreen screen;
        public readonly ClientPlugin data;

        public PluginDataControl(PluginManagerScreen screen, Manager manager, Control parent, ClientPlugin data, bool enabled)
            : base(manager)
        {
            // Setup
            Passive = true;
            Height = 70;
            Width = parent.ClientWidth;
            this.screen = screen;
            this.data = data;

            // Background "gradient" image
            // TODO: Make an actual control. not a statusbar
            gradient = new StatusBar(manager);
            gradient.Init();
            gradient.Alpha = .8f;
            Add(gradient);

            lblStatus = new Label(Manager)
            {
                Width = 100,
                Left = ClientWidth - 100 - 16,
                Top = 8,
                Alignment = Alignment.TopLeft,
                TextColor = new Color(160, 160, 160)
            };
            lblStatus.Init();
            lblStatus.Text = enabled ? "[color:green]Enabled[/color]" : "[color:red]Disabled[/color]";
            Add(lblStatus);

            // Add controls
            lblName = new Label(Manager)
            {
                Width = 100,
                Text = data.Name,
                Left = 4,
                Top = 4,
                Font = FontSize.Default14,
                Alignment = Alignment.TopLeft
            };
            lblName.Init();
            Add(lblName);
            lblName.Text = data.Name;

            lblDescription = new Label(Manager)
            {
                Width = 200,
                Left = 4,
                Top = lblName.Bottom + 4,
                Alignment = Alignment.TopLeft
            };
            lblDescription.Init();
            Add(lblDescription);
            lblDescription.Text = data.Description;


            if (data.Icon != null)
            {
                imgIcon = new ImageBox(Manager)
                {
                    Left = 10,
                    Image = data.Icon
                };
                imgIcon.Top = (ClientHeight/2) - (imgIcon.Height/2);
                imgIcon.Init();
                Add(imgIcon);
                lblName.Left += imgIcon.Width + 20;
                lblDescription.Left += imgIcon.Width + 20;
            }
        }

        public override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            // Don't draw anything
            // base.DrawControl(renderer,rect,gameTime);
        }
    }
}
