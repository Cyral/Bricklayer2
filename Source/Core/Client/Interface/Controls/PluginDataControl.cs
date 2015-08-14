using Bricklayer.Core.Client.Interface.Screens;
using Microsoft.Xna.Framework;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Controls
{
    /// <summary>
    /// A control for displaying plugin data in the plugin manager.
    /// </summary>
    public sealed class PluginDataControl : Control
    {
        public ClientPlugin Data { get; private set; }

        public PluginDataControl(Manager manager, Control parent, ClientPlugin data,
            bool enabled)
            : base(manager)
        {
            // Setup.
            Passive = true;
            Height = 64 + 3;
            Width = parent.ClientWidth;
            Data = data;

            // Background "gradient" image
            // TODO: Make an actual control. not a statusbar
            var gradient = new StatusBar(manager);
            gradient.Init();
            gradient.Alpha = .8f;
            Add(gradient);

            var lblStatus = new Label(Manager)
            {
                Width = 100,
                Top = 4,
                Alignment = Alignment.TopLeft
            };
            lblStatus.Init();
            lblStatus.Text = enabled ? "Enabled" : "Disabled";
            lblStatus.TextColor = enabled ? Color.Lime : Color.Red;
            lblStatus.Left = ClientWidth - 26 -
                              (int)Manager.Skin.Fonts[lblStatus.Font.ToString()].Resource.MeasureString(lblStatus.Text).X;
            Add(lblStatus);

            var lblName = new Label(Manager)
            {
                Width = ClientWidth,
                Text = data.Name,
                Left = 4,
                Top = 4,
                Font = FontSize.Default12,
                Alignment = Alignment.TopLeft
            };
            lblName.Init();
            Add(lblName);
            lblName.Text = data.Name;

            var lblDescription = new Label(Manager)
            {
                Width = ClientWidth,
                Left = 4,
                Top = lblName.Bottom + 2,
                Alignment = Alignment.TopLeft
            };
            lblDescription.Init();
            Add(lblDescription);
            lblDescription.Text = data.Description;

            var lblPackage = new Label(Manager)
            {
                Width = ClientWidth,
                Left = 4,
                Top = lblDescription.Bottom,
                Alignment = Alignment.TopLeft,
                TextColor = Color.Gray
            };
            lblPackage.Init();
            lblPackage.Text = "v" + Data.Version + " - " + Data.Identifier;
            Add(lblPackage);

            if (data.Icon != null)
            {
                var imgIcon = new ImageBox(Manager)
                {
                    Image = data.Icon
                };
                imgIcon.Top = (ClientHeight / 2) - (imgIcon.Height / 2) + 1;
                imgIcon.Left = 64 - (imgIcon.Height) + 1;
                imgIcon.Init();
                Add(imgIcon);
                lblName.Left = imgIcon.Width + 6;
                lblDescription.Left = imgIcon.Width + 6;
                lblPackage.Left = imgIcon.Width + 6;
            }
        }

        public override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            // Don't draw anything
            // base.DrawControl(renderer,rect,gameTime);
        }
    }
}