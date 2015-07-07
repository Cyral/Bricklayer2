using Bricklayer.Core.Client.Interface.Screens;
using Bricklayer.Core.Common.Data;
using Microsoft.Xna.Framework;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Controls
{
    /// <summary>
    /// A control for displaying a level's name, rating, online, etc, in the lobby.
    /// </summary>
    public sealed class LobbyDataControl : Control
    {
        public readonly LevelData Data;
        private readonly StatusBar gradient;
        private readonly ImageBox[] imgRating = new ImageBox[5];
        private readonly Label lblName, lblDescription, lblStats;
        private readonly LobbyScreen screen;

        public LobbyDataControl(LobbyScreen screen, Manager manager, LevelData data, Control parent)
            : base(manager)
        {
            //Setup
            Passive = false;
            Height = 60;
            Width = parent.ClientWidth;
            this.screen = screen;
            Data = data;

            //Background "gradient" image
            //TODO: Make an actual control. not a statusbar
            gradient = new StatusBar(manager);
            gradient.Init();
            gradient.Alpha = .8f;
            Add(gradient);

            for (var i = 0; i < 5; i++)
            {
                imgRating[i] = new ImageBox(Manager)
                {
                    Top = 4,
                    Width = 16,
                    Height = 16,
                    Left = ClientWidth - (((4 - i) * 18) + 48)
                };
                imgRating[i].Init();
                var half = data.Rating > i + .25 && data.Rating < i + .75;
                var whole = data.Rating >= i + .75;
                imgRating[i].Image = whole
                    ? screen.Client.Content["gui.icons.full_star"]
                    : half
                        ? screen.Client.Content["gui.icons.half_star"]
                        : screen.Client.Content["gui.icons.empty_star"];
                Add(imgRating[i]);
            }

            lblStats = new Label(Manager)
            {
                Width = 100,
                Left = ClientWidth - 100 - 16,
                Top = imgRating[0].Bottom + 4,
                Alignment = Alignment.TopLeft,
                TextColor = new Color(160, 160, 160)
            };
            lblStats.Init();
            lblStats.Text = $"Online: {data.Online}\nPlays: {"N/A"}";
            Add(lblStats);

            //Add controls
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
                Text = data.Name,
                Left = 4,
                Top = lblName.Bottom + 4,
                Alignment = Alignment.TopLeft
            };
            lblDescription.Init();
            Add(lblDescription);
            lblDescription.Text = data.Description;
        }

        public override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            //Don't draw anything
            //base.DrawControl(renderer,rect,gameTime);
        }
    }
}