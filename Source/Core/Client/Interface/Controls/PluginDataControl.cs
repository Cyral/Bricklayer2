using Microsoft.Xna.Framework;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Controls
{
    /// <summary>
    /// A control for displaying plugin data in the plugin manager.
    /// </summary>
    public sealed class PluginDataControl : Control
    {
        /// <summary>
        /// ClientPlugin this item is for.
        /// </summary>
        public ClientPlugin Data { get; }

        /// <summary>
        /// Label to display enabled or disabled status.
        /// </summary>
        public Label LblStatus { get; }

        public Label LblPackage { get; }
        public Label LblDescription { get; }
        public Label LblName { get; }
        public ImageBox ImgIcon { get; }

        public PluginDataControl(Manager manager, Control parent, ClientPlugin data)
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

            LblStatus = new Label(Manager)
            {
                Width = 100,
                Top = 4,
                Alignment = Alignment.TopLeft
            };
            LblStatus.Init();
            LblStatus.Text = data.IsEnabled ? "Enabled" : "Disabled";
            LblStatus.TextColor = data.IsEnabled ? Color.Lime : Color.Red;
            LblStatus.Left = ClientWidth - 26 -
                             (int)
                                 Manager.Skin.Fonts[LblStatus.Font.ToString()].Resource.MeasureString(LblStatus.Text).X;
            Add(LblStatus);

            LblName = new Label(Manager)
            {
                Width = ClientWidth,
                Text = data.Name,
                Left = 4,
                Top = 4,
                Font = FontSize.Default12,
                Alignment = Alignment.TopLeft
            };
            LblName.Init();
            Add(LblName);
            LblName.Text = data.Name;

            LblDescription = new Label(Manager)
            {
                Width = ClientWidth,
                Left = 4,
                Top = LblName.Bottom + 2,
                Alignment = Alignment.TopLeft
            };
            LblDescription.Init();
            Add(LblDescription);
            LblDescription.Text = data.Description;

            LblPackage = new Label(Manager)
            {
                Width = ClientWidth,
                Left = 4,
                Top = LblDescription.Bottom,
                Alignment = Alignment.TopLeft,
                TextColor = Color.Gray
            };
            LblPackage.Init();
            LblPackage.Text = "v" + Data.Version + " - " + Data.Identifier;
            Add(LblPackage);

            if (data.Icon != null)
            {
                ImgIcon = new ImageBox(Manager)
                {
                    Image = data.Icon
                };
                ImgIcon.Top = (ClientHeight/2) - (ImgIcon.Height/2) + 1;
                ImgIcon.Left = 64 - (ImgIcon.Height) + 1;
                ImgIcon.Init();
                Add(ImgIcon);
                LblName.Left = ImgIcon.Width + 6;
                LblDescription.Left = ImgIcon.Width + 6;
                LblPackage.Left = ImgIcon.Width + 6;
            }
        }

        public override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            // Don't draw anything
            // base.DrawControl(renderer,rect,gameTime);
        }
    }
}